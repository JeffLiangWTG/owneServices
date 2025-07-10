using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class InvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_RX_NKCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew();
			ChargeValidationHelperTest.TestCheckJ7_ExchangeRate(invoiceCharge);
		}
	}
}
