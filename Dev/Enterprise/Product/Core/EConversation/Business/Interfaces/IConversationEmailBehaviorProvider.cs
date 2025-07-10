
namespace Enterprise.EConversation.Business
{
	public interface IConversationEmailBehaviorProvider
	{
		bool ShouldExcludeSender { get; }

		bool ShouldSendEmailFromSender { get; }
	}
}
