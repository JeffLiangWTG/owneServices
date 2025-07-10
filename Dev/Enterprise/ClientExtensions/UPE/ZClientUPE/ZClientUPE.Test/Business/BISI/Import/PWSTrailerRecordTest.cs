using Enterprise.Client.UPE.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class PWSTrailerRecordTest : TestCase
	{
		public void TestNotifyWarningIfRecordLengthIsLessThan20Characters()
		{
			PWSTrailerRecord record = new PWSTrailerRecord("TEST", Notifications);
			AssertEquals("There should be 1 notification reported", 1, Notifications.Events.Length);
			AssertEquals("Trailer Record less than 20 characters", ((WarningNotification)Notifications.LastOne).AdditionalInfo);
			AssertEquals("Should be set to true", true, record.HasErrors);
			record = new PWSTrailerRecord(new string('X', 20), Notifications);
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
