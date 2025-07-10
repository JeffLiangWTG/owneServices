using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	[TestedType(typeof(PublicHoliday))]
	class PublicHolidayTest : DbCreateScriptTest
	{
		readonly IEqualityComparer<DataRow> comparator = DataRowComparer.Default;

		Guid staffPK;
		Guid branchPK;

		readonly Guid holidaySourceAU = Guid.NewGuid();
		readonly Guid holidaySourceCA = Guid.NewGuid();
		readonly Guid holidaySourceGB = Guid.NewGuid();

		protected override void SetUp()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			staffPK = TestDataCreator.CreateGlbStaff("ABC", "Andrew Churchill");
			branchPK = TestDataCreator.CreateBranch(companyPK, "YYZ", "WHAT");

			base.SetUp();
		}

		void SetUpHolidaysOfAllTypes()
		{
			var query = $@"
INSERT INTO dbo.GlbHolidaySource (GHS_PK, GHS_Code, GHS_Name, GHS_SystemCreateTimeUtc, GHS_SystemCreateUser, GHS_SystemLastEditTimeUtc, GHS_SystemLastEditUser)
VALUES
	(@australia, 'AUS', 'Australia', GETUTCDATE(), 'LLW', GETUTCDATE(), 'LLW'),
	(@canada, 'CAN', 'Canada', GETUTCDATE(), 'LLW', GETUTCDATE(), 'LLW'),
	(@britain, 'GBR', 'Great Britain', GETUTCDATE(), 'LLW', GETUTCDATE(), 'LLW');

INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Recurring, GH_Date, GH_ParentID, GH_ParentTableCode)
VALUES
	(NEWID(), 'Anzac Day', 1, '2022-04-25', @australia, 'GHS'),
	(NEWID(), 'Boxing Day', 1, '2022-12-26', @branch, 'GB');

INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Recurring, GH_RecurrType, GH_RecurrMonth, GH_RecurrDay, GH_ParentID, GH_ParentTableCode)
VALUES
	(NEWID(), 'May Day', 1, '1ST', 'MAY', 'MON', @britain, 'GHS'),
	(NEWID(), 'Thanksgiving', 1, '2ND', 'OCT', 'MON', @canada, 'GHS');

UPDATE dbo.GlbStaff
SET
	GS_LoginName = 'Andrew',
	GS_GB_HomeBranch = @branch,
	GS_SystemLastEditUser = 'E',
	GS_SystemLastEditTimeUtc = GetDate()
WHERE
	GS_PK = @staff;
";

			TestConnection.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@australia", SqlDbType.UniqueIdentifier, holidaySourceAU);
				p.AddParameter("@canada", SqlDbType.UniqueIdentifier, holidaySourceCA);
				p.AddParameter("@britain", SqlDbType.UniqueIdentifier, holidaySourceGB);
				p.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);
				p.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);
			});
		}

		void SetUpRecurringDateHolidays()
		{
			var query = $@"
INSERT INTO dbo.GlbHolidaySource (GHS_PK, GHS_Code, GHS_Name, GHS_SystemCreateTimeUtc, GHS_SystemCreateUser, GHS_SystemLastEditTimeUtc, GHS_SystemLastEditUser)
VALUES
	(@australia, 'AUS', 'Australia', GETUTCDATE(), 'LLW', GETUTCDATE(), 'LLW'),
	(@britain, 'GBR', 'Great Britain', GETUTCDATE(), 'LLW', GETUTCDATE(), 'LLW');

INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Recurring, GH_Date, GH_ParentID, GH_ParentTableCode)
VALUES
	(NEWID(), 'Anzac Day', 1, '2022-04-25', @australia, 'GHS'),
	(NEWID(), 'Boxing Day', 1, '2022-12-26', @branch, 'GB'),
	(NEWID(), 'Jane Austen Day', 1, '2022-12-16', @britain, 'GHS');

UPDATE dbo.GlbStaff
SET
	GS_LoginName = 'Andrew',
	GS_GB_HomeBranch = @branch,
	GS_SystemLastEditUser = 'E',
	GS_SystemLastEditTimeUtc = GetDate()
