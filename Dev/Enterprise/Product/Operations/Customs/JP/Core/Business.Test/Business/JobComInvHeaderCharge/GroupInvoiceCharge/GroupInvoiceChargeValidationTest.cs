using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(GroupInvoiceChargeValidation))]
	class GroupInvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_RX_NKCurrency()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			
			var topGroupInvoice = declaration.TopGroupInvoice;
			topGroupInvoice.JZ_RX_NKInvoice_Currency = "USD";
			var groupInvoiceCharge  = topGroupInvoice.Charges.AddNew();
			var targetInfo = groupInvoiceCharge.J7_RX_NKCurrencyInfo;
			groupInvoiceCharge.J7_RX_NKCurrency = "JPY";
			AssertHasMessageError(targetInfo, "The charge currency is different to the invoice amount currency.");

			groupInvoiceCharge.J7_RX_NKCurrency = "USD";
			AssertNoMessageError(targetInfo, "The charge currency is different to the invoice amount currency.");
		}
	}
}
