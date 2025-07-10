using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCI_RN_NKCountryOfOrigin()
		{
			var pivot = Part.PivotsForBinding.AddNew();
			AssertNoNotifications(pivot.CI_RN_NKCountryOfOriginInfo);
			pivot.CI_RN_NKCountryOfOrigin = "XX";
			AssertHasMessageErrorContaining(pivot.CI_RN_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_RN_NKCountryOfOrigin = "CA";
			AssertNoMessageError(pivot.CI_RN_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public void CheckCI_RW_NKOriginState()
		{
			var pivot = Part.PivotsForBinding.AddNew();
			AssertNoNotifications(pivot.CI_RW_NKOriginStateInfo);
			pivot.CI_RW_NKOriginState = "XX";
			AssertHasMessageErrorContaining(pivot.CI_RW_NKOriginStateInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_RN_NKCountryOfOrigin = "CA";
			pivot.CI_RW_NKOriginState = "AB";
			AssertNoMessageError(pivot.CI_RW_NKOriginStateInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCI_RN_NKCountryOfExport()
		{
			var pivot = Part.PivotsForBinding.AddNew();
			AssertNoNotifications(pivot.CI_RN_NKCountryOfExportInfo);
			pivot.CI_RN_NKCountryOfExport = "XX";
			AssertHasMessageErrorContaining(pivot.CI_RN_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_RN_NKCountryOfExport = "CA";
			AssertNoMessageError(pivot.CI_RN_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCI_TariffNum()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000");
			var pivot = Part.PivotsForBinding.AddNew();
			pivot.Validation.ValidateCI_TariffNum();
			AssertHasErrorContaining(pivot.CI_TariffNumInfo, "enter");
			pivot.CI_TariffNum = "1111111111";
			AssertHasMessageError(pivot.CI_TariffNumInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_TariffNum = "2713200000";
			AssertNoMessageError(pivot.CI_TariffNumInfo, ListValidation.InvalidCodeMessageError);
		}

		OrgSupplierPart Part => part ?? (part = Factory.New<OrgSupplierPart>());
		OrgSupplierPart part;
	}
}
