using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.MyAccount.Interfaces;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class UserAccountCreationRequestDataValidationTest : TestCaseWithFactory
	{
		#region Validate

		public void TestValidateNullUserIdShouldCreateErrorMessage()
		{
			var userInfo = new TrustedUserInfoForTest { FullName = "alexander blah", Email = "alex@g.com" };
			var validator = GetNewValidation(userInfo);
			validator.Validate();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(UserAccountCreationRequestDataValidation.UserIDEmptyErrorMessage, errors[0].Message);
			userInfo.UserId = "AGA";
			validator = GetNewValidation(userInfo);
			validator.Validate();
			errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(0, errors.Count);
		}

		public void TestValidateNullFullNameShouldCreateErrorMessage()
		{
			var userInfo = new TrustedUserInfoForTest { UserId = "U03217", Email = "alex@g.com" };
			var validator = GetNewValidation(userInfo);
			validator.Validate();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(UserAccountCreationRequestDataValidation.FullNameEmptyErrorMessage, errors[0].Message);
			userInfo.FullName = "alexander blah";
			validator = GetNewValidation(userInfo);
			validator.Validate();
			errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(0, errors.Count);
		}

		public void TestValidateNullEmailShouldCreateErrorMessage()
		{
			var userInfo = new TrustedUserInfoForTest { UserId = "U03217", FullName = "alexander blah", };
			var validator = GetNewValidation(userInfo);
			validator.Validate();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(UserAccountCreationRequestDataValidation.EmailEmptyErrorMessage, errors[0].Message);
			userInfo.Email = "alex@g.com";
			validator = GetNewValidation(userInfo);
			validator.Validate();
			errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(0, errors.Count);
		}

		public void TestValidateUserId_ExceedsMaxLength_ShouldCreateErrorMessage()
		{
			var maxLengthOverflowId = CreateStringOfLength(EdiCustomerUserAccountSchema.EUA_UserID.MaxLength + 1);
			var userInfo = new TrustedUserInfoForTest { UserId = maxLengthOverflowId, FullName = "alexander blah", Email = "alex@g.com" };
			var validator = GetNewValidation(userInfo);
			validator.Validate();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(UserAccountCreationRequestDataValidation.UserIDMaxLengthErrorMessage, errors[0].Message);
			userInfo.UserId = "AGA";
			validator = GetNewValidation(userInfo);
			validator.Validate();
			errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(0, errors.Count);
		}

		public void TestValidateFullName_ExceedsMaxLength_ShouldCreateErrorMessage()
		{
			var maxLengthOverflowFullName = CreateStringOfLength(EdiCustomerUserAccountSchema.EUA_FullName.MaxLength + 1);
			var userInfo = new TrustedUserInfoForTest { UserId = "U03217", FullName = maxLengthOverflowFullName, Email = "alex@g.com" };
			var validator = GetNewValidation(userInfo);
			validator.Validate();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(UserAccountCreationRequestDataValidation.FullNameMaxLengthErrorMessage, errors[0].Message);
			userInfo.FullName = "alexander blah";
			validator = GetNewValidation(userInfo);
			validator.Validate();
			errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(0, errors.Count);
		}

		public void TestValidateEmail_ExceedsMaxLength_ShouldCreateErrorMessage()
		{
			var maxLengthOverflowEmail = CreateStringOfLength(EdiCustomerUserAccountSchema.EUA_Email.MaxLength) + "@gmail.com";
			var userInfo = new TrustedUserInfoForTest { UserId = "U03217", FullName = "alexander blah", Email = maxLengthOverflowEmail };
			var validator = GetNewValidation(userInfo);
			validator.Validate();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(UserAccountCreationRequestDataValidation.EmailMaxLengthErrorMessage, errors[0].Message);
			userInfo.Email = "alex@g.com";
			validator = GetNewValidation(userInfo);
			validator.Validate();
			errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(0, errors.Count);
		}

		static string CreateStringOfLength(int length)
		{
			var stringBuilder = new StringBuilder(length);
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append("A");
			}
			return stringBuilder.ToString();
		}

		public void TestValidateInvalidEmail_ShouldCreateErrorMessage()
		{
			var userInfo = new TrustedUserInfoForTest { UserId = "U03217", FullName = "alexander blah", Email = "invalidEmail" };
			var validator = GetNewValidation(userInfo);
			validator.Validate();
			var errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(UserAccountCreationRequestDataValidation.EmailInvalidErrorMessage, errors[0].Message);
			userInfo.Email = "alex@g.com";
			validator = GetNewValidation(userInfo);
			validator.Validate();
			errors = validator.ErrorMessageBuilder.Messages;
			AssertEquals(0, errors.Count);
		}

		#endregion

		protected virtual UserAccountCreationRequestDataValidation GetNewValidation(ITrustedUserInfo userInfo)
		{
			return new UserAccountCreationRequestDataValidation(userInfo);
		}

		class TrustedUserInfoForTest : ITrustedUserInfo
		{
			public string UserId { get; set; }
			public string FullName { get; set; }
			public string Email { get; set; }
			public string UserCountry { get; set; }
			public string Product { get; set; }
			public DateTime InfoTimestamp { get; set; }
		}
	}
}
