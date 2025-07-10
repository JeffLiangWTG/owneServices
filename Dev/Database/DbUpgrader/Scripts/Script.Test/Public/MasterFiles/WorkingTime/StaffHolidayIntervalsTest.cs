using System;
using System.Globalization;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	[TestedType(typeof(StaffHolidayIntervals))]
	class StaffHolidayIntervalsTest : DbCreateScriptTest
	{
		[TestDate(2017, 5, 23)]
		public void TestHolidayIntervals_ShouldNotIncludeWorkingAway()
		{
			var referenceDate = new DateTime(2017, 5, 23);

			var staffPk = Guid.NewGuid();
			var personPk = Guid.NewGuid();

			var sql = new SqlQueryBuilder();
			sql.AppendLine(string.Format(CultureInfo.InvariantCulture, "INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName) VALUES ('{0}', 'AAA')", personPk));
			sql.AppendLine(string.Format(CultureInfo.InvariantCulture, "INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{0}', 'AAA', '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", staffPk, personPk));
			sql.AppendLine(string.Format(CultureInfo.InvariantCulture, "INSERT INTO dbo.GlbStaffHoliday (GA_PK, GA_GS, GA_StartTime, GA_EndTime, GA_IsWorkingAway, GA_SystemCreateTimeUtc, GA_SystemCreateUser, GA_SystemLastEditTimeUtc, GA_SystemLastEditUser) VALUES (NEWID(), '{0}', '{1}', '{2}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", staffPk, referenceDate.AddDays(-2).ToSqlFormat(), referenceDate.AddDays(-1).ToSqlFormat()));
			sql.AppendLine(string.Format(CultureInfo.InvariantCulture, "INSERT INTO dbo.GlbStaffHoliday (GA_PK, GA_GS, GA_StartTime, GA_EndTime, GA_IsWorkingAway, GA_SystemCreateTimeUtc, GA_SystemCreateUser, GA_SystemLastEditTimeUtc, GA_SystemLastEditUser) VALUES (NEWID(), '{0}', '{1}', '{2}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", staffPk, referenceDate.AddDays(-4).ToSqlFormat(), referenceDate.AddDays(-3).ToSqlFormat()));

			TestConnection.ExecuteNonQuery(sql.ToString());

			var startDateOnProperHoliday = TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT DateFrom FROM dbo.StaffHolidayIntervals('{0}', '{1}', '{2}')", staffPk, referenceDate.AddDays(-2).ToSqlFormat(), referenceDate.AddDays(-1).ToSqlFormat()));
			var endDateOnProperHoliday = TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT DateTo FROM dbo.StaffHolidayIntervals('{0}', '{1}', '{2}')", staffPk, referenceDate.AddDays(-2).ToSqlFormat(), referenceDate.AddDays(-1).ToSqlFormat()));
			AssertEquals(referenceDate.AddDays(-2), startDateOnProperHoliday);
			AssertEquals(referenceDate.AddDays(-1), endDateOnProperHoliday);

			var startDateWhenWorkingAway = TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT DateFrom FROM dbo.StaffHolidayIntervals('{0}', '{1}', '{2}')", staffPk, referenceDate.AddDays(-4).ToSqlFormat(), referenceDate.AddDays(-3).ToSqlFormat()));
			var endDateWhenWorkingAway = TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT DateTo FROM dbo.StaffHolidayIntervals('{0}', '{1}', '{2}')", staffPk, referenceDate.AddDays(-4).ToSqlFormat(), referenceDate.AddDays(-3).ToSqlFormat()));
			AssertNull(startDateWhenWorkingAway);
			AssertNull(endDateWhenWorkingAway);
		}
	}
}
