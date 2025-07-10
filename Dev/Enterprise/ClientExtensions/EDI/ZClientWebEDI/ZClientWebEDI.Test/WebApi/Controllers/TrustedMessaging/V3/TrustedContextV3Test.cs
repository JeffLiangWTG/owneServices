using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;
using WTG.TrustedMessaging.MyAccount.Models;
using ZClientWebEDI.Test.WebApi.Controllers.IdentityAndSecurity;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class TrustedContextV3Test : TestCaseWithFactory
	{
		public void TestNew()
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
				var product = "CW1";
				approvedAzps.AddPair(azp, product);
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, approvedAzps);

				var controller = new V3ControllerForTest();
				var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://www.cw1.com/api/sso/V3/test.auto-login-url"));
				request.Headers.Authorization = new AuthenticationHeaderValue(SystemToSystemTrustHelper.BearerKey, token);
				controller.Request = request;

				var requestInfo = new SystemToSystemTrustedInfoForTest() { Product = product, InfoTimestamp = ZDateTime.UtcNow.ToDateTime() };
				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);
				var context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, controller, trustHelper);

				AssertEquals(true, context.Success);
				AssertEquals(null, context.Messages);
				AssertEquals(product, context.Product);
				AssertNotNull(context.Token);
			}
		}

		public void TestCreateHttpActionResult()
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
				var product = "CW1";
				approvedAzps.AddPair(azp, product);
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, approvedAzps);

				var controller = new V3ControllerForTest(new NLogWrapperForTest(GetType()));
				var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://www.cw1.com/api/sso/V3/test.auto-login-url"));
				request.Headers.Authorization = new AuthenticationHeaderValue(SystemToSystemTrustHelper.BearerKey, token);
				controller.Request = request;
				controller.Configuration = new HttpConfiguration();

				var requestInfo = new SystemToSystemTrustedInfoForTest() { Product = product, InfoTimestamp = ZDateTime.UtcNow.ToDateTime() };
				var trustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);
				var context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, controller, trustHelper);
				context.ResponseInfo = new AutoLoginResponse(new Uri("https://www.cw.com/q?token=123"));

				AssertEquals(true, context.Success);
				AssertEquals(null, context.Messages);
				AssertEquals(product, context.Product);
				var responseMessage = context.CreateHttpActionResult().ExecuteAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
				AssertEquals("{\"url\":\"https://www.cw.com/q?token=123\"}", responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
				AssertContains("success,AutoLoginUrl: https://www.cw.com/q?token=123", controller.Logger.ToString());

				context.ResponseInfo = null;
				context.Success = false;
				context.Messages = new WTG.TrustedMessaging.Models.ErrorMessages();
				context.Messages.AddMessage("C01", "something wrong!");
				responseMessage = context.CreateHttpActionResult().ExecuteAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
				AssertEquals("{\"errors\":[{\"code\":\"C01\",\"message\":\"something wrong!\"}]}", responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
				AssertContains("C01 something wrong! | 400", controller.Logger.ToString());
			}
		}

		public void TestErrorMessages()
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
				var controller = new V3ControllerForTest();
				var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://www.cw1.com/api/sso/V3/test.auto-login-url"));
				request.Headers.Authorization = new AuthenticationHeaderValue(SystemToSystemTrustHelper.BearerKey, string.Empty);
				controller.Request = request;
				var requestInfo = new SystemToSystemTrustedInfoForTest() { Product = "CW1", InfoTimestamp = ZDateTime.UtcNow.ToDateTime() };

				var context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, controller);
				AssertEquals(false, context.Success);
				AssertEquals("Missing Authority Url", ErrorCodes.Codes.Server_Error, context.Messages.Messages.Single().Code);
				AssertEquals("Missing Authority Url", ErrorCodes.Descriptions.Server_Error, context.Messages.Messages.Single().Message);

				var trustedHelper = new SystemToSystemTrustHelperForTest(authorityUrl);

				context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, controller, trustedHelper);
				AssertEquals(false, context.Success);
				AssertEquals("Missing token", ErrorCodes.Codes.Validation_MissingSystemToSystemToken, context.Messages.Messages.Single().Code);
				AssertEquals("Missing token", ErrorCodes.Descriptions.Validation_MissingSystemToSystemToken, context.Messages.Messages.Single().Message);

				requestInfo.Product = string.Empty;
				request.Headers.Authorization = new AuthenticationHeaderValue(SystemToSystemTrustHelper.BearerKey, token);
				context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, controller, trustedHelper);
				AssertEquals(false, context.Success);
				AssertEquals("Product empty", ErrorCodes.Codes.Validation_InvalidSystem, context.Messages.Messages.Single().Code);
				AssertEquals("Product empty", ErrorCodes.Descriptions.Validation_InvalidSystem, context.Messages.Messages.Single().Message);

				requestInfo.Product = "CW1";
				requestInfo.InfoTimestamp = ZDateTime.UtcNow.AddHours(-25).ToDateTime();
				context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, controller, trustedHelper);
				AssertEquals(false, context.Success);
				AssertEquals("Request should have a 24 hour lifetime", ErrorCodes.Codes.Critical_InfoExpired, context.Messages.Messages.Single().Code);
				AssertEquals("Request should have a 24 hour lifetime", ErrorCodes.Descriptions.Critical_InfoExpired, context.Messages.Messages.Single().Message);

				requestInfo.InfoTimestamp = ZDateTime.UtcNow.AddHours(-20).ToDateTime();
				context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, controller, trustedHelper);
				AssertEquals(false, context.Success);
				AssertEquals("Request azp must be contained in registry", ErrorCodes.Codes.Validation_UnauthorizedParty, context.Messages.Messages.Single().Code);
				AssertEquals("Request azp must be contained in registry", ErrorCodes.Descriptions.Validation_UnauthorizedParty, context.Messages.Messages.Single().Message);

				var approvedAzps = new CodeDescriptionPairList();
				approvedAzps.AddPair(azp, "ZYX");
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, approvedAzps);
				context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, controller, trustedHelper);
				AssertEquals(false, context.Success);
				AssertEquals("Request azp product must match in registry", ErrorCodes.Codes.Validation_UnauthorizedParty, context.Messages.Messages.Single().Code);
				AssertEquals("Request azp product must match in registry", ErrorCodes.Descriptions.Validation_UnauthorizedParty, context.Messages.Messages.Single().Message);

				approvedAzps.AddPair(azp, "CW1");
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, approvedAzps);
				context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, controller, trustedHelper);
				AssertEquals(true, context.Success);
				AssertEquals("Should pass validation (azp and product match registry value)", null, context.Messages);
			}
		}

		protected override void SetUp()
		{
			TransactionedTestCase.RunClientDbCreateScripts();

			var product = "CW1";

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			Database = licence.Database;
			Database.LD_DatabaseNumber = 8000;
			Database.LD_Product = product;
			Database.LD_Password =
				"DE-3E-CC-F6-D3-2B-3E-58-E3-E1-55-80-F5-77-67-95-73-D9-1D-CD-C7-52-37-9A-C8-1A-E9-24-1B-23-75-7D-E9-B2-93-AB-BE-0E-2B-58-A8-8D-8B-64-03-33-95-DB-72-C2-1A-9C-1B-3F-BB-1E-8F-FB-5D-BE-B0-71-74-12";
			Database.LD_IsActive = true;
			Database.LD_Status = "REG";

			Factory.Save();

			base.SetUp();
		}

		LicenceDatabase Database;

		class SystemToSystemTrustedInfoForTest : SystemToSystemTrustedInfo
		{
		}
	}

	[RoutePrefix("api/sso/V3/test")]
	class V3ControllerForTest : TrustedController
	{
		public V3ControllerForTest() : base()
		{
		}

		public V3ControllerForTest(NLogWrapper logger) : base(logger)
		{
		}

		[Route("auto-login-url")]
		[HttpPost]
		public IHttpActionResult GetAutoLoginUrl([FromBody] SystemToSystemTrustedInfo requestInfo)
		{
			var context = TrustedContextV3<SystemToSystemTrustedInfo, AutoLoginResponse>.New(requestInfo, this);
			context.ResponseInfo = new AutoLoginResponse(new Uri("https://www.cw.com/q?token=123"));
			return context.CreateHttpActionResult();
		}
	}
}
