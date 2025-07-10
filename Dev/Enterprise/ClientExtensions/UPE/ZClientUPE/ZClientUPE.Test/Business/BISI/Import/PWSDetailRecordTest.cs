using Enterprise.Client.UPE.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class PWSDetailRecordTest : TestCase
	{
		public void TestNotifyWarningIfRecordLengthIsLessThan43Characters()
		{
			PWSDetailRecord record = new PWSDetailRecord("TEST", Notifications);
			AssertEquals("There should be 1 notification reported", 1, Notifications.Events.Length);
			AssertEquals("Detail Record less than 43 characters", ((WarningNotification)Notifications.LastOne).AdditionalInfo);
			AssertEquals("Should be set to true", true, record.HasErrors);
			record = new PWSDetailRecord(new string('X', 43), Notifications);
			AssertEquals("Should be set to false", false, record.HasErrors);
		}

		public void TestRecordCode()
		{
			PWSDetailRecord record = new PWSDetailRecord("TEST", Notifications);
			AssertEquals(false, record.HasCorrectRecordCode);
			record = new PWSDetailRecord("020Something", Notifications);
			AssertEquals(true, record.HasCorrectRecordCode);
		}

		public void TestPackageNumber()
		{
			AssertEquals("Empty space should be trimmed", "W6625665589", Record.PackageNumber);
		}

		public void TestDataRecordSequenceNumber()
		{
			AssertEquals(27, Record.DataRecordSequenceNumber);
		}

		public void TestIsLastRecord()
		{
			AssertEquals(true, Record.IsLastRecord);
		}

		public void TestData()
		{
			AssertEquals("        6/F PEAKSON BLDG. 1505 PRENCETON   ST.,CORNER SHAW                    MANDALUYONG                 1552      PHPH", Record.Data);
			PWSDetailRecord newRecord = new PWSDetailRecord("020W6625665589                        0027123", Notifications);
			AssertEquals("Should be padded to 120 characters if length is less than 120", "23                                                                                                                      ", newRecord.Data);
		}

		public void TestCanConstructTotalDataArea()
		{
			PWSDetailRecord record = new PWSDetailRecord("", Notifications);
			Assert("HasErrors, should not be able to construct Total Data Area", !record.CanConstructTotalDataArea(0, ""));
			record = new PWSDetailRecord(RecordDataForTest, Notifications);
			Assert("Should be valid", record.CanConstructTotalDataArea(27, "W6625665589"));
			Assert("Package number not as expected. Should not be valid", !record.CanConstructTotalDataArea(27, "W662566558"));
			Assert("Sequence number not as expected. Should not be valid", !record.CanConstructTotalDataArea(26, "W6625665589"));
			Assert("Package number not as expected. Should not be valid", !record.CanConstructTotalDataArea(26, ""));
			record = new PWSDetailRecord(RecordDataForTest2, Notifications);
			Assert("New sequence. Should be valid", record.CanConstructTotalDataArea(1, ""));
		}

		PWSDetailRecord Record
		{
			get
			{
				if (fRecord == null)
				{
					fRecord = new PWSDetailRecord(RecordDataForTest, Notifications);
				}

				return fRecord;
			}
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

		PWSDetailRecord fRecord;
		NotificationBufferForTesting fNotifications;
		const string RecordDataForTest = "020W6625665589                        0027D        6/F PEAKSON BLDG. 1505 PRENCETON   ST.,CORNER SHAW                    MANDALUYONG                 1552      PHPH       ";
		const string RecordDataForTest2 = "020W6625665589                        0001D        6/F PEAKSON BLDG. 1505 PRENCETON   ST.,CORNER SHAW                    MANDALUYONG                 1552      PHPH       ";
	}
}
