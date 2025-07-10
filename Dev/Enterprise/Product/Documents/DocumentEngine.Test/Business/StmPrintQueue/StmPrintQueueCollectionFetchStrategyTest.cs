using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class StmPrintQueueCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewCore()
		{
			var server1 = Factory.New<StmPrintServer>();
			server1.SPS_ServerName = "S1";
			var queue1 = Factory.New<StmPrintQueue>();
			queue1.SQ_QueueName = "Q1";
			queue1.SQ_SPS_Server = server1.PK;
			queue1.SQ_AllowPrinting = true;

			var server2 = Factory.New<StmPrintServer>();
			server2.SPS_ServerName = "S2";
			var queue2 = Factory.New<StmPrintQueue>();
			queue2.SQ_QueueName = "Q2";
			queue2.SQ_SPS_Server = server2.PK;
			queue2.SQ_AllowPrinting = true;

			var server3 = Factory.New<StmPrintServer>();
			server3.SPS_ServerName = "S3";
			var queue3 = Factory.New<StmPrintQueue>();
			queue3.SQ_QueueName = "Q3";
			queue3.SQ_SPS_Server = server3.PK;
			queue3.SQ_AllowPrinting = true;

			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			var collection = new StmPrintQueueCollection(factory2);
			collection.Load();
			AssertEquals(3, collection.Count);

			collection.FetchStrategy.FetchForView(collection.ToArray(), new[] { new TableColumn(StmPrintQueueSchema.Constants.TableName, StmPrintQueueSchema.Constants.SQ_SPS_Server) });

			AssertEquals("Precondition", 0, factory2.GetTableHitCount(StmPrintServerSchema.Constants.TableName));
			foreach (StmPrintQueue printQueue in collection)
			{
				AssertNotNullOrEmpty(printQueue.SQ_ServerName);
			}
			AssertEquals("Should be only one table hit for print servers", 1, factory2.GetTableHitCount(StmPrintServerSchema.Constants.TableName));
		}
	}
}
