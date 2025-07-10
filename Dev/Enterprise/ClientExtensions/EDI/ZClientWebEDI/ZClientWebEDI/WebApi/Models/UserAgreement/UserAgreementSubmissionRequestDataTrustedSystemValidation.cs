using CargoWiseOne.ResourceStrings;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAgreementSubmissionRequestDataTrustedSystemValidation : UserAgreementSubmissionRequestDataValidation
	{
		public UserAgreementSubmissionRequestDataTrustedSystemValidation(UserAgreementInfo userInfo) : base(userInfo)
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

		string SystemIDErrorMessage => Res.GetString("357393f9-4297-4647-ac55-8f7cb0afd12b", "Please enter a valid {0}.", nameof(UserAgreementInfo.SystemId));
	}
}
