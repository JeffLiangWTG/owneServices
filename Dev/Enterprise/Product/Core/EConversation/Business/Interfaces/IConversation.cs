using System.Collections.Generic;

namespace Enterprise.EConversation.Business
{
	public interface IConversation
	{
		bool IsEmpty { get; }
		bool HasChanges { get; }

		IList<IConversationMessage> GetTimeOrderedMessages();
		bool AnyLocalMessageContains(string text);
		void Reload();
	}
}
