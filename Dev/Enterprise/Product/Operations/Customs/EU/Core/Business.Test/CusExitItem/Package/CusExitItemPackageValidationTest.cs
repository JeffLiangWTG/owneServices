using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusExitItemPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheck_B5_UnitType()
		{
			Factory.SetupUnpackCusCode();
			Factory.SetupBulkCusCode();

			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var exitItem = exitDetail.CusExitItems.AddNew();
			var package = exitItem.Packages.AddNew();

			AssertNoMessageErrorContaining(package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(package.B5_UnitTypeInfo, ListValidation.InvalidCodeMessageError);

			package.B5_UnitCount = 1;
			package.RunPreSaveValidation();

			AssertHasMessageErrorContaining(package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(package.B5_UnitTypeInfo, ListValidation.InvalidCodeMessageError);

			package.B5_UnitType = "ZZ";
			AssertNoMessageErrorContaining(package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(package.B5_UnitTypeInfo, ListValidation.InvalidCodeMessageError);

			package.B5_UnitType = "VQ";
			AssertNoMessageErrorContaining(package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(package.B5_UnitTypeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
