using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAgreementSubmissionRequestDataValidation
	{
		public UserAgreementSubmissionRequestDataValidation(IUserAgreementInfo userInfo)
		{
			UserInfo = userInfo;
		}

		readonly IUserAgreementInfo UserInfo;

		public ErrorMessages ErrorMessageBuilder { get; } = new ErrorMessages();

		public void ValidateSubmissionRequestData()
		{
			CheckForNullFields();

			if (ErrorMessageBuilder.Messages.Count == 0)
			{
				ValidateType();
				ValidateFullName();
				ValidateEmail();
			}
		}

		void CheckForNullFields()
		{
			if (UserInfo.Product == null)
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_MissingRequiredField, ProductErrorMessage);
			}

			if (UserInfo.UserAgreementType == null)
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_MissingRequiredField, TypeErrorMessage);
			}

			if (UserInfo.FullName == null)
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_MissingRequiredField, FullNameErrorMessage);
			}

			CheckForAdditionalNullFields();
		}

		protected virtual void CheckForAdditionalNullFields()
		{
		}

		void ValidateType()
		{
			if (!UserAgreementRequestValidationHelper.IsValidAgreementType(UserInfo.UserAgreementType))
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, TypeErrorMessage);
			}
		}

		void ValidateFullName()
		{
			if (UserInfo.FullName.Length > EdiCustomerUserAccountSchema.EUA_FullName.MaxLength)
			{
				var message = Res.GetString("d3ce9495-6553-47a1-a590-fd96805d4ba1", "{0} exceeds its max length of {1}", nameof(UserAgreementInfo.FullName), EdiCustomerUserAccountSchema.EUA_FullName.MaxLength);
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, message);
			}
		}

		void ValidateEmail()
		{
			if (UserInfo.Email != null && UserInfo.Email.Length > EdiCustomerUserAccountSchema.EUA_Email.MaxLength)
			{
				var message = Res.GetString("25047e6f-2859-491a-a3fa-03d727322555", "{0} exceeds its max length of {1}", nameof(UserAgreementInfo.Email), EdiCustomerUserAccountSchema.EUA_Email.MaxLength);
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, message);
			}
		}

		public void CheckUserAgreementShouldExistAndNotBeAccepted(EdiUserAgreement currentAgreement, EdiCustomerUserAccount userAccount)
		{
			if (currentAgreement == null)
			{
				var message = Res.GetString("bd6fdd5a-f5fe-48cd-8b32-c763330cc1d0",
					"There are no current User Agreements for the given {0} and {1} combination.",
					nameof(UserAgreementInfo.UserAgreementType),
					nameof(UserAgreementInfo.UserCountry));
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, message);
			}
			else if (userAccount != null && userAccount.HasAcknowledgedUserAgreement(currentAgreement))
			{
				var message = Res.GetString("afa71da2-28f5-4a3e-b421-187bc033a425",
					"The User Agreement has already been submitted for the specified {0}.",
					nameof(UserAgreementInfo.UserId));
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, message);
			}
		}

		public void ValidateUserAccount(EdiCustomerUserAccount userAccount)
		{
			userAccount.Validation.ValidateAll();

			if (userAccount.HasErrors)
			{
				var errors = userAccount.Notifications.Where(x => x.Message.Contains("Error - "));
				foreach (var error in errors)
				{
					var errorMessage = error.Message;
					errorMessage = errorMessage.Replace(EdiCustomerUserAccountSchema.Constants.EUA_UserID, nameof(UserAgreementInfo.UserId));
					errorMessage = errorMessage.Replace(EdiCustomerUserAccountSchema.Constants.EUA_FullName, nameof(UserAgreementInfo.FullName));
					errorMessage = errorMessage.Replace(EdiCustomerUserAccountSchema.Constants.EUA_Email, nameof(UserAgreementInfo.Email));
					ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_InvalidValue, errorMessage);
				}
			}
		}

		string ProductErrorMessage => Res.GetString("ecfd4a39-b6ee-4c6f-bf35-720af86e948c", "Please enter a valid {0}.", nameof(UserAgreementInfo.Product));
		string TypeErrorMessage => Res.GetString("73ad8892-0fbd-417b-8958-07a7360c7188", "Please enter a valid {0}.", nameof(UserAgreementInfo.UserAgreementType));
		string FullNameErrorMessage => Res.GetString("fb07eff0-82d0-4ae2-b396-21aaba607df6", "Please enter a valid {0}.", nameof(UserAgreementInfo.FullName));
	}
}
