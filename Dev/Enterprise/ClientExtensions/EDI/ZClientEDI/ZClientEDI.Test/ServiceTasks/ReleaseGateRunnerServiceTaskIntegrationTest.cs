using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ZClientEDI.Business.Test;

namespace Enterprise.Client.EDI.Test
{
	class ReleaseGateRunnerServiceTaskIntegrationTest : TestCaseWithFactory
	{
		public void TestProcess_IncidentNumberShouldBeIncludedInLogs()
		{
			EDIClientDbSchemaUpgradeForTest.UpgradeViewClientProcessHeader(TestConnection);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INC", "Woo Hoo");
			config.Buffer.FC_DisplaySequence = 1;

			var job = BMSTestHelper.CreateJobHeader<SupportIncident>(Factory, addDefaultProcessHeaderIfNone: false);
			var incident = (SupportIncident)job.Parent;

			var workflow = BMSTestHelper.CreateWorkflow(job, "Fun Workflow");

			BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			AssertEquals("The workflow should be in the bucket.", config.Bucket, workflow.CurrentComponent);

			Factory.Save();

			var task = new ReleaseGateRunnerServiceTask();
			var logger = new BufferManagementLogger();

			task.ServiceLogger = logger;
			task.RunTask();

			workflow.Reload();

			AssertEquals("The workflow should now be in the buffer.", config.Buffer.PK, workflow.CurrentComponent.PK);

			var log = logger.ToString();

			AssertContains("Should contain the Job number for an incident's workflow that has been released.", String.Format("Woo Hoo - INC.buffer: Workflow Fun Workflow for job {0} has been released.", incident.JobNumber), log);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			disposables = new DisposableList(1) { BMSTestCaseWithFactory.DisableAsyncBehaviour() };

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		DisposableList disposables;

		#endregion
	}
}
