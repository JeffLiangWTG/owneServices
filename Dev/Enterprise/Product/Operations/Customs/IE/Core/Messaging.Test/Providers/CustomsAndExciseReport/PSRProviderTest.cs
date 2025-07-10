using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class PSRProviderTest : TestCaseWithFactory
	{
		PSRProvider provider;

		public void TestTimestamp()
		{
			provider = new PSRProvider(new PSRMessage { Timestamp = "1660211772103" });
			AssertEquals("1660211772103", provider.Timestamp);
		}

		public void TestEori()
		{
			provider = new PSRProvider(new PSRMessage { Eori = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Eori);
		}
		public void TestEori_XlsxField() => typeof(PSRProvider).TestXlsxField(nameof(PSRProvider.Eori), 1, "EORI");

		public void TestPeriod()
		{
			provider = new PSRProvider(new PSRMessage { Period = "20220801" });
			AssertEquals(new ZDateTime(2022, 8, 1), provider.Period);
		}
		public void TestPeriod_XlsxField() => typeof(PSRProvider).TestXlsxField(nameof(PSRProvider.Period), 2, "Period");

		public void TestTaxTotal()
		{
			provider = new PSRProvider(new PSRMessage { TaxTotal = 400.0M });
			AssertEquals(400.0M, provider.TaxTotal);
		}
		public void TestTaxTotal_XlsxField() => typeof(PSRProvider).TestXlsxField(nameof(PSRProvider.TaxTotal), 3, "Tax Total");

		public void TestPsrMessageObject()
		{
			provider = new PSRProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePSRMessage());
			AssertEquals(2, provider.TaxBreakdowns.Count);
			AssertEquals(2, provider.DailyBreakdowns.Count);
		}
		public void TestTaxBreakdowns_XlsxField() => typeof(PSRProvider).TestXlsxField(nameof(PSRProvider.TaxBreakdowns), 4, "Tax Breakdowns");
		public void TestDailyBreakdowns_XlsxField() => typeof(PSRProvider).TestXlsxField(nameof(PSRProvider.DailyBreakdowns), 5, "Daily Breakdowns");
	}
}
