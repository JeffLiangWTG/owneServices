using Enterprise.MasterFiles.Integration;

namespace Enterprise.EConversation.Business
{
	public interface IConversationParentHyperlinkProvider
	{
		bool ShouldUseThisProviderForHyperlink(IConversationParticipant participant = null);
		string GetHyperlinkToConversationParent();
	}
}
