using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowProvidersLinkageServiceTest : NonTransactionedTestCase
	{
		#region WorkflowProvidersLinked

		public void TestWorkflowProvidersLinked_WhenNoInterTemplateLinksExist()
		{
			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "I might be wrong");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "I could have sworn I saw a light coming on");

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			job1.O1_EnquiryType = "AAA";

			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			var workflow1 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I might be wrong");
			var workflow2 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I could have sworn I saw a light coming on");

			AssertEquals(false, workflow1.IsPrerequisiteOf(workflow2));

			WorkflowProvidersLinked(job2, job1, control =>
			{
				Fail("Should not show form when no links are templated");
			}, shouldShowForm: false);

			AssertEquals(false, workflow1.IsPrerequisiteOf(workflow2));
		}

		public void TestWorkflowProvidersLinked_WhenInterTemplateLinksExist()
		{
			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "I might be wrong");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "I could have sworn I saw a light coming on");

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			var interTemplateLink = BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			job1.O1_EnquiryType = "AAA";

			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			var workflow1 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I might be wrong");
			var workflow2 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I could have sworn I saw a light coming on");

			AssertEquals(false, workflow1.IsPrerequisiteOf(workflow2));

			WorkflowProvidersLinked(job2, job1, control =>
			{
				control.CreateLinkButton.PerformClick();

				AssertEquals(DialogResult.OK, control.DialogResult);
			});

			AssertEquals(true, workflow1.IsPrerequisiteOf(workflow2));
		}

		public void TestWorkflowProvidersLinked_WhenNotUserInteractive_ShouldNotDisplayForm()
		{
			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "I might be wrong");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "I could have sworn I saw a light coming on");

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			var interTemplateLink = BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			job1.O1_EnquiryType = "AAA";

			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			var workflow1 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I might be wrong");
			var workflow2 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I could have sworn I saw a light coming on");

			AssertEquals(false, workflow1.IsPrerequisiteOf(workflow2));

			var controlShown = false;

			Globals.IsUserInteractive = false;

			try
			{
				WorkflowProvidersLinked(job2, job1, control =>
				{
					controlShown = true;
				});

				AssertEquals(true, workflow1.IsPrerequisiteOf(workflow2));
				AssertEquals(false, controlShown);
			}
			finally
			{
				Globals.IsUserInteractive = true;
			}
		}

		public void TestWorkflowProvidersLinked_WhenWorkflowAppliedViaPartialTemplate()
		{
			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");
			var partialTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, isPartial: true);

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "I might be wrong");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "I could have sworn I saw a light coming on");
			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(partialTemplate, "Think about the good times");

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty, description: "One one two");
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty, description: "What would I do");
			BMSTestHelper.CreateTask(partialTemplate, templateWorkflow3, string.Empty, description: "Let's go down the waterfall");

			var interTemplateLink = BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1, templateWorkflow3);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			job1.O1_EnquiryType = "AAA";

			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			var workflow1 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I might be wrong");
			var workflow2_1 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I could have sworn I saw a light coming on");

			AssertEquals(1, job1.WorkflowItems.Tasks.Count);
			AssertEquals(1, job2.WorkflowItems.Tasks.Count);
			AssertEquals("One one two", job1.WorkflowItems[0].P9_Description);
			AssertEquals("What would I do", job2.WorkflowItems[0].P9_Description);

			job2.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { partialTemplate }));

			AssertEquals(2, job2.WorkflowItems.Tasks.Count);
			AssertEquals("Let's go down the waterfall", job2.WorkflowItems[1].P9_Description);

			var workflow2_2 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Think about the good times");

			BMSTestCaseWithFactory.AssertIsNotPrerequisite(workflow1, workflow2_2);

			Factory.Save();

			WorkflowProvidersLinked(job1, job2, control =>
			{
				control.CreateLinkButton.PerformClick();

				AssertEquals(DialogResult.OK, control.DialogResult);
			});

			BMSTestCaseWithFactory.AssertIsPrerequisite(workflow1, workflow2_2);
		}

		public void TestWorkflowProvidersLinked_WhenWorkflowTemplateAppliedManually()
		{
			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");
			var nonPartialTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "CCC");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "I might be wrong");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "I could have sworn I saw a light coming on");
			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(nonPartialTemplate, "Think about the good times");

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty, description: "One one two");
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty, description: "What would I do");
			BMSTestHelper.CreateTask(nonPartialTemplate, templateWorkflow3, string.Empty, description: "Let's go down the waterfall");

			var interTemplateLink = BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1, templateWorkflow3);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			job1.O1_EnquiryType = "AAA";

			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			var workflow1 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I might be wrong");
			var workflow2_1 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I could have sworn I saw a light coming on");

			AssertEquals(1, job1.WorkflowItems.Tasks.Count);
			AssertEquals(1, job2.WorkflowItems.Tasks.Count);
			AssertEquals("One one two", job1.WorkflowItems[0].P9_Description);
			AssertEquals("What would I do", job2.WorkflowItems[0].P9_Description);

			job2.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { nonPartialTemplate }));

			AssertEquals(2, job2.WorkflowItems.Tasks.Count);
			AssertEquals("Let's go down the waterfall", job2.WorkflowItems[1].P9_Description);

			var workflow2_2 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Think about the good times");

			BMSTestCaseWithFactory.AssertIsNotPrerequisite(workflow1, workflow2_2);

			Factory.Save();

			WorkflowProvidersLinked(job1, job2, control =>
			{
				control.CreateLinkButton.PerformClick();

				AssertEquals(DialogResult.OK, control.DialogResult);
			});

			BMSTestCaseWithFactory.AssertIsPrerequisite(workflow1, workflow2_2);
		}

		public void TestLink_WhenFromProviderIsDeleted_StillRemovesLink()
		{
			var (jobToName, workflowToName, jobFromName, workflowFromName) = MakeTemplateWithTemplateLink();

			var jobFrom = Factory.NewWithValidTestData<SalesEnquiry>();
			jobFrom.O1_EnquiryType = jobToName;

			var jobTo = Factory.NewWithValidTestData<SalesEnquiry>();
			jobTo.O1_EnquiryType = jobFromName;

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			jobFrom.Delete();

			AssertNoExceptionThrown("We can do this without throwing up lots of bits and bytes and such",
				() => ((IWorkflowProvidersLinkageService)new WorkflowProvidersLinkageService()).WorkflowProvidersLinked(jobFrom, jobTo, Factory));
			AssertNull(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("We found zero links for the job we deleted", 0, LoadLinksForJob<OrgColdCallRegister>(OrgColdCallRegisterSchema.PK, jobTo.PK, true).Count());
		}

		public void TestLink_WhenToProviderIsDeleted_StillRemovesLink()
		{
			var (jobToName, workflowToName, jobFromName, workflowFromName) = MakeTemplateWithTemplateLink();

			var jobFrom = Factory.NewWithValidTestData<SalesEnquiry>();
			jobFrom.O1_EnquiryType = jobToName;

			var jobTo = Factory.NewWithValidTestData<SalesEnquiry>();
			jobTo.O1_EnquiryType = jobFromName;

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			jobTo.Delete();

			AssertNoExceptionThrown("We can do this without throwing up lots of bits and bytes and such",
				() => ((IWorkflowProvidersLinkageService)new WorkflowProvidersLinkageService()).WorkflowProvidersLinked(jobFrom, jobTo, Factory));
			AssertNull(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("We found zero links for the job we deleted", 0, LoadLinksForJob<OrgColdCallRegister>(OrgColdCallRegisterSchema.PK, jobFrom.PK, false).Count());
		}

		#endregion

		#region WorkflowLinkedTemplatesApplied

		public void TestWorkflowLinkedTemplatesApplied()
		{
			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, "BBB");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "I might be wrong");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "I could have sworn I saw a light coming on");

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			var interTemplateLink = BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			job1.O1_EnquiryType = "AAA";

			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();
			job2.O1_EnquiryType = "BBB";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			var workflow1 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I might be wrong");
			var workflow2 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "I could have sworn I saw a light coming on");

			AssertEquals(false, workflow1.IsPrerequisiteOf(workflow2));
			var templateList = new List<IProcessTaskTemplate>();
			((IWorkflowProvidersLinkageService)service).WorkflowLinkedTemplatesApplied(job2, job1, Factory, templateList);

			AssertEquals("Should not link workflows since templatesToApply does not include the template", false, workflow1.IsPrerequisiteOf(workflow2));

			templateList.Add(template1);
			((IWorkflowProvidersLinkageService)service).WorkflowLinkedTemplatesApplied(job2, job1, Factory, templateList);
			AssertEquals(true, workflow1.IsPrerequisiteOf(workflow2));
		}

		#endregion

		#region WorkflowProvidersUnLinked

		public void TestWorkflowProvidersUnLinked_NoExistingLinks()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIFRISTORG";
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAISCNDORG";
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			WorkflowProvidersUnLinked(job1, job2);

			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestWorkflowProvidersUnLinked_JobToWorkflow()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIFRISTORG";
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAISCNDORG";
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			jobHeader1.MakePrerequisiteOf(workflow2);

			WorkflowProvidersUnLinked(job1, job2);

			AssertMultilineASCIIEquals("", @"Would you like to remove the DEP workflow relationship between these two jobs?

Job 1: Organization (MAIFRISTORG) - Job Organization (MAIFRISTORG) is complete.
Job 2: Organization (MAISCNDORG) - workflow2", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWorkflowProvidersUnLinked_WhenNotUserInteractive_ShouldNotShowDialog()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "And yet...");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Dave the ruiner strikes again!!!1");

			jobHeader1.MakePrerequisiteOf(workflow2);
			AssertEquals(true, jobHeader1.IsPrerequisiteOf(workflow2));

			Globals.IsUserInteractive = false;
			try
			{
				WorkflowProvidersUnLinked(jobHeader1.Parent, jobHeader2.Parent);
			}
			finally
			{
				Globals.IsUserInteractive = true;
			}

			AssertEquals(false, jobHeader1.IsPrerequisiteOf(workflow2));
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWorkflowProvidersUnLinked_WorkflowToJob()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIFRISTORG";
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAISCNDORG";
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			workflow1.MakePrerequisiteOf(jobHeader2);

			WorkflowProvidersUnLinked(job1, job2);

			AssertMultilineASCIIEquals("", @"Would you like to remove the DEP workflow relationship between these two jobs?

Job 1: Organization (MAIFRISTORG) - workflow1
Job 2: Organization (MAISCNDORG) - Job Organization (MAISCNDORG) is complete.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWorkflowProvidersUnLinked_WorkflowToWorkflow()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIFRISTORG";
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAISCNDORG";
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			workflow1.MakePrerequisiteOf(workflow2);

			WorkflowProvidersUnLinked(job1, job2);

			AssertMultilineASCIIEquals("", @"Would you like to remove the DEP workflow relationship between these two jobs?

Job 1: Organization (MAIFRISTORG) - workflow1
Job 2: Organization (MAISCNDORG) - workflow2", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWorkflowProvidersUnLinked_WorkflowToWorkflowFromAnotherJob()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIFRISTORG";
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAISCNDORG";
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			var job3 = Factory.NewWithValidTestData<OrgHeader>();
			job3.OH_Code = "MAITHRDORG";
			var jobHeader3 = ProcessJobHeader.GetForParent(job3, Factory);
			var workflow3 = jobHeader3.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";

			workflow1.MakePrerequisiteOf(workflow3);

			WorkflowProvidersUnLinked(job1, job2);

			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestWorkflowProvidersUnLinked_MultipleLinks()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIFRISTORG";
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAISCNDORG";
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			workflow1.MakePrerequisiteOf(jobHeader2);
			workflow1.MakePrerequisiteOf(workflow2);
			jobHeader1.MakePrerequisiteOf(jobHeader2);
			jobHeader1.MakePrerequisiteOf(workflow2);

			WorkflowProvidersUnLinked(job1, job2);

			AssertEquals(0, workflow1.LinksFromMeToOthers.Count());
			AssertEquals(0, jobHeader1.LinksFromMeToOthers.Count());

			AssertMultilineASCIIEquals("", @"There are multiple links between these two jobs. Would you like to remove all of them?

Job 1: Organization (MAIFRISTORG)
Job 2: Organization (MAISCNDORG)", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestUnlink_WhenFromProviderIsDeleted_StillRemovesLink()
		{
			var (jobToName, workflowToName, jobFromName, workflowFromName) = MakeTemplateWithTemplateLink();

			var jobFrom = Factory.NewWithValidTestData<SalesEnquiry>();
			jobFrom.O1_EnquiryType = jobToName;

			var jobTo = Factory.NewWithValidTestData<SalesEnquiry>();
			jobTo.O1_EnquiryType = jobFromName;

			Factory.Save();
			((IWorkflowProvidersLinkageService)new WorkflowProvidersLinkageService()).WorkflowProvidersLinked(jobFrom, jobTo, Factory);
			Factory.Save();

			AssertEquals("We made a single link for this job pair", 1, LoadLinksForJob<OrgColdCallRegister>(OrgColdCallRegisterSchema.PK, jobTo.PK, true).Count());

			UnitTestUserNotification.Instance.ClearMessages();
			jobFrom.Delete();

			AssertNoExceptionThrown("We can do this without throwing up lots of bits and bytes and such",
				() => ((IWorkflowProvidersLinkageService)new WorkflowProvidersLinkageService()).WorkflowProvidersUnLinked(jobFrom, jobTo, Factory));
			AssertNull(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("We found zero links for the job we deleted", 0, LoadLinksForJob<OrgColdCallRegister>(OrgColdCallRegisterSchema.PK, jobTo.PK, true).Count());
		}

		public void TestUnlink_WhenToProviderIsDeleted_StillRemovesLink()
		{
			var (jobToName, workflowToName, jobFromName, workflowFromName) = MakeTemplateWithTemplateLink();

			var jobFrom = Factory.NewWithValidTestData<SalesEnquiry>();
			jobFrom.O1_EnquiryType = jobToName;

			var jobTo = Factory.NewWithValidTestData<SalesEnquiry>();
			jobTo.O1_EnquiryType = jobFromName;

			Factory.Save();
			((IWorkflowProvidersLinkageService)new WorkflowProvidersLinkageService()).WorkflowProvidersLinked(jobFrom, jobTo, Factory);
			Factory.Save();

			AssertEquals("We made a single link for this job pair", 1, LoadLinksForJob<OrgColdCallRegister>(OrgColdCallRegisterSchema.PK, jobFrom.PK, false).Count());

			UnitTestUserNotification.Instance.ClearMessages();
			jobTo.Delete();

			AssertNoExceptionThrown("We can do this without throwing up lots of bits and bytes and such",
				() => ((IWorkflowProvidersLinkageService)new WorkflowProvidersLinkageService()).WorkflowProvidersUnLinked(jobFrom, jobTo, Factory));
			AssertNull(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("We found zero links for the job we deleted", 0, LoadLinksForJob<OrgColdCallRegister>(OrgColdCallRegisterSchema.PK, jobFrom.PK, false).Count());
		}

		#endregion

		#region Defaul Dialog Options

		public void TestDialogContext_ForLinkCreation()
		{
			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, OrgHeaderWorkflowDescriptor.WorkflowTypeCode);

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "Sales Enquiry Workflow");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Org Header Workflow");

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			((IWorkflowProvidersLinkageService)service).WorkflowProvidersLinked(job1, job2, Factory);

			var context = UnitTestUserNotification.Instance.LastMessage?.Context;
			AssertNotNull(context);
			AssertEquals("Should use the proper dialog identifier separate from the Remove Workflow Links dialog identifier", new ZGuid("81909AE5-58F7-4E57-BA0B-29465080D16E"), context.DialogIdentifier);
			AssertEquals("Should include jobs types in dialog defaults caption", "Create Workflow Links INQ->ORG", context.Caption);
			AssertEquals("Should allow applying to similar dialogs", "Apply to creation of all links between workflows in different jobs regardless job types", context.NullContextDescription.Caption);
			AssertEquals("Should not allow closing the dialog if cancelling is not allowed at the CMP or GLB levels", true, context.ForceOverriddenDefaults);
		}

		public void TestDialogContext_ForLinkDeletion()
		{
			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, OrgHeaderWorkflowDescriptor.WorkflowTypeCode);

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "Sales Enquiry Workflow");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Org Header Workflow");

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1, templateWorkflow2);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			var workflow1 = jobHeader1.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Sales Enquiry Workflow");
			var workflow2 = jobHeader2.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Org Header Workflow");

			workflow1.MakePrerequisiteOf(workflow2);

			Factory.Save();

			((IWorkflowProvidersLinkageService)service).WorkflowProvidersUnLinked(job1, job2, Factory);

			var context = UnitTestUserNotification.Instance.LastMessage?.Context;
			AssertNotNull(context);
			AssertEquals("Should use the proper dialog identifier separate from the Create Workflow Links dialog identifier", new ZGuid("E2358AEC-89AC-4709-B9DB-51FE490713AB"), context.DialogIdentifier);
			AssertEquals("Should include jobs types in dialog defaults caption", "Remove Workflow Links INQ->ORG", context.Caption);
			AssertEquals("Should allow applying to similar dialogs", "Apply to removal of all links", context.NullContextDescription.Caption);
			AssertEquals("Should not allow closing the dialog if cancelling is not allowed at the CMP or GLB levels", true, context.ForceOverriddenDefaults);
		}

		#endregion

		#region Validation

		public void TestCreateLinks_WhenValidationErrorsExist()
		{
			CreateTemplatesWithJobToJobLink(OrgHeaderWorkflowDescriptor.WorkflowTypeCode, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);

			WorkflowProvidersLinked(jobHeader1.Parent, jobHeader2.Parent, control =>
			{
				var link = control.ViewModel.ProposedProcessHeaderLinks[0];

				link.FP_LinkType = "ZZZ";

				control.CreateLinkButton.PerformClick();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.None, control.DialogResult);
				AssertEquals(false, jobHeader1.IsPrerequisiteOf(jobHeader2));

				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

				control.CreateLinkButton.PerformClick();

				AssertEquals(DialogResult.OK, control.DialogResult);
			});

			AssertEquals("Closing form shouldn't save the factory - the calling form still needs to validate created links", false, jobHeader1.IsInDatabase);
			AssertEquals("Closing form shouldn't save the factory - the calling form still needs to validate created links", false, jobHeader2.IsInDatabase);

			AssertEquals(true, jobHeader1.IsPrerequisiteOf(jobHeader2));
		}

		public void TestCreateLinks_WhenLinkAlreadyExists_DependencyLink()
		{
			CreateTemplatesWithJobToJobLink(OrgHeaderWorkflowDescriptor.WorkflowTypeCode, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);

			jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			AssertEquals(true, jobHeader1.IsPrerequisiteOf(jobHeader2));

			WorkflowProvidersLinked(jobHeader1.Parent, jobHeader2.Parent, control =>
			{
				Fail("Should not show form when the link already exist");
			}, shouldShowForm: false);

			AssertEquals("Attempt to link shouldn't save the factory - the calling form still needs to validate created links", false, jobHeader1.IsInDatabase);
			AssertEquals("Attempt to link shouldn't save the factory - the calling form still needs to validate created links", false, jobHeader2.IsInDatabase);

			AssertEquals(true, jobHeader1.IsPrerequisiteOf(jobHeader2));
		}

		public void TestCreateLinks_WhenLinkAlreadyExists_ParentChildLink()
		{
			CreateTemplatesWithJobToJobLink(OrgHeaderWorkflowDescriptor.WorkflowTypeCode, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);

			jobHeader1.GetOrCreateLinkToParent(jobHeader2);

			AssertEquals(true, jobHeader1.IsChildOf(jobHeader2));

			WorkflowProvidersLinked(jobHeader1.Parent, jobHeader2.Parent, control =>
			{
				Fail("Should not show form when the link already exist");
			}, shouldShowForm: false);

			AssertEquals("Closing form shouldn't save the factory - the calling form still needs to validate created links", false, jobHeader1.IsInDatabase);
			AssertEquals("Closing form shouldn't save the factory - the calling form still needs to validate created links", false, jobHeader2.IsInDatabase);

			AssertEquals(true, jobHeader1.IsChildOf(jobHeader2));
		}

		public void TestCreateLinks_WhenLinksCreatedWouldIntroduceCircularRelationships_DependencyLink()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CreateTemplatesWithJobToJobLink(OrgHeaderWorkflowDescriptor.WorkflowTypeCode, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);

			var link = jobHeader2.GetOrCreateDependencyLink(jobHeader1);

			WorkflowProvidersLinked(jobHeader1.Parent, jobHeader2.Parent, control =>
			{
				AssertEquals(true, jobHeader2.IsPrerequisiteOf(jobHeader1));
				AssertEquals(false, jobHeader1.IsPrerequisiteOf(jobHeader2));

				control.CreateLinkButton.PerformClick();

				AssertEquals(DialogResult.OK, control.DialogResult);
			});

			AssertEquals(true, jobHeader2.IsPrerequisiteOf(jobHeader1));
			AssertEquals(true, jobHeader1.IsPrerequisiteOf(jobHeader2));

			var newLink = jobHeader1.PostrequisiteLinks.Single(l => l.FP_FH_HeaderTo == jobHeader2.PK);
			var message = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Inquiry - Job Inquiry is complete.
