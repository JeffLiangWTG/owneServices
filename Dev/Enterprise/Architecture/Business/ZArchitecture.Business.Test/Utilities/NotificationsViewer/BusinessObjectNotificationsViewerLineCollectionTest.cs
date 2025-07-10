using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(BusinessObjectNotificationsViewerLineCollection))]
	public sealed class BusinessObjectNotificationsViewerLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BusinessObjectNotificationsViewerLineCollection>
	{
		public void TestLoadNotificationsFromBizObj()
		{
			var provider = new BusinessObjectCollectionNotificationsViewerProvider(Factory);
			var supporter = new BusinessObjectNotificationsViewerSupporter(provider);
			var dummyBizObj1 = Factory.New<DummyBusinessObject>();
			dummyBizObj1.Z0_Code = "Code1";
			var dummyBizObj2 = Factory.New<DummyBusinessObject>();
			dummyBizObj2.Z0_Code = "Code2";
			var dummyBizObj3 = Factory.New<DummyBusinessObject>();
			dummyBizObj3.Z0_Code = "Code3";
			var dummyBizObj4 = Factory.New<DummyBusinessObject>();
			dummyBizObj4.Z0_Code = "Code4";

			supporter.BusinessObjectCollection.Add(dummyBizObj1);
			supporter.BusinessObjectCollection.Add(dummyBizObj2);
			supporter.BusinessObjectCollection.Add(dummyBizObj3);
			supporter.BusinessObjectCollection.Add(dummyBizObj4);

			using (dummyBizObj1.SuspendValidationTesting())
			using (dummyBizObj2.SuspendValidationTesting())
			using (dummyBizObj3.SuspendValidationTesting())
			using (dummyBizObj4.SuspendValidationTesting())
			{
				dummyBizObj1.Z0_CodeInfo.AddWarning("Test Warning");
				dummyBizObj2.Z0_CodeInfo.AddMessageError("Test Message Error");
				dummyBizObj3.Z0_CodeInfo.AddError("Test Error");
			}

			var notifications = supporter.NotificationsCollection;
			AssertEquals(3, notifications.Count);
			AssertNotification("Notification Line 1", "Code1", "Warning", "Z0_Code: Test Warning", notifications[0]);
			AssertNotification("Notification Line 2", "Code2", "Message Error", "Z0_Code: Test Message Error", notifications[1]);
			AssertNotification("Notification Line 3", "Code3", "Error", "Z0_Code: Test Error", notifications[2]);
		}

		void AssertNotification(ZString message, ZString humanReadableColumn1, ZString notificationType, ZString notificationMessage, BusinessObjectNotificationsViewerLine notificationLine)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("HumanReadableColumn1", humanReadableColumn1, notificationLine.HumanReadableColumn1);
				AssertEquals("HumanReadableColumn2", ZString.Empty, notificationLine.HumanReadableColumn2);
				AssertEquals("NotificationType", notificationType, notificationLine.NotificationType);
				AssertEquals("NotificationMessage", notificationMessage, notificationLine.NotificationMessage);
			});
		}

		protected override BusinessObjectNotificationsViewerLineCollection GetCollectionToTest()
		{
			var provider = new BusinessObjectCollectionNotificationsViewerProvider(Factory);
			var supporter = new BusinessObjectNotificationsViewerSupporter(provider);
			return supporter.NotificationsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			dummyBizObj.Z0_Code = "Code";
			return new BusinessObjectNotificationsViewerLine(dummyBizObj);
		}
	}
}
