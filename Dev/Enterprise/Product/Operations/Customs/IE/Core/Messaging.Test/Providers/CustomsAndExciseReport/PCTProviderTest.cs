using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class PCTProviderTest : TestCaseWithFactory
	{
		PCTProvider provider;

		public void TestTimestamp()
		{
			provider = new PCTProvider(new PCTMessage() { Timestamp = "1660211772103" });
			AssertEquals("1660211772103", provider.Timestamp);
		}

		public void TestEori()
		{
			provider = new PCTProvider(new PCTMessage() { Eori = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Eori);
		}
		public void TestEori_XlsxField() => typeof(PCTProvider).TestXlsxField(nameof(PCTProvider.Eori), 1, "EORI");

		public void TestPeriod()
		{
			provider = new PCTProvider(new PCTMessage() { Period = "20220801" });
			AssertEquals("01-Aug-22", provider.Period.ToShortDateString());
		}
		public void TestPeriod_XlsxField() => typeof(PCTProvider).TestXlsxField(nameof(PCTProvider.Period), 3, "Period");

		public void TestPctMessageObject()
		{
			provider = new PCTProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePCTMessage());
			AssertEquals(2, provider.PaidOrders.Count);
		}
		public void TestPaidOrders_XlsxField() => typeof(PCTProvider).TestXlsxField(nameof(PCTProvider.PaidOrders), 4, "Paid Orders");
	}
}
