using System;
using System.Net;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.OAuth2.Token.TestFramework;
using WTG.TrustedMessaging.MyAccount.Models;
using ZClientWebEDI.Test.WebApi.Controllers.IdentityAndSecurity;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class LoginServiceV3ControllerTest : TrustedMessagingV3ControllerBaseTest
	{
		public void TestGetAutoLoginUrl()
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
				var approvedAzps = new CodeDescriptionPairList();
				approvedAzps.AddPair(azp, "CW1");
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, approvedAzps);

				var uri = new Uri("https://unit-testing/api/sso/v3/my-account/auto-login-url");
				var reqObj = new SystemToSystemTrustedAutoLoginInfo()
				{
					Product = "CW1",
					DatabaseNumber = "8000",
					UserId = "U048173",
					FullName = "User One",
					Email = "user.one@test.com",
					ReturnUrl = new Uri("https://www.cw1.com"),
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime(),
					UserCountry = "AU"
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				var (statusCode, content) = CallWebApi(uri, reqObj, trustHelper, token);
				AssertEquals(HttpStatusCode.OK, statusCode);
				Assert(content.StartsWith("{\"url\":\"https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token="));
			}
		}

		public void TestUpdateUserAccount()
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
				var approvedAzps = new CodeDescriptionPairList();
				approvedAzps.AddPair(azp, "CW1");
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, approvedAzps);

				var uri = new Uri("https://unit-testing/api/sso/v3/my-account/user-account");
				var reqObj = new SystemToSystemUpdateUserInfo()
				{
					Product = "CW1",
					DatabaseNumber = "8000",
					UserId = "U048173",
					FullName = "User One",
					Email = "user.one@test.com",
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime(),
					UserCountry = "AU",
					IsActive = true,
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				AssertWebApi(uri, reqObj, trustHelper, token, HttpStatusCode.OK, "");
			}
		}

		public void TestUpdateUserAccount_UpdateIsActive()
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
				var approvedAzps = new CodeDescriptionPairList();
				approvedAzps.AddPair(azp, "CW1");
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, approvedAzps);

				var uri = new Uri("https://unit-testing/api/sso/v3/my-account/user-account");
				var reqObj = new SystemToSystemUpdateUserInfo()
				{
					Product = "CW1",
					DatabaseNumber = "8000",
					UserId = "U048173",
					FullName = "User One",
					Email = "user.one@test.com",
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime(),
					UserCountry = "AU",
					IsActive = false,
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				var userAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, reqObj.UserId));
				AssertNotNull("Precondition", userAccount);
				AssertEquals("Precondition: Should have been active", true, userAccount.EUA_IsActive);
				AssertWebApi(uri, reqObj, trustHelper, token, HttpStatusCode.OK, "");

				var userAccountReloaded = new BusinessObjectFactory().LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, reqObj.UserId));
				AssertEquals("Active status should have been updated", false, userAccountReloaded.EUA_IsActive);
			}
		}

		protected override TrustedController GetTrustedController(NLogWrapper logger, SystemToSystemTrustHelper trustHelper)
		{
			return new LoginServiceV3Controller(logger, trustHelper);
		}
	}
}
