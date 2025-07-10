using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace CargoWise.Services.Calendar.Testing
{
	sealed class ReminderTest : TestCase
	{
		#region UTC Date Conversion

		public void TestUTCDateConversion()
		{
			DateTime fromDate = new DateTime(2005, 11, 26, 6, 0, 0);
			DateTime toDate = new DateTime(2005, 11, 26, 8, 0, 0);

			ReminderBase reminder = new ReminderForTesting("hello", DateTimeKind.Local, fromDate, toDate, "Hello", "hello");
			AssertEquals(new DateTime(2005, 11, 26, 6, 0, 0), reminder.LocalDateFrom);
			AssertEquals(new DateTime(2005, 11, 26, 8, 0, 0), reminder.LocalDateTo);
			AssertEquals("No TimeZone - so uses Branch timezone UTC difference", Env.Time.GetUtcFromLocalTime(reminder.LocalDateFrom.ToDateTime()), reminder.UTCDateFrom);
			AssertEquals("No TimeZone - so uses Branch timezone UTC difference", Env.Time.GetUtcFromLocalTime(reminder.LocalDateTo.ToDateTime()), reminder.UTCDateTo);

			reminder = new ReminderForTesting("hello", DateTimeKind.Utc, fromDate, toDate, "Hello", "hello");
			AssertEquals(new DateTime(2005, 11, 26, 6, 0, 0), reminder.UTCDateFrom);
			AssertEquals(new DateTime(2005, 11, 26, 8, 0, 0), reminder.UTCDateTo);
			AssertEquals("No TimeZone - so uses Branch timezone UTC difference", Env.Time.GetLocalTimeFromUtc(reminder.UTCDateFrom.ToDateTime()), reminder.LocalDateFrom);
			AssertEquals("No TimeZone - so uses Branch timezone UTC difference", Env.Time.GetLocalTimeFromUtc(reminder.UTCDateTo.ToDateTime()), reminder.LocalDateTo);

			reminder = new ReminderForTesting("hello", DateTimeKind.Local, fromDate, toDate, "Hello", "hello", "", new TestTimeZone());
			AssertEquals("Non-daylight savings - so UTC offset is 5", new DateTime(2005, 11, 26, 1, 0, 0), reminder.UTCDateFrom);
			AssertEquals("Non-daylight savings - so UTC offset is 5", new DateTime(2005, 11, 26, 3, 0, 0), reminder.UTCDateTo);

			reminder = new ReminderForTesting("hello", DateTimeKind.Utc, fromDate, toDate, "Hello", "hello", "", new TestTimeZone());
			AssertEquals("Non-daylight savings - so UTC offset is 5", new DateTime(2005, 11, 26, 11, 0, 0), reminder.LocalDateFrom);
			AssertEquals("Non-daylight savings - so UTC offset is 5", new DateTime(2005, 11, 26, 13, 0, 0), reminder.LocalDateTo);

			fromDate = new DateTime(2005, 6, 26, 6, 0, 0);
			toDate = new DateTime(2005, 6, 26, 8, 0, 0);

			reminder = new ReminderForTesting("hello", DateTimeKind.Local, fromDate, toDate, "Hello", "hello", "", new TestTimeZone());
			AssertEquals("Daylight Savings - 1 hour difference", new DateTime(2005, 6, 26, 0, 0, 0), reminder.UTCDateFrom);
			AssertEquals("Daylight Savings - 1 hour difference", new DateTime(2005, 6, 26, 2, 0, 0), reminder.UTCDateTo);

			reminder = new ReminderForTesting("hello", DateTimeKind.Utc, fromDate, toDate, "Hello", "hello", "", new TestTimeZone());
			AssertEquals("Daylight Savings - 1 hour difference", new DateTime(2005, 6, 26, 12, 0, 0), reminder.LocalDateFrom);
			AssertEquals("Daylight Savings - 1 hour difference", new DateTime(2005, 6, 26, 14, 0, 0), reminder.LocalDateTo);
		}

		#endregion

		#region Test TimeZone

		class TestTimeZone : TimeZoneBase
		{
			public TestTimeZone()
				: base("TestTimeZone", 5m, 6m)
			{
			}

			protected override bool HasDaylightSaving
			{
				get { return true; }
			}

			protected override RefTimeZoneRuleInfoStartAndEndPair GetStartAndEndDstRuleInfo(int year)
			{
				RefTimeZoneRuleInfo startDstInfo = new RefTimeZoneRuleInfo(
					year, TimeZoneConstants.DstTransitionTypeStart,
					TimeZoneConstants.DstRuleDayOfMonth, TimeZoneConstants.DstTimeBaseLocal, new DateTime(year, 3, 1),
					0, "", "");

				RefTimeZoneRuleInfo endDstInfo = new RefTimeZoneRuleInfo(
					year, TimeZoneConstants.DstTransitionTypeEnd,
					TimeZoneConstants.DstRuleDayOfMonth, TimeZoneConstants.DstTimeBaseLocal, new DateTime(year, 10, 1),
					0, "", "");

				RefTimeZoneRuleInfoStartAndEndPair result = new RefTimeZoneRuleInfoStartAndEndPair(startDstInfo, endDstInfo);
				return result;
			}
		}

		#endregion

		public void TestCreateReminderWithSameFromANDToDates()
		{
			ZDateTime testDate = new DateTime(2005, 11, 26, 10, 20, 30);
			ReminderBase testReminder = new ReminderForTesting("", DateTimeKind.Local, testDate, testDate, "", "");
			AssertEquals("Test FromDate", testDate, testReminder.LocalDateFrom);
			AssertEquals("Test ToDate", testDate.AddMinutes(1), testReminder.LocalDateTo);

			testReminder = new ReminderForTesting("", DateTimeKind.Utc, testDate, testDate, "", "");
			AssertEquals("Test FromDate", testDate, testReminder.UTCDateFrom);
			AssertEquals("Test ToDate", testDate.AddMinutes(1), testReminder.UTCDateTo);

			testReminder = new ReminderForTesting("", DateTimeKind.Local, testDate, testDate.AddMinutes(2), "", "");
			AssertEquals("Test FromDate", testDate, testReminder.LocalDateFrom);
			AssertEquals("Test ToDate", testDate.AddMinutes(2), testReminder.LocalDateTo);

			testReminder = new ReminderForTesting("", DateTimeKind.Utc, testDate, testDate.AddMinutes(2), "", "");
			AssertEquals("Test FromDate", testDate, testReminder.UTCDateFrom);
			AssertEquals("Test ToDate", testDate.AddMinutes(2), testReminder.UTCDateTo);
		}

		public void TestToDateAsDuration()
		{
			ReminderBase testReminder = new ReminderForTesting("", DateTimeKind.Local, new DateTime(2005, 11, 26, 10, 0, 0), new TimeSpan(3, 11, 0), "", "");
			AssertEquals(new DateTime(2005, 11, 26, 13, 11, 0), testReminder.LocalDateTo);

			testReminder = new ReminderForTesting("", DateTimeKind.Utc, new DateTime(2005, 11, 26, 10, 0, 0), new TimeSpan(3, 11, 0), "", "");
			AssertEquals(new DateTime(2005, 11, 26, 13, 11, 0), testReminder.UTCDateTo);
		}

		public void TestRecipientsCollection()
		{
			ReminderBase reminder = new ReminderForTesting("", DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "Test Subject", "Some Body");
			AssertEquals("No recipients", 0, reminder.Recipients.Count);

			reminder.Recipients.Add("Some Name", "");
			AssertEquals("No recipients - as email was blank", 0, reminder.Recipients.Count);

			reminder.Recipients.Add("Some One Else", "test@example.com");
			AssertEquals("Recipient Added", 1, reminder.Recipients.Count);

			reminder.Recipients.Add("", "test2@example.com");
			AssertEquals("Recipient Added - name is irrelevant", 2, reminder.Recipients.Count);
		}

		public void TestHasHtmlBody()
		{
			ReminderBase reminder = new ReminderForTesting("Test", DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "Test Subject", "Some Body");
			AssertEquals(false, reminder.HasHtmlBody);

			reminder = new ReminderForTesting("Test", DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "Test Subject", "Some Body", "<body>Some Body</body>");
			AssertEquals(true, reminder.HasHtmlBody);
		}

		[Data.Testing.UseSnapshotProtection]
		public void TestDeliveringReminderGetsASequenceNumber()
		{
			var sequence = CreateReminderAndGetItsSequenceNumberFromVCalendarExport();
			AssertEquals(123u, sequence);
		}

		public void TestWithMidnightReminder()
		{
			var fromDate = new DateTime(2016, 1, 1, 0, 0, 0);
			var toDate = new DateTime(2016, 1, 1, 0, 0, 0);
			var reminder = new ReminderForTesting("hello1", DateTimeKind.Local, fromDate, toDate, "Hello", "hello", "", null);
			AssertEquals(fromDate.AddHours(9), reminder.LocalDateFrom);
			AssertEquals(toDate.AddHours(9).AddMinutes(1), reminder.LocalDateTo);

			fromDate = new DateTime(2016, 1, 1, 0, 0, 0);
			toDate = new DateTime(2016, 1, 1, 0, 30, 0);
			var reminder2 = new ReminderForTesting("hello2", DateTimeKind.Local, fromDate, toDate, "Hello", "hello", "", null);
			AssertEquals(fromDate, reminder2.LocalDateFrom);
			AssertEquals(toDate, reminder2.LocalDateTo);
		}

		uint CreateReminderAndGetItsSequenceNumberFromVCalendarExport()
		{
			var reminderText = string.Empty;

			var reminder = new ReminderForTesting("test");
			reminder.Recipients.Add(new ReminderRecipient("me", "me@you.com"));
			reminder.CreateAppointment();

			return reminder.Sequence;
		}
	}
}
