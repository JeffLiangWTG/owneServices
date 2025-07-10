using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class PCIProviderTest : TestCaseWithFactory
	{
		PCIProvider provider;

		public void TestTimestamp()
		{
			provider = new PCIProvider(new PCIMessage { Timestamp = "1660211772103" });
			AssertEquals("1660211772103", provider.Timestamp);
		}

		public void TestEori()
		{
			provider = new PCIProvider(new PCIMessage { Eori = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Eori);
		}
		public void TestEori_XlsxField() => typeof(PCIProvider).TestXlsxField(nameof(PCIProvider.Eori), 1, "EORI");

		public void TestPeriod()
		{
			provider = new PCIProvider(new PCIMessage { Period = "20220801" });
			AssertEquals("01-Aug-22", provider.Period.ToShortDateString());
		}
		public void TestPeriod_XlsxField() => typeof(PCIProvider).TestXlsxField(nameof(PCIProvider.Period), 3, "Period");

		public void TestPciMessageObject()
		{
			provider = new PCIProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePCIMessage());
			AssertEquals(2, provider.PaidOrders.Count);
		}
		public void TestPaidOrders_XlsxField() => typeof(PCIProvider).TestXlsxField(nameof(PCIProvider.PaidOrders), 4, "Paid Orders");
	}
}
