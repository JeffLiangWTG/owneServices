using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(InvoiceChargeValidation))]
	sealed class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
	{
		public void TestCheckJ7_Amount()
		{
			var invoiceCharge = Factory.NewWithValidTestData<InvoiceCharge>();
			var expectedMessageError = "Amount must be whole number when Currency is JPY.";

			invoiceCharge.J7_RX_NKCurrency = "JPY";
			invoiceCharge.J7_Amount = 12.34;
			AssertHasMessageError(invoiceCharge.J7_AmountInfo, expectedMessageError);

			invoiceCharge.J7_Amount = 12.00;
			AssertNoMessageError(invoiceCharge.J7_AmountInfo, expectedMessageError);

			invoiceCharge.J7_RX_NKCurrency = "CAN";
			invoiceCharge.J7_Amount = 12.34;
			AssertNoMessageError(invoiceCharge.J7_AmountInfo, expectedMessageError);
		}

		public void TestCheckJ7_RX_NKCurrency()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var charge = invoice.Charges.AddNew();
			var targetInfo = charge.J7_RX_NKCurrencyInfo;
			charge.J7_RX_NKCurrency = "JPY";
			AssertHasMessageError(targetInfo, "The charge currency is different to the invoice amount currency.");

			charge.J7_RX_NKCurrency = "USD";
			AssertNoMessageError(targetInfo, "The charge currency is different to the invoice amount currency.");
		}
	}
}
