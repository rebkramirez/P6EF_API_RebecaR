using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P6EF_API_RebecaR.Models;
using P6EF_API_RebecaR.ModelsDTOs;

namespace P6EF_API_RebecaR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AnswersDbContext _context;

        public UsersController(AnswersDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpGet("GetUserInfoById")]
        public ActionResult<IEnumerable<UsuarioDTO>> GetUserInfoByIdl(int pUserId)
        {
            var query = (from u in _context.Users
                         join ur in _context.UserRoles
                         on u.UserRoleId equals ur.UserRoleId
                         where u.UserId == pUserId
                         select new
                         {
                             id = u.UserId,
                             nombreusuario = u.UserName,
                             primernombre = u.FirstName,
                             apellido = u.LastName,
                             telefono = u.PhoneNumber,
                             contrasennia = u.UserPassword,
                             strike = u.StrikeCount,
                             segundocorreo = u.BackUpEmail,
                             descripciontrabajo = u.JobDescription
                         }
                        ).ToList();

            List<UsuarioDTO> list = new List<UsuarioDTO>();

            foreach (var item in query)
            {
                UsuarioDTO nuevoUsuario = new UsuarioDTO()
                {
                    UserId = item.id,
                    UserName = item.nombreusuario,
                    FirstName = item.primernombre,
                    LastName = item.apellido,
                    PhoneNumber = item.telefono,
                    UserPassword = item.contrasennia,
                    StrikeCount = item.strike,
                    BackUpEmail = item.segundocorreo,
                    JobDescription = item.descripciontrabajo
                };

                list.Add(nuevoUsuario);
            }

            if (list == null) { return NotFound(); }

            return list;

        }

        [HttpGet("GetUserInfoByEmail")]
        public ActionResult<IEnumerable<UsuarioDTO>> GetUserInfoByEmail(string pEmail)
        {
            var query = (from u in _context.Users
                         join ur in _context.UserRoles
                         on u.UserRoleId equals ur.UserRoleId
                         where u.BackUpEmail == pEmail
                         select new
                         {
                             id = u.UserId,
                             correo = u.BackUpEmail,
                             nombreUsuario = u.UserName,
                             nombre = u.FirstName,
                             apellido = u.LastName,
                             telefono = u.PhoneNumber,
                             contrasennia = u.UserPassword,
                             rolid = u.UserRoleId,
                             descriprol = ur.UserRole1
                         }
                         ).ToList();

            List<UsuarioDTO> list = new List<UsuarioDTO>();

            foreach (var item in query)
            {
                UsuarioDTO nuevoUsuario = new UsuarioDTO()
                {
                    UserId = item.id,
                    BackUpEmail = item.correo,
                    UserName = item.nombreUsuario,
                    FirstName = item.nombre,
                    LastName = item.apellido,
                    PhoneNumber = item.telefono,
                    UserPassword = item.contrasennia,
                    UserRoleId = item.rolid
                };

                list.Add(nuevoUsuario);
            }

            if (list == null || list.Count == 0)
            {
                return NotFound();
            }

            return list;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost("ValidateUser")]
        public async Task<ActionResult<UsuarioDTO>> ValidateUser([FromBody] UserLoginDTO loginRequest)
        {
            var user = await _context.Users
                .Where(u => u.UserName == loginRequest.UserName && u.UserPassword == loginRequest.UserPassword)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized(new { message = "Credenciales incorrectas." });
            }
            UsuarioDTO usuarioDTO = new UsuarioDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                UserPassword = user.UserPassword,
                StrikeCount = user.StrikeCount,
                BackUpEmail = user.BackUpEmail,
                JobDescription = user.JobDescription,
                UserStatusId = user.UserStatusId,
                CountryId = user.CountryId,
                UserRoleId = user.UserRoleId
            };

            return Ok(usuarioDTO);
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        [HttpPost("AddUserFromApp")]

        public async Task<ActionResult<UsuarioDTO>> AddUserFromApp(UsuarioDTO user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User NuevoUsuarioNativo = new()
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                UserPassword = user.UserPassword,
                StrikeCount = user.StrikeCount,
                BackUpEmail = user.BackUpEmail,
                JobDescription = user.JobDescription,
                UserRole = null,
                UserStatus = null,
                Country = null
            };

            _context.Users.Add(NuevoUsuarioNativo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = NuevoUsuarioNativo.UserId }, NuevoUsuarioNativo);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
