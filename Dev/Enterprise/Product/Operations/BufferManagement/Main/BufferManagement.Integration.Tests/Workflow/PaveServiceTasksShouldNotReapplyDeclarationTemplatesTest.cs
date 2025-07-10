using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class TransferRuleServiceTaskShouldNotReapplyDeclarationTemplatesTest : PaveServiceTasksShouldNotReapplyDeclarationTemplatesTestCase
	{
		protected override void ConfigureConfigIfRequired()
		{
			base.ConfigureConfigIfRequired();

			Config.ComponentLink.FL_IsReleaseGateRuleApplied = false;
		}

		protected override void RunServiceTaskAction()
		{
			var runner = new TestTransferRuleRunner(Config.System, new DummyLogger());
			runner.Process_ForTest();
		}

		protected override void AssertWorkflowStateAfterProcessing(ProcessHeader updatedWorkflow)
		{
			AssertEquals("The workflow should have been moved. SAD!", Config.Buffer.FC_Name, updatedWorkflow.CurrentComponent?.FC_Name);
		}
	}

	class ReleaseGateShouldNotReapplyDeclarationTemplatesTest : PaveServiceTasksShouldNotReapplyDeclarationTemplatesTestCase
	{
		protected override void ConfigureConfigIfRequired()
		{
			base.ConfigureConfigIfRequired();

			Config.ComponentLink.FL_IsReleaseGateRuleApplied = true;
		}

		protected override void RunServiceTaskAction()
		{
			ReleaseGateKeeperTest.RunReleaseGate(Config.System);
		}

		protected override void AssertWorkflowStateAfterProcessing(ProcessHeader updatedWorkflow)
		{
			AssertEquals("The workflow should have been moved. SAD!", Config.Buffer.FC_Name, updatedWorkflow.CurrentComponent?.FC_Name);
		}
	}

	abstract class PaveServiceTasksShouldNotReapplyDeclarationTemplatesTestCase : BMSTestCaseWithFactory
	{
		public void TestRunServiceTask_WhenChangingCustomsDeclarationWorkflows_FromDifferentCompany_ShouldNotReapplyTemplates()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSTestHelper.EnableBMSInRegistry();
			Config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode);
			ConfigureConfigIfRequired();

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.CompanyName = "Service Task Company";
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.FillWithValidTestData();

			var existingTemplates = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode));

			foreach (var existingTemplate in existingTemplates)
			{
				existingTemplate.P0_IsActive = false;
			}

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode);
			template.GlobalTemplate = true;
			var workflow = BMSTestHelper.CreateWorkflow(template, "Applied Workflow");
			var task = BMSTestHelper.CreateTask(template, workflow, description: "From template");
			task.P9_ShareTasksForAllCompanies = false;

			Factory.Save();

			var declaration = (IWorkflowProvider)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			((BusinessObject)declaration).FillWithValidTestData();

			Factory.Save();

			var jobLevelWorkflow = BMSTestHelper.GetJobHeaderForParent(declaration, Factory, addDefaultProcessHeaderIfNone: false);
			var jobWorkflows = jobLevelWorkflow.ProcessHeaders.Cast<ProcessHeader>().ToArray();

			AssertContainsExactElementsInAnyOrder("If the template isn't applied here, then we haven't set up the test correctly. SAD!",
				new[] { "Applied Workflow" }, jobWorkflows.Select(x => x.FH_CompletionStatement));

			var originalWorkflow = jobWorkflows.Single();
			AssertEquals("The workflow should be in the starting component.", Config.Bucket.FC_Name, originalWorkflow.CurrentComponent?.FC_Name);
			AssertContainsExactElementsInAnyOrder("If the template isn't applied here, then we haven't set up the test correctly. SAD!",
				new[] { "From template" }, originalWorkflow.Tasks.Select(x => x.P9_Description));
			AssertEquals("The applied task should be for the current company.", GlbCompany.CurrentCompany.CompanyName, jobWorkflows.Single().Tasks.Single().Company?.CompanyName);

			AdjustOriginalWorkflowIfRequired(originalWorkflow);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				RunServiceTaskAction();
			}

			var newFactory = new BusinessObjectFactory();
			var updatedWorkflow = newFactory.Load<ProcessHeader>(originalWorkflow.PK);
			AssertWorkflowStateAfterProcessing(updatedWorkflow);

			var updatedTasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, SQLComparisonOperator.Equal, updatedWorkflow.FH_ParentId));

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("The service task should not be applying workflow templates at all. SAD!", new[] { "From template" }, updatedTasks.Select(x => x.P9_Description));
				AssertContainsExactElementsInAnyOrder("Tasks should not be created for the service task's company. SAD!", new[] { GlbCompany.CurrentCompany.CompanyName }, updatedTasks.Select(x => x.Company?.CompanyName));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		protected virtual void ConfigureConfigIfRequired()
		{
		}

		protected abstract void RunServiceTaskAction();

		protected abstract void AssertWorkflowStateAfterProcessing(ProcessHeader updatedWorkflow);

		protected virtual void AdjustOriginalWorkflowIfRequired(ProcessHeader originalWorkflow)
		{
		}

		protected SchematicTestConfig Config { get; private set; }
	}
}
