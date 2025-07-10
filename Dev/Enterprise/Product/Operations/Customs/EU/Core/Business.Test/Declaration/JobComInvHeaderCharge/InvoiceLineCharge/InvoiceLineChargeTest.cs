using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	public class InvoiceLineChargeTest : Customs.Business.Testing.BaseInvoiceLineChargeTest
	{
		public void TestDefaultCurrency()
		{
			invoice.JZ_RX_NKInvoice_Currency = "EUR";

			var charge1 = invoiceLine.Charges.AddNew();
			charge1.J7_Amount = 1m;
			AssertEquals("EUR", charge1.J7_RX_NKCurrency);

			charge1.J7_RX_NKCurrency = "";
			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_Amount = 1m;
			AssertEquals("EUR", charge2.J7_RX_NKCurrency);

			charge2.J7_RX_NKCurrency = "CNY";
			var charge3 = invoiceLine.Charges.AddNew();
			charge3.J7_Amount = 1m;
			AssertEquals("CNY", charge3.J7_RX_NKCurrency);
		}

		public void TestIsIncludedInInvoice_ForChargeStatisticalValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = ChargeTypeList.Codes.StatisticalValue;
			AssertEquals(declaration.IncoTermAndChargeFactory.GetCharge(ChargeTypeList.Codes.StatisticalValue) == null, charge.J7_Calc_IsIncludedInInvoiceAmount);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceLineCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestValidation()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertType<InvoiceLineChargeValidation>(parent.Validation);
		}

		public void TestLookups()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertType<InvoiceLineChargeLookups>(parent.Lookups);
		}
	}
}
