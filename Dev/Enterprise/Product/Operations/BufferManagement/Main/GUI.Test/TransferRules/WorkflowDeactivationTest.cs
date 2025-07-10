using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestDate(2015, 7, 14)]
	class WorkflowDeactivationTest : BMSTestCaseWithFactory
	{
		public void TestWorkflowCompletesCircuit_WhenCausedByMultipleStateChanges_ShouldNotDeactivate()
		{
			RunTransferRules(numberOfTimesToInterfereWithWorkflowStateAfterBatchProcessed: 99);

			AssertEquals("Workflow should complete a loop", startingAndFinishingComponent, workflow.CurrentComponent);
			AssertEquals("workflow should still be active because it was a state change that triggered the loop, not faulty transfer rules", true, workflow.FH_IsActive);
			AssertNotContains(GetDeactivationLogMessageFragment(), logger.ToString());
		}

		public void TestWorkflowCompletesCircuit_WhenCausedByStateChangeMidRun_ShouldNotDeactivate()
		{
			RunTransferRules(numberOfTimesToInterfereWithWorkflowStateAfterBatchProcessed: 1);

			AssertEquals("Workflow should complete a loop", startingAndFinishingComponent, workflow.CurrentComponent);
			AssertEquals("workflow should still be active because it was a state change that triggered the loop, not faulty transfer rules", true, workflow.FH_IsActive);
			AssertNotContains(GetDeactivationLogMessageFragment(), logger.ToString());
		}

		public void TestWorkflowCompletesCircuit_WhenCausedByInvalidRules_ShouldDeactivate()
		{
			RunTransferRules(numberOfTimesToInterfereWithWorkflowStateAfterBatchProcessed: 0);

			AssertEquals("Workflow should complete a loop", startingAndFinishingComponent, workflow.CurrentComponent);
			AssertEquals("workflow should have been deactivated because it completed a loop without being modified externally in the process", false, workflow.FH_IsActive);
			AssertContains(GetDeactivationLogMessageFragment(), logger.ToString());
		}

		public void TestWorkflowCompletesCircuit_WhenTransferFailedMidRun_ShouldNotDeactivate()
		{
			RunTransferRulesWithFailure(true);

			AssertEquals("Workflow should fail to transfer", bucket3, workflow.CurrentComponent);
			AssertEquals("workflow should still be active because it has not moved anywhere", true, workflow.FH_IsActive);
			AssertNotContains(GetDeactivationLogMessageFragment(), logger.ToString());
		}

		public void TestWorkflowCompletesCircuit_WhenLoopExists_ShouldDeactivate()
		{
			RunTransferRulesWithFailure(false);

			AssertEquals("Workflow should complete a loop", startingAndFinishingComponent, workflow.CurrentComponent);
			AssertEquals("workflow should have been deactivated because it completed a loop without being modified externally in the process", false, workflow.FH_IsActive);
			AssertContains(GetDeactivationLogMessageFragment(), logger.ToString());
		}

		string GetDeactivationLogMessageFragment() => $"Workflow Let's just say it moved me. TO A BIGGER HOUSE!! (PK = {workflow.PK}, Job = Inquiry (I00001000)) has been deactivated since it has completed a loop of the system";

		BMComponent startingAndFinishingComponent;
		ProcessHeader workflow;
		BMComponent bucket1;
		BMComponent bucket2;
		BMComponent bucket3;
		BMComponent bucket4;
		ILogger logger;

		protected override void SetUp()
		{
			base.SetUp();

			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			bucket1 = CreateBucket(system, "bucket1");
			bucket2 = CreateBucket(system, "bucket2");
			bucket3 = CreateBucket(system, "bucket3");
			bucket4 = CreateBucket(system, "bucket4");

			// 1 -> 2 -> 3 -> 4
			//      2 <- 3

			var link1_2 = LinkComponents(bucket1, bucket2);
			var link2_3 = LinkComponents(bucket2, bucket3);
			var link3_4 = LinkComponents(bucket3, bucket4, sequence: 2);
			var link3_2 = LinkComponents(bucket3, bucket2, sequence: 1); // Runs backwards link first

			FilterStripsTestHelper.AddCustomSQLFilterStrip(link1_2.FilterRule, "1=1");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(link2_3.FilterRule, "1=1");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(link3_4.FilterRule, "1=1");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(link3_2.FilterRule, "1=1"); // Workflow is going to move back along this link to its starting component. The key difference is whether its state changed midway through the run. If so, maybe the state change is responsible rather than the component link config.

			startingAndFinishingComponent = bucket2; // Workflows will end up back where they started due to the backwards link.

			workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Let's just say it moved me. TO A BIGGER HOUSE!!", bucket2);

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		void RunTransferRules(int numberOfTimesToInterfereWithWorkflowStateAfterBatchProcessed)
		{
			logger = new SimpleLogger();
			var runner = new DummyTransferRuleRunner(startingAndFinishingComponent.System, logger);

			runner.BeforeOnBatchProcessedAction = () => TestDateAttribute.AddMinutes(1);

			runner.AfterOnBatchProcessedAction = () =>
			{
				if (numberOfTimesToInterfereWithWorkflowStateAfterBatchProcessed > 0)
				{
					numberOfTimesToInterfereWithWorkflowStateAfterBatchProcessed--;
					TestDateAttribute.AddMinutes(1);

					var newFactory = workflow.Factory.CreateNewFactory();
					newFactory.RefreshEnabled = false; // Simulate this happening in another user's session.

					var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

					loadedWorkflow.FH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
					newFactory.Save();
				}
			};

			runner.Process_ForTest();
		}

		void RunTransferRulesWithFailure(bool transferHasFailed)
		{
			logger = new SimpleLogger();
			var runner = new DummyTransferRuleRunner(startingAndFinishingComponent.System, logger);

			runner.BeforeOnBatchProcessedAction = () => TestDateAttribute.AddMinutes(1);

			if (transferHasFailed)
			{
				runner.AfterOnBatchProcessedAction = () =>
				{
					var newFactory = workflow.Factory.CreateNewFactory();
					newFactory.RefreshEnabled = false;

					var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

					loadedWorkflow.FH_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
					loadedWorkflow.FH_FC_CurrentComponent = bucket2.PK;
					newFactory.Save();
				};
			}

			runner.Process_ForTest();
		}
	}
}
