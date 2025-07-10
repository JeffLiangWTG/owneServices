using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.EConversation.Business
{
	public class JobConversationMessageCollection : ActiveBusinessObjectCollection<JobConversationMessage>
	{
		public JobConversationMessageCollection(BusinessObjectFactory factory, JobConversation parent)
			: base(factory, parent, null, JobConversationMessageSchema.JCM_JCC_Conversation)
		{
			ApplySort(JobConversationMessageSchema.JCM_PostedTimeUtc.Name, ListSortDirection.Descending);
		}

		readonly List<JobConversationMessage> broadcastBuffer = new List<JobConversationMessage>();

		public void CopyFromBroadcast(IEnumerable<JobConversationMessage> source)
		{
			foreach (var message in source)
			{
				if (message.JCM_IsInternal || message.JCM_IsSystem)
				{
					continue;
				}

				if (this.Any(x =>
					x.JCM_PostedTimeUtc == message.JCM_PostedTimeUtc &&
					x.JCM_Body == message.JCM_Body &&
					x.JCM_JCP_Participant == message.JCM_JCP_Participant
				))
				{
					continue;
				}

				var copiedMessage = Factory.New<JobConversationMessage>();

				copiedMessage.JCM_Body = JobConversationMessage.TrimBody(message.JCM_Body);
				copiedMessage.JCM_PostedTimeUtc = message.JCM_PostedTimeUtc;
				copiedMessage.JCM_JCP_Participant = message.JCM_JCP_Participant;

				Add(copiedMessage);
				broadcastBuffer.Add(copiedMessage);
			}
		}

		public IEnumerable<JobConversationMessage> GetFromBroadcastBuffer() => this.broadcastBuffer;
		public void ClearBroadcastBuffer() => this.broadcastBuffer.Clear();

		public JobConversationMessage AddNew(JobConversationParticipant sender, string body, bool isInternal = true, bool isSystem = false, bool isBroadcast = false)
		{
			Argument.NotNullOrEmpty(body, nameof(body));

			var newMessage = Factory.New<JobConversationMessage>();
			newMessage.JCM_Body = JobConversationMessage.TrimBody(body);
			newMessage.JCM_IsInternal = isInternal;
			newMessage.JCM_IsSystem = isSystem;
			newMessage.JCM_IsBroadcast = isBroadcast;

			if (sender != null)
			{
				newMessage.JCM_JCP_Participant = sender.PK;
			}

			Add(newMessage);

			return newMessage;
		}
	}
}
