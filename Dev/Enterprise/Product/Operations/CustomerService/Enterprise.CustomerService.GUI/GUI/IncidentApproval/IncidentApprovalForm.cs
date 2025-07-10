using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.RemoteDesktopServices;
using Enterprise.Security;
using Enterprise.UserPortal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CustomerService.GUI
{
	public partial class IncidentApprovalForm : ZTemplateForm
	{
		public IncidentApprovalForm(IncidentApproval incident)
			: base(incident)
		{
			InitializeComponent();

			PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.AllowOverlap(SaveButtonUserControl);

			if (!DesignModeFinder.IsDesigning)
			{
				SetControlsVisibleAndEnable();
			}
		}

		/// <summary>
		/// Form becomes read-only when incident is sent.
		/// </summary>
		void SetReadOnlyIfNeeded(bool updateControl)
		{
			if (IncidentApproval.HasBeenSentNoIncidentNumberAssigned && IsEditable(DisplayMode))
			{
				DisplayMode = ODisplayMode.ReadOnly;
				if (updateControl)
				{
					SetReadOnlyIncludingChildren();
				}
			}
		}

		public override string FormCaption
		{
			get
			{
				string result = new IncidentApprovalData().HumanReadableName;
				var incident = IncidentApproval;
				if (incident != null && !incident.IA_ClientReference.IsEmpty)
				{
					result += " " + incident.IA_ClientReference
						+ (!incident.IA_IncidentNumber.IsEmpty ? "/" + incident.IA_IncidentNumber : "");
				}
				return result;
			}
		}

		static bool IsEditable(ODisplayMode mode)
		{
			return mode == ODisplayMode.Edit || mode == ODisplayMode.Browse || mode == ODisplayMode.New || mode == ODisplayMode.NewSaved;
		}

		protected override void OnLoad(EventArgs e)
		{
			// Do this before calling the base class, since the base class will update the form controls
			SetReadOnlyIfNeeded(false);

			base.OnLoad(e);
		}

		IncidentApproval IncidentApproval
		{
			get { return (IncidentApproval)BusinessEntity; }
		}

		#region Set Up Controls

		void SetControlsVisibleAndEnable()
		{
			MainTabControl.TabPages.Remove(RedirectToPortalTabPage);

			if (IncidentApproval.HasBeenSent && !IncidentApproval.IA_IncidentNumber.IsEmpty)
			{
				MainTabControl.TabPages.InsertPage(RedirectToPortalTabPage, 0);
			}

			SetupCriticalityDropDown();
			UpdateButtonsVisible();
			UpdateButtonsEnabled();
			SetupApproveAndSaveButtons();
		}

		void SetupCriticalityDropDown()
		{
			IncidentApproval.IA_CriticalityInfo.ValueChanged += new EventHandler(IA_CriticalityInfo_ValueChanged);
		}

		/// <summary>
		///  All button visibility changes go here.
		/// </summary>
		void UpdateButtonsVisible()
		{
			SaveAwaitingApprovalButton.Visible = !IncidentApproval.HasBeenSent;
			ApproveButton.Visible = !IncidentApproval.HasBeenSent;
		}

		/// <summary>
		///  All button enabling/disabling goes here.
		/// </summary>
		void UpdateButtonsEnabled()
		{
			if (IncidentApproval != null)
			{
				// Approve button is hidden once the request has been sent.
				ApproveButton.Enabled = (!IncidentApproval.HasBeenSent || IncidentApproval.HasChanges);

				// SaveAwaitingApprovalButton is hidden once the request has been sent.
				SaveAwaitingApprovalButton.Enabled = IncidentApproval.IA_Status.IsEmpty || IncidentApproval.HasChanges;
			}
			else
			{
#if !WINZOR
				var isRemoteAppSession = ObjectFactory.Get<TerminalService>().IsRemoteAppSession;
#else
				var isRemoteAppSession = (NoResString)"false (always false in Winzor)";
#endif
				ErrorReporter.ReportOnce("IncidentApprovalForm.UpdateButtonsEnabled()",
															"IncidentApproval=null"
															+ ", Form.Visible=" + this.Visible
															+ ", Form.IsDisposed=" + this.IsDisposed
															+ ", TerminalService.IsRemoteAppSession=" + isRemoteAppSession);
			}
		}

		void SetupApproveAndSaveButtons()
		{
			IncidentApproval.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(IncidentApproval_HasChangesChanged);
		}

		#endregion

		#region On Shown

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (!IncidentApproval.IsInDatabase)
			{
				if (ShowImportantNotice() == DialogResult.OK)
				{
					IncidentApprovalControl.FocusOnIncidentCriticalityAndShowToolTip();
				}
				else
				{
					DisplayMode = ODisplayMode.Browse;
					Close();
				}
			}

			SetupButtonsPanelAndFunctionAssignment();
			HideInvalidMenuItems();
		}

		void SetupButtonsPanelAndFunctionAssignment()
		{
			if (DisplayMode == ODisplayMode.Delete)
			{
				PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = false;
			}
			else
			{
				//AssignButtonsInternal sets DisplayMode to ODisplayMode.Browse
				ODisplayMode currentMode = DisplayMode;
				((IPostingButtonsProvider)this).AssignButtonsInternal(null, CloseButton, SaveAwaitingApprovalButton);
				DisplayMode = currentMode;
			}
		}

		void HideInvalidMenuItems()
		{
			if (FileMenuItem != null)
			{
				foreach (MenuItem menuItem in FileMenuItem.MenuItems)
				{
					if (menuItem.Name == ZFormMenuStrategy.FileNewMenuItemName
						|| menuItem.Name == ZFormMenuStrategy.FileSaveMenuItemName
						|| menuItem.Name == ZFormMenuStrategy.FileSaveAndCloseMenuItemName
						|| menuItem.Name == ZFormMenuStrategy.FileSeperator1MenuItemName
						|| menuItem.Name == ZFormMenuStrategy.FileSeperator2MenuItemName)
					{
						menuItem.Enabled = false;
						menuItem.Visible = false;
					}
				}
			}
		}

		#endregion

		#region Approve and Send

		ZString statusToRestoreIfStatusChangeFails;

		void ValidateAndSend(string newStatus = "")
		{
			statusToRestoreIfStatusChangeFails = IncidentApproval.IA_Status;
			if (!string.IsNullOrEmpty(newStatus))
			{
				IncidentApproval.IA_Status = newStatus;
			}

			try
			{
				ValidateAndSendCore();
				UpdateButtonsEnabled();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleSaveException(ex);
			}
			finally
			{
				if (DisplayMode != ODisplayMode.Browse && DisplayMode != ODisplayMode.ReadOnly)
				{
					// save failed
					IncidentApproval.IA_Status = statusToRestoreIfStatusChangeFails;
				}
			}
		}

		void ValidateAndSendCore()
		{
			if (!Env.CurrentUser.IsOperational)
			{
				Globals.Message.ShowError(SecurityCore.NonOperationalErrorMessage, Res.GetString("34937ff5-ed7a-40c4-8d4e-e47ea693f43a", "Access Denied"));
				return;
			}

			bool isFirstSend = !IncidentApproval.HasBeenSent;

			ContinueWithSave result = ValidateAndSave();
			if (result == ContinueWithSave.Yes
				&& IncidentApproval.NeedConfirmCriticality
				&& ShowCriticalityfirmationMessage() != DialogResult.OK)
			{
				result = ContinueWithSave.No;
			}

			if (result == ContinueWithSave.Yes)
			{
				//If new incident then send via eHub, otherwise update already been sent via web service
				if (isFirstSend && IncidentApproval.Approve())
				{
					ShowRequestSentMessage();
				}

				if (DisplayMode == ODisplayMode.New)
				{
					DisplayMode = ODisplayMode.NewSaved;
				}

				SetReadOnlyIfNeeded(true);
			}
		}

		void IncidentApproval_MemoryDumpUploadComplete(object sender, EventArgs e)
		{
			ErrorEventArgs error = e as ErrorEventArgs;
			if (error == null)
			{
				Globals.Message.ShowInformation(Res.GetString("1e9d1112-3c7f-4efc-a8d2-718dc8576055", "Memory dump has been successfully uploaded to CargoWise server."));
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("0385EFB2-F26A-486E-91EE-6A24CEDF1050", "Memory dump could not be uploaded to CargoWise server.\r\nYour request will be processed without this information.")
					+ "\r\n\r\n" + error.GetException().ToString());
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();
			if (result == ContinueWithSave.Yes)
			{
				CloseButton.Text = ZFormPostingButtonsStrategy.DefaultCloseButtonText;
			}
			return result;
		}

		#endregion

		#region Show Messages

		static DialogResult ShowImportantNotice()
		{
			return ZFormModaliser.ShowDialogAndDispose(new ImportantNoticeForm());
		}

		DialogResult ShowCriticalityfirmationMessage()
		{
			DialogResult result = DialogResult.OK;

			ZString confirmationString;
			switch (IncidentApproval.IA_Criticality)
			{
				case Constants.CustomerService.CriticalityCodes.CR2_ModuleDown:
					confirmationString = Res.GetString("3806228A-FEBC-489E-8BF4-2D389247023B", "ENTIRE MODULE NOT WORKING WITH NO MANUAL WORK AROUND");
					break;
				case Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround:
					confirmationString = Res.GetString("8F3CE684-071A-42EE-A7EB-EA859DA71389", "SINGLE FUNCTION NOT WORKING WITH NO MANUAL WORK AROUND");
					break;
				case Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround:
					confirmationString = Res.GetString("F9817CFC-F848-4C4B-B60C-AB2627F8DC77", "SINGLE FUNCTION NOT WORKING WITH MANUAL WORK AROUND");
					break;
				default:
					confirmationString = ZString.Empty;
					break;
			}

			if (!confirmationString.IsEmpty)
			{
				result = Globals.Message.ShowConfirmation(
					Res.GetString("71b9f7fb-86ce-4451-8a14-73be5220b5e0", "This Criticality {0} means \"{1}\".", IncidentApproval.IA_Criticality, confirmationString),
					Res.GetString("05e45f04-0362-4f64-af82-e27b2b6611a3", "Criticality Confirmation"),
					Res.GetString("d7d7a47a-b558-4f1c-aac2-6a4aa2f959ae", "Please confirm this is the case by typing the follow words:"),
					confirmationString, MessageBoxIcon.Warning, ConfirmationMessageLayout.LineBreakAfterEachPart);
			}

			return result;
		}

		static DialogResult ShowRequestQuoteMessage()
		{
			string confirmationString = (NoResString)"I HAVE AUTHORITY TO REQUEST A QUOTATION AND SIGN ORDERS FOR SOFTWARE DEVELOPMENT REQUIRED BY THIS QUOTE REQUEST";
			return Globals.Message.ShowConfirmation(
				Res.GetString("7e74a1ca-4953-4c88-9a80-abec94ca1599", "Because of the cost and effort of CargoWise providing a full formal and firm quotation and design, the request, once made, will be subject to a cancellation fee.\r\nPlease confirm that you would like to formally request a Quote and Order for this feature request.\r\n\r\nPlease note that formal requests will be subject to a cancellation fee:\r\n- If you choose to decline a formal Order, or\r\n- If no response is received within 30 days of the Order being issued\r\n\r\nIf the amount on the Order exceeds the High End estimate provided prior to this, the cancellation fee is waived."),
				Res.GetString("8fee76d7-c339-452e-9f07-e9941878bdf3", "Confirm Formal Quotation Request"),
				Res.GetString("48c755d2-dddb-4f88-89b8-1b2880e2e648", "Please retype this message:"),
				confirmationString, MessageBoxIcon.Warning, ConfirmationMessageLayout.LineBreakAfterEachPart);
		}

		static void ShowRequestSentMessage()
		{
			Globals.Message.ShowInformation(Res.GetString("5d87a898-07dd-4ea4-bafd-78cc7b96473d", "Your eRequest has been sent to CargoWise and will be responded to as soon as possible."), Res.GetString("13358964-1339-4af4-b253-911c3db93b40", "Request Sent"));
		}

		static void ShowUpdateSentMessage()
		{
			Globals.Message.ShowInformation(Res.GetString("8b0f2ea2-1a04-4694-be61-7aed9df4f0a4", "Your update has been sent to CargoWise."), Res.GetString("c35048d7-7fa7-4b9c-b242-708bcf7d117f", "Updated"));
		}

		#endregion

		#region Event Handlers

		#region Status Actions

		void ApproveButton_Click(object sender, EventArgs e)
		{
			if (!IncidentApproval.HasBeenSent && !IncidentApproval.CanApprove)
			{
				Env.Security.IncidentApprovalApprove.ShowError();
				return;
			}

			if (IncidentApproval.IA_Status == IncidentApprovalLookups.StatusCodes.ApprovedAndSent)
			{
				Globals.Message.Show(Res.GetString("10487d79-4a94-4474-bcfc-e5fb4d2442d9", "This request has already been sent to CargoWise - please contact CargoWise for more information."));
				return;
			}

			ValidateAndSend();
		}

		bool ValidateAndSendStatusChange(ZString newStatus)
		{
			ValidateAndSend(newStatus);
			bool ok = DisplayMode == ODisplayMode.Browse;
			return ok;
		}

		void RequestQuoteButton_Click(object sender, EventArgs e)
		{
			if (ShowRequestQuoteMessage() == DialogResult.OK && ValidateAndSendStatusChange(IncidentApprovalLookups.StatusCodes.FormalQuotationRequested))
			{
				ShowUpdateSentMessage();
			}
		}

		void AcceptQuoteButton_Click(object sender, EventArgs e)
		{
			if (Globals.Message.Show(
				Res.GetString("746aac81-2b3e-4bbf-9abc-50e9d25682d2", "You are about to accept the formal quotation, do you wish to continue?"),
				Res.GetString("58a37df3-ea9a-451d-8dce-3739d06cbde2", "Confirm Accept Formal Quotation"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes)
			{
				if (ValidateAndSendStatusChange(IncidentApprovalLookups.StatusCodes.FormalQuotationAccepted))
				{
					ShowUpdateSentMessage();
				}
			}
		}

		void DeclineQuoteButton_Click(object sender, EventArgs e)
		{
			if (Globals.Message.Show(
				Res.GetString("e064258a-69df-4970-8330-14eda106caf6", "You are about to decline the formal quotation, do you wish to continue?"),
				Res.GetString("a7ba7029-f726-4cea-9e2f-43c62f723f34", "Confirm Decline Formal Quotation"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes)
			{
				if (ValidateAndSendStatusChange(IncidentApprovalLookups.StatusCodes.FormalQuotationDeclinded))
				{
					ShowUpdateSentMessage();
				}
			}
		}

		void SaveAwaitingApprovalButton_Click(object sender, EventArgs e)
		{
			//To pass FormBasher test - Do nothing here because button function is attached by AssignButtonsInternal()
		}

		#endregion

		#region Criticality Change

		void IA_CriticalityInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!inCriticalityValueChangedHandler)
			{
				inCriticalityValueChangedHandler = true;

				var valueChangedEventArgs = e as ValueChangedEventArgs;
				if (IncidentApproval != null && valueChangedEventArgs != null && valueChangedEventArgs.OldValue != valueChangedEventArgs.NewValue)
				{
					if (IncidentApproval.IA_Criticality == Constants.CustomerService.CriticalityCodes.CR1_SystemDown)
					{
						HandleChangeToCR1(valueChangedEventArgs);
					}
					else if (IncidentApproval.IA_Criticality == Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest)
					{
					}
					else if (IncidentApproval.HasBeenSent)
					{
					}
					else
					{
						if (string.IsNullOrEmpty(IncidentApproval.IA_Module) && IncidentApproval.ModuleType == ModuleListType.MenuSection)
						{
							IncidentApproval.IA_Module = IncidentApproval.MenuSectionCode;
						}
					}
				}

				inCriticalityValueChangedHandler = false;
			}
		}
		bool inCriticalityValueChangedHandler;

		void HandleChangeToCR1(ValueChangedEventArgs valueChangedEventArgs)
		{
			Globals.Message.ShowError(Res.GetString("82ca5309-84bd-4c4b-84e8-fd59c2d41041", "You cannot lodge a genuine CR1 request electronically. CR1 means \"Entire System down/System failure\".\r\nPlease re-classify your incident to the appropriate criticality."),
									Res.GetString("5e513528-2ff4-410a-ba56-eae7e538b5a5", "Cannot Lodge CR1 Request"));
			ResetToLastCriticality(valueChangedEventArgs);
		}

		void ResetToLastCriticality(ValueChangedEventArgs valueChangedEventArgs)
		{
			valueChangedEventArgs.Info.Value = valueChangedEventArgs.OldValue;
			IncidentApprovalControl.SetCriticality(IncidentApproval.IA_Criticality);
		}

		#endregion

		#region Has Changes

		void IncidentApproval_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (IncidentApproval != null && !IncidentApproval.IsDeleted)
			{
				UpdateButtonsEnabled();

				if (IncidentApproval.HasChanges)
				{
					CloseButton.Text = Res.GetString("cac4a553-6abe-4d31-a76a-47f991b3854d", "Cancel");
				}
				else
				{
					CloseButton.Text = ZFormPostingButtonsStrategy.DefaultCloseButtonText;
				}
			}
		}

		#endregion

		#region eDocs Grid Update

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			IncidentApproval.AttachedEDocs.Load();
		}

		#endregion

		#endregion

		#region Show Notes Tab

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		#endregion

		void portalButton_Click(object sender, EventArgs e)
		{
			new UserPortalLauncher().GoToExistingIncident(IncidentApproval.IA_IncidentNumber);
		}
	}
}
