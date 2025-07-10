using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Interfaces;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAccountCreationRequestDataValidation
	{
		public UserAccountCreationRequestDataValidation(ITrustedUserInfo userInfo)
		{
			UserInfo = userInfo;
		}

		readonly ITrustedUserInfo UserInfo;

		public ErrorMessages ErrorMessageBuilder { get; } = new ErrorMessages();

		public void Validate()
		{
			CheckForNullFields();

			if (ErrorMessageBuilder.Messages.Count == 0)
			{
				ValidateUserId();
				ValidateFullName();
				ValidateEmail();
			}
		}

		void CheckForNullFields()
		{
			if (string.IsNullOrWhiteSpace(UserInfo.UserId))
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_MissingRequiredField, UserIDEmptyErrorMessage);
			}

			if (string.IsNullOrWhiteSpace(UserInfo.FullName))
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_MissingRequiredField, FullNameEmptyErrorMessage);
			}

			if (string.IsNullOrWhiteSpace(UserInfo.Email))
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_MissingRequiredField, EmailEmptyErrorMessage);
			}
		}

		void ValidateUserId()
		{
			if (UserInfo.UserId.Length > EdiCustomerUserAccountSchema.EUA_UserID.MaxLength)
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, UserIDMaxLengthErrorMessage);
			}
		}

		void ValidateFullName()
		{
			if (UserInfo.FullName.Length > EdiCustomerUserAccountSchema.EUA_FullName.MaxLength)
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, FullNameMaxLengthErrorMessage);
			}
		}

		void ValidateEmail()
		{
			if (UserInfo.Email != null && UserInfo.Email.Length > EdiCustomerUserAccountSchema.EUA_Email.MaxLength)
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, EmailMaxLengthErrorMessage);
			}
			else if (!EmailAddressValidation.IsEmailAddressValid(UserInfo.Email))
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, EmailInvalidErrorMessage);
			}
		}

		public static string UserIDEmptyErrorMessage => Res.GetString("34880646-f0de-43fa-ba34-6e15106d133f", "Please enter a valid '{0}'.", ITrustedUserInfo.Schema.UserId);
		public static string FullNameEmptyErrorMessage => Res.GetString("c34e5bf4-ffe0-4769-a714-b3710ea100fc", "Please enter a valid '{0}'.", ITrustedUserInfo.Schema.FullName);
		public static string EmailEmptyErrorMessage => Res.GetString("5b3bc291-0ab3-4b51-8a56-9dd83394ec08", "Please enter a valid '{0}'.", ITrustedUserInfo.Schema.Email);
		public static string UserIDMaxLengthErrorMessage => Res.GetString("13bc7dad-8ff4-462e-9838-86932f00d8a2", "'{0}' exceeds its max length of {1} characters.", ITrustedUserInfo.Schema.UserId, EdiCustomerUserAccountSchema.EUA_UserID.MaxLength);
		public static string FullNameMaxLengthErrorMessage => Res.GetString("523e7d31-40d2-4098-8da2-7804d247b4d1", "'{0}' exceeds its max length of {1} characters.", ITrustedUserInfo.Schema.FullName, EdiCustomerUserAccountSchema.EUA_FullName.MaxLength);
		public static string EmailMaxLengthErrorMessage => Res.GetString("f48bc7a4-ac56-499b-811c-6a01a2efe80c", "'{0}' exceeds its max length of {1} characters.", ITrustedUserInfo.Schema.Email, EdiCustomerUserAccountSchema.EUA_Email.MaxLength);
		public static string EmailInvalidErrorMessage => Res.GetString("06afdc24-a283-491c-8203-13ff26090958", "'{0}' is not valid.", ITrustedUserInfo.Schema.Email);
	}
}
