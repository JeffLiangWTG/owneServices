using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ProcessJobHeaderNonTransactionedTest : NonTransactionedTestCase
	{
		#region Apply Template

		public void TestApplyTemplate_ShouldNotDuplicateDefaultWorkflow_WhenApplyingTemplateWithoutWorkflow()
		{
			Factory.RefreshEnabled = false;

			var workflowType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			BMSTestHelper.CreateSystem(Factory, workflowType);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, workflowType, name: "Template", subType1: "AAA");
			var task = template.WorkflowItems.Tasks.AddNew();

			task.P9_EstDuration = new ZInt(1).GetDateTimeFromMinutes();
			task.P9_EstimateVariationFactor = 1;

			Factory.Save();

			var dummyJob = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummyJob.SubType1 = template.P0_SubType1;
			dummyJob.Z0_Description = nameof(DummyWithWorkflow);

			var jobHeader = BMSTestHelper.CreateJobHeader(dummyJob, addDefaultProcessHeaderIfNone: false);

			Factory.Save();

			var templatePK = template.PK;
			var jobHeaderPK = jobHeader.PK;
			var mrse = new ManualResetEvent(false);

			var thread1 = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factoryInThread1 = new BusinessObjectFactory() { RefreshEnabled = false };
					var jobHeaderInThread1 = factoryInThread1.Load<ProcessJobHeader>(jobHeaderPK);

					var templateInThread1 = factoryInThread1.Load<ProcessTaskTemplate>(templatePK);

					jobHeaderInThread1.ApplyTemplate(templateInThread1);

					factoryInThread1.Save();
					mrse.Set();
				}
			});

			var thread2 = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factoryInThread2 = new BusinessObjectFactory() { RefreshEnabled = false };
					var jobHeaderInThread2 = factoryInThread2.Load<ProcessJobHeader>(jobHeaderPK);

					jobHeaderInThread2.OnBeforeAddWorkflowsFromTemplate += (_, __) => mrse.WaitOne();

					var templateInThread2 = factoryInThread2.Load<ProcessTaskTemplate>(templatePK);

					jobHeaderInThread2.ApplyTemplate(templateInThread2);
					factoryInThread2.Save();
				}
			});

			thread1.Start();
			thread2.Start();

			thread1.Join();
			thread2.Join();

			var reloadQuery = new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobHeaderPK) { ReLoadExistingRows = true };
			var workflows = Factory.Load<ProcessHeader>(reloadQuery);

			AssertNullOrEmpty("No errors should be reported", ErrorReporter.LastMessageReported);
			AssertEquals("Only 1 workflow should be created", 1, workflows.Length);
			AssertEquals("Should be the default Workflow", true, workflows.Single().IsDefaultWorkflow);
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
