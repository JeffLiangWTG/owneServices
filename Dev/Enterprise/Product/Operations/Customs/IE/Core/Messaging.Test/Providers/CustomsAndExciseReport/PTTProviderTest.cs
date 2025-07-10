using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class PTTProviderTest : TestCaseWithFactory
	{
		PTTProvider provider;

		public void TestTimestamp()
		{
			provider = new PTTProvider(new PTTMessage { Timestamp = "1660211772103" });
			AssertEquals("1660211772103", provider.Timestamp);
		}

		public void TestEori()
		{
			provider = new PTTProvider(new PTTMessage { Eori = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Eori);
		}
		public void TestEori_XlsxField() => typeof(PTTProvider).TestXlsxField(nameof(PTTProvider.Eori), 1, "EORI");

		public void TestPeriod()
		{
			provider = new PTTProvider(new PTTMessage { Period = "20220801" });
			AssertEquals("01-Aug-22", provider.Period.ToShortDateString());
		}
		public void TestPeriod_XlsxField() => typeof(PTTProvider).TestXlsxField(nameof(PTTProvider.Period), 3, "Period");

		public void TestPttMessageObject()
		{
			provider = new PTTProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePTTMessage());
			AssertEquals(2, provider.TaxDetails.Count);
		}
		public void TestTaxDetails_XlsxField() => typeof(PTTProvider).TestXlsxField(nameof(PTTProvider.TaxDetails), 4, "Tax Details");
	}
}
