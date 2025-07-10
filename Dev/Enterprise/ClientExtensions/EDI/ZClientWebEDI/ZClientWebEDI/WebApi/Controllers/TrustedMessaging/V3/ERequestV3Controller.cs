using System.Web.Http;
using CargoWise.Data;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso/v3/erequest")]
	public class ERequestV3Controller : ERequestBaseController<SystemToSystemERequestInfo>
	{
		public ERequestV3Controller() : base()
		{
		}

		public ERequestV3Controller(NLogWrapper logger) : base(logger)
		{
		}

		public ERequestV3Controller(NLogWrapper logger, SystemToSystemTrustHelper trustHelper) : base(logger)
		{
			TrustHelper = trustHelper;
		}

		SystemToSystemTrustHelper TrustHelper { get; }

		[Route("auto-login-url")]
		[HttpPost]
		public IHttpActionResult GetAutoLoginUrl([FromBody] SystemToSystemERequestInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV3<SystemToSystemERequestInfo, AutoLoginResponse>.New(requestInfo, this, TrustHelper);
				GetAutoLoginUrlCore(context);
				return context.CreateHttpActionResult();
			}
		}

		[Route("upload")]
		[HttpPost]
		public IHttpActionResult Upload([FromBody] SystemToSystemERequestInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV3<SystemToSystemERequestInfo, bool>.New(requestInfo, this, TrustHelper);
				UploadCore(context);
				return context.CreateHttpActionResult();
			}
		}

		protected void GetAutoLoginUrlCore(TrustedContextV3<SystemToSystemERequestInfo, AutoLoginResponse> context)
		{
			GetAutoLoginUrlCore(context, context.RequestInfo);
		}

		protected void UploadCore(TrustedContextV3<SystemToSystemERequestInfo, bool> context)
		{
			UploadCore(context, context.RequestInfo);
		}

		protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, SystemToSystemERequestInfo info)
		{
			return SystemToSystemTrustHelper.GetLicenceDatabase(Factory, context, info);
		}
	}
}
