using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
#if !WINZOR
using System.Windows.Shapes;
#endif
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using OrgHeader = Enterprise.MasterFiles.Business.OrgHeader;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowManagementTabPageTest : BMSTestCaseWithFactory
	{
		#region Unique Index Handler

		public void TestSavingDuplicatedJobLevelWorkflows_ForWorkflowTemplateForm_ShouldReportStacktrace()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflow(template);
			var jobHeader = workflow.JobHeader;
			var task = BMSTestHelper.CreateTask(template, workflow);

			AssertNotNull(jobHeader);

			Factory.Save();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				Application.DoEvents();

				var anotherJobLevelWorkflow = ProcessJobHeaderTest.CreateDuplicateJobLevelWorkflowForTemplate((ProcessTaskTemplate)form.BusinessEntity);
				var result = form.FireSaveButton();

				AssertEquals(ContinueWithSave.No, result);

				AssertContains("There are duplicate job-level workflows. This form will need to be closed and re-opened to correct this problem.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Cannot Save", UnitTestUserNotification.Instance.LastMessage.Caption);

				AssertContains("at Enterprise.BufferManagement.GUI.Test.WorkflowManagementTabPageTest.TestSavingDuplicatedJobLevelWorkflows_ForWorkflowTemplateForm_ShouldReportStacktrace(", ErrorReporter.LastMessageReported);
				AssertContains("at Enterprise.BufferManagement.Business.Test.ProcessJobHeaderTest.CreateDuplicateJobLevelWorkflowForTemplate(", ErrorReporter.LastMessageReported);

				anotherJobLevelWorkflow.Delete();

				AssertSaved(form.FireSaveButton());

				ErrorReporter.Clear();
			}
		}

		public void TestSavingDuplicatedJobLevelWorkflows_ForOperationalJobForm_ShouldNotFailToSave()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var job = (OrgHeader)BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader(job);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Borten", releaseGroupPK: config.ReleaseGroup.PK);

			BMSRegistry.Instance.RequireReleaseGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false); //Otherwise, it gets the following error on the default workflow 'FH_GG_ReleaseGroup: Please enter a Release Group.'

			Factory.Save();

			using (var form = (ZOrganisationsForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow))
			{
				var organisation = (OrgHeader)form.BusinessEntity;

				BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(organisation);

				form.FireValidateAllForTest();
				AssertNoErrors(organisation);

				AssertSaved(form.FireSaveButton());

				var anotherJobLevelWorkflow = ProcessJobHeaderTest.CreateDuplicateJobLevelWorkflowForJob(organisation, organisation.Factory);
				var result = form.FireSaveButton();

				AssertSaved(form.FireSaveButton());
				AssertEquals(true, anotherJobLevelWorkflow.IsDeleted);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNotContains("Duplicate job-level workflow created.", ErrorReporter.LastMessageReported);
				AssertNotContains(@"Description: ...Backrolls?
Parent Table Code: OH
Is in database: False", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		public void TestShouldNotCreateDuplicateJobLevelWorkflow_WhenTemplateIsAppliedToAnEmptyJob_FromTwoInstancesOfAJobForm_RepresentingTheSameJob()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			AssertNull("This should be an empty job with no job level workflows", jobHeader);

			var templateFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var template = BMSTestHelper.CreateWorkflowTemplate(templateFactory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			templateWorkflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var templateJobHeader = templateWorkflow.JobHeader;
			templateJobHeader.FH_CompletionStatement = "Highlander";
			BMSTestHelper.CreateTask(template, templateWorkflow);

			templateFactory.Save();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			var job1 = factory1.Load<OrgHeader>(job.PK);
			var job2 = factory2.Load<OrgHeader>(job.PK);

			factory2.SeedQueryCache(ProcessHeaderSchema.Constants.TableName, new ZQuery()); // for some reason in the defect, the query cache on the second form's factory is there...

			using (var form1 = new ZOrganisationsForm(job1))
			using (var form2 = new ZOrganisationsForm(job2))
			{
				void SelectTabOnFormToInvokeJobWorkflowBindingAndOrJobWorkflowCreation(ZOrganisationsForm f, ZTabPage page)
				{
					f.OrganisationsTabControl.SelectedTab = page;
					Application.DoEvents();
				}

				form1.Show();
				Application.DoEvents();

				form2.Show();
				Application.DoEvents();

				var workflowTabPage = (ZWorkflowTabPage)form1.OrganisationsTabControl.TabPages["WorkflowTabPage"];

				SelectTabOnFormToInvokeJobWorkflowBindingAndOrJobWorkflowCreation(form1, workflowTabPage);

				var form1WorkflowsGrid = workflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				var form1JobHeader = form1WorkflowsGrid.List[0] as ProcessJobHeader;
				AssertNotNull("Selecting the workflow tab page should create a default job header", form1JobHeader);
				AssertNotEquals("Template should not be applied", "Highlander", form1JobHeader.FH_CompletionStatement);

				job1.OH_FullName = "Immortal Inc"; // to ensure that save is fired and the template is applied
				Application.DoEvents();

				((IBusinessObjectFactoryInternals)factory2).DisableQueryCacheReset = true; // Otherwise it will clear factory2's cache, which means this defect won't be reproduced.

				AssertSaved("Should save the original form without issues", form1.FireSaveButton());
				Application.DoEvents();

				((IBusinessObjectFactoryInternals)factory2).DisableQueryCacheReset = false;

				AssertEquals("Template should be applied", "Highlander", form1JobHeader.FH_CompletionStatement);
				AssertEquals("Zoot! Review.", form1JobHeader.ProcessHeaders.Single().FH_CompletionStatement);

				var form2WorkflowTabPage = (ZWorkflowTabPage)form2.OrganisationsTabControl.TabPages["WorkflowTabPage"];

				SelectTabOnFormToInvokeJobWorkflowBindingAndOrJobWorkflowCreation(form2, form2WorkflowTabPage);

				var form2WorkflowsGrid = form2WorkflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				var form2JobHeader = form2WorkflowsGrid.List[0] as ProcessJobHeader;
				AssertNotNull("Selecting the workflow tab page should create a default job header on the other form", form2JobHeader);

				AssertSaved("The new form should save without any duplicate workflow exceptions", form2.FireSaveButton());
				Application.DoEvents();
			}
		}

		public void TestShouldNotCreateDuplicateJobLevelWorkflow_WhenATemplateWithoutWorkflowsIsApplied_FromTwoInstancesOfAJobForm_RepresentingTheSameJob()
		{
			// create a job with no workflows or tasks
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			AssertNull("This should be an empty job with no job level workflows", jobHeader);

			// create a template without workflows
			var templateFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var template = BMSTestHelper.CreateWorkflowTemplate(templateFactory, "ORG");
			templateFactory.Save();

			// open 2 jobs
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			var job1 = factory1.Load<OrgHeader>(job.PK);
			var job2 = factory2.Load<OrgHeader>(job.PK);

			factory2.SeedQueryCache(ProcessHeaderSchema.Constants.TableName, new ZQuery()); // for some reason in the defect, the query cache on the second form's factory is there...

			using (var form1 = new ZOrganisationsForm(job1))
			using (var form2 = new ZOrganisationsForm(job2))
			{
				void SelectTabOnFormToInvokeJobWorkflowBindingAndOrJobWorkflowCreation(ZOrganisationsForm f, ZTabPage page)
				{
					f.OrganisationsTabControl.SelectedTab = page;
					Application.DoEvents();
				}

				void CheckFormHasOneJobLevelWorkflow(ZOrganisationsForm form, ZWorkflowTabPage workflowTabPage)
				{
					SelectTabOnFormToInvokeJobWorkflowBindingAndOrJobWorkflowCreation(form, workflowTabPage);

					var formWorkflowsGrid = workflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
					var formJobHeader = formWorkflowsGrid.List[0] as ProcessJobHeader;
					AssertNotNull("Selecting the workflow tab page should show a default job header", formJobHeader);
					AssertNotEquals("Template should not be applied", "Highlander", formJobHeader.FH_CompletionStatement);
				}

				form1.Show();
				Application.DoEvents();

				form2.Show();
				Application.DoEvents();

				var form1WorkflowTabPage = (ZWorkflowTabPage)form1.OrganisationsTabControl.TabPages["WorkflowTabPage"];
				var form2WorkflowTabPage = (ZWorkflowTabPage)form2.OrganisationsTabControl.TabPages["WorkflowTabPage"];

				// for both forms: navigate to Workflow and Tracking and assert both forms contain 1 default JLW
				CheckFormHasOneJobLevelWorkflow(form1, form1WorkflowTabPage);
				CheckFormHasOneJobLevelWorkflow(form2, form2WorkflowTabPage);

				// edit job on form1 - the workflows and JLW from the template should be applied and the original JLW should be replaced
				job1.OH_FullName = "Immortal Inc"; // to ensure that save is fired and the template is applied
				Application.DoEvents();

				((IBusinessObjectFactoryInternals)factory2).DisableQueryCacheReset = true; // Otherwise it will clear factory2's cache, which means this defect won't be reproduced.

				AssertSaved("Should save the original form without issues", form1.FireSaveButton());
				Application.DoEvents();

				((IBusinessObjectFactoryInternals)factory2).DisableQueryCacheReset = false;

				var form1WorkflowsGrid = form1WorkflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				var form1JobHeader = form1WorkflowsGrid.List[0] as ProcessJobHeader;

				AssertNotNullOrEmpty("A job level workflow should be applied", form1JobHeader.FH_CompletionStatement);
				AssertEquals("Job Workflow", form1JobHeader.ProcessHeaders.Single().FH_CompletionStatement);

				// add a new workflow to form 2
				var form2WorkflowsGrid = form2WorkflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				var form2JobHeader = form2WorkflowsGrid.List[0] as ProcessJobHeader;

				// fire save button: force replication of behaviour
				var result = form2.FireSaveButton();

				form2WorkflowsGrid = form2WorkflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				form2JobHeader = form2WorkflowsGrid.List[0] as ProcessJobHeader;

				AssertEquals(true, form2WorkflowsGrid.ReadOnly);
				AssertContains("Another user has made changes that will override yours.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("You can refer to the Job-Level Workflow's Job Notes", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("A user attempted to create and save new workflows", form2JobHeader.WorkflowNote.ST_NoteDataAsText);
			}
		}

		public void TestShouldNotCreateDuplicateJobLevelWorkflow_WhenAddingANewWorkflow_FromTwoInstancesOfAJobForm_RepresentingTheSameJob()
		{
			// create a job with no workflows or tasks
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			AssertNull("This should be an empty job with no job level workflows", jobHeader);

			// create a template with workflows and job level workflows
			var templateFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var template = BMSTestHelper.CreateWorkflowTemplate(templateFactory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			templateWorkflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var templateJobHeader = templateWorkflow.JobHeader;
			templateJobHeader.FH_CompletionStatement = "Highlander";
			BMSTestHelper.CreateTask(template, templateWorkflow);

			templateFactory.Save();

			// open 2 instances of the same job in different factories
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			var job1 = factory1.Load<OrgHeader>(job.PK);
			var job2 = factory2.Load<OrgHeader>(job.PK);

			factory2.SeedQueryCache(ProcessHeaderSchema.Constants.TableName, new ZQuery()); // for some reason in the defect, the query cache on the second form's factory is there...

			using (var form1 = new ZOrganisationsForm(job1))
			using (var form2 = new ZOrganisationsForm(job2))
			{
				void SelectTabOnFormToInvokeJobWorkflowBindingAndOrJobWorkflowCreation(ZOrganisationsForm f, ZTabPage page)
				{
					f.OrganisationsTabControl.SelectedTab = page;
					Application.DoEvents();
				}

				void CheckFormHasOneJobLevelWorkflow(ZOrganisationsForm form, ZWorkflowTabPage workflowTabPage)
				{
					SelectTabOnFormToInvokeJobWorkflowBindingAndOrJobWorkflowCreation(form, workflowTabPage);

					var formWorkflowsGrid = workflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
					var formJobHeader = formWorkflowsGrid.List[0] as ProcessJobHeader;
					AssertNotNull("Selecting the workflow tab page should show a default job header", formJobHeader);
					AssertNotEquals("Template should not be applied", "Highlander", formJobHeader.FH_CompletionStatement);
				}

				form1.Show();
				Application.DoEvents();

				form2.Show();
				Application.DoEvents();

				var form1WorkflowTabPage = (ZWorkflowTabPage)form1.OrganisationsTabControl.TabPages["WorkflowTabPage"];
				var form2WorkflowTabPage = (ZWorkflowTabPage)form2.OrganisationsTabControl.TabPages["WorkflowTabPage"];

				// for both forms: navigate to Workflow and Tracking and assert both forms contain 1 default JLW
				CheckFormHasOneJobLevelWorkflow(form1, form1WorkflowTabPage);
				CheckFormHasOneJobLevelWorkflow(form2, form2WorkflowTabPage);

				// edit job on form1 - the workflows and JLW from the template should be applied and the original JLW should be replaced
				job1.OH_FullName = "Immortal Inc"; // to ensure that save is fired and the template is applied
				Application.DoEvents();

				((IBusinessObjectFactoryInternals)factory2).DisableQueryCacheReset = true; // Otherwise it will clear factory2's cache, which means this defect won't be reproduced.

				AssertSaved("Should save the original form without issues", form1.FireSaveButton());
				Application.DoEvents();

				((IBusinessObjectFactoryInternals)factory2).DisableQueryCacheReset = false;

				var form1WorkflowsGrid = form1WorkflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				var form1JobHeader = form1WorkflowsGrid.List[0] as ProcessJobHeader;

				AssertEquals("Template should be applied", "Highlander", form1JobHeader.FH_CompletionStatement);
				AssertEquals("Zoot! Review.", form1JobHeader.ProcessHeaders.Single().FH_CompletionStatement);

				// add a new workflow to form 2
				var form2WorkflowsGrid = form2WorkflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				var form2JobHeader = form2WorkflowsGrid.List[0] as ProcessJobHeader;

				var workflowToAdd = BMSTestHelper.CreateWorkflow(form2JobHeader, "test new workflow", releaseGroupPK: config.ReleaseGroup.PK);
				var result = form2.FireSaveButton();

				form2WorkflowsGrid = form2WorkflowTabPage.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				form2JobHeader = form2WorkflowsGrid.List[0] as ProcessJobHeader;

				AssertEquals(true, form2WorkflowsGrid.ReadOnly);
				AssertContains("Another user has made changes that will override yours.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("You can refer to the Job-Level Workflow's Job Notes", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("test new workflow", form2JobHeader.WorkflowNote.ST_NoteDataAsText);
				AssertContains(form1JobHeader.ProcessHeaders.Single().FH_CompletionStatement, form2JobHeader.WorkflowNote.ST_NoteDataAsText);
			}
		}

		#endregion

		public void TestOpeningTabShouldNotTriggerHasChanges()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);
			CreateTaskWithoutBMS(job, "Peace on earth");
			CreateTaskWithoutBMS(job, "Will we ever see it?");
			CreateTaskWithoutBMS(job, "In our lifetimes");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var config = TestConfigsHelper.CreateSchematicTestConfig(newFactory);
			newFactory.Save();

			using (var form = new ZOrganisationsForm(job))
			{
				form.Show();

				var workflowTabPage = (ZWorkflowTabPage)form.OrganisationsTabControl.TabPages["WorkflowTabPage"];
				form.OrganisationsTabControl.SelectedTab = workflowTabPage;

				Application.DoEvents();

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
				AssertNotNull(jobHeader);
				AssertEquals(false, job.HasChanges);
			}
		}

		public void TestDeleteDefaultWorkflow_ShouldNotRecreateOnNextAccess()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			using (var form = new ZOrganisationsForm(job))
			{
				form.Show();

				var workflowTabPage = (ZWorkflowTabPage)form.OrganisationsTabControl.TabPages["WorkflowTabPage"];
				form.OrganisationsTabControl.SelectedTab = workflowTabPage;

				Application.DoEvents();

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
				AssertNotNull(jobHeader);

				var managementTab = (WorkflowManagementTabPage)workflowTabPage.FindAll<ZWorkflowUserControl>().First().MainTabControl.TabPages[0];
				var workflowsGrid = managementTab.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				var tasksGrid = managementTab.FindAll<TaskDetailsUserControl>().Single().TasksGrid;

				AssertEquals("Should just have the jobHeader record", 1, workflowsGrid.List.Count);
				AssertEquals("No tasks are present yet", 0, tasksGrid.List.Count);
				AssertEquals(workflowsGrid.List[0], jobHeader);

				var newTask = job.WorkflowItems.Tasks.AddNew();

				AssertEquals("Should have added the default workflow", 2, workflowsGrid.List.Count);
				AssertEquals("There is now one task in the grid", 1, tasksGrid.List.Count);

				AssertEquals(workflowsGrid.List[1], newTask.ProcessHeader);

				newTask.ProcessHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
				newTask.P9_Type = "UDF";
				AssertNoErrors(newTask);
				AssertNoErrors((ProcessHeader)newTask.ProcessHeader);

				AssertSaved(form.FireSaveButton());

				workflowsGrid.Select(1);
				workflowsGrid.DeleteMenuItem.PerformClick();

				AssertSaved(form.FireSaveButton());
			}

			using (var form = new ZOrganisationsForm(job))
			{
				form.Show();

				var workflowTabPage = (ZWorkflowTabPage)form.OrganisationsTabControl.TabPages["WorkflowTabPage"];
				form.OrganisationsTabControl.SelectedTab = workflowTabPage;

				Application.DoEvents();

				var managementTab = (WorkflowManagementTabPage)workflowTabPage.FindAll<ZWorkflowUserControl>().First().MainTabControl.TabPages[0];
				var workflowsGrid = managementTab.FindAll<WorkflowsUserControl>().First().WorkflowsGrid;
				var tasksGrid = managementTab.FindAll<TaskDetailsUserControl>().Single().TasksGrid;

				AssertEquals("Should just have the jobHeader record", 1, workflowsGrid.List.Count);
				AssertEquals("No tasks are present", 0, tasksGrid.List.Count);
			}
		}

		public void TestDeleteWorkflow_ShouldDeleteShapeAndAttachments()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);

			org.Validation.ValidateAll();
			AssertEquals(false, org.HasErrors);

			Factory.Save();

			var loadedOrg = new BusinessObjectFactory().Load<OrgHeader>(org.PK);

			using (var form = (ZOrganisationsForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task1))
			{
				form.Show();
				Application.DoEvents();

				var workflowTabPage = (ZWorkflowTabPage)form.OrganisationsTabControl.TabPages["WorkflowTabPage"];
				AssertEquals("Should be selected on workflow tab page", workflowTabPage, form.OrganisationsTabControl.SelectedTab);

				var trackingUserControl = workflowTabPage.FindAll<ZWorkflowUserControl>().First();
				var managementTab = (WorkflowManagementTabPage)trackingUserControl.MainTabControl.TabPages[0];
				AssertEquals("Should be selected on management tab page", managementTab, trackingUserControl.MainTabControl.SelectedTab);

				var workflowsUserControl = managementTab.FindAll<WorkflowsUserControl>().First();
				workflowsUserControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsUserControl.NCNTabPage_Exposed;
				workflowsUserControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsUserControl.WorkflowsTabPage_Exposed;

				workflowsUserControl.WorkflowsGrid.Select(1);
				workflowsUserControl.WorkflowsGrid.DeleteMenuItem.PerformClick();

				var saveResult = form.FireSaveButton();
				var message = string.Format("Should have been able to save form, but the error occurred: " + UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(message, ContinueWithSave.Yes, saveResult);
			}
		}

		public void TestRelationshipDesigner_DiagramDeletedViaDataBusRefresh()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory, false);
			var workflow1 = CreateWorkflow(jobHeader, "Nermy");
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);

			var defaultShape = jobHeader.GetDefaultDiagram();
			defaultShape.HasChanges = true; // Nudge the shape so it will save.

			Factory.Save();
			var formFactory = Factory.CreateNewFactory();
			var loadedOrg = formFactory.Load<OrgHeader>(org.PK);
			using (var form = new ZOrganisationsForm(loadedOrg))
			{
				form.Show();
				Application.DoEvents();
				ExposeNCNTabPage(task1, form);

				var loadedJobHeader = formFactory.Load<ProcessJobHeader>(jobHeader.PK);
				var loadedDefaultShape = loadedJobHeader.GetDefaultDiagram();

				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());

				jobHeader.GetDefaultDiagram().Delete();
				Factory.Save();
				Application.DoEvents();

				Assert("Additionally, we know that data refresh didn't explode the loaded NCN", loadedDefaultShape.IsDeleted);

				jobHeader.GetDefaultDiagram().Delete();
				Factory.Save();
				Application.DoEvents();
			}

			AssertEquals(0, Factory.Load<BMNCNShape>(new ZQuery()).Length);
		}

		public void TestRelationshipDesigner_JobHeaderDeletedViaDataBusRefresh()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory, false);
			var workflow1 = CreateWorkflow(jobHeader, "Nermy");
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);

			Factory.Save();
			var formFactory = Factory.CreateNewFactory();
			var loadedOrg = formFactory.Load<OrgHeader>(org.PK);
			using (var form = new ZOrganisationsForm(loadedOrg))
			{
				form.Show();
				Application.DoEvents();
				ExposeNCNTabPage(task1, form);

				var loadedJobHeader = formFactory.Load<ProcessJobHeader>(jobHeader.PK);
				var defaultDiagram = loadedJobHeader.GetDefaultDiagram();

				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());

				var defaultShape = loadedJobHeader.ProcessHeaders[0].GetDefaultShape(defaultDiagram);

				defaultShape.HasChanges = true;

				jobHeader.Delete();
				Factory.Save();
				Application.DoEvents();

				Assert("Additionally, we know that data refresh didn't explode the loaded NCN", loadedJobHeader.IsDeleted);

				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
			}

			AssertEquals(0, Factory.Load<BMNCNShape>(new ZQuery()).Length);
		}

