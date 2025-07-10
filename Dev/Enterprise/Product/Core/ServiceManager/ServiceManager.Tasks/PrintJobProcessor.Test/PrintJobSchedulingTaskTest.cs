using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing;

[TestedType(typeof(PrintJobSchedulingTask))]
sealed class PrintJobSchedulingTaskTest : ServiceTaskTestCase<PrintJobSchedulingTask>
{
	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);

		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", "PJQ", hostedServiceAttribute.Code);
			AssertEquals("Description", "Print Jobs Scheduling", hostedServiceAttribute.Description);
			AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
			AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
			AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("ActiveByDefault", true, hostedServiceAttribute.ActiveByDefault);
		});
	}

	public void TestServiceTaskCanRunInAnyBranch()
	{
		AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);

		var task = new PrintJobSchedulingTask { ServiceLogger = new TestServiceLogger() };

		using (PrintJobTaskTestHelper.ClearUserContext())
		using (Env.Instance.TemporaryServiceTaskContext(PrintJobSchedulingTask.Code, canRunInAnyBranch: true))
		{
			AssertNoExceptionThrown(() => task.RunTask());
		}

		AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
		ErrorReporter.Clear();
	}

	public void TestTaskRun_NewStmPrintJobQueueRecordIsCreated()
	{
		const long currentMaxSequence = 100;

		var oldPrintJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML));
		var oldPrintJobQueue = TestHelper.AddPrintJobToQueue(oldPrintJob);
		oldPrintJobQueue.SPQ_Sequence = currentMaxSequence;

		var printServer = Factory.New<StmPrintServer>();
		printServer.SPS_ServerName = "Server1";

		var printQueue = Factory.New<StmPrintQueue>();
		printQueue.SQ_ServerName = "Server1";
		printQueue.SQ_QueueName = "Queue1";
		printQueue.SQ_DisplayName = "QueueDisplay1";
		printQueue.SQ_WebPrintServiceAddress = "ServiceAddress1";
		printQueue.SQ_SPS_Server = printServer.PK;

		var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML));
		printJob.SP_SQ = printQueue.PK;
		printJob.SP_RunDateTime = new ZDateTime(2022, 2, 2, 2, 2, 2, DateTimeKind.Utc);

		Factory.Save();
		RunServiceTask();

		var printJobQueue = Factory.LoadTop1<StmPrintJobQueue>(new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob.PK));

		AssertNotNull("A new record should exist in StmPrintJobQueue table", printJobQueue);
		AssertEquals(printJob.SP_JobType, printJobQueue.SPQ_JobType);
		AssertEquals(currentMaxSequence + 1, printJobQueue.SPQ_Sequence);
		AssertEquals(printJob.SP_EDocsProcessed, printJobQueue.SPQ_EDocsProcessed);
		AssertEquals(printJob.SP_SB_DeliveryGroup, printJobQueue.SPQ_SB_DeliveryGroup);
		AssertEquals(printJob.SP_ParentGuid.ToString().ToUpperInvariant(), printJobQueue.SPQ_ParentGuidForLock);
		AssertEquals(printJob.SP_SB_DeliveryGroup.ToString().ToUpperInvariant(), printJobQueue.SPQ_DeliveryGroupForLock);
		AssertEquals(printServer.PK, printJobQueue.SPQ_SPS_PrintServer);
		AssertEquals(printQueue.PK, printJobQueue.SPQ_SQ_PrintQueue);
	}

	public void TestTaskRun_PrintJobsAreSavedInDeliveryGroup()
	{
		var deliveryGroup1 = Factory.New<StmDeliveryGroup>();
		deliveryGroup1.SB_IsProcessed = true;
		TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroup1.PK);
		TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroup1.PK);

		var deliveryGroup2 = Factory.New<StmDeliveryGroup>();
		deliveryGroup2.SB_IsProcessed = true;
		TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroup2.PK);
		TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroup2.PK);

		Factory.Save();

		var logger = RunServiceTask();

		AssertEquals(8, logger.Count);
		AssertStartsWith("Delivery group 1 is saved", "Information|Added 2 print jobs of a same delivery group into the queue.", logger[2]);
		AssertStartsWith("Delivery group 2 is saved", "Information|Added 2 print jobs of a same delivery group into the queue.", logger[3]);
	}

	public void TestTaskRun_OnlyLoadPrintJobsNotInStmPrintJobQueue()
	{
		var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML));
		var stmPrintJobQueue = Factory.New<StmPrintJobQueue>();
		stmPrintJobQueue.SPQ_JobType = printJob.SP_JobType;
		stmPrintJobQueue.SPQ_SB_DeliveryGroup = printJob.SP_SB_DeliveryGroup;
		stmPrintJobQueue.SPQ_SPS_PrintServer = printJob.PrintQueue?.SQ_SPS_Server ?? ZGuid.Empty;
		stmPrintJobQueue.SPQ_SP_PrintJob = printJob.PK;

		Factory.Save();

		AssertEquals("There should be only 1 StmPrintJobQueue record in the database", 1, Factory.GetDatabaseCount(typeof(StmPrintJobQueue)));
		AssertEquals("The only 1 record should be the existing one", 1, Factory.GetDatabaseCount(typeof(StmPrintJobQueue), new ZQuery(StmPrintJobQueueSchema.PK, stmPrintJobQueue.PK)));

		var serviceTask = new PrintJobSchedulingTask();
		InitialiseTaskSchedule(serviceTask);
		RunTaskSchedule(serviceTask);

		AssertEquals("There should be only 1 StmPrintJobQueue record in the database", 1, Factory.GetDatabaseCount(typeof(StmPrintJobQueue)));
		AssertEquals("The only 1 record should be the existing one", 1, Factory.GetDatabaseCount(typeof(StmPrintJobQueue), new ZQuery(StmPrintJobQueueSchema.PK, stmPrintJobQueue.PK)));

		stmPrintJobQueue.Delete();

		Factory.Save();

		AssertEquals("There should be no record in the table", 0, Factory.GetDatabaseCount(typeof(StmPrintJobQueue)));

		InitialiseTaskSchedule(serviceTask);
		RunTaskSchedule(serviceTask);

		AssertEquals("A new record was added into the table", 1, Factory.GetDatabaseCount(typeof(StmPrintJobQueue)));
	}

	public void TestTaskRun_OnlyLoadPrintJobsRunDateTimeIsPassed()
	{
		AssertReadyPrintJobIsAddedToTheQueue(unReadyPrintJob =>
		{
			unReadyPrintJob.SP_RunDateTime = ZDateTime.UtcNow.AddDays(1);
		});
	}

	public void TestTaskRun_OnlyLoadPrintJobsWhoseDeliveryGroupIsProcessed()
	{
		AssertReadyPrintJobIsAddedToTheQueue(unReadyPrintJob =>
		{
			unReadyPrintJob.DeliveryGroup.SB_IsProcessed = false;
		});
	}

	public void TestTaskRun_OnlyLoadPrintJobsWhoseRetryAttemptsIsLessThanThreeTimes()
	{
		AssertReadyPrintJobIsAddedToTheQueue(unReadyPrintJob =>
		{
			unReadyPrintJob.SP_RetryAttempts = 4;
		});
	}

	public void TestTaskRun_OnlyLoadPrintJobsNotFailed()
	{
		AssertReadyPrintJobIsAddedToTheQueue(unReadyPrintJob =>
		{
			unReadyPrintJob.SP_Status = nameof(PrintJobStatus.FAL);
		});
	}

	public void TestTaskRun_OnlyLoadPrintJobsWhoseEDocsProcessedIsFalse_WhenJobTypeIsDDS()
	{
		AssertReadyPrintJobIsAddedToTheQueue(unReadyPrintJob =>
		{
			unReadyPrintJob.SP_EDocsProcessed = true;
			unReadyPrintJob.SP_JobType = nameof(PrintJobType.DDS);
		}, readyPrintJob =>
		{
			readyPrintJob.SP_JobType = nameof(PrintJobType.DDS);
		});
	}

	public void TestTaskRun_OnlyLoadPrintJobsSigned_WhenSignByDOS()
	{
		AssertReadyPrintJobIsAddedToTheQueue(unReadyPrintJob =>
		{
			unReadyPrintJob.SP_SignBy = DocumentsSignBy.DOS;
			unReadyPrintJob.SP_IsSigned = false;
		}, readyPrintJob =>
		{
			readyPrintJob.SP_SignBy = DocumentsSignBy.DOS;
			readyPrintJob.SP_IsSigned = true;
		});
	}

	public void TestTaskRun_PrintJobsWithNoPrintServerWillNotBeLoaded_WhenJobTypeIsPRN()
	{
		var printServer = Factory.New<StmPrintServer>();
		printServer.SPS_ServerName = "Server1";

		var printQueue = Factory.New<StmPrintQueue>();
		printQueue.SQ_ServerName = "Server1";
		printQueue.SQ_QueueName = "Queue1";
		printQueue.SQ_DisplayName = "QueueDisplay1";
		printQueue.SQ_WebPrintServiceAddress = "ServiceAddress1";
		printQueue.SQ_SPS_Server = printServer.PK;

		AssertReadyPrintJobIsAddedToTheQueue(unReadyPrintJob =>
		{
			unReadyPrintJob.SP_JobType = nameof(PrintJobType.PRN);
		}, readyPrintJob =>
		{
			readyPrintJob.SP_JobType = nameof(PrintJobType.PRN);
			readyPrintJob.SP_SQ = printQueue.PK;
		});
	}

	public void TestTaskRun_PrintJobsWithPrintQueueDeletedWillNotBeLoaded_WhenJobTypeIsPRN()
	{
		var printServer = Factory.New<StmPrintServer>();
		printServer.SPS_ServerName = "Server1";

		var printQueue1 = Factory.New<StmPrintQueue>();
		printQueue1.SQ_ServerName = "Server1";
		printQueue1.SQ_QueueName = "Queue1";
		printQueue1.SQ_DisplayName = "QueueDisplay1";
		printQueue1.SQ_WebPrintServiceAddress = "ServiceAddress1";
		printQueue1.SQ_SPS_Server = printServer.PK;

		var printQueue2 = Factory.New<StmPrintQueue>();
		printQueue2.SQ_ServerName = "Server1";
		printQueue2.SQ_QueueName = "Queue2";
		printQueue2.SQ_DisplayName = "QueueDisplay2";
		printQueue2.SQ_WebPrintServiceAddress = "ServiceAddress1";
		printQueue2.SQ_SPS_Server = printServer.PK;
		printQueue2.SQ_QueueDeleted = ZDateTime.UtcNow;

		AssertReadyPrintJobIsAddedToTheQueue(unReadyPrintJob =>
		{
			unReadyPrintJob.SP_JobType = nameof(PrintJobType.PRN);
			unReadyPrintJob.SP_SQ = printQueue2.PK;
		}, readyPrintJob =>
		{
			readyPrintJob.SP_JobType = nameof(PrintJobType.PRN);
			readyPrintJob.SP_SQ = printQueue1.PK;
		});
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
	{
		new TaskNudgeInformationForTest(
			StmPrintJobSchema.Constants.TableName,
			"Print Jobs Scheduling",
			StmPrintJobSchema.Constants.SP_IsScheduled + "=0",
			StmPrintJobSchema.Constants.SP_Status + "!=FAL",
			StmPrintJobSchema.Constants.SP_SignBy + "!=DOS"),
		new TaskNudgeInformationForTest(
			StmPrintJobSchema.Constants.TableName,
			"Print Jobs Scheduling with Signed DOS",
			StmPrintJobSchema.Constants.SP_IsScheduled + "=0",
			StmPrintJobSchema.Constants.SP_Status + "!=FAL",
			StmPrintJobSchema.Constants.SP_SignBy + "=DOS",
			StmPrintJobSchema.Constants.SP_IsSigned + "=1")
	};

	public void TestPopulateSPQ_DeliveryGroupForLock()
	{
		var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML));
		Factory.Save();

		RunServiceTask();

		var printJobQueue = Factory.LoadTop1<StmPrintJobQueue>(new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob.PK));
		AssertNotNull(printJobQueue);

		AssertEquals("SPQ_DeliveryGroupForLock should have string value of SP_SB_DeliveryGroup",
			printJob.SP_SB_DeliveryGroup.ToString().ToUpperInvariant(), printJobQueue.SPQ_DeliveryGroupForLock);
	}

	public void TestPopulateSPQ_ParentGuidForLock()
	{
		var printJob1 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML));
		var printJob2 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML));
		printJob2.SP_ParentGuid = ZGuid.Empty;
		Factory.Save();

		RunServiceTask();

		var printJobQueue1 = Factory.LoadTop1<StmPrintJobQueue>(new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob1.PK));
		AssertNotNull(printJobQueue1);

		AssertEquals("SPQ_ParentGuidForLock should have string value of SP_ParentGuid",
			printJob1.SP_ParentGuid.ToString().ToUpperInvariant(), printJobQueue1.SPQ_ParentGuidForLock);

		var printJobQueue2 = Factory.LoadTop1<StmPrintJobQueue>(new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob2.PK));
		AssertNotNull(printJobQueue2);

		AssertEquals("SPQ_ParentGuidForLock should have string value of Print Job PK if SP_ParentGuid is empty",
			printJob2.PK.ToString().ToUpperInvariant(), printJobQueue2.SPQ_ParentGuidForLock);
	}

	public void TestPrintJobsFromSameDeliveryGroupAreQueuedTogether()
	{
		var deliveryGroup1 = Factory.New<StmDeliveryGroup>();
		deliveryGroup1.SB_IsProcessed = true;

		var deliveryGroup2 = Factory.New<StmDeliveryGroup>();
		deliveryGroup2.SB_IsProcessed = true;

		var deliveryGroup3 = Factory.New<StmDeliveryGroup>();
		deliveryGroup3.SB_IsProcessed = true;

		var sameRunTime1 = ZDateTime.UtcNow.AddMinutes(-2);
		var sameRunTime2 = ZDateTime.UtcNow.AddMinutes(-1);

		var printJob11 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup1.PK);
		var printJob21 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup2.PK);
		var printJob31 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup3.PK);
		var printJob12 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup1.PK);
		var printJob13 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup1.PK);
		var printJob32 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup3.PK);
		var printJob22 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup2.PK);

		printJob11.SP_RunDateTime = printJob12.SP_RunDateTime = printJob13.SP_RunDateTime = printJob31.SP_RunDateTime = printJob32.SP_RunDateTime = sameRunTime1;
		printJob21.SP_RunDateTime = printJob22.SP_RunDateTime = sameRunTime2;

		Factory.Save();

		var printJob33 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup3.PK);
		var printJob14 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup1.PK);
		var printJob23 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup2.PK);

		printJob14.SP_RunDateTime = printJob33.SP_RunDateTime = sameRunTime1;
		printJob23.SP_RunDateTime = sameRunTime2;

		Factory.Save();

		var deliveryGroup4 = Factory.New<StmDeliveryGroup>();
		deliveryGroup4.SB_IsProcessed = true;

		var printJob41 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup4.PK);
		var printJob34 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup3.PK);
		var printJob42 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup4.PK);

		printJob34.SP_RunDateTime = sameRunTime1;
		printJob41.SP_RunDateTime = printJob42.SP_RunDateTime = sameRunTime2;

		Factory.Save();

		RunServiceTask();

		// Check individual groups
		AssertSequencedTogether(printJob11.PK, printJob12.PK, printJob13.PK, printJob14.PK);
		AssertSequencedTogether(printJob21.PK, printJob22.PK, printJob23.PK);
		AssertSequencedTogether(printJob31.PK, printJob32.PK, printJob33.PK, printJob34.PK);
		AssertSequencedTogether(printJob41.PK, printJob42.PK);

		// Check all print jobs together - to ensure that sequence numbers are not repeated
		AssertSequencedTogether(printJob11.PK, printJob12.PK, printJob13.PK, printJob14.PK,
			printJob21.PK, printJob22.PK, printJob23.PK,
			printJob31.PK, printJob32.PK, printJob33.PK, printJob34.PK,
			printJob41.PK, printJob42.PK);
	}

	void AssertSequencedTogether(params ZGuid[] printJobPKs)
	{
		var minSequence = long.MaxValue;
		var maxSequence = long.MinValue;
		var sequences = new HashSet<long>();

		foreach (var printJobPK in printJobPKs)
		{
			var printJobQueue = Factory.LoadTop1<StmPrintJobQueue>(new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJobPK));
			AssertNotNull(printJobQueue);

			var sequence = printJobQueue.SPQ_Sequence;
			sequences.Add(sequence);
			if (minSequence > sequence) { minSequence = sequence; }
			if (maxSequence < sequence) { maxSequence = sequence; }
		}

		AssertEquals("All queued print jobs should have unique sequence", printJobPKs.Length, sequences.Count);
		AssertEquals("Sequence numbers should go together for listed print jobs", printJobPKs.Length, maxSequence - minSequence + 1);
	}

	public void TestPrintJobQueueGlobalOrder()
	{
		const long currentMaxSequence = 100;

		var printJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML));
		var printJobQueue = TestHelper.AddPrintJobToQueue(printJob);
		printJobQueue.SPQ_Sequence = currentMaxSequence;

		var deliveryGroup = Factory.New<StmDeliveryGroup>();
		deliveryGroup.SB_IsProcessed = true;

		var group1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
		var group2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
		var group3 = Guid.Parse("33333333-3333-3333-3333-333333333333");

		var printJob32 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup.PK);
		var printJob31 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup.PK);
		var printJob12 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup.PK);
		var printJob21 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup.PK);
		var printJob13 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup.PK);
		var printJob22 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup.PK);
		var printJob11 = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML), deliveryGroupGuid: deliveryGroup.PK);

		printJob11.SP_Group = group1;
		printJob11.SP_Sequence = 1;

		printJob12.SP_Group = group1;
		printJob12.SP_Sequence = 2;

		printJob13.SP_Group = group1;
		printJob13.SP_Sequence = 3;

		printJob21.SP_Group = group2;
		printJob21.SP_Sequence = 1;

		printJob22.SP_Group = group2;
		printJob22.SP_Sequence = 2;

		printJob31.SP_Group = group3;
		printJob31.SP_Sequence = 1;

		printJob32.SP_Group = group3;
		printJob32.SP_Sequence = 2;

		Factory.Save();

		AssertEquals("Precondition: there should be 1 existing StmPrintJobQueue record", 1, Factory.GetDatabaseCount(typeof(StmPrintJobQueue)));

		RunServiceTask();

		AssertSequenceOrder(currentMaxSequence + 1, printJob11.PK, printJob12.PK, printJob13.PK, printJob21.PK, printJob22.PK, printJob31.PK, printJob32.PK);
	}

	void AssertSequenceOrder(long startingSequence, params ZGuid[] printJobPKs)
	{
		var currentSequence = startingSequence;
		foreach (var printJobPK in printJobPKs)
		{
			var printJobQueue = Factory.LoadTop1<StmPrintJobQueue>(new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJobPK));
			AssertNotNull(printJobQueue);

			AssertEquals(currentSequence, printJobQueue.SPQ_Sequence);
			currentSequence++;
		}
	}

	#region Implementation

	void AssertReadyPrintJobIsAddedToTheQueue(Action<StmPrintJob> prepareUnReadyPrintJob, Action<StmPrintJob> prepareReadyPrintJob = default)
	{
		var readyPrintJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML));
		prepareReadyPrintJob?.Invoke(readyPrintJob);

		var unreadyPrintJob = TestHelper.CreateTestPrintJobOnly(nameof(PrintJobType.EML));
		prepareUnReadyPrintJob?.Invoke(unreadyPrintJob);

		Factory.Save();

		RunServiceTask();

		AssertEquals(1, Factory.GetDatabaseCount(typeof(StmPrintJobQueue)));
		AssertEquals(1, Factory.GetDatabaseCount(typeof(StmPrintJobQueue), new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, readyPrintJob.PK)));
	}

	TestServiceLogger RunServiceTask()
	{
		var serviceTask = new PrintJobSchedulingTask();
		var logger = new TestServiceLogger();
		serviceTask.ServiceLogger = logger;
		serviceTask.RunTask(CancellationToken.None);
		return logger;
	}

	PrintJobTaskTestHelper TestHelper => testHelper ??= new PrintJobTaskTestHelper(Factory);

	PrintJobTaskTestHelper testHelper;

	#endregion
}
