using System.Web.Http;
using CargoWise.Data;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso/v2/user-agreement")]
	public class UserAgreementV2Controller : UserAgreementTrustedSystemBaseController
	{
		public UserAgreementV2Controller() : base()
		{
		}

		public UserAgreementV2Controller(NLogWrapper logger) : base(logger)
		{
		}

		[HttpPost]
		[Route("agreement")]
		public IHttpActionResult GetRequiredUserAgreement([FromBody] UserAgreementInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<UserAgreementInfo, UserAgreementResponseData>.New(requestInfo, this);
				GetRequiredUserAgreementCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[HttpPost]
		[Route("acknowledge")]
		public IHttpActionResult AcknowledgeUserAgreement([FromBody] UserAgreementInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<UserAgreementInfo, bool>.New(requestInfo, this);
				AcknowledgeUserAgreementCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[HttpPost]
		[Route("enterprise-agreement")]
		public IHttpActionResult GetRequiredEnterpriseUserAgreementUrl([FromBody] EnterpriseAgreementInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<EnterpriseAgreementInfo, EnterpriseAgreementResponseData>.New(requestInfo, this);
				GetRequiredEnterpriseUserAgreementUrlCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[HttpGet]
		[Route("acceptances")]
		public IHttpActionResult GetAcceptances([FromBody] GetAcceptancesInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<GetAcceptancesInfo, GetAcceptancesResponse>.New(requestInfo, this);
				GetAcceptancesCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}
	}
}
