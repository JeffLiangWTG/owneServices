using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class OtherPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var test = Factory.New<OtherPackage>();
			test.Validation.ValidateCY_Code();
			AssertNoMessageErrors(test.CY_CodeInfo);
		}
	}
}
