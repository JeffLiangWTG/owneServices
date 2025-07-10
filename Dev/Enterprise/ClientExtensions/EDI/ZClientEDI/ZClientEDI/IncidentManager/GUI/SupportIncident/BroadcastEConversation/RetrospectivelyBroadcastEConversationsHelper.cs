using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public static class RetrospectivelyBroadcastEConversationsHelper
	{
		public static void OpenNewForm(SupportIncident currentIncident, List<SupportIncident> attachedIncidents)
		{
			if (attachedIncidents.Any() && currentIncident != null && currentIncident.RelatedWorkItems.Any())
			{
				var relatedWorkItemWithBroadcastMessages = currentIncident.RelatedWorkItems.Cast<NewWorkItem>().Where(wi =>
																					wi.HasEConversationBroadcastMessages()).ToList();

				OpenNewFormCore(relatedWorkItemWithBroadcastMessages, attachedIncidents);
			}
		}

		public static void OpenNewForm(SupportIncident currentIncident, List<NewWorkItem> attachedWorkitems)
		{
			if (attachedWorkitems.Any() && currentIncident != null)
			{
				var relatedWorkItemWithBroadcastMessages = attachedWorkitems.Where(wi => wi.HasEConversationBroadcastMessages()).ToList();
				OpenNewFormCore(relatedWorkItemWithBroadcastMessages, new List<SupportIncident>() { currentIncident });
			}
		}

		public static void OpenNewForm(NewWorkItem currentWorkItem, List<SupportIncident> attachedItemsCollection)
		{
			if (attachedItemsCollection.Any() && currentWorkItem != null)
			{
				var relatedWorkItemWithBroadcastMessages = currentWorkItem.RelatedItems.Where(r => r is NewWorkItem wi &&
																wi.HasEConversationBroadcastMessages()).Cast<NewWorkItem>().ToList();

				if (currentWorkItem.HasEConversationBroadcastMessages())
				{
					relatedWorkItemWithBroadcastMessages.Add(currentWorkItem);
				}

				OpenNewFormCore(relatedWorkItemWithBroadcastMessages, attachedItemsCollection);
			}
		}

		static void OpenNewFormCore(List<NewWorkItem> relatedWorkItemWithBroadcastMessages, List<SupportIncident> attachedItemsCollection)
		{
			if (relatedWorkItemWithBroadcastMessages.Any() && !IncidentHaveAllMessages(relatedWorkItemWithBroadcastMessages, attachedItemsCollection))
			{
				var jobConversationMessages = new List<JobConversationMessage>();
				foreach (var item in relatedWorkItemWithBroadcastMessages)
				{
					jobConversationMessages.AddRange(item.GetAllEConversationBroadcastMessages());
				}

				if (jobConversationMessages.Any())
				{
					using (var messageForm = new RetrospectivelyBroadcastEConversationsForm(jobConversationMessages, attachedItemsCollection))
					{
						ZFormModaliser.ShowDialogWithoutDispose(messageForm);
					}
				}
			}
		}

		static bool IncidentHaveAllMessages(List<NewWorkItem> relatedWorkItemWithBroadcastMessages, List<SupportIncident> attachedItemsCollection)
		{
			var result = false;

			// Validate if adding one WorkItem to one Incident
			if (relatedWorkItemWithBroadcastMessages.Count == 1 && attachedItemsCollection.Count == 1)
			{
				var incidentMessages = attachedItemsCollection[0].EConversation?.Conversation?.Messages.Where(m => m.JCM_IsBroadcast);
				var workItemMessages = relatedWorkItemWithBroadcastMessages[0].GetAllEConversationBroadcastMessages();

				var diff = workItemMessages.Where(item => !incidentMessages.Any(m => m.JCM_JCP_Participant == item.JCM_JCP_Participant && m.Body == item.Body));
				result = !diff.Any();
			}

			return result;
		}
	}
}
