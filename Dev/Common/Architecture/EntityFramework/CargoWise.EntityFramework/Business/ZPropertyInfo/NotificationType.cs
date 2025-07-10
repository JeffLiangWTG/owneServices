using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class NotificationType : ComponentModel.NotificationType
	{
		protected NotificationType(int priority, bool isFatal)
			: base(priority, isFatal)
		{
		}

		protected NotificationType(string name, int priority, bool isFatal)
			: base(name, priority, isFatal)
		{
		}

		public static readonly INotificationType MessageError = new ComponentModel.NotificationType((NoResString)"Message Error", 150, false, "MessageError");
	}
}

namespace CargoWise.EntityFramework
{
	public static class NotificationProviderConvenienceMethodExtensions
	{
		[Obsolete("Add using CargoWise.ComponentModel to the top of your source file", true)]
		public static void EnsureYouHaveDeclared_using_CargoWise_ComponentModel(this IEnumerable<INotification> notifications)
		{
		}

		#region MessageError

		/// <summary>
		/// Are there any notifications of type NotificationType.MessageError?
		/// </summary>
		public static bool HasMessageErrors(this IEnumerable<INotification> notifications)
		{ return notifications.HasNotifications(NotificationType.MessageError); }

		/// <summary>
		/// Are there any notifications of type NotificationType.MessageError?
		/// </summary>
		public static bool HasMessageErrors(this INotificationProvider provider)
		{ return provider.HasNotifications(NotificationType.MessageError); }

		/// <summary>
		/// Get notifications of type NotificationType.MessageError.
		/// </summary>
		public static IEnumerable<INotification> GetMessageErrors(this IEnumerable<INotification> notifications)
		{ return notifications.GetNotifications(NotificationType.MessageError); }

		/// <summary>
		/// Get notifications of type NotificationType.MessageError.
		/// </summary>
		public static IEnumerable<INotification> GetMessageErrors(this INotificationProvider provider)
		{ return provider.Notifications.GetMessageErrors(); }

		#endregion
	}
}
