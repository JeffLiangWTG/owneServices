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
	[TestedType(typeof(TG_Maintain_GSL_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GSL_EffectiveEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var classification = new GlbStaffClassification(staff.PK, new DateTime(2020, 1, 1));

			staff.AppendInsertAndReturnObject(sql);
			classification.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffClassification.AssertFromDB(TestConnection, classification.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSL_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var classifications = new[]
			{
				new GlbStaffClassification(staff.PK, new DateTime(2020, 1, 1)),
				new GlbStaffClassification(staff.PK, new DateTime(2020, 2, 1)),
				new GlbStaffClassification(staff.PK, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(classifications, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffClassification.AssertFromDB(TestConnection, classifications[0].PK)
				.ExpectEquals("A classifications end date should match the proceeding row", l => l.GSL_AutoEffectiveEndDate, classifications[1].GSL_EffectiveDate)
				.VerifyAll();

			GlbStaffClassification.AssertFromDB(TestConnection, classifications[1].PK)
				.ExpectEquals("A classifications end date should match the proceeding row", l => l.GSL_AutoEffectiveEndDate, classifications[2].GSL_EffectiveDate)
				.VerifyAll();

			GlbStaffClassification.AssertFromDB(TestConnection, classifications[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSL_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSL = new GlbStaffClassification(staff.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbStaffClassification(staff.PK, new DateTime(2020, 2, 1));
			var nextGSL = new GlbStaffClassification(staff.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSL, gslToUpdate, nextGSL }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbStaffClassification.GSL_Classification), "'CCC'", "CCC"),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE hrm.GlbStaffClassification
					SET
						{column} = {sqlValue}
					WHERE
						GSL_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbStaffClassificationSchema.PK));

				GlbStaffClassification.AssertFromDB(TestConnection, gslToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffClassification.AssertFromDB(TestConnection, previousGSL.PK)
					.ExpectNotEquals($"The update should not affect the previous classification ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffClassification.AssertFromDB(TestConnection, nextGSL.PK)
					.ExpectNotEquals($"The update should not affect the next classification ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbStaffClassification classification)
					=> classification.GetType().GetProperty(column).GetValue(classification);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSL = new GlbStaffClassification(staff.PK, new DateTime(2020, 1, 1), "AA1");
			var gslToUpdate = new GlbStaffClassification(staff.PK, new DateTime(2020, 2, 1), "BB2");
			var nextGSL = new GlbStaffClassification(staff.PK, new DateTime(2020, 3, 1), "CC3");
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSL, gslToUpdate, nextGSL }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffClassification
				SET
					GSL_EffectiveDate = @newDate
				WHERE
					GSL_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbStaffClassificationSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffClassificationSchema.GSL_EffectiveDate);
			});

			GlbStaffClassification.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSL_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GSL_AutoEffectiveEndDate, nextGSL.GSL_EffectiveDate)
				.VerifyAll();

			GlbStaffClassification.AssertFromDB(TestConnection, previousGSL.PK)
				.ExpectEquals("The update should not change the previous classification's effective date", r => r.GSL_EffectiveDate, previousGSL.GSL_EffectiveDate)
				.ExpectEquals("The update should change the previous classification's end date", r => r.GSL_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbStaffClassification.AssertFromDB(TestConnection, nextGSL.PK)
				.ExpectEquals("The update should not affect the next classification's effective date", r => r.GSL_EffectiveDate, nextGSL.GSL_EffectiveDate)
				.ExpectEquals("The update should not affect the next classification's end date", r => r.GSL_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSL = new GlbStaffClassification(staff.PK, new DateTime(2020, 1, 1), "A1A");
			var gslToUpdate = new GlbStaffClassification(staff.PK, new DateTime(2020, 2, 1), "B2B");
			var nextGSL = new GlbStaffClassification(staff.PK, new DateTime(2020, 3, 1), "C3C");
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSL, gslToUpdate, nextGSL }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffClassification
				SET
					GSL_EffectiveDate = @newDate
				WHERE
					GSL_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbStaffClassificationSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffClassificationSchema.GSL_EffectiveDate);
			});

			GlbStaffClassification.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSL_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GSL_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbStaffClassification.AssertFromDB(TestConnection, previousGSL.PK)
				.ExpectEquals("The update should not change the previous classification's effective date", r => r.GSL_EffectiveDate, previousGSL.GSL_EffectiveDate)
				.ExpectEquals("The update should change the previous classification's end date to the next row's end date", r => r.GSL_AutoEffectiveEndDate, nextGSL.GSL_EffectiveDate)
				.VerifyAll();

			GlbStaffClassification.AssertFromDB(TestConnection, nextGSL.PK)
				.ExpectEquals("The update should not affect the next classification's effective date", r => r.GSL_EffectiveDate, nextGSL.GSL_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GSL_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleStaffClassifications()
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
				INSERT INTO hrm.GlbStaffClassification
					(GSL_PK, GSL_GS_Staff, GSL_EffectiveDate, GSL_Classification, GSL_SystemCreateTimeUtc, GSL_SystemLastEditTimeUtc, GSL_SystemCreateUser, GSL_SystemLastEditUser)
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
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbStaffClassificationSchema.GSL_GS_Staff);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbStaffClassification.CountInDB(TestConnection));
			AssertClassificationDateHistory("First staff has two classifications on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertClassificationDateHistory("Second staff has two classifications on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertClassificationDateHistory("Third staff has two classifications split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GSL_PK", // We won't ever update the primary key of a row
				"GSL_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GSL_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GSL_PK", $"'{pk}'" },
				{ "GSL_AutoVersion", "0" },
				{ "GSL_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GSL_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GSL_Classification", "'B01'" },
				{ "GSL_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GSL_SystemCreateUser", "'XYZ'" },
				{ "GSL_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GSL_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GSL_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GSL_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GSL_Classification", "'B02'", "B02"),
				("GSL_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GSL_SystemCreateUser", "'E'", "E"),
				("GSL_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GSL_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbStaffClassification");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO hrm.GlbStaffClassification ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE hrm.GlbStaffClassification SET {edit.column}={edit.sqlValue} WHERE GSL_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM hrm.GlbStaffClassification WHERE GSL_PK='{pk}'");
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

			var previousGSL = new GlbStaffClassification(staff.PK, new DateTime(2020, 1, 1), "AC1");
			var middleRow = new GlbStaffClassification(staff.PK, new DateTime(2020, 2, 1), "AC2");
			var nextGSL = new GlbStaffClassification(staff.PK, new DateTime(2020, 3, 1), "AC3");

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousGSL.AppendInsertAndReturnObject(sql);
			nextGSL.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffClassification.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GSL_EffectiveDate, middleRow.GSL_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GSL_AutoEffectiveEndDate, nextGSL.GSL_EffectiveDate)
				.VerifyAll();

			GlbStaffClassification.AssertFromDB(TestConnection, previousGSL.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.GSL_AutoEffectiveEndDate, middleRow.GSL_EffectiveDate)
				.VerifyAll();

			GlbStaffClassification.AssertFromDB(TestConnection, nextGSL.PK)
				.ExpectEquals("The next rows end date is null", r => r.GSL_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertClassificationDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the classification dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GSL_EffectiveDate, GSL_AutoEffectiveEndDate FROM hrm.GlbStaffClassification WHERE GSL_GS_Staff = '{staff}' ORDER BY GSL_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbStaffClassification.GSL_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbStaffClassification.GSL_AutoEffectiveEndDate)];

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

	class GlbStaffClassification : SQLDataObject<GlbStaffClassification>
	{
		public GlbStaffClassification(Guid staff, DateTimeOffset effectiveDate, string classification = "ABC")
		{
			GSL_GS_Staff = staff;
			GSL_EffectiveDate = effectiveDate;

			GSL_Classification = classification;

			GSL_SystemCreateTimeUtc = DateTime.UtcNow;
			GSL_SystemLastEditTimeUtc = DateTime.UtcNow;

			GSL_SystemCreateUser = "E";
			GSL_SystemLastEditUser = "E";
		}

		public Guid GSL_GS_Staff { get; }
		public DateTimeOffset GSL_EffectiveDate { get; }
		public DateTimeOffset? GSL_AutoEffectiveEndDate { get; }

		public string GSL_Classification { get; }

		public string GSL_SystemCreateUser { get; }
		public string GSL_SystemLastEditUser { get; }
		public DateTime GSL_SystemCreateTimeUtc { get; }
		public DateTime GSL_SystemLastEditTimeUtc { get; }

		protected override ITableDefinition GetSchema() => base.GetSchema()
			.Extend(nameof(GSL_AutoEffectiveEndDate), new DateTimeOffsetColumnInfo(0, isNullable: true));
	}
}
