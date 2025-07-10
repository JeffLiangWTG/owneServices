using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	sealed class InvoiceLineChargeTest : Customs.Business.Testing.BaseInvoiceLineChargeTest
	{
		public void TestJ7_AmountDecimalPlaces()
		{
			var invoiceLineCharge = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().Charges.AddNew();
			var info = invoiceLineCharge.J7_AmountInfo;
			invoiceLineCharge.J7_RX_NKCurrency = "TWD";
			AssertHasDecimalPlacesAttribute(info, 2);

			invoiceLineCharge.J7_RX_NKCurrency = "JPY";
			AssertHasDecimalPlacesAttribute(info, 0);
		}
	}
}
