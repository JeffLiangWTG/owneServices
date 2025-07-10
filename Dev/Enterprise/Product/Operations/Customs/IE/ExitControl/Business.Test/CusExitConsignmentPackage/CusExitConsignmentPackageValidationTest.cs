using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business.Testing;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	sealed class CusExitConsignmentPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXP_Quantity()
		{
			var targetInfo = cusExitConsignmentPackage.CXP_QuantityInfo;
			Factory.SetupBreakBulkCusCode();
			Factory.SetupBulkCusCode();

			CombineAssertions(() =>
			{
				cusExitConsignmentPackage.CXP_PackageType = "NE";
				cusExitConsignmentPackage.CXP_Quantity = 0;
				cusExitConsignmentPackage.Validation.ValidateCXP_Quantity();
				AssertHasMessageErrorContaining("Check for ZERO", targetInfo, MandatoryValidation.ValueCannotBeZero);

				cusExitConsignmentPackage.CXP_PackageType = "VG";
				cusExitConsignmentPackage.CXP_Quantity = 0;
				cusExitConsignmentPackage.Validation.ValidateCXP_Quantity();
				AssertNoMessageErrorContaining("Validation passes for ZERO", targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining("Mandatory ZERO", targetInfo, "Package quantity must be zero when package type is bulk");
				cusExitConsignmentPackage.CXP_Quantity = 1;
				AssertHasMessageErrorContaining("Mandatory ZERO", targetInfo, "Package quantity must be zero when package type is bulk");

				cusExitConsignmentPackage.CXP_Quantity = 0;
				cusExitConsignmentPackage.CXP_PackageType = "BX";
				cusExitConsignmentPackage.Validation.ValidateCXP_Quantity();
				AssertNoMessageErrors("Validation passes for zero", targetInfo);
				cusExitConsignmentPackage.CXP_Quantity = -1;
				AssertHasErrorContaining("Check for Negative", targetInfo, MandatoryValidation.ValueCannotBeNegative);
				cusExitConsignmentPackage.CXP_Quantity = 1;
				AssertNoMessageErrors("Validation passes for Positive", targetInfo);
			});
		}

		public void TestCheckCXP_PackageType()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UnitedNationsPackageTypes, "Package Units");
			helper.CreateCusCodeList(UnitedNationsRecommendations, UnitedNationsPackageTypes, "01", "Test1", new ZDateTime(2011, 12, 20), ZDateTime.Now.AddDays(5));
			Factory.Save();

			var targetInfo = cusExitConsignmentPackage.CXP_PackageTypeInfo;
			CombineAssertions(() =>
			{
				cusExitConsignmentPackage.Validation.ValidateCXP_PackageType();
				AssertHasMessageErrorContaining("Check for empty.", targetInfo, MandatoryValidation.YouHaveNotEntered);

				cusExitConsignmentPackage.CXP_PackageType = "01";
				AssertNoMessageErrorContaining("Validation passes.", targetInfo, MandatoryValidation.YouHaveNotEntered);

				cusExitConsignmentPackage.CXP_PackageType = "X";
				AssertHasMessageErrorContaining("Check for invalid selection.", targetInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckCXP_MarksAndNumbers()
		{
			var targetInfo = cusExitConsignmentPackage.CXP_MarksAndNumbersInfo;
			Factory.SetupBulkCusCode();
			Factory.SetupBreakBulkCusCode();
			cusExitConsignmentPackage.Validation.ValidateCXP_MarksAndNumbers();

			CombineAssertions(() =>
			{
				cusExitConsignmentPackage.CXP_PackageType = "BX";
				AssertHasMessageErrorContaining("Check for empty.", targetInfo, MandatoryValidation.YouHaveNotEntered);
				cusExitConsignmentPackage.CXP_PackageType = "VG";
				cusExitConsignmentPackage.Validation.ValidateCXP_MarksAndNumbers();
				AssertNoMessageErrors("Validation passes - Bulk type.", targetInfo);
				cusExitConsignmentPackage.CXP_PackageType = "NE";
				cusExitConsignmentPackage.Validation.ValidateCXP_MarksAndNumbers();
				AssertNoMessageErrors("Validation passes - BreakBulk type.", targetInfo);
				cusExitConsignmentPackage.CXP_PackageType = "BX";
				cusExitConsignmentPackage.CXP_MarksAndNumbers = "MARKS";
				cusExitConsignmentPackage.Validation.ValidateCXP_MarksAndNumbers();
				AssertNoMessageErrors("Validation passes - Shipping marks entered", targetInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusExitConsignmentPackage = CusExitConsignmentPackageTest.GetNewBusinessObject(Factory);
		}

		CusExitConsignmentPackage cusExitConsignmentPackage;
	}
}
