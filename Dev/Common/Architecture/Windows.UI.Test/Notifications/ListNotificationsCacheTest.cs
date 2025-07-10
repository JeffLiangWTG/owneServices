using System.ComponentModel;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ListNotificationsCacheTest : NotificationsCacheTestCase
	{
		public void TestGetElementNotifications()
		{
			AssertElementsNotificationCorrect();

			TestEntity entity1 = Collection.AddNew();
			entity1.PropertyWithNotifications = "2";
			TestEntity entity2 = Collection.AddNew();
			entity2.PropertyWithNotifications = "1";
			AssertElementsNotificationCorrect();

			((IBindingList)Collection).ApplySort(TestEntity.Properties.PropertyWithNotifications, ListSortDirection.Ascending);
			AssertElementsNotificationCorrect();

			Collection.Remove(entity1);
			AssertElementsNotificationCorrect();

			Collection.Remove(entity2);
			AssertElementsNotificationCorrect();

			//Collection.RefreshForCalculatedFilters();
			AssertElementsNotificationCorrect();
		}

		public void TestGetElementNotifications_BeforeListChangedGivenAChanceToInvoke()
		{
			// testing for the situation GetElementNotifications is called before OnListChanged is given a chance to invoke
			NotificationsCache.List.ListChanged -= NotificationsCache.OnListChanged;
			TestGetElementNotifications();
		}

		public void TestGetHighestPriorityNotificationTypeToRender_WhenNotificationsRenderedAlways()
		{
			TestEntity entity = Collection.AddNew();
			NotificationRenderContext.SetShouldAlwaysRenderAllNotifications(true);

			entity.EntityNotifications = "error";
			entity.PropertyWithNotifications = "warning";
			AssertEquals(NotificationType.Error, NotificationsCache.GetHighestPriorityNotificationTypeToRender());

			entity.EntityNotifications = "warning";
			entity.PropertyWithNotifications = "error";
			AssertEquals(NotificationType.Error, NotificationsCache.GetHighestPriorityNotificationTypeToRender());

			entity.EntityNotifications = "warning";
			entity.PropertyWithNotifications = "warning";
			AssertEquals(NotificationType.Warning, NotificationsCache.GetHighestPriorityNotificationTypeToRender());

			entity.EntityNotifications = "";
			entity.PropertyWithNotifications = "warning";
			AssertEquals(NotificationType.Warning, NotificationsCache.GetHighestPriorityNotificationTypeToRender());

			entity.EntityNotifications = "warning";
			entity.PropertyWithNotifications = "";
			AssertEquals(NotificationType.Warning, NotificationsCache.GetHighestPriorityNotificationTypeToRender());

			entity.EntityNotifications = "";
			entity.PropertyWithNotifications = "";
			AssertEquals(null, NotificationsCache.GetHighestPriorityNotificationTypeToRender());
		}

		public void TestGetHighestPriorityNotificationTypeToRender_OnlyWhenPropertyOrEntityFocused()
		{
			TestEntity entity = Collection.AddNew();
			NotificationRenderContext.NotifyDataControlFocused(BoundControl, entity, TestEntity.Properties.OtherPropertyWithNotifications.Name);

			entity.PropertyWithNotifications = "error";
			AssertEquals("Should not show notifications until correct property focused", null, NotificationsCache.GetHighestPriorityNotificationTypeToRender());

			entity.PropertyWithNotifications = "error";
			NotificationRenderContext.NotifyDataControlFocused(BoundControl, entity, TestEntity.Properties.PropertyWithNotifications.Name);
			AssertEquals("Should not show notifications until correct property focused", NotificationType.Error, NotificationsCache.GetHighestPriorityNotificationTypeToRender());
		}

		#region Implementation

		void AssertElementsNotificationCorrect()
		{
			for (int i = 0; i < Collection.Count; i++)
			{
				AssertEquals("Element " + i, Collection[i], NotificationsCache.GetElementNotifications(i).Element);
			}
		}

		#endregion
	}
}
