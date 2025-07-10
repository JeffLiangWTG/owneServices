using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

internal class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
{
	public void TestCheckJ7_IsDutiable()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var charge = invoice.Charges.AddNew();
		charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
		charge.J7_IsDutiable = true;
		AssertNoMessageErrors(charge.J7_IsDutiableInfo);
	}
}
