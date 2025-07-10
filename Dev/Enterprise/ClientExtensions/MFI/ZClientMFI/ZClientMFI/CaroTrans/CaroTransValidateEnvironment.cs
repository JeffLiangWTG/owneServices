using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.MFI.CaroTrans
{
	public static class CaroTransValidateEnvironment
	{
		public static bool ValidateExport(INotifications notify)
		{
			if (MFIDataRegistry.Instance.CaroTransAgents.Length > 0)
			{
				string recipientEmailAddress = MFIDataRegistry.Instance.CaroTransTrackEmailAddress.Trim();
				if (EmailAddressValidation.IsEmailAddressValidAndNotEmpty(recipientEmailAddress))
				{
					return true;
				}
				else
				{
					notify.Notify(new ErrorNotification(ErrorType.Error, "CaroTrans track email address invalid or empty."));
				}
			}
			else
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, "CaroTrans agents not specified."));
			}

			return false;
		}
	}
}
