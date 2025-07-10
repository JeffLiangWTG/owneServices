using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentCargoWiseReopenEventTest : SupportIncidentEventTestCase
	{
		public void TestTrigger_SetAssignee()
		{
			var pm1 = Factory.NewWithValidTestData<GlbStaff>();
			pm1.GS_Code = "PM1";
			pm1.GS_EmailAddress = "pm1@test.com";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "UR1";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "UR2";

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "UR3";

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "CA1";

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "CA2";

			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			capability3.G4_Code = "CA3";

			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "Module A", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProductAreaAssignmentCollection assignmentCollection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = assignmentCollection.AddNew();
			assignment1.ProductArea = ProductAreaList.Codes.ARC;
			assignment1.Staff = pm1.GS_Code;
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			var incident = GetNewIncidentForTest() as SupportIncident;
			incident.IM_Module = "AAA";
			var incidentEvent = GetNewEventForTest(incident);
			var template = CreateWorkflowTemplate(incidentEvent);
			GlbCapability capability = Factory.NewWithValidTestData<GlbCapability>();
			foreach (ProcessTask task in template.WorkflowItems)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				task.P9_G4_RequiredCapability = ZGuid.Empty;
			}

			(ProcessTask, ProcessTask, ProcessTask) AddTasks()
			{
				var t1 = incident.WorkflowItems.AddNew();
				t1.P9_GS_NKAssignedStaffMember = "UR1";
				t1.P9_G4_RequiredCapability = capability1.PK;
				t1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				t1.P9_Sequence = 10;
				var t2 = incident.WorkflowItems.AddNew();
				t2.P9_GS_NKAssignedStaffMember = "UR3";
				t2.P9_G4_RequiredCapability = capability3.PK;
				t2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				t2.P9_Sequence = 5;
				var t3 = incident.WorkflowItems.AddNew();
				t3.P9_GS_NKAssignedStaffMember = "UR2";
				t3.P9_G4_RequiredCapability = capability2.PK;
				t3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				t3.P9_Sequence = 20;
				return (t1, t2, t3);
			}

			var (newTask1, newTask2, newTask3) = AddTasks();

			Factory.Save();

			incidentEvent.Trigger();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[3].P9_Status);
			AssertEquals("UR3", incident.WorkflowItems[3].P9_GS_NKAssignedStaffMember);
			AssertEquals("CA3", incident.WorkflowItems[3].RequiredCapability.G4_Code);

			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_Code = "SCW";
			staff4.GS_EmailAddress = "sam@test.com";
			incident.WorkflowItems.RemoveAndDeleteAll();
			incident.IM_GS_NKCustServiceContact = "SCW";
			Factory.Save();
			incidentEvent.Trigger();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[0].P9_Status);
			AssertEquals("SCW", incident.WorkflowItems[0].P9_GS_NKAssignedStaffMember);

			incident.WorkflowItems.RemoveAndDeleteAll();
			Factory.Save();
			(newTask1, newTask2, newTask3) = AddTasks();
			newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			newTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			foreach (ProcessTask task in template.WorkflowItems)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				task.P9_G4_RequiredCapability = ZGuid.Empty;
			}

			incidentEvent.Trigger();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[3].P9_Status);
			AssertEquals("PM1", incident.WorkflowItems[3].P9_GS_NKAssignedStaffMember);

			incident.WorkflowItems.RemoveAll();
			incidentEvent.Trigger();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[0].P9_Status);
			AssertEquals("PM1", incident.WorkflowItems[0].P9_GS_NKAssignedStaffMember);

			incident.WorkflowItems.RemoveAll();
			assignmentCollection.RemoveAll();
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			incidentEvent.Trigger();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[0].P9_Status);
			AssertEquals("", incident.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
		}

		public void TestArrangeTasksSequence()
		{
			SetupTemplate_OneTask();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Description = "Task 1";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Sequence = 2;
			task2.P9_Description = "Task 2";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var task3 = incident.WorkflowItems.AddNew();
			task3.P9_Sequence = 3;
			task3.P9_Description = "Task 3";
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			Factory.Save();
			var incidentEvent = GetNewEventForTest(incident);
			incidentEvent.Trigger();
			Factory.Save();
			incident.WorkflowItems.Sort(ProcessTasksSchema.Constants.P9_Sequence);
			AssertEquals(4, incident.WorkflowItems.Count);
			AssertTask(incident.WorkflowItems[0], "Task 1", 1, ProcessTaskStatusCodeList.Codes.Closed);
			AssertTask(incident.WorkflowItems[1], "Task 2", 2, ProcessTaskStatusCodeList.Codes.Closed);
			AssertTask(incident.WorkflowItems[2], "Task R", 10, ProcessTaskStatusCodeList.Codes.Assigned);
			AssertTask(incident.WorkflowItems[3], "Task 3", 13, ProcessTaskStatusCodeList.Codes.Assigned);

			incident.WorkflowItems[2].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task4 = incident.WorkflowItems.AddNew();
			task4.P9_Sequence = 20;
			task4.P9_Description = "Task 4";
			task4.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			Factory.Save();
			incidentEvent.Trigger();
			Factory.Save();
			incident.WorkflowItems.Sort(ProcessTasksSchema.Constants.P9_Sequence);
			AssertEquals(6, incident.WorkflowItems.Count);
			AssertTask(incident.WorkflowItems[0], "Task 1", 1, ProcessTaskStatusCodeList.Codes.Closed);
			AssertTask(incident.WorkflowItems[1], "Task 2", 2, ProcessTaskStatusCodeList.Codes.Closed);
			AssertTask(incident.WorkflowItems[2], "Task R", 10, ProcessTaskStatusCodeList.Codes.Closed);
			AssertTask(incident.WorkflowItems[3], "Task R", 20, ProcessTaskStatusCodeList.Codes.Assigned);
			AssertTask(incident.WorkflowItems[4], "Task 3", 23, ProcessTaskStatusCodeList.Codes.Assigned);
			AssertTask(incident.WorkflowItems[5], "Task 4", 30, ProcessTaskStatusCodeList.Codes.Assigned);

			incident.WorkflowItems[3].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ThirdPartySystemProblem, "");
			Factory.Save();
			incidentEvent.Trigger();
			Factory.Save();
			incident.WorkflowItems.Sort(ProcessTasksSchema.Constants.P9_Sequence);
			AssertEquals(7, incident.WorkflowItems.Count);
			AssertTask(incident.WorkflowItems[0], "Task 1", 1, ProcessTaskStatusCodeList.Codes.Closed);
			AssertTask(incident.WorkflowItems[1], "Task 2", 2, ProcessTaskStatusCodeList.Codes.Closed);
			AssertTask(incident.WorkflowItems[2], "Task R", 10, ProcessTaskStatusCodeList.Codes.Closed);
			AssertTask(incident.WorkflowItems[3], "Task R", 20, ProcessTaskStatusCodeList.Codes.Closed);
			AssertTask(incident.WorkflowItems[4], "Task 3", 23, ProcessTaskStatusCodeList.Codes.Cancelled);
			AssertTask(incident.WorkflowItems[5], "Task 4", 30, ProcessTaskStatusCodeList.Codes.Cancelled);
			AssertTask(incident.WorkflowItems[6], "Task R", 40, ProcessTaskStatusCodeList.Codes.Assigned);
		}

		protected override void AssertTriggerLastEventResult(bool result, int taskCountBeforeTrigger, int taskCountAfterTrigger)
		{
			Assert("Last event is not re-triggered", !result);
			AssertEquals("No tasks are added from last event", taskCountBeforeTrigger, taskCountAfterTrigger);
		}

		#region Arrange Tasks Sequence with Two Existing Tasks

		public void TestArrangeTasksSequence_TwoExistingTasks()
		{
			SetupTemplate_OneTask();

			RunTestWithTwoExistingTasks("CLS", "CLS",
							"RU 1", "Task 1", "CLS",
							"RU 2", "Task 2", "CLS",
							"RU 2", "Task R", "ASN");

			RunTestWithTwoExistingTasks("WRK", "CLS",
							"RU 1", "Task 1", "CLS",
							"RU 1", "Task R", "ASN",
							"RU 2", "Task 2", "CLS");

			RunTestWithTwoExistingTasks("CLS", "WRK",
							"RU 1", "Task 1", "CLS",
							"RU 2", "Task 2", "CLS",
							"RU 2", "Task R", "ASN");

			RunTestWithTwoExistingTasks("WRK", "WRK",
							"RU 1", "Task 1", "CLS",
							"RU 2", "Task 2", "CLS",
							"RU 2", "Task R", "ASN");

			RunTestWithTwoExistingTasks("CAN", "CLS",
							"RU 1", "Task 1", "CAN",
							"RU 2", "Task 2", "CLS",
							"RU 2", "Task R", "ASN");

			RunTestWithTwoExistingTasks("ASN", "CLS",
							"RU 1", "Task R", "ASN",
							"RU 1", "Task 1", "CAN",
							"RU 2", "Task 2", "CLS");

			RunTestWithTwoExistingTasks("CAN", "WRK",
							"RU 1", "Task 1", "CAN",
							"RU 2", "Task 2", "CLS",
							"RU 2", "Task R", "ASN");

			RunTestWithTwoExistingTasks("ASN", "WRK",
							"RU 1", "Task 1", "CAN",
							"RU 2", "Task 2", "CLS",
							"RU 2", "Task R", "ASN");

			RunTestWithTwoExistingTasks("CLS", "CAN",
							"RU 1", "Task 1", "CLS",
							"RU 2", "Task 2", "CAN",
							"RU 2", "Task R", "ASN");

			RunTestWithTwoExistingTasks("WRK", "CAN",
							"RU 1", "Task 1", "CLS",
							"RU 1", "Task R", "ASN",
							"RU 2", "Task 2", "CAN");

			RunTestWithTwoExistingTasks("CLS", "ASN",
							"RU 1", "Task 1", "CLS",
							"RU 2", "Task R", "ASN",
							"RU 2", "Task 2", "ASN");

			RunTestWithTwoExistingTasks("WRK", "ASN",
							"RU 1", "Task 1", "CLS",
							"RU 1", "Task R", "ASN",
							"RU 2", "Task 2", "ASN");

			RunTestWithTwoExistingTasks("CAN", "CAN",
							"RU 1", "Task 1", "CAN",
							"RU 2", "Task 2", "CAN",
							"RU 2", "Task R", "ASN");

			RunTestWithTwoExistingTasks("ASN", "CAN",
							"RU 1", "Task R", "ASN",
							"RU 1", "Task 1", "ASN",
							"RU 2", "Task 2", "CAN");

			RunTestWithTwoExistingTasks("CAN", "ASN",
							"RU 1", "Task 1", "CAN",
							"RU 2", "Task R", "ASN",
							"RU 2", "Task 2", "ASN");

			RunTestWithTwoExistingTasks("ASN", "ASN",
							"RU 1", "Task R", "ASN",
							"RU 1", "Task 1", "ASN",
							"RU 2", "Task 2", "ASN");
		}

		void SetupTemplate_OneTask()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_OH_Client = templateOrg.PK;
			template.P0_ProcessType = IncidentTemplateType;

			GlbCapability capability = Factory.New<GlbCapability>();
			capability.G4_Code = "AAA";

			var templateHeader1 = template.ProcessHeaders.AddNew();
			templateHeader1.FH_CompletionStatement = GetNewEventForTest(Factory.New<SupportIncident>()).Code;

			var task11 = template.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = templateHeader1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "AAA";
			task11.P9_Description = "Task R";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task11.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();
		}

		void RunTestWithTwoExistingTasks(ZString existingTaskStatus1, ZString existingTaskStatus2,
			ZString expectedProcessHeaderText1, ZString expectedDescription1, ZString expectedStatus1,
			ZString expectedProcessHeaderText2, ZString expectedDescription2, ZString expectedStatus2,
			ZString expectedProcessHeaderText3, ZString expectedDescription3, ZString expectedStatus3)
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(incident, incident.Factory);
			var processHeader1 = jobHeader.ProcessHeaders.AddNew();
			processHeader1.FH_CompletionStatement = "RU 1";
			var processHeader2 = jobHeader.ProcessHeaders.AddNew();
			processHeader2.FH_CompletionStatement = "RU 2";
			AssertEquals("Pre-condition", 3, jobHeader.ProcessHeaders.Count);

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = processHeader1.PK;
			task1.P9_Sequence = 10;
			task1.P9_Description = "Task 1";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = existingTaskStatus1;

			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = processHeader2.PK;
			task2.P9_Sequence = 20;
			task2.P9_Description = "Task 2";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = existingTaskStatus2;

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			Factory.Save();
			GetNewEventForTest(incident).Trigger();
			Factory.Save();
			incident.WorkflowItems.Sort(ProcessTasksSchema.Constants.P9_Sequence);
			AssertEquals("Process header count unchanged", 3, jobHeader.ProcessHeaders.Count);
			AssertEquals(3, incident.WorkflowItems.Count);
			AssertTask("Task #1", incident.WorkflowItems[0], expectedDescription1, 10, expectedStatus1, expectedProcessHeaderText1);
			AssertTask("Task #2", incident.WorkflowItems[1], expectedDescription2, 20, expectedStatus2, expectedProcessHeaderText2);
			AssertTask("Task #3", incident.WorkflowItems[2], expectedDescription3, 30, expectedStatus3, expectedProcessHeaderText3);
		}

		#endregion

		#region Arrange Tasks Sequence with Three Existing Tasks

		public void TestArrangeTasksSequence_ThreeExistingTasks()
		{
			SetupTemplate_TwoTasks();

			RunTestWithThreeExistingTasks(
				"WRK", "ASN", "ASN",
				"RU 1", "Task 1", "CLS",
				"RU 1", "Task R1", "ASN",
				"RU 1", "Task R2", "ASN",
				"RU 1", "Task 2", "ASN",
				"RU 2", "Task 3", "ASN");

			RunTestWithThreeExistingTasks(
				"CLS", "WRK", "ASN",
				"RU 1", "Task 1", "CLS",
				"RU 1", "Task 2", "CLS",
				"RU 1", "Task R1", "ASN",
				"RU 1", "Task R2", "ASN",
				"RU 2", "Task 3", "ASN");

			RunTestWithThreeExistingTasks(
				"ASN", "ASN", "ASN",
				"RU 1", "Task R1", "ASN",
				"RU 1", "Task R2", "ASN",
				"RU 1", "Task 1", "ASN",
				"RU 1", "Task 2", "ASN",
				"RU 2", "Task 3", "ASN");

			RunTestWithThreeExistingTasks(
				"CLS", "CLS", "ASN",
				"RU 1", "Task 1", "CLS",
				"RU 1", "Task 2", "CLS",
				"RU 2", "Task R1", "ASN",
				"RU 2", "Task R2", "ASN",
				"RU 2", "Task 3", "ASN");

			RunTestWithThreeExistingTasks(
				"CLS", "CLS", "WRK",
				"RU 1", "Task 1", "CLS",
				"RU 1", "Task 2", "CLS",
				"RU 2", "Task 3", "CLS",
				"RU 2", "Task R1", "ASN",
				"RU 2", "Task R2", "ASN");

			RunTestWithThreeExistingTasks(
				"CLS", "CAN", "ASN",
				"RU 1", "Task 1", "CLS",
				"RU 1", "Task 2", "CAN",
				"RU 2", "Task R1", "ASN",
				"RU 2", "Task R2", "ASN",
				"RU 2", "Task 3", "ASN");
		}

		void SetupTemplate_TwoTasks()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_OH_Client = templateOrg.PK;
			template.P0_ProcessType = IncidentTemplateType;

			GlbCapability capability = Factory.New<GlbCapability>();
			capability.G4_Code = "AAA";

			var templateHeader1 = template.ProcessHeaders.AddNew();
			templateHeader1.FH_CompletionStatement = GetNewEventForTest(Factory.New<SupportIncident>()).Code;

			var task11 = template.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = templateHeader1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "AAA";
			task11.P9_Description = "Task R1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task11.P9_G4_RequiredCapability = capability.PK;

			var task12 = template.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = templateHeader1.PK;
			task12.P9_Sequence = 20;
			task12.P9_Type = "AAA";
			task12.P9_Description = "Task R2";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task12.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();
		}

		void RunTestWithThreeExistingTasks(
			ZString existingTaskStatus1, ZString existingTaskStatus2, ZString existingTaskStatus3,
			ZString expectedProcessHeaderText1, ZString expectedDescription1, ZString expectedStatus1,
			ZString expectedProcessHeaderText2, ZString expectedDescription2, ZString expectedStatus2,
			ZString expectedProcessHeaderText3, ZString expectedDescription3, ZString expectedStatus3,
			ZString expectedProcessHeaderText4, ZString expectedDescription4, ZString expectedStatus4,
			ZString expectedProcessHeaderText5, ZString expectedDescription5, ZString expectedStatus5)
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(incident, incident.Factory);
			var processHeader1 = jobHeader.ProcessHeaders.AddNew();
			processHeader1.FH_CompletionStatement = "RU 1";
			var processHeader2 = jobHeader.ProcessHeaders.AddNew();
			processHeader2.FH_CompletionStatement = "RU 2";
			AssertEquals("Pre-condition", 3, jobHeader.ProcessHeaders.Count);

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = processHeader1.PK;
			task1.P9_Sequence = 10;
			task1.P9_Description = "Task 1";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = existingTaskStatus1;

			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = processHeader1.PK;
			task2.P9_Sequence = 20;
			task2.P9_Description = "Task 2";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = existingTaskStatus2;

			var task3 = incident.WorkflowItems.AddNew();
			task3.P9_FH_ProcessHeader = processHeader2.PK;
			task3.P9_Sequence = 30;
			task3.P9_Description = "Task 3";
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_Status = existingTaskStatus3;

			Factory.Save();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			Factory.Save();
			GetNewEventForTest(incident).Trigger();
			Factory.Save();
			incident.WorkflowItems.Sort(ProcessTasksSchema.Constants.P9_Sequence);
			AssertEquals("Process header count unchanged", 3, jobHeader.ProcessHeaders.Count);
			AssertEquals(5, incident.WorkflowItems.Count);
			AssertTask("Task #1", incident.WorkflowItems[0], expectedDescription1, 10, expectedStatus1, expectedProcessHeaderText1);
			AssertTask("Task #2", incident.WorkflowItems[1], expectedDescription2, 20, expectedStatus2, expectedProcessHeaderText2);
			AssertTask("Task #3", incident.WorkflowItems[2], expectedDescription3, 30, expectedStatus3, expectedProcessHeaderText3);
			AssertTask("Task #4", incident.WorkflowItems[3], expectedDescription4, 40, expectedStatus4, expectedProcessHeaderText4);
			AssertTask("Task #5", incident.WorkflowItems[4], expectedDescription5, 50, expectedStatus5, expectedProcessHeaderText5);
		}

		#endregion

		void AssertTask(ProcessTask task, ZString expectedDescription, ZInt expectedSequence, ZString expectedStatus)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Description", expectedDescription, task.P9_Description);
				AssertEquals("Sequence", expectedSequence, task.P9_Sequence);
				AssertEquals("Status", expectedStatus, task.P9_Status);
			});
		}

		void AssertTask(ZString message, ProcessTask task, ZString expectedDescription, ZInt expectedSequence, ZString expectedStatus, ZString expectedProcessHeaderText)
		{
			CombineAssertions(() =>
			{
				AssertEquals(message + " Description", expectedDescription, task.P9_Description);
				AssertEquals(message + " Sequence", expectedSequence, task.P9_Sequence);
				AssertEquals(message + " Status", expectedStatus, task.P9_Status);
				AssertEquals(message + " Workflow Description", expectedProcessHeaderText, task.ProcessHeader.FH_CompletionStatement);
			});
		}

		protected override IIncidentEvent GetNewEventForTest(IIncidentEventConsumer incident)
		{
			return new SupportIncidentCargoWiseReopenEvent((SupportIncident)incident);
		}
	}
}
