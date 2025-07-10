using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessJobHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFH_CompletionStatement()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			jobHeader.FH_CompletionStatement = ZString.Empty;
			AssertHasError(jobHeader.FH_CompletionStatementInfo, "Please enter a Description.");

			jobHeader.FH_CompletionStatement = "Dat Job";
			AssertNoErrors(jobHeader.FH_CompletionStatementInfo);
		}

		public void TestFH_GG_ReleaseGroupValidation()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.ReleaseGroups.AddNew();
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			jobHeader.Validation.ValidateAll();
			AssertNull(jobHeader.ReleaseGroup);
			AssertNoErrors(jobHeader.FH_GG_ReleaseGroupInfo);
		}

		public void TestFH_GG_ReleaseGroupValidation_PartialTemplates()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.ReleaseGroups.AddNew();

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", isPartial: true);
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow = template.ProcessHeaders.AddNew();

			templateJobHeader.Validation.ValidateAll();
			AssertNull(templateJobHeader.ReleaseGroup);
			AssertNoErrors(templateJobHeader.FH_GG_ReleaseGroupInfo);

			templateJobHeader.FH_GG_ReleaseGroup = system.ReleaseGroups.First().PK;

			templateJobHeader.Validation.ValidateAll();
			AssertNull(templateJobHeader.ReleaseGroup);
			AssertHasError(templateJobHeader.FH_GG_ReleaseGroupInfo, "A Release Group cannot be specified on the Job-Level Workflow of a Partial Template.");
		}

		public void TestFH_FC_CurrentComponentValidation()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";
			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var staffWithoutRights = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(staffWithoutRights.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				var job = Factory.NewWithValidTestData<OrgHeader>();
				var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
				var workflow = jobHeader.ProcessHeaders.AddNew();

				jobHeader.Validation.ValidateAll();
				workflow.Validation.ValidateAll();

				AssertNoErrors(jobHeader.FH_FC_CurrentComponentInfo);
				AssertNoErrors(workflow.FH_FC_CurrentComponentInfo);

				jobHeader.FH_FC_CurrentComponent = bucket2.PK;
				workflow.FH_FC_CurrentComponent = bucket2.PK;

				AssertNoErrors(jobHeader.FH_FC_CurrentComponentInfo);
				AssertHasError(workflow.FH_FC_CurrentComponentInfo, "You do not have security rights to change Current Component.");
			}
		}

		public void TestFH_CategoryValidation()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			AssertEquals("Default category value", BMConstants.JobLevelWorkflowCategoryCode, jobHeader.FH_Category);
			jobHeader.Validation.ValidateAll();
			AssertNoErrors(jobHeader.FH_CategoryInfo);

			jobHeader.FH_Category = "XXX";
			jobHeader.Validation.ValidateAll();
			AssertHasError(jobHeader.FH_CategoryInfo, "The category for job-level workflows must be of type JOB");
		}
	}
}