#if !WINZOR
		public void TestRelationshipDesigner_WhenInvalidNetworkExists_ShouldNotDrawUntilNetworkIsCorrected()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);
			var task3 = BMSTestHelper.CreateTask(workflow3);

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3_1 = workflow3.GetOrCreateDependencyLink(workflow1);

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.Organisation).ShowEditForm((OrgHeader)jobHeader.Parent))
			{
				Application.DoEvents();
				ExposeNCNTabPage(task1, form);

				var workflowsControl = form.FindAll<WorkflowsUserControl>().First();
				var coverLabel = workflowsControl.NCNTabPage_Exposed.FindAll<ZLabel>().Single();
				AssertEquals(true, coverLabel.Visible);

				AssertEquals("The Workflow Relationship Designer cannot be shown since the network has an invalid relationship. Please correct this relationship and click this message to show the network.", coverLabel.Text);

				workflowsControl.RelationshipDesignerUserControl.CoverLabel_Click(coverLabel, EventArgs.Empty);
				AssertEquals("This network is still invalid and cannot be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				var loadedLink = form.BusinessEntity.Factory.Load<ProcessHeaderLink>(link3_1.PK);
				loadedLink.Delete();

				// Network is now valid, so show the network control.
				workflowsControl.RelationshipDesignerUserControl.CoverLabel_Click(coverLabel, EventArgs.Empty);
				AssertEquals(false, coverLabel.Visible);

				var elementHost = workflowsControl.NCNTabPage_Exposed.FindAll<ZElementHost>().SingleOrDefault();

				AssertNotNull(elementHost);
				AssertEquals(true, elementHost.Visible);

				workflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsControl.WorkflowsTabPage_Exposed;
				Application.DoEvents();

				// Make the network invalid.
				var loadedWorkflow1 = form.BusinessEntity.Factory.Load<ProcessHeader>(workflow1.PK);
				var loadedWorkflow3 = form.BusinessEntity.Factory.Load<ProcessHeader>(workflow3.PK);

				var newInvalidLink = loadedWorkflow3.GetOrCreateDependencyLink(workflow1);

				// The cover label should be back and network should not be shown.
				workflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsControl.NCNTabPage_Exposed;
				Application.DoEvents();

				AssertEquals(true, coverLabel.Visible);
				AssertEquals(false, elementHost.Visible);

				workflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsControl.WorkflowsTabPage_Exposed;
				Application.DoEvents();

				// Make the network valid again.
				newInvalidLink.Delete();

				workflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsControl.NCNTabPage_Exposed;
				workflowsControl.RelationshipDesignerUserControl.CoverLabel_Click(coverLabel, EventArgs.Empty);
				Application.DoEvents();

				// Network should again be shown.
				AssertEquals(false, coverLabel.Visible);
				AssertEquals(true, elementHost.Visible);
			}
		}
