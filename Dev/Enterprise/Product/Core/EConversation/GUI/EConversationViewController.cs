using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.EConversation.GUI
{
	public class EConversationViewController : IConversationViewController
	{
		public void Initialize(IConversationView conversationView)
		{
			this.view = conversationView;
			view.SendButton.Click += SendButton_Click;

			if (view.AddInternalCommentButton != null)
			{
				view.AddInternalCommentButton.Click += AddInternalCommentButton_Click;
			}

			if (view.BroadcastButton != null)
			{
				view.BroadcastButton.Click += BroadcastButton_Click;
			}
		}

		IConversationView view;

		void SendButton_Click(object sender, EventArgs e)
		{
			OnSendMessage();
		}

		public void OnSendMessage()
		{
			SubmitMessage(isInternal: false);
		}

		void AddInternalCommentButton_Click(object sender, EventArgs e)
		{
			OnSendInternalMessage();
		}

		void BroadcastButton_Click(object sender, EventArgs e)
		{
			SubmitMessage(isInternal: false, true);
		}

		public void OnSendInternalMessage()
		{
			SubmitMessage(isInternal: true);
		}

		void SubmitMessage(bool isInternal, bool isBroadcast = false)
		{
			var textBox = view.MessageTextBox;
			var text = textBox.Text;

			if (view.Conversation == null)
			{
				Globals.Message.ShowWarning(Res.GetString("3c960e92-38c0-4775-8d7a-0e790a21e131", "Please save the form before using eConversation."));
			}
			else if (string.IsNullOrWhiteSpace(text))
			{
				Globals.Message.ShowWarning(Res.GetString("dda90152-f73d-404b-8791-1a5ce2f6fb2c", "Please enter a message to send"));
			}
			else
			{
				view.Conversation.AddMessageFromCurrentUser(text, isInternal: isInternal, false, isBroadcast);
				view.Conversation.NextMessage = ZBlob.Empty;
				textBox.Clear();
			}
		}
	}
}
