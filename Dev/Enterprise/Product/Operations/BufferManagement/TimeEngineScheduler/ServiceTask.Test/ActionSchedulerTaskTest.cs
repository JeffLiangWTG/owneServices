using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Integration.Forwarding;
using BusinessConstants = Enterprise.TimeEngineScheduler.Integration.Constants;
using StatusConstants = Enterprise.TimeEngineScheduler.Integration.Constants.TimeActionScheduleStatus;

namespace Enterprise.TimeEngineScheduler.ServiceTask.Test
{
	[TestedType(typeof(ActionSchedulerTask))]
	public class ActionSchedulerTaskTest : ServiceTaskTestCase<ActionSchedulerTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attributes = typeof(ActionSchedulerTask).Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false);
			var attribute = Array.Find((HostedServiceAttribute[])attributes, a => a.TypeName == typeof(ActionSchedulerTask).FullName);
			AssertEquals("Minimum Period should be 1 minute", "1minute", attribute.MinimumPeriod);
			AssertEquals("Should be able to run in any branch since any uses of CurrentBranch and CurrentDepartment will cause inconsistent behaviour, so should be carefully analysed.", true, attribute.CanRunInAnyBranch);
		}

		public void TestSchedulerTask_RunsTestAction()
		{
			ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks = 1;

			var provider = new ActionScheduleProvider();
			var schedule = provider.ScheduleAction("TST", ZDateTime.UtcNow.AddMinutes(-1), ZGuid.NewZGuid(), "JS", "|EVT=Z00|OFF=000:00|ACT=Reference1|EST=N");

			var task = GetSchedulerTask();
			task.RunTask(CancellationToken.None);

			var loadedSchedule = new BusinessObjectFactory().Load<TimeActionSchedule>(schedule.PK);
			AssertEquals(StatusConstants.Closed, loadedSchedule.TAS_ExecutionStatus);
			AssertEquals("Success", loadedSchedule.TAS_ExecutionResult);
			AssertEquals(@"Debug|Started executing scheduled actions
Information|Loaded batch of size [1]
Information|Executed scheduled action TST
Debug|Processed batch of size [1]
Information|Finished batch of size [1]
Debug|Finished executing scheduled actions
", logger.ToString());
		}

		[TestDate]
		public void TestSchedulerTask_RunsScheduledActions()
		{
			ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks = 1;

			var c = Factory.NewWithValidTestData<TimeActionSchedule>();
			c.TAS_ExecutionStatus = StatusConstants.Scheduled;
			c.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.AddMinutes(30);
			c.TAS_GB_Branch = Env.CurrentBranchPK;
			c.TAS_GE_Department = Env.CurrentDepartmentPK;
			Factory.Save();

			var action = new SchedulerTestAction();
			var actionFactory = new Mock<ISchedulerActionFactory>();
			actionFactory.Setup(p => p.GetSchedulerAction(It.IsAny<string>())).Returns(action);

			using (ObjectFactory.Substitute(actionFactory.Object))
			{
				var task = GetSchedulerTask();
				task.RunTask(CancellationToken.None);
			}

			Factory.ReloadAll<TimeActionSchedule>();

			Assert("Action was executed early", !action.WasExecuted);

			TestDateAttribute.AddHours(1);
			using (ObjectFactory.Substitute(actionFactory.Object))
			{
				var task = GetSchedulerTask();
				task.RunTask(CancellationToken.None);
			}

			Factory.ReloadAll<TimeActionSchedule>();

			Assert("Action was not executed", action.WasExecuted);
			AssertEquals(StatusConstants.Closed, c.TAS_ExecutionStatus);
			AssertEquals("Success", c.TAS_ExecutionResult);
			AssertContains($"Information|Executed scheduled action {c.TAS_ActionCode}", logger.ToString());
		}

		public void TestSchedulerTask_RerunsFailedActions()
		{
			ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks = 1;

			var c = Factory.NewWithValidTestData<TimeActionSchedule>();
			c.TAS_ExecutionStatus = StatusConstants.Scheduled;
			c.TAS_RetryAttempts = 1;
			c.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromHours(-1));
			c.TAS_GB_Branch = Env.CurrentBranchPK;
			c.TAS_GE_Department = Env.CurrentDepartmentPK;
			Factory.Save();

			var action = new SchedulerTestAction();
			var actionFactory = new Mock<ISchedulerActionFactory>();
			actionFactory.Setup(p => p.GetSchedulerAction(It.IsAny<string>())).Returns(action);

			using (ObjectFactory.Substitute(actionFactory.Object))
			{
				var task = GetSchedulerTask();
				task.RunTask(CancellationToken.None);
			}

			Factory.ReloadAll<TimeActionSchedule>();

			Assert("Action was not executed", action.WasExecuted);
			AssertEquals(StatusConstants.Closed, c.TAS_ExecutionStatus);
			AssertEquals("Success", c.TAS_ExecutionResult);
			AssertContains($"Information|Executed scheduled action {c.TAS_ActionCode}", logger.ToString());
		}

		[TestDate]
		public void TestSchedulerTask_LogsNotRegisteredActions()
		{
			var c = Factory.NewWithValidTestData<TimeActionSchedule>();
			c.TAS_ExecutionStatus = StatusConstants.Scheduled;
			c.TAS_GB_Branch = Env.CurrentBranchPK;
			c.TAS_GE_Department = Env.CurrentDepartmentPK;
			Factory.Save();

			var actionFactory = new Mock<ISchedulerActionFactory>();
			actionFactory.Setup(p => p.GetSchedulerAction(It.IsAny<string>())).Returns<ISchedulerAction>(null);

			using (ObjectFactory.Substitute(actionFactory.Object))
			{
				var task = GetSchedulerTask();
				task.RunTask(CancellationToken.None);
			}

			Factory.ReloadAll<TimeActionSchedule>();

			AssertEquals(StatusConstants.Failed, c.TAS_ExecutionStatus);
			AssertEquals("Action handler not registered", c.TAS_ExecutionResult);
			AssertContains($"Error|Error running scheduled action {c.TAS_ActionCode}. Action handler not registered.", logger.ToString());
		}

		public void TestSchedulerTask_LogsFailedTask()
		{
			var c = Factory.NewWithValidTestData<TimeActionSchedule>();
			c.TAS_ExecutionStatus = BusinessConstants.TimeActionScheduleStatus.Scheduled;
			c.TAS_RetryAttempts = 0;
			c.TAS_GB_Branch = Env.CurrentBranchPK;
			c.TAS_GE_Department = Env.CurrentDepartmentPK;
			Factory.Save();

			var action = new SchedulerTestAction(true);
			var actionFactory = new Mock<ISchedulerActionFactory>();
			actionFactory.Setup(p => p.GetSchedulerAction(It.IsAny<string>())).Returns(action);

			using (ObjectFactory.Substitute(actionFactory.Object))
			{
				var task = GetSchedulerTask();
				task.RunTask(CancellationToken.None);
			}

			Assert("Action was executed", !action.WasExecuted);
			AssertContains("Warning|Failure in processing [TimeActionSchedule], on attempt [1] due to exception [Just as planned", logger.ToString());
			AssertNotNull(ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		public void TestSchedulerTask_UpdatesSuccessfulTask()
		{
			var c = Factory.NewWithValidTestData<TimeActionSchedule>();
			c.TAS_ExecutionStatus = BusinessConstants.TimeActionScheduleStatus.Scheduled;
			c.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromMinutes(-1));
			Factory.Save();

			var message = "Success";
			var status = BusinessConstants.TimeActionScheduleStatus.Closed;
			var scheduleProvider = new Mock<IActionScheduleProvider>();

			using (ObjectFactory.Substitute(scheduleProvider.Object))
			{
				var task = GetSchedulerTask();
				task.SaveAction(c, status, message);
			}
			scheduleProvider.Verify(p => p.ChangeState(c));
			AssertEquals(status, c.TAS_ExecutionStatus);
			AssertEquals(message, c.TAS_ExecutionResult);
		}

		[TestDate]
		public void TestSchedulerTask_ReschedulesFailedTask()
		{
			var dateTime = ZDateTime.UtcNow;

			var c = Factory.NewWithValidTestData<TimeActionSchedule>();
			c.TAS_ExecutionStatus = BusinessConstants.TimeActionScheduleStatus.Scheduled;
			c.TAS_RetryAttempts = 0;
			c.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromMinutes(-1));
			Factory.Save();

			var message = "Error";
			var status = BusinessConstants.TimeActionScheduleStatus.Failed;
			var scheduleProvider = new Mock<IActionScheduleProvider>();

			using (ObjectFactory.Substitute(scheduleProvider.Object))
			{
				var task = GetSchedulerTask();
				task.SaveAction(c, status, message);
			}

			scheduleProvider.Verify(p => p.ChangeState(c));
			AssertEquals(BusinessConstants.TimeActionScheduleStatus.Scheduled, c.TAS_ExecutionStatus);
			AssertEquals(message, c.TAS_ExecutionResult);
			AssertEquals((byte)1, c.TAS_RetryAttempts);
			AssertEquals(dateTime.Add(BusinessConstants.RetryAfterPeriod), c.TAS_ExecutionDateTimeUtc);
		}

		public void TestSchedulerTask_DoesNotRescheduleFailedTaskOverMaxRetryAttempts()
		{
			var dateTime = ZDateTime.UtcNow.ToSmallDateTimeFloor();

			var c = Factory.NewWithValidTestData<TimeActionSchedule>();
			c.TAS_ExecutionStatus = BusinessConstants.TimeActionScheduleStatus.Failed;
			c.TAS_RetryAttempts = (byte)(BusinessConstants.MaxRetryAttempts - 1);
			c.TAS_ExecutionDateTimeUtc = dateTime;
			Factory.Save();

			var message = "Error";
			var status = BusinessConstants.TimeActionScheduleStatus.Failed;
			var scheduleProvider = new Mock<IActionScheduleProvider>();

			using (ObjectFactory.Substitute(scheduleProvider.Object))
			{
				var task = GetSchedulerTask();
				task.SaveAction(c, status, message);
			}

			scheduleProvider.Verify(p => p.ChangeState(c));
			AssertEquals(status, c.TAS_ExecutionStatus);
			AssertEquals(message, c.TAS_ExecutionResult);
			AssertEquals(BusinessConstants.MaxRetryAttempts, c.TAS_RetryAttempts);
			AssertEquals(dateTime, c.TAS_ExecutionDateTimeUtc);
		}

		[ExpectNoExceptions]
		public void TestSchedulerTask_WorkflowDelayedEventProcessor_ForwardingShipment_CausesNoException()
		{
			ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks = 1;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var dep1 = Factory.New<GlbDepartment>();
			dep1.GE_Code = "DP1";

			var shipment = Factory.New<IForwardingShipment>();

			Factory.Save();

			var provider = new ActionScheduleProvider();
			var schedule = provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddMinutes(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=BR1|DEP=DP1", branch1.PK, dep1.PK);

			AssertEquals("Precondition", StatusConstants.Scheduled, schedule.ExecutionStatus);

			var task = GetSchedulerTask();
			task.RunTask(CancellationToken.None);

			var loadedSchedule = (new BusinessObjectFactory()).Load<TimeActionSchedule>(schedule.PK);
			AssertEquals(StatusConstants.Closed, loadedSchedule.TAS_ExecutionStatus);
		}

		[TestDate(2022, 07, 03)]
		public void TestQueueSize()
		{
			Db.Connection.ExecuteNonQuery("truncate table dbo.TimeActionSchedule");
			var queueProvider = (IHostedServiceQueueProvider)new ActionSchedulerTaskQueue();

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_Code = "BR1";
			branch.GB_RL_NKHomePort = "AUMEL";

			var dep = Factory.New<GlbDepartment>();
			dep.GE_Code = "DP1";

			var shipment = Factory.New<IForwardingShipment>();
			Factory.Save();

			var provider = new ActionScheduleProvider();
			provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddDays(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference1|EST=N", branch.PK, dep.PK);
			provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddDays(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference2_1|EST=N", branch.PK, dep.PK);
			provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddDays(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference2_2|EST=N", branch.PK, dep.PK);
			provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddDays(1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference2_2|EST=N", branch.PK, dep.PK);
			provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddDays(1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference2_2|EST=N", branch.PK, dep.PK);
			provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddDays(1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference2_2|EST=N", branch.PK, dep.PK);

			var expectedAge = TimeSpan.FromDays(1);
			AssertEquals(3, queueProvider.QueueResult.QueueSize);
			AssertCloseEnough((int)expectedAge.TotalSeconds, (int)queueProvider.QueueResult.MaximumItemAge.TotalSeconds, 60);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(6);

			expectedAge = TimeSpan.FromDays(7);
			AssertEquals(6, queueProvider.QueueResult.QueueSize);
			AssertCloseEnough((int)expectedAge.TotalSeconds, (int)queueProvider.QueueResult.MaximumItemAge.TotalSeconds, 60);
		}

		#region Context Switching

		public void TestSchedulerTask_ShouldSwitchContext_WhenExecutingScheduledActions()
		{
			ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks = 1;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "BR2";
			branch2.GB_RL_NKHomePort = "AUMEL";

			var dep1 = Factory.New<GlbDepartment>();
			dep1.GE_Code = "DP1";

			var dep2 = Factory.New<GlbDepartment>();
			dep2.GE_Code = "DP2";

			var shipment = Factory.New<IForwardingShipment>();

			Factory.Save();

			var provider = new ActionScheduleProvider();
			var sch1 = provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddMinutes(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference1|EST=N", branch1.PK, dep1.PK);
			var sch2 = provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddMinutes(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference2|EST=N", branch2.PK, dep2.PK);

			var action = new SchedulerTestAction();
			var actionFactory = new Mock<ISchedulerActionFactory>();
			actionFactory.Setup(p => p.GetSchedulerAction(It.IsAny<string>())).Returns(action);

			using (ObjectFactory.Substitute(actionFactory.Object))
			{
				var task = GetSchedulerTask();
				task.RunTask(CancellationToken.None);
			}

			var newFactory = new BusinessObjectFactory();
			var loadedSch1 = newFactory.Load<TimeActionSchedule>(sch1.PK);
			var loadedSch2 = newFactory.Load<TimeActionSchedule>(sch2.PK);
			AssertEquals(StatusConstants.Closed, loadedSch1.TAS_ExecutionStatus);
			AssertEquals(StatusConstants.Closed, loadedSch2.TAS_ExecutionStatus);

			AssertEquals(2, action.ExecutionHistory.Count);
			var execution1 = action.ExecutionHistory.SingleOrDefault(e => e.ExecutionBranchCode == "BR1");
			AssertNotNull(execution1);
			var execution2 = action.ExecutionHistory.SingleOrDefault(e => e.ExecutionBranchCode == "BR2");
			AssertNotNull(execution2);

			AssertEquals("BR1", execution1.ExecutionBranchCode);
			AssertEquals("DP1", execution1.ExecutionDepartmentCode);

			AssertEquals("BR2", execution2.ExecutionBranchCode);
			AssertEquals("DP2", execution2.ExecutionDepartmentCode);
		}

		public void TestSchedulerTask_ShouldNotSwitchContextExcessively()
		{
			ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks = 1;

			var contextSwitchCount = 0;
			EnvProxy.Instance.UserContextChanging += (s, e) => contextSwitchCount++;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "BR2";
			branch2.GB_RL_NKHomePort = "AUMEL";

			var dep1 = Factory.New<GlbDepartment>();
			dep1.GE_Code = "DP1";

			var dep2 = Factory.New<GlbDepartment>();
			dep2.GE_Code = "DP2";

			var shipment = Factory.New<IForwardingShipment>();

			Factory.Save();

			var provider = new ActionScheduleProvider();
			var sch1 = provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddMinutes(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference1|EST=N", branch1.PK, dep1.PK);
			var sch2_1 = provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddMinutes(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference2_1|EST=N", branch2.PK, dep2.PK);
			var sch2_2 = provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddMinutes(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference2_2|EST=N", branch2.PK, dep2.PK);
			var sch2_3 = provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddMinutes(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference2_3|EST=N", branch1.PK, dep2.PK); // BR1, but DP2

			var task = GetSchedulerTask();
			task.RunTask(CancellationToken.None);

			var newFactory = new BusinessObjectFactory();
			var loadedSch1 = newFactory.Load<TimeActionSchedule>(sch1.PK);
			var loadedSch2_1 = newFactory.Load<TimeActionSchedule>(sch2_1.PK);
			var loadedSch2_2 = newFactory.Load<TimeActionSchedule>(sch2_2.PK);
			var loadedSch2_3 = newFactory.Load<TimeActionSchedule>(sch2_3.PK);
			AssertEquals(StatusConstants.Closed, loadedSch1.TAS_ExecutionStatus);
			AssertEquals(StatusConstants.Closed, loadedSch2_1.TAS_ExecutionStatus);
			AssertEquals(StatusConstants.Closed, loadedSch2_2.TAS_ExecutionStatus);
			AssertEquals(StatusConstants.Closed, loadedSch2_3.TAS_ExecutionStatus);

			AssertEquals("Should switch context just 6 times for 3 different combinations of branches and departments", 6, contextSwitchCount);
		}

		public void TestSchedulerTask_ShouldThrow_WhenCannotFindBranch()
		{
			var missingBranchPK = ZGuid.NewZGuid();

			var dep1 = Factory.New<GlbDepartment>();
			dep1.GE_Code = "DP1";

			var shipment = Factory.New<IForwardingShipment>();

			Factory.Save();

			var provider = new ActionScheduleProvider();
			AssertExceptionThrown<ZSaveException>(() => provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddMinutes(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N", missingBranchPK, dep1.PK));

			var task = GetSchedulerTask();
			task.RunTask(CancellationToken.None);
		}

		public void TestSchedulerTask_ShouldThrow_WhenCannotFindDepartment()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var missingDepartmentPK = ZGuid.NewZGuid();

			var shipment = Factory.New<IForwardingShipment>();

			Factory.Save();

			var provider = new ActionScheduleProvider();
			AssertExceptionThrown<ZSaveException>(() => provider.ScheduleAction("DLY", ZDateTime.UtcNow.AddMinutes(-1), shipment.PK, "JS", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N", branch1.PK, missingDepartmentPK));

			var task = GetSchedulerTask();
			task.RunTask(CancellationToken.None);
		}

		public void TestBatchOrder()
		{
			ObjectFactory.Get<IBMSRegistry>().TaskActionSchedulerBatchSize = 10;

			TimeActionSchedule MakeSchedule(ZDateTime time)
			{
				var c = Factory.NewWithValidTestData<TimeActionSchedule>();
				c.TAS_ExecutionStatus = StatusConstants.Scheduled;
				c.TAS_RetryAttempts = 0;
				c.TAS_ExecutionDateTimeUtc = time;
				c.TAS_GB_Branch = Env.CurrentBranchPK;
				c.TAS_GE_Department = Env.CurrentDepartmentPK;
				return c;
			}

			var now = ZDateTime.UtcNow;
			var schedules = Enumerable.Range(0, 100).Select(i => MakeSchedule(now.AddMinutes(-i - 60))).ToList();
			Factory.Save();

			var action = new SchedulerTestAction();
			var actionFactory = new Mock<ISchedulerActionFactory>();
			actionFactory.Setup(p => p.GetSchedulerAction(It.IsAny<string>())).Returns(action);
			var batches = new List<IList<IActionSchedule>>();
			using (ObjectFactory.Substitute(actionFactory.Object))
			{
				var task = new SchedulerTask_ForTest(logger);
				task.OnAfterBatch = batches.Add;
				task.RunTask(CancellationToken.None);
			}

			AssertEquals("100 / 10 == 10 right?", 10, batches.Count);
			AssertArrayEqualsByElements("Check the order", schedules.Select(s => s.PK).Reverse().ToArray(), batches.SelectMany(b => b).Select(s => s.PK).ToArray());
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		ActionSchedulerTask GetSchedulerTask() => new SchedulerTask_ForTest(logger);

		protected override void SetUpCore()
		{
			base.SetUpCore();

			logger = new TestServiceLogger();
		}

		TestServiceLogger logger;
	}

	public class SchedulerTask_ForTest : ActionSchedulerTask
	{
		public SchedulerTask_ForTest(ILogger logger)
		{
			ServiceLogger = logger;
		}

		public Action<IList<IActionSchedule>> OnAfterBatch { get; set; }

		protected override Processor GetProcessor(IActionScheduleProvider scheduleProvider, ILogger logger)
		{
			return new Processor_ForTest(scheduleProvider, logger, OnAfterBatch);
		}

		class Processor_ForTest : Processor
		{
			public Processor_ForTest(IActionScheduleProvider provider, ILogger logger, Action<IList<IActionSchedule>> onAfterBatch)
				: base(provider, logger)
			{
				this.onAfterBatch = onAfterBatch;
			}
			readonly Action<IList<IActionSchedule>> onAfterBatch;

			protected override void OnAfterBatch(IList<IActionSchedule> batch)
			{
				onAfterBatch?.Invoke(batch);
				base.OnAfterBatch(batch);
			}
		}

		public override void RunTask(CancellationToken token)
		{
			using (EnvProxy.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				base.RunTask(token);
			}
		}
	}
}
