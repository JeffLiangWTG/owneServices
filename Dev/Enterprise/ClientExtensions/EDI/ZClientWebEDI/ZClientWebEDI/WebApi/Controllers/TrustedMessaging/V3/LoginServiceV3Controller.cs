using System.Web.Http;
using CargoWise.Data;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso/v3/my-account")]
	public class LoginServiceV3Controller : LoginServiceBaseController<SystemToSystemTrustedUserInfo>
	{
		public LoginServiceV3Controller() : base()
		{
		}

		public LoginServiceV3Controller(NLogWrapper logger) : base(logger)
		{
		}

		public LoginServiceV3Controller(NLogWrapper logger, SystemToSystemTrustHelper trustHelper) : base(logger)
		{
			TrustHelper = trustHelper;
		}

		SystemToSystemTrustHelper TrustHelper { get; }

		[Route("auto-login-url")]
		[HttpPost]
		public IHttpActionResult GetAutoLoginUrl([FromBody] SystemToSystemTrustedAutoLoginInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV3<SystemToSystemTrustedAutoLoginInfo, AutoLoginResponse>.New(requestInfo, this, TrustHelper);
				GetAutoLoginUrlCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[Route("user-account")]
		[HttpPost]
		public IHttpActionResult UpdateUserAccount([FromBody] SystemToSystemUpdateUserInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV3<SystemToSystemUpdateUserInfo, bool>.New(requestInfo, this, TrustHelper);
				UpdateUserAccountCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, SystemToSystemTrustedUserInfo info)
		{
			return SystemToSystemTrustHelper.GetLicenceDatabase(Factory, context, info);
		}
	}
}
