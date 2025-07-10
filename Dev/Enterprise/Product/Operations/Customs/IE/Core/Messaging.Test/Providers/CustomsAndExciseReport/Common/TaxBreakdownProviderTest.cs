using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class TaxBreakdownProviderTest : TestCaseWithFactory
	{
		TaxBreakdownProvider provider;

		public void TestTaxType43()
		{
			provider = new TaxBreakdownProvider(new TaxBreakdown { TaxType43 = "A00" });
			AssertEquals("A00", provider.TaxType);
		}
		public void TestTaxType_XlsxField() => typeof(TaxBreakdownProvider).TestXlsxField(nameof(TaxBreakdownProvider.TaxType), 1, "Tax Type");

		public void TestPayableAmount46()
		{
			provider = new TaxBreakdownProvider(new TaxBreakdown { PayableAmount46 = 150.0M });
			AssertEquals(150.0M, provider.PayableAmount);
			AssertEquals("150.00", provider.PayableAmount.ToString("#,0.00"));
		}
		public void TestPayableAmount_XlsxField() => typeof(TaxBreakdownProvider).TestXlsxField(nameof(TaxBreakdownProvider.PayableAmount), 2, "Payable Amount");
	}
}
