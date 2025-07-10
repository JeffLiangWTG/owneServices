using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(InvoiceLineChargeValidation))]
	class InvoiceLineChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_RX_NKCurrency()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			var targetInfo = invoiceLineCharge.J7_RX_NKCurrencyInfo;
			invoiceLineCharge.J7_RX_NKCurrency = "JPY";
			AssertHasMessageError(targetInfo, "The charge currency is different to the invoice amount currency.");

			invoiceLineCharge.J7_RX_NKCurrency = "USD";
			AssertNoMessageError(targetInfo, "The charge currency is different to the invoice amount currency.");
		}
	}
}
