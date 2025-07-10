using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	class PrintJobSchedulingQueueTest : TestCaseWithFactory
	{
		public void TestQueueResult()
		{
			AddStmPrintJob(ZDateTime.UtcNow.AddMinutes(-1), "QUE", false);
			AddStmPrintJob(ZDateTime.UtcNow.AddMinutes(-3), "QUE", false);
			AddStmPrintJob(ZDateTime.UtcNow.AddMinutes(-5), "QUE", true);
			AddStmPrintJob(ZDateTime.UtcNow.AddMinutes(-7), "FAL", false);
			AddStmPrintJob(ZDateTime.UtcNow.AddMinutes(1), "QUE", false);
			AddStmPrintJob(ZDateTime.UtcNow.AddMinutes(5), "QUE", false);
			Factory.Save();

			var queueProvider = new PrintJobSchedulingQueue();
			var queueResult = queueProvider.QueueResult;

			AssertEquals("Should be 2 not-scheduled print jobs ready to print", 2, queueResult.QueueSize);
			AssertCloseEnough("Oldest matching print jobs should be 3 mintues old", 180, (int)queueResult.MaximumItemAge.TotalSeconds, 30);
		}

		void AddStmPrintJob(ZDateTime runDateTime, string status, bool isScheduled)
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RunDateTime = runDateTime;
			printJob.SP_Status = status;
			printJob.SP_IsScheduled = isScheduled;
		}
	}
}