Inquiry - Job Workflow
Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.
Organization (XVBQP68SIYXQ) - Job Workflow";

			AssertNoErrors(newLink);

			link.RunPreSaveValidation(); // In production we don't run pre save validation after creating intra-job links and don't check for validation errors. This check is added here for completeness.
			AssertHasError(link.FP_FH_HeaderToInfo, message);

			newLink.Delete();

			link.RunPreSaveValidation();
			AssertNoErrors(link);
		}

		public void TestCreateLinks_WhenLinksCreatedWouldIntroduceCircularRelationships_ParentChildLinks()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CreateTemplatesWithJobToJobLink(OrgHeaderWorkflowDescriptor.WorkflowTypeCode, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);

			var link = jobHeader2.GetOrCreateLinkToParent(jobHeader1);

			WorkflowProvidersLinked(jobHeader1.Parent, jobHeader2.Parent, control =>
			{
				AssertEquals(true, jobHeader2.IsChildOf(jobHeader1));
				AssertEquals(false, jobHeader1.IsChildOf(jobHeader2));

				var proposedLink = (ProposedProcessHeaderLink)control.ViewModel.ProposedProcessHeaderLinks.Single();

				proposedLink.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

				control.CreateLinkButton.PerformClick();

				AssertEquals(DialogResult.OK, control.DialogResult);
			});

			AssertEquals(true, jobHeader2.IsChildOf(jobHeader1));
			AssertEquals(true, jobHeader1.IsChildOf(jobHeader2));

			var newLink = jobHeader1.ParentLinks.Single(l => l.FP_FH_HeaderTo == jobHeader2.PK);

			var message = @"This link is part of a looped Parent-Child relationship. The following workflows are involved in a loop:
