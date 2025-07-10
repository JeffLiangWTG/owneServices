using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentApprovalWithAuthorisationForm : ZForm, IButtonPostTextOverride, IButtonCancelTextOverride
	{
		public PaymentApprovalWithAuthorisationForm(PaymentApprovalWithAuthorisation paymentApprovalBizO)
			: base(paymentApprovalBizO)
		{
			PrepareForSaveAsDraft();
			SetupPostingButtons();
			SetupMenuItems();
			AH_NumberOfSupportingDocumentsCalcEdit.Visible = paymentApprovalBizO.AH_NumberOfSupportingDocumentsVisible_ReadOnly;
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			HookEvents();

			WorkflowTabPage.Initialize(paymentApprovalBizO);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
			{
				SetEPaymentControlVisibility();
				if (Payment.AV_Status == PaymentApprovalStatus.Cancelled)
				{
					AcceptQuoteButton.Enabled = false;
					CheckExRateButton.Enabled = false;
					ProcessEPaymentButton.Enabled = false;
				}
				AV_PaymentTypeInfo_ValueChanged(this, null);
				AV_ABInfo_ValueChanged(this, null);
			}
		}

		void SetEPaymentControlVisibility()
		{
			var allowEPayment = AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider;
			EPaymentTab.TabVisible = allowEPayment;
			CheckExRateButton.Visible = allowEPayment;
			ProcessEPaymentButton.Visible = allowEPayment;
			LearnMoreButton.Visible = allowEPayment;
			if (CheckExRateButton.Visible)
			{
				EnableOrDisableCheckExRateButton();
			}
		}

		void AV_PaymentTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Payment != null)
			{
				PaymentReasonDropEdit.Visible = Payment.IsEPayment;
				FundingBankAccountFindBox.Visible = Payment.IsEPayment;
				FundingCurrencyCodeFindBox.Visible = Payment.IsEPayment;
				UpdateAccountCaption(Payment);
			}
		}

		void UpdateAccountCaption(PaymentApprovalWithAuthorisation payment)
		{
			if (payment.IsEPayment)
			{
				BankAccountGuidFindBox.CaptionResourceString = Res.GetData("f17a9a96-2134-48cf-8d34-1ba18739d2b2", "E-Payment Account");
			}
			else
			{
				BankAccountGuidFindBox.CaptionResourceString = Res.GetData("51692eae-8540-4b05-8a28-8ae9a9759afa", "Bank Account");
			}
			BankAccountGuidFindBox.UpdateCaption();
		}

		#region Hook and Unhook Events

		void HookEvents()
		{
			if (Payment != null)
			{
				Payment.CurrencyCodeInfo.ValueChanged += (sender, e) => EnableOrDisableCheckExRateButton();
				Payment.AV_PaymentTypeInfo.ValueChanged += AV_PaymentTypeInfo_ValueChanged;
				Payment.AV_ABInfo.ValueChanged += AV_ABInfo_ValueChanged;
			}
			DisplayModeChanged += PaymentApprovalWithAuthorisationForm_DisplayModeChanged;
		}

		void UnhookEvents()
		{
			if (Payment != null)
			{
				Payment.CurrencyCodeInfo.ValueChanged -= (sender, e) => EnableOrDisableCheckExRateButton();
				Payment.AV_PaymentTypeInfo.ValueChanged -= AV_PaymentTypeInfo_ValueChanged;
				Payment.AV_ABInfo.ValueChanged -= AV_ABInfo_ValueChanged;
			}
			DisplayModeChanged -= PaymentApprovalWithAuthorisationForm_DisplayModeChanged;
		}

		#endregion

		void AV_ABInfo_ValueChanged(object sender, EventArgs e)
		{
			var shouldDisplayAdditionalEPaymentControls = Payment?.BankAccount?.IsEPaymentAccount ?? false;
			var paymentProviderCode = Payment?.BankAccount?.AB_PaymentProvider ?? ZString.Empty;

			DisclaimerMessageLabel.Visible = shouldDisplayAdditionalEPaymentControls;
			DisclaimerMessageLabel.Text = shouldDisplayAdditionalEPaymentControls ? PaymentApprovalEPaymentHelper.GetDisclaimerMessage(paymentProviderCode) : string.Empty;
			ServiceProviderLabel.Visible = shouldDisplayAdditionalEPaymentControls;
			ProviderLogoPictureBox.Visible = shouldDisplayAdditionalEPaymentControls;
			ProviderLogoPictureBox.Image = shouldDisplayAdditionalEPaymentControls ? EPaymentProviderLogoFinder.FindEPaymentProviderLogo(paymentProviderCode) : null;
		}

		void PrepareForSaveAsDraft()
		{
			SaveAsDraftButton = PostingButtonsUserControl.InsertAdditionalButton("SaveAsDraftButton", Res.GetString("cafd5242-9c32-43df-ac6f-c9a8701e59fd", "Save as Draft"), Icons.GetImage(IconTypes.SaveButtonActive), 3);
			SaveAsDraftButton.Click += SaveAsDraftButton_Click;
		}

		void SaveAsDraftButton_Click(object sender, EventArgs e)
		{
			SaveAsDraft();
		}

		bool SaveAsDraft()
		{
			var isSavedAsDraftSuccessful = false;
			if (Payment != null)
			{
				var errorMessage = Payment.CheckCanSaveAsDraft();
				Payment.RunPrePostingValidation();
				var continueSave = string.IsNullOrEmpty(errorMessage) && !Payment.HasErrors;

				if (!string.IsNullOrEmpty(errorMessage))
				{
					Globals.Message.ShowError(errorMessage);
				}
				else if (continueSave)
				{
					using (new DisposableAction(() => Payment?.Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft), () => Payment?.Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft)))
					{
						isSavedAsDraftSuccessful = FireSaveButton() == ContinueWithSave.Yes;
					}
				}
			}
			return isSavedAsDraftSuccessful;
		}

		void EnableOrDisableCheckExRateButton() => CheckExRateButton.Enabled = Payment != null && Payment.CanCreateQuotes;

		#region Hot Cheques

		void PaymentApprovalBizO_DisplayHotCheques(object sender, HotChequeLink link)
		{
			HotChequeLinkForm chequeLinkForm = GetHotChequeLinkForm(link);
			chequeLinkForm.Closed += new EventHandler(ChequeLinkForm_Closed);
			ZFormModaliser.ShowDialogAndDispose(chequeLinkForm);
		}

		protected virtual HotChequeLinkForm GetHotChequeLinkForm(HotChequeLink chequeLink)
		{
			return new HotChequeLinkForm(chequeLink);
		}

		void ChequeLinkForm_Closed(object sender, EventArgs e)
		{
			HotChequeLinkForm linkForm = sender as HotChequeLinkForm;
			if (linkForm != null && linkForm.SelectedHotCheque != null)
			{
				Payment.ImportSelectedHotCheque(linkForm.SelectedHotCheque);
			}
		}

		protected void Payment_NotifyUserPaymentUneditable(object sender, string message)
		{
			Globals.Message.ShowInformation(message, Res.GetString("f4c6ab16-cb88-493e-a2ce-d95462710e0f", "Hot Check Imported"));
		}

		#endregion

		public static string Authorise
		{
			get { return Res.GetString("Accounting|PaymentApprovalWithAuthorisationForm|Authorise", "Authorize"); }
		}
		public static string UnAuthorise
		{
			get { return Res.GetString("Accounting|PaymentApprovalWithAuthorisationForm|UnAuthorise", "Unauthorize"); }
		}

		protected ZDateEdit PostDateEdit;
		protected ZDateEdit InvoiceDateEdit;
		protected ZTextBox PaymentNoTextBox;
		protected ZButton FirstAuthorisationButton;
		ZLabel AutoAllocateZLabel;
		ZLabel AutoPrintZLabel;
		protected ZLabel Authorisation3rdLabel;
		protected ZLabel Authorisation2ndLabel;
		protected ZLabel Authorisation1stLabel;
		ZStmNoteTabPage zStmNoteTabPage1;
		protected ZCodeFindBox ThirdAuthorisationStaffCodeFindBox;
		protected ZCodeFindBox SecondAuthorisationStaffCodeFindBox;
		protected ZCodeFindBox FirstAuthorisationStaffCodeFindBox;
		protected ZGroupBox RejectGroupBox;
		protected ZDropEdit RejectReasonCodeDropEdit;
		protected ZTextBox RejectReasonTextBox;
		protected ZButton RejectButton;
		PaymentAddressWithContactControl AddressWithContactControl;
		protected ZButton CheckExRateButton;
		protected ZPostingButtonsUserControl PostingButtonsUserControl;
		ZTabControl TabControlBankAndEPayment;
		ZTabPage BankDetailsTab;
		ZTabPage EPaymentTab;
		ZGrid zGrid1;
		ZGroupBox zGroupBox1;
		ZButton refreshButton;
		protected ZToolStripButton SaveAsDraftButton;
		ZButton AcceptQuoteButton;
		ZButton ProcessEPaymentButton;
		ZWorkflowTabPage WorkflowTabPage;
		ZGroupBox DealGroupBox;
		ZTextBox DealProviderTextBox;
		ZTextBox DealReferenceTextBox;
		ZTextBox DealStatusTextBox;
		ZTextBox DealErrorTextBox;
		ZDateEdit DealSubmittedDateEdit;
		ZDateEdit DealResponseDateEdit;
		ZButton LearnMoreButton;
		ZLabel DisclaimerMessageLabel;
		KPictureBox ProviderLogoPictureBox;
		ZCalcFindBox DealCostCalcFindBox;
		ZLabel ServiceProviderLabel;
		ZTabPage AuthorizationTab;
		ZDropEdit PaymentReasonDropEdit;
		MenuItem CancelEPaymentMenuItem;

		protected void PaymentApprovalBizO_FirstApprovalStatusChanged(object sender, string message)
		{
			bool isApproved = Payment.Level1AuthorisationStatus == PaymentApprovalWithAuthorisation.AuthorisationStatus.Authorised;
			FirstAuthorisationButton.Text = isApproved ? UnAuthorise : Authorise;
		}

		protected void PaymentApprovalBizO_SecondApprovalStatusChanged(object sender, string message)
		{
			bool isApproved = Payment.Level2AuthorisationStatus == PaymentApprovalWithAuthorisation.AuthorisationStatus.Authorised;
			SecondAuthorisationButton.Text = isApproved ? UnAuthorise : Authorise;
		}

		protected void PaymentApprovalBizO_ThirdApprovalStatusChanged(object sender, string message)
		{
			bool isApproved = Payment.Level3AuthorisationStatus == PaymentApprovalWithAuthorisation.AuthorisationStatus.Authorised;
			ThirdAuthorisationButton.Text = isApproved ? UnAuthorise : Authorise;
		}

		protected void PaymentApprovalBizO_RequiredAuthorisationChanged(object sender, string message) => UpdateAuthorizationButtonReadonly();

		protected void PaymentApprovalBizO_AV_AKChanged(object sender, EventArgs e) => UpdateAuthorizationButtonReadonly();

		void UpdateAuthorizationButtonReadonly()
		{
			var cantEditAuth = Payment.IsRejected || Payment.IsCancelled || Payment.NoAuthorisationRequired;
			var rejectBoxReadOnly = cantEditAuth || !Payment.UserHasRejectSecurity;

			FirstAuthorisationButton.ReadOnly = cantEditAuth || !(Payment.UserHasAuthoriseLevel1Security && Payment.Level1AuthorisationRequired);
			SecondAuthorisationButton.ReadOnly = cantEditAuth || !(Payment.UserHasAuthoriseLevel2Security && Payment.Level2AuthorisationRequired);
			ThirdAuthorisationButton.ReadOnly = cantEditAuth || !(Payment.UserHasAuthoriseLevel3Security && Payment.Level3AuthorisationRequired);
			RejectReasonCodeDropEdit.ReadOnly = rejectBoxReadOnly;
			RejectReasonTextBox.ReadOnly = rejectBoxReadOnly;
			RejectButton.ReadOnly = rejectBoxReadOnly;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected virtual void PaymentApprovalWithAuthorisationForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			if (e.ToMode == ODisplayMode.ReadOnly || Payment.IsCancelled)
			{
				PaymentDetailButton.ReadOnly = true;
			}

			var enableSaveAsDraft = e.ToMode == ODisplayMode.New || e.ToMode == ODisplayMode.Edit;
			SaveAsDraftButton.Enabled = SaveAsDraftButton.Visible = enableSaveAsDraft;
		}

		#region Implementation

		string IButtonPostTextOverride.PostButtonText
		{
			get
			{ return Payment.PostsOnSave ? (Payment.IsInDatabase ? Res.GetString("Accounting.Posting.Buttons.Post", "Post") : Res.GetString("Accounting.Posting.Buttons.PaymentDetail", "Payment Detail")) : string.Empty; }
		}

		string IButtonCancelTextOverride.CancelButtonText
		{
			get { return Payment.PostsOnSave ? Res.GetString("69a13478-5115-49f3-91bc-4de709677120", "&Close") : string.Empty; }
		}

		public override string FormCaption
		{
			get { return Payment.DefaultDescription; }
		}

		protected PaymentApprovalWithAuthorisation Payment
		{
			get { return BusinessEntity as PaymentApprovalWithAuthorisation; }
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			WorkflowTabPage.SuspendLayout();
			WorkflowTabPage.ResumeLayout(false);
			WorkflowTabPage.PerformLayout();
		}

		#region Setup Posting Buttons

		protected virtual void SetupPostingButtons()
		{
			if (Payment.PostsOnSave)
			{
				PostingButtonsUserControl.SaveButton.Visible = false;
				ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl, true);
			}
			else
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			}
		}

		#endregion

		#region Setup Menu Items

		protected virtual void SetupMenuItems()
		{
			if (Payment.IsInDatabase && Payment.CurrentDeal != null)
			{
				CancelEPaymentMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("Accounting|PaymentApprovalWithAuthorisationForm|CancelEPaymentMenuName", "Cancel E-Payment"), CancelEPayment_Click);
			}
		}

		void CancelEPayment_Click(object sender, EventArgs e)
		{
			if (Payment.IsCancelEPaymentAllowed)
			{
				if (Payment.IsCancelEPaymentPossible)
				{
					DialogResult options = Globals.Message.Show(CancelEPaymentQuestion, Res.GetString("1cafd328-f03f-4fba-b9b1-f98b6db61283", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

					switch (options)
					{
						case DialogResult.Yes:
							Payment.CancelEPayment();
							break;
						case DialogResult.No:
							break;
					}
				}
				else
				{
					Globals.Message.ShowError(CancelEPaymentInvalidStatusError);
				}
			}
			else
			{
				Globals.Message.ShowError(Payment.CancelEPaymentSecurityCheckPoint.ErrorMessageForNotAllowed);
			}
		}

		string CancelEPaymentQuestion
		{
			get
			{
				return Res.GetString("3b5b801a-67cd-47a3-9add-b95672265542", @"You are attempting to change the status of this E-Payment to CAN.
You should only make this change if you have been in contact with your FX provider who have advised that they have manually canceled your E-Payment.
Changing the status here will have no impact on the status in your FX provider's system. If you have not been advised that they have canceled this E-Payment, depending on your payment method, funds for this payment could still be taken from your nominated bank account.
Do you wish to proceed?");
			}
		}

		string CancelEPaymentInvalidStatusError
		{
			get
			{
				return Res.GetString("4c219376-a391-41d3-b22d-e9389ad743a5", @"This option can only be used to cancel an E-Payment with a status of ACP or INP. 
Its purpose is to cancel E-Payments which have been manually canceled with your OFX provider.");
			}
		}

		#endregion

		#region Matching Form

		NewMatchGroupForm fMatchingForm;

		void fMatchingForm_Closed(object sender, EventArgs e)
		{
			if (fMatchingForm.SaveFactoryResult == SaveFactoryFlag.OK)
			{
				var saveResult = FireSaveButton();
				if (saveResult == ContinueWithSave.Yes)
				{
					if (Payment.IsPosted && Payment.PostsOnSave && Payment.TransactionHeader != null)
					{
						// Print the Payment
						PrintManager.Print();
					}
					Payment.ResetPaymentMatchingBaseObject();
				}
			}
			else if (fMatchingForm.SaveFactoryResult == SaveFactoryFlag.Cancel)
			{
				(Payment.MatchingBaseObject as PaymentApprovalMatchingBase)?.DeleteTemporaryTransactions();
				Payment.MatchingBaseObject?.DeleteAllBalancingJournals();
				Payment.ResetPaymentMatchingBaseObject();
			}
		}

		protected virtual PaymentPrintManager PrintManager
		{
			get
			{
				if (fPrintManager == null)
				{
					fPrintManager = GetPaymentPrintManager();
				}

				return fPrintManager;
			}
		}

		protected virtual PaymentPrintManager GetPaymentPrintManager()
		{
			return new PaymentPrintManager(Payment.TransactionHeader.PK.ToGuid(), ZArchitecture.Core.TransactionTypes.Payment, BusinessEntity.Factory);
		}

		PaymentPrintManager fPrintManager;

		#endregion

		#endregion

		#region ShowPreSaveDialogs Override

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave baseResult = base.ShowPreSaveDialogs();

			if (baseResult == ContinueWithSave.Yes)
			{
				bool isSessionBalancesEqualToZero = false;
				if (Payment.IsInDatabase)
				{
					Payment.IsLoadedFromGUI = false;
					isSessionBalancesEqualToZero = Payment.PaymentMatchingBaseObject.SessionBalancesToZero;
					if (!isSessionBalancesEqualToZero && !Payment.IsSavingPaymentApprovalAsDraft)
					{
						Payment.ResetPaymentMatchingBaseObject();
						Payment.IsLoadedFromGUI = true;
					}
				}
				else
				{
					isSessionBalancesEqualToZero = Payment.PaymentMatchingBaseObject.SessionBalancesToZero;
				}

				if ((Payment.AV_Amount == 0 || !isSessionBalancesEqualToZero) && !Payment.IsSavingPaymentApprovalAsDraft)
				{
					ShowMatchingForm();
					baseResult = ContinueWithSave.No;
				}
			}

			if (baseResult == ContinueWithSave.Yes)
			{
				Payment.UpdateDraftStatus();
			}

			return baseResult;
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			base.Save(factories);

			if (!Payment.IsDeleted && Payment.AV_Status == ZArchitecture.Core.PaymentApprovalStatus.Posted)
			{
				PaymentDetailButton.ReadOnly = true;
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ValidateAll(ValidationType.Light);
			if (((PaymentApprovalBase)BusinessEntity).HasErrors)
			{
				ShouldContinueWithSave = ContinueWithSave.No;
				ShowErrorsDialog();
			}
			else
			{
				ShouldContinueWithSave = ContinueWithSave.Yes;
			}

			if (ShouldContinueWithSave == ContinueWithSave.Yes)
			{
				ShouldContinueWithSave = base.ValidateAndSave();
			}
			return ShouldContinueWithSave;
		}

		ContinueWithSave ShouldContinueWithSave;

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			base.OnApplyButtonClick(sender, e);
			EnableOrDisableCheckExRateButton();
		}

		void ShowMatchingForm()
		{
			fMatchingForm = new NewMatchGroupForm(Payment.PaymentMatchingBaseObject);
			Payment.PaymentMatchingBaseObject.CreateTemporaryTransactions();
			fMatchingForm.HideMatchAndContinueButtonForReceiptPayment();
			fMatchingForm.Closed += new EventHandler(fMatchingForm_Closed);
			ZFormModaliser.Show(fMatchingForm, this);
		}

		#endregion

		#region IDataBoundControl Members

		new PaymentApprovalWithAuthorisation DataSource
		{
			get { return (PaymentApprovalWithAuthorisation)base.DataSource; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataSource.DisplayHotCheques -= PaymentApprovalBizO_DisplayHotCheques;
				DataSource.NotifyUserPaymentUneditable -= Payment_NotifyUserPaymentUneditable;

				DataSource.RequiredAuthorisationChanged -= PaymentApprovalBizO_RequiredAuthorisationChanged;
				DataSource.FirstApprovalStatusChanged -= PaymentApprovalBizO_FirstApprovalStatusChanged;
				DataSource.SecondApprovalStatusChanged -= PaymentApprovalBizO_SecondApprovalStatusChanged;
				DataSource.ThirdApprovalStatusChanged -= PaymentApprovalBizO_ThirdApprovalStatusChanged;
				DataSource.AV_AKInfo.ValueChanged -= PaymentApprovalBizO_AV_AKChanged;

				FirstAuthorisationStaffCodeFindBox.GetExtension<LabelCaptionRenderer>().DataBindings.RemoveBinding((NoResString)"Caption"); // Hard-coded constant
				SecondAuthorisationStaffCodeFindBox.GetExtension<LabelCaptionRenderer>().DataBindings.RemoveBinding((NoResString)"Caption"); // Hard-coded constant
				ThirdAuthorisationStaffCodeFindBox.GetExtension<LabelCaptionRenderer>().DataBindings.RemoveBinding((NoResString)"Caption"); // Hard-coded constant
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				DataSource.DisplayHotCheques += PaymentApprovalBizO_DisplayHotCheques;
				DataSource.NotifyUserPaymentUneditable += Payment_NotifyUserPaymentUneditable;

				DataSource.RequiredAuthorisationChanged += PaymentApprovalBizO_RequiredAuthorisationChanged;
				DataSource.FirstApprovalStatusChanged += PaymentApprovalBizO_FirstApprovalStatusChanged;
				DataSource.SecondApprovalStatusChanged += PaymentApprovalBizO_SecondApprovalStatusChanged;
				DataSource.ThirdApprovalStatusChanged += PaymentApprovalBizO_ThirdApprovalStatusChanged;
				DataSource.AV_AKInfo.ValueChanged += PaymentApprovalBizO_AV_AKChanged;

				PaymentApprovalBizO_RequiredAuthorisationChanged(DataSource, string.Empty);
				PaymentApprovalBizO_FirstApprovalStatusChanged(DataSource, string.Empty);
				PaymentApprovalBizO_SecondApprovalStatusChanged(DataSource, string.Empty);
				PaymentApprovalBizO_ThirdApprovalStatusChanged(DataSource, string.Empty);

				FirstAuthorisationStaffCodeFindBox.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", DataSource, "Level1AuthorisationStatus", true));
				SecondAuthorisationStaffCodeFindBox.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", DataSource, "Level2AuthorisationStatus", true));
				ThirdAuthorisationStaffCodeFindBox.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", DataSource, "Level3AuthorisationStatus", true));
			}
		}

		#endregion

		#region IDisposable

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			UnhookEvents();
			base.Dispose(disposing);
		}

		#endregion

		#region Click Handlers

		protected void FirstAuthorisationButton_Click(object sender, EventArgs e)
		{
			if (Payment.HasActiveDeal)
			{
				PaymentApprovalEPaymentHelper.ActiveDealErrorMessage();
			}
			else if (Payment.IsDraft)
			{
				Globals.Message.ShowError(PaymentApprovalWithAuthorisation.DealWithDraftPaymentErrorMessage);
			}
			else if (Payment.Level1AuthorisationStatus == PaymentApprovalWithAuthorisation.AuthorisationStatus.AwaitingAuthorisation)
			{
				Payment.ApproveFirstApproval();
			}
			else
			{
				Payment.UnApproveFirstApproval();
			}
		}

		protected void SecondAuthorisationButton_Click(object sender, EventArgs e)
		{
			if (Payment.HasActiveDeal)
			{
				PaymentApprovalEPaymentHelper.ActiveDealErrorMessage();
			}
			else if (Payment.IsDraft)
			{
				Globals.Message.ShowError(PaymentApprovalWithAuthorisation.DealWithDraftPaymentErrorMessage);
			}
			else if (Payment.Level2AuthorisationStatus == PaymentApprovalWithAuthorisation.AuthorisationStatus.AwaitingAuthorisation)
			{
				Payment.ApproveSecondApproval();
			}
			else
			{
				Payment.UnApproveSecondApproval();
			}
		}

		protected void ThirdAuthorisationButton_Click(object sender, EventArgs e)
		{
			if (Payment.HasActiveDeal)
			{
				PaymentApprovalEPaymentHelper.ActiveDealErrorMessage();
			}
			else if (Payment.IsDraft)
			{
				Globals.Message.ShowError(PaymentApprovalWithAuthorisation.DealWithDraftPaymentErrorMessage);
			}
			else if (Payment.Level3AuthorisationStatus == PaymentApprovalWithAuthorisation.AuthorisationStatus.AwaitingAuthorisation)
			{
				Payment.ApproveThirdApproval();
			}
			else
			{
				Payment.UnApproveThirdApproval();
			}
		}

		protected void RejectButton_Click(object sender, EventArgs e)
		{
			if (Payment.HasActiveDeal)
			{
				PaymentApprovalEPaymentHelper.ActiveDealErrorMessage();
			}
			else if (Payment.IsDraft)
			{
				Globals.Message.ShowError(PaymentApprovalWithAuthorisation.DealWithDraftPaymentErrorMessage);
			}
			else if (string.IsNullOrWhiteSpace(Payment.AV_RejectionReasonCode) || !Payment.RejectionReasonCodesList.ContainsCode(Payment.AV_RejectionReasonCode))
			{
				Globals.Message.ShowError(Res.GetString("20c6cd10-0602-430c-bdd4-3559205e5645", "Please choose a valid reason code"));
			}
			else if (string.IsNullOrWhiteSpace(Payment.AV_RejectionReasonDetails))
			{
				Globals.Message.ShowError(Res.GetString("43ea89c3-f34d-4abc-b551-7c96f1d77015", "Please enter a non-blank reason text"));
			}
			else
			{
				var errorBuffer = new NotificationBuffer();
				Payment.TryRejectPayment(errorBuffer);
				if (errorBuffer.HasErrors)
				{
					Globals.Message.ShowError(errorBuffer.AsString.Trim());
				}
			}
		}

		protected void PaymentDetailButton_Click(object sender, EventArgs e)
		{
			using (new DisposableAction(() => Payment?.Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft), () => Payment?.Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft)))
			{
				Payment.Validation.ValidateAll();
				if (Payment.HasErrors)
				{
					ShowErrorsDialog();
				}
				else
				{
					ShowMatchingForm();
				}
			}
		}

		void CheckExRateButton_Click(object sender, EventArgs e)
		{
			try
			{
				var changeTab = false;
				if (Payment.IsInDatabase || SaveAsDraft())
				{
					if (Payment.HasActiveDeal)
					{
						PaymentApprovalEPaymentHelper.ActiveDealErrorMessage();
					}
					else if (!Payment.HasChanges)
					{
						var (status, userMessage) = EPaymentDealCreator.ValidateExRate(Payment);
						if (status != EPaymentDealCreator.QuoteAcceptingStatus.NoErrors)
						{
							switch (status)
							{
								case EPaymentDealCreator.QuoteAcceptingStatus.ValidationErrors:
									Globals.Message.ShowError(userMessage);
									break;
								case EPaymentDealCreator.QuoteAcceptingStatus.UserNotAuthorized:
									PaymentApprovalEPaymentHelper.PromptUserToAuthorize(Payment, userMessage);
									break;
								default:
									throw new DeveloperNotificationException(FormattableString.Invariant($"Unexpected QuoteAcceptingStatus value '{status}' found. Please implement processing logic for it here."));
							}
							return;
						}
						var (quote, errorMessage, quoteStatus) = Payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
						if (quoteStatus != EPaymentDealCreator.QuoteAcceptingStatus.NoErrors)
						{
							switch (quoteStatus)
							{
								case EPaymentDealCreator.QuoteAcceptingStatus.ValidationErrors:
									Globals.Message.ShowError(errorMessage);
									break;
								case EPaymentDealCreator.QuoteAcceptingStatus.QuoteRequested:
									var caption = Res.GetString("2f616c4d-5539-49f3-8850-7104b139fa98", "Quote already requested");
									var message = Res.GetString("1988db76-5213-45b9-a32c-9821cc4cd8b2", "E-Quote already requested. Response may take up to several minutes to be received. If you generate a new request, then the previous request will be discarded. Are you sure you want to proceed?");
									var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
									if (dialogResult == DialogResult.Yes)
									{
										quote = Payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX, true).Quote;
									}
									break;
							}
						}
						if (quote != null)
						{
							Payment.DiscardActiveFXQuotes(quote.QU_ProviderCode, discardOnlyIfPaymentDetailsAreChanged: false);
							quote.Factory.Save();
							Globals.Message.ShowInformation(Res.GetString("72696b1b-4275-4fd5-95aa-527fe833269a", "E-Quote request generated to service provider {0}. Response may take from a few moments up to several minutes to be received.", quote.QU_ProviderCode));
							changeTab = true;
							RefreshButton_Click(sender, e);
						}
					}
					else
					{
						PaymentApprovalEPaymentHelper.SavePaymentApprovalMessage();
					}
				}

				if (changeTab)
				{
					TabControlBankAndEPayment.SelectTab("EPaymentTab");
					Payment.PaymentQuotes.RefreshBindingIncludingChildren();
				}
			}
			catch (ZCannotSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			Payment.RefreshQuotes();
			Payment.RefreshCurrentDeal();
			Payment.RefreshBindingIncludingChildren();
		}

		void AcceptQuoteButton_Click(object sender, EventArgs e)
		{
			if (PaymentApprovalEPaymentHelper.TryToAcceptQuote(Payment, zGrid1.SelectedElements.Cast<EPaymentQuoteForDisplay>().Select(x => x.RealQuote).ToArray()))
			{
				TabControlBankAndEPayment.SelectTab("BankDetailsTab");
				SaveAsDraftButton.Enabled = false;
				PostingButtonsUserControl.SaveButton.Enabled = false;
				PostingButtonsUserControl.SaveAndCloseButton.Enabled = false;
			}
		}

		void ProcessEPaymentButton_Click(object sender, EventArgs e)
		{
			if (PaymentApprovalEPaymentHelper.TryToCreateDeal(Payment, () => CheckExRateButton_Click(this, EventArgs.Empty)))
			{
				TabControlBankAndEPayment.SelectTab("EPaymentTab");
				SaveAsDraftButton.Enabled = false;
				PostingButtonsUserControl.SaveButton.Enabled = false;
				PostingButtonsUserControl.SaveAndCloseButton.Enabled = false;
				RefreshButton_Click(sender, e);
			}
		}

		void LearnMoreButton_Click(object sender, EventArgs e) => EPaymentUrlLauncher.LaunchEPaymentProductMarketingURL();

		void ProviderLogoPictureBox_Click(object sender, EventArgs e)
		{
			var paymentProviderCode = Payment?.BankAccount?.AB_PaymentProvider ?? ZString.Empty;
			EPaymentUrlLauncher.LaunchEPaymentProviderURL(paymentProviderCode);
		}

		#endregion
	}
}

