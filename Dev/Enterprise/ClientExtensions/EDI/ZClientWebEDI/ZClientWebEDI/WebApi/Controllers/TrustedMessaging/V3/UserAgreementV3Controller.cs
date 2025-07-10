using System.Web.Http;
using CargoWise.Data;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso/v3/user-agreement")]
	public class UserAgreementV3Controller : UserAgreementBaseController<SystemToSystemUserAgreementInfo, SystemToSystemGetAcceptancesInfo, SystemToSystemTrustedRegisteredInfo>
	{
		public UserAgreementV3Controller() : base()
		{
		}

		public UserAgreementV3Controller(NLogWrapper logger) : base(logger)
		{
		}

		public UserAgreementV3Controller(NLogWrapper logger, SystemToSystemTrustHelper trustHelper) : base(logger)
		{
			TrustHelper = trustHelper;
		}

		SystemToSystemTrustHelper TrustHelper { get; }

		[HttpPost]
		[Route("agreement")]
		public IHttpActionResult GetRequiredUserAgreement([FromBody] SystemToSystemUserAgreementInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV3<SystemToSystemUserAgreementInfo, UserAgreementResponseData>.New(requestInfo, this, TrustHelper);
				GetRequiredUserAgreementCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[HttpPost]
		[Route("acknowledge")]
		public IHttpActionResult AcknowledgeUserAgreement([FromBody] SystemToSystemUserAgreementInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV3<SystemToSystemUserAgreementInfo, bool>.New(requestInfo, this, TrustHelper);
				AcknowledgeUserAgreementCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[HttpPost]
		[Route("acceptances")]
		public IHttpActionResult GetAcceptances([FromBody] SystemToSystemGetAcceptancesInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV3<SystemToSystemGetAcceptancesInfo, GetAcceptancesResponse>.New(requestInfo, this, TrustHelper);
				GetAcceptancesCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, SystemToSystemTrustedRegisteredInfo info)
		{
			return SystemToSystemTrustHelper.GetLicenceDatabase(Factory, context, info);
		}

		protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, IEnterpriseAgreementInfo info)
		{
			return SystemToSystemTrustHelper.GetLicenceDatabase(Factory, context, (SystemToSystemTrustedRegisteredInfo)info);
		}
	}
}
