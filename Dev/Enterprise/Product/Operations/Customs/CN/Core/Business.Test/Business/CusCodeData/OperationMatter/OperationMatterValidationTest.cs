using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class OperationMatterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var test = Factory.New<OperationMatter>();
			test.Validation.ValidateCY_Code();
			AssertNoMessageErrors(test.CY_CodeInfo);
		}
	}
}
