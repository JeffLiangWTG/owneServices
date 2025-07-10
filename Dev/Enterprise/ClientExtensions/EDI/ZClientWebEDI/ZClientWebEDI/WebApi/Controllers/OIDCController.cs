using System.Web.Http;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers
{
	[RoutePrefix("api/oidc")]
	public class OIDCController : TrustedController
	{
		public OIDCController() : base() { }

		public OIDCController(NLogWrapper logger) : base(logger) { }

		[HttpPost]
		[Route("forget-user")]
		public IHttpActionResult ClearCookieEndpoint()
		{
			ZPage.AppInstance.ApplicationCookie.WriteUser("", "", "");
			return Ok();
		}
		protected internal ZPage ZPage => new ZPage();
	}
}
