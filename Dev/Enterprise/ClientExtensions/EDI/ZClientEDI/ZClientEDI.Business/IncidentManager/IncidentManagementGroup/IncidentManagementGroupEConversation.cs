using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupEConversation : IConversation
	{
		public IncidentManagementGroupEConversation(IncidentManagementGroup incidentManagementGroup)
		{
			IncidentManagementGroup = incidentManagementGroup;
		}

		readonly IncidentManagementGroup IncidentManagementGroup;

		public JobConversation Conversation => conversation ?? (conversation = (LoadConversation() ?? CreateConversation(IncidentManagementGroup)));
		JobConversation conversation;

		JobConversation LoadConversation()
		{
			JobConversation conversation = null;
			if (!IncidentManagementGroup.PK.IsEmpty && !isLoaded)
			{
				isLoaded = true;
				conversation = IncidentManagementGroup.Factory.LoadTop1<JobConversation>(new ZQuery(JobConversationSchema.JCC_ParentID, IncidentManagementGroup.PK));
			}
			if (conversation != null)
			{
				IncidentManagementGroup.RegisterEditableChildObject(conversation);
			}
			return conversation;
		}
		bool isLoaded;

		static JobConversation CreateConversation(IncidentManagementGroup incidentManagementGroup)
		{
			if (!incidentManagementGroup.IsInDatabase)
			{
				if (!incidentManagementGroup.PK.IsValid && incidentManagementGroup.PK.IsEmpty)
				{
					throw new InvalidOperationException("Group's PK is empty or invalid");
				}
				var conversation = incidentManagementGroup.Factory.New<JobConversation>();
				using (conversation.SuspendSettingHasChanges())
				{
					conversation.JCC_ParentID = incidentManagementGroup.PK;
					conversation.JCC_ParentTableCode = IncidentManagementGroupSchema.Constants.Prefix;
				}
				incidentManagementGroup.RegisterEditableChildObject(conversation);
				return conversation;
			}
			return JobConversation.GetOrCreate(incidentManagementGroup);
		}

		readonly List<JobConversationMessage> newLocalMessages = new List<JobConversationMessage>();
		readonly List<JobConversationMessage> newMessages = new List<JobConversationMessage>();

		public event EventHandler MessageCountChanged;
		bool isPendingMessageCountChanged;

		void OnMessageCountChanged()
		{
			// Delay notifications if in a transaction.
			// The ActiveBusinessCollection will not be updated until after the save completes.
			if (!IncidentManagementGroup.Factory.IsInTransaction)
			{
				isPendingMessageCountChanged = false;
				MessageCountChanged?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				isPendingMessageCountChanged = true;
			}
		}

		public JobConversationMessage AddMessageFromCurrentUser(ZString comment, bool isInternal, bool isSystem)
		{
			if (Conversation != null)
			{
				ZQuery zQuery = new ZQuery();
				zQuery.AddToFilter(Conversation.Participants.CompleteFilter);
				zQuery.ReLoadExistingRows = false;
				Conversation.Participants.Factory.Load<JobConversationParticipant>(zQuery);
				ActiveBusinessObjectCollection.RefreshAll(typeof(JobConversationParticipant), Conversation.Participants.Factory);
			}

			var msg = Conversation.AddMessageFromCurrentUser(comment, isInternal);

			msg.JCM_IsSystem = isSystem;
			if (!msg.Sender.IsInDatabase)
			{
				msg.Sender.JCP_IsSubscribed = false;
			}

			newLocalMessages.Add(msg);
			newMessages.Add(msg);
			OnMessageCountChanged();

			return msg;
		}

		public void OnIncidentManagementGroupSaveSucceed()
		{
			newLocalMessages.Clear();
			newMessages.Clear();
			if (isPendingMessageCountChanged)
			{
				OnMessageCountChanged();
			}
		}

		public IEnumerable<JobConversationMessage> GetNewLocalPublishedCustomerMessages()
		{
			foreach (var msg in newLocalMessages)
			{
				if (!msg.IsDeleted)
				{
					yield return msg;
				}
			}
		}

		#region IConversation

		bool IConversation.IsEmpty => Conversation?.IsEmpty ?? false;

		bool IConversation.HasChanges => (Conversation?.HasChanges ?? false);

		bool IConversation.AnyLocalMessageContains(string text)
		{
			return (Conversation?.AnyLocalMessageContains(text) ?? false);
		}

		IList<IConversationMessage> IConversation.GetTimeOrderedMessages()
		{
			return Conversation?.GetTimeOrderedMessages() ?? Array.Empty<IConversationMessage>();
		}

		void IConversation.Reload()
		{
			Conversation?.Reload();
		}

		#endregion
	}
}
