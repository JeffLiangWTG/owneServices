using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.EConversation.GUI
{
	public partial class EConversationMessageListUserControl : ZUserControl
	{
		public enum ViewMode
		{
			AllMessages,
			UserMessagesOnly
		}

		#region Constructors

		public EConversationMessageListUserControl()
		{
			InitializeComponent();
			this.messagesLayoutPanel.Initialized = true;
			allMessagesRadioButton.Text = Enterprise.EConversation.GUI.Res.GetString("a4bcd42e-5136-46fe-b5e9-65e0a9a63115", "All Messages");
			userMessagesRadioButton.Text = Enterprise.EConversation.GUI.Res.GetString("c1b0bbdb-d01f-4a21-b248-1fec34b2e4de", "eConversation Only");
		}

		public EConversationMessageListUserControl(bool isClientSystem, bool hideViewModeOptions)
			: this(isClientSystem)
		{
			if (hideViewModeOptions)
			{
				viewTogglePanel.Visible = false;
				ControlDpiScalingHelper.SetHeight(messagesLayoutPanel, ClientSize.Height, false);
			}
		}

		public EConversationMessageListUserControl(bool isClientSystem)
			: this()
		{
			this.IsClientSystem = isClientSystem;
			currentViewMode = ViewMode.AllMessages;
		}

		#endregion

		protected bool IsClientSystem { get; }
		ViewMode currentViewMode;

		#if !WINZOR

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				// Turn on WS_EX_COMPOSITED to solve textbox flicking issue when resizing
				// This style causes the entire control and all descendants to be double buffered
				// As opposed to the DoubleBuffer property, which does not double buffer descendants. 
				cp.ExStyle |= 0x02000000;
				return cp;
			}
		}

		#endif

		protected IConversation EConversation
		{
			get { return (IConversation)CurrentDataItem; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (EConversation != null)
			{
				messagesLayoutPanel.SuspendLayout();
				BuildMessageControls();
				messagesLayoutPanel.ResumeLayout();
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			RefreshMessages();
		}

		public bool ForceSilentRefreshMessages { get; set; }

		public virtual void RefreshMessages(bool silentRefreshMessage = false)
		{
			if (EConversation != null && Visible)
			{
				messagesLayoutPanel.SuspendLayout();
				BuildMessageControls();
				FilterMessageControls();
				if (!ForceSilentRefreshMessages && !silentRefreshMessage)
				{
					messagesLayoutPanel.Focus();
				}
				messagesLayoutPanel.VerticalScroll.Value = 0;
				messagesLayoutPanel.ResumeLayout();
			}
		}

		#region Build Message Controls

		protected virtual void BuildMessageControls()
		{
			Dictionary<ZGuid, ConversationMessageUserControl> existingMessageControlsMap = new Dictionary<ZGuid, ConversationMessageUserControl>(messagesLayoutPanel.Controls.Count);
			Dictionary<string, int> existingDateLabelsMap = new Dictionary<string, int>();
			PopulateExistingControlsMaps(existingMessageControlsMap, existingDateLabelsMap);
			PopulateMessages(existingMessageControlsMap, existingDateLabelsMap);
		}

		protected void PopulateMessages(Dictionary<ZGuid, ConversationMessageUserControl> existingMessageControlsMap = null, Dictionary<string, int> existingDateLabelsMap = null)
		{
			var messages = GetOrderedMessages();
			int controlIndex = 0;
			IConversationMessage lastMessage = null;

			foreach (var message in messages)
			{
				if (lastMessage == null || message.SendLocalDateTime.Date != lastMessage.SendLocalDateTime.Date)
				{
					var dateText = (message.SendLocalDateTime.Date.IsEmpty || !message.SendLocalDateTime.Date.IsValid)
					? Enterprise.EConversation.GUI.Res.GetString("7450831e-5a28-4765-95f9-992c8c37a3d6", "No Date")
					: message.SendLocalDateTime.Date.ToDateTime().ToLongDateString();
					AddDateLabel(controlIndex, dateText, existingDateLabelsMap);
					controlIndex++;
				}

				AddMessageControl(controlIndex, lastMessage, message, existingMessageControlsMap);
				lastMessage = message;
				controlIndex++;
			}
		}

		protected virtual IList<IConversationMessage> GetOrderedMessages()
		{
			return EConversation.GetTimeOrderedMessages();
		}

		void PopulateExistingControlsMaps(Dictionary<ZGuid, ConversationMessageUserControl> existingMessageControlsMap, Dictionary<string, int> existingDateLabelsMap)
		{
			int dateLabelIndex = 0;
			foreach (Control control in messagesLayoutPanel.Controls)
			{
				ConversationMessageUserControl messageControl = control as ConversationMessageUserControl;
				if (messageControl != null && !existingMessageControlsMap.ContainsKey(messageControl.Message.Id))
				{
					existingMessageControlsMap.Add(messageControl.Message.Id, messageControl);
				}
				else
				{
					Label dateLabel = control as Label;
					if (dateLabel != null && !existingDateLabelsMap.ContainsKey(dateLabel.Text))
					{
						existingDateLabelsMap.Add(dateLabel.Text, dateLabelIndex);
						dateLabelIndex++;
					}
				}
			}
			FixCustomisedHyperlinks();
		}

		protected virtual void AddDateLabel(int controlIndex, string dateText, Dictionary<string, int> existingDateLabelsMap = null)
		{
			if (existingDateLabelsMap != null && !existingDateLabelsMap.ContainsKey(dateText))
			{
				AddDateLabelCore(controlIndex, dateText);
			}
		}

		protected void AddDateLabelCore(int controlIndex, string dateText)
		{
			var dateLabel = new ZLabel()
			{
				CaptionResourceString = new CargoWiseOne.ResourceStrings.ResourceStringData("7c01e301-cbe8-496c-9f43-7c8c0bdfd8dd", dateText),
				ForeColor = ColorTranslator.FromHtml("#555555"),
				Padding = ControlDpiScalingHelper.NewScaledPadding(5, 10, 0, 7),
				Font = new Font(Font.Name, 8, FontStyle.Bold)
			};
			dateLabel.MouseClick += (s, e) => { messagesLayoutPanel.Focus(); };
			dateLabel.MouseDoubleClick += (s, e) => { messagesLayoutPanel.Focus(); };
			messagesLayoutPanel.Controls.Add(dateLabel);
			messagesLayoutPanel.Controls.SetChildIndex(dateLabel, controlIndex);
		}

		protected virtual void AddMessageControl(int controlIndex, IConversationMessage lastMessage, IConversationMessage message, Dictionary<ZGuid, ConversationMessageUserControl> existingMessageControlsMap = null)
		{
			ConversationMessageUserControl messageControl;
			if (existingMessageControlsMap != null && !existingMessageControlsMap.ContainsKey(message.Id))
			{
				messageControl = GetNewMessageControl(controlIndex, message);
			}
			else
			{
				messageControl = existingMessageControlsMap[message.Id];
				messageControl.DeactivateRatingControlForOutdatedMessage();
			}

			SetLabelVisable(messageControl, lastMessage);
		}

		protected ConversationMessageUserControl GetNewMessageControl(int controlIndex, IConversationMessage message)
		{
			ConversationMessageUserControl messageControl = new ConversationMessageUserControl(message, IsClientSystem);
			messageControl.CaptionRenderingEnabled = true;
			messageControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			messageControl.MessageTextBoxMouseWheel += MessageControl_MessageTextBoxMouseWheel;
			messageControl.ChildrenControlsClick += MessageControl_ChildrenControlsClick;

			messagesLayoutPanel.Controls.Add(messageControl);
			messagesLayoutPanel.Controls.SetChildIndex(messageControl, controlIndex);
			return messageControl;
		}

		protected void SetLabelVisable(ConversationMessageUserControl messageControl, IConversationMessage message) => messageControl.SetUsernameLabelVisible(message);

		#endregion

		void FilterMessageControls()
		{
			IConversationMessage lastMessage = null;
			Label lastDateLabel = null;
			bool anyMessagesOnLastDateVisible = false;

			foreach (Control control in messagesLayoutPanel.Controls)
			{
				ConversationMessageUserControl messageControl = control as ConversationMessageUserControl;
				if (messageControl != null)
				{
					bool isVisible = true;
					if (currentViewMode == ViewMode.UserMessagesOnly)
					{
						isVisible = messageControl.Message.MessageType != MessageType.LocalInternal && messageControl.Message.MessageSubType != MessageSubType.SystemLog;
					}
					messageControl.Visible = isVisible;

					if (isVisible)
					{
						messageControl.SetUsernameLabelVisible(lastMessage);
						lastMessage = messageControl.Message;
						anyMessagesOnLastDateVisible = true;
					}
				}
				else
				{
					Label dateLabel = control as Label;
					if (dateLabel != null)
					{
						if (lastDateLabel != null)
						{
							lastDateLabel.Visible = anyMessagesOnLastDateVisible;
						}
						lastDateLabel = dateLabel;
						anyMessagesOnLastDateVisible = false;
					}
				}
			}

			if (lastDateLabel != null)
			{
				lastDateLabel.Visible = anyMessagesOnLastDateVisible;
			}

			FixCustomisedHyperlinks();
		}

		#region Event Implementation

		void UserMessagesRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (userMessagesRadioButton.Checked)
			{
				currentViewMode = ViewMode.UserMessagesOnly;

				messagesLayoutPanel.SuspendLayout();
				FilterMessageControls();
				messagesLayoutPanel.ResumeLayout();
			}
		}

		void AllMessagesRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (allMessagesRadioButton.Checked)
			{
				currentViewMode = ViewMode.AllMessages;

				messagesLayoutPanel.SuspendLayout();
				FilterMessageControls();
				messagesLayoutPanel.ResumeLayout();
			}
		}

		protected void MessageControl_ChildrenControlsClick(object sender, EventArgs e)
		{
			messagesLayoutPanel.Focus();
		}

		protected void MessageControl_MessageTextBoxMouseWheel(object sender, MouseEventArgs e)
		{
			messagesLayoutPanel.Focus();
		}

		void MessagesLayoutPanel_MouseClick(object sender, MouseEventArgs e)
		{
			messagesLayoutPanel.Focus();
		}

		void MessagesLayoutPanel_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			messagesLayoutPanel.Focus();
		}

		void EConversationUserControl_Resize(object sender, EventArgs e)
		{
			if (!messagesLayoutPanel.Focused)
			{
				messagesLayoutPanel.Focus();
			}
			FixCustomisedHyperlinks();
		}

		void FixCustomisedHyperlinks()
		{
			foreach (var messageControl in messagesLayoutPanel.Controls.OfType<ConversationMessageUserControl>())
			{
				RichTextActionManager.FixCustomisedHyperlinks(messageControl.bodyTextBox, messageControl.Message.Body, messageControl.Prefix.Length);
			}
		}

		#endregion
	}
}
