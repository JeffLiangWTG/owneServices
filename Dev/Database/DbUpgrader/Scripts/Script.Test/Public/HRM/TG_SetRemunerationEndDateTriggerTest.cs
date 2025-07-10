using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.HRM
{
	[TestedType(typeof(TG_SetRemunerationEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_SetRemunerationEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var remuneration = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 1, 1));

			staff.AppendInsertAndReturnObject(sql);
			remuneration.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffRemuneration.AssertFromDB(TestConnection, remuneration.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSR_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var remunerations = new[]
			{
				new GlbStaffRemuneration(staff.PK, new DateTime(2020, 1, 1)),
				new GlbStaffRemuneration(staff.PK, new DateTime(2020, 2, 1)),
				new GlbStaffRemuneration(staff.PK, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(remunerations, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffRemuneration.AssertFromDB(TestConnection, remunerations[0].PK)
				.ExpectEquals("A remunerations end date should match the proceeding row", l => l.GSR_AutoEffectiveEndDate, remunerations[1].GSR_EffectiveDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, remunerations[1].PK)
				.ExpectEquals("A remunerations end date should match the proceeding row", l => l.GSR_AutoEffectiveEndDate, remunerations[2].GSR_EffectiveDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, remunerations[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSR_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousRem = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 1, 1));
			var remToUpdate = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 2, 1));
			var nextRem = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new [] { previousRem, remToUpdate, nextRem }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbStaffRemuneration.GSR_RN_NKCountry), "'NZ'", "NZ"),
				(nameof(GlbStaffRemuneration.GSR_RX_NKCurrency), "'USD'", "USD"),
				(nameof(GlbStaffRemuneration.GSR_FullTimeEquivalent), "0.5", 0.5m)
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE hrm.GlbStaffRemuneration
					SET
						{column} = {sqlValue}
					WHERE
						GSR_PK = @remPk
					", p => p.AddParameterBasedOnDbColumn("@remPk", remToUpdate.PK, GlbStaffRemunerationSchema.PK));

				GlbStaffRemuneration.AssertFromDB(TestConnection, remToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffRemuneration.AssertFromDB(TestConnection, previousRem.PK)
					.ExpectNotEquals($"The update should not affect the previous remuneration ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffRemuneration.AssertFromDB(TestConnection, nextRem.PK)
					.ExpectNotEquals($"The update should not affect the next remuneration ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbStaffRemuneration remuneration)
					=> remuneration.GetType().GetProperty(column).GetValue(remuneration);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousRem = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 1, 1), fte: 1m);
			var remToUpdate = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 2, 1), fte: 1m);
			var nextRem = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 3, 1), fte: 1m);
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousRem, remToUpdate, nextRem }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffRemuneration
				SET
					GSR_EffectiveDate = @newDate
				WHERE
					GSR_PK = @remPk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@remPk", remToUpdate.PK, GlbStaffRemunerationSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffRemunerationSchema.GSR_EffectiveDate);
			});

			GlbStaffRemuneration.AssertFromDB(TestConnection, remToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSR_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GSR_AutoEffectiveEndDate, nextRem.GSR_EffectiveDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, previousRem.PK)
				.ExpectEquals("The update should not change the previous remuneration's effective date", r => r.GSR_EffectiveDate, previousRem.GSR_EffectiveDate)
				.ExpectEquals("The update should change the previous remuneration's end date", r => r.GSR_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, nextRem.PK)
				.ExpectEquals("The update should not affect the next remuneration's effective date", r => r.GSR_EffectiveDate, nextRem.GSR_EffectiveDate)
				.ExpectEquals("The update should not affect the next remuneration's end date", r => r.GSR_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateEffectiveDateOnExistingRow_WithEntitlement()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var remuneration = new GlbStaffRemuneration(staff.PK, DateTime.Today.AddYears(-2), fte: 1m);
			var entitlement = new GlbStaffEntitlement(remuneration.PK, DateTime.Today.AddYears(-1));

			staff.AppendInsertAndReturnObject(sql);
			remuneration.AppendInsertAndReturnObject(sql);
			entitlement.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffRemuneration
				SET
					GSR_EffectiveDate = @newDate
				WHERE
					GSR_PK = @remPk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@remPk", remuneration.PK, GlbStaffRemunerationSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", DateTime.Today, GlbStaffRemunerationSchema.GSR_EffectiveDate);
			});

			GlbStaffEntitlement.AssertFromDB(TestConnection, entitlement.PK)
				.ExpectNotNull("The update should not remove the remuneration's entitlement", r => r)
				.VerifyAll();
		}

		public void TestUpdateEffectiveDateOnExistingRow_SameValue()
		{
			var date = DateTime.Today.AddYears(-1);
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var remuneration = new GlbStaffRemuneration(staff.PK, date, fte: 1m);

			staff.AppendInsertAndReturnObject(sql);
			remuneration.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffRemuneration
				SET
					GSR_EffectiveDate = @newDate
				WHERE
					GSR_PK = @remPk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@remPk", remuneration.PK, GlbStaffRemunerationSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", date, GlbStaffRemunerationSchema.GSR_EffectiveDate);
			});

			GlbStaffRemuneration.AssertFromDB(TestConnection, remuneration.PK)
				.ExpectNotNull("The update should not remove the remuneration", r => r)
				.ExpectEquals("The update should not changed remuneration's effective date", r => r.GSR_EffectiveDate, remuneration.GSR_EffectiveDate)
				.VerifyAll();
		}

		public void TestUpdateEffectiveDateOnExistingRow_MultipleRows()
		{
			// Add four rem rows r1, r2, r3, r4 with effective dates d1, d2, d3, d4 such that d1 < d2 < d3 < d4
			// Update r2's effective date to a value between d3 & d4
			// Assert that r1's end date is now d3, and that r3's end date is r2's new effective date

			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var remuneration1 = new GlbStaffRemuneration(staff.PK, DateTime.Today.AddYears(-4), fte: 1m);
			var remuneration2 = new GlbStaffRemuneration(staff.PK, DateTime.Today.AddYears(-3), fte: 1m);
			var remuneration3 = new GlbStaffRemuneration(staff.PK, DateTime.Today.AddYears(-2), fte: 1m);
			var remuneration4 = new GlbStaffRemuneration(staff.PK, DateTime.Today.AddYears(-1), fte: 1m);

			var newDate = remuneration3.GSR_EffectiveDate.AddMonths(2);

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { remuneration1, remuneration2, remuneration3, remuneration4 }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffRemuneration
				SET
					GSR_EffectiveDate = @newDate
				WHERE
					GSR_PK = @remPk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@remPk", remuneration2.PK, GlbStaffRemunerationSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffRemunerationSchema.GSR_EffectiveDate);
			});

			GlbStaffRemuneration.AssertFromDB(TestConnection, remuneration1.PK)
				.ExpectEquals("The rem 1 still has the same effective date", r => r.GSR_EffectiveDate, remuneration1.GSR_EffectiveDate)
				.ExpectEquals("The rem 1 has the effective end date as rem 3's effective date", r => r.GSR_AutoEffectiveEndDate, remuneration3.GSR_EffectiveDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, remuneration3.PK)
				.ExpectEquals("The rem 3 still has the same effective date", r => r.GSR_EffectiveDate, remuneration3.GSR_EffectiveDate)
				.ExpectEquals("The rem 3 has the effective end date as rem 2's effective date", r => r.GSR_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, remuneration2.PK)
				.ExpectEquals("The rem 2's effective date is updated", r => r.GSR_EffectiveDate, newDate)
				.ExpectEquals("The rem 2 has the effective end date as rem 4's effective date", r => r.GSR_AutoEffectiveEndDate, remuneration4.GSR_EffectiveDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, remuneration4.PK)
				.ExpectEquals("The rem 4 still has the same effective date", r => r.GSR_EffectiveDate, remuneration4.GSR_EffectiveDate)
				.ExpectEquals("The rem 4 has the effective end date as null", r => r.GSR_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousRem = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 1, 1), fte: 1m);
			var remToUpdate = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 2, 1), fte: 1m);
			var nextRem = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 3, 1), fte: 1m);
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousRem, remToUpdate, nextRem }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffRemuneration
				SET
					GSR_EffectiveDate = @newDate
				WHERE
					GSR_PK = @remPk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@remPk", remToUpdate.PK, GlbStaffRemunerationSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffRemunerationSchema.GSR_EffectiveDate);
			});

			GlbStaffRemuneration.AssertFromDB(TestConnection, remToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSR_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GSR_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, previousRem.PK)
				.ExpectEquals("The update should not change the previous remuneration's effective date", r => r.GSR_EffectiveDate, previousRem.GSR_EffectiveDate)
				.ExpectEquals("The update should change the previous remuneration's end date to the next row's end date", r => r.GSR_AutoEffectiveEndDate, nextRem.GSR_EffectiveDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, nextRem.PK)
				.ExpectEquals("The update should not affect the next remuneration's effective date", r => r.GSR_EffectiveDate, nextRem.GSR_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GSR_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleStaffRemunerations()
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
				INSERT INTO hrm.GlbStaffRemuneration
					(GSR_PK, GSR_GS_Staff, GSR_EffectiveDate, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_FullTimeEquivalent, GSR_LeaveLiabilityHourlyRate, GSR_SystemCreateTimeUtc, GSR_SystemLastEditTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditUser)
				VALUES
					(NEWID(), @staff0, '2020-01-01 01:00:00 +01:00', 'AU', 'AUD', 1, '20', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff0, '2020-02-01 11:00:00 +07:00', 'AU', 'AUD', 1, '20', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-03-01 03:00:00 +11:00', 'AU', 'AUD', 1, '20', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-04-01 17:00:00 -03:00', 'AU', 'AUD', 1, '20', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-01-01 09:00:00 +00:00', 'AU', 'AUD', 1, '20', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-04-01 21:00:00 -05:00', 'AU', 'AUD', 1, '20', GETUTCDATE(), GETUTCDATE(), 'E', 'E')
			", p =>
			{
				for (var i = 0; i < staffs.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbStaffRemunerationSchema.GSR_GS_Staff);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbStaffRemuneration.CountInDB(TestConnection));
			AssertRemunerationDateHistory("First staff has two remunerations on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertRemunerationDateHistory("Second staff has two remunerations on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020,4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertRemunerationDateHistory("Third staff has two remunerations split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GSR_PK", // We won't ever update the primary key of a row
				"GSR_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GSR_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GSR_PK", $"'{pk}'" },
				{ "GSR_AutoVersion", "0" },
				{ "GSR_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GSR_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GSR_FullTimeEquivalent", "1" },
				{ "GSR_RN_NKCountry", "'AU'" },
				{ "GSR_RX_NKCurrency", "'AUD'" },
				{ "GSR_LeaveLiabilityHourlyRate", "25" },
				{ "GSR_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GSR_SystemCreateUser", "'XYZ'" },
				{ "GSR_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GSR_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GSR_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GSR_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GSR_FullTimeEquivalent", "0.5", 0.5m),
				("GSR_RN_NKCountry", "'US'", "US"),
				("GSR_RX_NKCurrency", "'USD'", "USD"),
				("GSR_LeaveLiabilityHourlyRate", "20", 20m),
				("GSR_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GSR_SystemCreateUser", "'E'", "E"),
				("GSR_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GSR_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbStaffRemuneration");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO hrm.GlbStaffRemuneration ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE hrm.GlbStaffRemuneration SET {edit.column}={edit.sqlValue} WHERE GSR_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM hrm.GlbStaffRemuneration WHERE GSR_PK='{pk}'");
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

			var previousRem = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 1, 1), fte: 1m);
			var middleRow = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 2, 1), fte: 1m);
			var nextRem = new GlbStaffRemuneration(staff.PK, new DateTime(2020, 3, 1), fte: 1m);

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousRem.AppendInsertAndReturnObject(sql);
			nextRem.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffRemuneration.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GSR_EffectiveDate, middleRow.GSR_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GSR_AutoEffectiveEndDate, nextRem.GSR_EffectiveDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, previousRem.PK)
				.ExpectEquals("The previous row has its end date set to the value in schema", r => r.GSR_AutoEffectiveEndDate, middleRow.GSR_EffectiveDate)
				.VerifyAll();

			GlbStaffRemuneration.AssertFromDB(TestConnection, nextRem.PK)
				.ExpectEquals("The next rows end date is smalldatetime max value, as it is the most recent", r => r.GSR_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertRemunerationDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the remuneration dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GSR_EffectiveDate, GSR_AutoEffectiveEndDate FROM hrm.GlbStaffRemuneration WHERE GSR_GS_Staff = '{staff}' ORDER BY GSR_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbStaffRemuneration.GSR_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbStaffRemuneration.GSR_AutoEffectiveEndDate)];

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

	class GlbStaffEntitlement : SQLDataObject<GlbStaffEntitlement>
	{
		public GlbStaffEntitlement(Guid remuneration, DateTime grantdate,
			string code = "BAS", decimal value = 1, string comment = "dummy comment", string frequency = "YRL", bool isFTEScalable =  true, bool isPartOfPackage = true)
		{
			GSI_GSR_Remuneration = remuneration;
			GSI_EntitlementCode = code;
			GSI_Value = value;
			GSI_Comment = comment;
			GSI_Frequency = frequency;
			GSI_GrantDate = grantdate;

			GSI_IsFTEScalable = isFTEScalable;
			GSI_IsPartOfPackage = isPartOfPackage;

			GSI_SystemCreateTimeUtc = DateTime.UtcNow;
			GSI_SystemLastEditTimeUtc = DateTime.UtcNow;

			GSI_SystemCreateUser = "E";
			GSI_SystemLastEditUser = "E";
		}

		public Guid GSI_GSR_Remuneration { get; }
		public string GSI_EntitlementCode { get; }
		public decimal GSI_Value { get; }
		public string GSI_Comment { get; }
		public string GSI_Frequency { get; }
		public DateTime? GSI_GrantDate { get; }
		public bool GSI_IsFTEScalable { get; }
		public bool GSI_IsPartOfPackage { get; }

		public string GSI_SystemCreateUser { get; }
		public string GSI_SystemLastEditUser { get; }
		public DateTime GSI_SystemCreateTimeUtc { get; }
		public DateTime GSI_SystemLastEditTimeUtc { get; }
	}

	class GlbStaffRemuneration : SQLDataObject<GlbStaffRemuneration>
	{
		public GlbStaffRemuneration(Guid staff, DateTimeOffset effectiveDate, string country = "AU", string currency = "AUD", decimal fte = 1.0m)
		{
			GSR_GS_Staff = staff;
			GSR_EffectiveDate = effectiveDate;

			GSR_RN_NKCountry = country;
			GSR_RX_NKCurrency = currency;
			GSR_FullTimeEquivalent = fte;

			GSR_SystemCreateTimeUtc = DateTime.UtcNow;
			GSR_SystemLastEditTimeUtc = DateTime.UtcNow;

			GSR_SystemCreateUser = "E";
			GSR_SystemLastEditUser = "E";
		}

		public Guid GSR_GS_Staff { get; }
		public DateTimeOffset GSR_EffectiveDate { get; }
		public DateTimeOffset? GSR_AutoEffectiveEndDate { get; }

		public decimal GSR_FullTimeEquivalent { get; }
		public string GSR_RN_NKCountry { get; }
		public string GSR_RX_NKCurrency { get; }

		public string GSR_SystemCreateUser { get; }
		public string GSR_SystemLastEditUser { get; }
		public DateTime GSR_SystemCreateTimeUtc { get; }
		public DateTime GSR_SystemLastEditTimeUtc { get; }

		protected override ITableDefinition GetSchema() => base.GetSchema()
			.Extend(nameof(GSR_AutoEffectiveEndDate), new DateTimeOffsetColumnInfo(0, isNullable: true));
	}

	class GlbStaff : SQLDataObject<GlbStaff>
	{
		public GlbStaff(string code)
		{
			GS_Code = code;
			GS_LoginName = code + ".Login";
			GS_SystemCreateUser = "E";
			GS_SystemLastEditUser = "E";
			GS_SystemCreateTimeUtc = DateTime.Now;
			GS_SystemLastEditTimeUtc = DateTime.Now;
		}

		public string GS_Code { get; }
		public string GS_LoginName { get; }
		public string GS_SystemCreateUser { get; }
		public string GS_SystemLastEditUser { get; }

		public DateTime? GS_SystemCreateTimeUtc { get; }
		public DateTime? GS_SystemLastEditTimeUtc { get; }
	}
}
