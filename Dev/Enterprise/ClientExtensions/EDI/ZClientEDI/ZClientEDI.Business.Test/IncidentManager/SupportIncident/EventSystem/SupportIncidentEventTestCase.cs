using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public abstract class SupportIncidentEventTestCase : IncidentEventTestCase
	{
		public void TestTrigger_SuspendStatusAndDispositionCalculationWhenApplyingTemplate()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var incident = GetNewIncidentForTest() as SupportIncident;
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			existingTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;
			existingTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var incidentEvent = GetNewEventForTest(incident);

			Factory.Save();

			incident.OnCloseIncident += (s, e) => { Assert("The on closing incident event handler should not be called when triggering event", false); };
			incidentEvent.Trigger();
			Assert(true);
		}

		public void TestTrigger_TaskUserDefinedConditionWithClientCode()
		{
			var incident = GetNewIncidentForTest() as SupportIncident;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DDDOOOSYD";
			var contact = org.Contacts.AddNew();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;

			var incidentEvent = GetNewEventForTest(incident);
			CreateWorkflowTemplate(incidentEvent);

			Factory.Save();

			incidentEvent.Trigger();

			AssertEquals(5, incident.WorkflowItems.Count);

			AssertEquals("AAA Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals("AAA Task 2", incident.WorkflowItems[1].P9_Description);
			AssertEquals("BBB Task 1", incident.WorkflowItems[2].P9_Description);
			AssertEquals("BBB Task 2", incident.WorkflowItems[3].P9_Description);
			AssertEquals("TTT Task 1", incident.WorkflowItems[4].P9_Description);
		}

		public void TestTriggerLastEvent()
		{
			var incident = GetNewIncidentForTest() as SupportIncident;
			var incidentEvent = GetNewEventForTest(incident);
			CreateWorkflowTemplate(incidentEvent);
			Factory.Save();

			incidentEvent.Trigger();
			Factory.Save();

			int taskCount = incident.WorkflowItems.Count;
			bool result = SupportIncidentEvent.TriggerLastEvent(incident);
			AssertTriggerLastEventResult(result, taskCount, incident.WorkflowItems.Count);
		}

		protected virtual void AssertTriggerLastEventResult(bool result, int taskCountBeforeTrigger, int taskCountAfterTrigger)
		{
			Assert("Last event is re-triggered", result);
			AssertEquals("Tasks are added from last event", taskCountBeforeTrigger * 2, taskCountAfterTrigger);
		}

		protected override ProcessTaskTemplate CreateWorkflowTemplate(IIncidentEvent incidentEvent)
		{
			return CreateSupportIncidentEventWorkflowTemplate(incidentEvent, base.CreateWorkflowTemplate);
		}

		protected override ProcessTaskTemplate CreateWorkflowTemplateWithHeaderLink(IIncidentEvent incidentEvent)
		{
			return CreateSupportIncidentEventWorkflowTemplate(incidentEvent, base.CreateWorkflowTemplateWithHeaderLink);
		}

		ProcessTaskTemplate CreateSupportIncidentEventWorkflowTemplate(IIncidentEvent incidentEvent, Func<IIncidentEvent, ProcessTaskTemplate> createTemplateFunction)
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = createTemplateFunction(incidentEvent);
			template.P0_OH_Client = templateOrg.PK;
			return template;
		}

		protected override IIncidentEventConsumer GetNewIncidentForTest()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			return incident;
		}
	}

	public class SupportIncidentEventTest : TestCaseWithFactory
	{
		public void TestTriggerLastEvent_InvalidEvents()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			var incidentEvent = new SupportIncidentGenericEvent(incident, "DDD");
			Factory.Save();

			incident.Logs.AddLog(Events.StatusChange, "Event - " + IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			bool result = SupportIncidentEvent.TriggerLastEvent(incident);
			AssertEquals("eRequest Reopen is not considered as valid last event", false, result);

			incident.Logs.AddLog(Events.StatusChange, "Event - " + IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			result = SupportIncidentEvent.TriggerLastEvent(incident);
			AssertEquals("CargoWise Reopen is not considered as valid last event", false, result);

			incident.Logs.AddLog(Events.StatusChange, "Event - " + IncidentEventFactory.Codes.ChangeCriticality);
			Factory.Save();
			result = SupportIncidentEvent.TriggerLastEvent(incident);
			AssertEquals("Change Criticality is not considered as valid last event", false, result);
		}

		public void TestTriggerLastEvent_ShouldLoadUnsavedLogs()
		{
			const string eventTriggerLogPrefix = "Event - ";
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;

			incident.Logs.AddNew(Events.StatusChange, eventTriggerLogPrefix + IncidentEventFactory.Codes.ChangeProperty);

			Factory.Save();

			var lastLog = SupportIncidentEvent.GetLastEventLog(incident);
			AssertEquals(eventTriggerLogPrefix + IncidentEventFactory.Codes.ChangeProperty, lastLog.SL_Reference);

			incident.Logs.AddNew(Events.StatusChange, eventTriggerLogPrefix + IncidentEventFactory.Codes.DevelopmentEstimateRequested);

			lastLog = SupportIncidentEvent.GetLastEventLog(incident);
			AssertEquals(eventTriggerLogPrefix + IncidentEventFactory.Codes.DevelopmentEstimateRequested, lastLog.SL_Reference);
		}
	}
}
