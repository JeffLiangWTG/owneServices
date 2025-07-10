using System;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Licensing.Billing.Business;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class FeatureControlControllerTest : TestCaseWithFactory, IDisposable
	{
		[TestDate(2024, 1, 1)]
		public void TestGetFeatureControlRule()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupControlRule();
			var request = BuildRequest("CW1", "1024", DateTime.MinValue);
			var response = CallGetFeatureControlRule(request, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, LicenceDatabase.GetOrCreateTrustedSystem());
			var rsp = JsonConvert.DeserializeObject<FeatureControlResponse>(content);
			AssertNotNull(rsp);
			AssertEquals(new DateTime(2024, 1, 1), rsp.RuleTimestampUtc);
			AssertEquals(DateTimeKind.Utc, rsp.RuleTimestampUtc.Kind);
			var rspXml = FeatureControlBaseControllerTest.DecompressString(rsp.RuleContent);
			AssertContains(@"<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>2024-01-01T00:00:00Z</TimestampUtc>
  <Rules>
    <Rule>
      <FCM_FeatureControlCode>CR5RESWIZ</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2001-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2002-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;global - 1 - CR5RESWIZ&lt;/a&gt;</FCR_Parameters>
    </Rule>
  </Rules>
</FeatureControl>", rspXml);

			var featureControlStorageProvider = new WritableFeatureControlStorageProvider();
			var featureControlRepository = new FeatureControlRuleRepository(featureControlStorageProvider, null);
			featureControlRepository.SaveFeatureControlRuleContent(rsp.RuleContent);

			var timeProvider = new FeatureControlTestHelper.FeatureControlManagerDateTimeProvider();
			var featureControlManager = new FeatureControlManager(featureControlStorageProvider, null, timeProvider);
			timeProvider.CurrentUtcDateTimeOverride = new DateTime(2001, 1, 1);
			var featureData = featureControlManager.GetFeatureData("CR5RESWIZ");
			AssertNotNull("Should load rule since it's the start date", featureData);
			AssertEquals("CR5RESWIZ", featureData.Code);
			AssertEquals("<a>global - 1 - CR5RESWIZ</a>", featureData.Parameter);

			timeProvider.CurrentUtcDateTimeOverride = new DateTime(2002, 1, 2);
			featureData = featureControlManager.GetFeatureData("CR5RESWIZ");
			AssertNull("Should not load rule since it's past the end date", featureData);

			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		[TestDate(2024, 1, 1)]
		public void TestGetFeatureControlRuleBadRequest()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupControlRule();
			var request = BuildRequest("CW1", "1241905", DateTime.MinValue);
			var response = CallGetFeatureControlRule(request, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(ErrorCodes.Descriptions.Validation_InvalidSystem, data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message failed to decrypt", "2001 The specified system is not found." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		TrustedRequestForTest BuildRequest(string product, string systemId, DateTime clientRuleTimestamp)
		{
			var userInfo = new FeatureControlRequest()
			{ Product = product, SystemId = systemId, RuleTimestampUtc = clientRuleTimestamp, InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime() };
			var json = JsonConvert.SerializeObject(userInfo);
			var certProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(certProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var trustedRequest = new TrustedRequest()
			{ EncryptedContent = trustedMessage.EncryptedContent, Product = product, SystemId = systemId };
			return new TrustedRequestForTest()
			{ Request = trustedRequest, Signature = trustedMessage.Signature, IV = trustedMessage.IV };
		}

		HttpResponseMessage Execute(HttpRequestMessage request, NLogWrapper logger)
		{
			using (ObjectFactory.Substitute<WTG.TrustedMessaging.ICertificatesProvider>(() => new CertificatesProviderForTest()))
			using (var controller = new FeatureControlV1Controller(logger))
			{
				return ControllerTestHelper.Execute(controller, request);
			}
		}

		HttpResponseMessage CallGetFeatureControlRule(TrustedRequestForTest request, NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/FeatureControl/Rule");
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request.Request), Encoding.UTF8, "application/json");
			requestMessage.Headers.Add("SIGNED", request.Signature);
			requestMessage.Headers.Add("WTG_I", request.IV);
			return Execute(requestMessage, logger);
		}

		void SetupControlRule()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "some org";
			org.OH_Code = "code";
			Factory.Save();
			TrustedSystem = Factory.New<EdiTrustedSystem>();
			TrustedSystem.ETS_Product = "CW1";
			TrustedSystem.ETS_SystemID = "1024";
			LicenceEnterprise = Factory.New<LicenceEnterprise>();
			LicenceEnterprise.LE_OH = org.PK;
			LicenceEnterprise.LE_EnterpriseCode = "BLA";
			LicenceDatabase = Factory.New<LicenceDatabase>();
			LicenceDatabase.LD_LE = LicenceEnterprise.PK;
			LicenceDatabase.LD_DatabaseNumber = 1024;
			LicenceDatabase.LD_ServerCode = "123";
			LicenceDatabase.LD_Product = "CW1";
			LicenceDatabase.LD_TenantID = "";
			LicenceDatabase.LD_OH_WebAccessOrg = org.PK;
			LicenceDatabase.LD_ETS_TrustedSystem = TrustedSystem.PK;
			securityKeyTestHelper.SetSecretKey(LicenceDatabase);

			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			header.FCM_FeatureControlCode = "CR5RESWIZ";
			var rule1 = header.FeatureControlRules.AddNew();
			rule1.IsGlobalRule = true;
			rule1.FCR_Description = "rule global - 1 - CR5RESWIZ";
			rule1.FCR_StartDateUtc = new ZDateTime(2001, 1, 1);
			rule1.FCR_EndDateUtc = new ZDateTime(2002, 1, 1);
			rule1.FCR_Parameters = "<a>global - 1 - CR5RESWIZ</a>";

			Factory.Save();
		}

		LicenceEnterprise LicenceEnterprise;
		LicenceDatabase LicenceDatabase;
		EdiTrustedSystem TrustedSystem;

		protected override void SetUp()
		{
			base.SetUp();
			controller = new FeatureControlV1Controller();
			securityKeyTestHelper = new SecurityKeyTestHelper();
		}

		FeatureControlV1Controller controller;
		SecurityKeyTestHelper securityKeyTestHelper;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				controller.Dispose();
			}
		}
	}
}
