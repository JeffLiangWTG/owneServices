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
	[TestedType(typeof(TG_Maintain_GEL_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GEL_EffectiveEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var location = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 1, 1));

			staff.AppendInsertAndReturnObject(sql);
			location.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentLocation.AssertFromDB(TestConnection, location.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GEL_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var locations = new[]
			{
				new GlbEmploymentLocation(staff.PK, new DateTime(2020, 1, 1)),
				new GlbEmploymentLocation(staff.PK, new DateTime(2020, 2, 1)),
				new GlbEmploymentLocation(staff.PK, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(locations, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentLocation.AssertFromDB(TestConnection, locations[0].PK)
				.ExpectEquals("A location's end date should match the proceeding row", l => l.GEL_AutoEffectiveEndDate, locations[1].GEL_EffectiveDate)
				.VerifyAll();

			GlbEmploymentLocation.AssertFromDB(TestConnection, locations[1].PK)
				.ExpectEquals("A location's end date should match the proceeding row", l => l.GEL_AutoEffectiveEndDate, locations[2].GEL_EffectiveDate)
				.VerifyAll();

			GlbEmploymentLocation.AssertFromDB(TestConnection, locations[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GEL_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGEL = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 2, 1));
			var nextGEL = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGEL, gslToUpdate, nextGEL }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbEmploymentLocation.GEL_LocationSource), "'WFH'", "WFH"),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE dbo.GlbEmploymentLocation
					SET
						{column} = {sqlValue}
					WHERE
						GEL_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbEmploymentLocationSchema.PK));

				GlbEmploymentLocation.AssertFromDB(TestConnection, gslToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbEmploymentLocation.AssertFromDB(TestConnection, previousGEL.PK)
					.ExpectNotEquals($"The update should not affect the previous location ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbEmploymentLocation.AssertFromDB(TestConnection, nextGEL.PK)
					.ExpectNotEquals($"The update should not affect the next location ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbEmploymentLocation location)
					=> location.GetType().GetProperty(column).GetValue(location);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGEL = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 1, 1), "OTH");
			var gslToUpdate = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 2, 1), "WFH");
			var nextGEL = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 3, 1), "OTH");
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGEL, gslToUpdate, nextGEL }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentLocation
				SET
					GEL_EffectiveDate = @newDate
				WHERE
					GEL_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbEmploymentLocationSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentLocationSchema.GEL_EffectiveDate);
			});

			GlbEmploymentLocation.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GEL_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GEL_AutoEffectiveEndDate, nextGEL.GEL_EffectiveDate)
				.VerifyAll();

			GlbEmploymentLocation.AssertFromDB(TestConnection, previousGEL.PK)
				.ExpectEquals("The update should not change the previous location's effective date", r => r.GEL_EffectiveDate, previousGEL.GEL_EffectiveDate)
				.ExpectEquals("The update should change the previous location's end date", r => r.GEL_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbEmploymentLocation.AssertFromDB(TestConnection, nextGEL.PK)
				.ExpectEquals("The update should not affect the next location's effective date", r => r.GEL_EffectiveDate, nextGEL.GEL_EffectiveDate)
				.ExpectEquals("The update should not affect the next location's end date", r => r.GEL_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGEL = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 1, 1), "OTH");
			var gslToUpdate = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 2, 1), "WFH");
			var nextGEL = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 3, 1), "OTH");
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGEL, gslToUpdate, nextGEL }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbEmploymentLocation
				SET
					GEL_EffectiveDate = @newDate
				WHERE
					GEL_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbEmploymentLocationSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbEmploymentLocationSchema.GEL_EffectiveDate);
			});

			GlbEmploymentLocation.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GEL_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GEL_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbEmploymentLocation.AssertFromDB(TestConnection, previousGEL.PK)
				.ExpectEquals("The update should not change the previous location's effective date", r => r.GEL_EffectiveDate, previousGEL.GEL_EffectiveDate)
				.ExpectEquals("The update should change the previous location's end date to the next row's end date", r => r.GEL_AutoEffectiveEndDate, nextGEL.GEL_EffectiveDate)
				.VerifyAll();

			GlbEmploymentLocation.AssertFromDB(TestConnection, nextGEL.PK)
				.ExpectEquals("The update should not affect the next location's effective date", r => r.GEL_EffectiveDate, nextGEL.GEL_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GEL_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleGlbEmploymentLocations()
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
				INSERT INTO dbo.GlbEmploymentLocation
					(GEL_PK, GEL_GS_Staff, GEL_EffectiveDate, GEL_LocationSource, GEL_SystemCreateTimeUtc, GEL_SystemLastEditTimeUtc, GEL_SystemCreateUser, GEL_SystemLastEditUser)
				VALUES
					(NEWID(), @staff0, '2020-01-01 01:00:00 +01:00', 'OTH', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff0, '2020-02-01 11:00:00 +07:00', 'WFH', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-03-01 03:00:00 +11:00', 'OTH', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-04-01 17:00:00 -03:00', 'WFH', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-01-01 09:00:00 +00:00', 'OTH', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-04-01 21:00:00 -05:00', 'WFH', GETUTCDATE(), GETUTCDATE(), 'E', 'E')
			", p =>
			{
				for (var i = 0; i < staffs.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbEmploymentLocationSchema.GEL_GS_Staff);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbEmploymentLocation.CountInDB(TestConnection));
			AssertLocationDateHistory("First staff has two locations on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertLocationDateHistory("Second staff has two locations on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertLocationDateHistory("Third staff has two locations split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GEL_PK", // We won't ever update the primary key of a row
				"GEL_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GEL_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GEL_PK", $"'{pk}'" },
				{ "GEL_AutoVersion", "0" },
				{ "GEL_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GEL_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GEL_LocationSource", "'OTH'" },
				{ "GEL_Address1", "'ADDRESS ONE'" },
				{ "GEL_Address2", "'ADDRESS TWO'" },
				{ "GEL_City", "'CITY A'" },
				{ "GEL_State", "'STATE 01'" },
				{ "GEL_PostCode", "'2020'" },
				{ "GEL_RN_NKCountryCode", "'AU'" },
				{ "GEL_ValidationStatus", "'CNA'" },
				{ "GEL_GB_SourceBranch", "null" },
				{ "GEL_OA_SourceOrgAddress", "null" },
				{ "GEL_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GEL_SystemCreateUser", "'XYZ'" },
				{ "GEL_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GEL_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GEL_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GEL_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GEL_LocationSource", "'WFH'", "WFH"),
				("GEL_Address1", "'ADDRESS ONE'", "ADDRESS ONE" ),
				("GEL_Address2", "'ADDRESS TWO'", "ADDRESS TWO" ),
				("GEL_City", "'CITY A'", "CITY A" ),
				("GEL_State", "'STATE 01'", "STATE 01" ),
				("GEL_PostCode", "'2020'", "2020"),
				("GEL_RN_NKCountryCode", "'AU'", "AU"),
				("GEL_ValidationStatus", "'CNA'", "CNA"),
				("GEL_GB_SourceBranch", "null", DBNull.Value),
				("GEL_OA_SourceOrgAddress", "null", DBNull.Value),
				("GEL_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GEL_SystemCreateUser", "'E'", "E"),
				("GEL_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GEL_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbEmploymentLocation");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO dbo.GlbEmploymentLocation ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE dbo.GlbEmploymentLocation SET {edit.column}={edit.sqlValue} WHERE GEL_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM dbo.GlbEmploymentLocation WHERE GEL_PK='{pk}'");
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

			var previousGEL = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 1, 1), "OTH");
			var middleRow = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 2, 1), "WFH");
			var nextGEL = new GlbEmploymentLocation(staff.PK, new DateTime(2020, 3, 1), "OTH");

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousGEL.AppendInsertAndReturnObject(sql);
			nextGEL.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbEmploymentLocation.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GEL_EffectiveDate, middleRow.GEL_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GEL_AutoEffectiveEndDate, nextGEL.GEL_EffectiveDate)
				.VerifyAll();

			GlbEmploymentLocation.AssertFromDB(TestConnection, previousGEL.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.GEL_AutoEffectiveEndDate, middleRow.GEL_EffectiveDate)
				.VerifyAll();

			GlbEmploymentLocation.AssertFromDB(TestConnection, nextGEL.PK)
				.ExpectEquals("The next rows end date is null", r => r.GEL_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertLocationDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the location dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GEL_EffectiveDate, GEL_AutoEffectiveEndDate FROM dbo.GlbEmploymentLocation WHERE GEL_GS_Staff = '{staff}' ORDER BY GEL_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbEmploymentLocation.GEL_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbEmploymentLocation.GEL_AutoEffectiveEndDate)];

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

	class GlbEmploymentLocation : SQLDataObject<GlbEmploymentLocation>
	{
		public GlbEmploymentLocation(Guid staff, DateTimeOffset effectiveDate, string location = "OTH")
		{
			GEL_GS_Staff = staff;
			GEL_EffectiveDate = effectiveDate;

			GEL_LocationSource = location;

			GEL_SystemCreateTimeUtc = DateTime.UtcNow;
			GEL_SystemLastEditTimeUtc = DateTime.UtcNow;

			GEL_SystemCreateUser = "E";
			GEL_SystemLastEditUser = "E";
		}

		public Guid GEL_GS_Staff { get; }
		public DateTimeOffset GEL_EffectiveDate { get; }
		public DateTimeOffset? GEL_AutoEffectiveEndDate { get; }

		public string GEL_LocationSource { get; }

		public string GEL_SystemCreateUser { get; }
		public string GEL_SystemLastEditUser { get; }
		public DateTime GEL_SystemCreateTimeUtc { get; }
		public DateTime GEL_SystemLastEditTimeUtc { get; }

		protected override ITableDefinition GetSchema() => base.GetSchema()
			.Extend(nameof(GEL_AutoEffectiveEndDate), new DateTimeOffsetColumnInfo(0, isNullable: true));
	}
}
