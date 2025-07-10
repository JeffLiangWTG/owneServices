using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(BusinessObjectNotificationsViewerLine))]
	public sealed class BusinessObjectNotificationsViewerLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var notificationLine = (BusinessObjectNotificationsViewerLine)GetNewBusinessObject();
			notificationLine.HumanReadableColumn1 = "~Column1~";
			notificationLine.HumanReadableColumn2 = "~Column2~";
			notificationLine.NotificationType = "~Type~";
			notificationLine.NotificationMessage = "~Message~";
			AssertEquals("HumanReadableColumn1", "~Column1~", notificationLine.HumanReadableColumn1);
			AssertEquals("HumanReadableColumn2", "~Column2~", notificationLine.HumanReadableColumn2);
			AssertEquals("NotificationType", "~Type~", notificationLine.NotificationType);
			AssertEquals("NotificationMessage", "~Message~", notificationLine.NotificationMessage);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			return new BusinessObjectNotificationsViewerLine(dummyBizObj);
		}
	}
}
