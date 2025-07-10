using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class CusExitContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXN_Status_Length()
		{
			CombineAssertions(() =>
			{
				var expectedError = "The length must be 0 or 3";
				var header = Factory.New<CusExitHeader>();
				var container = header.CusExitContainers.AddNew();

				container.CXN_Status = "AH";
				AssertHasErrorContaining(container.CXN_StatusInfo, expectedError);

				container.CXN_Status = "AH3";
				AssertNoErrorContaining(container.CXN_StatusInfo, expectedError);
			});
		}

		public void TestCheckCXN_Status_ListValidation()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<CusExitHeader>();
				var container = header.CusExitContainers.AddNew();

				container.CXN_Status = "AH3";
				AssertListValidationInvalidCodeMessageError(container.CXN_StatusInfo, true);

				container.CXN_Status = "DIF";
				AssertListValidationInvalidCodeMessageError(container.CXN_StatusInfo, false);
			});
		}
	}
}
