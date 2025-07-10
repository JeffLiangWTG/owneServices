using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Statistics.Testing
{
	class StatisticsServiceAggregateStaffIntegrationTest : TransactionedTestCase
	{
		public void TestAggregate()
		{
			var date_1 = new DateTime(2012, 08, 01);
			var date_2 = new DateTime(2012, 08, 02);
			var date_3 = new DateTime(2012, 08, 03);
			var date_4 = new DateTime(2012, 08, 04);
			var date_5 = new DateTime(2012, 08, 05);

			var period_1 = StatisticsTestHelper.InsertDateRange(date_1, date_1, 1, true);
			var period_2 = StatisticsTestHelper.InsertDateRange(date_2, date_2, 2, true);
			var period_3 = StatisticsTestHelper.InsertDateRange(date_3, date_3, 3, true);
			var period_4 = StatisticsTestHelper.InsertDateRange(date_4, date_4, 4, true);
			var period_5 = StatisticsTestHelper.InsertDateRange(date_5, date_5, 5, true);

			var version = StatisticsTestHelper.InsertVersion("1.1.1.1");

			var action_1 = StatisticsTestHelper.InsertActionKey("A", "A_1");

			StatisticsTestHelper.InsertSummary(period_1, version, "EDI", "B_1", "E", action_1, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_2, version, "EDI", "B_1", "F", action_1, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_3, version, "EDI", "B_1", "E", action_1, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_4, version, "EDI", "B_1", "F", action_1, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_4, version, "EDI", "B_1", "E", action_1, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_4, version, "EDI", "B_1", "F", action_1, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_5, version, "EDI", "B_2", "E", action_1, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_5, version, "EDI", "B_1", "F", action_1, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_5, version, "EDI", "B_1", "E", action_1, 1, 0.001m, 0.000001m, 0.000000001m);

			AssertEquals("Precondition", StatisticsTestHelper.FormatTableCounts(0, 0, 0, 5, 1, 1, 9), StatisticsTestHelper.SelectTableCounts());

			var expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_2", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Precondition", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateStaff(date_1.AddMinutes(1));
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_2", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Should perform period 0 - Clear Staff", expectedSummaries, StatisticsTestHelper.SelectSummary()); //This line fails because B_1 isn't removed. If we comment it out, the next part over fails because B_1 and B_1 aren't removed. So it's not working yet.

			StatisticsTestHelper.ExecStatisticsServiceAggregateStaff(date_2.AddMinutes(1));
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_2", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Should perform period 1 - Clear Staff", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateStaff(date_3.AddMinutes(1));
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_2", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Should perform period 2 - Clear Staff", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateStaff(date_4.AddMinutes(1));
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 3, 0.003m, 0.000003m, 0.000000003m, 0.003m, 0.000003m, 0.000000003m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_2", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "F", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1", "E", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Should perform period 3 - Clear Staff and aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateStaff(date_5.AddMinutes(1));
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(date_1, date_1, 1, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_2, date_2, 2, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_3, date_3, 3, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_4, date_4, 4, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 3, 0.003m, 0.000003m, 0.000000003m, 0.003m, 0.000003m, 0.000000003m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_2",  "", "A", "A_1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(date_5, date_5, 5, 1, "1.1.1.1", "EDI", "B_1",  "", "A", "A_1", 2, 0.002m, 0.000002m, 0.000000002m, 0.002m, 0.000002m, 0.000000002m),
			};

			AssertContainsExactElementsInAnyOrder("Should perform period 4  - Clear Staff and aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());

			StatisticsTestHelper.ExecStatisticsServiceAggregateStaff(date_5.AddMonths(1));
			AssertContainsExactElementsInAnyOrder("No rows matched that time - Nothing to aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());
		}
	}
}
