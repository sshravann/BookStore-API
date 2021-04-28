using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint to interact with Authors in the BookStore's database.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository, 
            ILoggerService loggerService,
            IMapper mapper)
        {
            _authorRepository = authorRepository;
            _loggerService = loggerService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _loggerService.LogInfo("Attempted Get All Authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _loggerService.LogInfo("Successfully Got All Authors");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Get an author by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Author's record</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _loggerService.LogInfo($"Attempted Get Author with id: {id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _loggerService.LogWarn($"Author with id: {id} was not found.");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _loggerService.LogInfo($"Successfully Got Author with id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Creates An Author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                if (authorDTO == null)
                {
                    _loggerService.LogWarn($"Empty request was submitted.");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"Author data was incomplete.");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);
                if (!isSuccess)
                {
                    return InternalError($"Author creation failed");
                }
                _loggerService.LogInfo("Author created.");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Updates An Author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _loggerService.LogWarn($"Author with id: {id} update attempted.");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _loggerService.LogWarn($"Author update failed with bad data.");
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"Author Data was incomplete.");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);
                if (!isSuccess)
                {
                    return InternalError($"Update operation failed.");
                }
                _loggerService.LogInfo($"Author with id: {id} sucessfully updated.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Removes An Author
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _loggerService.LogInfo($"Author with id: {id} delete attempted.");
                if (id < 1)
                {
                    _loggerService.LogInfo($"Author delete failed with bad data");
                    return BadRequest();
                }
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _loggerService.LogInfo($"Author with id: {id} was not found.");
                    return NotFound();
                }
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError("Couldn't delete the author");
                }
                _loggerService.LogInfo($"Author with id: {id} successfully deleted.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        private ObjectResult InternalError(string message)
        {
            _loggerService.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact Administrator");
        }
    }
}
