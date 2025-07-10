namespace Enterprise.EConversation.GUI
{
	public interface IConversationViewController
	{
		void Initialize(IConversationView view);
		void OnSendMessage();
		void OnSendInternalMessage();
	}
}
