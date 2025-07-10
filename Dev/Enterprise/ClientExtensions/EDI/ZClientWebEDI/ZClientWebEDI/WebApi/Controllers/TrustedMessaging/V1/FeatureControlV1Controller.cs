using System.Web.Http;
using CargoWise.Data;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/FeatureControl")]
	public class FeatureControlV1Controller : FeatureControlBaseController
	{
		public FeatureControlV1Controller() : base()
		{
		}

		public FeatureControlV1Controller(NLogWrapper logger) : base(logger)
		{
		}

		[HttpPost]
		[Route("Rule")]
		public IHttpActionResult GetRequiredFeatureControl([FromBody] TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<FeatureControlRequest, FeatureControlResponse>.New(trustedRequest, this);
				GetFeatureControlRuleCore(context);
				return context.CreateHttpActionResult();
			}
		}
	}
}
