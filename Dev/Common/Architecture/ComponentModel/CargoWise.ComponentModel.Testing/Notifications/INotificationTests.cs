#if DEBUG
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class INotificationTests : TestCase
	{
		#region HasNotifications / HasWarnings / HasErrors

		public void TestHasNotifications()
		{
			List<INotification> notifications = new List<INotification>();
			AssertEquals("No notifications", false, notifications.HasNotifications());
			notifications.Add(new Notification(NotificationType.Warning, "Message"));
			AssertEquals("Has notifications", true, notifications.HasNotifications());
		}

		public void TestHasNotifications_OfSpecificType()
		{
			List<INotification> notifications = new List<INotification>();

			notifications.Add(new Notification(NotificationType.Warning, "Message"));
			AssertEquals("No errors", false, notifications.HasNotifications(NotificationType.Error));
			AssertEquals("Has warnings", true, notifications.HasNotifications(NotificationType.Warning));

			notifications.Add(new Notification(NotificationType.Error, "Message"));
			AssertEquals("Has errors", true, notifications.HasNotifications(NotificationType.Error));
			AssertEquals("Has warnings", true, notifications.HasNotifications(NotificationType.Warning));
		}

		public void TestHasWarnings()
		{
			List<INotification> notifications = new List<INotification>();
			notifications.Add(new Notification(NotificationType.Error, "Message"));
			AssertEquals("No warnings", false, notifications.HasWarnings());
			notifications.Add(new Notification(NotificationType.Warning, "Message"));
			AssertEquals("Has warnings", true, notifications.HasWarnings());
		}

		public void TestHasErrors()
		{
			List<INotification> notifications = new List<INotification>();
			notifications.Add(new Notification(NotificationType.Warning, "Message"));
			AssertEquals("No errors", false, notifications.HasErrors());
			notifications.Add(new Notification(NotificationType.Error, "Message"));
			AssertEquals("Has errors", true, notifications.HasErrors());
		}

		#endregion

		#region ToMessageListString / ToUniqueMessageListString

		public void TestGetMessagesAsString()
		{
			INotification[] notifications = new INotification[]
			{
					new Notification(NotificationType.Error, "first"),
					new Notification(NotificationType.Warning, "second"),
					new Notification(NotificationType.Error, "third")
			};
			NotificationCollection collection = new NotificationCollection(notifications);
			string notificationsAsString = collection.ToMessageListString();
			AssertEquals("first\nsecond\nthird", notificationsAsString);
		}

		public void TestGetUniqueMessageListAsString()
		{
			INotification[] notifications = new INotification[]
			{
					new Notification(NotificationType.Error, "first"),
					new Notification(NotificationType.Error, "first"),
					new Notification(NotificationType.Warning, "second"),
					new Notification(NotificationType.Warning, "second"),
					new Notification(NotificationType.Error, "second"),
			};
			NotificationCollection collection = new NotificationCollection(notifications);
			AssertEquals("first\nsecond", collection.ToUniqueMessageListString());
		}

		#endregion

		#region Count / GetFirst

		public void TestCount()
		{
			List<INotification> list = new List<INotification>();
			list.Add(new Notification(NotificationType.Warning, "Message"));
			list.Add(new Notification(NotificationType.Error, "Message"));
			AssertEquals(2, list.Count());
		}

		public void TestGetFirst()
		{
			List<INotification> list = new List<INotification>();
			list.Add(new Notification(NotificationType.Warning, "Warning"));
			list.Add(new Notification(NotificationType.Error, "Error"));
			AssertEquals("Warning", list.GetFirstMessage());
		}

		#endregion

		#region GetHighestSeverityNotification / GetHighestSeverityNotificationType / IsFatal

		public void TestGetHighestSeverityNotification()
		{
			List<INotification> notifications = new List<INotification>();
			notifications.Add(new Notification(NotificationType.Warning, "warning"));
			notifications.Add(new Notification(NotificationType.Error, "error"));
			AssertEquals("error", notifications.GetHighestSeverityNotification().Message);
		}

		public void TestGetHighestSeverityNotificationType()
		{
			List<INotification> notifications = new List<INotification>();
			notifications.Add(new Notification(NotificationType.Warning, "warning"));
			notifications.Add(new Notification(NotificationType.Error, "error"));
			AssertEquals(NotificationType.Error, notifications.GetHighestSeverityNotificationType());
		}

		public void TestIsFatal()
		{
			List<INotification> notifications = new List<INotification>();
			notifications.Add(new Notification(NotificationType.Warning, "warning"));
			AssertEquals(false, notifications.IsFatal());
			notifications.Add(new Notification(NotificationType.Error, "error"));
			AssertEquals(true, notifications.IsFatal());
		}

		#endregion

		#region HasSameNotificationsIgnoringOrder

		public void TestHasSameNotificationsIgnoringOrder()
		{
			NotificationCollection notifications = new NotificationCollection();
			NotificationCollection notifications2 = new NotificationCollection();

			notifications.AddWarning("Warning");
			notifications.AddError("Error");
			notifications2.AddError("Error");
			notifications2.AddWarning("Warning");

			AssertEquals(true, notifications.HasSameNotificationsIgnoringOrder(notifications2));
			notifications2.AddWarning("Warning2");
			AssertEquals(false, notifications.HasSameNotificationsIgnoringOrder(notifications2));
		}

		#endregion
	}
}
#endif
