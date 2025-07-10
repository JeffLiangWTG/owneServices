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
	[TestedType(typeof(TG_Maintain_GHH_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GHH_EffectiveEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var holidaySource = new GlbHolidaySource("ABC");
			var holidaySourceHistory = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 1, 1), holidaySource.PK);

			staff.AppendInsertAndReturnObject(sql);
			holidaySource.AppendInsertAndReturnObject(sql);
			holidaySourceHistory.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, holidaySourceHistory.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GHH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var holidaySources = new[]
			{
				new GlbHolidaySource("ABC"),
				new GlbHolidaySource("DEF"),
				new GlbHolidaySource("GHI"),
			};

			var holidaySourceHistory = new[]
			{
				new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 1, 1), holidaySources[0].PK),
				new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 2, 1), holidaySources[1].PK),
				new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 3, 1), holidaySources[2].PK),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(holidaySources, r => r.AppendInsertAndReturnObject(sql));
			Array.ForEach(holidaySourceHistory, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, holidaySourceHistory[0].PK)
				.ExpectEquals("A holiday source's end date should match the proceeding row", l => l.GHH_AutoEffectiveEndDate, holidaySourceHistory[1].GHH_EffectiveDate)
				.VerifyAll();

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, holidaySourceHistory[1].PK)
				.ExpectEquals("A holiday source's end date should match the proceeding row", l => l.GHH_AutoEffectiveEndDate, holidaySourceHistory[2].GHH_EffectiveDate)
				.VerifyAll();

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, holidaySourceHistory[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GHH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var holidaySources = new[]
			{
				new GlbHolidaySource("ABC"),
				new GlbHolidaySource("DEF"),
				new GlbHolidaySource("GHI"),
				new GlbHolidaySource("JKL"),
			};

			var previousGHH = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 1, 1), holidaySources[0].PK);
			var ghhToUpdate = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 2, 1), holidaySources[1].PK);
			var nextGHH = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 3, 1), holidaySources[2].PK);

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(holidaySources, r => r.AppendInsertAndReturnObject(sql));
			Array.ForEach(new[] { previousGHH, ghhToUpdate, nextGHH }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var expectedHolidaySourcePk = holidaySources[3].PK;
			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbHolidaySourceHistory.GHH_GHS_HolidaySource), $"'{expectedHolidaySourcePk}'", expectedHolidaySourcePk),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE dbo.GlbHolidaySourceHistory
					SET
						{column} = {sqlValue}
					WHERE
						GHH_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", ghhToUpdate.PK, GlbHolidaySourceHistorySchema.PK));

				GlbHolidaySourceHistory.AssertFromDB(TestConnection, ghhToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbHolidaySourceHistory.AssertFromDB(TestConnection, previousGHH.PK)
					.ExpectNotEquals($"The update should not affect the previous holiday source ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbHolidaySourceHistory.AssertFromDB(TestConnection, nextGHH.PK)
					.ExpectNotEquals($"The update should not affect the next holiday source ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbHolidaySourceHistory holidaySource)
					=> holidaySource.GetType().GetProperty(column).GetValue(holidaySource);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var holidaySources = new[]
			{
				new GlbHolidaySource("ABC"),
				new GlbHolidaySource("DEF"),
				new GlbHolidaySource("GHI"),
			};

			var previousGHH = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 1, 1), holidaySources[0].PK);
			var ghhToUpdate = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 2, 1), holidaySources[1].PK);
			var nextGHH = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 3, 1), holidaySources[2].PK);
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(holidaySources, r => r.AppendInsertAndReturnObject(sql));
			Array.ForEach(new[] { previousGHH, ghhToUpdate, nextGHH }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbHolidaySourceHistory
				SET
					GHH_EffectiveDate = @newDate
				WHERE
					GHH_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", ghhToUpdate.PK, GlbHolidaySourceHistorySchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbHolidaySourceHistorySchema.GHH_EffectiveDate);
			});

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, ghhToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GHH_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GHH_AutoEffectiveEndDate, nextGHH.GHH_EffectiveDate)
				.VerifyAll();

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, previousGHH.PK)
				.ExpectEquals("The update should not change the previous holiday source's effective date", r => r.GHH_EffectiveDate, previousGHH.GHH_EffectiveDate)
				.ExpectEquals("The update should change the previous holiday source's end date", r => r.GHH_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, nextGHH.PK)
				.ExpectEquals("The update should not affect the next holiday source's effective date", r => r.GHH_EffectiveDate, nextGHH.GHH_EffectiveDate)
				.ExpectEquals("The update should not affect the next holiday source's end date", r => r.GHH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var holidaySources = new[]
			{
				new GlbHolidaySource("ABC"),
				new GlbHolidaySource("DEF"),
				new GlbHolidaySource("GHI"),
			};

			var previousGHH = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 1, 1), holidaySources[0].PK);
			var ghhToUpdate = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 2, 1), holidaySources[1].PK);
			var nextGHH = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 3, 1), holidaySources[2].PK);
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(holidaySources, r => r.AppendInsertAndReturnObject(sql));
			Array.ForEach(new[] { previousGHH, ghhToUpdate, nextGHH }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbHolidaySourceHistory
				SET
					GHH_EffectiveDate = @newDate
				WHERE
					GHH_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", ghhToUpdate.PK, GlbHolidaySourceHistorySchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbHolidaySourceHistorySchema.GHH_EffectiveDate);
			});

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, ghhToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GHH_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GHH_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, previousGHH.PK)
				.ExpectEquals("The update should not change the previous holiday source's effective date", r => r.GHH_EffectiveDate, previousGHH.GHH_EffectiveDate)
				.ExpectEquals("The update should change the previous holiday source's end date to the next row's end date", r => r.GHH_AutoEffectiveEndDate, nextGHH.GHH_EffectiveDate)
				.VerifyAll();

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, nextGHH.PK)
				.ExpectEquals("The update should not affect the next holiday source's effective date", r => r.GHH_EffectiveDate, nextGHH.GHH_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GHH_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleHolidaySources()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[]
			{
				new GlbStaff("XYZ"),
				new GlbStaff("YZX"),
				new GlbStaff("ZXY"),
			};

			var holidaySources = new[]
			{
				new GlbHolidaySource("ABC"),
				new GlbHolidaySource("DEF"),
				new GlbHolidaySource("GHI"),
				new GlbHolidaySource("JKL"),
				new GlbHolidaySource("MLO"),
				new GlbHolidaySource("PQR"),
			};

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));
			TestConnection.ExecuteNonQuery(GlbHolidaySource.GetBulkInsertStatement(holidaySources));
			TestConnection.ExecuteNonQuery(@"
				INSERT INTO dbo.GlbHolidaySourceHistory
					(GHH_PK, GHH_GS_Staff, GHH_EffectiveDate, GHH_GHS_HolidaySource, GHH_SystemCreateTimeUtc, GHH_SystemLastEditTimeUtc, GHH_SystemCreateUser, GHH_SystemLastEditUser)
				VALUES
					(NEWID(), @staff0, '2020-01-01 01:00:00 +01:00', @holidaySource0, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff0, '2020-02-01 11:00:00 +07:00', @holidaySource1, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-03-01 03:00:00 +11:00', @holidaySource2, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-04-01 17:00:00 -03:00', @holidaySource3, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-01-01 09:00:00 +00:00', @holidaySource4, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-04-01 21:00:00 -05:00', @holidaySource5, GETUTCDATE(), GETUTCDATE(), 'E', 'E')
			", p =>
			{
				for (var i = 0; i < staffs.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbHolidaySourceHistorySchema.GHH_GS_Staff);
				}

				for (var i = 0; i < holidaySources.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@holidaySource" + i, holidaySources[i].PK, GlbHolidaySourceHistorySchema.GHH_GHS_HolidaySource);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbHolidaySourceHistory.CountInDB(TestConnection));
			AssertGlbHolidaySourceHistory("First staff has two holiday sources on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertGlbHolidaySourceHistory("Second staff has two holiday sources on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertGlbHolidaySourceHistory("Third staff has two holiday sources split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };
			var holidaySources = new[] { new GlbHolidaySource("ABC"), new GlbHolidaySource("DEF") };

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));
			TestConnection.ExecuteNonQuery(GlbHolidaySource.GetBulkInsertStatement(holidaySources));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GHH_PK", // We won't ever update the primary key of a row
				"GHH_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GHH_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GHH_PK", $"'{pk}'" },
				{ "GHH_AutoVersion", "0" },
				{ "GHH_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GHH_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GHH_GHS_HolidaySource", $"'{holidaySources[0].PK}'" },
				{ "GHH_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GHH_SystemCreateUser", "'XYZ'" },
				{ "GHH_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GHH_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GHH_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GHH_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GHH_GHS_HolidaySource", $"'{holidaySources[1].PK}'", holidaySources[1].PK),
				("GHH_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GHH_SystemCreateUser", "'E'", "E"),
				("GHH_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GHH_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbHolidaySourceHistory");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO dbo.GlbHolidaySourceHistory ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE dbo.GlbHolidaySourceHistory SET {edit.column}={edit.sqlValue} WHERE GHH_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM dbo.GlbHolidaySourceHistory WHERE GHH_PK='{pk}'");
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

			var holidaySources = new[]
			{
				new GlbHolidaySource("ABC"),
				new GlbHolidaySource("DEF"),
				new GlbHolidaySource("GHI"),
			};

			var previousGHH = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 1, 1), holidaySources[0].PK);
			var middleRow = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 2, 1), holidaySources[1].PK);
			var nextGHH = new GlbHolidaySourceHistory(staff.PK, new DateTime(2020, 3, 1), holidaySources[2].PK);

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(holidaySources, r => r.AppendInsertAndReturnObject(sql));
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousGHH.AppendInsertAndReturnObject(sql);
			nextGHH.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GHH_EffectiveDate, middleRow.GHH_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GHH_AutoEffectiveEndDate, nextGHH.GHH_EffectiveDate)
				.VerifyAll();

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, previousGHH.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.GHH_AutoEffectiveEndDate, middleRow.GHH_EffectiveDate)
				.VerifyAll();

			GlbHolidaySourceHistory.AssertFromDB(TestConnection, nextGHH.PK)
				.ExpectEquals("The next rows end date is null", r => r.GHH_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertGlbHolidaySourceHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the holiday source dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GHH_EffectiveDate, GHH_AutoEffectiveEndDate FROM dbo.GlbHolidaySourceHistory WHERE GHH_GS_Staff = '{staff}' ORDER BY GHH_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbHolidaySourceHistory.GHH_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbHolidaySourceHistory.GHH_AutoEffectiveEndDate)];

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

	class GlbHolidaySourceHistory : SQLDataObject<GlbHolidaySourceHistory>
	{
		public GlbHolidaySourceHistory(Guid staff, DateTimeOffset effectiveDate, Guid holidaySource)
		{
			GHH_GS_Staff = staff;
			GHH_EffectiveDate = effectiveDate;

			GHH_GHS_HolidaySource = holidaySource;

			GHH_SystemCreateTimeUtc = DateTime.UtcNow;
			GHH_SystemLastEditTimeUtc = DateTime.UtcNow;

			GHH_SystemCreateUser = "E";
			GHH_SystemLastEditUser = "E";
		}

		public Guid GHH_GS_Staff { get; }
		public DateTimeOffset GHH_EffectiveDate { get; }
		public DateTimeOffset? GHH_AutoEffectiveEndDate { get; }

		public Guid GHH_GHS_HolidaySource { get; }

		public string GHH_SystemCreateUser { get; }
		public string GHH_SystemLastEditUser { get; }
		public DateTime GHH_SystemCreateTimeUtc { get; }
		public DateTime GHH_SystemLastEditTimeUtc { get; }

		protected override ITableDefinition GetSchema() => base.GetSchema()
			.Extend(nameof(GHH_AutoEffectiveEndDate), new DateTimeOffsetColumnInfo(0, isNullable: true));
	}
}
