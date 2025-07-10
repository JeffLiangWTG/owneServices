using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public static class NotificationIconSchemeRegistration
	{
		public static void EnsureRegistered()
		{
			lock (NotificationIconScheme.Instance)
			{
				if (!isRegistered)
				{
					NotificationIconScheme.Instance.SetIcon(NotificationType.Error, Icons.GetIcon(IconTypes.Error));
					NotificationIconScheme.Instance.SetIcon(NotificationType.Warning, Icons.GetIcon(IconTypes.Warning));
					NotificationIconScheme.Instance.SetIcon(NotificationType.MessageError, Icons.GetIcon(IconTypes.MessageError));

					NotificationIconScheme.Instance.SetMiniImage(NotificationType.Error, Icons.GetMiniImage(IconTypes.Error));
					NotificationIconScheme.Instance.SetMiniImage(NotificationType.Warning, Icons.GetMiniImage(IconTypes.Warning));
					NotificationIconScheme.Instance.SetMiniImage(NotificationType.MessageError, Icons.GetMiniImage(IconTypes.MessageError));

					isRegistered = true;
				}
			}
		}

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Safely handled, required for forms on other threads")]
		static bool isRegistered;
	}
}
