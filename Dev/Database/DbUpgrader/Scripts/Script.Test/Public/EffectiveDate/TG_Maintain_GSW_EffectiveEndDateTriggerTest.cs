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
	[TestedType(typeof(TG_Maintain_GSW_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GSW_EffectiveEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var workingBasis = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 1, 1));

			staff.AppendInsertAndReturnObject(sql);
			workingBasis.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, workingBasis.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSW_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var workingBases = new[]
			{
				new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 1, 1)),
				new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 2, 1)),
				new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(workingBases, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, workingBases[0].PK)
				.ExpectEquals("A workingBasis' end date should match the proceeding row", l => l.GSW_AutoEffectiveEndDate, workingBases[1].GSW_EffectiveDate)
				.VerifyAll();

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, workingBases[1].PK)
				.ExpectEquals("A workingBasis' end date should match the proceeding row", l => l.GSW_AutoEffectiveEndDate, workingBases[2].GSW_EffectiveDate)
				.VerifyAll();

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, workingBases[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSW_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSW = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 2, 1));
			var nextGSW = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSW, gslToUpdate, nextGSW }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbStaffWorkingBasis.GSW_WorkingBasis), "'CCC'", "CCC"),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE hrm.GlbStaffWorkingBasis
					SET
						{column} = {sqlValue}
					WHERE
						GSW_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbStaffWorkingBasisSchema.PK));

				GlbStaffWorkingBasis.AssertFromDB(TestConnection, gslToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffWorkingBasis.AssertFromDB(TestConnection, previousGSW.PK)
					.ExpectNotEquals($"The update should not affect the previous workingBasis ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffWorkingBasis.AssertFromDB(TestConnection, nextGSW.PK)
					.ExpectNotEquals($"The update should not affect the next workingBasis ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbStaffWorkingBasis workingBasis)
					=> workingBasis.GetType().GetProperty(column).GetValue(workingBasis);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSW = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 1, 1), "AA1");
			var gslToUpdate = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 2, 1), "BB2");
			var nextGSW = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 3, 1), "CC3");
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSW, gslToUpdate, nextGSW }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffWorkingBasis
				SET
					GSW_EffectiveDate = @newDate
				WHERE
					GSW_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbStaffWorkingBasisSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffWorkingBasisSchema.GSW_EffectiveDate);
			});

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSW_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GSW_AutoEffectiveEndDate, nextGSW.GSW_EffectiveDate)
				.VerifyAll();

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, previousGSW.PK)
				.ExpectEquals("The update should not change the previous workingBasis' effective date", r => r.GSW_EffectiveDate, previousGSW.GSW_EffectiveDate)
				.ExpectEquals("The update should change the previous workingBasis' end date", r => r.GSW_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, nextGSW.PK)
				.ExpectEquals("The update should not affect the next workingBasis' effective date", r => r.GSW_EffectiveDate, nextGSW.GSW_EffectiveDate)
				.ExpectEquals("The update should not affect the next workingBasis' end date", r => r.GSW_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSW = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 1, 1), "A1A");
			var gslToUpdate = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 2, 1), "B2B");
			var nextGSW = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 3, 1), "C3C");
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSW, gslToUpdate, nextGSW }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffWorkingBasis
				SET
					GSW_EffectiveDate = @newDate
				WHERE
					GSW_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbStaffWorkingBasisSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffWorkingBasisSchema.GSW_EffectiveDate);
			});

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSW_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GSW_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, previousGSW.PK)
				.ExpectEquals("The update should not change the previous workingbasis' effective date", r => r.GSW_EffectiveDate, previousGSW.GSW_EffectiveDate)
				.ExpectEquals("The update should change the previous workingbasis' end date to the next row's end date", r => r.GSW_AutoEffectiveEndDate, nextGSW.GSW_EffectiveDate)
				.VerifyAll();

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, nextGSW.PK)
				.ExpectEquals("The update should not affect the next workingbasis' effective date", r => r.GSW_EffectiveDate, nextGSW.GSW_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GSW_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleStaffWorkingbases()
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
				INSERT INTO hrm.GlbStaffWorkingBasis
					(GSW_PK, GSW_GS_Staff, GSW_EffectiveDate, GSW_WorkingBasis, GSW_SystemCreateTimeUtc, GSW_SystemLastEditTimeUtc, GSW_SystemCreateUser, GSW_SystemLastEditUser)
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
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbStaffWorkingBasisSchema.GSW_GS_Staff);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbStaffWorkingBasis.CountInDB(TestConnection));
			AssertWorkingBasisDateHistory("First staff has two workingBases on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertWorkingBasisDateHistory("Second staff has two workingBases on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertWorkingBasisDateHistory("Third staff has two workingBases split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GSW_PK", // We won't ever update the primary key of a row
				"GSW_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GSW_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GSW_PK", $"'{pk}'" },
				{ "GSW_AutoVersion", "0" },
				{ "GSW_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GSW_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GSW_WorkingBasis", "'B01'" },
				{ "GSW_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GSW_SystemCreateUser", "'XYZ'" },
				{ "GSW_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GSW_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GSW_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GSW_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GSW_WorkingBasis", "'B02'", "B02"),
				("GSW_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GSW_SystemCreateUser", "'E'", "E"),
				("GSW_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GSW_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbStaffWorkingBasis");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO hrm.GlbStaffWorkingBasis ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE hrm.GlbStaffWorkingBasis SET {edit.column}={edit.sqlValue} WHERE GSW_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM hrm.GlbStaffWorkingBasis WHERE GSW_PK='{pk}'");
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

			var previousGSW = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 1, 1), "AC1");
			var middleRow = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 2, 1), "AC2");
			var nextGSW = new GlbStaffWorkingBasis(staff.PK, new DateTime(2020, 3, 1), "AC3");

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousGSW.AppendInsertAndReturnObject(sql);
			nextGSW.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GSW_EffectiveDate, middleRow.GSW_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GSW_AutoEffectiveEndDate, nextGSW.GSW_EffectiveDate)
				.VerifyAll();

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, previousGSW.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.GSW_AutoEffectiveEndDate, middleRow.GSW_EffectiveDate)
				.VerifyAll();

			GlbStaffWorkingBasis.AssertFromDB(TestConnection, nextGSW.PK)
				.ExpectEquals("The next rows end date is null", r => r.GSW_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertWorkingBasisDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the workingBasis dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GSW_EffectiveDate, GSW_AutoEffectiveEndDate FROM hrm.GlbStaffWorkingBasis WHERE GSW_GS_Staff = '{staff}' ORDER BY GSW_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbStaffWorkingBasis.GSW_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbStaffWorkingBasis.GSW_AutoEffectiveEndDate)];

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

	class GlbStaffWorkingBasis : SQLDataObject<GlbStaffWorkingBasis>
	{
		public GlbStaffWorkingBasis(Guid staff, DateTimeOffset effectiveDate, string workingBasis = "ABC")
		{
			GSW_GS_Staff = staff;
			GSW_EffectiveDate = effectiveDate;

			GSW_WorkingBasis = workingBasis;

			GSW_SystemCreateTimeUtc = DateTime.UtcNow;
			GSW_SystemLastEditTimeUtc = DateTime.UtcNow;

			GSW_SystemCreateUser = "E";
			GSW_SystemLastEditUser = "E";
		}

		public Guid GSW_GS_Staff { get; }
		public DateTimeOffset GSW_EffectiveDate { get; }
		public DateTimeOffset? GSW_AutoEffectiveEndDate { get; }

		public string GSW_WorkingBasis { get; }

		public string GSW_SystemCreateUser { get; }
		public string GSW_SystemLastEditUser { get; }
		public DateTime GSW_SystemCreateTimeUtc { get; }
		public DateTime GSW_SystemLastEditTimeUtc { get; }

		protected override ITableDefinition GetSchema() => base.GetSchema()
			.Extend(nameof(GSW_AutoEffectiveEndDate), new DateTimeOffsetColumnInfo(0, isNullable: true));
	}
}
