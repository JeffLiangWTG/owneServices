using System;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestDate]
	[TestDateIncremental]
	public class SchematicTransferLoopMonitorTest : BMSTestCaseWithFactory
	{
		public void TestSchematicTransferLoopMonitor_ShouldTakeIntoAccountScanDepth()
		{
			using (var context = new BMSMultipleRunsTestingContext(Factory))
			{
				var logs = context.GetXFRRecords();
				var times = logs.Where(x => x.SL_Parent == context.Workflow.PK).OrderBy(x => x.SL_PostedTimeUtc).Select(x => x.SL_PostedTimeUtc);
				ZDateTime firstLogTime = times.First();
				ZDateTime interimLogTime = times.Skip(3).First(); //number of buckets - skipping the first loop
				ZDateTime endTime = ZDateTime.UtcNow;

				//CASE A: Standard @LoopThreshold, detection depth covers all logs
				var result = SchematicTransferLoopMonitor.GetLoopedWorkflows(firstLogTime, endTime, BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value);
				AssertEquals("Case A - one looped workflows (excluding the deleted and deactivated ones) should be found", 1, result.Count());

				//CASE B: Standard @LoopThreshold, detection depth is reduced
				result = SchematicTransferLoopMonitor.GetLoopedWorkflows(interimLogTime, endTime, BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value);
				AssertEquals("Case B - should be no looped workflows detected because the time depth does not cover all the logs", 0, result.Count());

				//CASE C: Increased @LoopThreshold, detection depth covers all logs
				result = SchematicTransferLoopMonitor.GetLoopedWorkflows(firstLogTime, endTime, BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value + 1);
				AssertEquals("Case C - should be no looped workflows detected because repetitions are not enough to make the decision that the workflows are looped", 0, result.Count());
			}
		}

		#region WorkflowManagementMode Restrictions

		public void TestProcess_WhenPlanningManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var logger = new TestServiceLogger();
			var processor = new SchematicTransferLoopMonitor(logger);
			processor.Process(CancellationToken.None);

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		public void TestProcess_WhenBufferManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var logger = new TestServiceLogger();
			var processor = new SchematicTransferLoopMonitor(logger);
			processor.Process(CancellationToken.None);

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		public void TestProcess_WhenEnhancedWorkflowManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);

			var logger = new TestServiceLogger();
			var processor = new SchematicTransferLoopMonitor(logger);
			processor.Process(CancellationToken.None);

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		public void TestProcess_WhenBasicWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			var logger = new TestServiceLogger();
			var processor = new SchematicTransferLoopMonitor(logger);
			processor.Process(CancellationToken.None);

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}
	}
}
