using System;

namespace CargoWise.EntityFramework
{
	public class NotificationsChangedEventArgs : EventArgs
	{
		public NotificationsChangedEventArgs(IBusiness sourceOfNotificationChange)
		{
			this.SourceOfNotificationChange = sourceOfNotificationChange;
		}

		public readonly IBusiness SourceOfNotificationChange;
	}
}
