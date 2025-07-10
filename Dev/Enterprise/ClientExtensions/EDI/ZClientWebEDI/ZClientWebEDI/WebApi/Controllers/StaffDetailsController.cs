using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/StaffDetails")]
	public class StaffDetailsController : BusinessObjectController
	{
		[HttpGet]
		[Route("GetUserEmailAddress")]
		public IHttpActionResult GetUserEmailAddress(string username)
		{
			if (!IsValidClientRequest())
			{
				return Content(HttpStatusCode.Forbidden, "Invalid IP");
			}

			var usernameWithoutDomain = RemoveDomainPrefix(username);
			if (!IsValidUsername(usernameWithoutDomain, out var errorMessage))
			{
				return BadRequest(errorMessage);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var email = FindStaff(usernameWithoutDomain)?.GS_EmailAddress;
				if (email.HasValue)
				{
					return Ok(email.Value.ToString());
				}
				else
				{
					return NotFound();
				}
			}
		}

		[HttpGet]
		[Route("GetUserStaffCode")]
		public IHttpActionResult GetUserStaffCode(string username)
		{
			if (!IsValidClientRequest())
			{
				return Content(HttpStatusCode.Forbidden, "Invalid IP");
			}

			var usernameWithoutDomain = RemoveDomainPrefix(username);
			if (!IsValidUsername(usernameWithoutDomain, out var errorMessage))
			{
				return BadRequest(errorMessage);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var staffCode = FindStaff(usernameWithoutDomain)?.GS_Code;
				if (staffCode.HasValue)
				{
					return Ok(staffCode.Value.ToString());
				}
				else
				{
					return NotFound();
				}
			}
		}

		static readonly Regex UserNameAllowedRegEx = new Regex(@"^[a-zA-Z]{1}[a-zA-Z0-9\._\-]{1,34}$", RegexOptions.Compiled);
		public static bool IsValidUsername(string userName, out string errorMessage)
		{
			if (!string.IsNullOrWhiteSpace(userName) && UserNameAllowedRegEx.IsMatch(userName))
			{
				errorMessage = null;
				return true;
			}
			errorMessage = $"Invalid Username [{userName}]";
			return false;
		}

		GlbStaff FindStaff(string username)
		{
			var factory = new BusinessObjectFactory();
			return factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, RemoveDomainPrefix(username))).FirstOrDefault();
		}

		string RemoveDomainPrefix(string username) => username.Split('\\').Last();

		static bool IsValidClientRequest()
		{
			var userHostAddress = HttpContext.Current.Request.UserHostAddress ?? string.Empty;
			if (userHostAddress == "127.0.0.1" || userHostAddress == "::1")
			{
				return true;
			}

			return RequestAuthorisationHelper.IsRequestPermitted(HttpContext.Current.Request);
		}
	}
}
