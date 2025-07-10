using System.Linq;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CollectionNotificationProviderHelperTest : TestCaseWithDummy
	{
		public void TestNotifications()
		{
			DummyWithDependentsBusinessObject dummy1 = Collection.AddNew();
			DummyWithDependentsBusinessObject dummy2 = Collection.AddNew();

			DummyBusinessObject dummyMaster = Factory.New<DummyBusinessObject>();
			dummy1.RegisterEditableChildObject(dummyMaster);

			using (dummyMaster.SuspendValidationTesting())
			using (dummy1.SuspendValidationTesting())
			using (dummy2.SuspendValidationTesting())
			{
				DummyDependantBusinessObject dependent = dummy2.Dependents.AddNew();
				dummyMaster.Z0_VarCharMaxInfo.AddError("Error0");
				dummy1.Z0_VarCharMaxInfo.AddError("Error1");
				dummy2.Z0_VarCharMaxInfo.AddError("Error2");
				dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;
				AssertEquals("All notifications in enumerable collection", 4, ((INotificationProvider)Collection).Notifications.Count());

				bool hasMasterNotification = ((INotificationProvider)Collection).Notifications.Any(notification => notification.Message == "Error - Z0_VarCharMax: Error0");
				Assert("Has notification from Top Level object", hasMasterNotification);

				dummyMaster.IsTopLevel = true;
				AssertEquals("All notifications in enumerable collection", 3, ((INotificationProvider)Collection).Notifications.Count());

				hasMasterNotification = ((INotificationProvider)Collection).Notifications.Any(notification => notification.Message == "Error - Z0_VarCharMax: Error0");
				Assert("No notifications from Top Level object", !hasMasterNotification);
			}
		}

		public void TestHasNotifications()
		{
			DummyWithDependentsBusinessObject dummy1 = Collection.AddNew();
			DummyWithDependentsBusinessObject dummy2 = Collection.AddNew();
			DummyDependantBusinessObject dependent = dummy2.Dependents.AddNew();

			AssertEquals("HasNotifications()", false, ((INotificationProvider)Collection).HasNotifications());
			AssertEquals("HasNotifications(INotificationType)", false, ((INotificationProvider)Collection).HasNotifications(NotificationType.Error));

			DummyBusinessObject dummyMaster = Factory.New<DummyBusinessObject>();
			dummy1.RegisterEditableChildObject(dummyMaster);
			using (dummyMaster.SuspendValidationTesting())
			{
				dummyMaster.Z0_VarCharMaxInfo.AddError("Error0");
			}
			AssertEquals("HasNotifications()", true, ((INotificationProvider)Collection).HasNotifications());
			AssertEquals("HasNotifications(INotificationType)", true, ((INotificationProvider)Collection).HasNotifications(NotificationType.Error));

			dummyMaster.IsTopLevel = true;
			AssertEquals("HasNotifications()", false, ((INotificationProvider)Collection).HasNotifications());
			AssertEquals("HasNotifications(INotificationType)", false, ((INotificationProvider)Collection).HasNotifications(NotificationType.Error));

			dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;
			AssertEquals("HasNotifications()", true, ((INotificationProvider)Collection).HasNotifications());
			AssertEquals("HasNotifications(INotificationType)", true, ((INotificationProvider)Collection).HasNotifications(NotificationType.Error));
		}

		ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject> Collection
		{
			get { return collection ?? (collection = new ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject>(Factory)); }
		}
		ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject> collection;
	}
}
