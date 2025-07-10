using System.Web.Http;
using CargoWise.Data;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/LoginService")]
	public class LoginServiceV1Controller : LoginServiceTrustedSystemBaseController
	{
		public LoginServiceV1Controller() : base()
		{
		}

		public LoginServiceV1Controller(NLogWrapper logger) : base(logger)
		{
		}

		[Route("AutoLoginUrl")]
		[HttpPost]
		public IHttpActionResult GetAutoLoginUrl([FromBody] TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<TrustedAutoLoginInfo, AutoLoginResponse>.New(trustedRequest, this);
				GetAutoLoginUrlCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[Route("UserAccount")]
		[HttpPost]
		public IHttpActionResult UpdateUserAccount([FromBody] TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<UpdateUserInfo, bool>.New(trustedRequest, this);
				UpdateUserAccountCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}
	}
}
