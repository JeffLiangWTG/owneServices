using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.WorkingTimeTestHelper;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	[TestedType(typeof(WorkingIntervalsForBufferPenetration))]
	class WorkingIntervalsForBufferPenetrationTest : DbCreateScriptTest
	{
		public void TestSelectSingleDay()
		{
			CreateWorkWeek(Db.Connection, "GE", dummyDepartmentPk,
				monday: "08:30 to 13:00, 14:00 to 17:30",
				tuesday: "08:30 to 14:00, 14:30 to 17:30", // 8.5 hours
				wednesday: "06:00 to 07:00, 08:00 to 09:00, 10:00 to 16:00");

			var actualMinutesTuesday = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervalsForBufferPenetration('2020/04/14', '2020/04/15', '{dummyDepartmentPk}')");
			AssertEquals("On Tuesday, the staff worked 8 hours", 8 * 60 + 30, actualMinutesTuesday);
		}

		public void TestSelectMultipleDays()
		{
			CreateWorkWeek(Db.Connection, "GE", dummyDepartmentPk,
				monday: "08:30 to 13:00, 14:00 to 17:30",
				tuesday: "08:30 to 14:00, 14:30 to 17:30", // 8.5 hours / 510 minutes
				wednesday: "06:00 to 11:00, 11:30 to 14:30", // 8 houts / 480 minutes
				thursday: "10:00 to 13:00, 13:30 to 16:00", // 5.5 hours / 330 minutes
				friday: "06:00 to 07:00, 08:00 to 09:00, 10:00 to 16:00");

			var actualMinutesTueToThu = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervalsForBufferPenetration('2020/04/14', '2020/04/17', '{dummyDepartmentPk}')");
			var expectedMinutesTueToThu = 510 + 480 + 330;

			AssertEquals("Should be the sum of the working minutes of Tue, Wed & Thu", expectedMinutesTueToThu, actualMinutesTueToThu);
		}

		public void TestSelectMultipleWeeks()
		{
			CreateWorkWeek(Db.Connection, "GE", dummyDepartmentPk,
				monday: "09:00 to 17:00",
				tuesday: "09:00 to 17:00",
				wednesday: "09:00 to 17:00",
				thursday: "09:00 to 17:00",
				friday: "09:00 to 17:00");

			var actualMinutes = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervalsForBufferPenetration('2020/04/20', '2020/05/04', '{dummyDepartmentPk}')");

			var hoursPerWeek = 5 * 8;
			var numberOfWeeks = 2;

			AssertEquals("Should be able to handle multiple weeks", hoursPerWeek * numberOfWeeks * 60, actualMinutes);
		}

		public void TestSelectPartialDay()
		{
			CreateWorkWeek(Db.Connection, "GE", dummyDepartmentPk,
				monday: "09:00 to 12:00, 12:30 to 17:00",
				tuesday: "09:00 to 12:00, 12:30 to 17:00");

			var actualMinutes = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervalsForBufferPenetration('2020/04/20 15:00', '2020/04/21', '{dummyDepartmentPk}')");
			AssertEquals("Selecting a partial day should return the remaining minutes of that day", 2 * 60, actualMinutes);
		}

		public void TestComplexPartialDay()
		{
			CreateWorkWeek(Db.Connection, "GE", dummyDepartmentPk,
				monday: "09:00 to 09:30, 10:00 to 11:30, 12:30 to 14:30, 15:00 to 17:00",
				tuesday: "09:00 to 17:00");

			var actualMinutes = Db.Connection.ExecuteScalar<int>($"SELECT SUM(working_minutes) FROM WorkingIntervalsForBufferPenetration('2020/04/20 10:30', '2020/04/20 14:00', '{dummyDepartmentPk}')");

			var firstInterval = 60; // 10:30 to 11:30
			var secondInterval = 90; // 12:30 to 14:00

			AssertEquals("Selecting a partial day should return the remaining minutes of that day", firstInterval + secondInterval, actualMinutes);
		}

		public void TestWorkingIntervalsForBufferPenetration_whenSameDayAndLessThenHalfHour()
		{
			var fromDate = new DateTime(2019, 03, 06, 9, 15, 0);
			var toDate = new DateTime(2019, 03, 06, 9, 25, 0);
			string sql = $"select * from dbo.WorkingIntervalsForBufferPenetration('{fromDate.ToSqlFormat()}', '{toDate.ToSqlFormat()}', '{Guid.NewGuid()}')";

			using (var command = TestConnection.Command(string.Format(sql, fromDate.ToSqlFormat(), toDate.ToSqlFormat())))
			using (var reader = command.ExecuteReader())
			{
				Assert(reader.Read());

				var dateFrom = reader.GetDateTime(0);
				var workingMinutes = reader.GetInt32(1);

				AssertEquals(fromDate, dateFrom);
				AssertEquals(10, workingMinutes);
			}
		}

		public void TestWorkingIntervalsForBufferPenetration_whenSameDayAndMoreThenHalfHour()
		{
			var fromDate = new DateTime(2019, 03, 06, 9, 15, 0);
			var toDate = new DateTime(2019, 03, 06, 9, 55, 0);
			string sql = $"select * from dbo.WorkingIntervalsForBufferPenetration('{fromDate.ToSqlFormat()}', '{toDate.ToSqlFormat()}', '{Guid.NewGuid()}')";

			using (var command = TestConnection.Command(string.Format(sql, fromDate.ToSqlFormat(), toDate.ToSqlFormat())))
			using (var reader = command.ExecuteReader())
			{
				Assert(reader.Read());

				var dateFrom = reader.GetDateTime(0);
				var workingMinutes = reader.GetInt32(1);

				AssertEquals(fromDate, dateFrom);
				AssertEquals(40, workingMinutes);
			}
		}
	}
}

