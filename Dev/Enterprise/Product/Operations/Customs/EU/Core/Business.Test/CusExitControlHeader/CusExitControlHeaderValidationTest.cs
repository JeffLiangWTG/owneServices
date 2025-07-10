using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusExitControlHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEH_ReferenceNumber()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_ReferenceNumber = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertHasErrorContaining(exitHeader.CEH_ReferenceNumberInfo, "Please enter a value.");
				exitHeader.CEH_ReferenceNumber = "Test";
				AssertNoErrorContaining(exitHeader.CEH_ReferenceNumberInfo, "Please enter a value.");
			});
		}
	}
}
