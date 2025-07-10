using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.EffectiveDate
{
	[TestedType(typeof(TG_Maintain_GET_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GET_EffectiveEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsertWithIsApprovedTrue()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var team = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1), isApproved: true);

			staff.AppendInsertAndReturnObject(sql);
			team.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentTeam.AssertFromDB(TestConnection, team.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GET_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestFirstSingleInsertWithIsApprovedFalse()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var team = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1), isApproved: false);

			staff.AppendInsertAndReturnObject(sql);
			team.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentTeam.AssertFromDB(TestConnection, team.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GET_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var teams = new[]
			{
				new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1)),
				new GlbEmploymentTeam(staff.PK, new DateTime(2020, 2, 1)),
				new GlbEmploymentTeam(staff.PK, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(teams, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentTeam.AssertFromDB(TestConnection, teams[0].PK)
				.ExpectEquals("A team's end date should match the proceeding row", l => l.GET_AutoEffectiveEndDate, teams[1].GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, teams[1].PK)
				.ExpectEquals("A team's end date should match the proceeding row", l => l.GET_AutoEffectiveEndDate, teams[2].GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, teams[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GET_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsertWithIsApproved()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var teams = new[]
			{
				new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1), isApproved: true),
				new GlbEmploymentTeam(staff.PK, new DateTime(2020, 2, 1), isApproved: false),
				new GlbEmploymentTeam(staff.PK, new DateTime(2020, 3, 1), isApproved: true),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(teams, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentTeam.AssertFromDB(TestConnection, teams[0].PK)
				.ExpectEquals("A team's end date should match the approved proceeding row", l => l.GET_AutoEffectiveEndDate, teams[2].GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, teams[1].PK)
				.ExpectEquals("End date will not be calculated for unapproved row", l => l.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, teams[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GET_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 2, 1));
			var nextGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGET, gslToUpdate, nextGET }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbEmploymentTeam.GET_GST_NKTeamCode), "'TN1'", "TN1"),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE dbo.GlbEmploymentTeam
					SET
						{column} = {sqlValue}
					WHERE
						GET_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbEmploymentTeamSchema.PK));

				GlbEmploymentTeam.AssertFromDB(TestConnection, gslToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbEmploymentTeam.AssertFromDB(TestConnection, previousGET.PK)
					.ExpectNotEquals($"The update should not affect the previous team ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbEmploymentTeam.AssertFromDB(TestConnection, nextGET.PK)
					.ExpectNotEquals($"The update should not affect the next team ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbEmploymentTeam team)
					=> team.GetType().GetProperty(column).GetValue(team);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1), "TN1");
			var gslToUpdate = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 2, 1), "TN2");
			var nextGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 3, 1), "TN3");
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGET, gslToUpdate, nextGET }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentTeam
				SET
					GET_EffectiveDate = @newDate
				WHERE
					GET_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbEmploymentTeamSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentTeamSchema.GET_EffectiveDate);
			});

			GlbEmploymentTeam.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GET_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GET_AutoEffectiveEndDate, nextGET.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousGET.PK)
				.ExpectEquals("The update should not change the previous team's effective date", r => r.GET_EffectiveDate, previousGET.GET_EffectiveDate)
				.ExpectEquals("The update should change the previous team's end date", r => r.GET_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextGET.PK)
				.ExpectEquals("The update should not affect the next team's effective date", r => r.GET_EffectiveDate, nextGET.GET_EffectiveDate)
				.ExpectEquals("The update should not affect the next team's end date", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateEffectiveDateOnExistingRowWithIsApproved()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1), "TN1", true);
			var previousUnApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 2, 1), "TN2", false);
			var toUpdate = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 3, 1), "TN3", true);
			var nextUnApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 4, 1), "TN4", false);
			var nextApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 5, 1), "TN5", true);
			var newDate = new DateTimeOffset(new DateTime(2020, 3, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousApprovedGET, previousUnApprovedGET, toUpdate, nextUnApprovedGET, nextApprovedGET }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentTeam.AssertFromDB(TestConnection, toUpdate.PK)
				.ExpectEquals("Before update effective date", r => r.GET_EffectiveDate, toUpdate.GET_EffectiveDate)
				.ExpectEquals("The row's end date is the next approved team's effective date", r => r.GET_AutoEffectiveEndDate, nextApprovedGET.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousUnApprovedGET.PK)
				.ExpectEquals("The previous unapproved team's effective date", r => r.GET_EffectiveDate, previousUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The previous unapproved team's end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousApprovedGET.PK)
				.ExpectEquals("The previous approved team's effective date", r => r.GET_EffectiveDate, previousApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The previous approved team's end date is the toUpdate team's effective date", r => r.GET_AutoEffectiveEndDate, toUpdate.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextUnApprovedGET.PK)
				.ExpectEquals("The next unapproved team's effective date", r => r.GET_EffectiveDate, nextUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The next unapproved team's end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextApprovedGET.PK)
				.ExpectEquals("The next approved team's effective date", r => r.GET_EffectiveDate, nextApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The next approved team's end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentTeam
				SET
					GET_EffectiveDate = @newDate
				WHERE
					GET_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", toUpdate.PK, GlbEmploymentTeamSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentTeamSchema.GET_EffectiveDate);
			});

			GlbEmploymentTeam.AssertFromDB(TestConnection, toUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GET_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GET_AutoEffectiveEndDate, nextApprovedGET.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousUnApprovedGET.PK)
				.ExpectEquals("The update should not change the previous unapproved team's effective date", r => r.GET_EffectiveDate, previousUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The update should not change the previous unapproved team's end date", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousApprovedGET.PK)
				.ExpectEquals("The update should not change the previous approved team's effective date", r => r.GET_EffectiveDate, previousApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The update should change the previous approved team's end date", r => r.GET_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextUnApprovedGET.PK)
				.ExpectEquals("The update should not affect the next unapproved team's effective date", r => r.GET_EffectiveDate, nextUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The update should not affect the next unapproved team's end date", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextApprovedGET.PK)
				.ExpectEquals("The update should not affect the next approved team's effective date", r => r.GET_EffectiveDate, nextApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The update should not affect the next approved team's end date", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1), "TN1");
			var gslToUpdate = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 2, 1), "TN2");
			var nextGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 3, 1), "TN3");
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGET, gslToUpdate, nextGET }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentTeam
				SET
					GET_EffectiveDate = @newDate
				WHERE
					GET_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbEmploymentTeamSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentTeamSchema.GET_EffectiveDate);
			});

			GlbEmploymentTeam.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GET_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousGET.PK)
				.ExpectEquals("The update should not change the previous team's effective date", r => r.GET_EffectiveDate, previousGET.GET_EffectiveDate)
				.ExpectEquals("The update should change the previous team's end date to the next row's end date", r => r.GET_AutoEffectiveEndDate, nextGET.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextGET.PK)
				.ExpectEquals("The update should not affect the next team's effective date", r => r.GET_EffectiveDate, nextGET.GET_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GET_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRowWithIsApproved()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1), "TN1", true);
			var previousUnApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 2, 1), "TN2", false);
			var toUpdate = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 3, 1), "TN3", true);
			var nextUnApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 4, 1), "TN4", false);
			var nextApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 5, 1), "TN5", true);
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousApprovedGET, previousUnApprovedGET, toUpdate, nextUnApprovedGET, nextApprovedGET }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentTeam.AssertFromDB(TestConnection, toUpdate.PK)
				.ExpectEquals("Before update effective date", r => r.GET_EffectiveDate, toUpdate.GET_EffectiveDate)
				.ExpectEquals("The row's end date is the next approved team's effective date", r => r.GET_AutoEffectiveEndDate, nextApprovedGET.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousUnApprovedGET.PK)
				.ExpectEquals("The previous unapproved team's effective date", r => r.GET_EffectiveDate, previousUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The previous unapproved team's end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousApprovedGET.PK)
				.ExpectEquals("The previous approved team's effective date", r => r.GET_EffectiveDate, previousApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The previous approved team's end date is the toUpdate team's effective date", r => r.GET_AutoEffectiveEndDate, toUpdate.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextUnApprovedGET.PK)
				.ExpectEquals("The next unapproved team's effective date", r => r.GET_EffectiveDate, nextUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The next unapproved team's end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextApprovedGET.PK)
				.ExpectEquals("The next approved team's effective date", r => r.GET_EffectiveDate, nextApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The next approved team's end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentTeam
				SET
					GET_EffectiveDate = @newDate
				WHERE
					GET_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", toUpdate.PK, GlbEmploymentTeamSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentTeamSchema.GET_EffectiveDate);
			});

			GlbEmploymentTeam.AssertFromDB(TestConnection, toUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GET_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousUnApprovedGET.PK)
				.ExpectEquals("The update should not change the previous unapproved team's effective date", r => r.GET_EffectiveDate, previousUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The update should not change the previous unapproved team's end date to the next approved row's end date", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousApprovedGET.PK)
				.ExpectEquals("The update should not change the previous approved team's effective date", r => r.GET_EffectiveDate, previousApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The update should change the previous approved team's end date to the next approved row's end date", r => r.GET_AutoEffectiveEndDate, nextApprovedGET.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextUnApprovedGET.PK)
				.ExpectEquals("The update should not affect the next unapproved team's effective date", r => r.GET_EffectiveDate, nextUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The update should not change the next unapproved team's end date to the next approved row's end date", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextApprovedGET.PK)
				.ExpectEquals("The update should not affect the next approved team's effective date", r => r.GET_EffectiveDate, nextApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GET_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleGlbEmploymentTeams()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[]
			{
				new GlbStaff("XYZ"),
				new GlbStaff("YZX"),
				new GlbStaff("ZXY"),
			};

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));
			TestConnection.ExecuteNonQuery(@"
				INSERT INTO dbo.GlbEmploymentTeam
					(GET_PK, GET_GS_Staff, GET_EffectiveDate, GET_GST_NKTeamCode, GET_SystemCreateTimeUtc, GET_SystemLastEditTimeUtc, GET_SystemCreateUser, GET_SystemLastEditUser)
				VALUES
					(NEWID(), @staff0, '2020-01-01 01:00:00 +01:00', 'TN0', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff0, '2020-02-01 11:00:00 +07:00', 'TN1', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-03-01 03:00:00 +11:00', 'TN2', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-04-01 17:00:00 -03:00', 'TN3', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-01-01 09:00:00 +00:00', 'TN4', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-04-01 21:00:00 -05:00', 'TN5', GETUTCDATE(), GETUTCDATE(), 'E', 'E')
			", p =>
			{
				for (var i = 0; i < staffs.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbEmploymentTeamSchema.GET_GS_Staff);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbEmploymentTeam.CountInDB(TestConnection));
			AssertRecordDateHistory("First staff has two teams on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertRecordDateHistory("Second staff has two teams on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertRecordDateHistory("Third staff has two teams split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GET_PK", // We won't ever update the primary key of a row
				"GET_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GET_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GET_PK", $"'{pk}'" },
				{ "GET_AutoVersion", "0" },
				{ "GET_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GET_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GET_GST_NKTeamCode", "'TN2'" },
				{ "GET_GCR_ChangeRequest", "null" },
				{ "GET_IsApproved", "'1'" },
				{ "GET_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GET_SystemCreateUser", "'XYZ'" },
				{ "GET_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GET_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GET_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GET_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GET_GST_NKTeamCode", "'TN1'", "TN1"),
				("GET_GCR_ChangeRequest", "null", DBNull.Value),
				("GET_IsApproved", "'1'", true),
				("GET_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GET_SystemCreateUser", "'E'", "E"),
				("GET_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GET_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbEmploymentTeam");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO dbo.GlbEmploymentTeam ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE dbo.GlbEmploymentTeam SET {edit.column}={edit.sqlValue} WHERE GET_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM dbo.GlbEmploymentTeam WHERE GET_PK='{pk}'");
					AssertEquals(edit.column, edit.assertValue, value);
				}
			});
		}

		ICollection<string> ColumnNames(string tablename)
		{
			var result = new HashSet<string>();
			TestConnection.ExecuteReader(
				"select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = @table",
				p => p.AddParameter("@table", System.Data.SqlDbType.NVarChar, tablename),
				r => result.Add(r.GetString(0))
			);

			return result;
		}

		public void TestInsertBothBeforeAndAfterCurrentRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");

			var previousGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1), "TN2");
			var middleRow = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 2, 1), "TN1");
			var nextGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 3, 1), "TN2");

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousGET.AppendInsertAndReturnObject(sql);
			nextGET.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentTeam.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GET_EffectiveDate, middleRow.GET_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GET_AutoEffectiveEndDate, nextGET.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousGET.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.GET_AutoEffectiveEndDate, middleRow.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextGET.PK)
				.ExpectEquals("The next rows end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestInsertBothBeforeAndAfterCurrentRowWithIsApproved()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");

			var previousApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 1, 1), "TN1", true);
			var previousUnApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 2, 1), "TN2", false);
			var middleRow = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 3, 1), "TN3", true);
			var nextUnApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 4, 1), "TN4", false);
			var nextApprovedGET = new GlbEmploymentTeam(staff.PK, new DateTime(2020, 5, 1), "TN5", true);

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentTeam.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date", r => r.GET_EffectiveDate, middleRow.GET_EffectiveDate)
				.ExpectEquals("There is no next end-date, so it should be null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			sql.Clear();
			previousApprovedGET.AppendInsertAndReturnObject(sql);
			previousUnApprovedGET.AppendInsertAndReturnObject(sql);
			nextUnApprovedGET.AppendInsertAndReturnObject(sql);
			nextApprovedGET.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentTeam.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GET_EffectiveDate, middleRow.GET_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GET_AutoEffectiveEndDate, nextApprovedGET.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousUnApprovedGET.PK)
				.ExpectEquals("The previous unapproved row's effective date", r => r.GET_EffectiveDate, previousUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The previous unapproved row's end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, previousApprovedGET.PK)
				.ExpectEquals("The previous approved row's effective date", r => r.GET_EffectiveDate, previousApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The previous approved row has its end date set to the value in middle row", r => r.GET_AutoEffectiveEndDate, middleRow.GET_EffectiveDate)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextUnApprovedGET.PK)
				.ExpectEquals("The next unapproved row's effective date", r => r.GET_EffectiveDate, nextUnApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The next unapproved row's end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentTeam.AssertFromDB(TestConnection, nextApprovedGET.PK)
				.ExpectEquals("The next approved row's effective date", r => r.GET_EffectiveDate, nextApprovedGET.GET_EffectiveDate)
				.ExpectEquals("The next approved row's end date is null", r => r.GET_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertRecordDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the team dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GET_EffectiveDate, GET_AutoEffectiveEndDate FROM dbo.GlbEmploymentTeam WHERE GET_GS_Staff = '{staff}' ORDER BY GET_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbEmploymentTeam.GET_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbEmploymentTeam.GET_AutoEffectiveEndDate)];

				actual.Add((effectiveDate, effectiveEndDate == DBNull.Value ? null : (DateTimeOffset?)effectiveEndDate));
			});

			var expected = expectedHistory.Zip(expectedHistory.Skip(1).Append(null), (a, b) => (a, b)).ToList();
			AssertSequencesEqual(message, expected, actual);
		}

		protected override void SetUp()
		{
			var dbName = TestConnection.CurrentDatabase;

			connectionThatCanAccessHrmSchema = Db.NewExtraUnrestrictedWriterConnection(TestConnection.ServerName, dbName);
			base.SetUp();
		}

		protected override void TearDown()
		{
			connectionThatCanAccessHrmSchema?.Dispose();
			connectionThatCanAccessHrmSchema = null;
			base.TearDown();
		}

		protected override DbConnection TestConnection => connectionThatCanAccessHrmSchema ?? base.TestConnection;
		DbConnection connectionThatCanAccessHrmSchema;
	}

	class GlbEmploymentTeam : SQLDataObject<GlbEmploymentTeam>
	{
		public GlbEmploymentTeam(Guid staff, DateTimeOffset effectiveDate, string teamName = "TM1", bool isApproved = true)
		{
			GET_GS_Staff = staff;
			GET_EffectiveDate = effectiveDate;

			GET_GST_NKTeamCode = teamName;

			GET_SystemCreateTimeUtc = DateTime.UtcNow;
			GET_SystemLastEditTimeUtc = DateTime.UtcNow;

			GET_SystemCreateUser = "E";
			GET_SystemLastEditUser = "E";
			GET_IsApproved = isApproved;
		}

		public Guid GET_GS_Staff { get; }
		public DateTimeOffset GET_EffectiveDate { get; }
		public DateTimeOffset? GET_AutoEffectiveEndDate { get; }

		public string GET_GST_NKTeamCode { get; }

		public string GET_SystemCreateUser { get; }
		public string GET_SystemLastEditUser { get; }
		public DateTime GET_SystemCreateTimeUtc { get; }
		public DateTime GET_SystemLastEditTimeUtc { get; }

		public bool GET_IsApproved { get; }

		protected override ITableDefinition GetSchema() => base.GetSchema()
			.Extend(nameof(GET_AutoEffectiveEndDate), new DateTimeOffsetColumnInfo(0, isNullable: true));
	}
}
