using CargoWiseOne.ResourceStrings;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAgreementRequestDataValidation
	{
		public UserAgreementRequestDataValidation(IUserAgreementInfo userInfo)
		{
			UserInfo = userInfo;
		}

		readonly IUserAgreementInfo UserInfo;

		public ErrorMessages ErrorMessageBuilder { get; } = new ErrorMessages();

		public void ValidateGetRequestData()
		{
			CheckForNullFields();

			if (ErrorMessageBuilder.Messages.Count == 0)
			{
				ValidateType();
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

		string ProductErrorMessage => Res.GetString("3b576996-7735-4e73-8b27-8f21ce35ea6d", "Please enter a valid {0}.", nameof(UserAgreementInfo.Product));
		string TypeErrorMessage => Res.GetString("ddcf0d50-cc49-4b22-8ea9-17f183f865bf", "Please enter a valid {0}.", nameof(UserAgreementInfo.UserAgreementType));
	}
}
