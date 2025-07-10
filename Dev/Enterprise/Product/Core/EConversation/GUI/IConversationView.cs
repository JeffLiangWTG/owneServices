using System.Windows.Forms;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.EConversation.GUI
{
	public interface IConversationView
	{
		JobConversation Conversation { get; }
		TextBoxBase MessageTextBox { get; }
		ZButton SendButton { get; }
		ZButton AddInternalCommentButton { get; }
		ZButton BroadcastButton { get; }
		Form FindForm();
	}
}
