using Enterprise.Client.UPE.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class PWSHeaderRecordTest : TestCase
	{
		public void TestNotifyWarningIfRecordLengthIsLessThan55Characters()
		{
			PWSHeaderRecord record = new PWSHeaderRecord("TEST", Notifications);
			AssertEquals("There should be 1 notification reported", 1, Notifications.Events.Length);
			AssertEquals("Header Record less than 55 characters", ((WarningNotification)Notifications.LastOne).AdditionalInfo);
			AssertEquals("Should be set to true", true, record.HasErrors);
			record = new PWSHeaderRecord(new string('X', 55), Notifications);
			AssertEquals("Should be set to false", false, record.HasErrors);
		}

		NotificationBufferForTesting Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new NotificationBufferForTesting();
				}

				return fNotifications;
			}
		}

		NotificationBufferForTesting fNotifications;
	}
}
