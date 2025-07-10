using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(EventRecord))]
	class EventRecordTest : NonPersistentBusinessObjectTestCase
	{
		readonly StringInterner interner = new StringInterner();

		public void TestFormattedMessage()
		{
			//all logging from now on should use milliseconds, but old logs will lack them
			var line = @"2008-05-03 00:00:09      Information              Test1→Test2↵Test3";
			var eventRecord = EventRecord.FromString(interner, line);
			AssertEquals("Test1\tTest2\nTest3", eventRecord.FormattedMessage);
			line = @"2008-05-03 00:00:09.123  Information              Test1→Test2↵Test3";
			eventRecord = EventRecord.FromString(interner, line);
			AssertEquals("Test1\tTest2\nTest3", eventRecord.FormattedMessage);
		}

		public void TestFormattedMessageContainingBackSlashTInFilePath()
		{
			var line = @"2023-04-28 00:00:00      Information              C:\\test→file\\";
			var eventRecord = EventRecord.FromString(interner, line);
			AssertEquals(@"C:\test	file\", eventRecord.FormattedMessage);
		}

		public void TestFormattedMessageContainingBackSlashNInFilePath()
		{
			var line = @"2023-04-28 00:00:00      Information              C:\\name";
			var eventRecord = EventRecord.FromString(interner, line);
			AssertEquals(@"C:\name", eventRecord.FormattedMessage);
		}

		public void TestFormattedMessageContainingNewLineArrow()
		{
			var line = @"2023-04-28 00:00:00      Information              hello↵world";
			var eventRecord = EventRecord.FromString(interner, line);
			AssertEquals("hello\nworld", eventRecord.FormattedMessage);
		}

		public void TestFormattedMessageContainingTabArrow()
		{
			var line = @"2023-04-28 00:00:00      Information              hello→world";
			var eventRecord = EventRecord.FromString(interner, line);
			AssertEquals(@"hello	world", eventRecord.FormattedMessage);
		}

		public void TestFormattedMessageContainingBackSlashNewLineArrow()
		{
			var line = @"2023-04-28 00:00:00      Information              hello\↵world";
			var eventRecord = EventRecord.FromString(interner, line);
			AssertEquals("hello↵world", eventRecord.FormattedMessage);
		}

		public void TestFormattedMessageContainingBackSlashTabArrow()
		{
			var line = @"2023-04-28 00:00:00      Information              hello\→world";
			var eventRecord = EventRecord.FromString(interner, line);
			AssertEquals("hello→world", eventRecord.FormattedMessage);
		}

		public void TestFromString()
		{
			//all logging from now on should use milliseconds, but old logs will lack them
			var line = "2008-05-03 00:00:09      Information              Check database is regularly backed up";
			var eventRecord = EventRecord.FromString(interner, line);
			AssertEquals(new ZDateTime(2008, 5, 3, 0, 0, 9), eventRecord.DateTime);
			AssertEquals("Information", eventRecord.Type);
			AssertEquals("Check database is regularly backed up", eventRecord.Message);
			AssertEquals(0, eventRecord.ProcessId);

			line = "2008-05-03 00:00:09.001  Information              Check database is regularly backed up";
			eventRecord = EventRecord.FromString(interner, line);
			AssertEquals(new ZDateTime(2008, 5, 3, 0, 0, 9, 1), eventRecord.DateTime);
			AssertEquals("Information", eventRecord.Type);
			AssertEquals("Check database is regularly backed up", eventRecord.Message);
			AssertEquals(0, eventRecord.ProcessId);

			line = "2008-##-03 00:00:09      Information              Check database is regularly backed up";
			eventRecord = EventRecord.FromString(interner, line);
			Assert(!eventRecord.DateTime.IsValid);
			AssertEquals("Information", eventRecord.Type);
			AssertEquals("Check database is regularly backed up", eventRecord.Message);
			AssertEquals(0, eventRecord.ProcessId);

			line = "2008-##-03 00:00:09.001  Information              Check database is regularly backed up";
			eventRecord = EventRecord.FromString(interner, line);
			Assert(!eventRecord.DateTime.IsValid);
			AssertEquals("Information", eventRecord.Type);
			AssertEquals("Check database is regularly backed up", eventRecord.Message);
			AssertEquals(0, eventRecord.ProcessId);

			line = "2008-05-03 00:00:09      Information              33204                    Check database is regularly backed up";
			eventRecord = EventRecord.FromString(interner, line);
			AssertEquals(new ZDateTime(2008, 5, 3, 0, 0, 9), eventRecord.DateTime);
			AssertEquals("Information", eventRecord.Type);
			AssertEquals("Check database is regularly backed up", eventRecord.Message);
			AssertEquals(33204, eventRecord.ProcessId);

			line = "2008-05-03 00:00:09      Information              33204                    Check database is regularly backed up";
			eventRecord = EventRecord.FromString(interner, line);
			AssertEquals(new ZDateTime(2008, 5, 3, 0, 0, 9), eventRecord.DateTime);
			AssertEquals("Information", eventRecord.Type);
			AssertEquals("Check database is regularly backed up", eventRecord.Message);
			AssertEquals(33204, eventRecord.ProcessId);

			line = "2008-05-03 00:00:09      Information              1                        Check database is regularly backed up";
			eventRecord = EventRecord.FromString(interner, line);
			AssertEquals("Check database is regularly backed up", eventRecord.Message);
			AssertEquals(1, eventRecord.ProcessId);

			line = "2008-05-03 00:00:09      Information              0                        Check database is regularly backed up";
			eventRecord = EventRecord.FromString(interner, line);
			AssertEquals("Check database is regularly backed up", eventRecord.Message);
			AssertEquals(0, eventRecord.ProcessId);
		}

		[TestUtcOffset(2, 0, 0)]
		public void TestDateTimeString()
		{
			var eventRecord = new EventRecord(new ZDateTime(2008, 6, 5, 11, 36, 46, 123), nameof(LogType.Information), "Test!", 10, null, 0);
			AssertEquals("2008-06-05 11:36:46.123", eventRecord.DateTimeUtcString);
			AssertEquals("2008-06-05 13:36:46.123", eventRecord.DateTimeString);

			TestUtcOffsetAttribute.Time = TimeSpan.FromHours(-4);
			AssertEquals("2008-06-05 11:36:46.123", eventRecord.DateTimeUtcString);
			AssertEquals("2008-06-05 07:36:46.123", eventRecord.DateTimeString);

			TestUtcOffsetAttribute.Time = TimeSpan.FromHours(0);
			AssertEquals("2008-06-05 11:36:46.123", eventRecord.DateTimeUtcString);
			AssertEquals(eventRecord.DateTimeUtcString, eventRecord.DateTimeString);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewEventRecord();
		}

		public static EventRecord GetNewEventRecord()
		{
			var r = new Random(dummy++);
			var dateTime = ZDateTime.Today.AddSeconds(r.Next(86400));
			var logTypes = Enum.GetNames(typeof(LogType));
			ZString type = logTypes[r.Next(logTypes.Length)];
			ZString message = string.Format("Message {0}", dummy);

			return new EventRecord(dateTime, type, message, 0, null, 0);
		}

		static int dummy;

		#endregion
	}
}
