using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class UDRProviderTest : TestCaseWithFactory
	{
		UDRProvider provider;

		public void TestTimestamp()
		{
			provider = new UDRProvider(new UDRMessage { Timestamp = "1660211772103" });
			AssertEquals("1660211772103", provider.Timestamp);
		}

		public void TestEori()
		{
			provider = new UDRProvider(new UDRMessage { Eori = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Eori);
		}
		public void TestEori_XlsxField() => typeof(UDRProvider).TestXlsxField(nameof(UDRProvider.Eori), 1, "EORI");

		public void TestUdrMessageObject()
		{
			provider = new UDRProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreateUDRMessage());
			AssertEquals(3, provider.UnpaidOrders.Count);
		}
		public void TestUnpaidOrders_XlsxField() => typeof(UDRProvider).TestXlsxField(nameof(UDRProvider.UnpaidOrders), 3, "Unpaid Orders");
	}
}