#endif

		public void TestRelationshipDesigner_WhenInvalidNetworkExists_LinksBetweenJobs_ShouldNotStackOverflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var job2 = Factory.NewWithValidTestData<OrgHeader>();

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job1);
			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job2);

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow2", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow3", releaseGroupPK: config.ReleaseGroup.PK);

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3_1 = workflow3.GetOrCreateDependencyLink(workflow1);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task1))
			{
				Application.DoEvents();

				var result = form.FireSaveButton();
				Application.DoEvents();

				AssertSaved("Although an invalid network exists, we don't validate it until it's touched as this can be very expensive, and we trust that the places we could create an invalid network won't let us get here anyway", result);

				var loadedLink = form.BusinessEntity.Factory.Load<ProcessHeaderLink>(link3_1.PK);
				loadedLink.Delete();

				result = form.FireSaveButton();
				Application.DoEvents();

				AssertSaved(result);
			}
		}

		public void TestRelationshipDesigner_DoNotAccessDeletedBusinessObjectProperties()
		{
			var system = CreateSystem("ORG");

			var org = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory, false);
			var workflow = CreateWorkflow(jobHeader, "Test");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0);

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var workflowInNewFactory = newFactory.Load<ProcessHeader>(workflow.PK);
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);

			using (var form = new ZOrganisationsForm(org))
			using (var form2 = new ZOrganisationsForm(orgInNewFactory))
			{
				form.Show();
				Application.DoEvents();

				ExposeNCNTabPage(task, form);

				form2.Show();
				Application.DoEvents();

				orgInNewFactory.OH_FullName = orgInNewFactory.OH_FullName + " Test";
				workflowInNewFactory.FH_CompletionStatement = "dfsdff";
				newFactory.Save();

				form2.FireSaveButton();
				Application.DoEvents();
				form2.Close();
				Application.DoEvents();

				form.BringToFront();
				Application.DoEvents();
				workflow.FH_CompletionStatement = workflow.FH_CompletionStatement + "Test";

				AssertNoExceptionThrown(delegate
				{
					org.Delete();
					Application.DoEvents();
				});
			}
		}

		static void ExposeNCNTabPage(ProcessTask task, ZForm form)
		{
			WorkflowParentFormFactory.NavigateToWorkflowItem(form, task);
			Application.DoEvents();

			var tabControl = form.FindAll<ZTabControl>().First();
			var workflowTabPage = (ZWorkflowTabPage)tabControl.TabPages["WorkflowTabPage"];
			AssertEquals("Should be selected on workflow tab page", workflowTabPage, tabControl.SelectedTab);

			var trackingUserControl = workflowTabPage.FindAll<ZWorkflowUserControl>().First();
			var managementTab = (WorkflowManagementTabPage)trackingUserControl.MainTabControl.TabPages[0];
			AssertEquals("Should be selected on management tab page", managementTab, trackingUserControl.MainTabControl.SelectedTab);

			var workflowsUserControl = managementTab.FindAll<WorkflowsUserControl>().First();
			workflowsUserControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsUserControl.NCNTabPage_Exposed;

			Application.DoEvents();
		}

		public void TestOpenRelationshipDesigner_ShouldCreateDiagramShapeAndAttachmentsForAllWorkflows()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "MAIORGSYD";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_PostCode = "2000";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);
			var task3 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 0);

			org.Validation.ValidateAll();
			AssertNoErrors(org);

			Factory.Save();

			using (var form = (ZOrganisationsForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task1))
			{
				Application.DoEvents();

				var workflowTabPage = (ZWorkflowTabPage)form.OrganisationsTabControl.TabPages["WorkflowTabPage"];
				AssertEquals("Should be selected on workflow tab page", workflowTabPage, form.OrganisationsTabControl.SelectedTab);

				var trackingUserControl = workflowTabPage.FindAll<ZWorkflowUserControl>().First();
				var managementTab = (WorkflowManagementTabPage)trackingUserControl.MainTabControl.TabPages[0];
				AssertEquals("Should be selected on management tab page", managementTab, trackingUserControl.MainTabControl.SelectedTab);

				var workflowsUserControl = managementTab.FindAll<WorkflowsUserControl>().First();
				workflowsUserControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsUserControl.NCNTabPage_Exposed;

				Application.DoEvents();

				var shape = form.BusinessEntity.Factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, workflow1.PK) { FetchOnlyFromLocalCache = true });
				shape.IsCriticalPath = true;

				form.BusinessEntity.Factory.Save();
			}

			var workflow1Shape = Factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, workflow1.PK));
			AssertNotNull(workflow1Shape);

			var workflow2Shape = Factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, workflow2.PK));
			AssertNull("No changes were made, so didn't need saving.", workflow2Shape);

			var workflow3Shape = Factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, workflow3.PK));
			AssertNull("No changes were made, so didn't need saving.", workflow3Shape);

			var diagramShape = Factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, jobHeader.PK));
			AssertNotNull(diagramShape);
		}

