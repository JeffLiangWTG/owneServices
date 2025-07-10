using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class CancellationSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckReason()
		{
			const string enterValueMsg = "Please enter a Reason.";
			const string enterValidSelectionMsg = "Enter a valid Reason.";
			var jobDeclaration = Factory.New<EMCSJobDeclaration>();
			var cancellation = new CancellationSendingAction(jobDeclaration);
			var info = cancellation.ReasonInfo;

			cancellation.Reason = ZString.Empty;
			cancellation.Validation.ValidateReason();
			AssertHasError(info, enterValueMsg);
			AssertNoError(info, enterValidSelectionMsg);

			cancellation.Reason = EMCSCancellationReasonList.Codes._1;
			AssertNoError(info, enterValueMsg);
			AssertNoError(info, enterValidSelectionMsg);

			cancellation.Reason = "@";
			AssertNoError(info, enterValueMsg);
			AssertHasError(info, enterValidSelectionMsg);
		}

		public void TestCheckInformation()
		{
			const string enterValueMsg = "Please enter an Information.";
			var jobDeclaration = Factory.New<EMCSJobDeclaration>();
			var cancellation = new CancellationSendingAction(jobDeclaration);
			var info = cancellation.InformationInfo;

			cancellation.Reason = ZString.Empty;
			cancellation.Information = ZString.Empty;
			cancellation.Validation.ValidateInformation();
			AssertNoError(info, enterValueMsg);

			cancellation.Reason = EMCSCancellationReasonList.Codes._0;
			cancellation.Validation.ValidateInformation();
			AssertHasError(info, enterValueMsg);

			cancellation.Reason = EMCSCancellationReasonList.Codes._1;
			cancellation.Validation.ValidateInformation();
			AssertNoError(info, enterValueMsg);
		}
	}
}
