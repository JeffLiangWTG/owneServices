using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Core.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.GUI;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Modules;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentManagementControlCenterUserControl : ZUserControl
	{
		const string selectIncidentMessage = "Please select one or more incidents";
		const string selectIncidentCaption = "Select Incident";

		new IncidentManagementGroup CurrentDataItem => (IncidentManagementGroup)base.CurrentDataItem;

		IncidentManagementGroup ParentFormBusinessEntity => ((IncidentManagementGroupForm)ParentForm).IncidentManagementGroup;

		IncidentManagementLink[] linkedIncidentsGridSelectedElements => LinkedIncidentsGrid.InnerGrid.GetSelectedElements<IncidentManagementLink>();

		protected IncidentManagementGroupMessage MessageInventoryGridCurrentElement => MessageInventoryGrid.InnerGrid.ListManager.GetCurrent() as IncidentManagementGroupMessage;
		IncidentManagementGroupMessageCollection MessageInventoryGridAllElements => MessageInventoryGrid.InnerGrid.ListManager == null ? null : MessageInventoryGrid.InnerGrid.ListManager.List as IncidentManagementGroupMessageCollection;

		readonly SpellChecker spellChecker;

		public IncidentManagementControlCenterUserControl()
			: base()
		{
			InitializeComponent();
			InitialiseLinkedIncidentsGridEvent();
			spellChecker = SpellChecker.InitialiseSpellcheck(MessageContentTextBox, nameof(MessageContentTextBox));
		}

		void InitialiseLinkedIncidentsGridEvent()
		{
			MessageInventoryGrid.InnerGrid.AfterBind += MessageInventoryGrid_AfterBind;
			LinkedIncidentsGrid.InnerGrid.AfterBind += LinkedIncidentsGrid_AfterBind;
			LinkedIncidentsGrid.Detached += LinkedIncidentsGrid_Detached;
			LinkedIncidentsGrid.Attaching += LinkedIncidentsGrid_Attaching;
			LinkedIncidentsGrid.InnerGrid.MouseUp += InnerGrid_SelectedRowsChanged;
			LinkedIncidentsGrid.InnerGrid.KeyUp += InnerGrid_KeyUp;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (ParentForm != null && ParentFormBusinessEntity != null)
			{
				((IncidentManagementGroupForm)ParentForm).Saved += (s, eventArgs) => { ParentFormBusinessEntity.LinkedIncidents.Cast<IncidentManagementLink>().ForEach(x => x.LastCommunicationInfo.RefreshBinding()); };
			}
			StartTimer();
		}

		void StartTimer()
		{
			ResetTimeCounter();
			timer.Interval = 1000;
			timer.Tick += new EventHandler(TimerTick);
			timer.Start();
		}

		TimeSpan timeCounter = TimeSpan.Zero;
		void ResetTimeCounter()
		{
			timeCounter = TimeSpan.Zero;
			timeCounter = timeCounter.Add(new TimeSpan(0, RefreshPeriodInMinutes, 0));
		}

		void TimerTick(object sender, EventArgs e)
		{
			try
			{
				timeCounter = timeCounter.Add(new TimeSpan(0, 0, -1));
				if (timeCounter.TotalSeconds == 0)
				{
					ResetTimeAndRefreshPage();
				}
				countdownLabel.Text = timeCounter.ToString(@"mm\:ss");
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("An exception was thrown during refresh", ex);
				timer.Stop();
				countdownLabel.Hide();
			}
		}

		void ResetTimeAndRefreshPage()
		{
			timer.Stop();
			ResetTimeCounter();
			countdownLabel.Text = timeCounter.ToString(@"mm\:ss");

			LinkedIncidentsGrid.ForceRefresh();
			if (CommunicationArea.Visible)
			{
				CommunicationArea.Refresh();
			}

			timer.Start();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		internal int RefreshPeriodInMinutes
		{
			get
			{
				if (refreshPeriodInMinutes < 0)
				{
					refreshPeriodInMinutes = EDIDataRegistry.Instance.LinkedIncidentGridRefreshRate.Value;
				}
				return refreshPeriodInMinutes;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		int refreshPeriodInMinutes = int.MinValue;

		void LinkedIncidentsGrid_Detached(object sender, EventArgs e)
		{
			CommunicationArea.Hide();
		}

		void LinkedIncidentsGrid_Attaching(object sender, ModuleButtonGridOperationCancelEventArgs args)
		{
			CurrentDataItem.RefreshBinding();
			CurrentDataItem.Lookups.RefreshIncidentNotLinked();
		}

		void LinkedIncidentsGrid_AfterBind(object sender, EventArgs e)
		{
			InnerGrid_SelectedRowsChanged(null, EventArgs.Empty);
		}

		#region LinkedIncidentGridClick

		ZGuid lastSelectedRowPK = ZGuid.Empty;

		void InnerGrid_SelectedRowsChanged(object sender, EventArgs e)
		{
			if (LinkedIncidentsGrid.InnerGrid.SelectedRowCount > 1)
			{
				if (!CommunicationArea.Visible)
				{
					return;
				}

				CommunicationArea.SuspendLayout();
				CommunicationArea.Hide();
				CommunicationArea.ResumeLayout();
			}
			else if (LinkedIncidentsGrid.InnerGrid.GetCurrent() is IncidentManagementLink currentSelectedLink)
			{
				if (CommunicationArea.Visible && currentSelectedLink.PK == lastSelectedRowPK)
				{
					return;
				}

				CommunicationArea.ResumeLayout();
				lastSelectedRowPK = currentSelectedLink.PK;
				CommunicationArea.SetDataBinding(currentSelectedLink?.SupportIncident, "");
				CommunicationArea.Refresh();
				CommunicationArea.Show();
				CommunicationArea.ResumeLayout();
			}
		}

		void InnerGrid_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.ControlKey)
			{
				InnerGrid_SelectedRowsChanged(null, EventArgs.Empty);
			}
		}

		#endregion

		void MessageInventoryGrid_AfterBind(object sender, EventArgs e)
		{
			if (MessageInventoryGrid.InnerGrid.ListManager != null)
			{
				MessageInventoryGrid.InnerGrid.ListManager.CurrentItemChanged -= MessageInventoryGridListManager_CurrentItemChanged;
				MessageInventoryGrid.InnerGrid.ListManager.CurrentItemChanged += MessageInventoryGridListManager_CurrentItemChanged;
				MessageInventoryGridListManager_CurrentItemChanged(null, null);

				MessageInventoryGrid.InnerGrid.ListManager.CurrentItemChanged -= MessageInventoryGridListManager_EnableInterimMessageButton;
				MessageInventoryGrid.InnerGrid.ListManager.CurrentItemChanged += MessageInventoryGridListManager_EnableInterimMessageButton;
				MessageInventoryGridListManager_EnableInterimMessageButton(null, null);

				MessageInventoryGrid.InnerGrid.ListManager.CurrentItemChanged -= MessageInventoryGridListManager_EnableAutoReplyMessageButton;
				MessageInventoryGrid.InnerGrid.ListManager.CurrentItemChanged += MessageInventoryGridListManager_EnableAutoReplyMessageButton;
				MessageInventoryGridListManager_EnableAutoReplyMessageButton(null, null);
			}
		}

		void MessageInventoryGridListManager_CurrentItemChanged(object sender, EventArgs e)
		{
			var record = MessageInventoryGridCurrentElement;
			if (record != null)
			{
				MessageContentTextBox.SetDataBinding(record, "IGM_Message");
				MessageContentTextBox.Enabled = !record.IGM_IsPublished;
				PublishButton.Enabled = !record.IGM_IsPublished && !string.IsNullOrWhiteSpace(record.IGM_Message);
				RevertToDraftButton.Enabled = record.IGM_IsPublished;

				var messages = GetIncidentManagementGroupMessagesOfType(record.IGM_Type);
				CreateNewDraftButton.Enabled = messages.Count() == 1 && messages.First().IGM_IsPublished;
			}
			else
			{
				MessageContentTextBox.ResetBindings();
				MessageContentTextBox.Enabled = false;
				PublishButton.Enabled = false;
				RevertToDraftButton.Enabled = false;
				CreateNewDraftButton.Enabled = false;
			}
		}

		void MessageContentTextBox_TextChanged(object sender, EventArgs e)
		{
			var record = MessageInventoryGridCurrentElement;
			if (record != null)
			{
				PublishButton.Enabled = !record.IGM_IsPublished && !string.IsNullOrWhiteSpace((sender as ZTextBox).Text);
			}
		}

		void CreateNewDraftButton_Click(object sender, EventArgs e)
		{
			var record = MessageInventoryGridCurrentElement;
			if (record != null && record.IGM_IsPublished &&
				GetIncidentManagementGroupMessagesOfType(record.IGM_Type).Count() == 1)
			{
				var message = CurrentDataItem.IncidentManagementGroupMessages.AddNew();
				message.IGM_Type = record.IGM_Type;
				MessageInventoryGrid.InnerGrid.ListManager.Position = MessageInventoryGrid.InnerGrid.ListManager.List.IndexOf(message);
			}
		}

		void RevertToDraftButton_Click(object sender, EventArgs e)
		{
			var record = MessageInventoryGridCurrentElement;
			if (record != null && record.IGM_IsPublished)
			{
				var messages = GetIncidentManagementGroupMessagesOfType(record.IGM_Type);
				if (messages.Count() > 1)
				{
					var messageToDelete = messages.First(m => m.IGM_Type == record.IGM_Type && m.PK != record.PK);
					if (Globals.Message.Show(Res.GetString("89C065EE-7B2E-42A9-A3EB-462A7EFD9033", "A draft message with type '{0}' already exists, this action will replace that message with the current content. Do you wish to proceed?", messageToDelete.TypeDescription), "Approval Gate", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
					{
						return;
					}
					CurrentDataItem.IncidentManagementGroupMessages.Remove(messageToDelete);
					messageToDelete.Delete();
				}
				record.SetIsPublished(false);
			}
		}

		void PublishButton_Click(object sender, EventArgs e)
		{
			var record = MessageInventoryGridCurrentElement;
			if (record != null && !record.IGM_IsPublished)
			{
				if (string.IsNullOrWhiteSpace(record.IGM_Message))
				{
					Globals.Message.ShowInformation(Res.GetString("80173939-B79F-4C9D-84B7-412517328878", "Cannot publish an empty message. Please enter a message and try again"));
					return;
				}

				var messages = GetIncidentManagementGroupMessagesOfType(record.IGM_Type);
				if (messages.Count() > 1)
				{
					var messageToDelete = messages.First(m => m.IGM_Type == record.IGM_Type && m.PK != record.PK);
					if (Globals.Message.Show(Res.GetString("56AF8275-6043-4E37-9B60-E3256D4E1C14", "A published message with type '{0}' already exists, this action will replace that message with the current content. Do you wish to proceed?", messageToDelete.TypeDescription), "Approval Gate", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
					{
						return;
					}
					CurrentDataItem.IncidentManagementGroupMessages.Remove(messageToDelete);
					messageToDelete.Delete();
				}

				spellChecker?.CheckSpelling();
				record.SetIsPublished(true);
			}
		}

		IWorkTaskRelatedItemSource RelatedItemSource
		{
			get { return (IWorkTaskRelatedItemSource)BindingSource.Current; }
		}

		void ShowRecordAttacher(WorkTaskRelatedItemModuleInfo relatedItem)
		{
			var moduleID = relatedItem.ModuleID;
			NewItemHelper.SetAdditionalFilter(relatedItem);

			LastAttacher = new WorkTaskAttacher(((IncidentManagementGroup)RelatedItemSource).AutoCascadeRelatedItems, relatedItem.FindBoxList, moduleID);
			LastAttacher.Show((IZForm)FindForm());
			AttachWorkItems();
		}

		void AttachWorkItems()
		{
			var attacher = LastAttacher;
			if (attacher is IFindBox findBox)
			{
				var popupForm = findBox.PopupForm as EmbeddedModulePopup;
				LastShownAttachPopupForTesting = popupForm;
				if (popupForm != null)
				{
					popupForm.Selected += (s, e) =>
					{
						var incidentGroup = (IncidentManagementGroup)RelatedItemSource;
						foreach (var workItem in e.SelectedBusinessObjects.OfType<WorkItem>())
						{
							incidentGroup.AutoCascadeRelatedItems.Add(workItem);
						}
					};
				}
			}
		}

		public ZRecordAttacher LastAttacher;
		public ZController LastController;

		#region Auto Cascade Work Items

		void WorkItemsGrid_DoubleClick(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void OpenRelatedItem()
		{
			if (workItemsGrid.ListManager.Position > -1)
			{
				var relatedItem = (IWorkTaskRelatedItem)workItemsGrid.ListManager.GetCurrent();
				LastController = ZControllerFactory.Create(relatedItem.ControllerID);
				LastController.ShowEditForm((BusinessObject)relatedItem);
			}
		}

		void NewButton_Click(object sender, EventArgs e)
		{
			LastController = ZControllerFactory.Create(ControllerIDs.WorkItem);
			NewItemHelper.AddNewItem(LastController, RelatedItemSource, ((IncidentManagementGroup)RelatedItemSource).AutoCascadeRelatedItems, WorkTaskRelatedItemTypes.WorkItem, sender == null, (IncidentManagementGroup)RelatedItemSource, true);
		}

		public void AttachButton_Click(object sender, EventArgs e)
		{
			ShowRecordAttacher(RelatedItemSource.SupportedRelatedItemModules.First(x => x.ControllerID == ControllerIDs.WorkItem));
		}

		void DetachButton_Click(object sender, EventArgs e)
		{
			if (workItemsGrid.ListManager.Position > -1)
			{
				var msg = Res.GetString("4f75e888-7aed-4364-af90-093f25763fc5", "Are you sure you want to detach the selected work item(s)?");
				var caption = Res.GetString("42859a19-a8f6-402b-847f-0e948b6e8f96", "Confirm Detach...");
				var dialogResult = Globals.Message.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.Yes)
				{
					var relatedItems = workItemsGrid.SelectedElements;
					foreach (var relatedItem in relatedItems)
					{
						var group = (IncidentManagementGroup)RelatedItemSource;
						group.AutoCascadeRelatedItems.Remove(relatedItem);
						group.RelatedItems.Remove(relatedItem);
					}
				}
			}
		}

		#endregion

		IEnumerable<IncidentManagementGroupMessage> GetIncidentManagementGroupMessagesOfType(string messageType)
		{
			return CurrentDataItem.IncidentManagementGroupMessages.Where(m => m.IGM_Type == messageType);
		}

		#region Broadcast Message

		void MessageInventoryGridListManager_EnableInterimMessageButton(object sender, EventArgs e)
		{
			EnableBroadcastButton(IncidentManagementGroupMessageTypePairList.Codes.Interim);
		}

		void MessageInventoryGridListManager_EnableAutoReplyMessageButton(object sender, EventArgs e)
		{
			EnableBroadcastButton(IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
		}

		void EnableBroadcastButton(string incidentManagementGroupMessageCode)
		{
			ZButton button;

			if (incidentManagementGroupMessageCode.Equals(IncidentManagementGroupMessageTypePairList.Codes.Interim))
			{
				button = broadcastInterimButton;
			}
			else if (incidentManagementGroupMessageCode.Equals(IncidentManagementGroupMessageTypePairList.Codes.AutoReply))
			{
				button = sendAutoReplyButton;
			}
			else
			{
				return;
			}

			var messages = MessageInventoryGridAllElements;
			if (!messages.IsNullOrEmpty())
			{
				var hasAnyPublishedMessages = messages.
						Cast<IncidentManagementGroupMessage>()
						.Any(message => message.IGM_IsPublished && message.IGM_Type.Equals(incidentManagementGroupMessageCode));

				button.Enabled = hasAnyPublishedMessages;
			}
			else
			{
				button.Enabled = false;
			}
		}

		void SendBroadcastMessage(string incidentManagementGroupMessageCode)
		{
			var publishedMessage = MessageInventoryGridAllElements
					.Cast<IncidentManagementGroupMessage>()
					.FirstOrDefault(message => message.IGM_IsPublished && message.IGM_Type.Equals(incidentManagementGroupMessageCode));

			if (publishedMessage != null)
			{
				if (publishedMessage.IGM_BroadcastDateUtc.IsEmpty)
				{
					CurrentDataItem.EConversation.AddMessageFromCurrentUser(publishedMessage.IGM_Message, true, false);
				}

				var selectedRecords = linkedIncidentsGridSelectedElements;
				foreach (var link in selectedRecords)
				{
					CurrentDataItem.SendBroadcastMessage(link, publishedMessage, false);
				}
			}
		}

		bool ShouldBroadcastMessage()
		{
			var selectedRecords = linkedIncidentsGridSelectedElements;
			if (selectedRecords?.Length > 0)
			{
				var noControlSelectedRecords = selectedRecords.Where(record => !record.IsControlled);
				var controlSelectedRecords = selectedRecords.Where(record => record.IsControlled);

				string message = "";

				if (noControlSelectedRecords.IsNullOrEmpty())
				{
					message = Res.GetString
						("A8C5B618-A9C8-433E-A36F-F4E4E115DA92",
						"You are about to send a broadcast message to '{0}'. Would you like to continue?",
						string.Join(",", selectedRecords.Select(record => record.SupportIncident.Number)));
				}
				else if (controlSelectedRecords.IsNullOrEmpty())
				{
					message = Res.GetString
						("EC056BDF-B1B9-41AA-AEF9-411489115A33",
						"The following non - controlled incidents are selected as recipients '{0}'. Would you like to continue?",
						string.Join(",", noControlSelectedRecords.Select(record => record.SupportIncident.Number)));
				}
				else
				{
					message = Res.GetString
						("44AAADE2-2999-4B92-B00E-7D25814042FD",
						"You are about to send a broadcast message to '{0}'. The following non - controlled incidents are selected as recipients '{1}'. Would you like to continue?",
						string.Join(",", selectedRecords.Select(record => record.SupportIncident.Number)),
						string.Join(",", noControlSelectedRecords.Select(record => record.SupportIncident.Number)));
				}

				MessageBoxButtons messageBoxButtons;
				DialogResult defaultDialogResult;
				DialogResult dialogResult;
				if (noControlSelectedRecords.IsNullOrEmpty())
				{
					messageBoxButtons = MessageBoxButtons.OKCancel;
					defaultDialogResult = DialogResult.Cancel;
					dialogResult = DialogResult.OK;
				}
				else
				{
					messageBoxButtons = MessageBoxButtons.YesNo;
					defaultDialogResult = DialogResult.No;
					dialogResult = DialogResult.Yes;
				}

				if (UserNotification.Instance.Show(message, "Confirmation Required", messageBoxButtons, defaultDialogResult) != dialogResult)
				{
					return false;
				}

				return true;
			}

			return false;
		}

		void BroadcastInterimButton_Click(object sender, EventArgs e)
		{
			if (linkedIncidentsGridSelectedElements?.Length > 0)
			{
				if (!ShouldBroadcastMessage())
				{
					return;
				}

				SendBroadcastMessage(IncidentManagementGroupMessageTypePairList.Codes.Interim);
			}
			else
			{
				NotificationHandler.Instance.ReportInformation(selectIncidentMessage, selectIncidentCaption);
			}
		}

		void SendAutoReplyButton_Click(object sender, EventArgs e)
		{
			if (linkedIncidentsGridSelectedElements?.Length > 0)
			{
				if (!ShouldBroadcastMessage())
				{
					return;
				}

				SendBroadcastMessage(IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
			}
			else
			{
				NotificationHandler.Instance.ReportInformation(selectIncidentMessage, selectIncidentCaption);
			}
		}

		#endregion

		void RefreshButton_Click(object sender, EventArgs e)
		{
			ResetTimeAndRefreshPage();
		}

		void AssignResponderButton_Click(object sender, EventArgs e)
		{
			var selectedRecords = linkedIncidentsGridSelectedElements;
			if (selectedRecords?.Length > 0)
			{
				var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff);
				if (module.SecurityCheckpoint.IsAllowed)
				{
					var provider = new SingleSelectModuleDecisionProvider();
					module.OverrideModuleDecisionProvider(provider);

					var popup = new EmbeddedModulePopup(module);
					provider.Popup = popup;
					ZFormModaliser.ShowDialogAndDispose(popup);

					if (provider.SelectedBusinessObject is GlbStaff staff)
					{
						foreach (var item in selectedRecords)
						{
							item.INL_GS_NKResponder = staff.GS_Code;
						}

						ParentFormBusinessEntity.HasChanges = true;
					}
				}
			}
			else
			{
				NotificationHandler.Instance.ReportInformation(selectIncidentMessage, selectIncidentCaption);
			}
		}

		void Popup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
		}

		void DisallowControlButton_Click(object sender, EventArgs e)
		{
			if (linkedIncidentsGridSelectedElements?.Length > 0)
			{
				if (!linkedIncidentsGridSelectedElements.Any(x => x.INL_IsGroupControlled))
				{
					Globals.Message.Show(ResString.GetMultilingualString("a89e335b-8d45-414e-9097-42ac7f35f4df", "Disallow Control cannot be executed if the selected incident status is already disabled."), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					var confirmationMessage = ResString.GetMultilingualString("a1952d01-6590-484d-9a11-f31a8e64a083",
						"Canceled tasks for the selected incident(s) will be re-opened and all controlled operations by the Incident group moving forward will not be applied:\r\n{0}\r\nWould you like to continue?",
						string.Join(System.Environment.NewLine, linkedIncidentsGridSelectedElements.Select(x => x.SupportIncident.Number)));
					if (Globals.Message.Show(confirmationMessage, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) != DialogResult.Yes)
					{
						return;
					}

					var alreadyReleasedLink = linkedIncidentsGridSelectedElements.Where(x => !x.INL_IsGroupControlled).Select(x => x.SupportIncident.Number);
					if (alreadyReleasedLink.Any())
					{
						var findReleasedIncidentsMessage = ResString.GetMultilingualString("fbf37224-2310-45ae-aab3-223f4a36673f",
							"Disallow Control status is already disabled for the selected Incident(s) below. \r\n{0}",
							string.Join(System.Environment.NewLine, alreadyReleasedLink));
						if (Globals.Message.Show(findReleasedIncidentsMessage, string.Empty, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
						{
							return;
						}
					}

					var shouldReopenTasks = true;
					if (CurrentDataItem.NowStage?.GroupCompleted ?? false)
					{
						var reopenMessage = ResString.GetMultilingualString("980ce777-c30b-4ecb-9421-888dc0011db7",
							"The current Incident Group Stage for the selected incidents is now in completed status:\r\n{0}\r\nDo you still wish to re-open the canceled tasks?",
							string.Join(System.Environment.NewLine, linkedIncidentsGridSelectedElements.Where(x => x.IsControlled).Select(x => x.SupportIncident.Number)));
						shouldReopenTasks = Globals.Message.Show(reopenMessage, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;
					}

					var controlledLinks = linkedIncidentsGridSelectedElements.Where(x => x.INL_IsGroupControlled);
					if (shouldReopenTasks)
					{
						foreach (var link in controlledLinks)
						{
							link.ReopenIncidentTasksClosedByGroup();
						}
					}
					controlledLinks.ForEach(x => x.INL_IsGroupControlled = false);
				}
			}
			else
			{
				NotificationHandler.Instance.ReportInformation(selectIncidentMessage, selectIncidentCaption);
			}
		}

		void AllowControlButton_Click(object sender, EventArgs e)
		{
			if (linkedIncidentsGridSelectedElements?.Length > 0)
			{
				if (!linkedIncidentsGridSelectedElements.Any(x => !x.INL_IsGroupControlled))
				{
					Globals.Message.Show(ResString.GetMultilingualString("91e9be00-fe2f-45f8-8029-749f8e92838c", "Allow Control cannot be executed if the selected incident(s) status is already enabled."), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					var confirmationMessage = ResString.GetMultilingualString("c69ed9a8-73c8-4cef-a553-03e4e2a4bcde",
						"Allowing Control will apply all controlled operations on the selected incident(s):\r\n{0}\r\nWould you like to continue?",
						string.Join(System.Environment.NewLine, linkedIncidentsGridSelectedElements.Select(x => x.SupportIncident.Number)));
					if (Globals.Message.Show(confirmationMessage, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) != DialogResult.Yes)
					{
						return;
					}

					var alreadyControlledLinks = linkedIncidentsGridSelectedElements.Where(x => x.INL_IsGroupControlled).Select(x => x.SupportIncident.Number);
					if (alreadyControlledLinks.Any())
					{
						if (Globals.Message.Show(ResString.GetMultilingualString("17d7327f-7c50-4065-854e-6158f70ba522", "Allow Control status is already enabled for the selected Incident(s) below: \r\n{0}", string.Join(System.Environment.NewLine, alreadyControlledLinks)), string.Empty, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
						{
							return;
						}
					}

					var shouldSendMessage = false;
					IncidentManagementGroupMessage currentPublishedMessage = null;
					if (CurrentDataItem.NowStage?.ControlIncidents ?? false)
					{
						currentPublishedMessage = CurrentDataItem.GetBroadcastMessage(false);
						var messageType = currentPublishedMessage?.IGM_Type.ToString() ?? string.Empty;
						if (messageType == IncidentManagementGroupMessageTypePairList.Codes.Interim ||
							messageType == IncidentManagementGroupMessageTypePairList.Codes.Opening ||
							messageType == IncidentManagementGroupMessageTypePairList.Codes.Closing)
						{
							shouldSendMessage = Globals.Message.Show(ResString.GetMultilingualString("0fbd46d6-2a13-48b8-88ed-0b51c329a45a", "The Incident Group stage has an active {0} Broadcast. Do you want to send the {0} broadcast message?", (new IncidentManagementGroupMessageTypePairList()).GetDescriptionFromCode(currentPublishedMessage.IGM_Type)), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
						}
					}

					foreach (var link in linkedIncidentsGridSelectedElements.Where(x => !x.INL_IsGroupControlled))
					{
						link.INL_IsGroupControlled = true;
						if (shouldSendMessage)
						{
							CurrentDataItem.SendBroadcastMessage(link, currentPublishedMessage);
						}
					}
				}
			}
			else
			{
				NotificationHandler.Instance.ReportInformation(selectIncidentMessage, selectIncidentCaption);
			}
		}

		public EmbeddedModulePopup LastShownAttachPopupForTesting;

		#region EmbeddedModulePopup Provider

		class SingleSelectModuleDecisionProvider : IModuleDecisionProvider
		{
			public bool ShouldDisplayNotifications => true;
			public bool ShouldLoadFilterBizObj => false;
			public bool ShouldSaveFilterBizObj => false;
			public bool ShouldIgnoreAdditionalFilter => true;
			public bool AllowExcelExport => false;
			public bool EnablePreviousNextSupport => false;
			public IBusinessObjectCollection List { get; private set; }
			public BusinessObject SelectedBusinessObject { get; private set; }
			public Form Popup { get; set; }

			public SingleSelectModuleDecisionProvider(IBusinessObjectCollection list = null)
			{
				List = list;
			}

			public void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
			{
				if (selectedBusinessObjects.Length == 1)
				{
					var bizo = selectedBusinessObjects[0];
					if (!bizo.HasRowErrors)
					{
						SelectedBusinessObject = selectedBusinessObjects[0];
						Popup.DialogResult = DialogResult.OK;
						Popup?.Close();
					}
					else
					{
						Globals.Message.ShowError(bizo.RowErrors.First().Message);
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("983D0B25-3395-4C00-8D57-72CD4DB84659", "Please select one item."));
				}
			}

			public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
			{
				HandleDefaultAction(selectedBusinessObjects);
			}

			public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
			{
			}

			public void InitialiseFindBoxControllerLink(ZController controller)
			{
			}

			public void SetFindBoxCodeDescription(BusinessObject bizo)
			{
			}
		}

		#endregion
	}
}
