using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusExitItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXI_GrossMassUQ()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(exitItem.CXI_GrossMassUQInfo);

			exitItem.CXI_GrossMass = 1m;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(exitItem.CXI_GrossMassUQInfo);
		}

		public void TestCheckCXI_GrossMassUQ_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(exitItem.CXI_GrossMassUQInfo, "BLA", "KG");
		}

		public void TestCheckCXI_NetMassUQ()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(exitItem.CXI_NetMassUQInfo);

			exitItem.CXI_NetMass = 1m;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(exitItem.CXI_NetMassUQInfo);
		}

		public void TestCheckCXI_NetMassUQ_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(exitItem.CXI_NetMassUQInfo, "BLA", "KG");
		}

		public void TestCheckCXI_Status()
		{
			var message = "Status length must be 3";
			CombineAssertions(() =>
			{
				exitItem.CXI_Status = "AB";
				AssertHasError("Has error", exitItem.CXI_StatusInfo, message);

				exitItem.CXI_Status = "ABC";
				AssertNoError("No error", exitItem.CXI_StatusInfo, message);
			});
		}

		public void TestCheckCXI_Status_ListValidation()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(exitItem.CXI_StatusInfo, "XXX", ExitItemStatusList.Codes.CAN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitItem = exitDetail.CusExitItems.AddNew();
		}
		CusExitItem exitItem;
	}
}
