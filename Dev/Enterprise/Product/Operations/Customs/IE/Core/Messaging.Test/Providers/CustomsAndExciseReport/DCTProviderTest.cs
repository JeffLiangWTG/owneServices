using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class DCTProviderTest : TestCaseWithFactory
	{
		DCTProvider provider;

		public void TestTimestamp()
		{
			provider = new DCTProvider(new DCTMessage() { Timestamp = "1660211772103" });
			AssertEquals("1660211772103", provider.Timestamp);
		}

		public void TestEori()
		{
			provider = new DCTProvider(new DCTMessage() { Eori = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Eori);
		}
		public void TestEori_XlsxField() => typeof(DCTProvider).TestXlsxField(nameof(DCTProvider.Eori), 1, "EORI");

		public void TestDay()
		{
			provider = new DCTProvider(new DCTMessage() { Day = "20220801" });
			AssertEquals("01-Aug-22", provider.Day.ToShortDateString());
		}
		public void TestDay_XlsxField() => typeof(DCTProvider).TestXlsxField(nameof(DCTProvider.Day), 3, "Day");

		public void TestDctMessageObject()
		{
			provider = new DCTProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreateDCTMessage());
			AssertEquals(2, provider.PaidOrders.Count);
		}
		public void TestPaidOrders_XlsxField() => typeof(DCTProvider).TestXlsxField(nameof(DCTProvider.PaidOrders), 4, "Paid Orders");
	}
}
