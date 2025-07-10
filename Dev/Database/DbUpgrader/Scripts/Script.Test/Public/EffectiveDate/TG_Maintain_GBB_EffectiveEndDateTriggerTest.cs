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
	[TestedType(typeof(TG_Maintain_GBB_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GBB_EffectiveEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var branch = new GlbBranch("B01");
			var department = new GlbDepartment("D01");

			var branchDepartment = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 1, 1));

			staff.AppendInsertAndReturnObject(sql);
			branch.AppendInsertAndReturnObject(sql);
			department.AppendInsertAndReturnObject(sql);
			branchDepartment.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, branchDepartment.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GBB_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var branch = new GlbBranch("B01");
			var department = new GlbDepartment("D01");
			var branchDepartments = new[]
			{
				new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 1, 1)),
				new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 2, 1)),
				new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			branch.AppendInsertAndReturnObject(sql);
			department.AppendInsertAndReturnObject(sql);
			Array.ForEach(branchDepartments, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, branchDepartments[0].PK)
				.ExpectEquals("A branch department's end date should match the proceeding row", l => l.GBB_AutoEffectiveEndDate, branchDepartments[1].GBB_EffectiveDate)
				.VerifyAll();

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, branchDepartments[1].PK)
				.ExpectEquals("A branch department's end date should match the proceeding row", l => l.GBB_AutoEffectiveEndDate, branchDepartments[2].GBB_EffectiveDate)
				.VerifyAll();

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, branchDepartments[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GBB_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var branch = new GlbBranch("B01");
			var department = new GlbDepartment("D01");
			var previousGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 2, 1));
			var nextGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			branch.AppendInsertAndReturnObject(sql);
			department.AppendInsertAndReturnObject(sql);

			var branch2 = new GlbBranch("B02");
			var department2 = new GlbDepartment("D02");
			branch2.AppendInsertAndReturnObject(sql);
			department2.AppendInsertAndReturnObject(sql);

			Array.ForEach(new[] { previousGBB, gslToUpdate, nextGBB }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbBeneficiaryBranchDepartment.GBB_GB_Branch), $"'{branch2.PK}'", branch2.PK),
				(nameof(GlbBeneficiaryBranchDepartment.GBB_GE_Department), $"'{department2.PK}'", department2.PK),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE dbo.GlbBeneficiaryBranchDepartment
					SET
						{column} = {sqlValue}
					WHERE
						GBB_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbBeneficiaryBranchDepartmentSchema.PK));

				GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, gslToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, previousGBB.PK)
					.ExpectNotEquals($"The update should not affect the previous branchDepartment ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, nextGBB.PK)
					.ExpectNotEquals($"The update should not affect the next branchDepartment ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbBeneficiaryBranchDepartment branchDepartment)
					=> branchDepartment.GetType().GetProperty(column).GetValue(branchDepartment);
			}
		}

		public void TestUpdateFieldsOnExistingRow_WithNullValue()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var branch = new GlbBranch("B01");
			var department = new GlbDepartment("D01");
			var previousGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, null, new DateTime(2020, 2, 1));
			var nextGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			branch.AppendInsertAndReturnObject(sql);
			department.AppendInsertAndReturnObject(sql);

			var branch2 = new GlbBranch("B02");
			var department2 = new GlbDepartment("D02");
			branch2.AppendInsertAndReturnObject(sql);
			department2.AppendInsertAndReturnObject(sql);

			Array.ForEach(new[] { previousGBB, gslToUpdate, nextGBB }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbBeneficiaryBranchDepartment.GBB_GE_Department), $"'{department2.PK}'", department2.PK),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE dbo.GlbBeneficiaryBranchDepartment
					SET
						{column} = {sqlValue}
					WHERE
						GBB_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbBeneficiaryBranchDepartmentSchema.PK));

				GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, gslToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, previousGBB.PK)
					.ExpectNotEquals($"The update should not affect the previous branchDepartment ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, nextGBB.PK)
					.ExpectNotEquals($"The update should not affect the next branchDepartment ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbBeneficiaryBranchDepartment branchDepartment)
					=> branchDepartment.GetType().GetProperty(column).GetValue(branchDepartment);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var branch = new GlbBranch("B01");
			var department = new GlbDepartment("D01");
			var previousGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 2, 1));
			var nextGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 3, 1));
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			branch.AppendInsertAndReturnObject(sql);
			department.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGBB, gslToUpdate, nextGBB }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbBeneficiaryBranchDepartment
				SET
					GBB_EffectiveDate = @newDate
				WHERE
					GBB_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbBeneficiaryBranchDepartmentSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbBeneficiaryBranchDepartmentSchema.GBB_EffectiveDate);
			});

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GBB_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GBB_AutoEffectiveEndDate, nextGBB.GBB_EffectiveDate)
				.VerifyAll();

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, previousGBB.PK)
				.ExpectEquals("The update should not change the previous branchDepartment's effective date", r => r.GBB_EffectiveDate, previousGBB.GBB_EffectiveDate)
				.ExpectEquals("The update should change the previous branchDepartment's end date", r => r.GBB_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, nextGBB.PK)
				.ExpectEquals("The update should not affect the next branchDepartment's effective date", r => r.GBB_EffectiveDate, nextGBB.GBB_EffectiveDate)
				.ExpectEquals("The update should not affect the next branchDepartment's end date", r => r.GBB_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var branch = new GlbBranch("B01");
			var department = new GlbDepartment("D01");
			var previousGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 2, 1));
			var nextGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 3, 1));
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			branch.AppendInsertAndReturnObject(sql);
			department.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGBB, gslToUpdate, nextGBB }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE dbo.GlbBeneficiaryBranchDepartment
				SET
					GBB_EffectiveDate = @newDate
				WHERE
					GBB_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbBeneficiaryBranchDepartmentSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbBeneficiaryBranchDepartmentSchema.GBB_EffectiveDate);
			});

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GBB_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GBB_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, previousGBB.PK)
				.ExpectEquals("The update should not change the previous branchDepartment's effective date", r => r.GBB_EffectiveDate, previousGBB.GBB_EffectiveDate)
				.ExpectEquals("The update should change the previous branchDepartment's end date to the next row's end date", r => r.GBB_AutoEffectiveEndDate, nextGBB.GBB_EffectiveDate)
				.VerifyAll();

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, nextGBB.PK)
				.ExpectEquals("The update should not affect the next branchDepartment's effective date", r => r.GBB_EffectiveDate, nextGBB.GBB_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GBB_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleStaffBranchDepartments()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[]
			{
				new GlbStaff("XYZ"),
				new GlbStaff("YZX"),
				new GlbStaff("ZXY"),
			};

			var branches = new[]
			{
				new GlbBranch("B01"),
				new GlbBranch("B02"),
			};

			var departments = new[]
			{
				new GlbDepartment("D01"),
				new GlbDepartment("D02"),
			};

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));
			TestConnection.ExecuteNonQuery(GlbBranch.GetBulkInsertStatement(branches));
			TestConnection.ExecuteNonQuery(GlbDepartment.GetBulkInsertStatement(departments));
			TestConnection.ExecuteNonQuery(@"
				INSERT INTO dbo.GlbBeneficiaryBranchDepartment
					(GBB_PK, GBB_GS_Staff, GBB_EffectiveDate, GBB_GB_Branch, GBB_GE_Department, GBB_SystemCreateTimeUtc, GBB_SystemLastEditTimeUtc, GBB_SystemCreateUser, GBB_SystemLastEditUser)
				VALUES
					(NEWID(), @staff0, '2020-01-01 01:00:00 +01:00', @branch0, @department0, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff0, '2020-02-01 11:00:00 +07:00', @branch1, @department1, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-03-01 03:00:00 +11:00', @branch1, @department0, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-04-01 17:00:00 -03:00', @branch0, @department1, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-01-01 09:00:00 +00:00', @branch0, @department1, GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-04-01 21:00:00 -05:00', @branch1, @department0, GETUTCDATE(), GETUTCDATE(), 'E', 'E')
			", p =>
			{
				for (var i = 0; i < staffs.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbBeneficiaryBranchDepartmentSchema.GBB_GS_Staff);
				}

				for (var i = 0; i < branches.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@branch" + i, branches[i].PK, GlbBeneficiaryBranchDepartmentSchema.GBB_GB_Branch);
				}

				for (var i = 0; i < departments.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@department" + i, departments[i].PK, GlbBeneficiaryBranchDepartmentSchema.GBB_GE_Department);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbBeneficiaryBranchDepartment.CountInDB(TestConnection));
			AssertBranchDepartmentDateHistory("First staff has two branches/departments on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertBranchDepartmentDateHistory("Second staff has two branches/departments on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertBranchDepartmentDateHistory("Third staff has two branches/departments split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[]
			{
				new GlbStaff("XYZ"),
				new GlbStaff("YZX")
			};

			var branches = new[]
			{
				new GlbBranch("B01"),
				new GlbBranch("B02"),
			};

			var departments = new[]
			{
				new GlbDepartment("D01"),
				new GlbDepartment("D02"),
			};

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));
			TestConnection.ExecuteNonQuery(GlbBranch.GetBulkInsertStatement(branches));
			TestConnection.ExecuteNonQuery(GlbDepartment.GetBulkInsertStatement(departments));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GBB_PK", // We won't ever update the primary key of a row
				"GBB_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GBB_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GBB_PK", $"'{pk}'" },
				{ "GBB_AutoVersion", "0" },
				{ "GBB_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GBB_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GBB_GB_Branch", $"'{branches[0].PK}'" },
				{ "GBB_GE_Department", $"'{departments[0].PK}'" },
				{ "GBB_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GBB_SystemCreateUser", "'XYZ'" },
				{ "GBB_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GBB_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GBB_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GBB_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GBB_GB_Branch", $"'{branches[1].PK}'", branches[1].PK),
				("GBB_GE_Department", $"'{departments[1].PK}'", departments[1].PK),
				("GBB_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GBB_SystemCreateUser", "'E'", "E"),
				("GBB_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GBB_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbBeneficiaryBranchDepartment");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO dbo.GlbBeneficiaryBranchDepartment ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE dbo.GlbBeneficiaryBranchDepartment SET {edit.column}={edit.sqlValue} WHERE GBB_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM dbo.GlbBeneficiaryBranchDepartment WHERE GBB_PK='{pk}'");
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
			var branch = new GlbBranch("B01");
			var department = new GlbDepartment("D01");

			var previousGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 1, 1));
			var middleRow = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 2, 1));
			var nextGBB = new GlbBeneficiaryBranchDepartment(staff.PK, branch.PK, department.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			branch.AppendInsertAndReturnObject(sql);
			department.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousGBB.AppendInsertAndReturnObject(sql);
			nextGBB.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GBB_EffectiveDate, middleRow.GBB_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GBB_AutoEffectiveEndDate, nextGBB.GBB_EffectiveDate)
				.VerifyAll();

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, previousGBB.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.GBB_AutoEffectiveEndDate, middleRow.GBB_EffectiveDate)
				.VerifyAll();

			GlbBeneficiaryBranchDepartment.AssertFromDB(TestConnection, nextGBB.PK)
				.ExpectEquals("The next rows end date is null", r => r.GBB_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertBranchDepartmentDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the record dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GBB_EffectiveDate, GBB_AutoEffectiveEndDate FROM dbo.GlbBeneficiaryBranchDepartment WHERE GBB_GS_Staff = '{staff}' ORDER BY GBB_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbBeneficiaryBranchDepartment.GBB_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbBeneficiaryBranchDepartment.GBB_AutoEffectiveEndDate)];

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

	class GlbBeneficiaryBranchDepartment : SQLDataObject<GlbBeneficiaryBranchDepartment>
	{
		public GlbBeneficiaryBranchDepartment(Guid staff, Guid branch, Guid? department, DateTimeOffset effectiveDate)
		{
			GBB_GS_Staff = staff;
			GBB_GB_Branch = branch;
			GBB_GE_Department = department;

			GBB_EffectiveDate = effectiveDate;

			GBB_SystemCreateTimeUtc = DateTime.UtcNow;
			GBB_SystemLastEditTimeUtc = DateTime.UtcNow;

			GBB_SystemCreateUser = "E";
			GBB_SystemLastEditUser = "E";
		}

		public Guid GBB_GS_Staff { get; }
		public Guid? GBB_GB_Branch { get; }
		public Guid? GBB_GE_Department { get; }

		public DateTimeOffset GBB_EffectiveDate { get; }
		public DateTimeOffset? GBB_AutoEffectiveEndDate { get; }

		public string GBB_SystemCreateUser { get; }
		public string GBB_SystemLastEditUser { get; }
		public DateTime GBB_SystemCreateTimeUtc { get; }
		public DateTime GBB_SystemLastEditTimeUtc { get; }

		protected override ITableDefinition GetSchema() => base.GetSchema()
			.Extend(nameof(GBB_AutoEffectiveEndDate), new DateTimeOffsetColumnInfo(0, isNullable: true));
	}
}
