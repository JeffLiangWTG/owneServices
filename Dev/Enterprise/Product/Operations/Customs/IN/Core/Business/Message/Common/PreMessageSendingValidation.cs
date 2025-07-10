using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Business;

public class PreMessageSendingValidation
{
	public ZString ValidateMessageSending()
	{
		var result = new ZStringBuilder();
		result.AppendIfNotEmpty(ValidateStaffCredentials());
		return result.ToStringWithNewLineBetweenAppends();
	}

	ZString ValidateStaffCredentials()
	{
		var result = ZString.Empty;

		if (!GlbStaff.CurrentUser.IsSupportUser)
		{
			var staffWrapper = GlbStaffWrapper.GetWrapperForCurrentUser();
			var certificatePassword = staffWrapper.GetCertificatePassword();
			var tokenProfileEmpty = certificatePassword == null || certificatePassword.LibraryName.IsEmpty || certificatePassword.GP_CertificateSerialNumber.IsEmpty;

			var loginPassword = staffWrapper?.GetLoginPassword();
			var userIDEmpty = loginPassword?.GP_UserID.IsEmpty ?? true;
			var mailBoxIDEmpty = loginPassword?.GP_MailBoxID.IsEmpty ?? true;

			if (userIDEmpty && mailBoxIDEmpty && tokenProfileEmpty)
			{
				result = Res.GetString("9394B757-1364-4CA2-A864-54C3E9861A35", "ICEGATE Login ID and Email ID and DSC Token Profile is not defined for current user under Staff & Resources Credentials.");
			}
			else if (userIDEmpty && mailBoxIDEmpty)
			{
				result = Res.GetString("FA12A65E-25D5-425A-8AF7-730DAC1BB0D9", "ICEGATE Login ID and Email ID are not defined for current user under Staff & Resources Credentials.");
			}
			else if (userIDEmpty)
			{
				result = Res.GetString("A6042607-7C18-4662-AA93-CC555D797E5E", "ICEGATE Login ID is not defined for current user.");
			}
			else if (mailBoxIDEmpty)
			{
				result = Res.GetString("86F93E9C-4B93-4186-A935-B0704367E3C1", "ICEGATE Email ID is not defined for current user.");
			}
			else if (tokenProfileEmpty)
			{
				result = Res.GetString("49BFAD6F-8429-43BC-B9B8-8DB1C843E320", "DSC Token Profile is not setup for current user.");
			}
		}
		return result;
	}
}
