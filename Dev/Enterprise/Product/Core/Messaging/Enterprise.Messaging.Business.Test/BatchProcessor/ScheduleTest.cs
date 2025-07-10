using System;
using System.Collections.Specialized;

namespace Enterprise.BatchProcessor
{
	sealed class ScheduleTest : NUnit.Framework.TestCase
	{
		public void TestScheduleStringExtraCharactersAreTruncated()
		{
			ScheduleForTest testSchedule = new ScheduleForTest();
			testSchedule.Setup("*".PadRight(60) + "*");
			StringCollection scheduleList = testSchedule.ScheduleList;
			AssertEquals("Only 1 entry in the schedule", 1, scheduleList.Count);
			AssertEquals("Sunday 00:00", nameof(DayOfWeek.Sunday) + " - From 00:00 To 00:30", scheduleList[0]);
		}

		public void TestSetupSchedule()
		{
			ScheduleForTest testSchedule = new ScheduleForTest();
			testSchedule.Setup("   *  ** ,,, *    *                                       , * *  ");
			StringCollection scheduleList = testSchedule.ScheduleList;

			AssertEquals("Sun 1st", nameof(DayOfWeek.Sunday) + " - From 01:30 To 02:00", scheduleList[0]);
			AssertEquals("Sun 2nd", nameof(DayOfWeek.Sunday) + " - From 03:00 To 04:00", scheduleList[1]);
			AssertEquals("Wed 1st", nameof(DayOfWeek.Wednesday) + " - From 00:30 To 01:00", scheduleList[2]);
			AssertEquals("Wed 2st", nameof(DayOfWeek.Wednesday) + " - From 03:00 To 03:30", scheduleList[3]);
			AssertEquals("Thu 1st", nameof(DayOfWeek.Thursday) + " - From 00:30 To 01:00", scheduleList[4]);
			AssertEquals("Thu 2st", nameof(DayOfWeek.Thursday) + " - From 01:30 To 02:00", scheduleList[5]);
		}

		public void TestSetNextScheduledTime()
		{
			ScheduleForTest testSchedule = new ScheduleForTest();
			testSchedule.NextScheduledTime = new DateTime(2003, 08, 03);
			testSchedule.NextScheduledIntervalStartTime = new DateTime(2003, 08, 03);
			testSchedule.NextScheduledIntervalEndTime = new DateTime(2003, 08, 03);
			testSchedule.Setup("   *    *     *  *,,*                                      *             ");

			AssertEquals("Sunday  03-Ago-2003 01:30", new DateTime(2003, 08, 03, 01, 30, 00), testSchedule.NextScheduledTimePlusPlus());
			AssertEquals("Sunday  03-Ago-2003 04:00", new DateTime(2003, 08, 03, 04, 00, 00), testSchedule.NextScheduledTimePlusPlus());
			AssertEquals("Sunday  03-Ago-2003 07:00", new DateTime(2003, 08, 03, 07, 00, 00), testSchedule.NextScheduledTimePlusPlus());
			AssertEquals("Sunday  03-Ago-2003 08:30", new DateTime(2003, 08, 03, 08, 30, 00), testSchedule.NextScheduledTimePlusPlus());
			AssertEquals("Tuesday 05-Ago-2003 00:00", new DateTime(2003, 08, 05, 00, 00, 00), testSchedule.NextScheduledTimePlusPlus());
			AssertEquals("Tuesday 05-Ago-2003 19:30", new DateTime(2003, 08, 05, 19, 30, 00), testSchedule.NextScheduledTimePlusPlus());
			AssertEquals("Sunday  10-Ago-2003 01:30", new DateTime(2003, 08, 10, 01, 30, 00), testSchedule.NextScheduledTimePlusPlus());
		}

		public void TestRandomiseNextScheduledTime()
		{
			ScheduleForTest testSchedule = new ScheduleForTest();
			testSchedule.NextScheduledTime = new DateTime(2003, 08, 03);
			testSchedule.NextScheduledIntervalStartTime = new DateTime(2003, 08, 03);
			testSchedule.NextScheduledIntervalEndTime = new DateTime(2003, 08, 03);
			testSchedule.Setup("   ***");

			AssertEquals("Scheduled for Sunday  03-Aug-2003 01:30", new DateTime(2003, 08, 03, 01, 30, 00), testSchedule.NextScheduledTime);
			AssertEquals("Scheduled interval starts Sunday  03-Aug-2003 01:30", new DateTime(2003, 08, 03, 01, 30, 00), testSchedule.NextScheduledIntervalStartTime);
			AssertEquals("Scheduled interval ends Sunday  03-Aug-2003 03:00", new DateTime(2003, 08, 03, 03, 00, 00), testSchedule.NextScheduledIntervalEndTime);

			testSchedule.RandomiseNextScheduledTime(10);
			Assert("Scheduled time randomised in the interval", testSchedule.NextScheduledTime >= testSchedule.NextScheduledIntervalStartTime && testSchedule.NextScheduledTime <= testSchedule.NextScheduledIntervalEndTime.AddMinutes(-10));
		}

		public void TestIsScheduledTime()
		{
			ScheduleForTest testSchedule = new ScheduleForTest();
			testSchedule.NextScheduledTime = new DateTime(2003, 08, 03);
			testSchedule.NextScheduledIntervalStartTime = new DateTime(2003, 08, 03);
			testSchedule.NextScheduledIntervalEndTime = new DateTime(2003, 08, 03);

			AssertEquals("Is not scheduled time because there is no schedule", false, testSchedule.IsScheduledTime);

			testSchedule.Setup("");
			AssertEquals("Is not scheduled time because schedule is empty string", false, testSchedule.IsScheduledTime);

			testSchedule.Setup("   ***");

			AssertEquals("Is scheduled time now", true, testSchedule.IsScheduledTime);
		}

		public void TestIsTimeToCheckSheduledActionsResult()
		{
			ScheduleForTest testSchedule = new ScheduleForTest();
			testSchedule.NextScheduledTime = new DateTime(2003, 08, 03);
			testSchedule.NextScheduledIntervalStartTime = new DateTime(2003, 08, 03);
			testSchedule.NextScheduledIntervalEndTime = new DateTime(2003, 08, 03);

			AssertEquals("Is not time to check resulr  because there is no schedule", false, testSchedule.IsTimeToCheckSheduledActionsResult);

			testSchedule.Setup("   ***");

			AssertEquals("Is time to check result now", true, testSchedule.IsTimeToCheckSheduledActionsResult);
		}
	}
}
