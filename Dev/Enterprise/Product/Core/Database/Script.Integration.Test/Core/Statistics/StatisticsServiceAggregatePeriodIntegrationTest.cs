using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Statistics.Testing
{
	class StatisticsServiceAggregatePeriodIntegrationTest : TransactionedTestCase
	{
		[TestDate(2012, 08, 01)]
		public void TestAggregate()
		{
			var now = new DateTime(2012, 08, 01);

			var from_1 = now;
			var from_2 = from_1.AddHours(1);
			var from_3 = from_2.AddHours(1);
			var from_4 = from_3.AddDays(1);

			var to_1 = from_2.AddMinutes(3);
			var to_2 = from_2.AddMinutes(5);
			var to_3 = from_2.AddMinutes(30);
			var to_4 = from_3;
			var to_5 = from_4;

			var period_1 = StatisticsTestHelper.InsertDateRange(from_1, to_1, 59212863, false);
			var period_2 = StatisticsTestHelper.InsertDateRange(from_2, to_2, 59212865, true);
			var period_3 = StatisticsTestHelper.InsertDateRange(from_1, to_3, 59212890, true);
			var period_4 = StatisticsTestHelper.InsertDateRange(from_3, to_4, 59212920, true);
			var period_5 = StatisticsTestHelper.InsertDateRange(from_4, to_5, 59214360, true);

			var version = StatisticsTestHelper.InsertVersion("1.1.1.1");
			var action = StatisticsTestHelper.InsertActionKey("Name 1", "SubName 1");

			StatisticsTestHelper.InsertSummary(period_1, version, "EDI", "BNE", "E", action, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_2, version, "EDI", "BNE", "E", action, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_3, version, "EDI", "BNE", "E", action, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_4, version, "EDI", "BNE", "E", action, 1, 0.001m, 0.000001m, 0.000000001m);
			StatisticsTestHelper.InsertSummary(period_5, version, "EDI", "BNE", "E", action, 1, 0.001m, 0.000001m, 0.000000001m);

			AssertEquals("Precondition TableCounts", StatisticsTestHelper.FormatTableCounts(0, 0, 0, 5, 1, 1, 5), StatisticsTestHelper.SelectTableCounts());

			var expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(from_1, to_1, 59212863, 0, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_2, to_2, 59212865, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_1, to_3, 59212890, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_3, to_4, 59212920, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_4, to_5, 59214360, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Precondition Summaries", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = to_1.AddDays(1).AddMinutes(1);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(from_1, to_1, 3947524, 0, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_2, to_2, 59212865, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_1, to_3, 59212890, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_3, to_4, 59212920, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_4, to_5, 59214360, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Change period 1", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = to_2.AddDays(1).AddMinutes(1);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(from_1, to_2, 3947524, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 2, 0.002m, 0.000002m, 0.000000002m, 0.002m, 0.000002m, 0.000000002m),
				StatisticsTestHelper.FormatSummary(from_1, to_3, 59212890, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_3, to_4, 59212920, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_4, to_5, 59214360, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Aggregate periods 1 and 2", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = to_3.AddDays(1).AddMinutes(1);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(from_1, to_2, 3947524, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 2, 0.002m, 0.000002m, 0.000000002m, 0.002m, 0.000002m, 0.000000002m),
				StatisticsTestHelper.FormatSummary(from_1, to_3, 3947526, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_3, to_4, 59212920, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_4, to_5, 59214360, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Change period 3", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = to_4.AddDays(1).AddMinutes(1);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(from_1, to_2, 3947524, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 2, 0.002m, 0.000002m, 0.000000002m, 0.002m, 0.000002m, 0.000000002m),
				StatisticsTestHelper.FormatSummary(from_1, to_3, 3947526, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_3, to_4, 3947528, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_4, to_5, 59214360, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Change period 4", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = now.AddMinutes(10);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			AssertContainsExactElementsInAnyOrder("Nothing to aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = now.AddMinutes(10);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			AssertContainsExactElementsInAnyOrder("Nothing to aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = to_5.AddDays(1).AddMinutes(1);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(from_1, to_2, 3947524, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 2, 0.002m, 0.000002m, 0.000000002m, 0.002m, 0.000002m, 0.000000002m),
				StatisticsTestHelper.FormatSummary(from_1, to_3, 3947526, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_3, to_4, 3947528, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_4, to_5, 3947624, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Change period 5", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = now.AddDays(10);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			AssertContainsExactElementsInAnyOrder("Nothing to aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = to_5.AddMonths(1).AddDays(1).AddMinutes(1);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(from_1, to_3, 986881, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 3, 0.003m, 0.000003m, 0.000000003m, 0.003m, 0.000003m, 0.000000003m),
				StatisticsTestHelper.FormatSummary(from_3, to_4, 986882, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
				StatisticsTestHelper.FormatSummary(from_4, to_5, 986906, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Aggregate periods 2 and 3, Change periods 4 and 5", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = to_5.AddYears(1).AddDays(1).AddMinutes(1);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(from_1, to_4, 41120, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 4, 0.004m, 0.000004m, 0.000000004m, 0.004m, 0.000004m, 0.000000004m),
				StatisticsTestHelper.FormatSummary(from_4, to_5, 41121, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m, 0.000001m, 0.000000001m, 0.001m, 0.000001m, 0.000000001m),
			};

			AssertContainsExactElementsInAnyOrder("Aggregate periods 3 and 4, Change period 5", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = to_5.AddYears(2).AddDays(1).AddMinutes(1);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			expectedSummaries = new string[]
			{
				StatisticsTestHelper.FormatSummary(from_1, to_5, 1351, 1, "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 5, 0.005m, 0.000005m, 0.000000005m, 0.005m, 0.000005m, 0.000000005m),
			};

			AssertContainsExactElementsInAnyOrder("Aggregate periods 4 and 5", expectedSummaries, StatisticsTestHelper.SelectSummary());

			now = to_5.AddYears(10).AddDays(1).AddMinutes(1);
			StatisticsTestHelper.ExecStatisticsServiceAggregatePeriod(now.AddDays(-1), now);
			AssertContainsExactElementsInAnyOrder("Nothing to aggregate", expectedSummaries, StatisticsTestHelper.SelectSummary());
		}
	}
}
