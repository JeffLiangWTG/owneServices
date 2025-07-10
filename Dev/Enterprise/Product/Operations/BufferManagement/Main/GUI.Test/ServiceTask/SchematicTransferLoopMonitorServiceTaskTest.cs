using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(SchematicTransferLoopMonitorServiceTask))]
	[TestDate]
	[TestDateIncremental]
	class SchematicTransferLoopMonitorServiceTaskTest : BMServiceTaskWithFilteredReaderTestCase<SchematicTransferLoopMonitorServiceTask>
	{
		#region Hosted Service Requirements

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, SchematicTransferLoopMonitorServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, SchematicTransferLoopMonitorServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertEquals("The service task should meet the requirements because EWF is enabled.", string.Empty, SchematicTransferLoopMonitorServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'EWF', 'BUF', 'PLN'.", SchematicTransferLoopMonitorServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		public void TestSchematicTransferLoopMonitor_ShouldReportIfNoLoopsFound()
		{
			var logs = RunServiceTaskAndGetLogs();
			AssertReportsNoLoopsFound(logs);
		}

		public void TestSchematicTransferLoopMonitor_ShouldDeactivateLoopedWorkflowsAndLog_WhenLoopedOverMultipleBMSRuns()
		{
			using (var context = new BMSMultipleRunsTestingContext(Factory))
			{
				var logs = RunServiceTaskAndGetLogs();

				AssertEquals("Workflow should be deactivated", false, context.Workflow.FH_IsActive);
				AssertReportsOneLoopFound(logs);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				logs = RunServiceTaskAndGetLogs();
				AssertReportsNoLoopsFound(logs);
			}
		}

		public void TestSchematicTransferLoopMonitor_ShouldNotDeactivateWorkflows_WhenLoopedOverMultipleBMSRuns_IfUserMadeChangesToWorkflows()
		{
			using (var context = new BMSMultipleRunsWithUserChangesToWorkflowsTestingContext(Factory))
			{
				var logs = RunServiceTaskAndGetLogs();
				AssertReportsNoLoopsFound(logs);
			}
		}

		public void TestSchematicTransferLoopMonitor_ShouldNotDeactivateWorkflows_WhenLoopedOverMultipleBMSRuns_IfUserMadeChangesToJob()
		{
			using (var context = new BMSMultipleRunsWithUserChangesToJobTestingContext(Factory))
			{
				var logs = RunServiceTaskAndGetLogs();
				AssertReportsNoLoopsFound(logs);
			}
		}

		public void TestSchematicTransferLoopMonitor_ShouldDeactivateLoopedWorkflowsAndLog_WhenLoopedBetweenBMSAndBMG()
		{
			using (var context = new LoopingBetweenBMSAndBMGTestingContext(Factory))
			{
				var logs = RunServiceTaskAndGetLogs();

				AssertEquals("Workflow should be deactivated", false, context.Workflow.FH_IsActive);
				AssertReportsOneLoopFound(logs);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				logs = RunServiceTaskAndGetLogs();
				AssertReportsNoLoopsFound(logs);
			}
		}

		public void TestSchematicTransferLoopMonitor_ShouldNotDeactivateWorkflows_WhenLoopedBetweenBMSAndBMG_IfUserMadeChangesToWorkflows()
		{
			using (var context = new LoopingBetweenBMSAndBMGWithUserChangesToWorkflowsTestingContext(Factory))
			{
				var logs = RunServiceTaskAndGetLogs();
				AssertReportsNoLoopsFound(logs);
			}
		}

		public void TestSchematicTransferLoopMonitor_ShouldNotDeactivateWorkflows_WhenLoopedBetweenBMSAndBMG_IfUserMadeChangesToJob()
		{
			using (var context = new LoopingBetweenBMSAndBMGWithUserChangesToJobTestingContext(Factory))
			{
				var logs = RunServiceTaskAndGetLogs();
				AssertReportsNoLoopsFound(logs);
			}
		}

		public void TestSchematicTransferLoopMonitor_ShouldNotDeactivateWorkflowsTwice_IfIssueAddressed()
		{
			using (var context = new BMSMultipleRunsTestingContext(Factory))
			{
				var logs = RunServiceTaskAndGetLogs();

				AssertEquals("Workflow should be deactivated", false, context.Workflow.FH_IsActive);
				AssertReportsOneLoopFound(logs);

				//let's pretend that we addressed the issue and reactivated the workflow
				context.Workflow.FH_IsActive = true;
				var log = Factory.New<StmALog>();
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
					log.SL_SE_NKEvent = Events.EditedARecordCode; //EDT
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
					log.SL_Table = "OrgHeader";
					log.SL_Parent = context.OrgHeader.PK;
				}
				Factory.Save();

				Env.OutgoingMailManager.EmailsCreated.Clear();
				logs = RunServiceTaskAndGetLogs();
				AssertReportsNoLoopsFound(logs);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Implementation

		string RunServiceTaskAndGetLogs()
		{
			var serviceTask = GetNewServiceTask();
			var loggerForMonitor = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();
			return loggerForMonitor.ToString();
		}

		void AssertReportsOneLoopFound(string actualLog)
		{
			AssertContains("Information|1 transfer loop(s) found:", actualLog);
			AssertContains("Warning|The following workflow has been deactivated since it has completed a loop. Please address the issue and reactivate the workflow:", actualLog);
			AssertContains("Code = Number one, Description = Organization (XVBQP68SIYXQ) - Number one, Job = Organization (XVBQP68SIYXQ)", actualLog);

			AssertEquals("Notification email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sendEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Workflow transfer loop detected", sendEmail.Subject);

			var emailLines = sendEmail.Body.Split('\n').Select(l => l.Trim());
			AssertContainsExactElementsInAnyOrder(new[] {
				"1 transfer loop(s) found:",
				"The following workflow has been deactivated since it has completed a loop. Please address the issue and reactivate the workflow:",
				"Code = Number one, Description = Organization (XVBQP68SIYXQ) - Number one, Job = Organization (XVBQP68SIYXQ)" },
				emailLines);

			AssertEquals(1, sendEmail.Recipients.Count);
			AssertEquals("king.arthur@avalon.com", sendEmail.Recipients[0].Email);
		}

		void AssertReportsNoLoopsFound(string actualLog)
		{
			string expected = @"Debug|No transfer loops found";
			AssertLog(expected, actualLog);
			AssertEquals("No email should be created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		void AssertLog(string expectedLog, string actualLog)
		{
			Assert(string.Format(CultureInfo.InvariantCulture, @"Service task log. Expected log:

{0}

But actual log was:

{1}", expectedLog, actualLog),
							string.Compare(expectedLog, actualLog, CultureInfo.InvariantCulture, CompareOptions.IgnoreSymbols) == 0);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		protected override SchematicTransferLoopMonitorServiceTask GetNewServiceTask()
		{
			return new SchematicTransferLoopMonitorServiceTask_ForTest();
		}

		class SchematicTransferLoopMonitorServiceTask_ForTest : SchematicTransferLoopMonitorServiceTask
		{
			protected override void RunTaskCore(CancellationToken token)
			{
				using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
				{
					base.RunTaskCore(token);
				}
			}
		}

		#endregion
	}
}
