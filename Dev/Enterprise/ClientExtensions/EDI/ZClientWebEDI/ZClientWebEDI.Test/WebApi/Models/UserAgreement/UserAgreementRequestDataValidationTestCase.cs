using CargoWise.EntityFramework.Testing;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class UserAgreementRequestDataValidationTestCase : TestCaseWithFactory
	{
		public void TestValidateGetRequestDataNullDataShouldCreateErrorMessage()
		{
			var testHelper = new UserAgreementTestHelper();
			testHelper.SetupCustomerUserAccount(Factory);
			var userInfo = new UserAgreementInfo();
			var validator = GetNewUserAgreementRequestDataValidation(userInfo);
			validator.ValidateGetRequestData();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.Product)}.", errors[0].Message);
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.UserAgreementType)}.", errors[1].Message);
		}

		public void TestValidateGetRequestDataInvalidTypeShouldCreateErrorMessage()
		{
			var testHelper = new UserAgreementTestHelper();
			testHelper.SetupCustomerUserAccount(Factory);
			var userInfo = new UserAgreementInfo { Product = testHelper.LicenceDatabase.LD_Product, SystemId = testHelper.LicenceDatabase.LD_TenantID, UserId = testHelper.UserAccount.EUA_UserID, UserCountry = "AU", UserAgreementType = "EXA" };
			var validator = GetNewUserAgreementRequestDataValidation(userInfo);
			validator.ValidateGetRequestData();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.UserAgreementType)}.", errors[0].Message);
			userInfo.UserAgreementType = "MYA";
			validator = GetNewUserAgreementRequestDataValidation(userInfo);
			validator.ValidateGetRequestData();
			AssertEquals(0, validator.ErrorMessageBuilder.Messages.Count);
		}

		public void TestValidateGetRequestDataMultipleErrorsShouldGetAppended()
		{
			var userInfo = new UserAgreementInfo { Product = "CW1", SystemId = "1241905", UserId = "AAA", UserCountry = "AU", UserAgreementType = "XXX" };
			var validator = GetNewUserAgreementRequestDataValidation(userInfo);
			validator.ValidateGetRequestData();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.UserAgreementType)}.", errors[0].Message);
		}

		protected virtual UserAgreementRequestDataValidation GetNewUserAgreementRequestDataValidation(IUserAgreementInfo userInfo)
		{
			return new UserAgreementRequestDataValidation(userInfo);
		}
	}
}
