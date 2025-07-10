using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(StaggeredReleaseDelayCalculatorTask))]
	[TestDate(2014, 5, 26, 9, 0, 0)]
	class StaggeredReleaseDelayCalculatorTaskTest : BMServiceTaskWithFilteredReaderTestCase<StaggeredReleaseDelayCalculatorTask>
	{
		#region Hosted Service Requirements

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, StaggeredReleaseDelayCalculatorTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, StaggeredReleaseDelayCalculatorTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", StaggeredReleaseDelayCalculatorTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", StaggeredReleaseDelayCalculatorTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		#region WorkflowManagementMode Restrictions

		public void TestProcess_WhenPlanningManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Staggered Release Delay Calculator.", log.ToString());
		}

		public void TestProcess_WhenBufferManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Staggered Release Delay Calculator.", log.ToString());
		}

		public void TestProcess_WhenEnhancedWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Staggered Release Delay Calculator.", log.ToString());
		}

		public void TestProcess_WhenBasicWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Staggered Release Delay Calculator.", log.ToString());
		}
		#endregion

		public void TestRun_NoDelaysSet_ShouldBeNull()
		{
			CreateTestData(Tuple.Create(0, 0.0m));
			RunTask();

			AssertEquals(ZDateTime.Empty, workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_DelayFactor1_ShouldBeTPlus1Hour()
		{
			CreateTestData(Tuple.Create(0, 1.0m));
			RunTask();

			AssertEquals(ZDateTime.UtcNow + TimeSpan.FromHours(1), workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_EnsureConcurrencyErrorIsLogged()
		{
			CreateTestData(Tuple.Create(0, 1.0m));
			Factory.Save();

			var serviceTask = GetNewServiceTask();
			serviceTask.FactoryProvider.Current.Saving += delegate
			{
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

				loadedWorkflow.FH_Status = "OPN";
				loadedWorkflow.FH_SystemLastEditTimeUtc = ZDateTime.UtcNow + TimeSpan.FromMinutes(-99);
				loadedWorkflow.FH_StaggeredReleaseDelayExpiry = ZDateTime.UtcNow + TimeSpan.FromMinutes(11);

				newFactory.Save();

				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("I'm really sorry, this was the best I could come up with to induce a concurrency error :("), ((IBusinessObjectInternals)loadedWorkflow).Row, ((IDbConnected)newFactory).Connection), newFactory);
			};

			var log = InitialiseAndRunTaskSchedule(serviceTask);

			AssertMultilineASCIIEquals("The service task should log the concurrency error as a warning in the service task log, and NOT log the success of the assignment, since it failed when committing to the db.",
$@"Warning|Concurrency error: 
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: ProcessHeader
PK: {workflow.PK}
RowState: Unchanged
Factory validation suspended: False
Factory name for debugging: 
Business object around row = Enterprise.BufferManagement.Business.ProcessHeader
Business object validation suspended: 0
Business object is marking as needing validation suspended: False
Business object light validation is enabled: False
Business object additional info: 

Inner Message = I'm really sorry, this was the best I could come up with to induce a concurrency error :(

Additional Information = 
--- Save Aborted Due to Concurrency Check ---

ROW INFORMATION
Table      = ProcessHeader
PK         = {workflow.PK}
RowState   = Unchanged

COLUMN INFORMATION
<ConcurrencyTableRows />", log.ToString());
		}

		public void TestRun_Delay10Mins_ShouldBeTPlus10Mins()
		{
			CreateTestData(Tuple.Create(10, 0.0m));
			RunTask();

			AssertEquals(ZDateTime.UtcNow + TimeSpan.FromMinutes(10), workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_Delay30MinsAndFactor1_ShouldBeTPlus1Hour()
		{
			CreateTestData(Tuple.Create(30, 1.0m));
			RunTask();

			AssertEquals(ZDateTime.UtcNow + TimeSpan.FromHours(1), workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_Delay90MinsAndFactor1_ShouldBeTPlus90Mins()
		{
			CreateTestData(Tuple.Create(90, 1.0m));
			RunTask();

			AssertEquals(ZDateTime.UtcNow + TimeSpan.FromMinutes(90), workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_NoDelaysSet_PrereqsClosed_ShouldBeEmpty()
		{
			CreateTestData(Tuple.Create(0, 0.0m));
			task2.P9_Status = task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			RunTask();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertEquals(ZDateTime.Empty, workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_DelayFactor2_ClosedImmediatePrereqOnly_ShouldBeTPlus2Hours()
		{
			CreateTestData(Tuple.Create(0, 2.0m));
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			RunTask();

			AssertEquals(ZDateTime.UtcNow + TimeSpan.FromHours(2), workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_DelayFactor1OnLink_ShouldBeTPlus1Hour()
		{
			CreateTestData(Tuple.Create(0, 0.0m), prereq1LinkDelays: Tuple.Create(0, 1.0m));
			RunTask();

			AssertEquals(ZDateTime.UtcNow + TimeSpan.FromHours(1), workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_Delay10MinsOnLink_ShouldBeTPlus10Mins()
		{
			CreateTestData(Tuple.Create(0, 0.0m), prereq1LinkDelays: Tuple.Create(10, 0.0m));
			RunTask();

			AssertEquals(ZDateTime.UtcNow + TimeSpan.FromMinutes(10), workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_DelayFactor1OnIrrelevantLink_ShouldBeEmpty()
		{
			CreateTestData(Tuple.Create(0, 0.0m), prereq2LinkDelays: Tuple.Create(0, 1.0m));
			RunTask();

			AssertEquals(ZDateTime.Empty, workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_Delay10MinsOnIrrelevantLink_ShouldBeEmpty()
		{
			CreateTestData(Tuple.Create(0, 0.0m), prereq2LinkDelays: Tuple.Create(10, 0.0m));
			RunTask();

			AssertEquals(ZDateTime.Empty, workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_ThenMoveAllPrereqsOutOfBufferAndRunAgain_ShouldClearDelayExpiry()
		{
			CreateTestData(Tuple.Create(10, 0m));
			RunTask();

			AssertNotEquals(ZDateTime.Empty, workflow.FH_StaggeredReleaseDelayExpiry);

			prereq1.FH_FC_CurrentComponent = config.Bucket.PK;
			prereq2.FH_FC_CurrentComponent = config.Bucket.PK;
			RunTask();

			AssertEquals(ZDateTime.Empty, workflow.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestRun_ImmediatePrereqIsClosedAndOutOfBuffer_ShouldLeaveDelayExpiry()
		{
			CreateTestData(Tuple.Create(10, 0m));
			RunTask();

			AssertNotEquals(ZDateTime.Empty, workflow.FH_StaggeredReleaseDelayExpiry);

			prereq1.FH_FC_CurrentComponent = config.Bucket.PK;
			prereq1.GetTasksWithoutAccessingWorkflowParent().First().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			RunTask();

			AssertNotEquals(ZDateTime.Empty, workflow.FH_StaggeredReleaseDelayExpiry);
		}

		[TestDate(2019, 1, 1)]
		public void TestRun_ShouldLogBetterExceptionMessages_AndMoveOnToFollowingWorkflows()
		{
			CreateTestData(Tuple.Create(10, 0m));

			var otherJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			otherJobHeader.FH_TimeDelayMinutes = 1;
			var otherWorkflow1 = BMSTestHelper.CreateWorkflow(otherJobHeader, "otherWorkflow1", config.Buffer);
			var otherWorkflow2 = BMSTestHelper.CreateWorkflow(otherJobHeader, "otherWorkflow2", config.Bucket);

			otherWorkflow1.GetOrCreateDependencyLink(otherWorkflow2);

			BMSTestHelper.CreateTask(otherWorkflow1);
			BMSTestHelper.CreateTask(otherWorkflow2);

			((OrgHeader)prereqJobHeader.Parent).OH_Code = "ORG1";
			((OrgHeader)jobHeader.Parent).OH_Code = "ORG2";
			((OrgHeader)otherWorkflow1.Parent).OH_Code = "ORG3";
			((OrgHeader)prereqJobHeader.Parent).OH_FullName = "Tools Up";
			((OrgHeader)jobHeader.Parent).OH_FullName = "Tools Down";
			((OrgHeader)otherWorkflow1.Parent).OH_FullName = "Tools Side-to-Side";

			Factory.Save();

			BMSTestHelper.CreateCircularDependency(workflow, prereq1, TestConnection);
			BMSTestHelper.CreateCircularDependency(prereq1, prereq2, TestConnection);
			BMSTestHelper.CreateCircularDependency(prereq2, workflow, TestConnection);

			var newFactory = Factory.CreateNewFactory();
			workflow = newFactory.Load<ProcessHeader>(workflow.PK);
			otherWorkflow1 = newFactory.Load<ProcessHeader>(otherWorkflow1.PK);
			otherWorkflow2 = newFactory.Load<ProcessHeader>(otherWorkflow2.PK);

			AssertEquals(ZDateTime.Empty, otherWorkflow2.FH_StaggeredReleaseDelayExpiry);

			RunTask(removeStacktraceLinesFromLog: true);

			otherWorkflow2.Reload();
			AssertNotEquals(ZDateTime.Empty, otherWorkflow2.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestStaggeredReleaseDelayImplemented()
		{
			//Run a check on the query to ensure that the offending OR has been removed.
			using (TestConnection.TrackExecutedCommands())
			{
				var calculator = GetNewServiceTask();
				calculator.RunTask();

				var relevantCommand = TestConnection.ExecutedCommands.First(x => x.Contains("FH_StaggeredReleaseDelayExpiry")).ToLower();

				Assert(!Regex.Match(relevantCommand, "fh_staggeredreleasedelayexpiry is not null\\s*or").Success);
			}
		}

		public void TestRun_WhenCapacityCalculationsDisabled_ShouldDoNothing()
		{
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CreateTestData(Tuple.Create(0, 0.0m), prereq1LinkDelays: Tuple.Create(10, 0.0m));
			RunTask(expectedLog: "Debug|Service task not run because the [Disable Capacity Calculations] registry item is enabled.");

			AssertEquals("The delay would normally be updated but since capacity is disabled, so is release, so there's no point running this service task.", ZDateTime.Empty, workflow.FH_StaggeredReleaseDelayExpiry);
		}

		#region Hosted Service Requirements

		public void TestServiceTask_WhenCapacityCalculationDisabled_ShouldNotMeetHostedServiceRequirement()
		{
			AssertEquals("Should be enabled by default if buffer management is enabled.", string.Empty, StaggeredReleaseDelayCalculatorTask.CheckCapacityCalculationsNotDisabled());

			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Release Gate -> Disable Capacity Calculations' requires a value other than 'True'.", StaggeredReleaseDelayCalculatorTask.CheckCapacityCalculationsNotDisabled());
		}

		#endregion

		#region Implementation

		void CreateTestData(Tuple<int, decimal> jobHeaderDelays, Tuple<int, decimal> prereq1LinkDelays = null, Tuple<int, decimal> prereq2LinkDelays = null)
		{
			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			workflow = BMSTestHelper.CreateWorkflow(jobHeader, "first workflow");

			prereqJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			prereq1 = BMSTestHelper.CreateWorkflow(prereqJobHeader, "prereq1");
			prereq2 = BMSTestHelper.CreateWorkflow(prereqJobHeader, "prereq2");

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			prereq1.FH_FC_CurrentComponent = prereq2.FH_FC_CurrentComponent = config.Buffer.PK;

			task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			task2 = BMSTestHelper.CreateTask(prereq1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			task3 = BMSTestHelper.CreateTask(prereq2, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);

			link1 = prereq1.GetOrCreateDependencyLink(workflow);
			link2 = prereq2.GetOrCreateDependencyLink(prereq1);

			jobHeader.FH_TimeDelayMinutes = jobHeaderDelays.Item1;
			jobHeader.FH_TimeDelayFactor = jobHeaderDelays.Item2;

			if (prereq1LinkDelays != null)
			{
				link1.FP_TimeDelayMinutes = prereq1LinkDelays.Item1;
				link1.FP_TimeDelayFactor = prereq1LinkDelays.Item2;
			}

			if (prereq2LinkDelays != null)
			{
				link2.FP_TimeDelayMinutes = prereq2LinkDelays.Item1;
				link2.FP_TimeDelayFactor = prereq2LinkDelays.Item2;
			}
		}

		void RunTask(string expectedLog = null, bool removeStacktraceLinesFromLog = false)
		{
			Factory.Save();
			var log = InitialiseAndRunTaskSchedule(GetNewServiceTask());

			if (expectedLog != null)
			{
				var actualLog = log.ToString();

				if (removeStacktraceLinesFromLog)
				{
					actualLog = string.Join(System.Environment.NewLine, actualLog.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Where(l => !l.StartsWith("   at ")));
				}

				AssertMultilineASCIIEquals("Service task log", expectedLog, actualLog);
			}

			workflow.Reload();
		}

		ProcessJobHeader jobHeader, prereqJobHeader;
		ProcessHeader workflow, prereq1, prereq2;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "WI00637617 - task1 is flagged by IDE00052, but this is needed to have a valid BMSystem, see http://crikey.wtg.zone/TestResults/ee1d3757-f61e-40ab-9941-fa3c4b90abcc")]
		ProcessTask task1, task2, task3;
		ProcessHeaderLink link1, link2;

		SchematicTestConfig config;

		protected override void SetUpCore()
		{
			base.SetUpCore();

			BMSTestHelper.EnableBMSInRegistry();
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override StaggeredReleaseDelayCalculatorTask GetNewServiceTask()
		{
			return new StaggeredReleaseDelayCalculatorTask_ForTest { ServiceLogger = new TestServiceLogger() };
		}

		protected override void InitializeProcessHeaders()
		{
			CreateTestData(Tuple.Create(0, 0.0m));
			var otherJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			otherJobHeader.FH_TimeDelayMinutes = 1;
			var otherWorkflow1 = BMSTestHelper.CreateWorkflow(otherJobHeader, "otherWorkflow1", config.Buffer);
			var otherWorkflow2 = BMSTestHelper.CreateWorkflow(otherJobHeader, "otherWorkflow2", config.Bucket);

			otherWorkflow1.GetOrCreateDependencyLink(otherWorkflow2);

			BMSTestHelper.CreateTask(otherWorkflow1);
			BMSTestHelper.CreateTask(otherWorkflow2);

			Factory.Save();
		}

		class StaggeredReleaseDelayCalculatorTask_ForTest : StaggeredReleaseDelayCalculatorTask
		{
			protected override void RunTaskCore(CancellationToken token)
			{
				using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
				{
					base.RunTaskCore(token);
				}
			}
		}
	}
}
