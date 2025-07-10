using CargoWise.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Environment
{
	[Immutable]
	public class GlobalNotificationsWrapper : INotifications
	{
		GlobalNotificationsWrapper()
		{
		}
		public static readonly GlobalNotificationsWrapper Instance = new GlobalNotificationsWrapper();

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		void Notify(INotification notification)
		{
			if (notification.Type.EnumValueName == NotificationType.Information.EnumValueName)
			{
				Globals.Message.ShowInformation(notification.Message);
			}
			else if (notification.Type.EnumValueName == NotificationType.Warning.EnumValueName)
			{
				Globals.Message.ShowWarning(notification.Message);
			}
			else
			{
				Globals.Message.ShowError(notification.Message);
			}
		}

		#endregion
	}
}
