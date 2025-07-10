using System.Web.Http;
using CargoWise.Data;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso/v2/registration")]
	public class LicenceRegistrationV2Controller : LicenceRegistrationTrustedSystemBaseController
	{
		public LicenceRegistrationV2Controller() : base()
		{
		}

		public LicenceRegistrationV2Controller(NLogWrapper logger) : base(logger)
		{
		}

		[Route("tenant")]
		[HttpPost]
		public IHttpActionResult RegisterTenant([FromBody] TenantRegistrationInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV2<TenantRegistrationInfo, ProductRegistrationResponse>.New(requestInfo, this);
				RegisterTenantCore(context);
				return context.CreateHttpActionResult();
			}
		}
	}
}
