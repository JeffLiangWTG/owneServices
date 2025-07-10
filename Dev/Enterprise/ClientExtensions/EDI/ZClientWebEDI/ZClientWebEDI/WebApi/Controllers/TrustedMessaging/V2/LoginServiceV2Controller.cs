using System.Web.Http;
using CargoWise.Data;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso/v2/my-account")]
	public class LoginServiceV2Controller : LoginServiceTrustedSystemBaseController
	{
		public LoginServiceV2Controller() : base()
		{
		}

		public LoginServiceV2Controller(NLogWrapper logger) : base(logger)
		{
		}

		[Route("auto-login-url")]
		[HttpPost]
		public IHttpActionResult GetAutoLoginUrl([FromBody] TrustedAutoLoginInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<TrustedAutoLoginInfo, AutoLoginResponse>.New(requestInfo, this);
				GetAutoLoginUrlCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[Route("user-account")]
		[HttpPost]
		public IHttpActionResult UpdateUserAccount([FromBody] UpdateUserInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<UpdateUserInfo, bool>.New(requestInfo, this);
				UpdateUserAccountCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[Route("oauth-login-url")]
		[HttpPost]
		public IHttpActionResult GetOAuthLoginUrl([FromBody] AuthenticationTokenInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<AuthenticationTokenInfo, OAuthLoginResponse>.New(requestInfo, this);
				GetOAuthLoginUrlCore(context);
				return context.CreateHttpActionResult();
			}
		}
	}
}
