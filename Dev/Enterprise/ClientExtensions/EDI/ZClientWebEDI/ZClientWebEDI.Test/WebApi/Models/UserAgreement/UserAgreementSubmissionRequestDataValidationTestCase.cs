using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class UserAgreementSubmissionRequestDataValidationTestCase : TestCaseWithFactory
	{
		#region ValidateSubmissionRequestData

		public void TestValidateSubmissionRequestDataInvalidTypeShouldCreateErrorMessage()
		{
			var testHelper = new UserAgreementTestHelper();
			testHelper.SetupCustomerUserAccount(Factory);
			const string type = "AAA";
			testHelper.SetupAgreement(Factory, type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			testHelper.DisableEffectiveDateTriggers(TestConnection);
			Factory.Save();
			testHelper.EnableEffectiveDateTriggers(TestConnection);
			var userInfo = new UserAgreementInfo { Product = testHelper.LicenceDatabase.LD_Product, SystemId = testHelper.LicenceDatabase.LD_TenantID, UserId = testHelper.UserAccount.EUA_UserID, UserCountry = "AU", UserAgreementType = type, FullName = "alexander blah", Email = "alex@g.com" };
			var validator = GetNewUserAgreementSubmissionRequestDataValidation(userInfo);
			validator.ValidateSubmissionRequestData();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.UserAgreementType)}.", errors[0].Message);
			userInfo.UserAgreementType = "MYA";
			validator = GetNewUserAgreementSubmissionRequestDataValidation(userInfo);
			validator.ValidateSubmissionRequestData();
			errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(0, errors.Count);
		}

		public void TestValidateSubmissionRequestDataInvalidAgreementShouldCreateErrorMessage()
		{
			var testHelper = new UserAgreementTestHelper();
			testHelper.SetupCustomerUserAccount(Factory);
			const string type = "EUA";
			testHelper.SetupAgreement(Factory, type, "NZ", ZDateTime.UtcNow.AddDays(-1));
			testHelper.DisableEffectiveDateTriggers(TestConnection);
			Factory.Save();
			testHelper.EnableEffectiveDateTriggers(TestConnection);
			var userInfo = new UserAgreementInfo { Product = testHelper.LicenceDatabase.LD_Product, SystemId = testHelper.LicenceDatabase.LD_TenantID, UserId = testHelper.UserAccount.EUA_UserID, UserCountry = "AU", UserAgreementType = type, FullName = "alexander blah", Email = "alex@g.com" };
			var validator = GetNewUserAgreementSubmissionRequestDataValidation(userInfo);
			validator.CheckUserAgreementShouldExistAndNotBeAccepted(null, null);
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals($"There are no current User Agreements for the given {nameof(UserAgreementInfo.UserAgreementType)} and {nameof(UserAgreementInfo.UserCountry)} combination.", errors[0].Message);
		}

		public void TestValidateSubmissionRequestDataMaxLengthErrors()
		{
			var testHelper = new UserAgreementTestHelper();
			testHelper.SetupCustomerUserAccount(Factory);
			var userInfo = new UserAgreementInfo { Product = testHelper.LicenceDatabase.LD_Product, SystemId = testHelper.LicenceDatabase.LD_TenantID, UserId = testHelper.UserAccount.EUA_UserID, UserCountry = "AU", UserAgreementType = "MYA", FullName = ZString.Replicate('a', EdiCustomerUserAccountSchema.EUA_FullName.MaxLength + 1), Email = ZString.Replicate('a', EdiCustomerUserAccountSchema.EUA_Email.MaxLength + 1) };
			var validator = GetNewUserAgreementSubmissionRequestDataValidation(userInfo);
			validator.ValidateSubmissionRequestData();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals($"{nameof(UserAgreementInfo.FullName)} exceeds its max length of {EdiCustomerUserAccountSchema.EUA_FullName.MaxLength}", errors[0].Message);
			AssertEquals($"{nameof(UserAgreementInfo.Email)} exceeds its max length of {EdiCustomerUserAccountSchema.EUA_Email.MaxLength}", errors[1].Message);
		}

		public void TestValidateSubmissionRequestDataNullDataShouldCreateErrorMessage()
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
		}

		public void TestValidateSubmissionRequestDataShouldNotSubmitAgreementWhichIsAlreadySigned()
		{
			var testHelper = new UserAgreementTestHelper();
			testHelper.SetupCustomerUserAccount(Factory);
			const string type = "MYA";
			const string countryCode = "AU";
			var agreement = testHelper.SetupAgreement(Factory, type, countryCode, ZDateTime.UtcNow.AddDays(-1));
			testHelper.DisableEffectiveDateTriggers(TestConnection);
			Factory.Save();
			testHelper.EnableEffectiveDateTriggers(TestConnection);
			var userInfo = new UserAgreementInfo { Product = testHelper.LicenceDatabase.LD_Product, SystemId = testHelper.LicenceDatabase.LD_TenantID, UserId = testHelper.UserAccount.EUA_UserID, UserCountry = countryCode, UserAgreementType = type, FullName = "alexander blah", Email = "alex@g.com" };
			var validator = GetNewUserAgreementSubmissionRequestDataValidation(userInfo);
			validator.CheckUserAgreementShouldExistAndNotBeAccepted(agreement, testHelper.UserAccount);
			AssertEquals("Should not append any errors since the user has not accepted the agreement yet.", 0, validator.ErrorMessageBuilder.Messages.Count);
			var userAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "BAT"));
			var agreementLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_EUA = userAccount.PK;
			agreementLog.EUL_ERA = agreement.PK;
			agreementLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			validator.CheckUserAgreementShouldExistAndNotBeAccepted(agreement, null);
			AssertEquals("If no user account exists, this should not append any errors", 0, validator.ErrorMessageBuilder.Messages.Count);
			validator.CheckUserAgreementShouldExistAndNotBeAccepted(agreement, testHelper.UserAccount);
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals($"The User Agreement has already been submitted for the specified {nameof(UserAgreementInfo.UserId)}.", errors[0].Message);
		}

		#endregion

		protected virtual UserAgreementSubmissionRequestDataValidation GetNewUserAgreementSubmissionRequestDataValidation(IUserAgreementInfo userInfo)
		{
			return new UserAgreementSubmissionRequestDataValidation(userInfo);
		}
	}
}
