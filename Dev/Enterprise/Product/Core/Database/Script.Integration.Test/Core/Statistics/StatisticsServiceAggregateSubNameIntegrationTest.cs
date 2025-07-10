using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Statistics.Testing
{
	class StatisticsServiceAggregateSubNameIntegrationTest : TransactionedTestCase
	{
		public void TestAggregate()
		{
			var date_1 = new DateTime(2012, 08, 01);
			var date_2 = new DateTime(2012, 08, 02);
			var date_3 = new DateTime(2012, 08, 03);
			var date_4 = new DateTime(2012, 08, 04);
			var date_5 = new DateTime(2012, 08, 05);

			var period_1 = StatisticsTestHelper.InsertDateRange(date_1, date_1, 1, false); // Just to test HasSubNameData
			var period_2 = StatisticsTestHelper.InsertDateRange(date_2, date_2, 2, true);
			var period_3 = StatisticsTestHelper.InsertDateRange(date_3, date_3, 3, true);
			var period_4 = StatisticsTestHelper.InsertDateRange(date_4, date_4, 4, true);
			var period_5 = StatisticsTestHelper.InsertDateRange(date_5, date_5, 5, true);

			var version = StatisticsTestHelper.InsertVersion("1.1.1.1");

			var action_1 = StatisticsTestHelper.InsertActionKey("A", "");
			var action_2 = StatisticsTestHelper.InsertActionKey("A", "A_1");
			var action_3 = StatisticsTestHelper.InsertActionKey("B", "");
			var action_4 = StatisticsTestHelper.InsertActionKey("B", "B_1");
			var action_5 = StatisticsTestHelper.InsertActionKey("B", "B_2");

			StatisticsTestHelper.InsertSummary(period_1, version, "EDI", "BNE", "E", action_2, 1, 0.001m, 0.000001m, 0.000000001m); // Just to test HasSubNameData
			StatisticsTestHelper.InsertSummary(period_2, version, "EDI", "BNE", "E", action_1, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_3, version, "EDI", "BNE", "E", action_2, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_4, version, "EDI", "BNE", "E", action_2, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_4, version, "EDI", "BNE", "E", action_2, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_4, version, "EDI", "BNE", "E", action_4, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_5, version, "EDI", "BNE", "E", action_3, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_5, version, "EDI", "BNE", "E", action_4, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_5, version, "EDI", "BNE", "E", action_5, 1, 0.001m, 0.000001m, 0.000000001m);

			AssertEquals("Precondition", StatisticsTestHelper.FormatTableCounts(0, 0, 0, 5, 1, 5, 9), StatisticsTestHelper.SelectTableCounts());

			var expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 0, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 1, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 1, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_2", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Precondition", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateSubName(date_2);
			AssertContainsExactElementsInAnyOrder("No rows matched that time - Nothing to aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateSubName(date_2.AddMinutes(1));
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 0, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 0, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 1, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_2", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Should perform period 1 - Nothing to aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateSubName(date_3.AddMinutes(1));
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 0, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 0, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 0, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_2", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Should perform period 2 - Clear SubName", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateSubName(date_4.AddMinutes(1));
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 0, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 0, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 0, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 0, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 2, 0.002m, 0.000002m, 0.000000002m, 0.002m, 0.000002m, 0.000000002m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 0, "1.1.1.1", "EDI", "BNE", "E", "B",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "BNE", "E", "B", "B_2", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Should perform period 3 - Clear SubName and aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateSubName(date_5.AddMinutes(1));
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 0, "1.1.1.1", "EDI", "BNE", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 0, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 0, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 0, "1.1.1.1", "EDI", "BNE", "E", "A",    "", 2, 0.002m, 0.000002m, 0.000000002m, 0.002m, 0.000002m, 0.000000002m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 0, "1.1.1.1", "EDI", "BNE", "E", "B",    "", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 0, "1.1.1.1", "EDI", "BNE", "E", "B",    "", 3, 0.003m, 0.000003m, 0.000000003m, 0.003m, 0.000003m, 0.000000003m),
			};

			AssertContainsExactElementsInAnyOrder("Should perform period 4 - Clear SubName and aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateSubName(date_5.AddMonths(1));
			AssertContainsExactElementsInAnyOrder("No rows matched that time - Nothing to aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());
		}
	}
}
