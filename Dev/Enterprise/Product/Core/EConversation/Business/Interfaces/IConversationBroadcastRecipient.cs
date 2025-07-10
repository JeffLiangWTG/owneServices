namespace Enterprise.EConversation.Business
{
	public interface IConversationBroadcastRecipient : IConversationProvider
	{
		void GenerateAndSendBroadcastEmailNotifications();

		JobConversation EConversation { get; }
	}
}
