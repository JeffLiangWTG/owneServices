using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.EConversation.ServiceTasks
{
	public class BusinessObjectEConversationAttacher
	{
		public JobConversationMessage AttachEmailToJobConversation(Email mailAsEDoc, string docType, IConversationProvider provider)
		{
			JobConversation convo = FindEConversation(provider);
			JobConversationMessage newMessage = null;
			if (mailAsEDoc != null)
			{
				if (provider is IAllowAttachEmailsToEDocs allowEmail)
				{
					var processResult = BusinessObjectEmailAttacher.ProcessAttachments(allowEmail, mailAsEDoc, docType, this);
					var sender = AddOrGetSender(mailAsEDoc, convo);
					newMessage = AddMessages(sender, processResult, convo);
					OnAttachedEmailToJobConversation(mailAsEDoc, convo);
				}
			}
			return newMessage;
		}

		public virtual void OnAttachedEmailToJobConversation(Email mailAsEDoc, JobConversation convo)
		{
		}

		public virtual JobConversation FindEConversation(IConversationProvider provider)
		{
			if (provider == null)
			{
				return null;
			}

			return provider.eConversation ?? JobConversation.GetOrCreate(provider as BusinessObject);
		}

		JobConversationMessage AddMessages(JobConversationParticipant sender, BusinessObjectEmailAttacher.ConversationMessages processResult, JobConversation convo)
		{
			JobConversationMessage newMessage = null;
			if (!string.IsNullOrEmpty(processResult.EmailBodyMessage))
			{
				newMessage = convo.Messages.AddNew(sender, processResult.EmailBodyMessage, isInternal: false, isSystem: false);
				newMessage.JCM_IsLocal = false;
			}

			if (!string.IsNullOrEmpty(processResult.AttachmentMessage))
			{
				convo.Messages.AddNew(sender, processResult.AttachmentMessage, isInternal: true, isSystem: true);
			}
			return newMessage;
		}

		JobConversationParticipant AddOrGetSender(Email mailAsEDoc, JobConversation convo)
		{
			var sender = mailAsEDoc.SenderAddress;
			if (!string.IsNullOrEmpty(sender))
			{
				var existingParticipant = convo.Participants
					.FirstOrDefault(p => p.EmailAddress.EqualsIgnoringCase(sender));

				if (existingParticipant != null)
				{
					return existingParticipant;
				}

				return AddParticipantFromEmail(sender, convo.Participants, convo);
			}

			return null;
		}

		protected virtual JobConversationParticipant AddParticipantFromEmail(string sender, JobConversationParticipantCollection participants, JobConversation eConvo)
		{
			return participants.AddNewParticipant(sender);
		}
	}
}
