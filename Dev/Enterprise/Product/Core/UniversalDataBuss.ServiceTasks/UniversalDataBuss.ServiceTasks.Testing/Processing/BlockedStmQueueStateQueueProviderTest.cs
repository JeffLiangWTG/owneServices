using Enterprise.Scheduler.GraphEngine;
using Enterprise.UniversalDataBuss.ServiceTasks.Processing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	public class BlockedStmQueueStateQueueProviderTest : TransactionedTestCase
	{
		public void TestBlockedStmQueueStateQueueProvider()
		{
			var queueProvider = new BlockedStmQueueStateQueueProvider();
			var startingQueueResult = queueProvider.QueueResult;
			StmQueueStateTestHelper.InsertRowIntoDatabase(UMIServiceTask.CODE, QueueStatusCodes.Codes.Blocked);
			AssertEquals(1, queueProvider.QueueResult.QueueSize - startingQueueResult.QueueSize);
		}
	}
}