Inquiry - Job Inquiry is complete.
Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.";

			AssertNoErrors(newLink);

			link.RunPreSaveValidation(); // In production we don't run pre save validation after creating intra-job links and don't check for validation errors. This check is added here for completeness.
			AssertHasError(link.FP_FH_HeaderToInfo, message);

			newLink.Delete();

			link.RunPreSaveValidation();
			AssertNoErrors(link);
		}

		public void TestCreateLinks_WhenLinksCreatedWouldIntroduceCircularRelationships_MixOfDependencyAndParentChildLinks()
		{
			CreateTemplatesWithJobToJobLink(OrgHeaderWorkflowDescriptor.WorkflowTypeCode, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);

			var link = jobHeader2.GetOrCreateLinkToParent(jobHeader1);

			WorkflowProvidersLinked(jobHeader1.Parent, jobHeader2.Parent, control =>
			{
				AssertEquals(true, jobHeader2.IsChildOf(jobHeader1));
				AssertEquals(false, jobHeader1.IsPrerequisiteOf(jobHeader2));

				control.CreateLinkButton.PerformClick();

				AssertEquals(DialogResult.OK, control.DialogResult);
			});

			AssertEquals(true, jobHeader2.IsChildOf(jobHeader1));
			AssertEquals(true, jobHeader1.IsPrerequisiteOf(jobHeader2));

			var newLink = jobHeader1.PostrequisiteLinks.Single(l => l.FP_FH_HeaderTo == jobHeader2.PK);

			AssertHasError(newLink.FP_FH_HeaderToInfo, "Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.");

			link.Validation.ValidateAll();
			AssertHasError(link.FP_FH_HeaderToInfo, "Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow.");

			newLink.Delete();

			link.Validation.ValidateAll();
			AssertNoErrors(link);
		}

		#endregion

		#region Implementation

		void WorkflowProvidersLinked(IWorkflowProvider sourceWorkflowProvider, IWorkflowProvider newlyConnectedWorkflowProvider, Action<WorkflowProvidersLinkageUserControl> controlShownAction, bool shouldShowForm = true)
		{
			ZFormModaliser.SetDelegateToCallOnFormShown(form =>
			{
				var control = (WorkflowProvidersLinkageUserControl)((Form)form).Controls.Cast<Control>().Single(c => c is WorkflowProvidersLinkageUserControl);
				controlShownAction(control);
			});
			try
			{
				((IWorkflowProvidersLinkageService)service).WorkflowProvidersLinked(sourceWorkflowProvider, newlyConnectedWorkflowProvider, Factory);
				AssertEquals(Globals.IsUserInteractive && shouldShowForm, !(UnitTestUserNotification.Instance.LastMessage?.Context is null));
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}
		}

		void WorkflowProvidersUnLinked(IWorkflowProvider sourceWorkflowProvider, IWorkflowProvider newlyDisconnectedWorkflowProvider)
		{
			((IWorkflowProvidersLinkageService)service).WorkflowProvidersUnLinked(sourceWorkflowProvider, newlyDisconnectedWorkflowProvider, Factory);
		}

		void CreateTemplatesWithJobToJobLink(string workflowType1, string workflowType2)
		{
			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, workflowType1);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, workflowType2);

			var jobHeader1 = template1.GetJobHeader();
			var jobHeader2 = template2.GetJobHeader();

			BMSTestHelper.CreateDependencyLink(template1, jobHeader1, jobHeader2);

			templateFactory.Save();
		}

		(string jobToName, string workflowToName, string jobFromName, string workflowFromName) MakeTemplateWithTemplateLink()
		{
			const string jobToName = "AAA";
			const string workflowToName = "workflow1";
			const string jobFromName = "BBB";
			const string workflowFromName = "workflow2";

			var templateFactory = Factory.CreateNewFactory();

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, jobToName);
			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, jobFromName);

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, workflowToName);
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, workflowFromName);

			BMSTestHelper.CreateTask(template1, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template2, templateWorkflow2, string.Empty);

			BMSTestHelper.CreateDependencyLink(template1, templateWorkflow1, templateWorkflow2);

			templateFactory.Save();

			return (jobToName, workflowToName, jobFromName, workflowFromName);
		}

		IEnumerable<ProcessHeaderLink> LoadLinksForJob<T>(SchemaColumn uniqueIDColumn, ZGuid pk, bool isHeaderTo)
		{
			var jobQuery = new ZDBOnlySubQuery(typeof(T), uniqueIDColumn);
			jobQuery.AddToFilter(uniqueIDColumn, pk);

			var headerQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			headerQuery.AddSubQuery(ProcessHeaderSchema.FH_ParentId, jobQuery, JoinCondition.And);

			var linkQuery = new ZDBOnlyQuery(typeof(ProcessHeaderLink));
			var directionColumn = isHeaderTo ? ProcessHeaderLinkSchema.FP_FH_HeaderTo : ProcessHeaderLinkSchema.FP_FH_HeaderFrom;
			linkQuery.AddSubQuery(directionColumn, headerQuery, JoinCondition.And);

			return Factory.Load<ProcessHeaderLink>(linkQuery);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			BMSTestHelper.AddWorkflowType(config.System, OrgHeaderWorkflowDescriptor.WorkflowTypeCode);

			service = new WorkflowProvidersLinkageService();

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
		}

		SchematicTestConfig config;
		WorkflowProvidersLinkageService service;

		#endregion
	}

	#region Transactioned Test

	class WorkflowProvidersLinkageServiceTransactionedTest : TestCaseWithFactory
	{
		public void TestProposeLink_DuringTransaction_ShouldNotDisplayForm()
		{
			Assert(Db.Connection.IsInTransaction);

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIFRISTORG";
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAISCNDORG";
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			((IWorkflowProvidersLinkageService)service).WorkflowProvidersLinked(job2, job1, Factory);

			AssertNull(UnitTestUserNotification.Instance.LastMessage?.Context);
		}

		protected override void SetUp()
		{
			base.SetUp();

			service = new WorkflowProvidersLinkageService();
		}

		WorkflowProvidersLinkageService service;
	}

	#endregion
}
