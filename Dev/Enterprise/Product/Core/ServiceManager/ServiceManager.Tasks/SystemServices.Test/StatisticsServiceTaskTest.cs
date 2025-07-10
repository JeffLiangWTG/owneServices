using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.SystemServices.Testing
{
	sealed class StatisticsServiceTaskTest : TestCaseWithFactory
	{
		public void TestCreateStatistics()
		{
			var now = ZDateTime.UtcNow.ToDateTime();

			StatisticsTestHelper.CreateUsage(Factory, now, "1.1.1.1", "EDI", "BNE", "E", "Action 1", "Action 1.1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now, "1.1.1.1", "EDI", "BNE", "E", "Action 1", "Action 1.1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now, "1.1.1.1", "EDI", "BNE", "E", "Action 1", "Action 1.1", 1, 0.001m);
			Factory.Save();

			StatisticsTestHelper.AssertTableCounts("Precondition.", 3, 0, 3, 0, 0, 0, 0);

			var serviceTask = new StatisticsServiceTask();
			serviceTask.ServiceLogger = new TestServiceLogger();

			SystemDataRegistry.Instance.StatisticsTopRowCountForSummarise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			serviceTask.CallCount_ForTest = 0;
			serviceTask.CreateStatistics_ForTest();
			StatisticsTestHelper.AssertTableCounts("Should create statistics.", 3, 0, 0, 1, 1, 1, 2);
			AssertEquals(2, serviceTask.CallCount_ForTest);

			serviceTask.CallCount_ForTest = 0;
			serviceTask.CreateStatistics_ForTest();
			StatisticsTestHelper.AssertTableCounts("Nothing to create.", 3, 0, 0, 1, 1, 1, 2);
			AssertEquals(1, serviceTask.CallCount_ForTest);
		}

		[TestDate(2012, 08, 27, 22, 42, 00)]
		public void TestDeleteOldStmUsage()
		{
			var now = TestDateAttribute.Date;
			var usage1 = StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-3), "1.1.1.1", "EDI", "BNE", "E", "Action 1", "Action 1.1", 1, 0.001m);
			var usage2 = StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-3), "1.1.1.1", "EDI", "BNE", "E", "Action 1", "Action 1.1", 1, 0.001m);
			var usage3 = StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-1), "1.1.1.1", "EDI", "BNE", "E", "Action 1", "Action 1.1", 1, 0.001m);
			var usage4 = StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-5), "1.1.1.1", "EDI", "BNE", "E", "Action 1", "Action 1.1", 1, 0.001m);
			Factory.Save();

			StatisticsTestHelper.DeleteQueue(usage1.PK.ToGuid());
			StatisticsTestHelper.DeleteQueue(usage2.PK.ToGuid());
			StatisticsTestHelper.DeleteQueue(usage3.PK.ToGuid());

			var expected = new string[]
			{
				FormatUsage(usage1.PK.ToGuid(), Guid.Empty),
				FormatUsage(usage2.PK.ToGuid(), Guid.Empty),
				FormatUsage(usage3.PK.ToGuid(), Guid.Empty),
				FormatUsage(usage4.PK.ToGuid(), usage4.PK.ToGuid()),
			};

			AssertStmUsage("Precondition", expected);

			SystemDataRegistry.Instance.StatisticsTopRowCountForDelete.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			SystemDataRegistry.Instance.StatisticsMinimumRetentionRawDataDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var serviceTask = new StatisticsServiceTask
			{
				ServiceLogger = new TestServiceLogger()
			};

			expected = new string[]
			{
				FormatUsage(usage3.PK.ToGuid(), Guid.Empty),
				FormatUsage(usage4.PK.ToGuid(), usage4.PK.ToGuid()),
			};

			serviceTask.CallCount_ForTest = 0;
			serviceTask.DeleteOldStmUsage_ForTest();
			AssertStmUsage("Should delete usage and action", expected);
			AssertEquals(3, serviceTask.CallCount_ForTest);

			serviceTask.CallCount_ForTest = 0;
			serviceTask.DeleteOldStmUsage_ForTest();
			AssertStmUsage("Nothing to delete", expected);
			AssertEquals(1, serviceTask.CallCount_ForTest);
		}

		[TestDate(2012, 08, 28, 13, 05, 00)]
		public void TestAggregateSubName()
		{
			var now = TestDateAttribute.Date;

			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-7), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-6), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-6), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-2), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			Factory.Save();

			var serviceTask = new StatisticsServiceTask
			{
				ServiceLogger = new TestServiceLogger()
			};

			SystemDataRegistry.Instance.StatisticsTopRowCountForSummarise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			serviceTask.CreateStatistics_ForTest();
			StatisticsTestHelper.AssertTableCounts("Precondition.", 4, 0, 0, 3, 1, 1, 4);

			SystemDataRegistry.Instance.StatisticsRemoveModuleIdAfterDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			serviceTask.AggregateSubName_ForTest();
			StatisticsTestHelper.AssertTableCounts("Should aggregate by SubName.", 4, 0, 0, 3, 1, 2, 3);

			serviceTask.AggregateSubName_ForTest();
			StatisticsTestHelper.AssertTableCounts("Nothing to aggregate.", 4, 0, 0, 3, 1, 2, 3);
		}

		[TestDate(2012, 08, 28, 13, 05, 00)]
		public void TestAggregateBranch()
		{
			var now = TestDateAttribute.Date;

			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-7), "1.1.1.1", "EDI", "B_1", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-6), "1.1.1.1", "EDI", "B_2", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-6), "1.1.1.1", "EDI", "B_1", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-2), "1.1.1.1", "EDI", "B_2", "E", "Name 1", "SubName 1", 1, 0.001m);
			Factory.Save();

			var serviceTask = new StatisticsServiceTask();
			serviceTask.ServiceLogger = new TestServiceLogger();

			SystemDataRegistry.Instance.StatisticsTopRowCountForSummarise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			serviceTask.CreateStatistics_ForTest();
			StatisticsTestHelper.AssertTableCounts("Precondition.", 4, 0, 0, 3, 1, 1, 4);

			SystemDataRegistry.Instance.StatisticsRemoveBranchAfterDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			serviceTask.AggregateBranch_ForTest();
			StatisticsTestHelper.AssertTableCounts("Should aggregate by Branch.", 4, 0, 0, 3, 1, 1, 3);

			serviceTask.AggregateBranch_ForTest();
			StatisticsTestHelper.AssertTableCounts("Nothing to aggregate.", 4, 0, 0, 3, 1, 1, 3);
		}

		[TestDate(2012, 08, 28, 13, 05, 00)]
		public void TestAggregatStaff()
		{
			var now = TestDateAttribute.Date;

			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-7), "1.1.1.1", "EDI", "B_1", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-6), "1.1.1.1", "EDI", "B_1", "F", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-6), "1.1.1.1", "EDI", "B_1", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(-2), "1.1.1.1", "EDI", "B_1", "F", "Name 1", "SubName 1", 1, 0.001m);
			Factory.Save();

			var serviceTask = new StatisticsServiceTask();
			serviceTask.ServiceLogger = new TestServiceLogger();

			SystemDataRegistry.Instance.StatisticsTopRowCountForSummarise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			serviceTask.CreateStatistics_ForTest();
			StatisticsTestHelper.AssertTableCounts("Precondition.", 4, 0, 0, 3, 1, 1, 4);

			SystemDataRegistry.Instance.StatisticsRemoveStaffAfterDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			serviceTask.AggregateStaff_ForTest();
			StatisticsTestHelper.AssertTableCounts("Should aggregate by Branch.", 4, 0, 0, 3, 1, 1, 3);

			serviceTask.AggregateStaff_ForTest();
			StatisticsTestHelper.AssertTableCounts("Nothing to aggregate.", 4, 0, 0, 3, 1, 1, 3);
		}

		[TestDate(2012, 08, 20, 16, 18, 00)]
		public void TestAggregatePeriod()
		{
			var now = TestDateAttribute.Date;

			var collection = new StatisticsFoldupInfoCollection();
			collection.AddNew(TimeFrameList.Codes.Day, 1, TimeFrameList.Codes.Month, 1);

			SystemDataRegistry.Instance.StatisticsAggregationParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var serviceTask = new StatisticsServiceTask();
			serviceTask.ServiceLogger = new TestServiceLogger();

			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(1), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(2), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(3), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			StatisticsTestHelper.CreateUsage(Factory, now.AddDays(4), "1.1.1.1", "EDI", "BNE", "E", "Name 1", "SubName 1", 1, 0.001m);
			Factory.Save();

			SystemDataRegistry.Instance.StatisticsTopRowCountForSummarise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			serviceTask.CreateStatistics_ForTest();
			StatisticsTestHelper.AssertTableCounts("Precondition.", 4, 0, 0, 4, 1, 1, 4);

			serviceTask.AggregatePeriod_ForTest();
			StatisticsTestHelper.AssertTableCounts("Nothing to aggregate.", 4, 0, 0, 4, 1, 1, 4);

			TestDateAttribute.Date = now.AddDays(4);
			serviceTask.AggregatePeriod_ForTest();
			StatisticsTestHelper.AssertTableCounts("Should aggregate by Period.", 4, 0, 0, 3, 1, 1, 3);

			TestDateAttribute.Date = now.AddDays(5);
			serviceTask.AggregatePeriod_ForTest();
			StatisticsTestHelper.AssertTableCounts("Should aggregate by Period.", 4, 0, 0, 2, 1, 1, 2);

			TestDateAttribute.Date = now.AddDays(6);
			serviceTask.AggregatePeriod_ForTest();
			StatisticsTestHelper.AssertTableCounts("Should aggregate by Period.", 4, 0, 0, 1, 1, 1, 1);
		}

		public void TestAlterPeriodFunction()
		{
			var collection = new StatisticsFoldupInfoCollection();
			collection.AddNew(TimeFrameList.Codes.Hour, 2, TimeFrameList.Codes.Minute, 30);
			collection.AddNew(TimeFrameList.Codes.Week, 1, TimeFrameList.Codes.Day, 1);
			collection.AddNew(TimeFrameList.Codes.Quarter, 3, TimeFrameList.Codes.Month, 1);

			SystemDataRegistry.Instance.StatisticsAggregationParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var expected = @"CREATE FUNCTION StatisticsPeriodInline
(
	@now  smalldatetime,
	@date smalldatetime
)

RETURNS TABLE
AS
RETURN

WITH Counts AS
(
	SELECT
		count_Year    = DATEDIFF(YEAR,    0, @date),
		count_Quarter = DATEDIFF(QUARTER, 0, @date),
		count_Month   = DATEDIFF(MONTH,   0, @date),
		count_Day     = DATEDIFF(DAY,     0, @date),
		count_Hour    = DATEDIFF(HOUR,    0, @date),
		count_Minute  = DATEDIFF(MINUTE,  0, @date)
)
SELECT
	calc_period =
		CASE
			WHEN @date < DATEADD(Quarter, -  3, @now) THEN count_Month
			WHEN @date < DATEADD(Week   , -  1, @now) THEN count_Day
			WHEN @date < DATEADD(Hour   , -  2, @now) THEN count_Minute / 30
			ELSE count_Minute
		END
FROM
	Counts
";

			var serviceTask = new StatisticsServiceTask();
			serviceTask.ServiceLogger = new TestServiceLogger();

			serviceTask.AlterPeriodFunction_ForTest(ZDateTime.UtcNow);
			AssertPeriodFunction("Function should be modified.", expected);

			collection = new StatisticsFoldupInfoCollection();
			collection.AddNew(TimeFrameList.Codes.Minute, 20, TimeFrameList.Codes.Hour, 1);
			collection.AddNew(TimeFrameList.Codes.Day, 10, TimeFrameList.Codes.Hour, 12);
			collection.AddNew(TimeFrameList.Codes.Year, 5, TimeFrameList.Codes.Month, 1);

			SystemDataRegistry.Instance.StatisticsAggregationParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			expected = @"CREATE FUNCTION StatisticsPeriodInline
(
	@now  smalldatetime,
	@date smalldatetime
)

RETURNS TABLE
AS
RETURN

WITH Counts AS
(
	SELECT
		count_Year    = DATEDIFF(YEAR,    0, @date),
		count_Quarter = DATEDIFF(QUARTER, 0, @date),
		count_Month   = DATEDIFF(MONTH,   0, @date),
		count_Day     = DATEDIFF(DAY,     0, @date),
		count_Hour    = DATEDIFF(HOUR,    0, @date),
		count_Minute  = DATEDIFF(MINUTE,  0, @date)
)
SELECT
	calc_period =
		CASE
			WHEN @date < DATEADD(Year   , -  5, @now) THEN count_Month
			WHEN @date < DATEADD(Day    , - 10, @now) THEN count_Hour / 12
			WHEN @date < DATEADD(Minute , - 20, @now) THEN count_Hour
			ELSE count_Minute
		END
FROM
	Counts
";

			serviceTask.AlterPeriodFunction_ForTest(ZDateTime.UtcNow);
			AssertPeriodFunction("Function should be modified.", expected);
		}

		#region Implementation

		void AssertStmUsage(string message, string[] expected)
		{
			var sql = @"
				SELECT
					XW_PK, XI_XW
				FROM
					dbo.StmUsage
					LEFT JOIN dbo.StmUsageQueue  ON XI_XW = XW_PK
				";

			var actual = new List<string>();

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var xw = (Guid)reader["XW_PK"];
					var xi = reader["XI_XW"] as Guid?;
					actual.Add(FormatUsage(xw, xi ?? Guid.Empty));
				}
			}

			AssertContainsExactElementsInAnyOrder(message, expected, actual);
		}

		void AssertPeriodFunction(string message, string expected)
		{
			var sql = @"EXEC sp_helptext StatisticsPeriodInline;";

			var actual = new ZStringBuilder();

			using (var connection = Db.NewAdminConnection())
			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actual.Append(reader.GetString(0));
				}
			}

			AssertEquals(message, expected, actual.ToString());
		}

		string FormatUsage(Guid usagePK, Guid queuePK)
		{
			return
				string.Format("Usage: {0}, Queue: {1}",
					usagePK.ToString().Substring(0, 2),
					queuePK.ToString().Substring(0, 2));
		}

		#endregion // Implementation
	}
}
