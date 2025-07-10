using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Xml;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	internal class IncidentManagementEConversationViewController : IConversationViewController
	{
		IConversationView view;
		public static int InputTextMaxLength => 1000;
		static string confirmationMessage => Res.GetString("c0a4899a-c3a7-4192-9819-8272f83b6992", "You are about to send this message to all subscribed recipients and the customer. If you want to send the message to internal staff only, please select No and click Add Internal Comment in eConversation tab page. Do you want to continue to send this message?");
		static string confirmationTitle => Res.GetString("96cf0091-f309-4dd6-b6f3-08106e7c2e39", "Message Confirmation");

		readonly bool warnUponSend;

		public IncidentManagementEConversationViewController(bool warnUponSend = true)
		{
			this.warnUponSend = warnUponSend;
		}

		public void Initialize(IConversationView view)
		{
			InitializeControls(view);
		}

		public void Initialize(IncidentManagementGroup dataSource, IConversationView view)
		{
			InitializeControls(view);
			DataSource = dataSource;
		}

		void InitializeControls(IConversationView view)
		{
			Argument.NotNull(view, nameof(view));
			this.view = view;
			if (SendButton != null)
			{
				SendButton.Enabled = false;
				SendButton.Click += SendButton_Click;
			}
			if (MessageTextBox != null)
			{
				MessageTextBox.TextChanged += MessageTextBox_TextChanged;
			}

			if (AddInternalCommentButton != null)
			{
				AddInternalCommentButton.Enabled = false;
				AddInternalCommentButton.Click += AddInternalCommentButton_Click;
			}

			if (BroadcastButton != null)
			{
				BroadcastButton.Click += BroadcastButton_Click;
			}
		}

		IncidentManagementGroup DataSource {
			get
			{
				return dataSource ?? (dataSource = ((ZForm)RootForm.FindForm()).BusinessEntity as IncidentManagementGroup);
			}

			set
			{
				dataSource = value;
			}
		}

		IncidentManagementGroup dataSource;

		#region Controls

		ZButton SendButton => view.SendButton;

		JobConversation Conversation => view.Conversation;

		TextBoxBase MessageTextBox => view.MessageTextBox;

		ZButton AddInternalCommentButton => view.AddInternalCommentButton;

		ZButton BroadcastButton => view.BroadcastButton;

		ZForm RootForm => rootForm ?? (rootForm = (ZForm)view.FindForm());
		ZForm rootForm;

		#endregion

		#region Control events

		void SendButton_Click(object sender, EventArgs e)
		{
			if (DataSource == null || MessageTextBox.Text.IsNullOrEmpty())
			{
				return;
			}

			var existExternalParticipant = DataSource.EConversation.Conversation.RelatedParties.Any();
			if (warnUponSend && existExternalParticipant)
			{
				if (Globals.Message.Show(confirmationMessage, confirmationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.No)
				{
					return;
				}
			}

			OnSendMessage();
			ClearMessage();
		}

		void AddInternalCommentButton_Click(object sender, EventArgs e)
		{
			if (DataSource == null || MessageTextBox.Text.IsNullOrEmpty())
			{
				return;
			}

			OnSendInternalMessage();
			ClearMessage();
		}

		void BroadcastButton_Click(object sender, EventArgs e)
		{
		}

		void MessageTextBox_TextChanged(object sender, EventArgs e)
		{
			int xmlCharsRemoved;
			string xmlEscapedText = ZXmlValidation.EscapeInvalidXmlCharacters(MessageTextBox.Text, out xmlCharsRemoved);
			if (xmlCharsRemoved > 0)
			{
				var previousSelectionStart = MessageTextBox.SelectionStart;
				MessageTextBox.Text = xmlEscapedText;
				MessageTextBox.SelectionStart = Math.Max(previousSelectionStart - xmlCharsRemoved, 0);
			}

			if (SendButton != null)
			{
				SendButton.Enabled = MessageTextBox.Enabled && !string.IsNullOrWhiteSpace(MessageTextBox.Text);
			}
			if (AddInternalCommentButton != null)
			{
				AddInternalCommentButton.Enabled = MessageTextBox.Enabled && !string.IsNullOrWhiteSpace(MessageTextBox.Text);
			}

			if (MessageTextBox.Enabled && !string.IsNullOrWhiteSpace(MessageTextBox.Text))
			{
				if (!DataSource.HasChanges)
				{
					DataSource.HasChanges = true;
				}
			}

			if (MessageTextBox.Text.Length > InputTextMaxLength)
			{
				MessageTextBox.Text = MessageTextBox.Text.Substring(0, InputTextMaxLength);
				Globals.Message.ShowWarning(Res.GetString("159a04ea-dec6-4d82-afd7-647d802ce6d6", "The message has been truncated to {0} characters.", InputTextMaxLength));
			}
		}

		#endregion

		public void OnSendInternalMessage()
		{
			if (DataSource != null && !string.IsNullOrWhiteSpace(MessageTextBox.Text))
			{
				DataSource.EConversation.AddMessageFromCurrentUser(MessageTextBox.Text, isInternal: true, isSystem: false);
			}
		}

		public void OnSendMessage()
		{
			if (DataSource != null && !string.IsNullOrWhiteSpace(MessageTextBox.Text))
			{
				DataSource.EConversation.AddMessageFromCurrentUser(MessageTextBox.Text, isInternal: false, isSystem: false);
			}
		}

		void ClearMessage()
		{
			if (Conversation != null)
			{
				//EConversationFullControl's MessageTextBox uses JobConversation.NextMessage as data source, so this property also needs to be cleared
				Conversation.NextMessage = ZBlob.Empty;
			}

			MessageTextBox.Clear();
		}
	}
}
