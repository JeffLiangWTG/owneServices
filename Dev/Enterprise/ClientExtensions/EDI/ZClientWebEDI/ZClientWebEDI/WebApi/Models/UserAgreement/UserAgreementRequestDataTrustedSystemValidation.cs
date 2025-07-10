using CargoWiseOne.ResourceStrings;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAgreementRequestDataTrustedSystemValidation : UserAgreementRequestDataValidation
	{
		public UserAgreementRequestDataTrustedSystemValidation(UserAgreementInfo userInfo) : base(userInfo)
		{
			UserInfo = userInfo;
		}

		readonly UserAgreementInfo UserInfo;

		protected override void CheckForAdditionalNullFields()
		{
			if (UserInfo.SystemId == null)
			{
				ErrorMessageBuilder.AddMessage(ErrorCodes.Codes.Validation_MissingRequiredField, SystemIDErrorMessage);
			}
		}

		string SystemIDErrorMessage => Res.GetString("39607c82-fc26-47df-ae5c-8b5d536ddfcc", "Please enter a valid {0}.", nameof(UserAgreementInfo.SystemId));
	}
}
