using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.EConversation.GUI
{
	public partial class EConversationFullControl : ZUserControl, IConversationView
	{
		JobConversation Conversation => ((JobConversation)DataSource);

		readonly EConversationSecurityCheckpoints securityCheckpoints;
		readonly IConversationViewController controller;

		public IConversationViewController Controller => controller;

		readonly SpellChecker spellChecker;

		bool SendButtonAvailability = true;
		bool AddInternalCommentButtonAvailability = true;
		bool BroadcastButtonAvailability = true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public EConversationFullControl(ModuleIdentifier parentModuleId, EConversationSecurityCheckpoints securityCheckpoints = null, IConversationViewController viewController = null)
		{
			InitializeComponent();
			SetInitialButtonLocations();
			this.securityCheckpoints = securityCheckpoints ?? new EConversationSecurityCheckpoints(parentModuleId);
			this.controller = viewController ?? new EConversationViewController();
			controller.Initialize(this);

			IntialiseSecurity();

			spellChecker = SpellChecker.InitialiseSpellcheck(econversationMessageTextBox, nameof(econversationMessageTextBox));
			econversationMessageTextBox.ItemSelected += NewSubscriberSelected;
			econversationMessageTextBox.Hotkeys.RegisterHotKey(Keys.Control | Keys.Enter, SendMessageFromTextbox, Res.GetString("1155093b-9d28-4f90-b64c-39e1423661f9", "Send message"));
			econversationMessageTextBox.TextChanged += EConversationMessageTextBox_TextChanged;
		}

		void SetInitialButtonLocations()
		{
			sendButton.Location = ControlDpiScalingHelper.NewScaledPoint(503, SendButtonTopWhenItsTheOnlyButton);
			AddInternalCommentButton.Location = ControlDpiScalingHelper.NewScaledPoint(503, SendButtonTopWhenItsTheOnlyButton);
			broadcastButton.Location = ControlDpiScalingHelper.NewScaledPoint(503, SendButtonTopWhenItsTheOnlyButton);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var oldConversation = Conversation;
			if (oldConversation != null)
			{
				oldConversation.Messages.CountChanged -= Messages_CountChanged;
			}

			var newConversation = (dataSource as IConversationProvider)?.eConversation;
			base.SetDataBinding(newConversation, "");

			if (newConversation != null)
			{
				var boundObject = (FindForm() as ZForm)?.BusinessEntity;
				var parent = (boundObject as IConversationProvider) ?? (newConversation.Parent as IConversationProvider);
				if (parent != null)
				{
					econversationMessageTextBox.AutocompleteManager = new SubscriberAutocompleteHelper(newConversation.Factory, parent);
				}

				if (!securityCheckpoints.EditInternalParticipants.IsAllowed)
				{
					SetChildrenToReadOnly(newConversation.Staff);
					SetChildrenToReadOnly(newConversation.Groups);
				}

				newConversation.Messages.CountChanged += Messages_CountChanged;
				SetDisplayChatControls(!newConversation.ReadOnly);

				econversationMessageTextBox.SetDataBinding(newConversation, nameof(newConversation.NextMessage));
			}

			chatboxControl.RefreshMessages();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			UpdateKnownNames();

			if (FindForm() is ZForm form)
			{
				form.Saved += EConversationFullControl_Saved;
			}
		}

		void EConversationFullControl_Saved(object sender, EventArgs e)
		{
			UpdateKnownNames();
		}

		void EConversationMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			int xmlCharsRemoved;
			string xmlEscapedText = ZXmlValidation.EscapeInvalidXmlCharacters(econversationMessageTextBox.Text, out xmlCharsRemoved);
			if (xmlCharsRemoved > 0)
			{
				var previousSelectionStart = econversationMessageTextBox.SelectionStart;
				econversationMessageTextBox.Text = xmlEscapedText;
				econversationMessageTextBox.SelectionStart = Math.Max(previousSelectionStart - xmlCharsRemoved, 0);
			}

			var enabledStatus = econversationMessageTextBox.Enabled && !string.IsNullOrWhiteSpace(econversationMessageTextBox.Text);

			sendButton.Enabled = enabledStatus && SendButtonAvailability;
			AddInternalCommentButton.Enabled = AddInternalCommentButton.Visible && enabledStatus && AddInternalCommentButtonAvailability;
			broadcastButton.Enabled = broadcastButton.Visible && enabledStatus && BroadcastButtonAvailability;

			if (enabledStatus && Conversation != null && !Conversation.HasChanges)
			{
				Conversation.HasChanges = true;
			}
		}

		void UpdateKnownNames()
		{
			var knownNames = new List<string>();

			Conversation?.Staff.ForEach(staff => knownNames.Add(staff.Parent?.Name));
			Conversation?.RelatedParties.ForEach(staff => knownNames.Add(staff.Parent?.Name));

			spellChecker.UpdateWordsToIgnore(knownNames.Where(name => !string.IsNullOrWhiteSpace(name)));
		}

		void SetDisplayChatControls(bool display)
		{
			MessagePanelSplitContainer.Panel1Collapsed = !display;
			MessagePanelSplitContainer.IsSplitterFixed = !display;
		}

		void Messages_CountChanged(object sender, EventArgs e)
		{
			chatboxControl.RefreshMessages();
		}

		void SetChildrenToReadOnly(JobConversationParticipantCollection collection)
		{
			foreach (var child in collection)
			{
				child.ReadOnly = true;
			}
		}

		void NewSubscriberSelected(object sender, ItemSelectedEventArgs args)
		{
			NewSubscriberSelected_Core((SubscriberWrapper)args.SelectedItem);
		}

		protected void NewSubscriberSelected_Core(SubscriberWrapper selectedItem)
		{
			var newParticipant = selectedItem.Parent;
			var isAllowed = newParticipant.IsInternal || securityCheckpoints.ModifyExternalParticipants.IsAllowed;
			if (isAllowed)
			{
				if (!(newParticipant is BusinessObject))
				{ return; }
				var participant = Conversation.Participants.GetOrAdd(newParticipant);
				if (string.IsNullOrEmpty(participant.JCP_Relation) && !string.IsNullOrEmpty(selectedItem.Relation))
				{
					participant.JCP_Relation = selectedItem.Relation;
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("a6514a07-90c0-4d57-b880-b23204aa3592", "You do not have permission to add external participants"));
			}
		}

		void SendMessageFromTextbox()
		{
			controller.OnSendMessage();
		}

		#region Control Behaviour

		public EConversationViewMode ViewMode
		{
			get
			{
				return !MainSplitContainer.Panel1Collapsed && !MainSplitContainer.Panel2Collapsed
					? EConversationViewMode.ShowEverything
					: !MainSplitContainer.Panel1Collapsed
						? EConversationViewMode.ShowOnlyEConversation
						: EConversationViewMode.ShowOnlyParticipants;
			}
			set
			{
				switch (value)
				{
					case EConversationViewMode.ShowEverything:
						MainSplitContainer.Panel1Collapsed = false;
						MainSplitContainer.Panel2Collapsed = false;
						break;

					case EConversationViewMode.ShowOnlyEConversation:
						MainSplitContainer.Panel1Collapsed = false;
						MainSplitContainer.Panel2Collapsed = true;
						break;

					case EConversationViewMode.ShowOnlyParticipants:
						MainSplitContainer.Panel1Collapsed = true;
						MainSplitContainer.Panel2Collapsed = false;
						break;
				}
			}
		}

		public bool ShouldShowAddInternalCommentButton
		{
			get => AddInternalCommentButton.Visible;
			set
			{
				AddInternalCommentButton.Visible = value;
				SetButtonLocations();
			}
		}

		void SetButtonLocations()
		{
			if (AddInternalCommentButton.Visible || broadcastButton.Visible)
			{
				ControlDpiScalingHelper.SetTop(sendButton, econversationMessageTextBox.Top, false);
				if (AddInternalCommentButton.Visible && broadcastButton.Visible)
				{
					ControlDpiScalingHelper.SetTop(broadcastButton, ThirdButtonTop, true);
				}
			}
			else
			{
				ControlDpiScalingHelper.SetTop(sendButton, SendButtonTopWhenItsTheOnlyButton, true);
			}
		}

		const int SendButtonTopWhenItsTheOnlyButton = 54;
		const int ThirdButtonTop = 104;

		public void SetMessageTextboxText(string text)
		{
			econversationMessageTextBox.Text = text;
		}

		public void DisableInputs()
		{
			EnableOrDisableInputs(true);
			econversationMessageTextBox.ForeColor = Color.DarkGray;
		}

		public void EnableInputs()
		{
			EnableOrDisableInputs(false);
			econversationMessageTextBox.BackColor = Color.White;
			econversationMessageTextBox.ForeColor = Color.Black;
		}

		void EnableOrDisableInputs(bool shouldBeReadOnly)
		{
			foreach (var button in this.FindAll<ZButton>())
			{
				button.SetReadOnly(shouldBeReadOnly);
			}

			econversationMessageTextBox.SetReadOnly(shouldBeReadOnly);
		}

		public bool AreInputsReadOnly => econversationMessageTextBox.ReadOnly;

		#endregion

		#region IConversationView

		JobConversation IConversationView.Conversation => Conversation;
		TextBoxBase IConversationView.MessageTextBox => econversationMessageTextBox;
		ZButton IConversationView.SendButton => sendButton;
		ZButton IConversationView.AddInternalCommentButton => AddInternalCommentButton;
		ZButton IConversationView.BroadcastButton => broadcastButton;

		#endregion

		#region Security

		void IntialiseSecurity()
		{
			var sendMessageCheckpoint = securityCheckpoints.SendMessages;
			if (!sendMessageCheckpoint.IsAllowed)
			{
				sendButton.Enabled = false;
				sendButton.ToolTipCaption = sendMessageCheckpoint.ErrorMessageForNotAllowed;

				econversationMessageTextBox.Text = sendMessageCheckpoint.ErrorMessageForNotAllowed;
				econversationMessageTextBox.ReadOnly = true;
			}

			contactGrid.ReadOnly = !securityCheckpoints.ModifyExternalParticipants.IsAllowed;
		}

		public void ToggleButtonsAvailability(bool send, bool addInternalComment, bool broadcast)
		{
			SendButtonAvailability = send;
			sendButton.Enabled = send;

			AddInternalCommentButtonAvailability = addInternalComment;
			AddInternalCommentButton.Enabled = addInternalComment;

			BroadcastButtonAvailability = broadcast;
			broadcastButton.Enabled = broadcast;
			broadcastButton.Visible = true;

			SetButtonLocations();
		}

		#endregion

		#region Broadcast

		public void ToggleBroadcastAvailability(bool toggle)
		{
			this.broadcastButton.Visible = toggle;
			SetButtonLocations();
		}

		#endregion

#if DEBUG
		public ZArchitecture.GUI.ZButton SendButtonForTest => sendButton;
		public ZArchitecture.GUI.ZButton AddInternalCommentButtonForTest => AddInternalCommentButton;
		public ZArchitecture.GUI.ZButton BroadcastButtonForTest => broadcastButton;
#endif
	}
}
