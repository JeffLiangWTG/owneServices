using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Xml;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class IncidentConversationViewController : IConversationViewController
	{
		public IncidentConversationViewController(bool warnUponSend = false)
		{
			WarnUponSend = warnUponSend;
		}
		/// <summary>
		/// For the generic eConversation plugin tab which only has a text box and "send" button.
		/// </summary>
		public void Initialize(IConversationView conversationView)
		{
			this.view = conversationView;
			this.textBox = conversationView.MessageTextBox;
			this.sendButton = conversationView.SendButton;
			sendButton.Click += SendButton_Click;
			textBox.TextChanged += ConversationMessageTextBox_TextChanged;
		}

		/// <summary>
		/// For the incident main tab which also has "awaiting response" button
		/// </summary>
		public void Initialize(SupportIncidentForm incidentForm)
		{
			Initialize((IConversationView)incidentForm);

			spellChecker = incidentForm.spellChecker;
			awaitingResponseButton = incidentForm.AwaitingResponseButton;
			if (Incident.IM_Priority.Equals(Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest))
			{
				awaitingResponseButton.Image = global::Enterprise.Client.EDI.IncidentManager.GUI.Properties.Resources.arrow_drop1;
				awaitingResponseButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			}
			awaitingResponseButton.Click += AwaitingResponseButton_Click;
		}

		IConversationView view;
		TextBoxBase textBox;
		ZButton sendButton;
		ZButton awaitingResponseButton;
		SpellChecker spellChecker;
		bool WarnUponSend { get; }
		protected virtual SupportIncident Incident => (SupportIncident)Form.BusinessEntity;

		ZForm Form => form ?? (form = (ZForm)view.FindForm());
		ZForm form;

		void SendButton_Click(object sender, EventArgs e)
		{
			SendMessage();
		}

		public void OnSendMessage()
		{
			SendMessage();
		}

		public void OnSendInternalMessage()
		{ }

		void ConversationMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			int xmlCharsRemoved;
			string xmlEscapedText = ZXmlValidation.EscapeInvalidXmlCharacters(textBox.Text, out xmlCharsRemoved);
			if (xmlCharsRemoved > 0)
			{
				var previousSelectionStart = textBox.SelectionStart;
				textBox.Text = xmlEscapedText;
				textBox.SelectionStart = Math.Max(previousSelectionStart - xmlCharsRemoved, 0);
			}

			sendButton.Enabled = textBox.Enabled && !string.IsNullOrWhiteSpace(textBox.Text);
			if (awaitingResponseButton != null && textBox.Enabled && !string.IsNullOrWhiteSpace(textBox.Text))
			{
				awaitingResponseButton.Enabled = true;
			}

			if (textBox.Enabled && !string.IsNullOrWhiteSpace(textBox.Text))
			{
				var incident = Incident;
				if (!incident.HasChanges)
				{
					incident.HasChanges = true;
				}

				if (!incident.EConversation.Conversation.HasChanges)
				{
					incident.EConversation.Conversation.HasChanges = true;
				}
			}

			if (textBox.Text.Length > MaxTextLength)
			{
				textBox.Text = textBox.Text.Substring(0, MaxTextLength);
				Globals.Message.ShowWarning(Res.GetString("0eb8280f-f509-48b8-8672-451b24851b30", "The message has been truncated to {0} characters.", MaxTextLength));
			}
		}

		void AwaitingResponseButton_Click(object sender, EventArgs e)
		{
			if (Incident.IM_Priority.Equals(Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest))
			{
				AwaitingCustomerResponseMenuStrip.Show(awaitingResponseButton, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), ToolStripDropDownDirection.BelowLeft);
			}
			else
			{
				AwaitingResponse();
			}
		}

		public KContextMenuStrip AwaitingCustomerResponseMenuStrip
		{
			get
			{
				if (awaitingCustomerResponseMenuStrip == null)
				{
					awaitingCustomerResponseMenuStrip = new KContextMenuStrip();
					awaitingCustomerResponseMenuStrip.Items.Add(new ZToolStripMenuItem(Res.GetString("d3d27b15-77f4-4d10-a2c4-1630c41df36f", "Development Estimate Provided - Awaiting Customer Response"), DEPToolStripMenuItem_Click));
					awaitingCustomerResponseMenuStrip.Items.Add(new ZToolStripMenuItem(Res.GetString("345cafb0-bcd6-4363-9378-6646bb437ecf", "Formal Quotation Provided - Awaiting Customer Response"), FQPToolStripMenuItem_Click));
					awaitingCustomerResponseMenuStrip.Items.Add(new ZToolStripMenuItem(Res.GetString("a20eee02-d524-491f-8e25-b82d71bff06e", "Awaiting Customer Response"), ACRToolStripMenuItem_Click));
				}

				return awaitingCustomerResponseMenuStrip;
			}
		}

		protected KContextMenuStrip awaitingCustomerResponseMenuStrip;

		void DEPToolStripMenuItem_Click(object sender, EventArgs e)
		{
			spellChecker?.CheckSpelling();
			var result = SendMessage();
			var incident = Incident;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided;
			if (result)
			{
				incident.CloseIncident(incident.IM_ResolutionCode, "", true);
			}
		}

		void FQPToolStripMenuItem_Click(object sender, EventArgs e)
		{
			spellChecker?.CheckSpelling();
			var result = SendMessage();
			var incident = Incident;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided;
			if (result)
			{
				incident.CloseIncident(incident.IM_ResolutionCode, "", true);
			}
		}

		void ACRToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AwaitingResponse();
		}

		internal bool AwaitingResponse(string messageOverride = null)
		{
			spellChecker?.CheckSpelling();

			var result = SendMessage(messageOverride);

			var incident = Incident;
			var disposition = incident.ShouldKeepCurrentDispositionWhileAwaitingResponse
							? incident.IM_ResolutionCode
							: (ZString)SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			incident.CloseIncident(disposition, "", true);
			incident.SuspendTriggerCloseIncident = true;
			return result;
		}

		const int MaxTextLength = 10000;

		internal bool SendMessage(string messageOverride = null)
		{
			var message = !string.IsNullOrWhiteSpace(messageOverride) ? messageOverride : textBox.Text;
			if (string.IsNullOrWhiteSpace(message))
			{
				return false;
			}

			var confirmationMessage = Res.GetString("5E9C72E7-79B4-4E60-899E-7C9DA8A92506", "You are about to send this message to all subscribed recipients and the customer. If you want to send the message to internal staff only, please click No and add an Internal Log from the Action Menu. Do you want to continue to send this message?");
			var confirmationTitle = Res.GetString("E8D4F961-B74F-401F-91B6-E1BCE4A254D7", "Message Confirmation");
			if (WarnUponSend && Globals.Message.Show(confirmationMessage, confirmationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.No)
			{
				return false;
			}

			ClearMessage();

			var incident = Incident;
			incident.AddStaffMessageToCustomer(message);
			incident.CreateClientCommunicationTaskIfNeeded();
			return true;
		}

		internal void ClearMessage()
		{
			var conversation = view.Conversation;
			if (conversation != null)
			{
				conversation.NextMessage = ZBlob.Empty;
				conversation.NextMessagePlainText = ZString.Empty;
			}

			textBox.Clear();
		}

		internal TextBoxBase TextBoxForTest => textBox;
		internal KContextMenuStrip AwaitingCustomerResponseMenuStripForTest => AwaitingCustomerResponseMenuStrip;
	}
}