#if !WINZOR
		public void TestRelationshipDesigner_ShouldNotShowHeaderLine()
		{
			CreateSystem("INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.SalesEnquiry);

			using (var form = (ZForm)controller.ShowNewForm())
			{
				Application.DoEvents();
				ExposeNCNTabPage(task, form);

				var elementHost = form.FindAll<ElementHost>().Single();
				var networkControl = elementHost.Child;
				var line = networkControl.FindChildren<Line>().Single(x => x.Name == "HeaderLine");

				AssertEquals(false, line.IsVisible);
			}
		}
#endif

		public void TestHostedManagementUserControl()
		{
			TabControl.TabPages.Add(ManagementTabPage);
			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();

			AssertEquals("Hosted control type", typeof(WorkflowManagementUserControl), ManagementTabPage.Controls[0].GetType());
			AssertEquals("Hosted control DockStyle", DockStyle.Fill, ManagementTabPage.Controls[0].Dock);
		}

		public void TestNavigateToTask()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			var task1 = jobHeader.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			var task2 = jobHeader.Parent.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;

			using (var form = new ZForm())
			using (var tabPage = new WorkflowManagementTabPage())
			using (var tabControl = new ZTabControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage);
				form.Show();
				tabPage.SetDataBinding(jobHeader.Parent, "");

				var control = tabPage.Controls.OfType<WorkflowManagementUserControl>().Single();

				control.NavigateToWorkflowItem(task2);

				AssertEquals(workflow2, control.WorkflowsGrid.ListManager.GetCurrent());
				AssertEquals(task2, control.TasksGrid_ForTest.ListManager.GetCurrent());

				control.NavigateToWorkflowItem(task1);

				AssertEquals(workflow1, control.WorkflowsGrid.ListManager.GetCurrent());
				AssertEquals(task1, control.TasksGrid_ForTest.ListManager.GetCurrent());
			}
		}

		public void TestNavigateToWorkflow()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var system = CreateSystem("ORG");
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			workflow1.MakePrerequisiteOf(workflow2);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow2))
			{
				Application.DoEvents();
				var workflowsControl = form.FindAll<WorkflowsUserControl>().Single();
				var tasksControl = form.FindAll<TaskDetailsUserControl>().Single();

				var selectedWorkflow = workflowsControl.WorkflowsGrid.ListManager.GetCurrent() as ProcessHeader;
				var selectedTask = tasksControl.TasksGrid.ListManager.GetCurrent() as ProcessTask;

				AssertNotNull(selectedWorkflow);
				AssertNotNull(selectedTask);

				AssertEquals(workflow2.PK, selectedWorkflow.PK);
				AssertEquals(task.PK, selectedTask.PK);
			}
		}

		public void TestCustomFields()
		{
			var workItem = Factory.New<IWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();
				var tabControl = form.FindAll<ZTemplateTabControl>().Single();
				var customFieldsControl = form.Controls.Find("WorkItemCustomFields", true)[0];
				tabControl.SelectedIndex = 0;
				Assert(customFieldsControl.Visible);

				tabControl.SelectedIndex = 3;
				Application.DoEvents();

				tabControl.SelectedIndex = 0;
				Application.DoEvents();

				Assert(customFieldsControl.Visible);
			}
		}

		public void TestCompletedTime_ShouldDisplayCompletedTimeAsLocalTime()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = new ZForm())
			using (var tabPage = new WorkflowManagementTabPage())
			using (var tabControl = new ZTabControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage);
				form.Show();
				tabPage.SetDataBinding(jobHeader.Parent, "");

				Application.DoEvents();

				workflow.Tasks.First().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				var now = ZDateTimeOffset.Now;
				workflow.Tasks.First().P9_CompletedTime = now;

				Application.DoEvents();

				AssertEquals("Pre-condition: The CompletedTimeLocal property for the task should be correct", now.ToZDateTime().ToStandardDateTimeString(), workflow.Tasks.First().CompletedTimeLocal.ToStandardDateTimeString());

				var control = tabPage.Controls.OfType<WorkflowManagementUserControl>().Single();

				control.NavigateToWorkflowItem(task);
				control.TasksGrid_ForTest.SetColumnVisible(true, "CompletedTimeLocal");

				Application.DoEvents();

				var completedTimeGridIndex = control.TasksGrid_ForTest.Columns.Where(c => c.IsVisible).IndexOf(c => c.ColumnName == "CompletedTimeLocal");

				AssertEquals("Correct CompletedTimeLocal value should be displayed in the appropriate column", now.ToZDateTime().ToString(), control.TasksGrid_ForTest[0, completedTimeGridIndex].ToString());
			}
		}

		public void TestDoNotSortMultipleTimes()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = new ZForm())
			using (var tabPage = new WorkflowManagementTabPage())
			using (var tabControl = new ZTabControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage);
				form.Show();
				tabPage.SetDataBinding(jobHeader.Parent, "");

				Application.DoEvents();

				var control = tabPage.Controls.OfType<WorkflowManagementUserControl>().Single();

				control.TaskDetailsControl.SetDataBinding(workflow, "");
				control.TaskDetailsControl.SetDataBinding(workflow, "");
				control.TaskDetailsControl.SetDataBinding(workflow, "");

				int sortCount = 0;
				int listChanged = 0;
				((IBusinessObjectCollection)workflow.TaskCollectionIncludingChildWorkflowTasks).ListChanged += (s, e) => listChanged++;
				((IBusinessObjectCollection)workflow.TaskCollectionIncludingChildWorkflowTasks).SortChanged += (s, e) => sortCount++;
				workflow.TaskCollectionIncludingChildWorkflowTasks.Rebuild();

				AssertEquals(0, sortCount);
				AssertEquals(0, listChanged);
			}
		}

		public void TestButDoSortAtLeastOnce()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);
			BMSTestHelper.CreateTask(workflow2);

			Factory.Save();
			((IBusinessObjectCollection)workflow.TaskCollectionIncludingChildWorkflowTasks).ApplySort(new SortInfo("P9_Sequence", System.ComponentModel.ListSortDirection.Descending));

			int sortCount = 0;
			int listChangedCount = 0;
			((IBusinessObjectCollection)workflow.TaskCollectionIncludingChildWorkflowTasks).SortChanged += (s, e) => sortCount++;
			((IBusinessObjectCollection)workflow.TaskCollectionIncludingChildWorkflowTasks).ListChanged += (s, e) => listChangedCount++;

			using (var form = new ZForm())
			using (var tabPage = new WorkflowManagementTabPage())
			using (var tabControl = new ZTabControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage);
				tabControl.Dock = DockStyle.Fill;
				form.Size = new System.Drawing.Size(1000, 1000);
				form.Show();
				tabPage.SetDataBinding(jobHeader.Parent, "");

				Application.DoEvents();

				var control = tabPage.Controls.OfType<WorkflowManagementUserControl>().Single();
				control.WorkflowsGrid.CurrentRowIndex = 0;

				Application.DoEvents();
				control.WorkflowsGrid.CurrentRowIndex = 1;

				AssertEquals("No sorts is better right?", 4, sortCount);
			}
		}

		public void TestGridOrdering()
		{
			var system = CreateSystem("ORG");
			Factory.Save();
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var wfl = BMSTestHelper.CreateWorkflow(template);
			var templateTask = BMSTestHelper.CreateTask(template, wfl);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);
			var task3 = BMSTestHelper.CreateTask(workflow);
			task1.P9_Sequence = 1;
			task2.P9_Sequence = 3;
			task3.P9_Sequence = 2;
			task3.P9_ParentTemplateID = templateTask.PK;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow))
			{
				Application.DoEvents();
				var workflowsControl = form.FindAll<WorkflowManagementTabPage>().Single();
				var control = workflowsControl.Controls.OfType<WorkflowManagementUserControl>().Single();
				var sequenceColumn = control.TasksGrid_ForTest.Columns.IndexOf(c => c.ColumnName == "P9_Sequence");
				AssertEquals("Correct sequence", "1", control.TasksGrid_ForTest[0, sequenceColumn].ToString());
				AssertEquals("Correct sequence", "2", control.TasksGrid_ForTest[1, sequenceColumn].ToString());
				AssertEquals("Correct sequence", "3", control.TasksGrid_ForTest[2, sequenceColumn].ToString());

				(workflowsControl.Controls.OfType<WorkflowManagementUserControl>().Single().TasksGrid_ForTest.List as WorkflowItemCollectionView).Sort(nameof(task3.P9_ParentTemplateID));
				(workflowsControl.Controls.OfType<WorkflowManagementUserControl>().Single().TasksGrid_ForTest.List as WorkflowItemCollectionView).Rebuild();
				Application.DoEvents();
				AssertEquals("Correct sequence", "1", control.TasksGrid_ForTest[0, sequenceColumn].ToString());
				AssertEquals("Correct sequence", "3", control.TasksGrid_ForTest[1, sequenceColumn].ToString());
				AssertEquals("Correct sequence", "2", control.TasksGrid_ForTest[2, sequenceColumn].ToString());
			}
		}

		public void TestManagementTabIsDefaultOnFormsWithWorkflowEnabled()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var system = CreateSystem("ORG");
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow1))
			{
				Application.DoEvents();
				var workflowsControl = form.FindAll<WorkflowManagementTabPage>().Single();

				var parent = (ZTabControl)workflowsControl.Parent;

				AssertEquals("ManagementTab", parent.SelectedTab.Name);
			}
		}

		public void TestTagLinkDeletedByTagilator_AndEditedByForm_ShouldBeRecoverable()
		{
			Factory.RefreshEnabled = false;
			BMSTestHelper.EnableBMSInRegistry();
			CreateSystem("INQ");

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");
			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add and Remove CCC", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(rule.Filter, "Completion Statement", "This one!");

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = BMSTestHelper.CreateJobHeader(enquiry, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Not this one?");
			workflow.AddTag(tagMag);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			enquiry.Validation.ValidateAll();
			AssertNoErrors(enquiry);

			Factory.Save();

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedEnquiry = secondFactory.Load<SalesEnquiry>(enquiry.PK);

			using (var form = new SalesEnquiryForm(loadedEnquiry))
			{
				form.ControllerID = ControllerIDs.SalesEnquiry;
				form.Show();

				var workflowTabPage = form.FindAll<ZWorkflowTabPage>().Single();
				((TabControl)workflowTabPage.Parent).SelectTab(workflowTabPage);

				Application.DoEvents();

				var managementTab = (WorkflowManagementTabPage)workflowTabPage.FindAll<ZWorkflowUserControl>().First().MainTabControl.TabPages[0];
				var tagGrid = managementTab.FindAll<TagGrid>().Single();

				AssertEquals(1, tagGrid.List.Count);

				var tagLinkInGrid = (TagLink)tagGrid.List[0];
				tagLinkInGrid.TGL_Magnitude = 7;

				RunTagRules(rule);

				var thirdFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedTagLink = thirdFactory.Load<TagLink>(tagLinkInGrid.PK);
				AssertNull("The tag link should have been deleted by the service task, and yet...", loadedTagLink);
				AssertEquals("The form should not know that the tag link has been deleted yet, and yet...", 1, tagGrid.List.Count);

				UnitTestUserNotification.Instance.AddOKAnswer();
				form.FireSaveButton();

				AssertContains("There should be a concurrency warning shown to the user, and yet...", @"The following objects have been deleted:
Tag", UnitTestUserNotification.Instance.LastMessage.Text);

				Application.DoEvents();
				AssertEquals("The tag grid should be updated since the tag link was deleted by the service task, and yet...", 0, tagGrid.List.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.FireSaveButton();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestTagLinkChangedMagnitudeByTagilator_AndDeletedByForm_ShouldBeRecoverable()
		{
			Factory.RefreshEnabled = false;
			BMSTestHelper.EnableBMSInRegistry();
			CreateSystem("INQ");

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");
			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add and Remove CCC", TagRuleActionTypeList.Codes.MaintainMagnitude, 7);
			FilterStripsTestHelper.AddStartsWithFilter(rule.Filter, "Completion Statement", "This one!");

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = BMSTestHelper.CreateJobHeader(enquiry, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "This one!");
			var tagLink = workflow.AddTag(tagMag).Link;
			tagLink.TGL_Magnitude = 1;
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			enquiry.Validation.ValidateAll();
			AssertNoErrors(enquiry);

			Factory.Save();

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedEnquiry = secondFactory.Load<SalesEnquiry>(enquiry.PK);

			using (var form = new SalesEnquiryForm(loadedEnquiry))
			{
				form.ControllerID = ControllerIDs.SalesEnquiry;
				form.Show();

				var workflowTabPage = form.FindAll<ZWorkflowTabPage>().Single();
				((TabControl)workflowTabPage.Parent).SelectTab(workflowTabPage);

				Application.DoEvents();

				var managementTab = (WorkflowManagementTabPage)workflowTabPage.FindAll<ZWorkflowUserControl>().First().MainTabControl.TabPages[0];
				var tagGrid = managementTab.FindAll<TagGrid>().Single();

				AssertEquals(1, tagGrid.List.Count);

				var tagLinkInGrid = (TagLink)tagGrid.List[0];
				tagGrid.ListManager.RemoveAt(0);

				RunTagRules(rule);

				var thirdFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedTagLink = thirdFactory.Load<TagLink>(tagLinkInGrid.PK);
				AssertEquals("The tag link should have been updated by the service task, and yet...", 7m, loadedTagLink.TGL_Magnitude);
				AssertEquals(0, tagGrid.List.Count);

				UnitTestUserNotification.Instance.AddOKAnswer();
				form.FireSaveButton();

				AssertEquals("now that TGL_Magnitude is correctly always ignored, there's no concurrency warning shown to the user at all, it's auto-resolved", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				Application.DoEvents();
				AssertEquals("The tag grid should still be empty since the tag link was deleted by the user, and yet...", 0, tagGrid.List.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.FireSaveButton();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestDeleteWorkflow_ShouldNotAccessDeletedBO_OnConcurrencyConflictResolved()
		{
			SetupAndDeleteWorkflow((SalesEnquiryForm form) =>
		   {
			   UnitTestUserNotification.Instance.AddOKAnswer(); //concurrency error occurred, so this is to confirm concurrency resolved
			   AssertNoExceptionThrown(() => form.FireSaveButton());
		   });
		}

		public void TestDeleteWorkflow_ShouldNotAccessDeletedBO_OnValidation()
		{
			SetupAndDeleteWorkflow((SalesEnquiryForm form) =>
		   {
			   try
			   {
				   form.FireSaveButton();
			   }
			   catch (ZSaveConcurrencyException ex)
			   {
				   ZExceptionReporting.HandleZSaveConcurrencyException(ex, new DummyNotificationHandler());
				   form.FireSaveButton();
			   }
		   });
		}

		#region Git Context Menu
		public void TestGitMenuItemIsLinkedToTaskDetailsControl()
		{
			var taskDetailsMenuItemMock = new Mock<ITaskDetailsMenuItem>();
			taskDetailsMenuItemMock.Setup(x => x.AttachToTaskDetailsControl(It.IsAny<ITaskDetailsControl>())).Callback((ITaskDetailsControl control) =>
			{
				CombineAssertions(() =>
				{
					AssertNotNull(control);
					AssertNotNull(control.NotesRichTextBox);
					AssertNotNull(control.TasksGrid);

					Assert("TaskDetailsControl must have a texbox for editing notes where the popup menu is hosted", control.NotesRichTextBox is ZRichTextBox);
					Assert("TaskDetailsControl must have a grid for the tasks", control.TasksGrid is ZGrid);
				});
			});

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			using (var form = new ZOrganisationsForm(job))
			{
				form.Show();

				var workflowTabPage = (ZWorkflowTabPage)form.OrganisationsTabControl.TabPages["WorkflowTabPage"];
				form.OrganisationsTabControl.SelectedTab = workflowTabPage;

				var managementTab = (WorkflowManagementTabPage)workflowTabPage.FindAll<ZWorkflowUserControl>().First().MainTabControl.TabPages[0];
				managementTab.AddTaskDetailsMenuItem(taskDetailsMenuItemMock.Object);

				taskDetailsMenuItemMock.VerifyAll();
			}
		}
		#endregion

		delegate void AssertAction(SalesEnquiryForm form);

		void SetupAndDeleteWorkflow(AssertAction assertAction)
		{
			BMSTestHelper.EnableBMSInRegistry();
			CreateSystem("INQ");

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = BMSTestHelper.CreateJobHeader(enquiry, addDefaultProcessHeaderIfNone: false);

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Test 1";
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Test 2";
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Test 3";
			BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code);

			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			workflow4.FH_CompletionStatement = "Test 4";
			BMSTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code);

			workflow3.GetOrCreateLinkToParent(workflow2);
			workflow4.GetOrCreateLinkToParent(workflow3);
			Factory.Save();

			using (var form = new SalesEnquiryForm(enquiry))
			{
				form.ControllerID = ControllerIDs.SalesEnquiry;
				form.Show();

				var workflowTabPage = form.FindAll<ZWorkflowTabPage>().Single();
				((TabControl)workflowTabPage.Parent).SelectTab(workflowTabPage);

				Application.DoEvents();

				var managementTab = (WorkflowManagementTabPage)workflowTabPage.FindAll<ZWorkflowUserControl>().First().MainTabControl.TabPages[0];

				var workflowsControl = managementTab.Controls.OfType<WorkflowManagementUserControl>().Single();
				workflowsControl.NavigateToWorkflowItem(workflow3);
				AssertEquals(workflow3, workflowsControl.WorkflowsGrid.ListManager.GetCurrent());

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var workflow3InNewFactory = newFactory.Load<ProcessHeader>(workflow3.PK);
				workflow3InNewFactory.Delete();
				newFactory.Save();

				workflow3.FH_CompletionStatement = "Test 3 something";

				assertAction(form);
			}
		}

		#region Implementation

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm(Dummy);
					form.ControllerID = DummyControllerIDs.Dummy;
				}
				return form;
			}
		}
		ZForm form;

		ZTemplateTabControl TabControl
		{
			get
			{
				if (tabControl == null)
				{
					tabControl = new ZTemplateTabControl();
				}
				return tabControl;
			}
		}
		ZTemplateTabControl tabControl;

		WorkflowManagementTabPage ManagementTabPage
		{
			get
			{
				if (managementTabPage == null)
				{
					managementTabPage = new WorkflowManagementTabPage();
				}
				return managementTabPage;
			}
		}
		WorkflowManagementTabPage managementTabPage;

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			TestCaseHelper.ClearTable(BMNCNAttachmentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMNCNShapeSchema.Constants.TableName);
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (form != null)
			{
				form.Dispose();
			}

			if (tabControl != null)
			{
				tabControl.Dispose();
			}

			if (managementTabPage != null)
			{
				managementTabPage.Dispose();
			}
		}

		class DummyNotificationHandler : INotificationHandler
		{
			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null) { }
			public void ReportInformation(string message, string caption) { }
		}

		#endregion
	}

	[TestedType(typeof(ZForm))]
	class WorkflowManagementTabPage_BashingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow 2");

			workflow1.MakePrerequisiteOf(workflow2);

			BMSTestHelper.CreateTask(workflow1);
			BMSTestHelper.CreateTask(workflow2);

			Factory.Save();

			var form = new ZForm
			{
				Size = ControlDpiScalingHelper.NewScaledSize(1200, 1000),
				CaptionRenderingEnabled = true,
				CaptionResourceString = Res.GetData("CE648A4D-49CC-43C6-9AF0-8B27976325C4", "text")
			};

			var tabPage = new WorkflowManagementTabPage
			{
				CaptionResourceString = Res.GetData("46E9A06E-1FE2-4F44-B44B-77DE7CA8B4AA", "text"),
			};
			var tabControl = new ZTabControl { Dock = DockStyle.Fill };

			form.ControllerID = DummyControllerIDs.Dummy;
			form.Controls.Add(tabControl);
			tabControl.TabPages.Add(tabPage);
			tabPage.SetDataBinding(jobHeader.Parent, "");
			form.Show();

			// Remove problematic NetworkUserControl which is a child of the unnamed ZElementHost in the tab control.
			ExposeAllTabPages(form);
			var host = tabControl.FindSingleOrDefault<ZElementHost>();
			host.Child = null;

			Application.DoEvents();

			return form;
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
