using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.WorkingTimeTestHelper;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	[TestedType(typeof(WorkingIntervals))]
	class WorkingIntervalsTest : DbCreateScriptTest
	{
		public void TestSelectSingleDay()
		{
			CreateWorkWeek(Db.Connection, "GS", dummyStaffPk,
				monday: "08:30 to 13:00, 14:00 to 17:30",
				tuesday: "08:30 to 14:00, 14:30 to 17:30", // 8.5 hours
				wednesday: "06:00 to 07:00, 08:00 to 09:00, 10:00 to 16:00");

			var actualMinutesTuesday = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervals('2020/04/14', '2020/04/15', null, '{dummyStaffPk}')");
			AssertEquals("On Tuesday, the staff worked 8 hours", 8 * 60 + 30, actualMinutesTuesday);
		}

		public void TestSelectMultipleDays()
		{
			CreateWorkWeek(Db.Connection, "GS", dummyStaffPk,
				monday: "08:30 to 13:00, 14:00 to 17:30",
				tuesday: "08:30 to 14:00, 14:30 to 17:30", // 8.5 hours / 510 minutes
				wednesday: "06:00 to 11:00, 11:30 to 14:30", // 8 hours / 480 minutes
				thursday: "10:00 to 13:00, 13:30 to 16:00", // 5.5 hours / 330 minutes
				friday: "06:00 to 07:00, 08:00 to 09:00, 10:00 to 16:00");

			var actualMinutesTueToThu = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervals('2020/04/14', '2020/04/17', null, '{dummyStaffPk}')");
			var expectedMinutesTueToThu = 510 + 480 + 330;

			AssertEquals("Should be the sum of the working minutes of Tue, Wed & Thu", expectedMinutesTueToThu, actualMinutesTueToThu);
		}

		public void TestSelectMultipleWeeks()
		{
			CreateWorkWeek(Db.Connection, "GS", dummyStaffPk,
				monday: "09:00 to 17:00",
				tuesday: "09:00 to 17:00",
				wednesday: "09:00 to 17:00",
				thursday: "09:00 to 17:00",
				friday: "09:00 to 17:00");

			var actualMinutes = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervals('2020/04/20', '2020/05/04', null, '{dummyStaffPk}')");

			var hoursPerWeek = 5 * 8;
			var numberOfWeeks = 2;

			AssertEquals("Should be able to handle multiple weeks", hoursPerWeek * numberOfWeeks * 60, actualMinutes);
		}

		public void TestSelectPartialDay()
		{
			CreateWorkWeek(Db.Connection, "GS", dummyStaffPk,
				monday: "09:00 to 12:00, 12:30 to 17:00",
				tuesday: "09:00 to 12:00, 12:30 to 17:00");

			var actualMinutes = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervals('2020/04/20 15:00', '2020/04/21', null, '{dummyStaffPk}')");
			AssertEquals("Selecting a partial day should return the remaining minutes of that day", 2 * 60, actualMinutes);
		}

		public void TestComplexPartialDay()
		{
			CreateWorkWeek(Db.Connection, "GS", dummyStaffPk,
				monday: "09:00 to 09:30, 10:00 to 11:30, 12:30 to 14:30, 15:00 to 17:00",
				tuesday: "09:00 to 17:00");

			var actualMinutes = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervals('2020/04/20 10:30', '2020/04/20 14:00', null, '{dummyStaffPk}')");

			var firstInterval = 60; // 10:30 to 11:30
			var secondInterval = 90; // 12:30 to 14:00

			AssertEquals("Selecting a partial day should return the remaining minutes of that day", firstInterval + secondInterval, actualMinutes);
		}

		public void TestSelectEmptyDays()
		{
			CreateWorkWeek(Db.Connection, "GS", dummyStaffPk,
				monday: "08:30 to 13:00, 14:00 to 17:30",
				tuesday: "08:30 to 14:00, 14:30 to 17:30",
				wednesday: "06:00 to 11:00, 11:30 to 14:30",
				thursday: "10:00 to 13:00, 13:30 to 16:00",
				friday: "06:00 to 07:00, 08:00 to 09:00, 10:00 to 16:00");

			var actualWeekend = Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM WorkingIntervals('2020/04/18', '2020/04/20', null, '{dummyStaffPk}')");
			AssertEquals("The staff didn't work on the weekend, so we should have zero minutes", 0, actualWeekend);
		}
	}
}

