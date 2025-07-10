using System.Web.Http;
using CargoWise.Data;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/sso/v3/registration")]
	public class LicenceRegistrationV3Controller : LicenceRegistrationBaseController<SystemToSystemTrustTenantRegistrationInfo>
	{
		public LicenceRegistrationV3Controller() : base()
		{
		}

		public LicenceRegistrationV3Controller(NLogWrapper logger) : base(logger)
		{
		}

		public LicenceRegistrationV3Controller(NLogWrapper logger, SystemToSystemTrustHelper trustHelper) : base(logger)
		{
			TrustHelper = trustHelper;
		}

		[Route("tenant")]
		[HttpPost]
		public IHttpActionResult RegisterTenant([FromBody] SystemToSystemTrustTenantRegistrationInfo requestInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV3<SystemToSystemTrustTenantRegistrationInfo, LicenceRegistrationV3Response>.New(requestInfo, this, TrustHelper);
				var response = RegisterTenantCore(context, context.RequestInfo);

				if (response != null)
				{
					context.ResponseInfo = (LicenceRegistrationV3Response)response;
				}

				return context.CreateHttpActionResult();
			}
		}

		SystemToSystemTrustHelper TrustHelper { get; }

		protected override LicenceDatabaseRegistrationImporter.ImportResult TryImportLicenceDatabase(SystemToSystemTrustTenantRegistrationInfo tenantRegistrationInfo, LicenceDatabase duplicateDatabase = null)
		{
			return LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(tenantRegistrationInfo, duplicateDatabase);
		}

		protected override ILicenceRegistrationResponse GetProductRegistrationResponse(int databaseNumber)
		{
			return new LicenceRegistrationV3Response() { DatabaseNumber = databaseNumber };
		}
	}
}
