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
	[TestedType(typeof(TG_Maintain_GSZ_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GSZ_EffectiveEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var timezone = new GlbStaffTimezone(staff.PK, new DateTime(2020, 1, 1));

			staff.AppendInsertAndReturnObject(sql);
			timezone.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffTimezone.AssertFromDB(TestConnection, timezone.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSZ_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var timezones = new[]
			{
				new GlbStaffTimezone(staff.PK, new DateTime(2020, 1, 1)),
				new GlbStaffTimezone(staff.PK, new DateTime(2020, 2, 1)),
				new GlbStaffTimezone(staff.PK, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(timezones, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffTimezone.AssertFromDB(TestConnection, timezones[0].PK)
				.ExpectEquals("A timezones end date should match the proceeding row", l => l.GSZ_AutoEffectiveEndDate, timezones[1].GSZ_EffectiveDate)
				.VerifyAll();

			GlbStaffTimezone.AssertFromDB(TestConnection, timezones[1].PK)
				.ExpectEquals("A timezones end date should match the proceeding row", l => l.GSZ_AutoEffectiveEndDate, timezones[2].GSZ_EffectiveDate)
				.VerifyAll();

			GlbStaffTimezone.AssertFromDB(TestConnection, timezones[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSZ_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSZ = new GlbStaffTimezone(staff.PK, new DateTime(2020, 1, 1));
			var gszToUpdate = new GlbStaffTimezone(staff.PK, new DateTime(2020, 2, 1));
			var nextGSZ = new GlbStaffTimezone(staff.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSZ, gszToUpdate, nextGSZ }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbStaffTimezone.GSZ_R3_NKTimeZoneSetName), "'CCC'", "CCC"),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE dbo.GlbStaffTimezone
					SET
						{column} = {sqlValue}
					WHERE
						GSZ_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", gszToUpdate.PK, GlbStaffTimezoneSchema.PK));

				GlbStaffTimezone.AssertFromDB(TestConnection, gszToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffTimezone.AssertFromDB(TestConnection, previousGSZ.PK)
					.ExpectNotEquals($"The update should not affect the previous timezone ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffTimezone.AssertFromDB(TestConnection, nextGSZ.PK)
					.ExpectNotEquals($"The update should not affect the next timezone ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbStaffTimezone timezone)
					=> timezone.GetType().GetProperty(column).GetValue(timezone);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSZ = new GlbStaffTimezone(staff.PK, new DateTime(2020, 1, 1), "AA1");
			var gszToUpdate = new GlbStaffTimezone(staff.PK, new DateTime(2020, 2, 1), "BB2");
			var nextGSZ = new GlbStaffTimezone(staff.PK, new DateTime(2020, 3, 1), "CC3");
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSZ, gszToUpdate, nextGSZ }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbStaffTimezone
				SET
					GSZ_EffectiveDate = @newDate
				WHERE
					GSZ_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gszToUpdate.PK, GlbStaffTimezoneSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffTimezoneSchema.GSZ_EffectiveDate);
			});

			GlbStaffTimezone.AssertFromDB(TestConnection, gszToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSZ_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GSZ_AutoEffectiveEndDate, nextGSZ.GSZ_EffectiveDate)
				.VerifyAll();

			GlbStaffTimezone.AssertFromDB(TestConnection, previousGSZ.PK)
				.ExpectEquals("The update should not change the previous timezone's effective date", r => r.GSZ_EffectiveDate, previousGSZ.GSZ_EffectiveDate)
				.ExpectEquals("The update should change the previous timezone's end date", r => r.GSZ_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbStaffTimezone.AssertFromDB(TestConnection, nextGSZ.PK)
				.ExpectEquals("The update should not affect the next timezone's effective date", r => r.GSZ_EffectiveDate, nextGSZ.GSZ_EffectiveDate)
				.ExpectEquals("The update should not affect the next timezone's end date", r => r.GSZ_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSZ = new GlbStaffTimezone(staff.PK, new DateTime(2020, 1, 1), "A1A");
			var gszToUpdate = new GlbStaffTimezone(staff.PK, new DateTime(2020, 2, 1), "B2B");
			var nextGSZ = new GlbStaffTimezone(staff.PK, new DateTime(2020, 3, 1), "C3C");
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSZ, gszToUpdate, nextGSZ }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbStaffTimezone
				SET
					GSZ_EffectiveDate = @newDate
				WHERE
					GSZ_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gszToUpdate.PK, GlbStaffTimezoneSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffTimezoneSchema.GSZ_EffectiveDate);
			});

			GlbStaffTimezone.AssertFromDB(TestConnection, gszToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSZ_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GSZ_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbStaffTimezone.AssertFromDB(TestConnection, previousGSZ.PK)
				.ExpectEquals("The update should not change the previous timezone's effective date", r => r.GSZ_EffectiveDate, previousGSZ.GSZ_EffectiveDate)
				.ExpectEquals("The update should change the previous timezone's end date to the next row's end date", r => r.GSZ_AutoEffectiveEndDate, nextGSZ.GSZ_EffectiveDate)
				.VerifyAll();

			GlbStaffTimezone.AssertFromDB(TestConnection, nextGSZ.PK)
				.ExpectEquals("The update should not affect the next timezone's effective date", r => r.GSZ_EffectiveDate, nextGSZ.GSZ_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GSZ_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleStaffTimezones()
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
				INSERT INTO dbo.GlbStaffTimezone
					(GSZ_PK, GSZ_GS_Staff, GSZ_EffectiveDate, GSZ_R3_NKTimeZoneSetName, GSZ_SystemCreateTimeUtc, GSZ_SystemLastEditTimeUtc, GSZ_SystemCreateUser, GSZ_SystemLastEditUser)
				VALUES
					(NEWID(), @staff0, '2020-01-01 01:00:00 +01:00', 'AC1', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff0, '2020-02-01 11:00:00 +07:00', 'AC2', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-03-01 03:00:00 +11:00', 'AC3', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-04-01 17:00:00 -03:00', 'AC4', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-01-01 09:00:00 +00:00', 'AC5', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-04-01 21:00:00 -05:00', 'AC6', GETUTCDATE(), GETUTCDATE(), 'E', 'E')
			", p =>
			{
				for (var i = 0; i < staffs.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbStaffTimezoneSchema.GSZ_GS_Staff);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbStaffTimezone.CountInDB(TestConnection));
			AssertTimezoneDateHistory("First staff has two timezones on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertTimezoneDateHistory("Second staff has two timezones on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertTimezoneDateHistory("Third staff has two timezones split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GSZ_PK", // We won't ever update the primary key of a row
				"GSZ_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GSZ_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GSZ_PK", $"'{pk}'" },
				{ "GSZ_AutoVersion", "0" },
				{ "GSZ_IsValid", "1" },
				{ "GSZ_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GSZ_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GSZ_R3_NKTimeZoneSetName", "'B01'" },
				{ "GSZ_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GSZ_SystemCreateUser", "'XYZ'" },
				{ "GSZ_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GSZ_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GSZ_IsValid", "'1'", true),
				("GSZ_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GSZ_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GSZ_R3_NKTimeZoneSetName", "'B02'", "B02"),
				("GSZ_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GSZ_SystemCreateUser", "'E'", "E"),
				("GSZ_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GSZ_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbStaffTimezone");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO dbo.GlbStaffTimezone ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE dbo.GlbStaffTimezone SET {edit.column}={edit.sqlValue} WHERE GSZ_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM dbo.GlbStaffTimezone WHERE GSZ_PK='{pk}'");
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

			var previousGSZ = new GlbStaffTimezone(staff.PK, new DateTime(2020, 1, 1), "AC1");
			var middleRow = new GlbStaffTimezone(staff.PK, new DateTime(2020, 2, 1), "AC2");
			var nextGSZ = new GlbStaffTimezone(staff.PK, new DateTime(2020, 3, 1), "AC3");

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousGSZ.AppendInsertAndReturnObject(sql);
			nextGSZ.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffTimezone.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GSZ_EffectiveDate, middleRow.GSZ_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GSZ_AutoEffectiveEndDate, nextGSZ.GSZ_EffectiveDate)
				.VerifyAll();

			GlbStaffTimezone.AssertFromDB(TestConnection, previousGSZ.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.GSZ_AutoEffectiveEndDate, middleRow.GSZ_EffectiveDate)
				.VerifyAll();

			GlbStaffTimezone.AssertFromDB(TestConnection, nextGSZ.PK)
				.ExpectEquals("The next rows end date is null", r => r.GSZ_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertTimezoneDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the timezone dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GSZ_EffectiveDate, GSZ_AutoEffectiveEndDate FROM dbo.GlbStaffTimezone WHERE GSZ_GS_Staff = '{staff}' ORDER BY GSZ_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbStaffTimezone.GSZ_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbStaffTimezone.GSZ_AutoEffectiveEndDate)];

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
}
