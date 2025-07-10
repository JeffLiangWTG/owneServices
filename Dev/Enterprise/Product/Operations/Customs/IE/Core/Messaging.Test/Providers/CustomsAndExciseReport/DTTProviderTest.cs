using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class DTTProviderTest : TestCaseWithFactory
	{
		DTTProvider provider;

		public void TestTimestamp()
		{
			provider = new DTTProvider(new DTTMessage { Timestamp = "1660211772103" });
			AssertEquals("1660211772103", provider.Timestamp);
		}

		public void TestEori()
		{
			provider = new DTTProvider(new DTTMessage() { Eori = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Eori);
		}
		public void TestEori_XlsxField() => typeof(DTTProvider).TestXlsxField(nameof(DTTProvider.Eori), 1, "EORI");

		public void TestDay()
		{
			provider = new DTTProvider(new DTTMessage() { Day = "20220801" });
			AssertEquals("01-Aug-22", provider.Day.ToShortDateString());
		}
		public void TestDay_XlsxField() => typeof(DTTProvider).TestXlsxField(nameof(DTTProvider.Day), 3, "Day");

		public void TestDttMessageObject()
		{
			provider = new DTTProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreateDTTMessage());
			AssertEquals(2, provider.TaxDetails.Count);
		}
		public void TestTaxDetails_XlsxField() => typeof(DTTProvider).TestXlsxField(nameof(DTTProvider.TaxDetails), 4, "Tax Details");
	}
}
