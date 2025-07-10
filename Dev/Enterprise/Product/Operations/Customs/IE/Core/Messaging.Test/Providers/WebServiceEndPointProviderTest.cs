using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	class WebServiceEndPointProviderTest : TestCaseWithFactory
	{
		public void TestGetMailboxAcknowledgeURL() => AssertGetURL("IEMAILACK", WebServiceEndPointProvider.GetMailboxAcknowledgeURL);
		public void TestGetEMCSMailboxAcknowledgeURL() => AssertGetURL("IEMAILAKE", WebServiceEndPointProvider.GetEMCSMailboxAcknowledgeURL);
		public void TestGetMailboxCollectURL() => AssertGetURL("IEMAILCOL", WebServiceEndPointProvider.GetMailboxCollectURL);
		public void TestGetEMCSMailboxCollectURL() => AssertGetURL("IEMAILCLE", WebServiceEndPointProvider.GetEMCSMailboxCollectURL);
		public void TestGetTransactionIDURL() => AssertGetURL("IETRANSID", (factory) => WebServiceEndPointProvider.GetTransactionIDURL(factory, EDIInterchange.ApplicationCodes.IECustomsCommon));
		public void TestGetTransactionIDURL_EMCS() => AssertGetURL("IETRANIDE", (factory) => WebServiceEndPointProvider.GetTransactionIDURL(factory, EDIInterchange.ApplicationCodes.IECustomsEMCS));
		public void TestGetSubmissionURL() => AssertGetURL("IEISUBM", GetSubmissionURL);
		public void TestGetEMCSSubmissionURL() => AssertGetURL("IESUBM815", GetEMCSSubmissionURL);
		public void TestGetNCTSSubmissionURL() => AssertGetURL("IEIENSUBM", GetNCTSSubmissionURL);
		public void TestGetCustomsAndExciseReportURL() => AssertGetURL("IEIERSUBM", GetCustomsAndExciseReportURL);
		public void TestGetPBNURL() => AssertGetURL("IESUBPCPB", (factory) => WebServiceEndPointProvider.GetSubmissionURL(factory, EDIInterchange.ApplicationCodes.IECustomsPBN, "CPB"));

		void AssertGetURL(string configCode, Func<BusinessObjectFactory, string> getURL)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefSysConfigType(configCode + "P", "PRODUCTION DESCRIPTION", "PRODUCTION LONG DESCRIPTION");
			helper.CreateRefSysConfigType(configCode + "T", "TEST DESCRIPTION", "TEST LONG DESCRIPTION");
			helper.CreateRefSysConfig(configCode + "P", "https://www.production.ie/webservice/v1/soap/url", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			helper.CreateRefSysConfig(configCode + "T", "https://www.test.ie/webservice/v1/soap/url", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			Factory.Save();
			var registration = ObjectFactory.Get<IProductRegistration>();
			registration.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			var factory = new BusinessObjectFactory();
			AssertEquals($"TEST - {configCode}", "https://www.test.ie/webservice/v1/soap/url", getURL(factory));
			var hitCount = factory.GetTableHitCount(RefSysConfig.Schema.TableName);
			AssertEquals($"TEST - {configCode}", "https://www.test.ie/webservice/v1/soap/url", getURL(factory));
			AssertEquals("Should not cause more DB hits fir TEST", hitCount, factory.GetTableHitCount(RefSysConfig.Schema.TableName));

			registration.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			factory = new BusinessObjectFactory();
			AssertEquals($"PRODUCTION - {configCode}", "https://www.production.ie/webservice/v1/soap/url", getURL(factory));
			hitCount = factory.GetTableHitCount(RefSysConfig.Schema.TableName);
			AssertEquals($"PRODUCTION - {configCode}", "https://www.production.ie/webservice/v1/soap/url", getURL(factory));
			AssertEquals("Should not cause more DB hits for PRODUCTION", hitCount, factory.GetTableHitCount(RefSysConfig.Schema.TableName));
		}

		string GetSubmissionURL(BusinessObjectFactory factory)
		{
			return WebServiceEndPointProvider.GetSubmissionURL(Factory, "I", "");
		}

		string GetEMCSSubmissionURL(BusinessObjectFactory factory)
		{
			return WebServiceEndPointProvider.GetSubmissionURL(Factory, EDIMessage.ApplicationCodes.IECustomsEMCS, "815");
		}

		string GetNCTSSubmissionURL(BusinessObjectFactory factory)
		{
			return WebServiceEndPointProvider.GetSubmissionURL(Factory, EDIMessage.ApplicationCodes.IECustomsNCTS, "");
		}

		string GetCustomsAndExciseReportURL(BusinessObjectFactory factory)
		{
			return WebServiceEndPointProvider.GetCustomsAndExciseReportURL(Factory);
		}
	}

	public class WebServiceEndPointProviderTestHelper : UniversalReferenceTestDataHelper
	{
		public WebServiceEndPointProviderTestHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public string SetupMailboxAcknowledgeURL() => SetupURL(WebServiceEndPointProvider.Constants.MailboxAcknowledge + WebServiceEndPointProvider.GetSuffix(false));
		public string SetupMailboxCollectURL() => SetupURL(WebServiceEndPointProvider.Constants.MailboxCollect + WebServiceEndPointProvider.GetSuffix(false));
		public string SetupEMCSMailboxCollectURL() => SetupURL(WebServiceEndPointProvider.EMCSConstants.MailboxCollect + WebServiceEndPointProvider.GetSuffix(false));
		public string SetupTransactionIDURL(bool setupEMCS = false) => SetupURL((setupEMCS ? WebServiceEndPointProvider.EMCSConstants.TransactionID : WebServiceEndPointProvider.Constants.TransactionID) + WebServiceEndPointProvider.GetSuffix(false));

		string SetupURL(string configCode)
		{
			CreateRefSysConfigType(configCode, $"{configCode} desc", $"{configCode} long desc");
			var result = $@"https://www.test.com/{configCode}";
			CreateRefSysConfig(configCode, result, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			return result;
		}
	}
}
