using System;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.UniversalDataBuss.ServiceTasks.Processing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	public class QueuedStmQueueStateQueueProviderTest : TransactionedTestCase
	{
		public void TestQueuedStmQueueStateQueueProvider()
		{
			var queueProvider = new QueuedStmQueueStateQueueProvider();
			StmQueueStateTestHelper.InsertRowIntoDatabase(UMIServiceTaskWorker.CODE, QueueStatusCodes.Codes.Queued, parentTableCode: "EM");
			AssertEquals(1, queueProvider.QueueResult.QueueSize);
			AssertEquals(true, queueProvider.QueueResult.MaximumItemAge.TotalMilliseconds >= 0);
		}

		public void TestParentTableCodeConstraint()
		{
			var validParentTableCode = "EM";
			var invalidParentTableCode = "ZZZ";

			AssertNoExceptionThrown(() => StmQueueStateTestHelper.InsertRowIntoDatabase(UMIServiceTaskWorker.CODE, QueueStatusCodes.Codes.Queued, parentTableCode: validParentTableCode));

			var ex = AssertExceptionThrown<Exception>(() => StmQueueStateTestHelper.InsertRowIntoDatabase(UMIServiceTaskWorker.CODE, QueueStatusCodes.Codes.Queued, parentTableCode: invalidParentTableCode));
			AssertContains("The INSERT statement conflicted with the CHECK constraint \"Constraint_SQS_ParentTableCode_NoCheck\".", ex.Message);
		}
	}
}
