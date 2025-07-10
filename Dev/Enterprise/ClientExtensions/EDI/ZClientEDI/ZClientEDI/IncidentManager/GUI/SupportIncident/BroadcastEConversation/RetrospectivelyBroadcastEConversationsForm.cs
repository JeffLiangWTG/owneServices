using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class RetrospectivelyBroadcastEConversationsForm : ZChildForm
	{
		protected CustomJobConversationMessageUserControl CustomJobConversationMessageUserControl;
		protected CustomSupportIncidentUserControl CustomSupportIncidentUserControl;

		public RetrospectivelyBroadcastEConversationsForm(List<JobConversationMessage> jobConversationMessages, List<SupportIncident> incidents)
			: base()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor; // Set BackColor before calling InitializeComponent() so that child checkboxes inherit the BackColor
			}

			AddCustomJobConversationMessageUserControl(jobConversationMessages);
			AddCustomSupportIncidentUserControl(incidents);

			var closeStripButton = new ZToolStripButton();
			InitialiseButton(closeStripButton, Res.GetString("5fd7730a-5758-457a-9bff-b92a08d41b98", "Close"), Icons.GetImage(IconTypes.BlackWhite_Close), CloseButton_Click);

			var saveStripButton = new ZToolStripButton();
			InitialiseButton(saveStripButton, Res.GetString("03d8d7c0-bc2e-4d85-b25f-1e9e9be8f372", "Send"), Icons.GetImage(IconTypes.BlackWhite_Save), SendButton_Click);

			this.ButtonPanel.AllowOverlap(this.MainGridBottomPanel);
		}

		void AddCustomJobConversationMessageUserControl(List<JobConversationMessage> jobConversationMessages)
		{
			CustomJobConversationMessageUserControl = new CustomJobConversationMessageUserControl(jobConversationMessages);
			CustomJobConversationMessageUserControl.Dock = DockStyle.Fill;
			MainGridTopPanel.Controls.Add(CustomJobConversationMessageUserControl);
		}

		void AddCustomSupportIncidentUserControl(List<SupportIncident> incidents)
		{
			CustomSupportIncidentUserControl = new CustomSupportIncidentUserControl(incidents);
			CustomSupportIncidentUserControl.Dock = DockStyle.Fill;
			MainGridBottomPanel.Controls.Add(CustomSupportIncidentUserControl);
		}

		void InitialiseButton(ToolStripItem button, string text, Image image, EventHandler clickHandler)
		{
			button.Image = image;
			button.ImageScaling = ToolStripItemImageScaling.SizeToFit;
			button.Text = text;
			button.Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0);
			button.AutoToolTip = false;
			button.Alignment = ToolStripItemAlignment.Right;
			button.Click += clickHandler;
			saveToolStrip.Items.Add(button);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		protected void SendButton_Click(object sender, EventArgs e)
		{
			var conversationMessages = CustomJobConversationMessageUserControl.SelectedItems;
			var incidents = CustomSupportIncidentUserControl.SelectedItems;

			if (conversationMessages.Any() && incidents.Any())
			{
				if (SendConfirmationMessage() == DialogResult.Yes)
				{
					var utcNow = DateTime.UtcNow;
					var index = 0;
					var orderedMessages = conversationMessages.OrderBy(c => c.PostedTimeUtc);
					foreach (var incident in incidents)
					{
						foreach (var message in orderedMessages)
						{
							if (!incident.EConversation.Conversation.Messages.Any(m => m.Body.EqualsIgnoringCase(message.Body) && m.Sender == message.Sender))
							{
								var newMessage = incident.EConversation.AddMessageFromCurrentUser(message.Body, message.IsInternal, message.IsSystemMessage, SupportIncidentEConversation.LocalMessageKind.ForCustomer);
								newMessage.JCM_JCP_Participant = message.Sender != null ? message.Sender.PK : ZGuid.Empty;
								newMessage.JCM_IsBroadcast = true;
								newMessage.JCM_PostedTimeUtc = utcNow.AddSeconds(index++);
							}
						}
					}
					Close();
				}
			}
			else
			{
				Globals.Message.Show(
						Res.GetString("075cc4ea-6be9-4b0e-94bd-6ce2c95d4af9", "Please, select an eConversation and an Incident to proceed"),
						Res.GetString("21ab3045-ba35-408f-a2e4-6b5dae775bf1", "Send"),
						MessageBoxButtons.OK, DialogResult.OK);
			}
		}

		protected virtual DialogResult SendConfirmationMessage()
		{
			return Globals.Message.Show(
						Res.GetString("7c5036d4-72d4-429e-8eb7-9238c79c374f", "You are about to send the selected eConversation to the selected incidents. eConversation already sent to the respective incidents will not be resend. Are you sure you want to proceed?"),
						Res.GetString("eccbfaeb-f71e-4c89-b5fd-93778cba3c0c", "Send"),
						MessageBoxButtons.YesNo, DialogResult.No);
		}

		protected void CloseButton_Click(object sender, EventArgs e)
		{
			var shouldClose = DialogResult.Yes;
			var conversationMessages = CustomJobConversationMessageUserControl.SelectedItems;
			var incidents = CustomSupportIncidentUserControl.SelectedItems;

			if (conversationMessages.Any() && incidents.Any())
			{
				shouldClose = Globals.Message.Show(
						Res.GetString("08fd3a4f-9cfc-4e03-8c99-2dfa95b06a7a", "Are you sure you want to close this form?"),
						Res.GetString("b39d18c5-e534-47b2-9216-758de2f86321", "Close"),
						MessageBoxButtons.YesNo, DialogResult.No);
			}

			if (shouldClose == DialogResult.Yes)
			{
				Close();
			}
		}

		public override string FormVerb => string.Empty;
	}
}
