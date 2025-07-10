using System.Net;
using WTG.TrustedMessaging.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class OAuthResponse<TResponse>
	{
		public TResponse ResponseInfo { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public ErrorMessages Messages { get; set; }
	}
}
