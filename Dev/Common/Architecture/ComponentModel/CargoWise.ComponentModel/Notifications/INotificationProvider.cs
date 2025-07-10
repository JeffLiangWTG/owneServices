using System.Collections.Generic;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Provides notifications from a business component.
	/// </summary>
	public interface INotificationProvider
	{
		IEnumerable<INotification> Notifications { get; }
		bool HasNotifications();
		bool HasNotifications(INotificationType type);
		INotificationType GetHighestSeverityNotificationType();
	}
}
