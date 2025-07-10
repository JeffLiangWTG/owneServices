using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso")]
	public class SlidingExpirationController : ControllerWithEnvironment
	{
		[HttpGet]
		[Route("sliding-expiration")]
		public HttpResponseMessage UpdateCookieExpiry()
		{
			MyAccountLoginLiteHelper.RefreshCookiesExpire();
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[Route("sliding-expiration")]
		[HttpOptions]
		public HttpResponseMessage Options()
		{
			var requestOrigin = Request.Headers.GetValues("Origin").FirstOrDefault() ?? string.Empty;

			if (string.Equals(requestOrigin, "https://myaccount.cargowise.com", StringComparison.OrdinalIgnoreCase))
			{
				return new HttpResponseMessage(HttpStatusCode.OK);
			}

			return new HttpResponseMessage(HttpStatusCode.Forbidden);
		}
	}
}
