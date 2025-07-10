using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class InvoiceLineChargeValidationTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckJ7_ChargeType()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "XXX";
			charge.J7_IsDutiable = true;
			charge.J7_IsIncludedInITOT = false;

			CombineAssertions(() =>
			{
				var expectedMessageError = "Invalid Charge code dutiable and not included in lines is not allowed.";
				charge.Validation.ValidateAll();
				AssertHasMessageErrorContaining(charge.J7_ChargeTypeInfo, expectedMessageError);

				charge.J7_IsDutiable = false;
				charge.J7_IsIncludedInITOT = true;
				charge.Validation.ValidateAll();
				AssertNoMessageErrorContaining(charge.J7_ChargeTypeInfo, expectedMessageError);

				charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
				charge.Validation.ValidateAll();
				AssertNoMessageErrorContaining(charge.J7_ChargeTypeInfo, expectedMessageError);
			});
		}
	}
}
