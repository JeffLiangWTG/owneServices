using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSPackageValidatorTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_MarksAndNumbers()
		{
			var package = Factory.New<EMCSPackage>();
			CombineAssertions(() =>
			{
				package.B5_UnitType = EMCSPackageTestHelper.UncountableUnitType;
				package.B5_MarksAndNumbers = ZString.Empty;
				AssertNoMessageErrorContaining("Should not validate if uncountable", package.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

				package.B5_UnitType = EMCSPackageTestHelper.CountableUnitType;
				package.B5_MarksAndNumbers = ZString.Empty;
				AssertHasMessageErrorContaining("Should validate if countable", package.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			EMCSPackageTestHelper.SetupPackageTypeCodeList(Factory);
		}
	}
}
