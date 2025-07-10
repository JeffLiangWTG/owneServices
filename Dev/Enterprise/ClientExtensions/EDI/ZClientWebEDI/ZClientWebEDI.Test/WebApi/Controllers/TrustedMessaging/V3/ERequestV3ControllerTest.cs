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
	public class ERequestV3ControllerTest : TrustedMessagingV3ControllerBaseTest
	{
		public void TestGetAutoLoginUrl()
		{
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.GlowNewERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "eRequestPortal#/workflow");

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

				var uri = new Uri("https://unit-testing/api/sso/v3/erequest/auto-login-url");
				var reqObj = new SystemToSystemERequestInfo()
				{
					LandingPageId = UserPortal.UserPortalLauncher.eRequestNewLandingPageId,
					Product = "CW1",
					DatabaseNumber = "8000",
					UserId = "U048173",
					FullName = "User One",
					Email = "user.one@test.com",
					Module = "RAT",
					SubModule = "AIRFreightRate",
					Criticality = "CR4",
					ReferenceId = "6E6B6A65-609E-41BE-97F6-19C743D790FC",
					IncidentNumber = "",
					LicenceCode = "",
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				var (statusCode, content) = CallWebApi(uri, HttpMethod.Post, reqObj, trustHelper, token);
				AssertEquals(HttpStatusCode.OK, statusCode);
				Assert(content.StartsWith("{\"url\":\"https://myaccount-portal.cargowise.com/myaccount/Login/GlowPortalAutoLogin.aspx?qdata="));
			}
		}

		public void TestGetAutoLoginUrl_InvalidDatabaseNumber()
		{
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.GlowNewERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "eRequestPortal#/workflow");

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

				var uri = new Uri("https://unit-testing/api/sso/v3/erequest/auto-login-url");
				var reqObj = new SystemToSystemERequestInfo()
				{
					LandingPageId = UserPortal.UserPortalLauncher.eRequestNewLandingPageId,
					Product = "CW1",
					DatabaseNumber = "invalid",
					UserId = "U048173",
					FullName = "User One",
					Email = "user.one@test.com",
					Module = "RAT",
					SubModule = "AIRFreightRate",
					Criticality = "CR4",
					ReferenceId = "6E6B6A65-609E-41BE-97F6-19C743D790FC",
					IncidentNumber = "",
					LicenceCode = "",
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				var (statusCode, content) = CallWebApi(uri, reqObj, trustHelper, token);
				AssertEquals(HttpStatusCode.BadRequest, statusCode);
				Assert("Should not succeed because the database number doesn't match a database", content.Contains("Database Number is not valid."));

				reqObj.DatabaseNumber = "8001";
				(statusCode, content) = CallWebApi(uri, reqObj, trustHelper, token);
				AssertEquals(HttpStatusCode.BadRequest, statusCode);
				Assert("Should not succeed because the database number doesn't match a database", content.Contains("Database Number is not valid."));

				reqObj.DatabaseNumber = "8000";
				(statusCode, content) = CallWebApi(uri, reqObj, trustHelper, token);
				AssertEquals(HttpStatusCode.OK, statusCode);
				Assert(content.StartsWith("{\"url\":\"https://myaccount-portal.cargowise.com/myaccount/Login/GlowPortalAutoLogin.aspx?qdata="));
			}
		}

		public void TestUpload()
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

				var uri = new Uri("https://unit-testing/api/sso/v3/erequest/upload");
				var reqObj = new SystemToSystemERequestInfo()
				{
					Product = "CW1",
					DatabaseNumber = "8000",
					ERequestDocument = ERequestBaseControllerTest.CreateERequestDocumentAsXmString(ZGuid.NewZGuid(), "20200218_000000"),
					InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
				};

				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				var (statusCode, content) = CallWebApi(uri, reqObj, trustHelper, token);
				AssertEquals(HttpStatusCode.OK, statusCode);
				AssertEquals("", content);
			}
		}

		protected override TrustedController GetTrustedController(NLogWrapper logger, SystemToSystemTrustHelper trustHelper)
		{
			return new ERequestV3Controller(logger, trustHelper);
		}
	}
}
