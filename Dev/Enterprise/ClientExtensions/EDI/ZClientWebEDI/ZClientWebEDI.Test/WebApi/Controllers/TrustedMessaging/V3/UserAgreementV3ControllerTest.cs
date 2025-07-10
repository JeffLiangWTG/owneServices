using System;
using System.Net;
using System.Net.Http;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.OAuth2.Token.TestFramework;
using WTG.TrustedMessaging.MyAccount.Models;
using ZClientWebEDI.Test.WebApi.Controllers.IdentityAndSecurity;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class UserAgreementV3ControllerTest : TrustedMessagingV3ControllerBaseTest
	{
		public void TestGetRequiredUserAgreement()
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

				var uri = new Uri("https://unit-testing/api/sso/v3/user-agreement/agreement");
				var reqObj = new SystemToSystemUserAgreementInfo()
				{
					Product = "CW1",
					DatabaseNumber = "8000",
					UserId = "U048173",
					FullName = "User One",
					UserCountry = "AU",
					UserAgreementType = "MYA",
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				AssertWebApi(uri, reqObj, trustHelper, token, HttpStatusCode.OK, "{\"required\":false,\"allow_online_clickthrough\":false,\"title\":\"\",\"content\":\"\",\"version_number\":0,\"minor_version_number\":0,\"variant\":\"\",\"level\":\"\"}");
			}
		}

		public void TestAcknowledgeUserAgreement()
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

				var uri = new Uri("https://unit-testing/api/sso/v3/user-agreement/acknowledge");
				var reqObj = new SystemToSystemUserAgreementInfo()
				{
					Product = "CW1",
					DatabaseNumber = "8000",
					UserId = "U048173",
					FullName = "User One",
					UserCountry = "AU",
					UserAgreementType = "MYA",
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				AssertWebApi(uri, reqObj, trustHelper, token, HttpStatusCode.BadRequest, "{\"errors\":[{\"code\":\"2003\",\"message\":\"There are no current User Agreements for the given UserAgreementType and UserCountry combination.\"}]}");
			}
		}

		public void TestGetAcceptances()
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

				var uri = new Uri("https://unit-testing/api/sso/v3/user-agreement/acceptances");
				var reqObj = new SystemToSystemGetAcceptancesInfo()
				{
					Product = "CW1",
					DatabaseNumber = "8000",
					UserAgreementType = "MYA",
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				AssertWebApi(uri, HttpMethod.Post, reqObj, trustHelper, token, HttpStatusCode.OK, "{\"Acceptances\":[]}");
			}
		}

		protected override TrustedController GetTrustedController(NLogWrapper logger, SystemToSystemTrustHelper trustHelper)
		{
			return new UserAgreementV3Controller(logger, trustHelper);
		}
	}
}
