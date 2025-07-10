using System;
using System.Net;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.OAuth2.Token.TestFramework;
using WTG.TrustedMessaging.MyAccount.Models;
using ZClientWebEDI.Test.WebApi.Controllers.IdentityAndSecurity;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class LicenceRegistrationV3ControllerTest : TrustedMessagingV3ControllerBaseTest
	{
		public void TestRegisterTenant()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate.Application.IDA_LD = Database.PK;
				existingCertificate.Application.IDA_ClientID = identityServer.ClientIdentifier;
				Factory.Save();

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var uri = new Uri("https://unit-testing/api/sso/v3/registration/tenant");
				var approvedAzps = new CodeDescriptionPairList();
				approvedAzps.AddPair(azp, "CSP");
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, approvedAzps);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var enterprise = Factory.New<LicenceEnterprise>();
				enterprise.LE_EnterpriseCode = "TST";
				enterprise.LE_OH = org.PK;
				Factory.Save();
				EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

				var info = new SystemToSystemTrustTenantRegistrationInfo()
				{
					Product = "CSP",
					TenantId = "hello",
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

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);
				AssertWebApi(uri, info, trustHelper, token, HttpStatusCode.OK, "{\"licence_id\":101}");
			}
		}

		public void TestRegisterTenant_DuplicatePreregisteredDatabase()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate.Application.IDA_LD = Database.PK;
				existingCertificate.Application.IDA_ClientID = identityServer.ClientIdentifier;
				Factory.Save();

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var uri = new Uri("https://unit-testing/api/sso/v3/registration/tenant");
				var approvedAzps = new CodeDescriptionPairList();
				approvedAzps.AddPair(azp, "CSP");
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, approvedAzps);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var enterprise = Factory.New<LicenceEnterprise>();
				enterprise.LE_EnterpriseCode = "TST";
				enterprise.LE_OH = org.PK;

				var db1 = enterprise.Databases.AddNew();
				db1.LD_ServerCode = "DB1";
				db1.LD_Product = "CSP";
				db1.LD_TenantID = string.Empty;
				db1.LD_Status = DatabaseStatusList.Codes.Preregistered;
				db1.LD_PreRegistrationExpiryDateUTC = ZDateTime.UtcNow.AddDays(10);
				org.OH_FullName = "Org Name 123";
				db1.LD_OH_WebAccessOrg = org.PK;

				Factory.Save();

				EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

				var info = new SystemToSystemTrustTenantRegistrationInfo()
				{
					Product = "CSP",
					TenantId = "hello",
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime(),
					LicenceType = "PRD",
					OrgName = "Org Name 123",
					EnterpriseID = "enterprise_id_1",
					EnterpriseCode = "enterprise_code_2",
					ServerCode = "server_code_3",
					CargowiseCompanyCode = "cargowise_company_code_4",
					DatabaseNumber = "",
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);
				AssertWebApi(uri, info, trustHelper, token, HttpStatusCode.OK, "{\"licence_id\":101}");

				var databaseReloaded = new BusinessObjectFactory().Load<LicenceDatabase>(db1.PK);
				AssertEquals("Should update tenant ID to match", info.TenantId, databaseReloaded.LD_TenantID);
				AssertEquals("Should have registered", DatabaseStatusList.Codes.REG, databaseReloaded.LD_Status);
			}
		}

		protected override TrustedController GetTrustedController(NLogWrapper logger, SystemToSystemTrustHelper trustHelper)
		{
			return new LicenceRegistrationV3Controller(logger, trustHelper);
		}
	}
}
