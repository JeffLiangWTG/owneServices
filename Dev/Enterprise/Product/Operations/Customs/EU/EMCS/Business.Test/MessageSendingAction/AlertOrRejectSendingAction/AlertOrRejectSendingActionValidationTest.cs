using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class AlertOrRejectSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDateOfAlertOrRejection()
		{
			var sendingAction = new AlertOrRejectSendingAction(Factory.New<EMCSJobDeclaration>());
			var info = sendingAction.DateOfAlertOrRejectionInfo;

			sendingAction.DateOfAlertOrRejection = ZDateTime.Empty;
			sendingAction.Validation.ValidateDateOfAlertOrRejection();
			AssertHasError(info, "Please enter a Date.");

			sendingAction.DateOfAlertOrRejection = new ZDateTime(2022, 11, 8);
			sendingAction.Validation.ValidateDateOfAlertOrRejection();
			AssertNoErrors(info);
		}
	}
}
