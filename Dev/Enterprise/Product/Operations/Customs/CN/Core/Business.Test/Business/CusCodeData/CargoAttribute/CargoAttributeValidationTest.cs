using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CargoAttributeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var test = Factory.New<CargoAttribute>();
			test.Validation.ValidateCY_Code();
			AssertNoMessageErrors(test.CY_CodeInfo);
		}
	}
}
