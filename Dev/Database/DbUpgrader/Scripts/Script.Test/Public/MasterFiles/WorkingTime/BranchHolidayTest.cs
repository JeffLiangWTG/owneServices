using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	[TestedType(typeof(BranchHoliday))]
	class BranchHolidaysTest : DbCreateScriptTest
	{
		readonly IEqualityComparer<DataRow> comparator = DataRowComparer.Default;

		void SetUpHolidaysOfAllTypes(Guid branchPK)
		{
			var insertions = $@"
INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Date, GH_ParentID, GH_ParentTableCode)
VALUES
	(NEWID(), 'Date Holiday 0', '2022-02-10', @branch, 'GB'),
	(NEWID(), 'Date Holiday 1', '2022-08-16', @branch, 'GB'),
	(NEWID(), 'Date Holiday 2', '2022-08-30', @branch, 'GB');

INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Recurring, GH_RecurrType, GH_RecurrMonth, GH_RecurrDay, GH_ParentID, GH_ParentTableCode)
VALUES
	(NEWID(), 'Recurring Holiday 0', 1, '1ST', 'MAR', 'WED', @branch, 'GB'),
	(NEWID(), 'Recurring Holiday 1', 1, '2ND', 'AUG', 'MON', @branch, 'GB'),
	(NEWID(), 'Recurring Holiday 2', 1, '3RD', 'SEP', 'FRI', @branch, 'GB');

INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Recurring, GH_RecurrType, GH_ParentID, GH_ParentTableCode)
VALUES
	(NEWID(), 'Good Friday', 1, 'EFR', @branch, 'GB'),
	(NEWID(), 'Easter Monday', 1, 'EMN', @branch, 'GB');

INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Recurring, GH_Date, GH_ParentID, GH_ParentTableCode)
VALUES
	(NEWID(), 'Recurring Date Holiday 0', 1, '2022-11-26', @branch, 'GB');

";

			TestConnection.ExecuteNonQuery(insertions, p => p.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK));
		}

		void SetUpRecurringDateHolidays(Guid branchPK)
		{
			var insertions = $@"
INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Recurring, GH_Date, GH_ParentID, GH_ParentTableCode)
VALUES
	(NEWID(), 'Recurring Date Holiday 0', 1, '2022-11-26', @branch, 'GB'),
	(NEWID(), 'Recurring Date Holiday 1', 1, '2022-10-20', @branch, 'GB'),
	(NEWID(), 'Recurring Date Holiday 2', 1, '2022-08-11', @branch, 'GB');
