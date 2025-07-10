using System.Web.Http;
using CargoWise.Data;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso/v2/erequest")]
	public class ERequestV2Controller : ERequestTrustedSystemBaseController
	{
		public ERequestV2Controller() : base()
		{
		}

		public ERequestV2Controller(NLogWrapper logger) : base(logger)
		{
		}

		[Route("auto-login-url")]
		[HttpPost]
		public IHttpActionResult GetAutoLoginUrl([FromBody] ERequestInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, this);
				GetAutoLoginUrlCore(context);
				return context.CreateHttpActionResult();
			}
		}

		[Route("upload")]
		[HttpPost]
		public IHttpActionResult Upload([FromBody] ERequestInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<ERequestInfo, bool>.New(requestInfo, this);
				UploadCore(context);
				return context.CreateHttpActionResult();
			}
		}
	}
}
