using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.RemotePrinting.Server.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing.Business
{
	[TestedType(typeof(StmPrintQueueExt))]
	class StmPrintQueueExtTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<StmPrintQueueExt>();
		}

		public void TestPrintJobs()
		{
			var printQueue = Factory.New<StmPrintQueueExt>();
			printQueue.SQ_ServerName = "Server1";
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_WebPrintServiceAddress = "http://10.61.224.23:4545/";
			printQueue.SQ_AllowPrinting = true;

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_Status = "FAL";
			printJob.SP_JobType = "PRN";

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_SQ = printQueue.PK;
			printJob2.SP_Status = "QUE";
			printJob2.SP_JobType = "PRN";

			var printJob3 = Factory.New<StmPrintJob>();
			printJob3.SP_SQ = printQueue.PK;
			printJob3.SP_Status = "WRK";
			printJob3.SP_JobType = "PRN";

			var printJob4 = Factory.New<StmPrintJob>();
			printJob4.SP_SQ = printQueue.PK;
			printJob4.SP_Status = "WRK";
			printJob4.SP_JobType = "PRN";
			Factory.Save();

			StmPrintQueueExtCollection printQueues = new StmPrintQueueExtCollection(Factory);
			printQueues.Load();
			StmPrintQueueExt stmPrintQueueExt = (StmPrintQueueExt)printQueues.FirstOrDefault(p => p.PK == printQueue.PK);

			AssertNotNull(stmPrintQueueExt);
			AssertEquals(1, stmPrintQueueExt.FailedPrintJobs);
			AssertEquals(1, stmPrintQueueExt.QueuedPrintJobs);
			AssertEquals(2, stmPrintQueueExt.WorkingPrintJobs);
		}
	}
}
