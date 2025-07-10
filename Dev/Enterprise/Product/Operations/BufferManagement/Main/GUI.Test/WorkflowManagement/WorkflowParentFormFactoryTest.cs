using CargoWise.Common;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowParentFormFactoryTest : BMSTestCaseWithFactory
	{
		public void TestGetFormForDeclarationCreatedOnAnotherCompany()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "BRK");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "XXX";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "XXX";
			Factory.Save();

			Enterprise.Integration.Customs.IBaseJobDeclaration declaration;
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "BRK", description: "Template X");
				var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "Template Workflow X");
				var templateWorkflowTask = BMSTestHelper.CreateTask(template, templateWorkflow, description: "Template Workflow Task X");
				Factory.Save();

				declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				Factory.Save();
			}

			AssertNotEquals("GIVEN declaration is created in another company ", declaration.CompanyPK, GlbCompany.CurrentCompany.GC_Code);

			var loadedDeclaration = Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(declaration.PK);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(loadedDeclaration);

			CombineAssertions("WHEN ShowFormAndNavigateToWorkflowItem THEN should show error to user BUT not sending error reporter", () =>
			{
				AssertContains(
					"Should show error to user",
					"You are trying to view a declaration that belongs to a different company. Please log into the company",
					UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNullOrEmpty("Should not send error reporter", ErrorReporter.LastMessageReported);
			});
		}

		public void TestGetFormForDeletedTask()
		{
			var system = CreateSystem("ORG");
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var task = job.WorkflowItems.AddNew();

			Factory.Save();

			AssertNotNull(task.ProcessHeader);
			task.Delete();

			using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				AssertNull(form);
			}
		}

		public void TestGetFormForJob()
		{
			var system = CreateSystem("ORG");
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory);

			Factory.Save();

			using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(jobHeader))
			{
				AssertNotNull(form);
			}
		}

		public void TestGetFormForWorkflow()
		{
			var system = CreateSystem("ORG");
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow))
			{
				AssertNotNull(form);
			}
		}
	}
}
