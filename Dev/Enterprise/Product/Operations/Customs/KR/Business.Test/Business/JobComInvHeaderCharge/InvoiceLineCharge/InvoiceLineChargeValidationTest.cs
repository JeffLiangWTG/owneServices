using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class InvoiceLineChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_RX_NKCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var lineCharge = invoiceLine.Charges.AddNew();
			ChargeValidationHelperTest.TestCheckJ7_ExchangeRate(lineCharge);
		}
	}
}
