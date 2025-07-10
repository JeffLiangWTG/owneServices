using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.System.Testing
{
	sealed class StmNumberSequenceUpgradeTaskTest : TransactionedTestCase
	{
		[SnailTest]
		public void TestIsRequired()
		{
			TestConnection.ExecuteNonQuery("TRUNCATE TABLE dbo.StmNumberSequence;");

			var task = new StmNumberSequenceUpgradeTask();
			AssertCounts("No records in table StmNumberSequence", 0, 0, 0);
			AssertEquals("IsRequired", true, task.IsRequired);

			var sql = @"
INSERT dbo.StmNumberSequence (SNS_Number) VALUES
	(2),
	(25),
	(999999);
";

			TestConnection.ExecuteNonQuery(sql);
			AssertCounts("3 records in table StmNumberSequence", 2, 999999, 3);
			AssertEquals("IsRequired", true, task.IsRequired);

			TestConnection.ExecuteNonQuery("INSERT dbo.StmNumberSequence (SNS_Number) VALUES (0);");
			AssertCounts("4 records in table StmNumberSequence", 0, 999999, 4);
			AssertEquals("IsRequired", true, task.IsRequired);

			TestConnection.ExecuteNonQuery("INSERT dbo.StmNumberSequence (SNS_Number) VALUES (1000000);");
			AssertCounts("5 records in table StmNumberSequence", 0, 1000000, 5);
			AssertEquals("IsRequired", true, task.IsRequired);

			task.Run();
			AssertCounts("1000001 records in table StmNumberSequence", 0, 1000000, 1000001);
			AssertEquals("IsRequired", false, task.IsRequired);
		}

		public void TestTaskNameWhenUpgrading()
		{
			var task = new StmNumberSequenceUpgradeTask();
			AssertEquals("TaskNameWhenUpgrading", "Populating dbo.StmNumberSequence", task.TaskNameWhenUpgrading);
		}

		[SnailTest]
		public void TestRun()
		{
			var sql = @"
TRUNCATE TABLE dbo.StmNumberSequence;
INSERT dbo.StmNumberSequence (SNS_Number) VALUES
	(2),
	(25),
	(999999);
";

			TestConnection.ExecuteNonQuery(sql);

			AssertCounts("[BEFORE] 3 record in table StmNumberSequence", 2, 999999, 3);

			var task = new StmNumberSequenceUpgradeTask();
			task.Run();

			AssertCounts("[AFTER] 1000001 records in table StmNumberSequence", 0, 1000000, 1000001);
		}

		#region Implementation

		void AssertCounts(string message, int minValue, int maxValue, int rowCount)
		{
			var sql = @"
SELECT
	minValue = ISNULL(MIN(SNS_Number), 0),
	maxValue = ISNULL(MAX(SNS_Number), 0),
	[rowCount] = COUNT(*)
FROM
	dbo.StmNumberSequence
";

			var actualMinValue = -1;
			var actualMaxValue = -1;
			var actualRowCount = 0;
			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					actualMinValue = (int)reader["minValue"];
					actualMaxValue = (int)reader["maxValue"];
					actualRowCount = (int)reader["rowCount"];
				}
			}

			CombineAssertions(message, () =>
			{
				AssertEquals("MinValue", minValue, actualMinValue);
				AssertEquals("MaxValue", maxValue, actualMaxValue);
				AssertEquals("RowCount", rowCount, actualRowCount);
			});
		}

		#endregion // Implementation
	}
}
