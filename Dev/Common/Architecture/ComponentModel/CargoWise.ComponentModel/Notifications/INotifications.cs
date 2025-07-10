using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Implemented on an object that subscribes to notifications. Typically this is implemented on
	/// a notification collection (for example NotificationCollection) or a user interface, such as a form.
	/// </summary>
	public interface INotifications
	{
		/// <summary>
		/// Add a notification to a collection of notifications, or to a user interface.
		/// </summary>
		void Add(INotification notification);
	}

	public static class NotificationsExtensions
	{
		#region Adding

		/// <summary>
		/// Calls INotifications.Add().
		/// </summary>
		public static void Notify(this INotifications notifications, INotification notification)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			notifications.Add(notification);
		}

		/// <summary>
		/// Add multiple notifications.
		/// </summary>
		public static void AddRange(this INotifications notifications, IEnumerable<INotification> list)
		{
			Argument.NotNull(notifications, nameof(notifications));
			if (list != null)
			{
				foreach (INotification notification in list)
				{
					if (notification != null)
					{
						notifications.Add(notification);
					}
				}
			}
		}

		/// <summary>
		/// Add a warning message to the collection.
		/// </summary>
		public static void AddWarning(this INotifications notifications, string message)
		{
			Argument.NotNull(notifications, nameof(notifications));
			Argument.NotNullOrEmpty(message, nameof(message));
			notifications.Add(NotificationType.Warning, message);
		}

		/// <summary>
		/// Add an error message to the collection.
		/// </summary>
		public static void AddError(this INotifications notifications, string message)
		{
			Argument.NotNull(notifications, nameof(notifications));
			Argument.NotNullOrEmpty(message, nameof(message));
			notifications.Add(NotificationType.Error, message);
		}

		/// <summary>
		/// Add a notification to the collection.
		/// </summary>
		public static void Add(this INotifications notifications, INotificationType type, string message)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			Argument.NotNull(type, nameof(type));
			Argument.NotNullOrEmpty(message, nameof(message));
			notifications.Add(new Notification(type, message));
		}

		#endregion
	}
}