WHERE
	GS_PK = @staff;
";

			TestConnection.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@australia", SqlDbType.UniqueIdentifier, holidaySourceAU);
				p.AddParameter("@britain", SqlDbType.UniqueIdentifier, holidaySourceGB);
				p.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);
				p.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);
			});
		}

		DataTable ExpectedFixedDateHolidays(int numHolidays, int startYear, int month, int day, string name)
		{
			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("Holiday", typeof(DateTime)));
			expected.Columns.Add(new DataColumn("HolidayName", typeof(string)));

			foreach (var years in Enumerable.Range(0, numHolidays))
			{
				var row = expected.NewRow();
				row["Holiday"] = new DateTime(startYear + years, month, day);
				row["HolidayName"] = name;
				expected.Rows.Add(row);
			}

			return expected;
		}

		void CreateHolidaySourceHistory(DateTimeOffset effectiveDate, Guid holidaySource)
		{
			var query = $@"
INSERT INTO dbo.GlbHolidaySourceHistory (GHH_PK, GHH_GS_Staff, GHH_EffectiveDate, GHH_GHS_HolidaySource, GHH_SystemCreateTimeUtc, GHH_SystemCreateUser, GHH_SystemLastEditTimeUtc, GHH_SystemLastEditUser)
VALUES
	(NEWID(), @staff, @effectiveDate, @source, GETUTCDATE(), 'LLW', GETUTCDATE(), 'LLW');
";

			TestConnection.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);
				p.AddParameter("@effectiveDate", SqlDbType.DateTimeOffset, effectiveDate);
				p.AddParameter("@source", SqlDbType.UniqueIdentifier, holidaySource);
			});
		}

		public void TestOneHolidaySource_EffectiveAndEndDates_ExactlyMatchDateRange()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2019, 1, 1), holidaySourceAU);
			CreateHolidaySourceHistory(new DateTime(2022, 12, 31), holidaySourceGB);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			AssertContainsExactElementsInAnyOrder(comparator, ExpectedFixedDateHolidays(4, 2019, 4, 25, "Anzac Day").AsEnumerable(), results.AsEnumerable());
		}

		public void TestOneHolidaySource_EffectiveDateBeforeDateRange_EndDateMatches()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2018, 1, 1), holidaySourceAU);
			CreateHolidaySourceHistory(new DateTime(2022, 12, 31), holidaySourceGB);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			AssertContainsExactElementsInAnyOrder(comparator, ExpectedFixedDateHolidays(4, 2019, 4, 25, "Anzac Day").AsEnumerable(), results.AsEnumerable());
		}

		public void TestOneHolidaySource_EffectiveDateMatches_EndDateAfterDateRange()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2019, 1, 1), holidaySourceAU);
			CreateHolidaySourceHistory(new DateTime(2023, 12, 31), holidaySourceGB);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			AssertContainsExactElementsInAnyOrder(comparator, ExpectedFixedDateHolidays(4, 2019, 4, 25, "Anzac Day").AsEnumerable(), results.AsEnumerable());
		}

		public void TestOneHolidaySource_EffectiveDateMatches_NoEndDate()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2019, 1, 1), holidaySourceAU);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			AssertContainsExactElementsInAnyOrder(comparator, ExpectedFixedDateHolidays(4, 2019, 4, 25, "Anzac Day").AsEnumerable(), results.AsEnumerable());
		}

		public void TestOneHolidaySource_EffectiveDateBefore_EndDateAfterDateRange()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2018, 1, 1), holidaySourceAU);
			CreateHolidaySourceHistory(new DateTime(2023, 12, 31), holidaySourceGB);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			AssertContainsExactElementsInAnyOrder(comparator, ExpectedFixedDateHolidays(4, 2019, 4, 25, "Anzac Day").AsEnumerable(), results.AsEnumerable());
		}

		public void TestTwoHolidaySources_FirstEffectiveDateMatches_SecondEndDateMatches()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2019, 1, 1), holidaySourceAU);
			CreateHolidaySourceHistory(new DateTime(2021, 1, 1), holidaySourceGB);
			CreateHolidaySourceHistory(new DateTime(2022, 12, 31), holidaySourceCA);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(2, 2019, 4, 25, "Anzac Day");

			var row = expected.NewRow();
			row["Holiday"] = new DateTime(2021, 5, 3);
			row["HolidayName"] = "May Day";
			expected.Rows.Add(row);

			row = expected.NewRow();
			row["Holiday"] = new DateTime(2022, 5, 2);
			row["HolidayName"] = "May Day";
			expected.Rows.Add(row);

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestTwoHolidaySources_FirstEffectiveDateBefore_SecondEndDateAfter()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2018, 1, 1), holidaySourceAU);
			CreateHolidaySourceHistory(new DateTime(2021, 1, 1), holidaySourceGB);
			CreateHolidaySourceHistory(new DateTime(2023, 12, 31), holidaySourceCA);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(2, 2019, 4, 25, "Anzac Day");

			var row = expected.NewRow();
			row["Holiday"] = new DateTime(2021, 5, 3);
			row["HolidayName"] = "May Day";
			expected.Rows.Add(row);

			row = expected.NewRow();
			row["Holiday"] = new DateTime(2022, 5, 2);
			row["HolidayName"] = "May Day";
			expected.Rows.Add(row);

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestMultipleHolidaySourcesInRange()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2018, 1, 1), holidaySourceAU);
			CreateHolidaySourceHistory(new DateTime(2020, 1, 1), holidaySourceGB);
			CreateHolidaySourceHistory(new DateTime(2021, 12, 31), holidaySourceCA);
			CreateHolidaySourceHistory(new DateTime(2023, 12, 31), holidaySourceAU);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(1, 2019, 4, 25, "Anzac Day");

			var row = expected.NewRow();
			row["Holiday"] = new DateTime(2020, 5, 4);
			row["HolidayName"] = "May Day";
			expected.Rows.Add(row);

			row = expected.NewRow();
			row["Holiday"] = new DateTime(2021, 5, 3);
			row["HolidayName"] = "May Day";
			expected.Rows.Add(row);

			row = expected.NewRow();
			row["Holiday"] = new DateTime(2022, 10, 10);
			row["HolidayName"] = "Thanksgiving";
			expected.Rows.Add(row);

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestOneHolidaySource_EffectiveDateAfter_OneBranchHoliday()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2020, 1, 1), holidaySourceAU);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(3, 2020, 4, 25, "Anzac Day");
			var row = expected.NewRow();
			row["Holiday"] = new DateTime(2019, 12, 26);
			row["HolidayName"] = "Boxing Day";
			expected.Rows.Add(row);

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestOneHolidaySource_EffectiveDateAfter_MultipleBranchHolidays()
		{
			SetUpHolidaysOfAllTypes();

			CreateHolidaySourceHistory(new DateTime(2020, 1, 1), holidaySourceAU);

			var query = @"
INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Recurring, GH_Date, GH_ParentID, GH_ParentTableCode)
VALUES
	(NEWID(), 'Should be a public holiday', 1, '2022-03-08', @branch, 'GB');
