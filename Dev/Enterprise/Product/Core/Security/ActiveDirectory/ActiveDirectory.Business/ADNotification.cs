using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	public static class ADNotification
	{
		public static void ShowError(string message)
		{
			if (Globals.Message.IsInteractive)
			{
				Globals.Message.ShowError(message);
			}
			else
			{
				UnattendedUserNotification.Instance.ShowErrorToEmailGroup(message, GetADNotificationEmailGroup());
			}
		}

		public static void ShowWarning(string message)
		{
			if (Globals.Message.IsInteractive)
			{
				Globals.Message.ShowWarning(message);
			}
			else
			{
				UnattendedUserNotification.Instance.ShowWarningToEmailGroup(message, GetADNotificationEmailGroup());
			}
		}

		static Guid GetADNotificationEmailGroup() => ActiveDirectoryRegistry.Instance.ADNotificationGroup.Value;
	}
}
