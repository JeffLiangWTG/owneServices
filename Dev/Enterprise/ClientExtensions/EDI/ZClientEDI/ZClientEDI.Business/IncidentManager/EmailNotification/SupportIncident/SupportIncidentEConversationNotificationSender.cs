using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.EConversation.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	///<summary>
	/// This is temporary class that is used to control eConversation notification.
	/// It will be removed in the future
	///</summary>
	public static class SupportIncidentEConversationNotificationSender
	{
		public static void GenerateAndQueueEmailNotifications(BusinessObject businessObject, JobConversation conversation)
		{
			if (SupportIncidentEmailTriggeringRules.AllowGeneratingByCode(businessObject, ConversationCode))
			{
				EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation);
			}
		}

		public static void GenerateAndQueueEmailNotifications(BusinessObject businessObject, JobConversation conversation, JobConversationParticipant sender, IEnumerable<JobConversationMessage> newMessages)
		{
			if (SupportIncidentEmailTriggeringRules.AllowGeneratingByCode(businessObject, ConversationCode))
			{
				EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation, sender, newMessages);
			}
		}

		public static void GenerateAndQueueStaffOnlyBroadcastEmailNotifications(SupportIncident incident, JobConversation conversation, IEnumerable<string> emailsToExclude, IEnumerable<JobConversationMessage> newMessages)
		{
			if (SupportIncidentEmailTriggeringRules.AllowGeneratingByCode(incident, ConversationCode))
			{
				EConversationEmailBuilder.GenerateAndQueueStaffOnlyBroadcastEmailNotifications(conversation, incident, emailsToExclude, newMessages);
			}
		}

		public static bool GenerateStaffNotification(
			BusinessObject businessObject,
			BusinessObjectFactory emailCreationFactory,
			JobConversation convo,
			IEnumerable<JobConversationMessage> newMessages,
			string emailToExclude,
			string formLink = null)
		{
			if (SupportIncidentEmailTriggeringRules.AllowGeneratingByCode(businessObject, ConversationCode))
			{
				return EConversationEmailBuilder.GenerateStaffNotification(emailCreationFactory, convo, newMessages, emailToExclude, formLink);
			}

			return false;
		}

		public static bool GenerateStaffAndContactNotifications(
			BusinessObject businessObject,
			BusinessObjectFactory emailCreationFactory,
			JobConversation convo,
			IEnumerable<JobConversationMessage> newMessages,
			IEnumerable<string> emailsToExclude,
			string staffLink,
			string contactLink)
		{
			if (SupportIncidentEmailTriggeringRules.AllowGeneratingByCode(businessObject, ConversationCode) && !newMessages.IsNullOrEmpty())
			{
				convo.HideStaffName = true;
				return EConversationEmailBuilder.GenerateStaffAndContactNotifications(emailCreationFactory, convo, newMessages, emailsToExclude, staffLink, contactLink);
			}

			return false;
		}

		static string ConversationCode => SupportIncidentEmailTemplateConstants.Codes.IncidentEConversationNotification;
	}
}
