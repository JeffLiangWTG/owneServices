using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	class InvoiceLineChargeTest : Customs.Business.Testing.BaseInvoiceLineChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseInvoiceLineCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestIsJ7_IsDutiable_ReadOnly()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			Assert(!invoiceLineCharge.J7_IsDutiableInfo.ReadOnly);
			Assert(!invoiceLineCharge.J7_IsGSTApplicableInfo.ReadOnly);
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			Assert(invoiceLineCharge.J7_IsDutiableInfo.ReadOnly);
			Assert(invoiceLineCharge.J7_IsGSTApplicableInfo.ReadOnly);
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			Assert(invoiceLineCharge.J7_IsDutiableInfo.ReadOnly);
			Assert(invoiceLineCharge.J7_IsGSTApplicableInfo.ReadOnly);
		}

		public void TestAllowNonWesternEuropeanCharacterForChargeDescription()
		{
			var charge = Factory.New<InvoiceLineCharge>();
			Assert("AllowNonWesternEuropeanCharacterForChargeDescription should be true", charge.AllowNonWesternEuropeanCharacterForChargeDescription);
		}
	}
}
