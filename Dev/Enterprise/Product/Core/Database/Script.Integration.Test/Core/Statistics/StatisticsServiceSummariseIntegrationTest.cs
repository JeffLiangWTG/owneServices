using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Statistics.Testing
{
	class StatisticsServiceSummariseIntegrationTest : TransactionedTestCase
	{
		public void TestTopRowCount()
		{
			var now = new DateTime(2012, 08, 01);
			int topRowCount = 2;

			var factory = new BusinessObjectFactory();
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "2.2.2.2", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "3.3.3.3", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "4.4.4.4", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "5.5.5.5", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			factory.Save();

			var expectedTableCounts = StatisticsTestHelper.FormatTableCounts(5, 0, 5, 0, 0, 0, 0);
			AssertEquals("Precondition", expectedTableCounts, StatisticsTestHelper.SelectTableCounts());

			var performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 2, performed);

			expectedTableCounts = StatisticsTestHelper.FormatTableCounts(5, 0, 3, 1, 2, 1, 2);
			AssertEquals("Should create statistics", expectedTableCounts, StatisticsTestHelper.SelectTableCounts());

			performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 2, performed);

			expectedTableCounts = StatisticsTestHelper.FormatTableCounts(5, 0, 1, 1, 4, 1, 4);
			AssertEquals("Should create statistics", expectedTableCounts, StatisticsTestHelper.SelectTableCounts());

			performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 1, performed);

			expectedTableCounts = StatisticsTestHelper.FormatTableCounts(5, 0, 0, 1, 5, 1, 5);
			AssertEquals("Should create statistics", expectedTableCounts, StatisticsTestHelper.SelectTableCounts());

			performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 0, performed);
			AssertEquals("Nothing to create", StatisticsTestHelper.FormatTableCounts(5, 0, 0, 1, 5, 1, 5), StatisticsTestHelper.SelectTableCounts());
		}

		public void TestSubMillisecondDuration()
		{
			var now = new DateTime(2012, 08, 01);
			var factory = new BusinessObjectFactory();
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.0001m);
			factory.Save();

			var expectedTableCounts = StatisticsTestHelper.FormatTableCounts(1, 0, 1, 0, 0, 0, 0);
			AssertEquals("Precondition", expectedTableCounts, StatisticsTestHelper.SelectTableCounts());

			var performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(1);
			AssertEquals("Number of performed rows", 1, performed);

			expectedTableCounts = StatisticsTestHelper.FormatTableCounts(1, 0, 0, 1, 1, 1, 1);
			AssertEquals("Should create statistics", expectedTableCounts, StatisticsTestHelper.SelectTableCounts());
		}

		public void TestGroupByPeriod()
		{
			int topRowCount = 10;

			var from = new DateTime(2012, 08, 01, 11, 22, 11, 111);
			var to_1 = from.AddDays(1);
			var to_2 = from.AddDays(2);
			var to_3 = from.AddDays(3);

			var small_from = new ZDateTime(from).ToSmallDateTime().ToDateTime();
			var small_to_1 = new ZDateTime(to_1).ToSmallDateTime().ToDateTime();
			var small_to_2 = new ZDateTime(to_2).ToSmallDateTime().ToDateTime();
			var small_to_3 = new ZDateTime(to_3).ToSmallDateTime().ToDateTime();

			var factory = new BusinessObjectFactory();
			StatisticsTestHelper.CreateUsage(factory, from, to_1.AddMilliseconds(100), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, from.AddHours(1), to_1.AddMilliseconds(200), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, from, to_2.AddSeconds(1), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, from, to_2.AddSeconds(2), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, from, to_3.AddSeconds(10), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			factory.Save();

			AssertEquals("Precondition", StatisticsTestHelper.FormatTableCounts(5, 0, 5, 0, 0, 0, 0), StatisticsTestHelper.SelectTableCounts());

			var performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 5, performed);
			AssertEquals("Should create statistics", StatisticsTestHelper.FormatTableCounts(5, 0, 0, 3, 1, 1, 3), StatisticsTestHelper.SelectTableCounts());

			var expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(small_from, small_to_1, 59214922, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 2, 0.004m, 0.000008m, 0.000000016m, 0.004m, 0.000008m, 0.000000016m),
				StatisticsTestHelper.FormatSummary(small_from, small_to_2, 59216362, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 2, 0.004m, 0.000008m, 0.000000016m, 0.004m, 0.000008m, 0.000000016m),
				StatisticsTestHelper.FormatSummary(small_from, small_to_3, 59217802, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m, 0.000004m, 0.000000008m, 0.002m, 0.000004m, 0.000000008m),
			};

			AssertContainsExactElementsInAnyOrder("Should create statistics", expectedSummaries, StatisticsTestHelper.SelectSummary());

			performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 0, performed);
			AssertEquals("Nothing to create", StatisticsTestHelper.FormatTableCounts(5, 0, 0, 3, 1, 1, 3), StatisticsTestHelper.SelectTableCounts());
		}

		public void TestGroupByVersion()
		{
			int topRowCount = 10;
			var now = new DateTime(2012, 08, 01);

			var factory = new BusinessObjectFactory();
			StatisticsTestHelper.CreateUsage(factory, now, now, "3.3.3.3", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "2.2.2.2", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "2.2.2.2", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m);
			factory.Save();

			AssertEquals("Precondition", StatisticsTestHelper.FormatTableCounts(6, 0, 6, 0, 0, 0, 0), StatisticsTestHelper.SelectTableCounts());

			var performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 6, performed);

			AssertEquals("Should create statistics", StatisticsTestHelper.FormatTableCounts(6, 0, 0, 1, 3, 1, 3), StatisticsTestHelper.SelectTableCounts());

			var expectedVersions = new string[]
			{
				StatisticsTestHelper.FormatVersion("1.1.1.1"),
				StatisticsTestHelper.FormatVersion("2.2.2.2"),
				StatisticsTestHelper.FormatVersion("3.3.3.3"),
			};

			AssertContainsExactElementsInAnyOrder("Should create versions", expectedVersions, StatisticsTestHelper.SelectVersion());

			var expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 3, 0.006m, 0.000012m, 0.000000024m, 0.006m, 0.000012m, 0.000000024m),
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "2.2.2.2", "EDI", "BNE", "E", "Name 1", "SubName 1", 2, 0.004m, 0.000008m, 0.000000016m, 0.004m, 0.000008m, 0.000000016m),
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "3.3.3.3", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.002m, 0.000004m, 0.000000008m, 0.002m, 0.000004m, 0.000000008m),
			};

			AssertContainsExactElementsInAnyOrder("Should create statistics", expectedSummaries, StatisticsTestHelper.SelectSummary());
		}

		public void TestGroupByCompanyCode()
		{
			int topRowCount = 10;
			var now = new DateTime(2012, 08, 01);

			var factory = new BusinessObjectFactory();
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_2", "B_2", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_2", "B_2", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 1, 0.002m);
			factory.Save();

			AssertEquals("Precondition", StatisticsTestHelper.FormatTableCounts(5, 0, 5, 0, 0, 0, 0), StatisticsTestHelper.SelectTableCounts());

			var performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 5, performed);

			AssertEquals("Should create statistics", StatisticsTestHelper.FormatTableCounts(5, 0, 0, 1, 1, 1, 2), StatisticsTestHelper.SelectTableCounts());

			var expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 3, 0.006m, 0.000012m, 0.000000024m, 0.006m, 0.000012m, 0.000000024m),
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "C_2", "B_2", "E", "Name 1", "SubName 1", 2, 0.004m, 0.000008m, 0.000000016m, 0.004m, 0.000008m, 0.000000016m),
			};

			AssertContainsExactElementsInAnyOrder("Should create statistics", expectedSummaries, StatisticsTestHelper.SelectSummary());
		}

		public void TestGroupByBranchCode()
		{
			int topRowCount = 10;
			var now = new DateTime(2012, 08, 01);

			var factory = new BusinessObjectFactory();
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_2", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_2", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 1, 0.002m);
			factory.Save();

			AssertEquals("Precondition", StatisticsTestHelper.FormatTableCounts(5, 0, 5, 0, 0, 0, 0), StatisticsTestHelper.SelectTableCounts());

			var performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 5, performed);

			AssertEquals("Should create statistics", StatisticsTestHelper.FormatTableCounts(5, 0, 0, 1, 1, 1, 2), StatisticsTestHelper.SelectTableCounts());

			var expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 3, 0.006m, 0.000012m, 0.000000024m, 0.006m, 0.000012m, 0.000000024m),
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "C_1", "B_2", "E", "Name 1", "SubName 1", 2, 0.004m, 0.000008m, 0.000000016m, 0.004m, 0.000008m, 0.000000016m),
			};

			AssertContainsExactElementsInAnyOrder("Should create statistics", expectedSummaries, StatisticsTestHelper.SelectSummary());
		}

		public void TestGroupByStaffCode()
		{
			int topRowCount = 10;
			var now = new DateTime(2012, 08, 01);

			var factory = new BusinessObjectFactory();
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "F", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "F", "Name 1", "SubName 1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 1, 0.002m);
			factory.Save();

			AssertEquals("Precondition", StatisticsTestHelper.FormatTableCounts(5, 0, 5, 0, 0, 0, 0), StatisticsTestHelper.SelectTableCounts());

			var performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 5, performed);

			AssertEquals("Should create statistics", StatisticsTestHelper.FormatTableCounts(5, 0, 0, 1, 1, 1, 2), StatisticsTestHelper.SelectTableCounts());

			var expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "C_1", "B_1", "E", "Name 1", "SubName 1", 3, 0.006m, 0.000012m, 0.000000024m, 0.006m, 0.000012m, 0.000000024m),
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "C_1", "B_1", "F", "Name 1", "SubName 1", 2, 0.004m, 0.000008m, 0.000000016m, 0.004m, 0.000008m, 0.000000016m),
			};

			AssertContainsExactElementsInAnyOrder("Should create statistics", expectedSummaries, StatisticsTestHelper.SelectSummary());
		}

		public void TestGroupByActionKey()
		{
			int topRowCount = 10;
			var now = new DateTime(2012, 08, 01);

			var factory = new BusinessObjectFactory();
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1.1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1.2", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 2", "SubName 2.1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 2", "SubName 2.1", 1, 0.002m);
			StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1.1", 1, 0.002m);
			factory.Save();

			AssertEquals("Precondition", StatisticsTestHelper.FormatTableCounts(5, 0, 5, 0, 0, 0, 0), StatisticsTestHelper.SelectTableCounts());

			var performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(topRowCount);
			AssertEquals("Number of performed rows", 5, performed);

			AssertEquals("Should create statistics", StatisticsTestHelper.FormatTableCounts(5, 0, 0, 1, 1, 3, 3), StatisticsTestHelper.SelectTableCounts());

			var expectedVersions = new string[]
			{
				StatisticsTestHelper.FormatActionKey("Name 1", "SubName 1.1"),
				StatisticsTestHelper.FormatActionKey("Name 1", "SubName 1.2"),
				StatisticsTestHelper.FormatActionKey("Name 2", "SubName 2.1"),
			};

			AssertContainsExactElementsInAnyOrder("Should create action keys", expectedVersions, StatisticsTestHelper.SelectActionKey());

			var expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1.1", 2, 0.004m, 0.000008m, 0.000000016m, 0.004m, 0.000008m, 0.000000016m),
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1.2", 1, 0.002m, 0.000004m, 0.000000008m, 0.002m, 0.000004m, 0.000000008m),
				StatisticsTestHelper.FormatSummary(now, now, 59212800, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 2", "SubName 2.1", 2, 0.004m, 0.000008m, 0.000000016m, 0.004m, 0.000008m, 0.000000016m),
			};

			AssertContainsExactElementsInAnyOrder("Should create statistics", expectedSummaries, StatisticsTestHelper.SelectSummary());
		}

		public void TestDecimalSeparator()
		{
			var now = new DateTime(2019, 02, 18);

			var decimalSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
			try
			{
				System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator = ",";

				var factory = new BusinessObjectFactory();
				StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
				factory.Save();
			}
			finally
			{
				System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator = decimalSeparator;
			}

			var expectedTableCounts = StatisticsTestHelper.FormatTableCounts(1, 0, 1, 0, 0, 0, 0);
			AssertEquals("Precondition", expectedTableCounts, StatisticsTestHelper.SelectTableCounts());

			var performed = StatisticsTestHelper.ExecStatisticsServiceSummarise(1);
			AssertEquals("Number of performed rows", 1, performed);

			expectedTableCounts = StatisticsTestHelper.FormatTableCounts(1, 0, 0, 1, 1, 1, 1);
			AssertEquals("Should create statistics", expectedTableCounts, StatisticsTestHelper.SelectTableCounts());
		}
	}
}
