using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ImportCusCAClassificationValidationTest : TestCaseWithFactory
	{
		public void TestCheckCCA_AMMVProperties()
		{
			#region SetUp Data
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var concurrencyErrorMsg = "You can enter either Assist/AMMV Unit or Assist/AMMV Percent, not both.";
			#endregion

			pivot.CCA_AMMVPercentage = 4m;
			pivot.CCA_AMMVPerUnit = ZDecimal.Zero;
			AssertNoMessageErrors(pivot.CCA_AMMVPerUnitInfo);

			pivot.CCA_AMMVPerUnit = 2m;
			AssertHasMessageError(pivot.CCA_AMMVPerUnitInfo, concurrencyErrorMsg);

			pivot.CCA_AMMVPercentage = ZDecimal.Zero;
			AssertNoMessageErrors(pivot.CCA_AMMVPercentageInfo);

			pivot.CCA_AMMVPercentage = 4m;
			AssertHasMessageError(pivot.CCA_AMMVPercentageInfo, concurrencyErrorMsg);

			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			details.Validation.ValidateCCA_AMMVPercentage();
			AssertNoMessageErrors(pivot.CCA_AMMVPercentageInfo);
			details.Validation.ValidateCCA_AMMVPerUnit();
			AssertNoMessageErrors(pivot.CCA_AMMVPerUnitInfo);

			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			details.Validation.ValidateCCA_AMMVPercentage();
			AssertNoMessageErrors(pivot.CCA_AMMVPercentageInfo);
			details.Validation.ValidateCCA_AMMVPerUnit();
			AssertNoMessageErrors(pivot.CCA_AMMVPerUnitInfo);
		}
	}
}
