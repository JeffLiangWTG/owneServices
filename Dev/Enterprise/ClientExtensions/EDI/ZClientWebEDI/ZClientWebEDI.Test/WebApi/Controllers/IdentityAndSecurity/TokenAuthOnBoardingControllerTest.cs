using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZClientWebCargoWiseEDI;
using Enterprise.ZClientWebCargoWiseEDI.Testing;
using Newtonsoft.Json;
using WTG.OAuth2.Token.TestFramework;
using WTG.TrustedMessaging.MyAccount.Models;

namespace ZClientWebEDI.Test.WebApi.Controllers.IdentityAndSecurity
{
	[HttpContextEnabledTest]
	class TokenAuthOnBoardingControllerTest : TestCaseWithFactory
	{
		#region TestFetchOidcConfig

		public void TestFetchOidcConfig_NoAccessToken()
		{
			var logger = new NLogWrapperForTest(GetType());
			var databaseNumber = LicenseDatabaseTest.LD_DatabaseNumber.ToString();
			var response = InvokeFetchOidcConfig(databaseNumber, "", logger, $"https://unit-testing/{FetchOidcConfigUrl}");
			var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestFetchOidcConfig_InvalidToken()
		{
			var logger = new NLogWrapperForTest(GetType());
			var databaseNumber = LicenseDatabaseTest.LD_DatabaseNumber.ToString();
			var response = InvokeFetchOidcConfig(databaseNumber, "Invalid Access Token", logger, $"https://unit-testing/{FetchOidcConfigUrl}");
			var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestFetchOidcConfig_InvalidLicensePk()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var logger = new NLogWrapperForTest(GetType());
				var databaseNumber = "123q";
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeFetchOidcConfig(databaseNumber, token, logger, $"https://unit-testing/{FetchOidcConfigUrl}", authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

				databaseNumber = "123";
				response = InvokeFetchOidcConfig(databaseNumber, token, logger, $"https://unit-testing/{FetchOidcConfigUrl}", authorityUrl);
				var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: Database Number Is Invalid";
				AssertContains(message, logger.ToString());
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestFetchOidcConfig_NullEdiTokenAuthOnBoardingData()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var logger = new NLogWrapperForTest(GetType());
				var databaseNumber = LicenseDatabaseTest.LD_DatabaseNumber.ToString();
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeFetchOidcConfig(databaseNumber, token, logger, $"https://unit-testing/{FetchOidcConfigUrl}", authorityUrl);
				var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: Database Number Is Invalid";
				AssertContains(message, logger.ToString());
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestFetchOidcConfig_BadStatusWithEdiTokenAuthOnBoardingDataStatus()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Queued;
				ediTokenAuthOnBoardingData.TOD_LE = LicenseDatabaseTest.LD_LE;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier = OIDCClaimMappingIdentifiers.Codes.LoginName;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingName = "user_name";
				ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "Azure";
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "AZU";
				ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
				ediTokenAuthOnBoardingData.TOD_Retry = 5;
				ediTokenAuthOnBoardingData.TOD_VerificationUsername = "user";
				ediTokenAuthOnBoardingData.TOD_VerificationUserPassword = "password";
				Factory.Save();

				EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection(identityServer.ClientIdentifier));

				var logger = new NLogWrapperForTest(GetType());
				var databaseNumber = LicenseDatabaseTest.LD_DatabaseNumber.ToString();
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeFetchOidcConfig(databaseNumber, token, logger, $"https://unit-testing/{FetchOidcConfigUrl}", authorityUrl);
				var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: OIDC Settings Are Not Ready";
				AssertContains(message, logger.ToString());
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Completed;
				Factory.Save();
				logger.ClearLog();
				response = InvokeFetchOidcConfig(databaseNumber, token, logger, $"https://unit-testing/{FetchOidcConfigUrl}", authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
			}
		}

		public void TestFetchOidcConfig()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Verified;
				ediTokenAuthOnBoardingData.TOD_LE = LicenseDatabaseTest.LD_LE;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier = OIDCClaimMappingIdentifiers.Codes.LoginName;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingName = "user_name";
				ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "Azure";
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "AZU";
				ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
				ediTokenAuthOnBoardingData.TOD_Retry = 5;
				ediTokenAuthOnBoardingData.TOD_VerificationUsername = "user";
				ediTokenAuthOnBoardingData.TOD_VerificationUserPassword = "password";
				ediTokenAuthOnBoardingData.Environment = AzureB2CEnvironmentCodeDescriptionList.Codes.PRD;
				Factory.Save();

				EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection(identityServer.ClientIdentifier));