";
			TestConnection.ExecuteNonQuery(query, p => p.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK));

			query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2017-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(9, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(3, 2020, 4, 25, "Anzac Day");
			expected.Merge(ExpectedFixedDateHolidays(3, 2017, 12, 26, "Boxing Day"));
			expected.Merge(ExpectedFixedDateHolidays(3, 2017, 3, 8, "Should be a public holiday"));

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestNoHolidaySources_OneBranch()
		{
			SetUpHolidaysOfAllTypes();

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			AssertContainsExactElementsInAnyOrder(comparator, ExpectedFixedDateHolidays(4, 2019, 12, 26, "Boxing Day").AsEnumerable(), results.AsEnumerable());
		}

		public void TestNoHolidaySources_NoBranch()
		{
			SetUpHolidaysOfAllTypes();

			var staffPK = TestDataCreator.CreateGlbStaff("DEF", "Diane Feng");

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2019-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(0, results.Rows.Count);
		}

		public void TestLessThanFiftyYearSpan_OneBranch_OneHolidaySource_ReturnsAllHolidays()
		{
			SetUpRecurringDateHolidays();

			CreateHolidaySourceHistory(new DateTime(2020, 7, 1), holidaySourceAU);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2018-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(4, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(2, 2021, 4, 25, "Anzac Day");
			expected.Merge(ExpectedFixedDateHolidays(2, 2018, 12, 26, "Boxing Day"));

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestLessThanFiftyYearSpan_TwoHolidaySources_ReturnsAllHolidays()
		{
			SetUpRecurringDateHolidays();

			CreateHolidaySourceHistory(new DateTime(2020, 7, 1), holidaySourceGB);
			CreateHolidaySourceHistory(new DateTime(2017, 12, 31), holidaySourceAU);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2018-01-01', '2022-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(6, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(3, 2018, 4, 25, "Anzac Day");
			expected.Merge(ExpectedFixedDateHolidays(3, 2020, 12, 16, "Jane Austen Day"));

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestExactlyFiftyYearSpan_OneBranch_OneHolidaySource_ReturnsAllHolidays()
		{
			SetUpRecurringDateHolidays();

			CreateHolidaySourceHistory(new DateTime(2037, 7, 1), holidaySourceAU);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2013-01-01', '2062-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(49, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(24, 2013, 12, 26, "Boxing Day");
			expected.Merge(ExpectedFixedDateHolidays(25, 2038, 4, 25, "Anzac Day"));

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestExactlyFiftyYearSpan_TwoHolidaySources_ReturnsAllHolidays()
		{
			SetUpRecurringDateHolidays();

			CreateHolidaySourceHistory(new DateTime(2037, 7, 1), holidaySourceGB);
			CreateHolidaySourceHistory(new DateTime(2012, 12, 31), holidaySourceAU);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2013-01-01', '2062-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(51, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(25, 2013, 4, 25, "Anzac Day");
			expected.Merge(ExpectedFixedDateHolidays(26, 2037, 12, 16, "Jane Austen Day"));

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestMoreThanFiftyYearSpan_AtHolidaySourcesForLessThanFiftyYearsEach()
		{
			SetUpRecurringDateHolidays();

			CreateHolidaySourceHistory(new DateTime(2018, 1, 1), holidaySourceAU);
			CreateHolidaySourceHistory(new DateTime(2010, 1, 1), holidaySourceGB);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '2003-01-01', '2062-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(60, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(7, 2003, 12, 26, "Boxing Day");
			expected.Merge(ExpectedFixedDateHolidays(8, 2010, 12, 16, "Jane Austen Day"));
			expected.Merge(ExpectedFixedDateHolidays(45, 2018, 4, 25, "Anzac Day"));

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}

		public void TestMoreThanFiftyYearSpan_TwoHolidaySources_ReturnsHolidaysFromFirstFiftyYears()
		{
			SetUpRecurringDateHolidays();

			CreateHolidaySourceHistory(new DateTime(2012, 1, 1), holidaySourceAU);
			CreateHolidaySourceHistory(new DateTime(1959, 1, 1), holidaySourceGB);

			var query = "SELECT Holiday, HolidayName FROM dbo.PublicHoliday(@staff, '1960-01-01', '2070-12-31')";
			var command = TestConnection.Command(query);
			command.AddParameter("@staff", SqlDbType.UniqueIdentifier, staffPK);

			var results = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(100, results.Rows.Count);

			var expected = ExpectedFixedDateHolidays(50, 1960, 12, 16, "Jane Austen Day");
			expected.Merge(ExpectedFixedDateHolidays(50, 2012, 4, 25, "Anzac Day"));

			AssertContainsExactElementsInAnyOrder(comparator, expected.AsEnumerable(), results.AsEnumerable());
		}
	}
}
