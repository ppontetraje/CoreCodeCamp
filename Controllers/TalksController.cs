using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/[controller]")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator )
        {
            _repository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }
        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await _repository.GetTalksByMonikerAsync(moniker, true);
                
                return _mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks");
            }
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Not Found.");
                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks");
            }
        }
        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp= await _repository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp does not exist.");

                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null) return BadRequest("Speaker Id is Required.");

                var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                talk.Speaker = speaker; 

                _repository.Add(talk);
                
                if (await _repository.SaveChangesAsync())
                {
                    var url = _linkGenerator.GetPathByAction(HttpContext,
                        "Get",
                        values: new { moniker, id = talk.TalkId});

                    return Created(url, _mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Failed to save new Talk.");
                }

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talk");
            }
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return BadRequest("Talk does not exist.");

                _mapper.Map(model, talk);


                if (model.Speaker != null)
                {
                    var speaker = await  _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Failed to save new Speaker.");
                } 
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks");
            }
        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TalkModel[]>> Delete(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return BadRequest("Talk does not exist.");

                _repository.Delete(talk);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to Delete talk.");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks");
            }
        }
    }
}
