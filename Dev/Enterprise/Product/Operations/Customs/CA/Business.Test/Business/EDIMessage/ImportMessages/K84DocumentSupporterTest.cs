using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(K84DocumentSupporter))]
	sealed class K84DocumentSupporterTest : DocumentSupporterTest
	{
		public void TestShowReasonForNotPrinting()
		{
			var message1 = Factory.New<K84Message>();
			AssertEquals(false, ((IDocumentSupportable)message1).DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		public void TestK84DocumentSupporter()
		{
			var message1 = Factory.New<K84Message>();
			var dataContextValue = new DataContextValue(".K84Report");
			Assert(((IDocumentSupportable)message1).DocumentSupporter.IsDataContextSupported(dataContextValue));

			message1.EM_MessageText = "UNH+1+CUSDEC:S:99B:UN'BGM+++9'DTM+137:20110408:102'RFF+ABP:10207'UNS+D'UNS+S'UNT+7+1'";
			message1.EM_MessageSubType = K84ReportTypes.Codes.Daily;
			var providers = ((IDocumentSupportable)message1).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(K84DailyReportDocumentWrapper), providers[0].ParentBusinessObject.GetType());
			message1.EM_MessageSubType = K84ReportTypes.Codes.Monthly;
			providers = ((IDocumentSupportable)message1).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(K84MonthlyReportDocumentWrapper), providers[0].ParentBusinessObject.GetType());
			message1.EM_MessageSubType = "XXX";
			providers = ((IDocumentSupportable)message1).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertNull(providers);

			var message2 = Factory.New<K84Message>();
			message2.EM_MessageText = "UNH+1+CUSRES:S:99B:UN'BGM+++9'DTM+137:20110415:102'RFF+ABP:10207'RFF+AEA:0495'UNS+D'UNS+S'UNT+8+1'";
			message2.EM_MessageSubType = K84ReportTypes.Codes.Overdue;
			providers = ((IDocumentSupportable)message2).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(OverdueReleaseNoticeDocumentWrapper), providers[0].ParentBusinessObject.GetType());

			var message3 = Factory.New<K84Message>();
			message3.EM_MessageText = "UNH+1+CUSDEC:S:99B:UN'BGM+++9'DTM+137:20160608:102'RFF+ABP:12310'RFF+AEA:0813'RFF+ARA:836263228RM0001'RFF+TN:001209310:N'RFF+AFB:2130PARS55218127'RFF+AEJ:007:Y'DTM+204:20160527:102'UNS+D'UNS+S'UNT+13+1'";
			message3.EM_MessageSubType = K84ReportTypes.Codes.Overdue;
			providers = ((IDocumentSupportable)message3).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(OverdueReleaseNoticeDocumentWrapper), providers[0].ParentBusinessObject.GetType());
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var result = Factory.New<K84Message>();
			result.EM_MessageSubType = K84ReportTypes.Codes.Daily;
			result.EM_MessageText = "UNH+1+CUSDEC:S:99B:UN'BGM+++9'DTM+137:20110408:102'RFF+ABP:10207'UNS+D'UNS+S'UNT+7+1'";
			return result;
		}
	}
}
