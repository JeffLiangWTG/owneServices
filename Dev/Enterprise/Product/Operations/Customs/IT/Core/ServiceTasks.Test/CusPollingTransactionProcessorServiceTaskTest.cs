using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.IT.ServiceTasks.Testing;

[TestedType(typeof(CusPollingTransactionProcessorServiceTask))]
sealed class CusPollingTransactionProcessorServiceTaskTest : MessagingServiceTaskTest<CusPollingTransactionProcessorServiceTask>
{
	public void TestInitializeSchedule()
	{
		var testTask = new CusPollingTransactionProcessorServiceTask();
		InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);

		CombineAssertions(() =>
		{
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.DaysRange);
			AssertEquals("TaskPeriodCount", 1, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);

			var dailyUtcStartTime = taskSchedule.Recurrence.RecurringStartTimeUtc;
			AssertEquals("DailyStartTime", "21:00:00", $"{dailyUtcStartTime.Hour}:{dailyUtcStartTime.Minute:#00}:{dailyUtcStartTime.Second:#00}");
		});
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

	[TestDate]
	public void TestRunTask_ProcessIvistoPollingTransactions()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var interchangeToBeReQueued = helper.CreateIvistoRequestInterchange();
		var pollingTransactionToBeReQueued = helper.CreateIvistoPollingTransaction("OPN", ZDateTime.Now.AddDays(-1), interchangeToBeReQueued, ZDateTime.Now.AddDays(-1));

		var interchangeWhoseTransactionToBeDeleted = helper.CreateIvistoRequestInterchange();
		var pollingTransactionToBeDeleted = helper.CreateIvistoPollingTransaction("OPN", ZDateTime.Now.AddDays(-1), interchangeWhoseTransactionToBeDeleted, ZDateTime.Now.AddDays(-241));

		var interchangeWhoseTransactionToBeSkip = helper.CreateIvistoRequestInterchange();
		var pollingTransactionToBeSkip = helper.CreateIvistoPollingTransaction("OPN", ZDateTime.Now.AddDays(+1), interchangeToBeReQueued, ZDateTime.Now.AddDays(-1));
		Factory.Save();

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertEquals("Interchange Status", "SNT", interchangeToBeReQueued.EI_Status);
			AssertEquals("Polling Transaction Status", "OPN", pollingTransactionToBeReQueued.CPT_Status);

			AssertEquals("Interchange Status", "SNT", interchangeWhoseTransactionToBeDeleted.EI_Status);
			AssertEquals("Polling Transaction Status", "OPN", pollingTransactionToBeDeleted.CPT_Status);

			AssertEquals("Interchange Status", "SNT", interchangeWhoseTransactionToBeSkip.EI_Status);
			AssertEquals("Polling Transaction Status", "OPN", pollingTransactionToBeSkip.CPT_Status);
		});

		var logger = InitialiseAndRunTaskSchedule(new CusPollingTransactionProcessorServiceTask());

		interchangeToBeReQueued.ReloadSafe();
		pollingTransactionToBeReQueued.ReloadSafe();
		interchangeWhoseTransactionToBeDeleted.ReloadSafe();
		interchangeWhoseTransactionToBeSkip.ReloadSafe();
		pollingTransactionToBeSkip.ReloadSafe();

		CombineAssertions("POST-CONDITION", () =>
		{
			var actualLogs = logger.ToString();

			AssertEquals("Re-Queued interchange status", "QUE", interchangeToBeReQueued.EI_Status);
			AssertEquals("Re-Queued polling transaction status", "PND", pollingTransactionToBeReQueued.CPT_Status);
			AssertContains("Re-Queued polling transaction log", "1 CusPollingTransaction(s) have been re-queued for transmission.", actualLogs);

			var separateFactory = new BusinessObjectFactory();
			var isPollingTransactionDeleted = !separateFactory.Exists(typeof(CusPollingTransaction), new ZQuery(CusPollingTransactionSchema.PK, pollingTransactionToBeDeleted.PK));
			AssertEquals("Interchange status whose transaction has been deleted", "SNT", interchangeWhoseTransactionToBeDeleted.EI_Status);
			AssertEquals("Is polling transaction deleted ", true, isPollingTransactionDeleted);
			AssertContains("Deleted polling transaction log", "1 CusPollingTransaction(s) have been deleted due to expiration.", actualLogs);

			AssertEquals("Skipped interchange status", "SNT", interchangeWhoseTransactionToBeSkip.EI_Status);
			AssertEquals("Skipped polling transaction status", "OPN", pollingTransactionToBeSkip.CPT_Status);
			AssertContains("Skipped polling transaction log", "1 CusPollingTransaction(s) have been ignored because CPT_EarliestTimeOfNextAttemptUtc is in the future.", actualLogs);
		});
	}

	[TestDate]
	public void TestRunTask_ProcessIrildesPollingTransactions()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var interchangeToBeReQueued = helper.CreateIrildesRequestInterchange();
		var pollingTransactionToBeReQueued = helper.CreateIrildesPollingTransaction("OPN", ZDateTime.Now.AddDays(-1), interchangeToBeReQueued, ZDateTime.Now.AddDays(-1));

		var interchangeWhoseTransactionToBeDeleted = helper.CreateIrildesRequestInterchange();
		var pollingTransactionToBeDeleted = helper.CreateIrildesPollingTransaction("OPN", ZDateTime.Now.AddDays(-1), interchangeWhoseTransactionToBeDeleted, ZDateTime.Now.AddDays(-241));

		var interchangeWhoseTransactionToBeSkip = helper.CreateIrildesRequestInterchange();
		var pollingTransactionToBeSkip = helper.CreateIrildesPollingTransaction("OPN", ZDateTime.Now.AddDays(+1), interchangeToBeReQueued, ZDateTime.Now.AddDays(-1));
		Factory.Save();

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertEquals("Interchange Status", "SNT", interchangeToBeReQueued.EI_Status);
			AssertEquals("Polling Transaction Status", "OPN", pollingTransactionToBeReQueued.CPT_Status);

			AssertEquals("Interchange Status", "SNT", interchangeWhoseTransactionToBeDeleted.EI_Status);
			AssertEquals("Polling Transaction Status", "OPN", pollingTransactionToBeDeleted.CPT_Status);

			AssertEquals("Interchange Status", "SNT", interchangeWhoseTransactionToBeSkip.EI_Status);
			AssertEquals("Polling Transaction Status", "OPN", pollingTransactionToBeSkip.CPT_Status);
		});

		var logger = InitialiseAndRunTaskSchedule(new CusPollingTransactionProcessorServiceTask());

		interchangeToBeReQueued.ReloadSafe();
		pollingTransactionToBeReQueued.ReloadSafe();
		interchangeWhoseTransactionToBeDeleted.ReloadSafe();
		interchangeWhoseTransactionToBeSkip.ReloadSafe();
		pollingTransactionToBeSkip.ReloadSafe();

		CombineAssertions("POST-CONDITION", () =>
		{
			var actualLogs = logger.ToString();

			AssertEquals("Re-Queued interchange status", "QUE", interchangeToBeReQueued.EI_Status);
			AssertEquals("Re-Queued polling transaction status", "PND", pollingTransactionToBeReQueued.CPT_Status);
			AssertContains("Re-Queued polling transaction log", "1 CusPollingTransaction(s) have been re-queued for transmission.", actualLogs);

			var separateFactory = new BusinessObjectFactory();
			var isPollingTransactionDeleted = !separateFactory.Exists(typeof(CusPollingTransaction), new ZQuery(CusPollingTransactionSchema.PK, pollingTransactionToBeDeleted.PK));
			AssertEquals("Interchange status whose transaction has been deleted", "SNT", interchangeWhoseTransactionToBeDeleted.EI_Status);
			AssertEquals("Is polling transaction deleted ", true, isPollingTransactionDeleted);
			AssertContains("Deleted polling transaction log", "1 CusPollingTransaction(s) have been deleted due to expiration.", actualLogs);

			AssertEquals("Skipped interchange status", "SNT", interchangeWhoseTransactionToBeSkip.EI_Status);
			AssertEquals("Skipped polling transaction status", "OPN", pollingTransactionToBeSkip.CPT_Status);
			AssertContains("Skipped polling transaction log", "1 CusPollingTransaction(s) have been ignored because CPT_EarliestTimeOfNextAttemptUtc is in the future.", actualLogs);
		});
	}

	protected override void AssertSpecificHostedServiceAttributeProperties(HostedServiceAttribute attribute)
	{
		AssertEquals("Code", "ITI", attribute.Code);
		AssertEquals("Description", ServiceTaskCodeList.Descriptions.CusPollingTransactionProcessor, attribute.Description);
		AssertEquals("Type", typeof(CusPollingTransactionProcessorServiceTask), attribute.Type);
		AssertEquals("MinimumPeriod", "1day", attribute.MinimumPeriod);
		AssertEquals("DefaultScheduleRunEvery", "1day", attribute.DefaultScheduleRunEvery);
		AssertEquals("DefaultScheduleStartAtUtc", "21hours", attribute.DefaultScheduleStartAtUtc);
	}
}
