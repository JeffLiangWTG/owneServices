using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class AddInfoJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestCheckZG_UsePostponedVatAccounting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferType = "A";
			declaration.ZG_UsePostponedVatAccounting = true;
			AssertHasMessageError(declaration.ZG_UsePostponedVatAccountingInfo, "This field is mutually exclusive with VAT field.");

			declaration.ZG_VATDeferType = "";
			declaration.Validation.ValidateZG_UsePostponedVatAccounting();
			AssertNoMessageError(declaration.ZG_UsePostponedVatAccountingInfo, "This field is mutually exclusive with VAT field.");

			declaration.ZG_VATDeferType = "A";
			declaration.ZG_UsePostponedVatAccounting = false;
			AssertNoMessageError(declaration.ZG_UsePostponedVatAccountingInfo, "This field is mutually exclusive with VAT field.");
		}

		public void TestCheckZG_VATDeferType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_UsePostponedVatAccounting = true;
			declaration.ZG_VATDeferType = "A";
			AssertHasMessageError(declaration.ZG_VATDeferTypeInfo, "This field is mutually exclusive with 'Use postponed VAT accounting?' field.");

			declaration.ZG_UsePostponedVatAccounting = false;
			declaration.AddInfoValidation.ValidateZG_VATDeferType();
			AssertNoMessageError(declaration.ZG_VATDeferTypeInfo, "This field is mutually exclusive with 'Use postponed VAT accounting?' field.");

			declaration.ZG_UsePostponedVatAccounting = true;
			declaration.ZG_VATDeferType = "";
			AssertNoMessageError(declaration.ZG_VATDeferTypeInfo, "This field is mutually exclusive with 'Use postponed VAT accounting?' field.");
		}
	}
}
