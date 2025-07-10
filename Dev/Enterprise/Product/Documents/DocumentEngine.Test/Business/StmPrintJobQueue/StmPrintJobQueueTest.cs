using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing;

[TestedType(typeof(StmPrintJobQueue))]
sealed class StmPrintJobQueueTest : EnterpriseBusinessObjectTestCase
{
	public void TestStmPrintJobQueue_Insert_MarkJobAsScheduled()
	{
		var job = Factory.NewWithValidTestData<StmPrintJob>();
		job.SP_JobType = "EML";
		job.SP_RunDateTime = ZDateTime.UtcNow;
		Factory.Save();

		AssertEquals(false, job.SP_IsScheduled);
		job.Reload();
		AssertEquals(false, job.SP_IsScheduled);

		AddPrintJobToQueue(job);
		AssertEquals(false, job.SP_IsScheduled);

		Factory.Save();
		AssertEquals(false, job.SP_IsScheduled);

		job.Reload();
		AssertEquals("SP_IsScheduled should be set in database", true, job.SP_IsScheduled);
	}

	public void TestStmPrintJobQueue_Delete_MarkJobAsUnscheduled()
	{
		var job = Factory.NewWithValidTestData<StmPrintJob>();
		job.SP_JobType = "EML";
		job.SP_RunDateTime = ZDateTime.UtcNow;
		Factory.Save();

		var jobQueue = AddPrintJobToQueue(job);
		Factory.Save();

		job.Reload();
		AssertEquals(true, job.SP_IsScheduled);

		jobQueue.Delete();

		AssertEquals(true, job.SP_IsScheduled);

		Factory.Save();
		AssertEquals(true, job.SP_IsScheduled);

		job.Reload();
		AssertEquals("SP_IsScheduled should be reset in database", false, job.SP_IsScheduled);
	}

	public void TestStmPrintJobQueue_Delete_MarkJobAsUnscheduled_Sql()
	{
		var job = Factory.NewWithValidTestData<StmPrintJob>();
		job.SP_JobType = "EML";
		job.SP_RunDateTime = ZDateTime.UtcNow;
		Factory.Save();

		var jobQueue = AddPrintJobToQueue(job);
		Factory.Save();

		job.Reload();
		AssertEquals(true, job.SP_IsScheduled);

		using (var command = TestConnection.Command($"DELETE StmPrintJobQueue WHERE SPQ_PK = '{jobQueue.PK}'"))
		{
			command.ExecuteNonQuery();
		}
		AssertEquals(true, job.SP_IsScheduled);

		job.Reload();
		AssertEquals("SP_IsScheduled should be reset in database", false, job.SP_IsScheduled);
	}

	public void TestStmPrintJobChange_EDocsProcessed_Update_StmPRintJobQueue()
	{
		var job = Factory.NewWithValidTestData<StmPrintJob>();
		job.SP_JobType = "EML";
		job.SP_RunDateTime = ZDateTime.UtcNow;
		job.SP_EDocsProcessed = false;
		Factory.Save();

		var jobQueue = AddPrintJobToQueue(job);
		Factory.Save();

		AssertEquals("SPQ_EDocsProcessed should not be set yet", false, jobQueue.SPQ_EDocsProcessed);

		job.SP_EDocsProcessed = true;
		Factory.Save();

		jobQueue.Reload();

		AssertEquals("SPQ_EDocsProcessed should be set", true, jobQueue.SPQ_EDocsProcessed);
	}

	public void TestStmPrintJobChange_Type_Statuse_Retries_Delete_StmPrintJobQueue()
	{
		AssertStmPrintJobChange_Delete_StmPrintJobQueue(job => { job.SP_JobType = "DDS"; }, true);
		AssertStmPrintJobChange_Delete_StmPrintJobQueue(job => { job.SP_Status = "FAL"; }, true);
		AssertStmPrintJobChange_Delete_StmPrintJobQueue(job => { job.SP_RetryAttempts = (byte)(job.SP_RetryAttempts + 1); }, true);

		AssertStmPrintJobChange_Delete_StmPrintJobQueue(job => { job.SP_EDocsProcessed = true; }, false);
	}

	public void TestStmPrintJobDelete_Delete_StmPrintJobQueue()
	{
		AssertStmPrintJobChange_Delete_StmPrintJobQueue(job => { job.Delete(); }, true);
	}

	void AssertStmPrintJobChange_Delete_StmPrintJobQueue(Action<StmPrintJob> changeJob, bool shouldJobQueueBeDeleted)
	{
		var job = Factory.NewWithValidTestData<StmPrintJob>();
		job.SP_JobType = "EML";
		job.SP_RunDateTime = ZDateTime.UtcNow;
		job.SP_Status = "QUE";
		job.SP_RetryAttempts = 0;
		Factory.Save();

		var jobQueue = AddPrintJobToQueue(job);
		Factory.Save();

		AssertNotNull("StmPrintJobQueue record should exist in DB", new BusinessObjectFactory { RefreshEnabled = false }.Load<StmPrintJobQueue>(jobQueue.PK));

		changeJob(job);
		Factory.Save();

		var reloadedJobQueue = new BusinessObjectFactory { RefreshEnabled = false }.Load<StmPrintJobQueue>(jobQueue.PK);
		if (shouldJobQueueBeDeleted)
		{
			AssertNull("StmPrintJobQueue record should be deleted in DB", reloadedJobQueue);
		}
		else
		{
			AssertNotNull("StmPrintJobQueue record should remain in DB", reloadedJobQueue);
		}
	}

	StmPrintJobQueue AddPrintJobToQueue(StmPrintJob printJob)
	{
		var printJobQueue = printJob.Factory.New<StmPrintJobQueue>();
		printJobQueue.SPQ_JobType = printJob.SP_JobType;
		printJobQueue.SPQ_SP_PrintJob = printJob.PK;
		printJobQueue.SPQ_SB_DeliveryGroup = printJob.SP_SB_DeliveryGroup;
		printJobQueue.SPQ_EDocsProcessed = printJob.SP_EDocsProcessed;
		printJobQueue.SPQ_Sequence = 0;

		return printJobQueue;
	}
}
