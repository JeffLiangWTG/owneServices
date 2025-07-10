using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	sealed class InvoiceLineChargeTest : Customs.Business.Testing.BaseInvoiceLineChargeTest
	{
		public override void TestSettingAmountWillUpdateCurrencyIfNotEntered()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CA_RX_DeclaredCurr = ZGuid.Empty;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			InvoiceLineCharge charge = Factory.New<InvoiceLineCharge>();
			charge.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			charge.J7_Amount = 1.2m;
			AssertEquals("J7_RX_NKCurrency", JobDeclaration.LocalCurrencyConstantCode, charge.J7_RX_NKCurrency);

			charge.Parent = invoiceLine;
			charge.J7_RX_NKCurrency = ZString.Empty;
			charge.J7_Amount = 1.3m;
			AssertEquals("J7_RX_NKCurrency", ZString.Empty, charge.J7_RX_NKCurrency);
			charge.J7_Amount = 1.2m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			charge.J7_RX_NKCurrency = ZString.Empty;
			charge.J7_Amount = 1.3m;
			AssertEquals("J7_RX_NKCurrency", JobDeclaration.LocalCurrencyConstantCode, charge.J7_RX_NKCurrency);

			charge.J7_Amount = 1.2m;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Belarus;
			charge.J7_Amount = 1.3m;
			AssertEquals("J7_RX_NKCurrency", Core.Constants.CurrencyCodes.Belarus, charge.J7_RX_NKCurrency);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceLineCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
