using System.Collections.Generic;
using System.Linq;

namespace Enterprise.EConversation.Business
{
	public static class EConversationMessageProvider
	{
		public static IEnumerable<JobConversationMessage> GetNewMessages(JobConversation conversation, bool includeInternal)
		{
			if (conversation.Messages.GetFromBroadcastBuffer().Any())
			{
				return conversation.Messages.GetFromBroadcastBuffer();
			}

			return conversation.Messages
				.Where(message =>
					!message.IsInDatabase
					&& (includeInternal || !message.JCM_IsInternal)
					&& !message.IsSystemMessage)
				.OrderByDescending(message => message.JCM_PostedTimeUtc);
		}

		public static IEnumerable<JobConversationMessage> GetOldMessages(JobConversation conversation, bool includeInternal, int numberOfPreviousMessagesToInclude, IEnumerable<IConversationMessage> newMessages)
		{
			return conversation.Messages
				.Where(message => !message.IsSystemMessage
					&& (includeInternal || !message.JCM_IsInternal)
					&& !newMessages.Any(newMsg => newMsg.Id == message.PK || newMsg.SystemCreateTimeInUtc < message.SystemCreateTimeInUtc))
				.OrderByDescending(message => message.JCM_PostedTimeUtc)
				.Take(numberOfPreviousMessagesToInclude);
		}
	}
}
