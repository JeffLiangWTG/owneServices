using System;
using System.Net;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class LicenceRegistrationV2ControllerTest : TrustedMessagingV2ControllerBaseTest
	{
		public void TestRegisterTenant()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/registration/tenant");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;
			TrustedSystem.ETS_Product = Database.LD_Product = "CSP";
			TrustedSystem.ETS_SystemID = "8000";
			Factory.Save();
			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

			var info = new TenantRegistrationInfo()
			{
				Product = "CSP",
				SystemId = "8000",
				TenantId = "DEMO",
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime(),
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			AssertWebApi(uri, info, HttpStatusCode.OK, "{\"database_number\":100,\"myaccount_endpoint_base_url\":null}");
		}

		protected override TrustedController GetTrustedController(NLogWrapper logger)
		{
			return new LicenceRegistrationV2Controller(logger);
		}
	}
}
