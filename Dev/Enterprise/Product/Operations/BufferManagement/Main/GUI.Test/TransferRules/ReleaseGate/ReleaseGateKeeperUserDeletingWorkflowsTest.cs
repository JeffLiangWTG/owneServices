using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test.TransferRules.ReleaseGate
{
	class ReleaseGateKeeperUserDeletingWorkflowsTest : BMSTestCaseWithFactory
	{
		[TestDate(2018, 10, 22)]
		public void TestProcess_WhenUserDeletingWorkflowAtStartOfList()
		{
			Assert_Process_WhenUserDeletingWorkflows(deletedWorkflows: new[] { Workflow0 });
		}

		[TestDate(2018, 10, 22)]
		public void TestProcess_WhenUserDeletingWorkflowAtMiddleOfList()
		{
			Assert_Process_WhenUserDeletingWorkflows(deletedWorkflows: new[] { Workflow1 });
		}

		[TestDate(2018, 10, 22)]
		public void TestProcess_WhenUserDeletingWorkflowAtEndOfList()
		{
			Assert_Process_WhenUserDeletingWorkflows(deletedWorkflows: new[] { Workflow2 });
		}

		[TestDate(2018, 10, 22)]
		public void TestProcess_WhenUserDeletingWorkflowsAtStartAndMiddleOfList()
		{
			Assert_Process_WhenUserDeletingWorkflows(deletedWorkflows: new[] { Workflow0, Workflow1 });
		}

		[TestDate(2018, 10, 22)]
		public void TestProcess_WhenUserDeletingWorkflowsAtMiddleAndEndOfList()
		{
			Assert_Process_WhenUserDeletingWorkflows(deletedWorkflows: new[] { Workflow1, Workflow2 });
		}

		[TestDate(2018, 10, 22)]
		public void TestProcess_WhenUserDeletingWorkflowsAtStartAndEndOfList()
		{
			Assert_Process_WhenUserDeletingWorkflows(deletedWorkflows: new[] { Workflow0, Workflow2 });
		}

		[TestDate(2018, 10, 22)]
		public void TestProcess_WhenUserDeletingWorkflowsAll()
		{
			Assert_Process_WhenUserDeletingWorkflows(deletedWorkflows: new[] { Workflow0, Workflow1, Workflow2 });
		}

		void Assert_Process_WhenUserDeletingWorkflows(ProcessHeader[] deletedWorkflows)
		{
			void SimulateUserDeletingWorkflow()
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				foreach (var deletedWorkflow in deletedWorkflows)
				{
					var loadedWorkflow = newFactory.Load<ProcessHeader>(deletedWorkflow.PK);
					if (loadedWorkflow != null)
					{
						loadedWorkflow.Delete();
					}
				}

				newFactory.Save();
			}

			AssertContainsExactElementsInAnyOrder("AllWorkflows should contains all workflows", AllWorkflows, new[] { Workflow0, Workflow1, Workflow2 });

			CombineAssertions("Precondition, all workflows should be in bucket", () =>
			{
				AssertEquals("Workflow0", Config.Bucket, Workflow0.CurrentComponent);
				AssertEquals("Workflow1", Config.Bucket, Workflow1.CurrentComponent);
				AssertEquals("Workflow2", Config.Bucket, Workflow2.CurrentComponent);
			});

			var logger = new BufferManagementLogger();
			ReleaseGateKeeperTest.RunReleaseGate(Config.System, logger, coordinator: new ReleaseGateTestCoordinator { PreWorkflowReloadAction = SimulateUserDeletingWorkflow });

			Workflow0.Reload();
			Workflow1.Reload();
			Workflow2.Reload();

			var processedWorkflows = AllWorkflows.Except(deletedWorkflows);
			CombineAssertions("Non-deleted or processed workflows should be in buffer", () =>
			{
				processedWorkflows.ForEach(w => AssertEquals(message: w.FH_CompletionStatement, Config.Buffer, w.CurrentComponent));
			});

			AssertNullOrEmpty("Should not have error reported", ErrorReporter.LastMessageReported);

			var releasedMessage = string.Join(System.Environment.NewLine, processedWorkflows.Select(processedWorkflow => $"WTGDEV - ORG.buffer: Workflow {processedWorkflow.FH_CompletionStatement} for job XVBQP68SIYXQ has been released."));
			if (processedWorkflows.Any())
			{
				releasedMessage = releasedMessage + System.Environment.NewLine;
			}

			AssertContainsExactLinesInAnyOrder("Should show warning 'workflow PK is no longer in database'",
$@"WTGDEV - ORG.buffer: Determining workflows eligible for Release Gate.
WTGDEV - ORG.buffer: Calculating Capacity for 1 staff.
Using Capacity Query...
Staff Capabilities query executed in 00:00:00
Task Estimate query executed in 00:00:00
Finished loading workflows in 00:00:00
WTGDEV - ORG.buffer: Assembling reservation tracker.
WTGDEV - ORG.buffer: Sorting workflows into release sequence.
WTGDEV - ORG.buffer: Starting Release Gate run with 3 workflows to process.
{string.Join(System.Environment.NewLine, deletedWorkflows.Select(deletedWorkflow => $"Warning - Workflow PK '{deletedWorkflow.PK}' is no longer in database."))}
{releasedMessage}WTGDEV - ORG.buffer: Persisting updated capacity after Release Gate run.", logger.ToString().Trim());
		}

		#region Implementation

		AcceptabilityBandTestConfig Config;
		ProcessHeader Workflow0;
		ProcessHeader Workflow1;
		ProcessHeader Workflow2;
		readonly List<ProcessHeader> AllWorkflows = new List<ProcessHeader>();

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);

			var resource = CreateStaffInCurrentBranchDept("RES", "Resource N");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			Workflow0 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow-0", releaseDateTime: ZDateTime.UtcNow.AddMinutes(-10));
			CreateTask(Workflow0, resource.GS_Code, 60);
			AllWorkflows.Add(Workflow0);

			Workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow-1", releaseDateTime: ZDateTime.UtcNow.AddMinutes(-9));
			CreateTask(Workflow1, resource.GS_Code, 60);
			AllWorkflows.Add(Workflow1);

			Workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow-2", releaseDateTime: ZDateTime.UtcNow.AddMinutes(-8));
			CreateTask(Workflow2, resource.GS_Code, 60);
			AllWorkflows.Add(Workflow2);

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();
		}

		#endregion
	}
}
