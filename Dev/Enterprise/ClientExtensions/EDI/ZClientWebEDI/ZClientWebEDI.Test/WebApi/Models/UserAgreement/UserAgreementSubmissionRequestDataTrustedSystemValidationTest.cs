using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class UserAgreementSubmissionRequestDataTrustedSystemValidationTest : UserAgreementSubmissionRequestDataValidationTestCase
	{
		#region ValidateSubmissionRequestData

		public void TestValidateSubmissionRequestDataNullDataShouldCreateSystemIdErrorMessage()
		{
			var testHelper = new UserAgreementTestHelper();
			testHelper.SetupCustomerUserAccount(Factory);
			var userInfo = new UserAgreementInfo();
			var validator = GetNewUserAgreementSubmissionRequestDataValidation(userInfo);
			validator.ValidateSubmissionRequestData();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.Product)}.", errors[0].Message);
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.UserAgreementType)}.", errors[1].Message);
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.FullName)}.", errors[2].Message);
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.SystemId)}.", errors[3].Message);
		}

		#endregion

		protected override UserAgreementSubmissionRequestDataValidation GetNewUserAgreementSubmissionRequestDataValidation(IUserAgreementInfo userInfo)
		{
			return new UserAgreementSubmissionRequestDataTrustedSystemValidation((UserAgreementInfo)userInfo);
		}
	}
}
