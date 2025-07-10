using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class FindTriageForm : ZChildForm
	{
		public FindTriageForm(FindTriageFilterHelper helper, SupportIncidentForm parentIncidentForm = null)
			: base(helper)
		{
			ControllerID = ClientControllerRegistration.IncidentTriage;
			ParentIncidentForm = parentIncidentForm;
		}

		public SupportIncidentForm ParentIncidentForm { get; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			resultsGrid.SelectedRowsChangedInMouseDown += ResultsGrid_SelectedRowsChangedInMouseDown;
			checklistItemsForMessageGrid.SelectedRowsChangedInMouseDown += ChecklistItemsGrid_SelectedRowsChangedInMouseDown;
			reviewChecklistTabPage.TabVisible = false;
		}

		internal FindTriageFilterHelper Helper => BusinessEntity as FindTriageFilterHelper;

		void SearchButton_Click(object sender, EventArgs e)
		{
			Helper.RefreshTriageCollection();

			if (Helper.TriageCollection.Count != 0)
			{
				resultsGrid.Select(0);
			}
		}

		void ResultsGrid_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
		{
			if (resultsGrid.SelectedRowCount > 1)
			{
				Globals.Message.ShowWarning(Res.GetString("c7badf9f-9300-4dfb-ae70-aa759d82b02d", "Please select only one triage node."));
				resultsGrid.UnSelectAll();
			}
		}

		void RefreshReviewControlsEnabledStatus()
		{
			addAllToMessageButton.Enabled = checklistItemsForMessageGrid.ListManager?.List.Count > 0;
			addToMessageButton.Enabled = checklistItemsForMessageGrid.SelectedRowCount > 0;
		}

		internal IncidentTriage Triage => resultsGrid.SelectedRowCount == 1 ? resultsGrid.SelectedElements[0] as IncidentTriage : null;

		void CancelButton_Click(object sender, EventArgs e)
		{
			if (PromptSaveTriageSupportNotes() != ZDialogResult.Cancel)
			{
				Close();
			}
		}

		void NextButton_Click(object sender, EventArgs e)
		{
			if (resultsGrid.SelectedRowCount != 1)
			{
				Globals.Message.ShowWarning(Res.GetString("96e1c702-794e-4377-b95d-9ea6487036f5", "Please select one triage node to continue."));
			}
			else
			{
				findTriageTabPage.TabVisible = false;
				reviewChecklistTabPage.TabVisible = true;
				RefreshReviewControlsEnabledStatus();
				mainTabControl.SelectedTab = reviewChecklistTabPage;
			}
		}

		void ChecklistItemsGrid_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
		{
			addToMessageButton.Enabled = checklistItemsForMessageGrid.SelectedRowCount > 0;
		}

		void UpdateButton_Click(object sender, EventArgs e)
		{
			if (Triage != null && Triage.HasChanges)
			{
				SaveSupportNotes();
			}
		}

		void ChecklistItemsGrid_DoubleClick(object sender, EventArgs e)
		{
			if (checklistItemsForMessageGrid.ListManager.Position > -1)
			{
				var relatedItem = (IncidentTriageChecklistItemPivot)checklistItemsForMessageGrid.ListManager.GetCurrent();
				var controller = ZControllerFactory.Create(ClientControllerRegistration.IncidentTriageChecklistItem);
				controller.ShowEditForm(relatedItem.ChecklistItem);
			}
		}

		void AddToMessageButton_Click(object sender, EventArgs e)
		{
			if (checklistItemsForMessageGrid.SelectedRowCount > 0)
			{
				AddChecklistItemsToMessageBuilder(checklistItemsForMessageGrid.SelectedElements.Cast<IncidentTriageChecklistItemPivot>().ToArray());
			}
		}

		void AddAllToMessageButton_Click(object sender, EventArgs e)
		{
			if (checklistItemsForMessageGrid.ListManager.List.Count > 0)
			{
				AddChecklistItemsToMessageBuilder(checklistItemsForMessageGrid.ListManager.List.Cast<IncidentTriageChecklistItemPivot>().ToArray());
			}
		}

		void AddChecklistItemsToMessageBuilder(IncidentTriageChecklistItemPivot[] checklistItems)
		{
			if (checklistItems.Length > 0)
			{
				var sbMessage = new StringBuilder(ClientMessageBuilderTextBox.Text);

				var notPublishedList = new List<string>();
				foreach (var item in checklistItems.OrderBy(o => o.IMP_Sequence))
				{
					if (item.ChecklistItem.IMC_IsPublished)
					{
						sbMessage.Append(sbMessage.Length == 0 || sbMessage.ToString().EndsWith("\r\n") ? "\r\n-- " : "\r\n\r\n-- ");
						sbMessage.Append(item.ChecklistItem.PublishedDescriptionText);
					}
					else
					{
						notPublishedList.Add(item.IMP_Sequence.ToString());
					}
				}
				sbMessage.Append("\r\n");

				if (notPublishedList.Count > 0)
				{
					var notPublishedSequences = string.Join(", ", notPublishedList);
					Globals.Message.ShowError(FormattableString.Invariant($"The message could not include the specified Checklist Item(s) (Sequence {notPublishedSequences}) because they are not published."));
				}

				ClientMessageBuilderTextBox.Text = sbMessage.ToString();
			}
		}

		void ClearContentButton_Click(object sender, EventArgs e)
		{
			ClientMessageBuilderTextBox.Text = string.Empty;
		}

		void CopyToClipboardButton_Click(object sender, EventArgs e)
		{
			SafeClipboard.SetText(ClientMessageBuilderTextBox.Text);
		}

		void BackToSearchButton_Click(object sender, EventArgs e)
		{
			if (PromptSaveTriageSupportNotes() != ZDialogResult.Cancel)
			{
				reviewChecklistTabPage.TabVisible = false;
				findTriageTabPage.TabVisible = true;
				mainTabControl.SelectedTab = findTriageTabPage;
			}
		}

		ZDialogResult PromptSaveTriageSupportNotes()
		{
			var result = ZDialogResult.Yes;
			if (Triage != null && Triage.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(Res.GetString("B41387AB-5F94-4A16-8017-1E7143296BE0", "Do you want to save your changes to support notes?"), "Save", ZMessageBoxButtons.YesNoCancel, ZDialogResult.No);
				if (messageBoxResult == ZDialogResult.Yes)
				{
					result = SaveSupportNotes() ? ZDialogResult.Yes : ZDialogResult.Cancel;
				}
				else
				{
					result = messageBoxResult;
				}
			}
			return result;
		}

		bool SaveSupportNotes()
		{
			var result = true;
			Triage.RunPreSaveValidation();
			if (Triage.HasErrors)
			{
				Globals.Message.ShowError(new ZNotificationCollector(Triage, true, false, ZNotificationCollector.PropertyDescriptionType.None).ToMessageListString());
				result = false;
			}
			else
			{
				Triage.Factory.Save();
			}
			return result;
		}

		void SaveAndCloseButton_Click(object sender, EventArgs e)
		{
			if (Triage.HasErrors)
			{
				Globals.Message.ShowError(new ZNotificationCollector(Triage, true, false, ZNotificationCollector.PropertyDescriptionType.None).ToMessageListString());
				return;
			}

			if (!string.IsNullOrEmpty(ClientMessageBuilderTextBox.Text) && ParentIncidentForm != null)
			{
				if (!ParentIncidentForm.ConversationMessageTextBox.Enabled)
				{
					var messageBoxResult = Globals.Message.Show(Res.GetString("575ef27d-6776-4fa9-b02a-83272d519f52", "The Incident's eConversation is disabled. If you continue with this action, the client message on this form will be discarded. Alternatively you click No and make sure the eConversation is enabled before trying to save again. Would you like to continue and discard the client message?"), "Discard client message", ZMessageBoxButtons.YesNo, ZDialogResult.No);

					if (messageBoxResult == ZDialogResult.No)
					{
						return;
					}
				}
				else
				{
					ParentIncidentForm.ConversationMessageTextBox.Text += ClientMessageBuilderTextBox.Text;
				}
			}

			Helper.Incident.IM_IMT_Triage = Triage.PK;
			Helper.Factory.Save();
			Close();
		}
	}
}