";
			TestConnection.ExecuteNonQuery(insertions, p => p.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK));
		}

		public void TestFixedDateHoliday()
		{
			var branchPK = Guid.NewGuid();
			SetUpHolidaysOfAllTypes(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2022-02-01', '2022-02-20')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, results.Rows.Count);

			var result = results.Rows[0];
			AssertEquals(new DateTime(2022, 2, 10), result["Holiday"]);
			AssertEquals("Date Holiday 0", result["HolidayName"]);
		}

		public void TestRecurringHoliday()
		{
			var branchPK = Guid.NewGuid();
			SetUpHolidaysOfAllTypes(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2022-02-20', '2022-03-10')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, results.Rows.Count);

			var result = results.Rows[0];
			AssertEquals(new DateTime(2022, 3, 2), result["Holiday"]);
			AssertEquals("Recurring Holiday 0", result["HolidayName"]);
		}

		public void TestRecurringFixedDateHoliday()
		{
			var branchPK = Guid.NewGuid();
			SetUpHolidaysOfAllTypes(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2022-11-01', '2022-11-30')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, results.Rows.Count);

			var result = results.Rows[0];
			AssertEquals(new DateTime(2022, 11, 26), result["Holiday"]);
			AssertEquals("Recurring Date Holiday 0", result["HolidayName"]);

			query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2012-11-01', '2012-11-30')";
			command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, results.Rows.Count);

			result = results.Rows[0];
			AssertEquals(new DateTime(2012, 11, 26), result["Holiday"]);
			AssertEquals("Recurring Date Holiday 0", result["HolidayName"]);
		}

		public void TestEasterHoliday()
		{
			var branchPK = Guid.NewGuid();
			SetUpHolidaysOfAllTypes(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2022-04-10', '2022-04-17')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, results.Rows.Count);

			var result = results.Rows[0];
			AssertEquals(new DateTime(2022, 4, 15), result["Holiday"]);
			AssertEquals("Good Friday", result["HolidayName"]);
		}

		public void TestAllHolidayTypes()
		{
			var branchPK = Guid.NewGuid();
			SetUpHolidaysOfAllTypes(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2022-04-17', '2022-08-17')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(3, results.Rows.Count);

			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("Holiday", typeof(DateTime)));
			expected.Columns.Add(new DataColumn("HolidayName", typeof(string)));

			var row = expected.NewRow();
			row["Holiday"] = new DateTime(2022, 4, 18);
			row["HolidayName"] = "Easter Monday";
			expected.Rows.Add(row);

			row = expected.NewRow();
			row["Holiday"] = new DateTime(2022, 8, 8);
			row["HolidayName"] = "Recurring Holiday 1";
			expected.Rows.Add(row);

			row = expected.NewRow();
			row["Holiday"] = new DateTime(2022, 8, 16);
			row["HolidayName"] = "Date Holiday 1";
			expected.Rows.Add(row);

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestMixedHolidayTypes()
		{
			var branchPK = Guid.NewGuid();
			SetUpHolidaysOfAllTypes(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2022-08-01', '2022-08-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(3, results.Rows.Count);

			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("Holiday", typeof(DateTime)));
			expected.Columns.Add(new DataColumn("HolidayName", typeof(string)));

			var row = expected.NewRow();
			row["Holiday"] = new DateTime(2022, 8, 8);
			row["HolidayName"] = "Recurring Holiday 1";
			expected.Rows.Add(row);

			row = expected.NewRow();
			row["Holiday"] = new DateTime(2022, 8, 16);
			row["HolidayName"] = "Date Holiday 1";
			expected.Rows.Add(row);

			row = expected.NewRow();
			row["Holiday"] = new DateTime(2022, 8, 30);
			row["HolidayName"] = "Date Holiday 2";
			expected.Rows.Add(row);

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestBothEasterHolidays()
		{
			var branchPK = Guid.NewGuid();
			SetUpHolidaysOfAllTypes(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2021-04-01', '2021-04-10')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(2, results.Rows.Count);

			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("Holiday", typeof(DateTime)));
			expected.Columns.Add(new DataColumn("HolidayName", typeof(string)));

			var row = expected.NewRow();
			row["Holiday"] = new DateTime(2021, 4, 2);
			row["HolidayName"] = "Good Friday";
			expected.Rows.Add(row);

			row = expected.NewRow();
			row["Holiday"] = new DateTime(2021, 4, 5);
			row["HolidayName"] = "Easter Monday";
			expected.Rows.Add(row);

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestRecurringHolidayIsCalculatedCorrectlyEachYear()
		{
			var branchPK = Guid.NewGuid();
			SetUpHolidaysOfAllTypes(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2022-09-01', '2022-09-17')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, results.Rows.Count);

			var result = results.Rows[0];
			AssertEquals(new DateTime(2022, 9, 16), result["Holiday"]);
			AssertEquals("Recurring Holiday 2", result["HolidayName"]);

			query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2021-09-01', '2021-09-17')";
			command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, results.Rows.Count);

			result = results.Rows[0];
			AssertEquals(new DateTime(2021, 9, 17), result["Holiday"]);
			AssertEquals("Recurring Holiday 2", result["HolidayName"]);
		}

		public void TestLessThanFiftyYearSpanReturnsAllHolidays()
		{
			var branchPK = Guid.NewGuid();
			SetUpRecurringDateHolidays(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2018-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(3 * 5, results.Rows.Count); // 3 holidays per year, 5 years

			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("Holiday", typeof(DateTime)));
			expected.Columns.Add(new DataColumn("HolidayName", typeof(string)));

			for (int i = 0; i < 5; i++)
			{
				var row = expected.NewRow();
				row["Holiday"] = new DateTime(2018 + i, 11, 26);
				row["HolidayName"] = "Recurring Date Holiday 0";
				expected.Rows.Add(row);

				row = expected.NewRow();
				row["Holiday"] = new DateTime(2018 + i, 10, 20);
				row["HolidayName"] = "Recurring Date Holiday 1";
				expected.Rows.Add(row);

				row = expected.NewRow();
				row["Holiday"] = new DateTime(2018 + i, 8, 11);
				row["HolidayName"] = "Recurring Date Holiday 2";
				expected.Rows.Add(row);
			}

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestExactlyFiftyYearSpanReturnsAllHolidays()
		{
			var branchPK = Guid.NewGuid();
			SetUpRecurringDateHolidays(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2013-01-01', '2062-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(3 * 50, results.Rows.Count); // 3 holidays per year, 50 years

			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("Holiday", typeof(DateTime)));
			expected.Columns.Add(new DataColumn("HolidayName", typeof(string)));

			for (int i = 0; i < 50; i++)
			{
				var row = expected.NewRow();
				row["Holiday"] = new DateTime(2013 + i, 11, 26);
				row["HolidayName"] = "Recurring Date Holiday 0";
				expected.Rows.Add(row);

				row = expected.NewRow();
				row["Holiday"] = new DateTime(2013 + i, 10, 20);
				row["HolidayName"] = "Recurring Date Holiday 1";
				expected.Rows.Add(row);

				row = expected.NewRow();
				row["Holiday"] = new DateTime(2013 + i, 8, 11);
				row["HolidayName"] = "Recurring Date Holiday 2";
				expected.Rows.Add(row);
			}

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestMoreThanFiftyYearSpanReturnsHolidaysInFirstTenYears()
		{
			var branchPK = Guid.NewGuid();
			SetUpRecurringDateHolidays(branchPK);

			var query = "SELECT Holiday, HolidayName FROM dbo.BranchHoliday(@branch, '2012-01-01', '2062-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(3 * 50, results.Rows.Count); // 3 holidays per year, truncated at 50 years

			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("Holiday", typeof(DateTime)));
			expected.Columns.Add(new DataColumn("HolidayName", typeof(string)));

			for (int i = 0; i < 50; i++)
			{
				var row = expected.NewRow();
				row["Holiday"] = new DateTime(2012 + i, 11, 26);
				row["HolidayName"] = "Recurring Date Holiday 0";
				expected.Rows.Add(row);

				row = expected.NewRow();
				row["Holiday"] = new DateTime(2012 + i, 10, 20);
				row["HolidayName"] = "Recurring Date Holiday 1";
				expected.Rows.Add(row);

				row = expected.NewRow();
				row["Holiday"] = new DateTime(2012 + i, 8, 11);
				row["HolidayName"] = "Recurring Date Holiday 2";
				expected.Rows.Add(row);
			}

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}
	}
}