				var logger = new NLogWrapperForTest(GetType());
				var databaseNumber = LicenseDatabaseTest.LD_DatabaseNumber.ToString();
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeFetchOidcConfig(databaseNumber, token, logger, $"https://unit-testing/{FetchOidcConfigUrl}", authorityUrl);
				var tokenAuthOnboardingDataResponse = JsonConvert.DeserializeObject<TokenAuthOnboardingDataResponse>(response.Content.ReadAsStringAsync().Result);
				AssertNotNull(tokenAuthOnboardingDataResponse);
				AssertEquals(ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier, tokenAuthOnboardingDataResponse.DomainHint);
				AssertEquals(identityServer.ClientIdentifier, tokenAuthOnboardingDataResponse.ConfigurationIdentifier);
				AssertEquals(AuthorityUrl, tokenAuthOnboardingDataResponse.AuthorityUrl);
				AssertEquals(ediTokenAuthOnBoardingData.TOD_ClaimMappingName, tokenAuthOnboardingDataResponse.ClaimMappingName);
				AssertEquals(ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier, tokenAuthOnboardingDataResponse.ClaimMappingIdentifier);

				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.CustomerTestCompleted;
				Factory.Save();

				logger.ClearLog();
				response = InvokeFetchOidcConfig(databaseNumber, token, logger, $"https://unit-testing/{FetchOidcConfigUrl}", authorityUrl);
				tokenAuthOnboardingDataResponse = JsonConvert.DeserializeObject<TokenAuthOnboardingDataResponse>(response.Content.ReadAsStringAsync().Result);
				AssertNotNull(tokenAuthOnboardingDataResponse);
				AssertEquals(ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier, tokenAuthOnboardingDataResponse.DomainHint);
				AssertEquals(identityServer.ClientIdentifier, tokenAuthOnboardingDataResponse.ConfigurationIdentifier);
				AssertEquals(AuthorityUrl, tokenAuthOnboardingDataResponse.AuthorityUrl);
				AssertEquals(ediTokenAuthOnBoardingData.TOD_ClaimMappingName, tokenAuthOnboardingDataResponse.ClaimMappingName);
				AssertEquals(ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier, tokenAuthOnboardingDataResponse.ClaimMappingIdentifier);
			}
		}
		#endregion

		public void TestUseCorrectAuthorityUrl()
		{
			var logger = new NLogWrapperForTest(GetType());
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://unit-testing/api/TokenAuthOnBoarding/fetch/oidcconfig/");
			using (var controller = new TokenAuthOnBoardingControllerForTest(logger))
			{
				var authorityUrl = controller.GetAuthorityUrlForTest();
				AssertContains("Registry 'WiseTech Global Client Extensions -> Azure Application Management -> Azure Application Management Tenant ID' is not overridden.", logger.ToString());
			}

			using (EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TenantId))
			using (var controller = new TokenAuthOnBoardingControllerForTest(logger))
			{
				var authorityUrl = controller.GetAuthorityUrlForTest();
				AssertEquals("https://login.microsoftonline.com/804a70cf-4a61-4a08-908e-afdf43432443/v2.0/", authorityUrl);
			}
		}

		#region TestEnableOidcConfig

		public void TestEnableOidcConfig_NoAccessToken()
		{
			var logger = new NLogWrapperForTest(GetType());
			var databaseNumber = LicenseDatabaseTest.LD_DatabaseNumber.ToString();
			var uri = $"https://unit-testing/{EnableOidcConfigUrl}";
			var response = InvokeFetchOidcConfig(databaseNumber, "", logger, uri);
			var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestEnableOidcConfig_InvalidToken()
		{
			var logger = new NLogWrapperForTest(GetType());
			var databaseNumber = LicenseDatabaseTest.LD_DatabaseNumber.ToString();
			var uri = $"https://unit-testing/{EnableOidcConfigUrl}";
			var response = InvokeEnableOidcConfig(databaseNumber, "Invalid Access Token", logger, uri);
			var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestEnableOidcConfig_InvalidLicensePk()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var logger = new NLogWrapperForTest(GetType());
				var databaseNumber = "123";
				var uri = $"https://unit-testing/{EnableOidcConfigUrl}";
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeEnableOidcConfig(databaseNumber, token, logger, uri, authorityUrl);
				var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: Database Number Is Invalid";
				AssertContains(message, logger.ToString());
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestEnableOidcConfig_NullEdiTokenAuthOnBoardingData()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var logger = new NLogWrapperForTest(GetType());
				var databaseNumber = LicenseDatabaseTest.LD_DatabaseNumber.ToString();
				var uri = $"https://unit-testing/{EnableOidcConfigUrl}";
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeEnableOidcConfig(databaseNumber, token, logger, uri, authorityUrl);
				var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: Database Number Is Invalid";
				AssertContains(message, logger.ToString());
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestEnableOidcConfig_BadStatusWithEdiTokenAuthOnBoardingDataStatus()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.CustomerTestCompleted;
				ediTokenAuthOnBoardingData.TOD_Enabled = false;
				ediTokenAuthOnBoardingData.TOD_LE = LicenseDatabaseProduction.LD_LE;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier = OIDCClaimMappingIdentifiers.Codes.LoginName;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingName = "user_name";
				ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "Azure";
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "AZU";
				ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
				ediTokenAuthOnBoardingData.TOD_Retry = 5;
				ediTokenAuthOnBoardingData.TOD_VerificationUsername = "user";
				ediTokenAuthOnBoardingData.TOD_VerificationUserPassword = "password";
				Factory.Save();

				EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection(identityServer.ClientIdentifier));

				var logger = new NLogWrapperForTest(GetType());
				var databaseNumber = LicenseDatabaseProduction.LD_DatabaseNumber.ToString();
				var uri = $"https://unit-testing/{EnableOidcConfigUrl}";
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeEnableOidcConfig(databaseNumber, token, logger, uri, authorityUrl);
				var message = $"TokenAuthOnBoardingControllerTest: {NLogWrapper.Status.BadRequest}: OIDC Settings Are Not Ready";
				AssertContains(message, logger.ToString());
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestEnableOidcConfig_SuccessWithTest()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Verified;
				ediTokenAuthOnBoardingData.TOD_LE = LicenseDatabaseTest.LD_LE;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier = OIDCClaimMappingIdentifiers.Codes.LoginName;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingName = "user_name";
				ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "Azure";
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "AZU";
				ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
				ediTokenAuthOnBoardingData.TOD_Retry = 5;
				ediTokenAuthOnBoardingData.TOD_VerificationUsername = "user";
				ediTokenAuthOnBoardingData.TOD_VerificationUserPassword = "password";
				Factory.Save();

				EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection(identityServer.ClientIdentifier));

				var logger = new NLogWrapperForTest(GetType());
				var databaseNumber = LicenseDatabaseTest.LD_DatabaseNumber.ToString();
				var uri = $"https://unit-testing/{EnableOidcConfigUrl}";
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeEnableOidcConfig(databaseNumber, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.CustomerTestCompleted;
				Factory.Save();

				logger.ClearLog();
				response = InvokeEnableOidcConfig(databaseNumber, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Completed;
				logger.ClearLog();
				Factory.Save();
				response = InvokeEnableOidcConfig(databaseNumber, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
			}
		}

		public void TestEnableOidcConfig_SuccessWithProduction()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.CustomerTestCompleted;
				ediTokenAuthOnBoardingData.TOD_Enabled = true;
				ediTokenAuthOnBoardingData.TOD_LE = LicenseDatabaseProduction.LD_LE;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier = OIDCClaimMappingIdentifiers.Codes.LoginName;
				ediTokenAuthOnBoardingData.TOD_ClaimMappingName = "user_name";
				ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "Azure";
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "AZU";
				ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
				ediTokenAuthOnBoardingData.TOD_Retry = 5;
				ediTokenAuthOnBoardingData.TOD_VerificationUsername = "user";
				ediTokenAuthOnBoardingData.TOD_VerificationUserPassword = "password";
				Factory.Save();

				EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection(identityServer.ClientIdentifier));

				var logger = new NLogWrapperForTest(GetType());
				var databaseNumber = LicenseDatabaseProduction.LD_DatabaseNumber.ToString();
				var uri = $"https://unit-testing/{EnableOidcConfigUrl}";
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeEnableOidcConfig(databaseNumber, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Completed;
				logger.ClearLog();
				Factory.Save();
				response = InvokeEnableOidcConfig(databaseNumber, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
			}
		}
		#endregion

		#region Execute

		HttpResponseMessage Execute(HttpRequestMessage request, NLogWrapper logger, string authorityUrl)
		{
			using (var controller = new TokenAuthOnBoardingControllerForTest(logger, authorityUrl))
			{
				return ControllerTestHelper.Execute(controller, request, typeof(TokenAuthOnBoardingController));
			}
		}

		HttpResponseMessage InvokeFetchOidcConfig(string databaseNumber, string accessToken, NLogWrapper logger, string requestUri, string authorityUrl = "")
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{requestUri}{databaseNumber}");
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			return Execute(requestMessage, logger, authorityUrl);
		}

		HttpResponseMessage InvokeEnableOidcConfig(string databaseNumber, string accessToken, NLogWrapper logger, string requestUri, string authorityUrl = "")
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{requestUri}{databaseNumber}");
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			return Execute(requestMessage, logger, authorityUrl);
		}

		#endregion Execute

		protected override void SetUp()
		{
			base.SetUp();
			var product = "CW1";
			var licenceEnterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "TET", "SYD");
			LicenseDatabaseTest = licence.Database;
			LicenseDatabaseTest.LD_DatabaseNumber = 8000;
			LicenseDatabaseTest.LD_Product = product;
			LicenseDatabaseTest.LD_Password = "DE-3E-CC-F6-D3-2B-3E-58-E3-E1-55-80-F5-77-67-95-73-D9-1D-CD-C7-52-37-9A-C8-1A-E9-24-1B-23-75-7D-E9-B2-93-AB-BE-0E-2B-58-A8-8D-8B-64-03-33-95-DB-72-C2-1A-9C-1B-3F-BB-1E-8F-FB-5D-BE-B0-71-74-12";
			LicenseDatabaseTest.LD_IsActive = true;
			LicenseDatabaseTest.LD_Status = "REG";
			LicenseDatabaseTest.LD_LE = licenceEnterprise1.PK;
			LicenseDatabaseTest.LD_LicenceType = DatabaseTypes.Codes.Test;
			Factory.Save();

			var licenceEnterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licence2 = BillingTestHelper.CreateLicence(Factory, "WTG", "PRD", "SYS");
			LicenseDatabaseProduction = licence2.Database;
			LicenseDatabaseProduction.LD_DatabaseNumber = 8001;
			LicenseDatabaseProduction.LD_Product = "CW1";
			LicenseDatabaseProduction.LD_Password = "DE-3E-CC-F6-D3-2B-3E-58-E3-E1-55-80-F5-77-67-95-73-D9-1D-CD-C7-52-37-9A-C8-1A-E9-24-1B-23-75-7D-E9-B2-93-AB-BE-0E-2B-58-A8-8D-8B-64-03-33-95-DB-72-C2-1A-9C-1B-3F-BB-1E-8F-FB-5D-BE-B0-71-74-12";
			LicenseDatabaseProduction.LD_IsActive = true;
			LicenseDatabaseProduction.LD_Status = "REG";
			LicenseDatabaseProduction.LD_LE = licenceEnterprise2.PK;
			LicenseDatabaseProduction.LD_LicenceType = DatabaseTypes.Codes.Production;
			Factory.Save();
		}

		LicenceDatabase LicenseDatabaseTest;

		LicenceDatabase LicenseDatabaseProduction;

		AzureOpenIDConnectConfigurationCollection InitializeCollection(string clientId)
		{
			var collection = new AzureOpenIDConnectConfigurationCollection();
			var azureApplicationManagement = collection.AddNew();
			azureApplicationManagement.Code = Code;
			azureApplicationManagement.AuthorityUrl = AuthorityUrl;
			azureApplicationManagement.ClientID = clientId;
			return collection;
		}

		const string Code = "PRD";

		const string AuthorityUrl = "https://www.example.com";

		const string TenantId = "804a70cf-4a61-4a08-908e-afdf43432443";

		const string FetchOidcConfigUrl = "api/TokenAuthOnBoarding/oidcconfig/fetch/";

		const string EnableOidcConfigUrl = "api/TokenAuthOnBoarding/oidcconfig/enable/";
	}

	class TokenAuthOnBoardingControllerForTest : TokenAuthOnBoardingController
	{
		public TokenAuthOnBoardingControllerForTest(NLogWrapper logger, string authorityUrl = null) : base(logger)
		{
			if (authorityUrl != null)
			{
				SetAuthorityUrl(authorityUrl);
			}
		}

		void SetAuthorityUrl(string authorityUrl)
		{
			TrustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);
		}

		public string GetAuthorityUrlForTest()
		{
			return TrustHelper.GetAuthorityUrl(Logger);
		}
	}
}
