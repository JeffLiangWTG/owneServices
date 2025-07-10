using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class DSRProviderTest : TestCaseWithFactory
	{
		DSRProvider provider;

		public void TestTimestamp()
		{
			provider = new DSRProvider(new DSRMessage { Timestamp = "1660211772103" });
			AssertEquals("1660211772103", provider.Timestamp);
		}

		public void TestEori()
		{
			provider = new DSRProvider(new DSRMessage { Eori = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Eori);
		}
		public void TestEori_XlsxField() => typeof(DSRProvider).TestXlsxField(nameof(DSRProvider.Eori), 1, "EORI");

		public void TestDate()
		{
			provider = new DSRProvider(new DSRMessage { Date = "20220801" });
			AssertEquals("01-Aug-22", provider.Date.ToShortDateString());
		}
		public void TestDate_XlsxField() => typeof(DSRProvider).TestXlsxField(nameof(DSRProvider.Date), 2, "Date");

		public void TestTaxTotal()
		{
			provider = new DSRProvider(new DSRMessage { TaxTotal = 400.0M });
			AssertEquals(400.0M, provider.TaxTotal);
		}
		public void TestTaxTotalXlsxField() => typeof(DSRProvider).TestXlsxField(nameof(DSRProvider.TaxTotal), 4, "Tax Total");

		public void TestDsrMessageObject()
		{
			provider = new DSRProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreateDSRMessage());
			AssertEquals(2, provider.TaxBreakdowns.Count);
		}
		public void TestTaxBreakdowns_XlsxField() => typeof(DSRProvider).TestXlsxField(nameof(DSRProvider.TaxBreakdowns), 5, "Tax Breakdowns");
	}
}
