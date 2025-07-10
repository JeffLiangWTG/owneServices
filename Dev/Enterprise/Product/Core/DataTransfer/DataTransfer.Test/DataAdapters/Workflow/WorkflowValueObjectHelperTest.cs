using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class WorkflowValueObjectHelperTest : TestCaseWithFactory
	{
		public void TestImportFromValueObject()
		{
			Xsd.Workflow xmlWorkflow = new Xsd.Workflow();
			Xsd.WorkflowTrigger xmlTrigger = xmlWorkflow.Triggers.AddNew();
			xmlTrigger.Description = "TEST TRIGGER";
			xmlTrigger.DescriptionSpecified = true;
			Xsd.WorkflowTriggerEvent xmlTriggerEvent = xmlTrigger.Item;
			xmlTriggerEvent.CodeSpecified = true;
			xmlTriggerEvent.Code = "DIM";
			xmlTriggerEvent.ReferenceSpecified = true;
			xmlTriggerEvent.Reference = "REFERENCE";
			xmlTriggerEvent.Actions.IsSpecified = true;
			Xsd.WorkflowTriggerAction xmlTriggerAction = xmlTriggerEvent.Actions.AddNew();
			xmlTriggerAction.EmailAddressSpecified = true;
			xmlTriggerAction.EmailAddress = "dummy@test.com";
			xmlTriggerAction.EmailHtmlContentSpecified = true;
			xmlTriggerAction.EmailHtmlContent = "Hello World";
			xmlTriggerAction.PurposeSpecified = true;
			xmlTriggerAction.Purpose = Xsd.WorkflowTriggerActionPurpose.APP;
			xmlTriggerAction.Recipient = Xsd.WorkflowTriggerActionRecipient.EML;
			xmlTriggerAction.Type = Xsd.WorkflowTriggerActionType.NTF;

			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			WorkflowValueObjectHelper helper = new WorkflowValueObjectHelper();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			helper.ImportFromValueObject(xmlWorkflow, opp.WorkflowItems, context);

			AssertEquals(1, opp.WorkflowItems.Triggers.Count);
			ProcessTask trigger = opp.WorkflowItems.Triggers[0];
			AssertEquals("TEST TRIGGER", trigger.P9_Description);
			AssertEquals("DIM", trigger.P9_SE_NKMilestoneEvent);
			AssertEquals(EventReferenceConditionList.Codes.EventReference, trigger.P9_TriggerCondition);
			AssertEquals("REFERENCE", trigger.P9_TriggerConditionValue);
			AssertEquals(1, trigger.ProcessTaskNotifications.Count);
			ProcessTaskNotification triggerAction = trigger.ProcessTaskNotifications[0];
			AssertEquals(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, triggerAction.PQ_TriggerType);
			AssertEquals(MessageRecipientPartyTypeList.Codes.Email, triggerAction.PQ_TriggerParty);
			AssertEquals("APP", triggerAction.PQ_MessagePurpose);
			AssertEquals("dummy@test.com", triggerAction.PQ_EmailAddr);
			AssertEquals(true, triggerAction.OverrideEmail);
			AssertEquals("Hello World", triggerAction.PQ_EmailText);
		}

		public void TestExportToValueObject()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			ProcessTask trigger = opp.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TEST TRIGGER";
			trigger.TriggerConditions.TriggerEventCode = Events.DataImport.Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger.TriggerConditions.TriggerConditionValue = "REFERENCE";
			ProcessTaskNotification triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_MessagePurpose = "APP";
			triggerAction.PQ_EmailAddr = "dummy@email.com";
			triggerAction.OverrideEmail = true;
			triggerAction.PQ_EmailText = "Hello World";

			WorkflowValueObjectHelper helper = new WorkflowValueObjectHelper();
			NotificationBuffer buffer = new NotificationBuffer();
			Xsd.Workflow xmlWorkflow = new Xsd.Workflow();
			xmlWorkflow.IsSpecified = false;
			helper.ExportToValueObject(null, xmlWorkflow, buffer);
			AssertEquals(false, xmlWorkflow.IsSpecified);
			AssertEquals(0, xmlWorkflow.Triggers.Count);
			AssertEquals(false, xmlWorkflow.Triggers.IsSpecified);

			helper.ExportToValueObject(opp.WorkflowItems, xmlWorkflow, buffer);
			AssertEquals(true, xmlWorkflow.IsSpecified);
			AssertEquals(true, xmlWorkflow.Triggers.IsSpecified);
			AssertEquals(1, xmlWorkflow.Triggers.Count);

			Xsd.WorkflowTrigger xmlTrigger = xmlWorkflow.Triggers[0];
			AssertEquals(true, xmlTrigger.IsSpecified);
			AssertEquals("TEST TRIGGER", xmlTrigger.Description);
			AssertEquals(true, xmlTrigger.DescriptionSpecified);

			AssertEquals(true, xmlTrigger.ItemSpecified);
			Xsd.WorkflowTriggerEvent xmlTriggerEvent = xmlTrigger.Item;
			AssertEquals(true, xmlTriggerEvent.IsSpecified);
			AssertEquals(true, xmlTriggerEvent.CodeSpecified);
			AssertEquals(Events.DataImport.Code, xmlTriggerEvent.Code);
			AssertEquals(true, xmlTriggerEvent.ReferenceSpecified);
			AssertEquals("REFERENCE", xmlTriggerEvent.Reference);
			AssertEquals(true, xmlTriggerEvent.Actions.IsSpecified);
			AssertEquals(1, xmlTriggerEvent.Actions.Count);

			Xsd.WorkflowTriggerAction xmlTriggerAction = xmlTriggerEvent.Actions[0];
			AssertEquals(true, xmlTriggerAction.IsSpecified);
			AssertEquals(true, xmlTriggerAction.EmailAddressSpecified);
			AssertEquals("dummy@email.com", xmlTriggerAction.EmailAddress);
			AssertEquals(true, xmlTriggerAction.EmailHtmlContentSpecified);
			AssertEquals("Hello World", xmlTriggerAction.EmailHtmlContent);
			AssertEquals(true, xmlTriggerAction.PurposeSpecified);
			AssertEquals(Xsd.WorkflowTriggerActionPurpose.APP, xmlTriggerAction.Purpose);
			AssertEquals(Xsd.WorkflowTriggerActionRecipient.EML, xmlTriggerAction.Recipient);
			AssertEquals(Xsd.WorkflowTriggerActionType.NTF, xmlTriggerAction.Type);
		}
	}
}
