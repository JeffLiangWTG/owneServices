using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class ReasonForShortageSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateGeneralExplanation()
		{
			var sendReasonForShortage = new ReasonForShortageSendingAction(Factory.New<EMCSJobDeclaration>());
			var generalExplanationInfo = sendReasonForShortage.GeneralExplanationInfo;

			sendReasonForShortage.GeneralExplanation = ZString.Empty;
			sendReasonForShortage.Validation.ValidateGeneralExplanation();
			AssertHasError(generalExplanationInfo, "Please enter a General Explanation.");

			sendReasonForShortage.GeneralExplanation = new string('*', 100);
			sendReasonForShortage.Validation.ValidateGeneralExplanation();
			AssertNoError(generalExplanationInfo, "Please enter a General Explanation.");
		}
	}
}
