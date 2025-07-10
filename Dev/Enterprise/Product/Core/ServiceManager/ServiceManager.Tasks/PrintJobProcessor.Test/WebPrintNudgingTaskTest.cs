using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing;

[TestedType(typeof(WebPrintNudgingTask))]
sealed class WebPrintNudgingTaskTest : ServiceTaskTestCase<WebPrintNudgingTask>
{
	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);

		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", "WPN", hostedServiceAttribute.Code);
			AssertEquals("Description", "WebPrint Nudging", hostedServiceAttribute.Description);
			AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
			AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("IsMandatory", false, hostedServiceAttribute.IsMandatory);
			AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("MinimumPeriod", "1hour", hostedServiceAttribute.DefaultScheduleRunEvery);
			AssertEquals("ActiveByDefault", true, hostedServiceAttribute.ActiveByDefault);
		});
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
	{
		new TaskNudgeInformationForTest(
			StmPrintJobQueueSchema.Constants.TableName,
			"WebPrint Jobs Nudging",
			StmPrintJobQueueSchema.Constants.SPQ_JobType + "=PRN")
	};

	public void TestServiceTaskCanRunInAnyBranch()
	{
		AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);

		var task = new WebPrintNudgingTaskForTest { ServiceLogger = new TestServiceLogger() };

		using (PrintJobTaskTestHelper.ClearUserContext())
		using (Env.Instance.TemporaryServiceTaskContext(WebPrintNudgingTask.Code, canRunInAnyBranch: true))
		{
			AssertNoExceptionThrown(() => task.RunTask());
		}

		AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
		ErrorReporter.Clear();
	}

	public void TestGetNudgeablePrintQueues()
	{
		AssertNudgeablePrintQueue("some_url", "PRN", true, false, false, true); // No existing watermark
		AssertNudgeablePrintQueue("some_url", "PRN", true, true, true, true); // Watermark to non-existing print job
		AssertNudgeablePrintQueue("some_url", "PRN", true, true, false, false); // Watermark to exintisg print job
		AssertNudgeablePrintQueue("some_url", "EML", true, true, false, false); // DOD print job
		AssertNudgeablePrintQueue("", "PRN", true, true, true, false); // No web address on print queue
	}

	void AssertNudgeablePrintQueue(string webPrintServiceAddress, string jobType, bool addToQueue, bool addWaterMark, bool watermarkToDifferentGuid, bool expectQueueToBeNudgeable)
	{
		var printServer = Factory.NewWithValidTestData<StmPrintServer>();

		var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
		printQueue.SQ_SPS_Server = printServer.PK;
		printQueue.SQ_WebPrintServiceAddress = webPrintServiceAddress;

		var printJobQueue = CreatePrintJobAndNudgeWaterMark(printQueue, jobType, addToQueue, addWaterMark, watermarkToDifferentGuid, out _);

		Factory.Save();

		var task = new WebPrintNudgingTaskForTest { ServiceLogger = new TestServiceLogger() };
		task.RunTask();

		if (!expectQueueToBeNudgeable)
		{
			AssertEquals("Should not send any nudges", 0, task.NudgesLog.Count);
		}
		else
		{
			AssertEquals("Should have 1 nudge logged", 1, task.NudgesLog.Count);
			AssertEquals(printQueue.PK, task.NudgesLog[0].Item1);
			AssertEquals(printJobQueue.SPQ_SP_PrintJob, task.NudgesLog[0].Item2);
		}
	}

	public void TestUpdateWebPrintNudgeWaterMark()
	{
		AssertNudgeWatermark("some_url", "PRN", true, false, false, true); // No existing watermark
		AssertNudgeWatermark("some_url", "PRN", true, true, true, true); // Watermark to non-existing print job
		AssertNudgeWatermark("some_url", "PRN", true, true, false, false); // Watermark to exintisg print job
		AssertNudgeWatermark("some_url", "EML", true, true, false, false); // DOD print job
		AssertNudgeWatermark("", "PRN", true, true, true, false); // No web address on print queue
	}

	void AssertNudgeWatermark(string webPrintServiceAddress, string jobType, bool addToQueue, bool addOriginalWaterMark, bool watermarkToDifferentGuid, bool expectUpdatedNudgeWatermark)
	{
		var printServer = Factory.NewWithValidTestData<StmPrintServer>();

		var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
		printQueue.SQ_SPS_Server = printServer.PK;
		printQueue.SQ_WebPrintServiceAddress = webPrintServiceAddress;

		var printJobQueue = CreatePrintJobAndNudgeWaterMark(printQueue, jobType, addToQueue, addOriginalWaterMark, watermarkToDifferentGuid, out var originalNudgeWatermark);

		Factory.Save();

		var task = new WebPrintNudgingTaskForTest { ServiceLogger = new TestServiceLogger() };
		task.RunTask();

		var newNudgeWatermarks = LoadNudgeWatermark(printQueue);

		if (!expectUpdatedNudgeWatermark && !addOriginalWaterMark)
		{
			AssertEquals("Should not have any nudge watermarks", 0, newNudgeWatermarks.Length);
		}
		else
		{
			AssertEquals("There should be only 1 nudge watermarks for print queue", 1, newNudgeWatermarks.Length);

			var newNudgeWatermark = newNudgeWatermarks[0];

			if (addOriginalWaterMark)
			{
				AssertEquals(originalNudgeWatermark.PK, newNudgeWatermark.PK);
			}

			if (expectUpdatedNudgeWatermark)
			{
				AssertEquals(printJobQueue.PK, newNudgeWatermark.SD_GuidValue);
			}
			else if (watermarkToDifferentGuid)
			{
				AssertNotEquals(printJobQueue.PK, newNudgeWatermark.SD_GuidValue);
			}
		}
	}

	#region Implementation

	PrintJobTaskTestHelper TestHelper => testHelper ??= new PrintJobTaskTestHelper(Factory);
	PrintJobTaskTestHelper testHelper;

	StmPrintJobQueue CreatePrintJobAndNudgeWaterMark(StmPrintQueue printQueue, string jobType, bool addToQueue, bool addWaterMark, bool watermarkToDifferentGuid, out StmData nudgeWatermark)
	{
		var printJob = TestHelper.CreateTestPrintJobOnly(jobType, printQueueGuid: printQueue.PK);

		StmPrintJobQueue printJobQueue = null;
		if (addToQueue)
		{
			printJobQueue = TestHelper.AddPrintJobToQueue(printJob);
		}

		if (addWaterMark)
		{
			nudgeWatermark = printQueue.Factory.New<StmData>();
			nudgeWatermark.SD_Name = WebPrintNudgingTask.WebPrintNudgeWaterMark;
			nudgeWatermark.SD_Owner = printQueue.SQ_SPS_Server;
			nudgeWatermark.SD_DepartmentGuid  = printQueue.PK;

			nudgeWatermark.SD_GuidValue = watermarkToDifferentGuid || printJobQueue == null ? ZGuid.NewZGuid() : printJobQueue.PK;
		}
		else
		{
			nudgeWatermark = null;
		}

		return printJobQueue;
	}

	StmData[] LoadNudgeWatermark(StmPrintQueue printQueue)
	{
		var query = new ZQuery(StmDataSchema.SD_Name, WebPrintNudgingTask.WebPrintNudgeWaterMark);
		query.AddToFilter(StmDataSchema.SD_Owner, printQueue.SQ_SPS_Server);
		query.AddToFilter(StmDataSchema.SD_DepartmentGuid, printQueue.PK);
		query.ReLoadExistingRows = true;

		return printQueue.Factory.Load<StmData>(query);
	}

	#endregion

	class WebPrintNudgingTaskForTest : WebPrintNudgingTask
	{
		protected override void NudgePrintServerCore(StmPrintQueue printQueue, ZGuid printJobPk)
		{
			NudgesLog.Add((printQueue.PK, printJobPk));
		}

		public List<(ZGuid, ZGuid)> NudgesLog { get; } = new();
	}
}
