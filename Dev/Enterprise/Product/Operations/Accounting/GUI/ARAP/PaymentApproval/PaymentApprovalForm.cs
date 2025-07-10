using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentApprovalForm : ZForm
	{
		public PaymentApprovalForm()
		{
		}

		public PaymentApprovalForm(PaymentApprovalBase paymentApprovalBizO)
			: base(paymentApprovalBizO)
		{
			if (paymentApprovalBizO.Ledger != LedgerTypes.AccountsPayable)
			{
				PostWithoutMatchingButton.Click -= PostWithoutMatchingButton_Click;
				PostWithoutMatchingButton.Dispose();

				SaveAsDraftButton.Location = ControlDpiScalingHelper.NewScaledPoint(274, PaymentDetailButton.Location.Y, true);
			}

			SetupPostingButtons();

			AdjustLocationOfProcessEPaymentButton();

			AH_NumberOfSupportingDocumentsCalcEdit.Visible = paymentApprovalBizO.AH_NumberOfSupportingDocumentsVisible_ReadOnly;

			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.Audit);
			HookEvents();
			ChequeNoTextBox.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", // Adding a binding manually
				BusinessEntity, "AV_Calc_ChequeReferenceLabel", true)); // Adding a binding manually

			WorkflowTabPage.Initialize(paymentApprovalBizO);
		}

		protected virtual void AdjustLocationOfProcessEPaymentButton()
		{
			if (Payment.Ledger != LedgerTypes.AccountsPayable)
			{
				var locationXForProcessEPaymentButton = SaveAsDraftButton.Bounds.X - ProcessEPaymentButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				ProcessEPaymentButton.Location = ControlDpiScalingHelper.NewScaledPoint(locationXForProcessEPaymentButton, PaymentDetailButton.Location.Y, true);
			}
		}

		#region Hook and Unhook Events

		void HookEvents()
		{
			if (Payment != null)
			{
				Payment.DisplayHotCheques += PaymentApprovalBizO_DisplayHotCheques;
				Payment.NotifyUserPaymentUneditable += Payment_NotifyUserPaymentUneditable;
				Payment.AV_PaymentTypeInfo.ValueChanged += AV_PaymentTypeInfo_ValueChanged;
				Payment.AV_RX_NKPaymentCurrencyInfo.ValueChanged += AV_RX_NKPaymentCurrencyInfo_ValueChanged;
				Payment.AV_ABInfo.ValueChanged += AV_ABInfo_ValueChanged;
			}
			DisplayModeChanged += PaymentApprovalForm_DisplayModeChanged;
		}

		void UnhookEvents()
		{
			if (Payment != null)
			{
				Payment.DisplayHotCheques -= PaymentApprovalBizO_DisplayHotCheques;
				Payment.NotifyUserPaymentUneditable -= Payment_NotifyUserPaymentUneditable;
				Payment.AV_PaymentTypeInfo.ValueChanged -= AV_PaymentTypeInfo_ValueChanged;
				Payment.AV_RX_NKPaymentCurrencyInfo.ValueChanged -= AV_RX_NKPaymentCurrencyInfo_ValueChanged;
				Payment.AV_ABInfo.ValueChanged -= AV_ABInfo_ValueChanged;
			}
			DisplayModeChanged -= PaymentApprovalForm_DisplayModeChanged;
		}

		#endregion

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Hot Cheques

		protected void PaymentApprovalBizO_DisplayHotCheques(object sender, HotChequeLink link)
		{
			HotChequeLinkForm chequeLinkForm = GetHotChequeLinkForm(link);
			chequeLinkForm.Closed += new EventHandler(ChequeLinkForm_Closed);
			ZFormModaliser.ShowDialogAndDispose(chequeLinkForm);
		}

		protected virtual HotChequeLinkForm GetHotChequeLinkForm(HotChequeLink chequeLink)
		{
			return new HotChequeLinkForm(chequeLink);
		}

		protected void ChequeLinkForm_Closed(object sender, EventArgs e)
		{
			HotChequeLinkForm linkForm = sender as HotChequeLinkForm;
			if (linkForm != null && linkForm.SelectedHotCheque != null)
			{
				Payment.ImportSelectedHotCheque(linkForm.SelectedHotCheque);
			}
		}

		protected void Payment_NotifyUserPaymentUneditable(object sender, string message)
		{
			Globals.Message.ShowInformation(message, Res.GetString("670208de-72a1-4739-b9ea-df4625b46d0a", "Hot Check Imported"));
		}

		#endregion

		#region Implementation

		public override ODisplayMode DisplayMode
		{
			get => Payment != null && !Payment.IsDeleted && Payment.IsDraft && Payment.IsProcessingPaymentDetail ? ODisplayMode.Edit : base.DisplayMode;
			set => base.DisplayMode = value;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignMode)
			{
				CardSecurityCodeTextBox.Visible = Payment.AV_PaymentType == ReceiptTypes.eNettCreditCard;
				SetEPaymentControlVisibility();
				if (Payment.AV_Status == PaymentApprovalStatus.Cancelled)
				{
					AcceptQuoteButton.Enabled = false;
					CheckEPayRateButton.Enabled = false;
					ProcessEPaymentButton.Enabled = false;
				}
				AV_PaymentTypeInfo_ValueChanged(this, null);
				AV_ABInfo_ValueChanged(this, null);
			}
		}

		void SetEPaymentControlVisibility()
		{
			var allowEPayment = AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider;
			EPaymentTabPage.TabVisible = allowEPayment;
			CheckEPayRateButton.Visible = allowEPayment;
			LearnMoreButton.Visible = allowEPayment;
			ProcessEPaymentButton.Visible = allowEPayment;
			if (CheckEPayRateButton.Visible)
			{
				EnableOrDisableCheckEPayRateButton();
			}
		}

		void AV_ABInfo_ValueChanged(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			var shouldDisplayAdditionalEPaymentControls = Payment?.BankAccount?.IsEPaymentAccount ?? false;
			var paymentProviderCode = Payment?.BankAccount?.AB_PaymentProvider ?? ZString.Empty;

			DisclaimerMessageLabel.Visible = shouldDisplayAdditionalEPaymentControls;
			DisclaimerMessageLabel.Text = shouldDisplayAdditionalEPaymentControls ? PaymentApprovalEPaymentHelper.GetDisclaimerMessage(paymentProviderCode) : string.Empty;
			ServiceProviderLabel.Visible = shouldDisplayAdditionalEPaymentControls;
			ProviderLogoPictureBox.Visible = shouldDisplayAdditionalEPaymentControls;
			ProviderLogoPictureBox.Image = shouldDisplayAdditionalEPaymentControls ? EPaymentProviderLogoFinder.FindEPaymentProviderLogo(paymentProviderCode) : null;
		}

		void AV_PaymentTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			if (Payment != null)
			{
				CardSecurityCodeTextBox.Visible = Payment.AV_PaymentType == ReceiptTypes.eNettCreditCard;
				PaymentReasonDropEdit.Visible = Payment.IsEPayment;
				FundingBankAccountFindBox.Visible = Payment.IsEPayment;
				FundingCurrencyCodeFindBox.Visible = Payment.IsEPayment;
				UpdateAccountCaption(Payment);
			}
		}

		void UpdateAccountCaption(PaymentApprovalBase payment)
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

		void AV_RX_NKPaymentCurrencyInfo_ValueChanged(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			EnableOrDisableCheckEPayRateButton();
		}

		void EnableOrDisableCheckEPayRateButton() => CheckEPayRateButton.Enabled = Payment != null && Payment.CanCreateQuotes;

		public override string FormCaption
		{
			get { return Payment == null ? ZString.Empty : Payment.DefaultDescription; }
		}

		protected PaymentApprovalBase Payment
		{
			get { return BusinessEntity as PaymentApprovalBase; }
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			WorkflowTabPage.SuspendLayout();
			WorkflowTabPage.ResumeLayout(false);
			WorkflowTabPage.PerformLayout();
		}

		#region eNett Related

		GetFxQuoteResult ENettGetExchangeRate()
		{
			using (new CursorSwitcher(Cursors.WaitCursor))
			{
				return Payment.GetEnettExchangeRate(x => Globals.Message.ShowError(x));
			}
		}

		#endregion

		#region Setup Posting Buttons

		protected virtual void SetupPostingButtons()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, PostWithoutMatchingButton);
		}

		#endregion

		protected ZButton PostWithoutMatchingButton;
		protected ZButton PaymentDetailButton;
		protected ZButton SaveAsDraftButton;
		protected ZButton CloseButton;

		#region Matching Form

		protected NewMatchGroupForm fMatchingForm;
		ZLabel AutoAllocateZLabel;
		ZLabel AutoPrintZLabel;
		ZStmNoteTabPage zStmNoteTabPage1;
		ZTextBox CardSecurityCodeTextBox;
		PaymentAddressWithContactControl AddressWithContactControl;
		ZButton CheckEPayRateButton;
		protected ZTabControl BankAndEPaymentTabControl;
		protected ZTabPage BankDetailsTabPage;
		protected ZTabPage EPaymentTabPage;
		ZGrid EPaymentGrid;
		ZGroupBox EQuoteGroupBox;
		ZButton RefreshEPaymentButton;
		ZButton AcceptQuoteButton;
		protected ZButton ProcessEPaymentButton;
		protected ZButton SubmitForApprovalButton;
		protected ZButton ApproveForPostingButton;
		ZButton LearnMoreButton;
		KPictureBox ProviderLogoPictureBox;
		ZLabel ServiceProviderLabel;
		ZLabel DisclaimerMessageLabel;
		ZDropEdit PaymentReasonDropEdit;
		protected ZDropEdit StatusDropEdit;
		bool SaveIsBeingCalledFromMatchingForm;
		protected ZGuidFindBox FundingBankAccountFindBox;
		ZCodeFindBox FundingCurrencyCodeFindBox;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected void fMatchingForm_Closed(object sender, EventArgs e)
		{
			if (fMatchingForm.SaveFactoryResult == SaveFactoryFlag.OK)
			{
				using (SuspendReiterantEventsDueToDangerousApplicationDoEvents.GetSuspender())
				{
					try
					{
						SaveIsBeingCalledFromMatchingForm = true;

						fMatchingForm.Closed -= new EventHandler(fMatchingForm_Closed);
						fMatchingForm.Hide();
						Application.DoEvents();

						//User clicked Save in Matching Form.
						// Run ValidateAndSave() on the PaymentApprovalForm
						// for the second time
						FireSaveButton();
					}
					finally
					{
						SaveIsBeingCalledFromMatchingForm = false;

						if (Payment != null && Payment.IsInDatabase && Payment.IsDraft && Payment.IsSavingPaymentApprovalAsDraft)
						{
							Close();
						}
					}
				}
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
		protected override void HandleSaveException(Exception e)
		{
			if (e is AllocationSaveException)
			{
				Globals.Message.ShowError(((AllocationSaveException)e).UserFriendlyMessage, Res.GetString("0f9a9bb3-c701-4eff-a6ee-ff835ff968f1", "Check Book Busy"));
			}
			else if (e is AllocationChequeBookException)
			{
				Globals.Message.ShowError(((AllocationChequeBookException)e).UserFriendlyMessage, Res.GetString("e89dab34-5dad-4654-940e-d3bfb5c9fa8f", "Check Book Full"));
				if (fMatchingForm != null && fMatchingForm.Visible)
				{
					fMatchingForm.Close();
				}
			}
			else if (e is ENettProcessCreditCardException)
			{
				ENettProcessCreditCardException ex = e as ENettProcessCreditCardException;
				Globals.Message.ShowError(ex.UserFriendlyMessage + "\r\n" + Res.GetString("c630ce76-587d-40f6-8b0b-cbcf7f493f70", "ComPay Error: ({0}) {1}", ex.eNettErrorCode, ex.eNettErrorMessage), Res.GetString("eaf08c4b-d5be-4261-b2ed-352d845b504d", "ComPay Error"));
			}
			else if (e is ENettProcessDirectDebitFxException)
			{
				ENettProcessDirectDebitFxException ex = e as ENettProcessDirectDebitFxException;
				Globals.Message.ShowError(ex.Message + "\r\n" + Res.GetString("c630ce76-587d-40f6-8b0b-cbcf7f493f70", "ComPay Error: ({0}) {1}", ex.eNettErrorCode, ex.eNettErrorMessage), Res.GetString("c0f23d07-3e59-4b48-a28b-d02502e979f3", "Payment Failed"));
				Payment.UndoCreateNewPayment();
			}
			else
			{
				base.HandleSaveException(e);
			}
		}

		protected virtual PaymentPrintManager GetPaymentPrintManager()
		{
			return new PaymentPrintManager(Payment.TransactionHeader.PK.ToGuid(), TransactionTypes.Payment, BusinessEntity.Factory);
		}

		protected PaymentPrintManager fPrintManager;

		#endregion

		#endregion

		#region ShowPreSaveDialogs Override

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave baseResult = base.ShowPreSaveDialogs();
			if (baseResult == ContinueWithSave.Yes)
			{
				if (Payment.IsSavingPaymentApprovalAsDraft && !Payment.IsProcessingPaymentDetail)
				{
					Payment.UpdateDraftStatus();
				}
				else if (IsPostWithoutMatching && !Env.Security.NewPayablesPaymentAllowPostingWithoutMatching.IsAllowed)
				{
					baseResult = ContinueWithSave.No;
					Globals.Message.ShowError(Res.GetString("4ce64213-e6df-4f12-816e-07a79717e8ee", "{0} Posting Without Matching.", SecurityCore.SecurityErrorMessage));
				}
				else if (!IsPostWithoutMatching &&
					(Payment.AV_Amount == 0 || !SessionBalancesToZero ||
						(Payment.PostsOnSave && !SaveIsBeingCalledFromMatchingForm) || Payment.MatchingBaseObject.BalancingAPJournals.HasErrors() ||
						Payment.MatchingBaseObject.BalancingARJournals.HasErrors() || Payment.MatchingBaseObject.MatchedTransactions.HasErrors()))
				{
					using (Payment.PostPaymentWithMatchingSetExchangeSuspender.GetSuspender())
					{
						ShowMatchingForm();
						baseResult = ContinueWithSave.No;
					}
				}
			}

			return baseResult;
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave continueWithSaveResult;
			ValidateAll(ValidationType.Light);

			if (Payment.HasErrors)
			{
				continueWithSaveResult = ContinueWithSave.No;
				ShowErrorsDialog();
			}
			else if (Payment.IsHotChequeImported || Payment.IsSavingPaymentApprovalAsDraft)
			{
				continueWithSaveResult = ContinueWithSave.Yes;
			}
			else
			{
				continueWithSaveResult = MessageHelper.ShowMessageIfChequeBookUsesSamePrinterReturnsCancel(((PaymentApprovalBase)BusinessEntity).ChequeBook) ? ContinueWithSave.No : ContinueWithSave.Yes;
			}

			if (continueWithSaveResult == ContinueWithSave.Yes && Payment.UseExchangeRateFromENettWebService)
			{
				GetFxQuoteResult quoteResult = ENettGetExchangeRate();

				if (quoteResult.LocalAmount != Payment.AV_Calc_LocalAmount)
				{
					if (IsPostWithoutMatching || Payment.HasPaymentMatchingBaseObjectBeenCreated)
					{
						DialogResult result = DialogResult.Cancel;

						while (result == DialogResult.Cancel)
						{
							result = new CountdownMessageBox(Res.GetString("bec9f540-268f-4bdc-8ab6-40899086c1d7", @"ComPay exchange rate for {0} has changed since it was last retrieved.
Current exchange rate is {1}.
Overseas payment amount is {2} {3}.
Local payment amount is {4} {5}.

Do you wish to use the updated exchange rate for this payment?

If you click 'Yes', the exchange rate will be updated and saving will continue.
If you click 'No', the exchange rate will not be updated and saving will be canceled.
If you click 'Refresh', a new exchange rate will be retrieved.", Payment.CurrencyCode, quoteResult.Rate, Payment.AV_Amount, Payment.AV_RX_NKPaymentCurrency, quoteResult.LocalAmount, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency), Res.GetString("781fe72a-3577-426b-b88f-3102a8a616d1", "Confirm Exchange Rate Update"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, 20).ShowDialog(this);

							if (result == DialogResult.Yes)
							{
								if (IsPostWithoutMatching)
								{
									Payment.AV_Calc_LocalAmount = quoteResult.LocalAmount;
								}
							}
							else if (result == DialogResult.No)
							{
								continueWithSaveResult = ContinueWithSave.No;
							}
							else
							{
								quoteResult = ENettGetExchangeRate();
							}
						}
					}

					if (!IsPostWithoutMatching && Payment.HasPaymentMatchingBaseObjectBeenCreated)
					{
						ExchangeDifference exchangeDifference = Payment.MatchingBaseObject.ExchangeDiffCurrent;

						if (exchangeDifference == null)
						{
							exchangeDifference = (ExchangeDifference)Payment.MatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
							Payment.MatchingBaseObject.AddMiscellaneousTransaction(exchangeDifference);
						}

						exchangeDifference.BindableOSAmount -= quoteResult.LocalAmount - Payment.AV_Calc_LocalAmount;
						Payment.AV_Calc_LocalAmount = quoteResult.LocalAmount;
					}
				}
			}

			if (continueWithSaveResult == ContinueWithSave.Yes)
			{
				continueWithSaveResult = base.ValidateAndSave();
			}

			return continueWithSaveResult;
		}

		AccountingMessageHelper MessageHelper
		{
			get
			{
				if (fMessageHelper == null)
				{
					fMessageHelper = new AccountingMessageHelper();
				}
				return fMessageHelper;
			}
		}
		AccountingMessageHelper fMessageHelper;

		protected override void Save(ITransactionParticipant[] factories1)
		{
			if (Payment != null)
			{
				bool isNew = !Payment.IsInDatabase;
				bool isPostingDraftPaymentWithoutMatching = (ZString)Payment.AV_StatusInfo.OriginalValue == PaymentApprovalStatus.Draft && IsPostWithoutMatching;
				bool isPostingDraftPaymentWithMatching = Payment.IsDraft && SaveIsBeingCalledFromMatchingForm && !Payment.IsSavingPaymentApprovalAsDraft && Payment.PaymentType != ReceiptTypes.EPayment;

				if (isPostingDraftPaymentWithMatching || isPostingDraftPaymentWithoutMatching)
				{
					Payment.SetContext(BusinessContext.PostDraftPaymentApproval);
					if (isPostingDraftPaymentWithMatching)
					{
						Payment.AV_Status = PaymentApprovalStatus.FullyApproved;
					}
				}

				if (IsPostWithoutMatching)
				{
					if (Payment.HasPaymentMatchingBaseObjectBeenCreated)
					{
						Payment.PaymentMatchingBaseObject.MoveAllFromMatchToUnmatch();
					}
					Payment.AV_Discount = 0M;
					Payment.AV_ExchangeDifference = 0M;
				}

				try
				{
					if (((IChequeNumberAutoAllocation)Payment).IsAutoAllocationEnabled)
					{
#if DEBUG
						if (Globals.IsTest && !Globals.GetIsUnitTestingProductionFunctionality())
						{
							Test_Allocator = new PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator(Payment, PaymentChequeNumberAllocator.PrintingMode.PaymentApproval, Payment.Factory);
							Test_Allocator.SetChequeBookToInactiveOnSaving = Test_DeactivateChequeBookOnAllocation;
							base.Save(Test_Allocator.GetFactoriesForTest());
						}
						else
						{
#endif
							var allocator = new PaymentChequeNumberAllocator(Payment, PaymentChequeNumberAllocator.PrintingMode.PaymentApproval, Payment.Factory);
							base.Save(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(factories1));
#if DEBUG
						}
#endif
					}
					else
					{
						List<ITransactionParticipant> factories = new List<ITransactionParticipant>(factories1);
						if ((SaveIsBeingCalledFromMatchingForm || IsPostWithoutMatching))
						{
							if (Payment != null && Payment.AV_PaymentType == ReceiptTypes.eNettCreditCard
								&& Payment.BankAccount != null
								&& Payment.BankAccount.IsCreditCardOrLinkedAccount
								)
							{
								eNettPaymentTransactionParticipant transactionParticipant = new eNettPaymentTransactionParticipant(Payment);
								factories.Add(transactionParticipant);
							}
							else if (Payment != null && Payment.AV_PaymentType == ReceiptTypes.eNettDirectDebit &&
								Payment.BankAccount != null &&
								Payment.IsENettPayment)
							{
								eNettPaymentTransactionParticipant transactionParticipant = new eNettPaymentTransactionParticipant(Payment);
								factories.Add(transactionParticipant);
							}
						}
						base.Save(factories.ToArray());
					}

					if (Payment.TransactionHeader != null && !Payment.TransactionHeader.HasErrors)
					{
						IsPostWithoutMatching = true;

						if ((isNew || isPostingDraftPaymentWithoutMatching || isPostingDraftPaymentWithMatching) && !Payment.IsDeleted && Payment.AV_Status == PaymentApprovalStatus.Posted)
						{
							if (Payment.IsPosted)
							{
								if (Payment.NewPayment != null && ((IChequeNumberAutoAllocation)Payment.NewPayment).ChequeIsAutoPrinted)
								{
									PrintManager.SetChequeIsAutoPrinted();
								}
								PrintManager.Print(); // Print the Payment
								PaymentProcessingGUIHelper.PromptBankTransferForEPayment(Payment.NewPayment);
							}

							var newController = AccountingControllerCreator.GetNewController(Payment.TransactionHeader);
							if (newController == null)
							{
								var transactionHeader = Payment.TransactionHeader;
								Globals.Message.ShowError(Res.GetString("fe7aa455-7df9-4d5b-b919-626a28372a33", "Failed to open the edit form."));
								ErrorReporter.ReportOnce("PaymentApprovalForm.Save", string.Format("newController object creation should not be null in PaymentApprovalForm.Save(). TransactionHeader.Ah_Ledger = {0}, TransactionHeader.AH_TransactionType = {1}", // Error message, not key
									transactionHeader.AH_Ledger, transactionHeader.AH_TransactionType));
							}
							else
							{
								newController.ShowEditForm(Payment.TransactionHeader);
							}

							CloseFormWhenValidateAndSave();
						}
					}
				}
				catch (OnSavingCriticalCheckException ex)
				{
					Globals.Message.ShowError(ex.Message);
					CloseFormWhenValidateAndSave();
				}
				catch (NullReferenceException ex)
				{
					if (Payment == null)
					{
						throw new InvalidOperationException("Payment has unexpectedly become null while saving the Payment Approval Form.", ex);
					}

					throw;
				}
				catch (ReportException ex)
				{
					Globals.Message.Show(ex.Message);
					CloseFormWhenValidateAndSave();
				}
				finally
				{
					if (Payment != null && (isPostingDraftPaymentWithMatching || isPostingDraftPaymentWithoutMatching))
					{
						Payment.RemoveContext(BusinessContext.PostDraftPaymentApproval);
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("3aede98e-a1de-4cba-92cd-038e85f3a726", "Failed to save payment.  No payment to save."));
			}
		}

		protected void ShowMatchingForm()
		{
			fMatchingForm = new NewMatchGroupForm(Payment.PaymentMatchingBaseObject);
			Payment.PaymentMatchingBaseObject.CreateTemporaryTransactions();
			fMatchingForm.HideMatchAndContinueButtonForReceiptPayment();
			fMatchingForm.Closed += new EventHandler(fMatchingForm_Closed);
			ZFormModaliser.Show(fMatchingForm, this);

#if DEBUG
			if (Globals.IsTest)
			{
				OpenedMatchFormsForTest++;
				if (AutoCloseMatchingForms)
				{
					fMatchingForm.Close();
				}
			}
#endif
		}

		protected bool SessionBalancesToZero
		{
			get { return Payment.PaymentMatchingBaseObject.SessionBalancesToZero; }
		}

		void CloseFormWhenValidateAndSave()
		{
			ZFormModaliser.EnableForm(this, true); // In ZForm.ValidateAndSave, the form will be disabled via "ZFormModaliser.EnableForm(this, false)", which means the form cannot be actually closed. Hence enable it here before closing form.
			try
			{
				Close();
			}
			finally
			{
				ZFormModaliser.EnableForm(this, false);
			}
		}

		#endregion

		#region DisplayModeChanged

		protected virtual void PaymentApprovalForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
		}

		#endregion

		#region IDisposable Members

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

		#region Event Handlers
		PaymentProcessingGUIHelper PaymentProcessingGUIHelper => paymentProcessingGUIHelper ?? (paymentProcessingGUIHelper = new PaymentProcessingGUIHelper());
		PaymentProcessingGUIHelper paymentProcessingGUIHelper;

		ZBool IsPostWithoutMatching
		{
			get
			{
				return fIsPostWithoutMatching;
			}
			set
			{
				fIsPostWithoutMatching = value;
				if (Payment != null)
				{
					Payment.IsPostWithoutMatching = value;
				}
			}
		}
		ZBool fIsPostWithoutMatching;

		void PostWithoutMatchingButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			IsPostWithoutMatching = true;
			if (Payment.IsDraft)
			{
				Payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			}
		}

		void PaymentDetailButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			IsPostWithoutMatching = false;

			using (new DisposableAction(
					() =>
					{
						if (Payment != null)
						{
							Payment.Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft);
							Payment.IsProcessingPaymentDetail = true;
						}
					},
					() =>
					{
						if (Payment != null)
						{
							Payment.Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft);
							Payment.IsProcessingPaymentDetail = false;
						}
					}))
			{
				OnApplyButtonClick(sender, e);
			}
		}

		protected void HideSaveAsDraftButton()
		{
			SaveAsDraftButton.Click -= SaveAsDraftButton_Click;
			SaveAsDraftButton.Dispose();
		}

		void SaveAsDraftButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			IsPostWithoutMatching = false;
			SavePaymentAsDraft(true);
		}

		void CheckEPayRateButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			IsPostWithoutMatching = false;
			try
			{
				if (Payment.IsInDatabase || SavePaymentAsDraft(false))
				{
					if (!Payment.HasChanges)
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
							BankAndEPaymentTabControl.SelectTab("EPaymentTabPage");
							RefreshEPaymentButton_Click(this, null);
						}
					}
					else
					{
						PaymentApprovalEPaymentHelper.SavePaymentApprovalMessage();
					}
				}
			}
			catch (ZCannotSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void RefreshEPaymentButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			Payment.RefreshQuotes();
			Payment.RefreshCurrentDeal();
			Payment.RefreshBindingIncludingChildren();
		}

		bool SavePaymentAsDraft(bool showPopup)
		{
			var isSaveSuccessful = false;

			var errorMessage = Payment.CheckCanSaveAsDraft();
			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.ShowError(errorMessage);
			}
			else
			{
				using (new DisposableAction(() => Payment?.Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft), () => Payment?.Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft)))
				{
					isSaveSuccessful = FireSaveButton() == ContinueWithSave.Yes;
					if (isSaveSuccessful && showPopup)
					{
						Globals.Message.ShowInformation(Res.GetString("2df442f2-4c4e-4756-b169-9f5a8a8d2b96", @"Save as Draft successful.
If payment is closed without posting, it can be found in the Payment Processing module."));
					}
				}
			}

			UpdatePaymentProcessingButtonVisiblity();
			return isSaveSuccessful;
		}

		void AcceptQuoteButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			if (PaymentApprovalEPaymentHelper.TryToAcceptQuote(Payment, EPaymentGrid.SelectedElements.Cast<EPaymentQuoteForDisplay>().Select(x => x.RealQuote).ToArray()))
			{
				BankAndEPaymentTabControl.SelectTab("BankDetailsTabPage");
				SaveAsDraftButton.Enabled = false;
			}
		}

		void ProcessEPaymentButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			if (PaymentApprovalEPaymentHelper.TryToCreateDeal(Payment, () => CheckEPayRateButton_Click(this, EventArgs.Empty)))
			{
				BankAndEPaymentTabControl.SelectTab("EPaymentTabPage");
				SaveAsDraftButton.Enabled = false;
				RefreshEPaymentButton_Click(sender, e);
			}
		}

		void LearnMoreButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			EPaymentUrlLauncher.LaunchEPaymentProductMarketingURL();
		}

		void ProviderLogoPictureBox_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			var paymentProviderCode = Payment?.BankAccount?.AB_PaymentProvider ?? ZString.Empty;
			EPaymentUrlLauncher.LaunchEPaymentProviderURL(paymentProviderCode);
		}

		protected override void OnPostButtonClick(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			base.OnPostButtonClick(sender, e);
		}

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}

			base.OnApplyButtonClick(sender, e);
		}

		void OnSubmitForApprovalButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}
			PaymentProcessingGUIHelper.SubmitForApproval(new BusinessObject[] { Payment }, true);

			if (Payment.IsAwaitingApproval)
			{
				CloseFormWhenValidateAndSave();
			}
		}

		void OnApproveForPostingButton_Click(object sender, EventArgs e)
		{
			if (SuspendReiterantEventsDueToDangerousApplicationDoEvents.IsSuspended)
			{
				return;
			}
			PaymentProcessingGUIHelper.ApproveForPosting(new BusinessObject[] { Payment });
		}

		void UpdatePaymentProcessingButtonVisiblity()
		{
			if (Payment.IsDraft)
			{
				var paymentApprovalWithAuthorisation = Payment.Factory.LoadTop1<PaymentApprovalWithAuthorisation>(new ZQuery(AccPaymentApprovalSchema.PK, Payment.PK));

				if (paymentApprovalWithAuthorisation.NoAuthorisationRequired)
				{
					ApproveForPostingButton.Visible = true;
					SubmitForApprovalButton.Visible = false;
				}
				else
				{
					SubmitForApprovalButton.Visible = true;
					ApproveForPostingButton.Visible = false;
				}
			}
		}

		FunctionalitySuspender SuspendReiterantEventsDueToDangerousApplicationDoEvents => suspendReiterantEventsDueToDangerousApplicationDoEvents ?? (suspendReiterantEventsDueToDangerousApplicationDoEvents = new FunctionalitySuspender());
		FunctionalitySuspender suspendReiterantEventsDueToDangerousApplicationDoEvents;
		#endregion

		#region Test
#if DEBUG

		internal ZBool Test_DeactivateChequeBookOnAllocation = ZBool.False;
		internal PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator Test_Allocator;
		internal ZInt OpenedMatchFormsForTest;
		internal ZBool AutoCloseMatchingForms = ZBool.False;

#endif
		#endregion
	}
}

