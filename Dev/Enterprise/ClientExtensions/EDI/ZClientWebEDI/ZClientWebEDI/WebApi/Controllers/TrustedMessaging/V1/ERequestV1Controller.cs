using System.Web.Http;
using CargoWise.Data;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/ERequest")]
	public class ERequestV1Controller : ERequestTrustedSystemBaseController
	{
		public ERequestV1Controller() : base()
		{
		}

		public ERequestV1Controller(NLogWrapper logger) : base(logger)
		{
		}

		[Route("AutoLoginUrl")]
		[HttpPost]
		public IHttpActionResult GetAutoLoginUrl([FromBody] TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<ERequestInfo, AutoLoginResponse>.New(trustedRequest, this);
				GetAutoLoginUrlCore(context);
				return context.CreateHttpActionResult();
			}
		}

		[Route("Upload")]
		[HttpPost]
		public IHttpActionResult Upload([FromBody] TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<ERequestInfo, bool>.New(trustedRequest, this);
				UploadCore(context);
				return context.CreateHttpActionResult();
			}
		}
	}
}
