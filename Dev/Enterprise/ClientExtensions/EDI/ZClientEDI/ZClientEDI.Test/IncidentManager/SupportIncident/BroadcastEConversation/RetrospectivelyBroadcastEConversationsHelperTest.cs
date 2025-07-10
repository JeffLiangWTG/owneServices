using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Test
{
	public class RetrospectivelyBroadcastEConversationsHelperTest : TestCaseWithFactory
	{
		public void TestOpenNewForm_AttachIncidentToIncident()
		{
			var incident = CreateIncidentWithAttachedItems();
			var attachedItemsCollection = new List<SupportIncident>();

			RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(incident, attachedItemsCollection);
			AssertEquals("No attached items", false, ZFormModaliser.LastFormShownDialogForTest is RetrospectivelyBroadcastEConversationsForm);

			var attachedIncident = Factory.NewWithValidTestData<SupportIncident>();
			attachedItemsCollection.Add(attachedIncident);

			RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(incident, attachedItemsCollection);
			AssertEquals(true, ZFormModaliser.LastFormShownDialogForTest is RetrospectivelyBroadcastEConversationsForm);
		}

		public void TestOpenNewForm_AttachIncidentToWorkItem()
		{
			var currentWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			var attachedWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();
			attachedWorkItem.Conversation.Messages.AddNew(null, "broadcast1", false, false, true);
			currentWorkItem.RelatedItems.Add(attachedWorkItem);

			var attachedItemsCollection = new List<SupportIncident>();
			attachedItemsCollection.Add(Factory.NewWithValidTestData<SupportIncident>());

			RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(currentWorkItem, attachedItemsCollection);
			AssertEquals("Current WorkItem has no broadcast messages", false, ZFormModaliser.LastFormShownDialogForTest is RetrospectivelyBroadcastEConversationsForm);

			currentWorkItem.Conversation.Messages.AddNew(null, "broadcast test", false, false, true);

			RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(currentWorkItem, attachedItemsCollection);
			AssertEquals(true, ZFormModaliser.LastFormShownDialogForTest is RetrospectivelyBroadcastEConversationsForm);
		}

		public void TestOpenNewForm_AttachWorkItemToIncident()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var attachedItemsCollection = new List<NewWorkItem>();
			var workitem = Factory.NewWithValidTestData<NewWorkItem>();
			attachedItemsCollection.Add(workitem);
			Factory.Save();

			RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(incident, attachedItemsCollection);
			AssertEquals("Attached WorkItem has no broadcast message", false, ZFormModaliser.LastFormShownDialogForTest is RetrospectivelyBroadcastEConversationsForm);

			workitem.Conversation.Messages.AddNew(null, "Broadcast message", false, false, true);

			RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(incident, attachedItemsCollection);
			AssertEquals(true, ZFormModaliser.LastFormShownDialogForTest is RetrospectivelyBroadcastEConversationsForm);
		}

		public void TestIncidentHaveAllMessages()
		{
			var currentWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			var attachedWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();
			attachedWorkItem.Conversation.Messages.AddNew(null, "broadcast1", false, false, true);
			currentWorkItem.RelatedItems.Add(attachedWorkItem);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var attachedItemsCollection = new List<SupportIncident>();
			attachedItemsCollection.Add(incident);

			var incidentMsg1 = incident.EConversation.Conversation.AddMessageFromCurrentUser("any broadcast message", false, false, true);
			incidentMsg1.JCM_JCP_Participant = ZGuid.Empty;

			var message1 = currentWorkItem.Conversation.Messages.AddNew(null, "broadcast test", false, false, true);

			ZFormModaliser.LastFormShownDialogForTest = null;
			RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(currentWorkItem, attachedItemsCollection);
			AssertEquals(true, ZFormModaliser.LastFormShownDialogForTest is RetrospectivelyBroadcastEConversationsForm);

			var message2 = currentWorkItem.Conversation.Messages.AddNew(null, "broadcast test 2", false, false, true);
			var incidentMsg2 = incident.EConversation.Conversation.AddMessageFromCurrentUser(message1.Body, message1.JCM_IsInternal, message1.JCM_IsSystem, true);
			incidentMsg2.JCM_JCP_Participant = ZGuid.Empty;

			ZFormModaliser.LastFormShownDialogForTest = null;
			RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(currentWorkItem, attachedItemsCollection);
			AssertEquals("Show form because WorkItem has a new message", true, ZFormModaliser.LastFormShownDialogForTest is RetrospectivelyBroadcastEConversationsForm);

			var incidentMsg3 = incident.EConversation.Conversation.AddMessageFromCurrentUser(message2.Body, message2.JCM_IsInternal, message2.JCM_IsSystem, true);
			incidentMsg3.JCM_JCP_Participant = ZGuid.Empty;

			ZFormModaliser.LastFormShownDialogForTest = null;
			RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(currentWorkItem, attachedItemsCollection);
			AssertEquals("Incident already have all the messages, should no open", false, ZFormModaliser.LastFormShownDialogForTest is RetrospectivelyBroadcastEConversationsForm);
		}

		SupportIncident CreateIncidentWithAttachedItems()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();
			workItem.Conversation.Messages.AddNew(null, "broadcast1", false, false, true);
			incident.RelatedItems.Add(workItem);
			return incident;
		}
	}
}
