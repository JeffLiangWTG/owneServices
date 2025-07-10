using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal abstract class JXCRecordTestCase : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			JXCRecord record = GetNewRecord("ABCD", "EFGH");
			AssertEquals("ABCD", record.LineType);
			AssertEquals("EFGH", record.LineContent);
		}

		protected void AssertContainAttachingExistingShipmentNotification(NotificationBuffer notificationBuffer, JASForwardingConsol consol, JASForwardingShipment shipment)
		{
			string expectedMessage = "Attaching " + shipment.HumanReadableName + " to " + consol.HumanReadableName;
			AssertContainNotification("Should contain AttachingExistingShipment notification", notificationBuffer, NotificationSubscriberType.Info, expectedMessage);
		}

		protected void AssertContainAttachingNewShipmentNotification(NotificationBuffer notificationBuffer, JASForwardingConsol consol)
		{
			string expectedMessage = "Attaching new shipment to " + consol.HumanReadableName;
			AssertContainNotification("Should contain AttachingNewShipment notification", notificationBuffer, NotificationSubscriberType.Info, expectedMessage);
		}

		void AssertContainNotification(string failureMessage, NotificationBuffer notificationBuffer, NotificationSubscriberType notificationType, string expectedMessage)
		{
			INotification[] notifications = notificationBuffer.GetEventsByType(notificationType);
			foreach (INotification notification in notifications)
			{
				INotificationSubscriberNotification subscriberNotification = notification as INotificationSubscriberNotification;
				if (subscriberNotification != null && subscriberNotification.AdditionalInfo == expectedMessage)
				{
					Assert(true);
					return;
				}
			}

			Fail(failureMessage);
		}

		protected JXCRecordFactory RecordFactory
		{
			get
			{
				if (fRecordFactory == null)
				{
					fRecordFactory = new JXCRecordFactory();
				}

				return fRecordFactory;
			}
		}

		protected abstract JXCRecord GetNewRecord(ZString lineType, ZString lineContent);
		JXCRecordFactory fRecordFactory;
	}
}
