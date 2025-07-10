using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class UserAgreementRequestDataTrustedSystemValidationTest : UserAgreementRequestDataValidationTestCase
	{
		public void TestValidateGetRequestDataNullDataShouldCreateSystemIdErrorMessage()
		{
			var testHelper = new UserAgreementTestHelper();
			testHelper.SetupCustomerUserAccount(Factory);
			var userInfo = new UserAgreementInfo();
			var validator = GetNewUserAgreementRequestDataValidation(userInfo);
			validator.ValidateGetRequestData();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.Product)}.", errors[0].Message);
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.UserAgreementType)}.", errors[1].Message);
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.SystemId)}.", errors[2].Message);
		}

		protected override UserAgreementRequestDataValidation GetNewUserAgreementRequestDataValidation(IUserAgreementInfo userInfo)
		{
			return new UserAgreementRequestDataTrustedSystemValidation((UserAgreementInfo)userInfo);
		}
	}
}
