using System.Web.Http;
using CargoWise.Data;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/UserAgreement")]
	public class UserAgreementV1Controller : UserAgreementTrustedSystemBaseController
	{
		public UserAgreementV1Controller() : base()
		{
		}

		public UserAgreementV1Controller(NLogWrapper logger) : base(logger)
		{
		}

		[HttpPost]
		[Route("Agreement")]
		public IHttpActionResult GetRequiredUserAgreement([FromBody] TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<UserAgreementInfo, UserAgreementResponseData>.New(trustedRequest, this);
				GetRequiredUserAgreementCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[HttpPost]
		[Route("EnterpriseAgreement")]
		public IHttpActionResult GetRequiredEnterpriseUserAgreementUrl([FromBody] TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<EnterpriseAgreementInfo, EnterpriseAgreementResponseData>.New(trustedRequest, this);
				GetRequiredEnterpriseUserAgreementUrlCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[HttpPost]
		[Route("Acknowledge")]
		public IHttpActionResult AcknowledgeUserAgreement([FromBody] TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<UserAgreementInfo, bool>.New(trustedRequest, this);
				AcknowledgeUserAgreementCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}

		[HttpPost]
		[Route("Acceptances")]
		public IHttpActionResult GetAcceptances([FromBody] TrustedRequest requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<GetAcceptancesInfo, GetAcceptancesResponse>.New(requestInfo, this);
				GetAcceptancesCore(context, context.RequestInfo);
				return context.CreateHttpActionResult();
			}
		}
	}
}
