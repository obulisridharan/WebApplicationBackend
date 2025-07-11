using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using WebApplication1.model;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]


    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config) => _config = config;

        private MySqlConnection GetConnection()
            => new MySqlConnection(_config.GetConnectionString("projectConnection"));


        [HttpGet]
        public IActionResult GetAll()
        {
            var list = new List<Student>();

            using var conn = GetConnection();
            conn.Open();

            const string sql = "SELECT id, name, age, dept FROM student";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new Student
                {
                    id = rdr.GetInt32("id"),
                    name = rdr.GetString("name"),
                    age = rdr.GetInt32("age"),
                    dept = rdr.GetString("dept")
                });
            }
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            using var conn = GetConnection();
            conn.Open();

            const string sql =
                "SELECT id, name, age, dept FROM student WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read()) return NotFound($"No student with id {id}");

            var s = new Student
            {
                id = rdr.GetInt32("id"),
                name = rdr.GetString("name"),
                age = rdr.GetInt32("age"),
                dept = rdr.GetString("dept")
            };
            return Ok(s);
        }



        [HttpPost]
        public IActionResult Create([FromBody] Student dto)
        {
            if (dto is null) return BadRequest("Body required");

            using var conn = GetConnection();
            conn.Open();

            const string sql =
                @"INSERT INTO student (id, name, age, dept)
                  VALUES (@id, @name, @age, @dept)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", dto.id);
            cmd.Parameters.AddWithValue("@name", dto.name);
            cmd.Parameters.AddWithValue("@age", dto.age);
            cmd.Parameters.AddWithValue("@dept", dto.dept);

            int rows = cmd.ExecuteNonQuery();
            return rows > 0
                ? CreatedAtAction(nameof(GetById), new { id = dto.id }, dto)
                : BadRequest("Insert failed");
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Student dto)
        {
            if (dto is null || id != dto.id) return BadRequest("id mismatch");

            using var conn = GetConnection();
            conn.Open();

            const string sql =
                @"UPDATE student
                  SET name = @name, age = @age, dept = @dept
                  WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", dto.id);
            cmd.Parameters.AddWithValue("@name", dto.name);
            cmd.Parameters.AddWithValue("@age", dto.age);
            cmd.Parameters.AddWithValue("@dept", dto.dept);

            int rows = cmd.ExecuteNonQuery();
            return rows > 0 ? Ok(dto) : NotFound("Update failed");
        }


        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            using var conn = GetConnection();
            conn.Open();

            const string sql = "DELETE FROM student WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();
            return rows > 0
                ? Ok($"Student {id} deleted")
                : NotFound($"No student with id {id}");
        }
    }
}
