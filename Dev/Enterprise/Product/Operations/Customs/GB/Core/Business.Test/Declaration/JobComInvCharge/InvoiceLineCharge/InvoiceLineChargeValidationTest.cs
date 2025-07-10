using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class InvoiceLineChargeValidationTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestValidateChargeType()
		{
			var invoice = Factory.New<JobComInvoiceLine>();
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = "XXX";
			AssertEquals("Charge is not valid", true, charge.J7_ChargeTypeInfo.HasNotifications());
			charge.Validation.ValidateJ7_ChargeType();
			AssertHasMessageErrorContaining(charge.J7_ChargeTypeInfo, "Please enter a valid Charge code.");
			charge.J7_ChargeType = EU.Business.UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge;
			charge.Validation.ValidateJ7_ChargeType();
			AssertHasMessageErrorContaining(charge.J7_ChargeTypeInfo, "CDS no longer allows the declaration of a charge type to reduce the item price by the duty that the price includes, e.g. payment term DDP");
		}
	}
}
