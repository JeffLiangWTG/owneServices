using Enterprise.Scheduler.GraphEngine;
using Enterprise.UniversalDataBuss.ServiceTasks.Processing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	public class PreKeyStmQueueStateQueueProviderTest : TransactionedTestCase
	{
		public void TestPreKeyStmQueueStateQueueProvider()
		{
			var queueProvider = new PreKeyStmQueueStateQueueProvider();
			var startingQueueResult = queueProvider.QueueResult;
			StmQueueStateTestHelper.InsertRowIntoDatabase(UMIServiceTask.CODE, QueueStatusCodes.Codes.PreKey);
			AssertEquals(1, queueProvider.QueueResult.QueueSize - startingQueueResult.QueueSize);
		}
	}
}
