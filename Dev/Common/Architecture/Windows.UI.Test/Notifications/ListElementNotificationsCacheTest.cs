using System;
using System.Runtime.CompilerServices;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ListElementNotificationsCacheTest : NotificationsCacheTestCase
	{
		public void TestGarbageCollected()
		{
			Entity.EntityNotifications = "warning";
			Entity.PropertyWithNotifications = "warning";

			var elementNotificationsRef = CreateUnreferencedCache();
			GC.Collect();

			AssertEquals("Should be collected even while the entity and DNotificationsCache are alive", false, elementNotificationsRef.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference CreateUnreferencedCache()
		{
			ListElementNotificationsCache element_notifications = new ListElementNotificationsCache(NotificationsCache, Entity);
			GC.KeepAlive(element_notifications.ElementNotifications);
			GC.KeepAlive(element_notifications.ElementPropertyNotifications);
			GC.KeepAlive(element_notifications.GetCombinedNotificationsToRender());

			return new WeakReference(element_notifications);
		}

		#region GetCombinedNotificationsToRender

		public void TestGetCombinedNotificationsToRender_WhenNotificationsRenderedAlways()
		{
			NotificationRenderContext.SetShouldAlwaysRenderAllNotifications(true);

			Entity.EntityNotifications = "error";
			Entity.PropertyWithNotifications = "warning";
			AssertEquals(NotificationType.Error, ElementNotificationsCache.GetCombinedNotificationsToRender().GetHighestSeverityNotificationType());

			Entity.EntityNotifications = "warning";
			Entity.PropertyWithNotifications = "error";
			AssertEquals(NotificationType.Error, ElementNotificationsCache.GetCombinedNotificationsToRender().GetHighestSeverityNotificationType());

			Entity.EntityNotifications = "warning";
			Entity.PropertyWithNotifications = "warning";
			AssertEquals(NotificationType.Warning, ElementNotificationsCache.GetCombinedNotificationsToRender().GetHighestSeverityNotificationType());

			Entity.EntityNotifications = "";
			Entity.PropertyWithNotifications = "warning";
			AssertEquals(NotificationType.Warning, ElementNotificationsCache.GetCombinedNotificationsToRender().GetHighestSeverityNotificationType());

			Entity.EntityNotifications = "warning";
			Entity.PropertyWithNotifications = "";
			AssertEquals(NotificationType.Warning, ElementNotificationsCache.GetCombinedNotificationsToRender().GetHighestSeverityNotificationType());

			Entity.EntityNotifications = "";
			Entity.PropertyWithNotifications = "";
			AssertEquals(null, ElementNotificationsCache.GetCombinedNotificationsToRender());
		}

		public void TestGetCombinedNotificationsToRender_OnlyWhenPropertyOrEntityFocused()
		{
			NotificationRenderContext.NotifyDataControlFocused(BoundControl, Entity, TestEntity.Properties.OtherPropertyWithNotifications.Name);

			Entity.PropertyWithNotifications = "error";
			AssertEquals("Should not show notifications until correct property focused", null, ElementNotificationsCache.GetCombinedNotificationsToRender());

			Entity.PropertyWithNotifications = "error";
			NotificationRenderContext.NotifyDataControlFocused(BoundControl, Entity, TestEntity.Properties.PropertyWithNotifications.Name);
			AssertEquals("Should not show notifications until correct property focused", NotificationType.Error, ElementNotificationsCache.GetCombinedNotificationsToRender().GetHighestSeverityNotificationType());
		}

		#endregion

		#region ElementNotifications

		public void TestElementNotifications()
		{
			AssertEquals(null, ElementNotificationsCache.ElementNotifications);
			Entity.EntityNotifications = "error";
			AssertEquals(true, ElementNotificationsCache.ElementNotifications.HasErrors());
		}

		public void TestElementNotifications_IncludeOtherPropertiesInElementNotifications()
		{
			ElementNotificationsCache.ListNotifications.IncludeOtherPropertiesInElementNotifications = true;

			Entity.PropertyWithNotifications = "error";
			AssertEquals("Has property error", true, ElementNotificationsCache.ElementPropertyNotifications[0].HasErrors());
			AssertEquals("No element error", null, ElementNotificationsCache.ElementNotifications);

			ElementNotificationsCache.ListNotifications.IncludeOtherPropertiesInElementNotifications = true;
			Entity.OtherPropertyWithNotifications = "error";
			AssertEquals(
				"Has element error for property not included in ListNotificationsCache.Properties",
				true, ElementNotificationsCache.ElementNotifications.HasErrors());

			ElementNotificationsCache.ListNotifications.IncludeOtherPropertiesInElementNotifications = false;
			AssertEquals(
				"No error on element notifications after IncludeOtherPropertiesInElementNotifications=false",
				null, ElementNotificationsCache.ElementNotifications);
		}

		#endregion

		#region ElementPropertyNotifications

		public void TestElementPropertyNotifications()
		{
			AssertEquals(false, ElementNotificationsCache.ElementPropertyNotifications[0].HasErrors());
			Entity.PropertyWithNotifications = "error";
			AssertEquals(true, ElementNotificationsCache.ElementPropertyNotifications[0].HasErrors());
		}

		#endregion

		#region CombinedNotifications

		public void TestCombinedNotifications()
		{
			Entity.EntityNotifications = "warning";
			Entity.PropertyWithNotifications = "error";
			AssertEquals(2, ElementNotificationsCache.CombinedNotifications.Count);
			AssertEquals("Entity notifications", "warning", ElementNotificationsCache.CombinedNotifications[0].Message);
			AssertEquals("Property notifications", "error", ElementNotificationsCache.CombinedNotifications[1].Message);
		}

		#endregion

		#region Implementation

		ListElementNotificationsCache ElementNotificationsCache
		{
			get
			{
				if (Collection.Count == 0)
				{
					Collection.AddNew();
				}
				return NotificationsCache.GetElementNotifications(0);
			}
		}

		#endregion
	}
}
