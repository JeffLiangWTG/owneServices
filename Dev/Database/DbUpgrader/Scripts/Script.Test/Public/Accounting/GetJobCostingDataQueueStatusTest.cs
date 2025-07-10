using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetJobCostingDataQueueStatus))]
	class GetJobCostingDataQueueStatusTest : DbCreateScriptTest
	{
		public void TestNoItemsAddedToQueue()
		{
			var result = RunScript();
			AssertEquals("GetJobCostingDataQueueStatus should only ever return a single row", 1, result.Rows.Count);

			var row = result.Rows[0];

			AssertEquals("When nothing has been added to the JCDQueue, GetJobCostingDataQueueStatus should say there are no records in the queue", 0, row["HasRecordInQueue"]);
		}

		[TestDate(2012, 09, 24)]
		public void TestSingleItemAddedToQueue()
		{
			var insertionTime = DateTime.Today;
			InsertTransactionLine(insertionTime);

			var result = RunScript();
			AssertEquals("GetJobCostingDataQueueStatus should only ever return a single row", 1, result.Rows.Count);

			var row = result.Rows[0];

			AssertEquals("Since a record has been added to the queue, the HasRecordInQueue flag should be true", 1, row["HasRecordInQueue"]);
			AssertEquals("There should be one record in the queue", 1, row["RecordCount"]);
			AssertEquals("The oldest post date should be the one just added", insertionTime.Date, Convert.ToDateTime(row.Field<string>("OldestPostDate")));
		}

		[TestDate(2012, 09, 24)]
		public void TestTwoItemsAddedToQueue()
		{
			var insertionTime1 = DateTime.Today.AddDays(-3);
			var insertionTime2 = DateTime.Today;
			InsertTransactionLine(insertionTime1);
			InsertTransactionLine(insertionTime2);

			var result = RunScript();
			AssertEquals("GetJobCostingDataQueueStatus should only ever return a single row", 1, result.Rows.Count);

			var row = result.Rows[0];

			AssertEquals("Since a record has been added to the queue, the HasRecordInQueue flag should be true", 1, row["HasRecordInQueue"]);
			AssertEquals("There should be two records in the queue", 2, row["RecordCount"]);
			AssertEquals("The oldest post date should be the earliest one", insertionTime1.Date, Convert.ToDateTime(row.Field<string>("OldestPostDate")));
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.GetJobCostingDataQueueStatus()");
		}

		void InsertTransactionLine(DateTime postDate)
		{
			using (DbCommand command = TestConnection.Command(@"INSERT
					INTO JobCostingDataQueue
						(JCQ_ALPK,
						JCQ_GCPK,
						JCQ_PostDate,
						JCQ_ReverseDate)
						VALUES
						(NEWID(), 
						NEWID(), 
						@JCQ_PostDate,
						@JCQ_ReverseDate)"))
			{
				command.AddParameter("@JCQ_PostDate", SqlDbType.DateTime, postDate);
				command.AddParameter("@JCQ_ReverseDate", SqlDbType.DateTime, DateTime.Today.AddMonths(-2));
				command.ExecuteNonQuery();
			}
		}
	}
}
