using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.EffectiveDate;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.EffectiveDate
{
	[TestedType(typeof(TG_Maintain_GEH_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GEH_EffectiveEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsertWithIsApprovedTrue()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var record = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1), isApproved: true);

			staff.AppendInsertAndReturnObject(sql);
			record.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentHistory.AssertFromDB(TestConnection, record.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestFirstSingleInsertWithIsApprovedFalse()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var record = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1), isApproved: false);

			staff.AppendInsertAndReturnObject(sql);
			record.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentHistory.AssertFromDB(TestConnection, record.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var records = new[]
			{
				new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1)),
				new GlbEmploymentHistory(staff.PK, new DateTime(2020, 2, 1)),
				new GlbEmploymentHistory(staff.PK, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(records, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentHistory.AssertFromDB(TestConnection, records[0].PK)
				.ExpectEquals("A record's end date should match the proceeding row", l => l.GEH_AutoEffectiveEndDate, records[1].GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, records[1].PK)
				.ExpectEquals("A record's end date should match the proceeding row", l => l.GEH_AutoEffectiveEndDate, records[2].GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, records[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsertWithIsApproved()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var records = new[]
			{
				new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1), isApproved: true),
				new GlbEmploymentHistory(staff.PK, new DateTime(2020, 2, 1), isApproved: false),
				new GlbEmploymentHistory(staff.PK, new DateTime(2020, 3, 1), isApproved: true),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(records, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentHistory.AssertFromDB(TestConnection, records[0].PK)
				.ExpectEquals("A record's end date should match the approved proceeding row", l => l.GEH_AutoEffectiveEndDate, records[2].GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, records[1].PK)
				.ExpectEquals("End date will not be calculated for unapproved row", l => l.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, records[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 2, 1));
			var nextGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGEH, gslToUpdate, nextGEH }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbEmploymentHistory.GEH_JobTitle), "'JobTitle1'", "JobTitle1"),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE dbo.GlbEmploymentHistory
					SET
						{column} = {sqlValue}
					WHERE
						GEH_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbEmploymentHistorySchema.PK));

				GlbEmploymentHistory.AssertFromDB(TestConnection, gslToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbEmploymentHistory.AssertFromDB(TestConnection, previousGEH.PK)
					.ExpectNotEquals($"The update should not affect the previous record ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbEmploymentHistory.AssertFromDB(TestConnection, nextGEH.PK)
					.ExpectNotEquals($"The update should not affect the next record ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbEmploymentHistory record)
					=> record.GetType().GetProperty(column).GetValue(record);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1), "JobTitle0");
			var gslToUpdate = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 2, 1), "JobTitle1");
			var nextGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 3, 1), "JobTitle2");
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGEH, gslToUpdate, nextGEH }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentHistory
				SET
					GEH_EffectiveDate = @newDate
				WHERE
					GEH_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbEmploymentHistorySchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentHistorySchema.GEH_EffectiveDate);
			});

			GlbEmploymentHistory.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GEH_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GEH_AutoEffectiveEndDate, nextGEH.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousGEH.PK)
				.ExpectEquals("The update should not change the previous record's effective date", r => r.GEH_EffectiveDate, previousGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should change the previous record's end date", r => r.GEH_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextGEH.PK)
				.ExpectEquals("The update should not affect the next record's effective date", r => r.GEH_EffectiveDate, nextGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should not affect the next record's end date", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateEffectiveDateOnExistingRowWithIsApproved()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1), "JobTitle0", true);
			var previousUnApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 2, 1), "JobTitle1", false);
			var toUpdate = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 3, 1), "JobTitle2", true);
			var nextUnApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 4, 1), "JobTitle3", false);
			var nextApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 5, 1), "JobTitle4", true);
			var newDate = new DateTimeOffset(new DateTime(2020, 3, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousApprovedGEH, previousUnApprovedGEH, toUpdate, nextUnApprovedGEH, nextApprovedGEH }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentHistory.AssertFromDB(TestConnection, toUpdate.PK)
				.ExpectEquals("Before update effective date", r => r.GEH_EffectiveDate, toUpdate.GEH_EffectiveDate)
				.ExpectEquals("The row's end date is the next approved record's effective date", r => r.GEH_AutoEffectiveEndDate, nextApprovedGEH.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousUnApprovedGEH.PK)
				.ExpectEquals("The previous unapproved record's effective date", r => r.GEH_EffectiveDate, previousUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The previous unapproved record's end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousApprovedGEH.PK)
				.ExpectEquals("The previous approved record's effective date", r => r.GEH_EffectiveDate, previousApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The previous approved record's end date is the toUpdate record's effective date", r => r.GEH_AutoEffectiveEndDate, toUpdate.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextUnApprovedGEH.PK)
				.ExpectEquals("The next unapproved record's effective date", r => r.GEH_EffectiveDate, nextUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The next unapproved record's end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextApprovedGEH.PK)
				.ExpectEquals("The next approved record's effective date", r => r.GEH_EffectiveDate, nextApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The next approved record's end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentHistory
				SET
					GEH_EffectiveDate = @newDate
				WHERE
					GEH_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", toUpdate.PK, GlbEmploymentHistorySchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentHistorySchema.GEH_EffectiveDate);
			});

			GlbEmploymentHistory.AssertFromDB(TestConnection, toUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GEH_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GEH_AutoEffectiveEndDate, nextApprovedGEH.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousUnApprovedGEH.PK)
				.ExpectEquals("The update should not change the previous unapproved record's effective date", r => r.GEH_EffectiveDate, previousUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should not change the previous unapproved record's end date", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousApprovedGEH.PK)
				.ExpectEquals("The update should not change the previous approved record's effective date", r => r.GEH_EffectiveDate, previousApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should change the previous approved record's end date", r => r.GEH_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextUnApprovedGEH.PK)
				.ExpectEquals("The update should not affect the next unapproved record's effective date", r => r.GEH_EffectiveDate, nextUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should not affect the next unapproved record's end date", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextApprovedGEH.PK)
				.ExpectEquals("The update should not affect the next approved record's effective date", r => r.GEH_EffectiveDate, nextApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should not affect the next approved record's end date", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1), "JobTitle0");
			var gslToUpdate = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 2, 1), "JobTitle1");
			var nextGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 3, 1), "JobTitle2");
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGEH, gslToUpdate, nextGEH }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentHistory
				SET
					GEH_EffectiveDate = @newDate
				WHERE
					GEH_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbEmploymentHistorySchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentHistorySchema.GEH_EffectiveDate);
			});

			GlbEmploymentHistory.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GEH_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousGEH.PK)
				.ExpectEquals("The update should not change the previous record's effective date", r => r.GEH_EffectiveDate, previousGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should change the previous record's end date to the next row's end date", r => r.GEH_AutoEffectiveEndDate, nextGEH.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextGEH.PK)
				.ExpectEquals("The update should not affect the next record's effective date", r => r.GEH_EffectiveDate, nextGEH.GEH_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GEH_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRowWithIsApproved()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1), "JobTitle0", true);
			var previousUnApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 2, 1), "JobTitle1", false);
			var toUpdate = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 3, 1), "JobTitle2", true);
			var nextUnApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 4, 1), "JobTitle3", false);
			var nextApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 5, 1), "JobTitle4", true);
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousApprovedGEH, previousUnApprovedGEH, toUpdate, nextUnApprovedGEH, nextApprovedGEH }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentHistory.AssertFromDB(TestConnection, toUpdate.PK)
				.ExpectEquals("Before update effective date", r => r.GEH_EffectiveDate, toUpdate.GEH_EffectiveDate)
				.ExpectEquals("The row's end date is the next approved record's effective date", r => r.GEH_AutoEffectiveEndDate, nextApprovedGEH.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousUnApprovedGEH.PK)
				.ExpectEquals("The previous unapproved record's effective date", r => r.GEH_EffectiveDate, previousUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The previous unapproved record's end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousApprovedGEH.PK)
				.ExpectEquals("The previous approved record's effective date", r => r.GEH_EffectiveDate, previousApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The previous approved record's end date is the toUpdate record's effective date", r => r.GEH_AutoEffectiveEndDate, toUpdate.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextUnApprovedGEH.PK)
				.ExpectEquals("The next unapproved record's effective date", r => r.GEH_EffectiveDate, nextUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The next unapproved record's end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextApprovedGEH.PK)
				.ExpectEquals("The next approved record's effective date", r => r.GEH_EffectiveDate, nextApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The next approved record's end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentHistory
				SET
					GEH_EffectiveDate = @newDate
				WHERE
					GEH_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", toUpdate.PK, GlbEmploymentHistorySchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentHistorySchema.GEH_EffectiveDate);
			});

			GlbEmploymentHistory.AssertFromDB(TestConnection, toUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GEH_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousUnApprovedGEH.PK)
				.ExpectEquals("The update should not change the previous unapproved record's effective date", r => r.GEH_EffectiveDate, previousUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should not change the previous unapproved record's end date to the next approved row's end date", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousApprovedGEH.PK)
				.ExpectEquals("The update should not change the previous approved record's effective date", r => r.GEH_EffectiveDate, previousApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should change the previous approved record's end date to the next approved row's end date", r => r.GEH_AutoEffectiveEndDate, nextApprovedGEH.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextUnApprovedGEH.PK)
				.ExpectEquals("The update should not affect the next unapproved record's effective date", r => r.GEH_EffectiveDate, nextUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The update should not change the next unapproved record's end date to the next approved row's end date", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextApprovedGEH.PK)
				.ExpectEquals("The update should not affect the next approved record's effective date", r => r.GEH_EffectiveDate, nextApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GEH_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleGlbEmploymentHistorys()
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
				INSERT INTO dbo.GlbEmploymentHistory
					(GEH_PK, GEH_GS_Staff, GEH_EffectiveDate, GEH_JobTitle, GEH_SystemCreateTimeUtc, GEH_SystemLastEditTimeUtc, GEH_SystemCreateUser, GEH_SystemLastEditUser)
				VALUES
					(NEWID(), @staff0, '2020-01-01 01:00:00 +01:00', 'JobTitle0', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff0, '2020-02-01 11:00:00 +07:00', 'JobTitle1', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-03-01 03:00:00 +11:00', 'JobTitle2', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-04-01 17:00:00 -03:00', 'JobTitle3', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-01-01 09:00:00 +00:00', 'JobTitle4', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-04-01 21:00:00 -05:00', 'JobTitle5', GETUTCDATE(), GETUTCDATE(), 'E', 'E')
			", p =>
			{
				for (var i = 0; i < staffs.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbEmploymentHistorySchema.GEH_GS_Staff);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbEmploymentHistory.CountInDB(TestConnection));
			AssertRecordDateHistory("First staff has two records on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertRecordDateHistory("Second staff has two records on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertRecordDateHistory("Third staff has two records split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GEH_PK", // We won't ever update the primary key of a row
				"GEH_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GEH_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GEH_PK", $"'{pk}'" },
				{ "GEH_AutoVersion", "0" },
				{ "GEH_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GEH_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GEH_JobTitle", "'JobTitle0'" },
				{ "GEH_HJ_JobRole", "null" },
				{ "GEH_JobFamily", "'JF0'" },
				{ "GEH_IsInternalPosition", "'0'" },
				{ "GEH_WorksOutsideBranch", "'1'" },
				{ "GEH_JobDescription", "'Job Description 111'" },
				{ "GEH_EmploymentType", "'ET0'" },
				{ "GEH_CompanyName", "'Company 01'" },
				{ "GEH_DepartureReason", "'DR1'" },
				{ "GEH_DepartureComments", "'Comments 0'" },
				{ "GEH_GCR_ChangeRequest", "null" },
				{ "GEH_IsApproved", "'1'" },
				{ "GEH_IsPromotion", "'0'" },
				{ "GEH_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GEH_SystemCreateUser", "'XYZ'" },
				{ "GEH_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GEH_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GEH_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GEH_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GEH_JobTitle", "'JobTitle1'", "JobTitle1"),
				("GEH_HJ_JobRole", "null", DBNull.Value),
				("GEH_JobFamily", "'JF0'", "JF0"),
				("GEH_IsInternalPosition", "'0'", false),
				("GEH_WorksOutsideBranch", "'1'", true),
				("GEH_JobDescription", "'Job Description 111'", "Job Description 111"),
				("GEH_EmploymentType", "'ET0'", "ET0"),
				("GEH_CompanyName", "'Company 02'", "Company 02"),
				("GEH_DepartureReason", "'DR2'", "DR2"),
				("GEH_DepartureComments", "'Comments 0'", "Comments 0"),
				("GEH_GCR_ChangeRequest", "null", DBNull.Value),
				("GEH_IsApproved", "'1'", true),
				("GEH_IsPromotion", "'1'", true),
				("GEH_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GEH_SystemCreateUser", "'E'", "E"),
				("GEH_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GEH_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbEmploymentHistory");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO dbo.GlbEmploymentHistory ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE dbo.GlbEmploymentHistory SET {edit.column}={edit.sqlValue} WHERE GEH_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM dbo.GlbEmploymentHistory WHERE GEH_PK='{pk}'");
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

			var previousGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1), "JobTitle0");
			var middleRow = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 2, 1), "JobTitle1");
			var nextGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 3, 1), "JobTitle0");

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousGEH.AppendInsertAndReturnObject(sql);
			nextGEH.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentHistory.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GEH_EffectiveDate, middleRow.GEH_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GEH_AutoEffectiveEndDate, nextGEH.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousGEH.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.GEH_AutoEffectiveEndDate, middleRow.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextGEH.PK)
				.ExpectEquals("The next rows end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestInsertBothBeforeAndAfterCurrentRowWithIsApproved()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");

			var previousApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 1, 1), "JobTitle0", true);
			var previousUnApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 2, 1), "JobTitle1", false);
			var middleRow = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 3, 1), "JobTitle2", true);
			var nextUnApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 4, 1), "JobTitle3", false);
			var nextApprovedGEH = new GlbEmploymentHistory(staff.PK, new DateTime(2020, 5, 1), "JobTitle4", true);

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentHistory.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date", r => r.GEH_EffectiveDate, middleRow.GEH_EffectiveDate)
				.ExpectEquals("There is no next end-date, so it should be null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			sql.Clear();
			previousApprovedGEH.AppendInsertAndReturnObject(sql);
			previousUnApprovedGEH.AppendInsertAndReturnObject(sql);
			nextUnApprovedGEH.AppendInsertAndReturnObject(sql);
			nextApprovedGEH.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentHistory.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GEH_EffectiveDate, middleRow.GEH_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GEH_AutoEffectiveEndDate, nextApprovedGEH.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousUnApprovedGEH.PK)
				.ExpectEquals("The previous unapproved row's effective date", r => r.GEH_EffectiveDate, previousUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The previous unapproved row's end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, previousApprovedGEH.PK)
				.ExpectEquals("The previous approved row's effective date", r => r.GEH_EffectiveDate, previousApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The previous approved row has its end date set to the value in middle row", r => r.GEH_AutoEffectiveEndDate, middleRow.GEH_EffectiveDate)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextUnApprovedGEH.PK)
				.ExpectEquals("The next unapproved row's effective date", r => r.GEH_EffectiveDate, nextUnApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The next unapproved row's end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentHistory.AssertFromDB(TestConnection, nextApprovedGEH.PK)
				.ExpectEquals("The next approved row's effective date", r => r.GEH_EffectiveDate, nextApprovedGEH.GEH_EffectiveDate)
				.ExpectEquals("The next approved row's end date is null", r => r.GEH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertRecordDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the record dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GEH_EffectiveDate, GEH_AutoEffectiveEndDate FROM dbo.GlbEmploymentHistory WHERE GEH_GS_Staff = '{staff}' ORDER BY GEH_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbEmploymentHistory.GEH_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbEmploymentHistory.GEH_AutoEffectiveEndDate)];

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

	class GlbEmploymentHistory : SQLDataObject<GlbEmploymentHistory>
	{
		public GlbEmploymentHistory(Guid staff, DateTimeOffset effectiveDate, string jobtitle = "JobTitle0", bool isApproved = true)
		{
			GEH_GS_Staff = staff;
			GEH_EffectiveDate = effectiveDate;

			GEH_JobTitle = jobtitle;

			GEH_SystemCreateTimeUtc = DateTime.UtcNow;
			GEH_SystemLastEditTimeUtc = DateTime.UtcNow;

			GEH_SystemCreateUser = "E";
			GEH_SystemLastEditUser = "E";
			GEH_IsApproved = isApproved;
		}

		public Guid GEH_GS_Staff { get; }
		public DateTimeOffset GEH_EffectiveDate { get; }
		public DateTimeOffset? GEH_AutoEffectiveEndDate { get; }

		public string GEH_JobTitle { get; }

		public string GEH_SystemCreateUser { get; }
		public string GEH_SystemLastEditUser { get; }
		public DateTime GEH_SystemCreateTimeUtc { get; }
		public DateTime GEH_SystemLastEditTimeUtc { get; }

		public bool GEH_IsApproved { get; }

		protected override ITableDefinition GetSchema() => base.GetSchema()
			.Extend(nameof(GEH_AutoEffectiveEndDate), new DateTimeOffsetColumnInfo(0, isNullable: true));
	}
}
