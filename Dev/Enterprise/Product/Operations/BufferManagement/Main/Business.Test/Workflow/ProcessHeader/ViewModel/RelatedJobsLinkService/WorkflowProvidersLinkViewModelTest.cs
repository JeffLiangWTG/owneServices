using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkflowProvidersLinkViewModel))]
	class WorkflowProvidersLinkViewModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Manually Created Links

		public void TestManuallyCreatedLinks_ShouldWorkInEitherDirection()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_2");

			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow2_1");
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow2_2");

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(0, viewModel.ProposedProcessHeaderLinks.Count);

			var link1 = viewModel.ProposedProcessHeaderLinks.AddNew();

			link1.FP_FH_HeaderFrom = workflow1_1.PK;
			link1.FP_FH_HeaderTo = workflow2_1.PK;

			var link2 = viewModel.ProposedProcessHeaderLinks.AddNew();

			link2.FP_FH_HeaderFrom = workflow2_2.PK;
			link2.FP_FH_HeaderTo = workflow1_2.PK;

			AssertNoErrors("It should be possible to express links from/to any workflow in the jobs being linked", viewModel);
		}

		public void TestManuallyCreatedLinks_ShouldAccumulateBugFixLog()
		{
			string[] strings = { "flow 1", "flow 2" };
			BMSRegistry.Instance.DebuggingStringsForProcessHeaderLinks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, strings);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow 1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow 1_2");

			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow 2_1");
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow 2_2");

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			var link1 = viewModel.ProposedProcessHeaderLinks.AddNew();

			link1.FP_FH_HeaderFrom = workflow1_1.PK;
			link1.FP_FH_HeaderTo = workflow2_1.PK;

			var link2 = viewModel.ProposedProcessHeaderLinks.AddNew();

			link2.FP_FH_HeaderFrom = workflow2_2.PK;
			link2.FP_FH_HeaderTo = workflow1_2.PK;

			viewModel.CreateRealLinksFromProposed();
			var links = Factory.Load<ProcessHeaderLink>(new ZQuery());

			var pattern1 = "Thread [0-9]+, .*, Header From: " + workflow1_1.PK + ", Header To: " + workflow2_1.PK + ", Link Type: DEP, Description: \\[workflow 1_1\\] will be made a pre-requisite of \\[workflow 2_1\\], Stack Trace:";
			var pattern2 = "Thread [0-9]+, .*, Header From: " + workflow2_2.PK + ", Header To: " + workflow1_2.PK + ", Link Type: DEP, Description: \\[workflow 2_2\\] will be made a pre-requisite of \\[workflow 1_2\\], Stack Trace:";

			var link1Matches = Regex.Match(links[0].DefectFixLog, pattern1).Success || Regex.Match(links[0].DefectFixLog, pattern2).Success;
			var link2Matches = Regex.Match(links[1].DefectFixLog, pattern1).Success || Regex.Match(links[1].DefectFixLog, pattern2).Success;

			Assert(link1Matches && link2Matches);
		}

		#endregion

		#region Templated Links: Simple Relationships

		public void TestTemplatedLinks()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "Stahp");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			var crossTemplateLink = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1, templateWorkflow2);

			crossTemplateLink.FP_TimeDelayFactor = 2.5;
			crossTemplateLink.FP_TimeDelayMinutes = 11;

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Stahp", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(1, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(1, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(1, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(true, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var link = jobHeader1.ProcessHeaders[0].Links.First();

			AssertEquals(2.5m, link.FP_TimeDelayFactor);
			AssertEquals(11, link.FP_TimeDelayMinutes);
		}

		public void TestTemplatedLinks_HeaderLinkSourceTemplates()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var templateFactory = Factory.CreateNewFactory();

			TestConfigsHelper.CreateSchematicTestConfig(templateFactory, new[] { SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, OrgHeaderWorkflowDescriptor.WorkflowTypeCode });

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Template 1");
			var template1Workflow = BMSTestHelper.CreateWorkflow(template1, "Workflow 1");
			BMSTestHelper.CreateTask(template1, template1Workflow);

			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, OrgHeaderWorkflowDescriptor.WorkflowTypeCode, name: "Template 2");
			var template2Workflow = BMSTestHelper.CreateWorkflow(template2, "Workflow 2");
			BMSTestHelper.CreateTask(template2, template2Workflow);

			BMSTestHelper.CreateDependencyLink(template1, template1Workflow, template2Workflow);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			var templateList = new List<IProcessTaskTemplate>();
			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2, templateList);
			AssertEquals("Shouldn't propose link if not included in the HeaderLinkSourceTemplates", 0, viewModel.ProposedProcessHeaderLinks.Count);

			templateList.Add(template1);
			viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2, templateList);
			AssertEquals("Should propose link if included in the HeaderLinkSourceTemplates", 1, viewModel.ProposedProcessHeaderLinks.Count);

			templateList.Remove(template1);
			templateList.Add(template2);
			viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2, templateList);
			AssertEquals("Should propose link if included in the HeaderLinkSourceTemplates", 1, viewModel.ProposedProcessHeaderLinks.Count);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals("Should create link", 1, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals("Should create link", 1, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals("Should create link", true, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));
		}

		public void TestTemplatedLinks_TwoWayRelationships_TwoWaysBetweenTemplates()
		{
			// Templates should be constructed in a new factory since inter-template links don't appear in ABOC when saved on other template.
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1_1 = BMSTestHelper.CreateWorkflow(template1, "Stahp");
			var templateWorkflow1_2 = BMSTestHelper.CreateWorkflow(template1, "That");
			var templateWorkflow2_1 = BMSTestHelper.CreateWorkflow(template2, "Pls");
			var templateWorkflow2_2 = BMSTestHelper.CreateWorkflow(template2, "Kthxbye");

			var internalTemplateLink1 = BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1_1, templateWorkflow1_2);
			var internalTemplateLink2 = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow2_1, templateWorkflow2_2);
			var crossTemplateLink1 = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1_1, templateWorkflow2_1);
			var crossTemplateLink2 = BMSTestHelper.CreateDependencyLink(template1, templateWorkflow2_2, templateWorkflow1_2);

			/*
			 * Template1: 1 -> 2
			 *            |    ^
			 *            v    |
			 * Template2: 1 -> 2
			 * 
			 * */

			BMSTestHelper.CreateTask(template1, templateWorkflow1_1, string.Empty);
			BMSTestHelper.CreateTask(template1, templateWorkflow1_2, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2_1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2_2, string.Empty);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(2, jobHeader1.ProcessHeaders.Count);
			AssertEquals(2, jobHeader2.ProcessHeaders.Count);

			var workflow1_1 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Stahp");
			var workflow1_2 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "That");

			var workflow2_1 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Pls");
			var workflow2_2 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Kthxbye");

			AssertEquals("Stahp", workflow1_1.FH_CompletionStatement);
			AssertEquals("That", workflow1_2.FH_CompletionStatement);
			AssertEquals("Pls", workflow2_1.FH_CompletionStatement);
			AssertEquals("Kthxbye", workflow2_2.FH_CompletionStatement);

			AssertEquals(true, workflow1_1.IsPrerequisiteOf(workflow1_2));
			AssertEquals(true, workflow2_1.IsPrerequisiteOf(workflow2_2));
			AssertEquals(false, workflow1_1.IsPrerequisiteOf(workflow2_1));
			AssertEquals(false, workflow2_2.IsPrerequisiteOf(workflow1_2));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(2, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(true, workflow1_1.IsPrerequisiteOf(workflow1_2));
			AssertEquals(true, workflow2_1.IsPrerequisiteOf(workflow2_2));
			AssertEquals(true, workflow1_1.IsPrerequisiteOf(workflow2_1));
			AssertEquals(true, workflow2_2.IsPrerequisiteOf(workflow1_2));
		}

		public void TestTemplatedLinks_TwoWayRelationships_FromOneTemplateToSecond()
		{
			// Templates should be constructed in a new factory since inter-template links don't appear in ABOC when saved on other template.
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1_1 = BMSTestHelper.CreateWorkflow(template1, "Stahp");
			var templateWorkflow1_2 = BMSTestHelper.CreateWorkflow(template1, "That");
			var templateWorkflow2_1 = BMSTestHelper.CreateWorkflow(template2, "Pls");
			var templateWorkflow2_2 = BMSTestHelper.CreateWorkflow(template2, "Kthxbye");

			var internalTemplateLink1 = BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1_1, templateWorkflow1_2);
			var internalTemplateLink2 = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow2_1, templateWorkflow2_2);
			var crossTemplateLink1 = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1_1, templateWorkflow2_1);
			var crossTemplateLink2 = BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1_2, templateWorkflow2_2);

			/*
			 * Template1: 1 -> 2 
			 *            |    |
			 *            v    v
			 * Template2: 1 -> 2
			 * 
			 * */

			BMSTestHelper.CreateTask(template1, templateWorkflow1_1, string.Empty);
			BMSTestHelper.CreateTask(template1, templateWorkflow1_2, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2_1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2_2, string.Empty);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(2, jobHeader1.ProcessHeaders.Count);
			AssertEquals(2, jobHeader2.ProcessHeaders.Count);

			var workflow1_1 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Stahp");
			var workflow1_2 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "That");

			var workflow2_1 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Pls");
			var workflow2_2 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Kthxbye");

			AssertEquals(true, workflow1_1.IsPrerequisiteOf(workflow1_2));
			AssertEquals(true, workflow2_1.IsPrerequisiteOf(workflow2_2));
			AssertEquals(false, workflow1_1.IsPrerequisiteOf(workflow2_1));
			AssertEquals(false, workflow1_2.IsPrerequisiteOf(workflow2_2));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(2, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(true, workflow1_1.IsPrerequisiteOf(workflow1_2));
			AssertEquals(true, workflow2_1.IsPrerequisiteOf(workflow2_2));
			AssertEquals(true, workflow1_1.IsPrerequisiteOf(workflow2_1));
			AssertEquals(true, workflow1_2.IsPrerequisiteOf(workflow2_2));
		}

		public void TestTemplatedLinks_ParentChildRelationship()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "Stahp");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			var crossTemplateLink = BMSTestHelper.CreateParentChildLink(template2, templateWorkflow1, templateWorkflow2);

			crossTemplateLink.FP_SynchroniseBufferPenetration = true;

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Stahp", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsChildOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(1, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(1, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(1, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(true, jobHeader1.ProcessHeaders[0].IsChildOf(jobHeader2.ProcessHeaders[0]));

			var link = jobHeader1.ProcessHeaders[0].Links.First();

			AssertEquals(true, link.FP_SynchroniseBufferPenetration);
		}

		#endregion

		#region Templated Links: Edge Case Handling

		public void TestTemplatedLinks_WhenWorkflowHasBeenRenamed()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "Stahp");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			var crossTemplateLink = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1, templateWorkflow2);

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Stahp", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			jobHeader1.ProcessHeaders[0].FH_CompletionStatement = "Stahp it";

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(0, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));
		}

		public void TestTemplatedLinks_WhenJobLevelWorkflowHasBeenRenamed()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateJobHeader1 = template1.GetJobHeader();
			var templateJobHeader2 = template2.GetJobHeader();
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "Stahp");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			var crossTemplateLink = BMSTestHelper.CreateDependencyLink(template2, templateJobHeader1, templateWorkflow2);

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			jobHeader1.FH_CompletionStatement = "I think I'm going to call him Stampy";

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Stahp", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			AssertEquals(0, jobHeader1.Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(1, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(1, jobHeader1.Links.Count());
			AssertEquals(1, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals("There can only be one job-level workflow, so match based on that rather than by name", true, jobHeader1.IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));
		}

		#endregion

		#region Templated Links: Template Fallbacks

		public void TestTemplatedLinks_WithTemplateFallbacks_ShouldApplyLinkFromLessSpecificTemplate()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1_1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template1_2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			template1_1.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var templateWorkflow1_1 = BMSTestHelper.CreateWorkflow(template1_1, "Stahp");
			var templateWorkflow1_2 = BMSTestHelper.CreateWorkflow(template1_2, "Stahp");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			BMSTestHelper.CreateTask(template1_1, templateWorkflow1_1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			// Cross-template link is on the less-specific template. There won't be anything on the job that refers to this template, so it'll have to find currently-applicable templates.
			var crossTemplateLink = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1_2, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Stahp", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(1, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(1, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(1, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(true, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));
		}

		public void TestTemplatedLinks_WithTemplateFallbacks_FallBackIfEmpty_WhenConditionsExcludeTask_ShouldApplyLinkFromLessSpecificTemplate()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1_1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template1_2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1_1 = BMSTestHelper.CreateWorkflow(template1_1, "Stahp");
			var templateWorkflow1_2 = BMSTestHelper.CreateWorkflow(template1_2, "Stahp");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			var taskToRuleOutWithCondition = BMSTestHelper.CreateTask(template1_1, templateWorkflow1_1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			taskToRuleOutWithCondition.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			taskToRuleOutWithCondition.TemplateConditions.TemplateCondition2Value = "\"<P9_Description>\" == \"Sherson\"";

			// Cross-template link is on the less-specific template. There won't be anything on the job that refers to this template, so it'll have to find currently-applicable templates.
			var crossTemplateLink = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1_2, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Stahp", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals("Even though there is a task on the most-specific template, and we have FBE fallback mode, that templated task is ruled out by its condition", 1, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(1, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(1, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(true, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));
		}

		public void TestTemplatedLinks_WithTemplateFallbacks_FallBackIfEmpty_WhenConditionsExcludeOnlyOneTask_ShouldNotApplyLinkFromLessSpecificTemplate()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1_1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template1_2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1_1 = BMSTestHelper.CreateWorkflow(template1_1, "Stahp");
			var templateWorkflow1_2 = BMSTestHelper.CreateWorkflow(template1_2, "Stahp");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			var taskToRuleOutWithCondition = BMSTestHelper.CreateTask(template1_1, templateWorkflow1_1, string.Empty);
			BMSTestHelper.CreateTask(template1_2, templateWorkflow1_2, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			taskToRuleOutWithCondition.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			taskToRuleOutWithCondition.TemplateConditions.TemplateCondition2Value = "\"<P9_Description>\" == \"Sherson\"";

			// Cross-template link is on the less-specific template. There won't be anything on the job that refers to this template, so it'll have to find currently-applicable templates.
			var crossTemplateLink = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1_2, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Stahp", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals("There is a task without a condition on the most-specific template, so the condition not matching isn't enough to prevent falling back to less-specific template", 1, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(1, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(1, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(true, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));
		}

		public void TestTemplatedLinks_WithTemplateFallbacks_WhenFallbackNotSet_ShouldNotApplyLink()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1_1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template1_2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1_1 = BMSTestHelper.CreateWorkflow(template1_1, "Stahp");
			var templateWorkflow1_2 = BMSTestHelper.CreateWorkflow(template1_2, "Stahp");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			BMSTestHelper.CreateTask(template1_1, templateWorkflow1_1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			// Cross-template link is on the less-specific template. Since the more specific template is set to
			// not fall back (default value of P0_TaskFallbackMethod is FBE), this link should not be applied.
			var crossTemplateLink = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1_2, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Stahp", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(0, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));
		}

		public void TestTemplatedLinks_WithTemplateFallbacks_WhenFallbackDisallowed_ShouldNotApplyLink()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1_1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template1_2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			template1_1.P0_TaskFallbackMethod = FallbackTypeList.Codes.NeverFallback;

			var templateWorkflow1_1 = BMSTestHelper.CreateWorkflow(template1_1, "Job Workflow");
			var templateWorkflow1_2 = BMSTestHelper.CreateWorkflow(template1_2, "Job Workflow");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			// Task is on less-specific template, so default fallback rules would mean it (and the external link) would be applied. With fallback mode = NBF it won't get applied.
			BMSTestHelper.CreateTask(template1_2, templateWorkflow1_1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			// Cross-template link is on the less-specific template. Since the more specific template is set to
			// not fall back, this link should not be applied.
			var crossTemplateLink = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1_2, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Job Workflow", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals(0, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));
		}

		public void TestTemplatedLinks_WithTemplateFallbacks_WhenRelationshipExistsAtMultipleLevels()
		{
			var templateFactory = Factory.CreateNewFactory();

			var config = TestConfigsHelper.CreateSchematicTestConfig(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var template1_1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template1_2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1_1 = BMSTestHelper.CreateWorkflow(template1_1, "Stahp");
			var templateWorkflow1_2 = BMSTestHelper.CreateWorkflow(template1_2, "Stahp");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Pls");

			BMSTestHelper.CreateTask(template1_1, templateWorkflow1_1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			// Cross-template link is on both versions of the same workflow.
			var crossTemplateLink1 = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1_1, templateWorkflow2);
			var crossTemplateLink2 = BMSTestHelper.CreateDependencyLink(template2, templateWorkflow1_2, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			AssertEquals(1, jobHeader1.ProcessHeaders.Count);
			AssertEquals(1, jobHeader2.ProcessHeaders.Count);

			AssertEquals("Stahp", jobHeader1.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals("Pls", jobHeader2.ProcessHeaders[0].FH_CompletionStatement);

			AssertEquals(0, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(0, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(false, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));

			var viewModel = new WorkflowProvidersLinkViewModel(jobHeader1, jobHeader2);

			AssertEquals("Shouldn't duplicate the same link as that would be redundant", 1, viewModel.ProposedProcessHeaderLinks.Count);
			AssertNoErrors(viewModel);

			viewModel.CreateRealLinksFromProposed();

			AssertEquals(1, jobHeader1.ProcessHeaders[0].Links.Count());
			AssertEquals(1, jobHeader2.ProcessHeaders[0].Links.Count());
			AssertEquals(true, jobHeader1.ProcessHeaders[0].IsPrerequisiteOf(jobHeader2.ProcessHeaders[0]));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkflowProvidersLinkViewModel(
				BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false),
				BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false)
				);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
