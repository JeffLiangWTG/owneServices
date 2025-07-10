using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public abstract class IncidentEventTestCase : TestCaseWithFactory
	{
		public virtual void TestTrigger()
		{
			var incident = GetNewIncidentForTest();
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			existingTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;
			existingTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var incidentEvent = GetNewEventForTest(incident);
			CreateWorkflowTemplate(incidentEvent);

			Factory.Save();

			incidentEvent.Trigger();

			AssertEquals(6, incident.WorkflowItems.Count);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, incident.WorkflowItems[0].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[1].P9_Status);

			AssertEquals("AAA Task 1", incident.WorkflowItems[2].P9_Description);
			AssertEquals("AAA Task 2", incident.WorkflowItems[3].P9_Description);
			AssertEquals("BBB Task 1", incident.WorkflowItems[4].P9_Description);
			AssertEquals("BBB Task 2", incident.WorkflowItems[5].P9_Description);

			AssertEquals("AAA", incident.WorkflowItems[2].ProcessHeader.FH_CompletionStatement);
			AssertEquals("AAA", incident.WorkflowItems[3].ProcessHeader.FH_CompletionStatement);
			AssertEquals("BBB", incident.WorkflowItems[4].ProcessHeader.FH_CompletionStatement);
			AssertEquals("BBB", incident.WorkflowItems[5].ProcessHeader.FH_CompletionStatement);

			AssertEquals(25, incident.WorkflowItems[2].P9_Sequence);
			AssertEquals(40, incident.WorkflowItems[3].P9_Sequence);
			AssertEquals(50, incident.WorkflowItems[4].P9_Sequence);
			AssertEquals(70, incident.WorkflowItems[5].P9_Sequence);

			Factory.Save();

			incidentEvent.Trigger();

			AssertEquals(10, incident.WorkflowItems.Count);

			AssertEquals("AAA Task 1", incident.WorkflowItems[6].P9_Description);
			AssertEquals("AAA Task 2", incident.WorkflowItems[7].P9_Description);
			AssertEquals("BBB Task 1", incident.WorkflowItems[8].P9_Description);
			AssertEquals("BBB Task 2", incident.WorkflowItems[9].P9_Description);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(incident, incident.Factory);
			AssertEquals("One default process header, plus two from template", 3, jobHeader.ProcessHeaders.Count);
			var processHeader1 = incident.WorkflowItems[2].ProcessHeader;
			var processHeader2 = incident.WorkflowItems[4].ProcessHeader;
			AssertEquals(processHeader1.PK, incident.WorkflowItems[6].ProcessHeader.PK);
			AssertEquals(processHeader1.PK, incident.WorkflowItems[7].ProcessHeader.PK);
			AssertEquals(processHeader2.PK, incident.WorkflowItems[8].ProcessHeader.PK);
			AssertEquals(processHeader2.PK, incident.WorkflowItems[9].ProcessHeader.PK);

			AssertEquals("AAA", incident.WorkflowItems[6].ProcessHeader.FH_CompletionStatement);
			AssertEquals("AAA", incident.WorkflowItems[7].ProcessHeader.FH_CompletionStatement);
			AssertEquals("BBB", incident.WorkflowItems[8].ProcessHeader.FH_CompletionStatement);
			AssertEquals("BBB", incident.WorkflowItems[9].ProcessHeader.FH_CompletionStatement);

			AssertEquals(75, incident.WorkflowItems[6].P9_Sequence);
			AssertEquals(90, incident.WorkflowItems[7].P9_Sequence);
			AssertEquals(100, incident.WorkflowItems[8].P9_Sequence);
			AssertEquals(120, incident.WorkflowItems[9].P9_Sequence);
		}

		public virtual void TestTrigger_DeleteUnsavedEventTasks()
		{
			var incident = GetNewIncidentForTest();
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			existingTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;
			existingTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var incidentEvent = GetNewEventForTest(incident);
			CreateWorkflowTemplate(incidentEvent);

			Factory.Save();

			incidentEvent.Trigger();

			AssertEquals(6, incident.WorkflowItems.Count);

			AssertEquals("AAA Task 1", incident.WorkflowItems[2].P9_Description);
			AssertEquals("AAA Task 2", incident.WorkflowItems[3].P9_Description);
			AssertEquals("BBB Task 1", incident.WorkflowItems[4].P9_Description);
			AssertEquals("BBB Task 2", incident.WorkflowItems[5].P9_Description);

			AssertEquals(25, incident.WorkflowItems[2].P9_Sequence);
			AssertEquals(40, incident.WorkflowItems[3].P9_Sequence);
			AssertEquals(50, incident.WorkflowItems[4].P9_Sequence);
			AssertEquals(70, incident.WorkflowItems[5].P9_Sequence);

			var existingTask3 = incident.WorkflowItems.AddNew();
			existingTask3.P9_Sequence = 30;
			existingTask3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incidentEvent.Trigger();

			AssertEquals(7, incident.WorkflowItems.Count);

			AssertEquals("AAA Task 1", incident.WorkflowItems[3].P9_Description);
			AssertEquals("AAA Task 2", incident.WorkflowItems[4].P9_Description);
			AssertEquals("BBB Task 1", incident.WorkflowItems[5].P9_Description);
			AssertEquals("BBB Task 2", incident.WorkflowItems[6].P9_Description);

			AssertEquals(35, incident.WorkflowItems[3].P9_Sequence);
			AssertEquals(50, incident.WorkflowItems[4].P9_Sequence);
			AssertEquals(60, incident.WorkflowItems[5].P9_Sequence);
			AssertEquals(80, incident.WorkflowItems[6].P9_Sequence);
		}

		public void TestLinkWorkItemAndIncident_WhenNoInterTemplateLinksExist()
		{
			var incident = (SupportIncident)GetNewIncidentForTest();
			var incidentTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			incidentTemplate.P0_ProcessType = IncidentTemplateType;
			var incidentTemplateWorkflow = bmTestHelper.CreateWorkflow(incidentTemplate, "STAHP");
			var incidentTemplateTask = bmTestHelper.CreateTask(incidentTemplate, incidentTemplateWorkflow);

			var workItemTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workItemTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItem.Code;

			var workItemTemplateWorkflow = bmTestHelper.CreateWorkflow(workItemTemplate, "PLIZ");
			var workItemTemplateTask = bmTestHelper.CreateTask(workItemTemplate, workItemTemplateWorkflow);

			Factory.Save();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident.ApplyWorkflowTemplates();

			Factory.Save();

			var jobHeader1 = ProcessJobHeaderProvider.GetForParent(incident, Factory);
			var jobHeader2 = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "STAHP");
			var workflow2 = jobHeader2.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "PLIZ");

			bmTestHelper.AssertIsNotPrerequisite(workflow1, workflow2);

			workItem.RelatedItems.Add(incident);

			bmTestHelper.AssertIsNotPrerequisite(workflow1, workflow2);
		}

		[GuiTest]
		public void TestLinkWorkItemAndIncident_WhenInterTemplateLinksExist()
		{
			var incident = (SupportIncident)GetNewIncidentForTest();
			var incidentTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			incidentTemplate.P0_ProcessType = IncidentTemplateType;
			var incidentTemplateWorkflow = bmTestHelper.CreateWorkflow(incidentTemplate, "STAHP");
			var incidentTemplateTask = bmTestHelper.CreateTask(incidentTemplate, incidentTemplateWorkflow);

			var workItemTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workItemTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItem.Code;

			var workItemTemplateWorkflow = bmTestHelper.CreateWorkflow(workItemTemplate, "PLIZ");
			var workItemTemplateTask = bmTestHelper.CreateTask(workItemTemplate, workItemTemplateWorkflow);
			var templateLink = bmTestHelper.CreateDependencyLink(workItemTemplate, incidentTemplateWorkflow, workItemTemplateWorkflow);

			Factory.Save();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident.ApplyWorkflowTemplates();

			Factory.Save();

			var jobHeader1 = ProcessJobHeaderProvider.GetForParent(incident, Factory);
			var jobHeader2 = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "STAHP");
			var workflow2 = jobHeader2.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "PLIZ");

			bmTestHelper.AssertIsNotPrerequisite(workflow1, workflow2);

			workItem.RelatedItems.Add(incident);

			bmTestHelper.AssertIsPrerequisite(workflow1, workflow2);
		}

		[GuiTest]
		public void TestLinkWorkItemAndIncident_WhenInterTemplateLinksExist_AndWorkflowNamePrefixedByIncidentEventCode()
		{
			var incident = (SupportIncident)GetNewIncidentForTest();
			var incidentEvent = GetNewEventForTest(incident);
			var incidentTemplate = CreateWorkflowTemplate(incidentEvent);
			var incidentTemplateWorkflow = incidentTemplate.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == incidentEvent.Code + " AAA");

			var workItemTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workItemTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItem.Code;

			var workItemTemplateWorkflow = bmTestHelper.CreateWorkflow(workItemTemplate, "PLIZ");
			var workItemTemplateTask = bmTestHelper.CreateTask(workItemTemplate, workItemTemplateWorkflow);
			var templateLink = bmTestHelper.CreateDependencyLink(workItemTemplate, incidentTemplateWorkflow, workItemTemplateWorkflow);

			Factory.Save();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incidentEvent.Trigger();

			Factory.Save();

			var jobHeader1 = ProcessJobHeaderProvider.GetForParent(incident, Factory);
			var jobHeader2 = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "AAA");
			var workflow2 = jobHeader2.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "PLIZ");

			bmTestHelper.AssertIsNotPrerequisite(workflow1, workflow2);

			workItem.RelatedItems.Add(incident);

			bmTestHelper.AssertIsPrerequisite(workflow1, workflow2);
		}

		public void TestLinkWorkItemAndIncident_WhenInterTemplateLinkDoesNotExist_AndWorkflowNamePrefixedByIncidentEventCode()
		{
			var incident = (SupportIncident)GetNewIncidentForTest();
			var incidentEvent = GetNewEventForTest(incident);
			var incidentTemplate = CreateWorkflowTemplate(incidentEvent);
			var incidentTemplateWorkflow = incidentTemplate.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == incidentEvent.Code + " AAA");

			var workItemTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workItemTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItem.Code;

			var workItemTemplateWorkflow = bmTestHelper.CreateWorkflow(workItemTemplate, "PLIZ");
			var workItemTemplateTask = bmTestHelper.CreateTask(workItemTemplate, workItemTemplateWorkflow);

			Factory.Save();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incidentEvent.Trigger();

			Factory.Save();

			var jobHeader1 = ProcessJobHeaderProvider.GetForParent(incident, Factory);
			var jobHeader2 = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "AAA");
			var workflow2 = jobHeader2.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "PLIZ");

			bmTestHelper.AssertIsNotPrerequisite(workflow1, workflow2);

			workItem.RelatedItems.Add(incident);

			bmTestHelper.AssertIsNotPrerequisite(workflow1, workflow2);
		}

		protected virtual ProcessTaskTemplate CreateWorkflowTemplate(IIncidentEvent incidentEvent)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = IncidentTemplateType;

			var header1 = template.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = incidentEvent.Code + " AAA";
			var header2 = template.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = incidentEvent.Code + " BBB";
			var header3 = template.ProcessHeaders.AddNew();
			header3.FH_CompletionStatement = "ZZZ Template Not For Event";
			var header4 = template.ProcessHeaders.AddNew();
			header4.FH_CompletionStatement = incidentEvent.Code + " Process header which has one task with UDF";

			var task11 = template.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 5;
			task11.P9_Type = "AAA";
			task11.P9_Description = "AAA Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task12 = template.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 20;
			task12.P9_Type = "AAA";
			task12.P9_Description = "AAA Task 2";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task21 = template.WorkflowItems.AddNew();
			task21.P9_FH_ProcessHeader = header2.PK;
			task21.P9_Sequence = 30;
			task21.P9_Type = "BBB";
			task21.P9_Description = "BBB Task 1";
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task22 = template.WorkflowItems.AddNew();
			task22.P9_FH_ProcessHeader = header2.PK;
			task22.P9_Sequence = 50;
			task22.P9_Type = "BBB";
			task22.P9_Description = "BBB Task 2";
			task22.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task31 = template.WorkflowItems.AddNew();
			task31.P9_FH_ProcessHeader = header3.PK;
			task31.P9_Sequence = 60;
			task31.P9_Type = "ZZZ";
			task31.P9_Description = "ZZZ Task 1";
			task31.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task41 = template.WorkflowItems.AddNew();
			task41.P9_FH_ProcessHeader = header4.PK;
			task41.P9_Sequence = 70;
			task41.P9_Type = "TTT";
			task41.P9_Description = "TTT Task 1";
			task41.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task41.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task41.TemplateConditions.TemplateCondition2Value = @"""<Client.OH_Code>"" == ""DDDOOOSYD""";

			return template;
		}

		public void TestApplyWorkflowTemplate_DuplicateWorkFlowLink()
		{
			void SetTaskData(ProcessTask task, ZInt seq)
			{
				task.P9_Sequence = seq;
				task.P9_Type = "AAA";
				task.P9_Description = $"AAA Task {seq}";
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			}

			var incident = GetNewIncidentForTest();
			var incidentEvent = new IncidentEventForBaseTest(incident);

			var incTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			incTemplate1.P0_ProcessType = IncidentTemplateType;

			var header1 = incTemplate1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "WorkFlow";
			var task1 = incTemplate1.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = header1.PK;
			SetTaskData(task1, 5);

			Factory.Save();

			incident.ApplyWorkflowTemplates();
			Factory.Save();
			var jobHeader1 = ProcessJobHeaderProvider.GetForParent(incident, Factory);
			AssertNotNull(jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "WorkFlow"));

			var incTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			incTemplate2.P0_ProcessType = IncidentTemplateType;
			incTemplate2.P0_Name = "ChildTemplate";

			var header2 = incTemplate2.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = $"{incidentEvent.Code} {header1.FH_CompletionStatement}";
			var task2 = incTemplate2.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = header2.PK;
			SetTaskData(task2, 55);

			var header22 = incTemplate2.ProcessHeaders.AddNew();
			header22.FH_CompletionStatement = $"{incidentEvent.Code} Wow";
			var task22 = incTemplate2.WorkflowItems.AddNew();
			task22.P9_FH_ProcessHeader = header22.PK;
			SetTaskData(task22, 66);

			var header23 = incTemplate2.ProcessHeaders.AddNew();
			header23.FH_CompletionStatement = $"{incidentEvent.Code} Anti Wow";
			var task23 = incTemplate2.WorkflowItems.AddNew();
			task23.P9_FH_ProcessHeader = header23.PK;
			SetTaskData(task23, 77);

			bmTestHelper.CreateDependencyLink(incTemplate2, header2, header22);
			bmTestHelper.CreateDependencyLink(incTemplate2, header23, header2);

			Factory.Save();

			incidentEvent.Trigger();
			var processHeader1 = jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "WorkFlow");
			var processHeader12 = jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "Wow");
			var processHeader13 = jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "Anti Wow");

			AssertEquals(3, jobHeader1.ProcessHeaders.Count);
			AssertNotNull(processHeader1);
			AssertNotNull(processHeader12);
			AssertNotNull(processHeader13);

			Assert(processHeader1.LinksFromMeToOthers[0].FP_FH_HeaderTo == processHeader12.PK);
			Assert(processHeader1.LinksFromOthersToMe[0].FP_FH_HeaderFrom == processHeader13.PK);

			foreach (ProcessHeaderLink link in processHeader1.Links)
			{
				AssertNoErrors("There shouldn't be the UNIQUE error before firing again", link.FP_FH_HeaderFromInfo);
				AssertNoErrors("There shouldn't be the UNIQUE error before firing again", link.FP_FH_HeaderToInfo);
			}

			Factory.Save();

			incidentEvent.Trigger();

			AssertEquals(3, jobHeader1.ProcessHeaders.Count);
			AssertEquals("Only one link should be created", 1, processHeader1.LinksFromMeToOthers.Count);
			AssertEquals(processHeader1.LinksFromMeToOthers[0].FP_FH_HeaderTo, processHeader12.PK);

			AssertEquals("Only one link should be created", 1, processHeader1.LinksFromOthersToMe.Count);
			AssertEquals(processHeader1.LinksFromOthersToMe[0].FP_FH_HeaderFrom, processHeader13.PK);
		}

		public void TestApplyWorkflowTemplate_LinkToSelf()
		{
			void SetTaskData(ProcessTask task, ZInt seq)
			{
				task.P9_Sequence = seq;
				task.P9_Type = "AAA";
				task.P9_Description = $"AAA Task {seq}";
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			}

			var incident = GetNewIncidentForTest();
			var incidentEvent = new IncidentEventForBaseTest(incident);
			var incTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			incTemplate1.P0_ProcessType = IncidentTemplateType;

			var header1 = incTemplate1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "WorkFlow";
			var task1 = incTemplate1.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = header1.PK;
			SetTaskData(task1, 5);

			var header12 = incTemplate1.ProcessHeaders.AddNew();
			header12.FH_CompletionStatement = "Anti WorkFlow";
			var task12 = incTemplate1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header12.PK;
			SetTaskData(task12, 6);

			Factory.Save();

			incident.ApplyWorkflowTemplates();
			Factory.Save();
			var jobHeader1 = ProcessJobHeaderProvider.GetForParent(incident, Factory);
			AssertNotNull(jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "WorkFlow"));

			var incTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			incTemplate2.P0_ProcessType = IncidentTemplateType;
			incTemplate2.P0_Name = "ChildTemplate";

			var header2 = incTemplate2.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = $"{incidentEvent.Code} {header1.FH_CompletionStatement}";
			var task2 = incTemplate2.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = header2.PK;
			SetTaskData(task2, 55);

			var header22 = incTemplate2.ProcessHeaders.AddNew();
			header22.FH_CompletionStatement = $"{incidentEvent.Code} {header12.FH_CompletionStatement}";
			var task22 = incTemplate2.WorkflowItems.AddNew();
			task22.P9_FH_ProcessHeader = header22.PK;
			SetTaskData(task22, 66);

			bmTestHelper.CreateDependencyLink(incTemplate2, header2, header1);
			bmTestHelper.CreateDependencyLink(incTemplate1, header12, header22);
			Factory.Save();

			incidentEvent.Trigger();
			var processHeader1 = jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "WorkFlow");
			var processHeader12 = jobHeader1.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "Anti WorkFlow");

			AssertNotNull(processHeader1);
			AssertNotNull(processHeader12);
			AssertEquals(2, jobHeader1.ProcessHeaders.Count);

			AssertEquals("No links should be created", 0, processHeader1.Links.Count());
			AssertEquals("No links should be created", 0, processHeader12.Links.Count());
		}

		#region Process Header Links

		public void TestTrigger_ProcessHeaderLinks()
		{
			var incident = GetNewIncidentForTest();
			var incidentEvent = GetNewEventForTest(incident);
			CreateWorkflowTemplateWithHeaderLink(incidentEvent);
			Factory.Save();

			// No existing link
			incidentEvent.Trigger();
			Factory.Save();
			AssertProcessHeaderLink(incident);

			// Existing link AAA => BBB
			incident = GetNewIncidentForTest();
			CreateExistingProcessHeaderLink(incident, "AAA", "BBB");
			GetNewEventForTest(incident).Trigger();
			Factory.Save();
			AssertProcessHeaderLink(incident);

			// Existing link AAA => CCC
			incident = GetNewIncidentForTest();
			CreateExistingProcessHeaderLink(incident, "AAA", "CCC");
			GetNewEventForTest(incident).Trigger();
			Factory.Save();
			AssertProcessHeaderLink(incident);

			// Existing link DDD => BBB
			incident = GetNewIncidentForTest();
			CreateExistingProcessHeaderLink(incident, "DDD", "BBB");
			GetNewEventForTest(incident).Trigger();
			Factory.Save();
			AssertProcessHeaderLink(incident);
		}

		protected virtual ProcessTaskTemplate CreateWorkflowTemplateWithHeaderLink(IIncidentEvent incidentEvent)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = IncidentTemplateType;

			var templateHeader1 = template.ProcessHeaders.AddNew();
			templateHeader1.FH_CompletionStatement = incidentEvent.Code + " AAA";
			var templateHeader2 = template.ProcessHeaders.AddNew();
			templateHeader2.FH_CompletionStatement = incidentEvent.Code + " BBB";

			bmTestHelper.CreateDependencyLink(template, templateHeader1, templateHeader2);

			var templateTask1 = template.WorkflowItems.AddNew();
			templateTask1.P9_FH_ProcessHeader = templateHeader1.PK;
			templateTask1.P9_Description = "Template AAA Task";

			var templateTask2 = template.WorkflowItems.AddNew();
			templateTask2.P9_FH_ProcessHeader = templateHeader2.PK;
			templateTask2.P9_Description = "Template BBB Task";

			return template;
		}

		static void CreateExistingProcessHeaderLink(IIncidentEventConsumer incident, ZString fromHeaderCompletionStatement, ZString toHeaderCompletionStatement)
		{
			var jobHeader = ProcessJobHeaderProvider.GetForParent(incident, incident.Factory);

			var existingHeader1 = jobHeader.ProcessHeaders.AddNew();
			existingHeader1.FH_CompletionStatement = fromHeaderCompletionStatement;
			var existingHeader2 = jobHeader.ProcessHeaders.AddNew();
			existingHeader2.FH_CompletionStatement = toHeaderCompletionStatement;

			var existingLink1To2 = existingHeader1.LinksFromMeToOthers.AddNew() as IProcessHeaderLink;
			existingLink1To2.FP_FH_HeaderTo = existingHeader2.PK;
			existingLink1To2.FP_LinkType = "DEP";

			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_FH_ProcessHeader = existingHeader1.PK;

			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_FH_ProcessHeader = existingHeader2.PK;
		}

		static void AssertProcessHeaderLink(IIncidentEventConsumer incident)
		{
			var processHeader1 = incident.WorkflowItems.Cast<ProcessTask>().First(t => t.P9_Description == "Template AAA Task").ProcessHeader;
			var processHeader2 = incident.WorkflowItems.Cast<ProcessTask>().First(t => t.P9_Description == "Template BBB Task").ProcessHeader;

			AssertEquals("AAA", processHeader1.FH_CompletionStatement);
			AssertEquals("BBB", processHeader2.FH_CompletionStatement);
			AssertEquals(true, processHeader1.LinksFromMeToOthers.Cast<IProcessHeaderLink>().Any(link => link.FP_FH_HeaderTo == processHeader2.PK));
			AssertEquals(true, processHeader2.LinksFromOthersToMe.Cast<IProcessHeaderLink>().Any(link => link.FP_FH_HeaderFrom == processHeader1.PK));
		}

		#endregion

		#region Implementation

		protected abstract IIncidentEventConsumer GetNewIncidentForTest();
		protected abstract IIncidentEvent GetNewEventForTest(IIncidentEventConsumer incident);

		protected static ZString IncidentTemplateType => "INC";

		IBMTestHelper bmTestHelper;

		protected override void SetUp()
		{
			base.SetUp();

			bmTestHelper = ObjectFactory.Get<IBMTestHelper>();

			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, IncidentTemplateType, JobInvoicingConsumerTypes.WorkItem.Code);
		}

		class IncidentEventForBaseTest : IncidentEvent
		{
			public IncidentEventForBaseTest(IIncidentEventConsumer incident) : base(incident)
			{
			}

			public override ZString Code => "ABC";
		}

		#endregion
	}
}
