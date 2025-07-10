using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class SpecialBusinessIdentifierValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var test = Factory.New<SpecialBusinessIdentifier>();
			test.Validation.ValidateCY_Code();
			AssertNoMessageErrors(test.CY_CodeInfo);
		}
	}
}
