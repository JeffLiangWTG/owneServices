#if DEBUG
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class NotificationCollectionTests : TestCase
	{
		public void TestConstruction()
		{
			INotification[] notifications = new INotification[2]
				{
					new Notification(NotificationType.Error, "first"),
					new Notification(NotificationType.Error, "second")
				};
			NotificationCollection collection = new NotificationCollection(notifications);

			AssertEquals("Count", 2, collection.Count);
			AssertEquals("First item", "first", collection[0].Message);
			AssertEquals("Second item", "second", collection[1].Message);
		}

		public void TestDontOverrideToString()
		{
			MethodInfo method = typeof(NotificationCollection).GetMethod("ToString");
			AssertEquals(
				"ToString() not overridden on NotificationCollection because IEnumerable<INotification> " +
				"may be yielded from a different location, in which case the outcome of ToString() may not " +
				"be a NotificationCollection object which can produce unpredictable results.", typeof(object), method.DeclaringType);
		}

		public void TestClone()
		{
			Notifications.AddError("splaty");

			NotificationCollection clonedCollection = Notifications.Clone();
			AssertEquals(1, clonedCollection.Count);
			AssertEquals("splaty", clonedCollection[0].Message);
		}

		public void TestListChanged()
		{
			bool changed = false;
			Notifications.ListChanged += delegate
			{ changed = true; };
			AssertEquals("Not changed initially", false, changed);

			Notifications.AddError("Error");
			AssertEquals("Changed when a notification has been added", true, changed);
			changed = false;

			Notifications.RemoveAt(0);
			AssertEquals("Changed when a notification has been removed", true, changed);
			Notifications.AddError("Error");
			changed = false;

			Notifications.Remove(Notifications[0]);
			AssertEquals("Changed when a notification has been removed", true, changed);
			Notifications.AddError("Error");
			changed = false;

			Notifications[0] = new Notification(NotificationType.Warning, "Warning");
			AssertEquals("Changed when a notification has been set", true, changed);
			changed = false;
		}

		public void TestHasNotifications()
		{
			AssertEquals("No error", false, Notifications.HasNotifications(NotificationType.Error));
			AssertEquals("No warning", false, Notifications.HasNotifications(NotificationType.Warning));

			Notifications.AddError("Error");
			AssertEquals("Has error", true, Notifications.HasNotifications(NotificationType.Error));
			AssertEquals("No warning", false, Notifications.HasNotifications(NotificationType.Warning));

			Notifications.AddWarning("Warning");
			AssertEquals("Has error", true, Notifications.HasNotifications(NotificationType.Error));
			AssertEquals("Has warning", true, Notifications.HasNotifications(NotificationType.Warning));

			Notifications.RemoveAt(0);
			AssertEquals("No error", false, Notifications.HasNotifications(NotificationType.Error));
			AssertEquals("Has warning", true, Notifications.HasNotifications(NotificationType.Warning));

			Notifications.Clear();
			AssertEquals("No error", false, Notifications.HasNotifications(NotificationType.Error));
			AssertEquals("No warning", false, Notifications.HasNotifications(NotificationType.Warning));
		}

		public void TestRemoveContaining()
		{
			Notifications.AddError("Errors should never pass silently");

			Notifications.Remove(NotificationType.Error, "never pass silently");
			Assert(Notifications.HasErrors());

			Notifications.Remove(NotificationType.Error, "Now is better than never", true);
			Assert(Notifications.HasErrors());

			Notifications.Remove(NotificationType.Error, "never pass silently", true);
			Assert(!Notifications.HasErrors());
		}

		public void TestRemoveWhenMultipleMatches()
		{
			var message = "This is an error that happens multiple times.";
			Enumerable.Range(0, 5).ForEach(x => Notifications.AddError(message));
			Notifications.Remove(NotificationType.Error, message);
			Assert(!Notifications.HasErrors());
		}

		public void TestGetHighestSeverityNotificationType()
		{
			NotificationCollection notifications = new NotificationCollection();
			notifications.AddWarning("warning");
			notifications.AddError("error");
			AssertEquals(NotificationType.Error, notifications.GetHighestSeverityNotificationType());
		}

		public void TestEmptyIsSingleton()
		{
			Assert(ReferenceEquals(NotificationCollection.Empty, NotificationCollection.Empty));
		}

		#region Implementation

		readonly NotificationCollection Notifications = new NotificationCollection();

		#endregion
	}
}
#endif
