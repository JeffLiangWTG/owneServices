using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class PciPaidOrderProviderTest : TestCaseWithFactory
	{
		PciPaidOrderProvider provider;

		public void TestPayerName()
		{
			provider = new PciPaidOrderProvider(new PciPaidOrder { PayerName = "MR Test ONeill" });
			AssertEquals("MR Test ONeill", provider.PayerName);
		}
		public void TestIPayerName_XlsxField() => typeof(PciPaidOrderProvider).TestXlsxField(nameof(PciPaidOrderProvider.PayerName), -1, "Payer Name");
	}
}
