using System.Web.Http;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso/v2/trusted-messaging")]
	public class TrustedMessagingV2Controller : TrustedMessagingBaseController
	{
		public TrustedMessagingV2Controller() : base()
		{
		}

		public TrustedMessagingV2Controller(NLogWrapper logger) : base(logger)
		{
		}

		[HttpPost]
		[Route("certificate")]
		public IHttpActionResult Certificate([FromBody] CertificateInfo requestInfo)
		{
			var context = TrustedContextV2<CertificateInfo, CertificateResponse>.New(requestInfo, this);
			CertificateCore(context);

			if (context.ResponseInfo == null && context.Messages == null)
			{
				return NotFound();
			}
			else
			{
				return context.CreateHttpActionResult();
			}
		}
	}
}
