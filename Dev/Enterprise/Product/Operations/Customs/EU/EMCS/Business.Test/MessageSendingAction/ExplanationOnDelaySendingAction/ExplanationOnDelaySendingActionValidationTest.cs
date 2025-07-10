using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class ExplanationOnDelaySendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckExplanationCode()
		{
			var info = explanation.ExplanationCodeInfo;

			explanation.ExplanationCode = ZString.Empty;
			explanation.Validation.ValidateExplanationCode();
			AssertHasError(info, "Please enter an Explanation Code.");

			explanation.ExplanationCode = "A";
			explanation.Validation.ValidateExplanationCode();
			AssertHasError(info, "Enter a valid Explanation Code.");

			explanation.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.CancelledCommercialTransaction;
			explanation.Validation.ValidateExplanationCode();
			AssertNoErrors(info);
		}

		public void TestCheckInformation()
		{
			var info = explanation.InformationInfo;

			explanation.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.Other;
			explanation.Information = ZString.Empty;
			explanation.Validation.ValidateInformation();
			AssertHasError(info, "Please enter an Information.");

			explanation.Information = "Test Information";
			explanation.Validation.ValidateInformation();
			AssertNoErrors(info);

			explanation.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.CancelledCommercialTransaction;
			explanation.Information = ZString.Empty;
			explanation.Validation.ValidateInformation();
			AssertNoErrors(info);
		}

		public void TestCheckMessageRole()
		{
			var info = explanation.MessageRoleInfo;

			explanation.MessageRole = ZString.Empty;
			explanation.Validation.ValidateMessageRole();
			AssertHasError(info, "Please enter a Message Role.");

			explanation.MessageRole = "A";
			explanation.Validation.ValidateMessageRole();
			AssertHasError(info, "Enter a valid Message Role.");

			explanation.MessageRole = EMCSExplanationOnDelayMessageRoleCodeList.Codes._1;
			explanation.Validation.ValidateMessageRole();
			AssertNoErrors(info);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var jobDeclaration = Factory.New<EMCSJobDeclaration>();
			explanation = new ExplanationOnDelaySendingAction(jobDeclaration);
		}

		ExplanationOnDelaySendingAction explanation;
	}
}
