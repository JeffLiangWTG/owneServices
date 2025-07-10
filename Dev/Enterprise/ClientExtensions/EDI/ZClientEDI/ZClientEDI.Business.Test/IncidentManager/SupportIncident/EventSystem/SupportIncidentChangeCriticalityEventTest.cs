using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentChangeCriticalityEventTest : SupportIncidentEventTestCase
	{
		public void TestTrigger_SetAssignee()
		{
			var incident = GetNewIncidentForTest() as SupportIncident;
			var incidentEvent = GetNewEventForTest(incident);
			CreateWorkflowTemplate(incidentEvent);
			Factory.Save();

			incidentEvent.Trigger();

			var workflowProvider = incident as IWorkflowProvider;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[0].P9_Status);
			AssertEquals("", incident.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
		}

		public void TestTrigger_CloseExistingTasks()
		{
			var incident = GetNewIncidentForTest();
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			existingTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;
			existingTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var incidentEvent = GetNewEventForTest(incident);

			Factory.Save();

			incidentEvent.Trigger();
			AssertEquals("No template found so should not close existing tasks", ProcessTaskStatusCodeList.Codes.Working, incident.WorkflowItems[0].P9_Status);
			AssertEquals("No template found so should not close existing tasks", ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[1].P9_Status);

			CreateWorkflowTemplate(incidentEvent);
			Factory.Save();

			incidentEvent.Trigger();
			AssertEquals("Matched template found", ProcessTaskStatusCodeList.Codes.Closed, incident.WorkflowItems[0].P9_Status);
			AssertEquals("Matched template found", ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[1].P9_Status);
		}

		protected override void AssertTriggerLastEventResult(bool result, int taskCountBeforeTrigger, int taskCountAfterTrigger)
		{
			Assert("Last event is not re-triggered", !result);
			AssertEquals("No tasks are added from last event", taskCountBeforeTrigger, taskCountAfterTrigger);
		}

		protected override IIncidentEvent GetNewEventForTest(IIncidentEventConsumer incident)
		{
			return new SupportIncidentChangeCriticalityEvent((SupportIncident)incident);
		}
	}
}