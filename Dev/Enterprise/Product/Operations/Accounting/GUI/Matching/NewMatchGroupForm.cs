using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.Matching;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public enum SaveFactoryFlag
	{
		OK,
		Cancel
	}

	public partial class NewMatchGroupForm : ZForm, IDoDisplayModeNewOverride, IDoDisplayModeNewSavedOverride, IDoDisplayModeEditOverride, IDoDisplayModeBrowseOverride
	{
		public NewMatchGroupForm(MatchingBase matchingBizO)
			: base(matchingBizO)
		{
			fMatchingBase = matchingBizO;

			ControllerID = fMatchingBase.LedgerType == LedgerTypes.AccountsPayable ? ControllerIDs.ZAPMatching : ControllerIDs.ZARMatching;

			// hook to set displaymode to New when Matching Form is Edited
			DisplayModeChanged += NewMatchGroupForm_DisplayModeChanged;
			PrepareForSaveAsDraft();

			ZFormPostingButtonsStrategy.SetupPosting(this, MatchAndCloseButton, CloseButton, MatchAndContinueButton);

			HookEvents();

			// hook for reloading from DB
			MatchingTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
			GridsTabPage.RunWhenBindingOrFirstShown(delegate
			{
				ZCalcEditColumnStyleInfo matchGridExRateColumn = (ZCalcEditColumnStyleInfo)MatchTransactionsGrid.GetColumnStyle("ExchangeRateAmount");
				ZCalcEditColumnStyleInfo unmatchGridExRateColumn = (ZCalcEditColumnStyleInfo)UnmatchedTransactionsGrid.GetColumnStyle("ExchangeRateAmount");
				ZCalcEditColumnStyleInfo avgExRateColumn = (ZCalcEditColumnStyleInfo)CurrencySummaryGrid.GetColumnStyle("AverageExRate");
				ZInt decimalPlaces = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
				matchGridExRateColumn.Decimals = decimalPlaces;
				unmatchGridExRateColumn.Decimals = decimalPlaces;
				avgExRateColumn.Decimals = decimalPlaces;

				ChangePaymentAmountButton.ReadOnly = !fMatchingBase.AllowAlteringOfPayment;
				ChangePaymentAmountButton.Enabled = fMatchingBase.AllowAlteringOfPayment;
				// hooks for context menus
				MatchTransactionsGrid.ContextMenu.Popup += MatchTransactionsGridMenu_Popup;
				UnmatchedTransactionsGrid.ContextMenu.Popup += UnmatchTransactionsGridMenu_Popup;

				if (!fMatchingBase.ShouldShowRelatedDisbursementTransactions)
				{
					UnmatchedTransactionsGrid.RemoveFromAvailableColumns("RelatedDisbursementTransactions");
				}

				ARJournalsGrid.ContextMenu.Popup += ARJournalsGridMenu_Popup;
				APJournalsGrid.ContextMenu.Popup += APJournalsGridMenu_Popup;

				if (!AccountingMasterFilesUtils.HasGLAccountSelectionAndEntry)
				{
					ARJournalsGrid.RemoveFromAvailableColumns("AlternateGLAccountNumber");
					ARJournalsGrid.RemoveFromAvailableColumns("AlternateGLAccountDescription");
					APJournalsGrid.RemoveFromAvailableColumns("AlternateGLAccountNumber");
					APJournalsGrid.RemoveFromAvailableColumns("AlternateGLAccountDescription");
				}
			});

			SettlementOrgsTabPage.RunWhenBindingOrFirstShown(delegate
			{
				OrgInfoGrid.ReadOnly = IsOrgInfoGridReadOnlyWhenFirstShown;
			});

			ReceiptPaymentDetailGUISettings();

			// Payment Mediator should not be able to save newly created form
			SaveFactoryResult = SaveFactoryFlag.Cancel;

			PrimaryOrgGuidFindBox.Focus();
			DisableNewAction();

			ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileNewMenuItemName, Res.GetString("Accounting|NewMAtchGroupForm|FileNewMenuName", "&Match"));

			SetupMatchTransactionsGridContextMenu();
			SetCoveringLabelsText();
			SetUpClaimRelatedColumns();
			SetUpWTHColumns();
			SetUpCashAdvanceControls();

			ARJournalsGrid.AllowOverlap(ARCoveringLabel);
			APJournalsGrid.AllowOverlap(APCoveringLabel);

			matchingBizO.BalancingARJournals.ShowGLAccountsForImportAction = ShowGLAccountsForImportAction;
			matchingBizO.BalancingAPJournals.ShowGLAccountsForImportAction = ShowGLAccountsForImportAction;
		}

		void ShowGLAccountsForImportAction(AccGLHeaderCollection collection, List<AccGLHeader> glHeaderList)
		{
			ZFormModaliser.ShowDialogAndDispose(new GLAccountSelectionForm(collection, glHeaderList));
		}

		PaymentApprovalBase PaymentApprovalWithAuthorisation => (fMatchingBase as PaymentApprovalMatchingBase)?.PaymentApproval;

		void PrepareForSaveAsDraft()
		{
			if (PaymentApprovalWithAuthorisation != null)
			{
				SaveAsDraftButton.Enabled = true;
				SaveAsDraftButton.Visible = true;

				this.SaveAsDraftButton.Click += HandleSaveAsDraft;
			}
			else
			{
				SaveAsDraftButton.Enabled = false;
				SaveAsDraftButton.Visible = false;
			}
		}

		void HandleSaveAsDraft(object sender, EventArgs e)
		{
			if (PaymentApprovalWithAuthorisation != null)
			{
				var errorMessage = PaymentApprovalWithAuthorisation.CheckCanSaveAsDraft();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Globals.Message.ShowError(errorMessage);
				}
				else
				{
					using (new DisposableAction(() => PaymentApprovalWithAuthorisation.Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft),
						() => PaymentApprovalWithAuthorisation.Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft)))
					{
						MatchAndCloseButton.PerformClick();
					}
				}
			}
		}

		void SetCoveringLabelsText()
		{
			ARCoveringLabel.Text = Res.GetString("170467F0-3C01-4197-BB59-88ED933DC9BF", "You can only create AR Journals when you are matching AR Transactions. Move an AR Transaction to the 'Transactions Selected for Matching' grid to make this tab accessible.");
			APCoveringLabel.Text = Res.GetString("2663F302-DA77-484a-9BF3-07BB5011E8D5", "You can only create AP Journals when you are matching AP Transactions. Move an AP Transaction to the 'Transactions Selected for Matching' grid to make this tab accessible.");
		}

		ZDateEdit MatchDateEdit;
		ZGroupBox MatchDateGroupBox;
		ZCalcFindBox BankFeeCalcFindBox;
		ZButton BankFeeButton;
		ZPanel RightPanel;
		ZPanel SettlementOrgTabTopPanel;
		ZCheckBox IncludeAllARTransactions;
		ZCheckBox IncludeAllAPTransactionsCheckBox;
		ZTabPage APJournalsTabPage;
		ZTabPage ARJournalsTabPage;
		ZLabel APCoveringLabel;
		ZLabel ARCoveringLabel;
		ZGrid APJournalsGrid;
		ZGrid ARJournalsGrid;
		ZGroupBox TransactionSearchGroupBox;
		MatchingFormFilterControl TransactionFilterControl;
		AccountingOnFormFilterControl CashAdvanceFilterControl;
		KSplitContainer MainSplitContainer;
		ZTabPage CashAdvanceTabPage;
		KSplitContainer CashAdvanceSplitContainer;
		ZGroupBox CashAdvanceSearchGroupBox;
		ZGroupBox RequestedCashAdvanceGroupBox;
		ZDisplayGrid UnmatchedCashAdvanceRequestsGrid;
		ZPanel OutstandingTransactionsPanel;
		ZPanel OutstandingTransactionsGridPanel;
		ZButton ExpandViewButton;
		ZBool IsOrgInfoGridReadOnlyWhenFirstShown = false;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			TransactionFilterControl = new MatchingFormFilterControl(fMatchingBase.MatchingFilterBizO);
			TransactionFilterControl.Name = nameof(TransactionFilterControl);
			TransactionFilterControl.SetMaxFilterStripPanelHeight(128);
			TransactionFilterControl.Dock = DockStyle.Fill;
			TransactionFilterControl.PerformSearch += HandleFind;
			TransactionFilterControl.FilteredGrid.SizeChanged += TransactionFilteredGrid_BoundsChanged;
			TransactionFilterControl.FilteredGrid.LocationChanged += TransactionFilteredGrid_BoundsChanged;
			TransactionFilteredGrid_BoundsChanged(TransactionFilterControl.FilteredGrid, EventArgs.Empty);

			OutstandingTransactionsPanel.Controls.Add(TransactionFilterControl);
			OutstandingTransactionsGridPanel.AllowOverlap(TransactionFilterControl);
		}

		void TransactionFilteredGrid_BoundsChanged(object sender, EventArgs e)
		{
			OutstandingTransactionsGridPanel.Bounds = TransactionFilterControl.FilteredGrid.Bounds;
		}

		void SetUpClaimRelatedColumns()
		{
			ZTextBoxColumnStyleInfo relatedClaimStatusInfo = (ZTextBoxColumnStyleInfo)UnmatchedTransactionsGrid.GetColumnStyle("RelatedClaimStatus");
			ZTextBoxColumnStyleInfo queryNumberInfo = (ZTextBoxColumnStyleInfo)UnmatchedTransactionsGrid.GetColumnStyle("QueryNumber");
			if (Ledger != LedgerTypes.AccountsPayable)
			{
				if (relatedClaimStatusInfo != null)
				{
					UnmatchedTransactionsGrid.ColumnStyles.Remove(relatedClaimStatusInfo);
				}
				if (queryNumberInfo != null)
				{
					UnmatchedTransactionsGrid.ColumnStyles.Remove(queryNumberInfo);
				}
			}
		}

		void SetUpCashAdvanceControls()
		{
			if (fMatchingBase.CanCashAdvanceRequestBeMatched)
			{
				CashAdvanceFilterControl = new AccountingOnFormFilterControl(fMatchingBase.UnmatchedCashAdvanceRequests, fMatchingBase.CashAdvanceFilter);
				CashAdvanceFilterControl.Name = nameof(CashAdvanceFilterControl);
				CashAdvanceFilterControl.SetMaxFilterStripPanelHeight(220);
				CashAdvanceFilterControl.Dock = DockStyle.Fill;
				CashAdvanceSearchGroupBox.Controls.Add(CashAdvanceFilterControl);
				CashAdvanceFilterControl.PerformSearch += CashAdvanceFilterControl_PerformSearch;
				CashAdvanceFilterControl.FiltersCleared += CashAdvanceFilterControl_ClearButtonClicked;
				CashAdvanceTabPage.TabVisible = true;
			}
			else
			{
				CashAdvanceTabPage.TabVisible = false;
			}
		}

		void SetUpWTHColumns()
		{
			if (!GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled())
			{
				RemoveColumnFromGrid(UnmatchedTransactionsGrid, UnmatchedTransactionsGrid.GetColumnStyle(nameof(IMatching.NotionalWHTTax)));
				RemoveColumnFromGrid(UnmatchedTransactionsGrid, UnmatchedTransactionsGrid.GetColumnStyle(nameof(IMatching.RealizedWHTTax)));

				RemoveColumnFromGrid(MatchTransactionsGrid, MatchTransactionsGrid.GetColumnStyle(nameof(IMatching.NotionalWHTTax)));
				RemoveColumnFromGrid(MatchTransactionsGrid, MatchTransactionsGrid.GetColumnStyle(nameof(IMatching.RealizedWHTTax)));
			}
		}

		void RemoveColumnFromGrid(ZGrid grid, ZGridColumnInfo columnInfo)
		{
			if (columnInfo != null)
			{
				grid.ColumnStyles.Remove(columnInfo);
			}
		}

		void ReceiptPaymentDetailGUISettings()
		{
			// since PrimaryOrg is prepopulated in the case of Payment, OrgInfos should not be readonly
			if (!fMatchingBase.IsMatchingPaymentOrReceipt)
			{
				IsOrgInfoGridReadOnlyWhenFirstShown = true;
			}
			else
			{
				if (fMatchingBase.IsNotMatchingPayment)
				{
					ChangePaymentAmountButton.Text = Res.GetString("06DAAEB8-64F3-433f-A0BD-C3BB08263288", "Change Receipt Amount");
				}
			}
			ChangePaymentAmountButton.ReadOnly = !fMatchingBase.AllowAlteringOfPayment;
			ChangePaymentAmountButton.Enabled = fMatchingBase.AllowAlteringOfPayment;
		}

		void HookEvents()
		{
			fMatchingBase.PrimaryOrganizationInfo.ValueChanged += PrimaryOrganizationInfo_ValueChanged;
			fMatchingBase.MatchingSuccessful += fMatchingBase_MatchingSuccessful;
			fMatchingBase.TransactionsSelectedChanged += fMatchingBase_TransactionsSelectedChanged;
			fMatchingBase.AskUserForConfirmation += fAskUser;

			// hooks for changing button text
			fMatchingBase.DiscountBizObjChanged += fMatchingBase_DiscountBizObjChanged;
			fMatchingBase.OverpaymentBizObjChanged += fMatchingBase_OverpaymentBizObjChanged;
			fMatchingBase.ExchangeDiffBizObjChanged += fMatchingBase_ExchangeDiffBizObjChanged;
			fMatchingBase.BankFeeBizObjChanged += fMatchingBase_BankFeeBizObjChanged;

			// GUI notification for changing PrimaryOrg
			fMatchingBase.PrimaryOrganisationForGUINotificationInfo.ValueChanged += PrimaryOrganisationForGUINotificationInfo_ValueChanged;

			fMatchingBase.OnLoadFilterWithTooManyParameters += new EventHandler<UserMessageEventArgs>(LoadFilterWithTooManyParameters);
		}

		void UnhookEvents()
		{
			if (fMatchingBase != null)
			{
				fMatchingBase.PrimaryOrganizationInfo.ValueChanged -= PrimaryOrganizationInfo_ValueChanged;
				fMatchingBase.MatchingSuccessful -= fMatchingBase_MatchingSuccessful;
				fMatchingBase.TransactionsSelectedChanged -= fMatchingBase_TransactionsSelectedChanged;
				fMatchingBase.AskUserForConfirmation -= fAskUser;

				fMatchingBase.DiscountBizObjChanged -= fMatchingBase_DiscountBizObjChanged;
				fMatchingBase.OverpaymentBizObjChanged -= fMatchingBase_OverpaymentBizObjChanged;
				fMatchingBase.ExchangeDiffBizObjChanged -= fMatchingBase_ExchangeDiffBizObjChanged;
				fMatchingBase.BankFeeBizObjChanged -= fMatchingBase_BankFeeBizObjChanged;

				fMatchingBase.PrimaryOrganisationForGUINotificationInfo.ValueChanged -= PrimaryOrganisationForGUINotificationInfo_ValueChanged;

				fMatchingBase.OnLoadFilterWithTooManyParameters -= new EventHandler<UserMessageEventArgs>(LoadFilterWithTooManyParameters);
			}
		}

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			UnhookEvents();
			base.Dispose(isNotFinalizing);
		}

		#endregion

		public override string FormCaption
		{
			get { return Res.GetString("NewMatchGroupForm|D4D6F9DA-633D-4a03-9617-2A387F5A0874", "{0} Match Group", fMatchingBase is ARMatchingBase ? LedgerTypesList.Descriptions.AccountsReceivable : LedgerTypesList.Descriptions.AccountsPayable); }
		}

		protected string Ledger
		{
			get { return fMatchingBase != null && fMatchingBase is ARMatchingBase ? ZArchitecture.Core.LedgerTypes.AccountsReceivable : ZArchitecture.Core.LedgerTypes.AccountsPayable; }
		}

		#region Display Mode Overrides

		void IDoDisplayModeNewOverride.DoDisplayModeNew()
		{
			MatchAndCloseButton.Text = Res.GetString("NewMatchGroupForm|6adfdab7-b8f9-48f0-a1b9-d52054d15835", "Match and Close");
			MatchAndCloseButton.Enabled = true;

			SetPostingButtonsBasedOnContext();
		}

		void IDoDisplayModeNewSavedOverride.DoDisplayModeNewSaved()
		{
			MatchAndCloseButton.Text = Res.GetString("NewMatchGroupForm|6adfdab7-b8f9-48f0-a1b9-d52054d15835", "Match and Close");
			MatchAndCloseButton.Enabled = true;

			SetPostingButtonsBasedOnContext();
		}

		void IDoDisplayModeEditOverride.DoDisplayModeEdit()
		{
			// whenever DisplayMode is set to 'Edit', 'Match and Continue' button should be enabled 
			// depending on whether form is accessed from Receipt or Payment

			MatchAndCloseButton.Text = Res.GetString("Posting.Buttons.MatchClose", "Match and Close");
			MatchAndCloseButton.Enabled = true;

			SetPostingButtonsBasedOnContext();
		}

		void SetPostingButtonsBasedOnContext()
		{
			MatchAndContinueButton.Text = MatchButtonCaption;
			CloseButton.Text = Res.GetString("2e1c0378-2a09-4bd0-bda1-d33964620527", "&Close");
			MatchAndContinueButton.Enabled = !fMatchingBase.IsMatchingPaymentOrReceipt;
			MatchAndContinueButton.Visible = !fMatchingBase.IsMatchingPaymentOrReceipt;
		}

		string MatchButtonCaption
		{
			get { return Res.GetString("Posting.Buttons.Match", "Match"); }
		}

		void IDoDisplayModeBrowseOverride.DoDisplayModeBrowse()
		{
			ZFormStrategy.DoDisplayModeBrowse(this);
			MatchAndContinueButton.Text = MatchButtonCaption;
		}

		void NewMatchGroupForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			// The DisplayMode of MatchingForm should never be set 
			// to Edit because MatchingForm is non-persistent

			if (e.ToMode == ODisplayMode.Edit || e.ToMode == ODisplayMode.New)
			{
				DisplayMode = ODisplayMode.NewSaved;
			}
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			DisplayMode = ODisplayMode.ReadOnly;
			base.SetReadOnlyIncludingChildren();
			EXXMenuItem.Enabled = false;
			DSCMenuItem.Enabled = false;
			OVPMenuItem.Enabled = false;
			PayLinesMenuItem.Enabled = false;
		}

		#endregion

		void fAskUser(object sender, UserQueryEventArgs e)
		{
			e.Response = Globals.Message.Show(e.QueryMessage, MessageCaption, MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes;
		}

		string MessageCaption => Res.GetString("5B725D91-3310-4b81-8882-6503CAEB99B4", "Matching");

		protected override void HandleSaveException(Exception e)
		{
			if (e is AllocationSaveException)
			{
				Globals.Message.ShowError(((AllocationSaveException)e).UserFriendlyMessage, Res.GetString("3116d070-8de3-4c43-af75-9c3e659faa36", "Check Book Busy"));
			}
			else if (e is AllocationChequeBookException)
			{
				Globals.Message.ShowError(((AllocationChequeBookException)e).UserFriendlyMessage, Res.GetString("e8418bdb-4677-45e5-b715-bd93e2b12d74", "Check Book Full"));
			}
			else if (e is OnSavingCriticalCheckException)
			{
				OnSavingCriticalCheckException ex = (OnSavingCriticalCheckException)e;
				BusinessObject businessObject = ex.BusinessEntity as BusinessObject;
				IBusinessObjectCollection collection = ex.BusinessEntity as IBusinessObjectCollection;
				if (businessObject != null)
				{
					businessObject.AddRowError(ex.Message);
				}
				if (collection != null)
				{
					foreach (BusinessObject element in collection)
					{
						element.AddRowError(ex.Message);
					}
				}
				Globals.Message.ShowError(Res.GetString("0b3f899a-5215-4101-9384-75b88acbbbb4", "{0}\r\nPlease cancel and re-enter the transaction you are trying to post.", ex.Message));
			}
			else
			{
				base.HandleSaveException(e);
			}
		}

		#region ValidateAndSave

		protected override ContinueWithSave ValidateAndSave()
		{
			this.SaveFactoryResult = SaveFactoryFlag.Cancel;
			ContinueWithSave result = ContinueWithSave.No;

			#region Additionally validate Balancing Journals in their AR and AP collections and Matched Transactions

			foreach (Journal journal in fMatchingBase.BalancingAPJournals)
			{
				ValidateBalancingJournal(journal, false);
			}

			foreach (Journal journal in fMatchingBase.BalancingARJournals)
			{
				ValidateBalancingJournal(journal, false);
			}

			foreach (IMatching header in fMatchingBase.MatchedTransactions)
			{
				Journal journal = header as Journal;
				if (journal != null && !journal.IsInDatabase)
				{
					ValidateBalancingJournal(journal, true);
				}
				else
				{
					if (header.HasErrors()) //clear previous line errors
					{
						header.RunPreSaveValidation();
					}
					fMatchingBase.RefreshExistingPaymentApprovalItems(header);
				}
			}

			if (fMatchingBase.BalancingAPJournals.HasErrors() ||
				fMatchingBase.BalancingARJournals.HasErrors() ||
				fMatchingBase.MatchedTransactions.HasErrors())
			{
				ShowErrorsDialog();
				return ContinueWithSave.No;
			}

			#endregion

			fMatchingBase.RunPreSaveValidation();
			OrgHeader primaryOrg = fMatchingBase.Factory.Load<OrgHeader>(fMatchingBase.PrimaryOrganization);
			if (fMatchingBase.HasErrors)
			{
				if (fMatchingBase.IsReceiptPaymentDetailChequeNumberInvalid && !fMatchingBase.IsNotMatchingPayment)
				{
					fMatchingBase.MakeChequeNumEditableOnReceiptPaymentDetail();
				}
				ShowErrorsDialog();
			}
			else if (fMatchingBase.MatchedTransactions.Count == 0)
			{
				string caption = Res.GetString("d2b1359b-97de-4564-a10e-7fb1acd974ea", "Match Transactions");
				string message = Res.GetString("c2c8ee27-4fd3-420e-b44e-fe025143d37e", "Please select a transaction or transactions to be matched");
				Globals.Message.ShowInformation(message, caption);
			}
			else if (!fMatchingBase.MatchedTransactions.ContainsTransactionFromSpecifiedOrg(primaryOrg))
			{
				string caption = Res.GetString("d2b1359b-97de-4564-a10e-7fb1acd974ea", "Match Transactions");
				string message = Res.GetString("1ad8f153-74ab-4af3-84be-b368f7beb2d5", "At least one of the chosen transactions must relate to the primary organization");
				Globals.Message.ShowInformation(message, caption);
			}
			else if ((fMatchingBase as PaymentApprovalMatchingBase)?.IsMatchTransactionsInDbChangedForDraft ?? false)
			{
				string caption = Res.GetString("1B881744-C01B-419D-8288-4A5E5A3C822D", "Match Transactions");
				string message = Res.GetString("6CC3ED98-827E-4692-96BE-83D0695CDAFE", "The matched transactions were already changed by another user. Please reopen this form.");
				Globals.Message.ShowInformation(message, caption);
			}
			else
			{
				bool matchingSucceeded = false;
				try
				{
					Cursor.Current = Cursors.WaitCursor;
					matchingSucceeded = fMatchingBase.MatchAndClearTransactions();
				}
				finally
				{
					Cursor.Current = Cursors.Arrow;

					if (fMatchingBase.Corrupted)
					{
						SetReadOnlyIncludingChildren();
					}
				}

				if (matchingSucceeded && !fMatchingBase.Corrupted)
				{
					PrimaryOrgGuidFindBox.Focus();
					SaveFactoryResult = SaveFactoryFlag.OK;
					result = ContinueWithSave.Yes;
				}

				if (!matchingSucceeded)
				{
					ShowErrorsDialog();
				}
			}
			return result;
		}

		void ValidateBalancingJournal(Journal journal, bool switchToOpposite)
		{
			if (!journal.HasErrors)
			{
				bool original = journal.UseJournalValidation;
				try
				{
					if (!original || switchToOpposite)
					{
						journal.UseJournalValidation = !original;
					}

					journal.RunPreSaveValidation();
				}
				finally
				{
					journal.UseJournalValidation = original;
				}
			}
		}

		#endregion

		protected override DialogResult ShowErrorsDialogCore(bool includeIgnoreOption)
		{
			if (!Globals.IsTest)
			{
				using (var msgBox = new ZErrorMessageBox(BusinessEntity, Res.GetString("64a24157-28e8-4490-ab53-06ef20c11945", "selection"), Res.GetString("2ac760c4-6b56-43ff-938b-b6e7aae86b54", "match"), Res.GetString("cd215586-effa-4100-9c1d-377eabaac009", "matched"), includeIgnoreOption))
				{
					return ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("cb26a373-0947-4a6d-9ca0-d13a609d3656", "There are errors - can't save."), Res.GetString("456b015d-7a69-4cb7-92d4-a780bed8813d", "Errors!"));
				return DialogResult.Abort;
			}
		}

		// for integration with O_Receipt and Payment
		public SaveFactoryFlag SaveFactoryResult
		{
			get { return fSaveFactoryResult; }
			set { fSaveFactoryResult = value; }
		}

		SaveFactoryFlag fSaveFactoryResult;

		#region Action Menu

		// TODO check if this realy needed
		//protected override void InitialiseMainMenu()
		//{
		//    base.InitialiseMainMenu();
		//    ActionsMenuItem.Enabled = true;
		//}

		#endregion

		#region LineMatching

		Dictionary<TransactionHeader, InvoicingBasePayLineMediator> fPayLineMediators;
		Dictionary<TransactionHeader, InvoicingBasePayLineMediator> PayLineMediators
		{
			get { return fPayLineMediators ?? (fPayLineMediators = new Dictionary<TransactionHeader, InvoicingBasePayLineMediator>()); }
		}

		void PayLines(object sender, EventArgs e)
		{
			if (MatchTransactionsGrid.CurrentRowIndex != -1)
			{
				InvoicingBase invoicingBase = ((MatchingBase)MatchTransactionsGrid.DataSource).MatchedTransactions[MatchTransactionsGrid.CurrentRowIndex] as InvoicingBase;

				if (invoicingBase == null)
				{
					Globals.Message.Show(Res.GetString("1ae04e75-8f65-483c-8eea-2ca9d55720f9", "Only Invoices, Credit Notes, and Adjustment Notes can use this function."));
				}
				else
				{
					if (invoicingBase.AH_PostedToEFT && invoicingBase.AH_Ledger == LedgerTypes.AccountsPayable)
					{
						Globals.Message.ShowError(Res.GetString("02cf53eb-f6e2-4ede-88e5-ea6ef5c4db84", "You cannot match transaction lines for AP invoices and credit notes where you have used the 'Use Job Exchange Rate' tick box."));
					}
					else if (((ISupportMatchingOfMyLines)invoicingBase).LineTotalPaidAmount.IsEmpty && ((ISupportMatchingOfMyLines)invoicingBase).LineTotalPaidAmountPosted.IsEmpty && ((IMatching)invoicingBase).OSOutstandingAmount != invoicingBase.AH_OSTotal)
					{
						Globals.Message.ShowError(Res.GetString("b2eadef8-ac7b-483a-b836-3944821abf7d", "You cannot match transaction lines on this transaction because the transaction has already been part paid without that previous part payment being matched at a line level."));
					}
					else
					{
						if (!PayLineMediators.ContainsKey(invoicingBase))
						{
							PayLineMediators[invoicingBase] = new InvoicingBasePayLineMediator(fMatchingBase, invoicingBase);
						}

						ZFormModaliser.Show(new PayLinesForm(PayLineMediators[invoicingBase]), this);
					}
				}
			}
		}

		MenuItem payLinesMenuItem;
		MenuItem PayLinesMenuItem
		{
			get { return payLinesMenuItem ?? (payLinesMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting|NewMAtchGroupForm|PayLines", "Pay Lines"), PayLines, Shortcut.CtrlL)); }
		}

		MenuItem fPayLinesMenuSeparator;
		MenuItem PayLinesMenuSeparator
		{
			get { return fPayLinesMenuSeparator ?? (fPayLinesMenuSeparator = new ZMenuItem("-")); }
		}

		#endregion

		#region Miscellaneous Transaction Related

		#region HandleMiscellaneousTransaction

		// Validates and decides whether to pop-up the New form or View form
		void HandleMiscellaneousTransaction(ZString transactionType)
		{
			if (transactionType == TransactionTypes.Overpayment && !fMatchingBase.IsOverPaymentAllowedInThisMatchingSession)
			{
				Globals.Message.Show(Res.GetString("599c2c82-0311-42a5-b471-3440f795e2fe", "You cannot create an Over Payment in this matching session"));
				return;
			}

			var additionalCheckpoint = fMatchingBase.CheckpointForNewMiscTransaction(transactionType);
			if (additionalCheckpoint != null && !additionalCheckpoint.IsAllowed)
			{
				additionalCheckpoint.ShowError();
				return;
			}

			TransactionHeader miscTransaction = null;
			ZString readableTransactionType = ZString.Empty;
			switch (transactionType)
			{
				case TransactionTypes.Overpayment:
					miscTransaction = fMatchingBase.OverpaymentCurrent;
					readableTransactionType = Res.GetString("5087df6f-2c8a-4e89-96de-d9c086c5f3f7", "Overpayment");
					break;
				case TransactionTypes.ExchangeDifference:
					miscTransaction = fMatchingBase.ExchangeDiffCurrent;
					readableTransactionType = Res.GetString("94af7116-4d4a-4423-8c6e-d3f486c497e4", "Exchange Difference");
					break;
				case TransactionTypes.Discount:
					miscTransaction = fMatchingBase.DiscountCurrent;
					readableTransactionType = Res.GetString("e0ace3c8-b19a-49d3-9b51-1cc5d64e04c9", "Discount");
					break;
				case TransactionTypes.Journal:
					miscTransaction = fMatchingBase.BankFeeCurrent;
					readableTransactionType = Res.GetString("c8ec6494-50e0-4941-b493-719f2493fedc", "Bank Fee");
					break;
			}
			if (miscTransaction == null && fMatchingBase.Balance != 0M)
			{
				ShowNewMiscTransactionForm(transactionType);
			}
			else if (miscTransaction != null)
			{
				ShowEditMiscTransactionForm(transactionType);
			}
			else
			{
				string caption = Res.GetString("d2b1359b-97de-4564-a10e-7fb1acd974ea", "Match Transactions");
				string message = Res.GetString("b723ca43-8f52-42b8-ab63-6fd3de4d15b0", "{0} cannot be created here because the balance is zero.", readableTransactionType);
				Globals.Message.ShowInformation(message, caption);
			}
		}

		#endregion

		#region ShowNewMiscTransactionForm

		IZForm ShowNewMiscTransactionForm(ZString transactionType)
		{
			IZForm formToReturn = null;
			TransactionHeader miscTrans = fMatchingBase.GetMiscellaneousTransaction(transactionType);
			if (miscTrans != null)
			{
				ZController controller = AccountingControllerCreator.GetNewController(miscTrans, ModuleID);
				if (controller != null)
				{
					TransactionHeaderCollection defaultMiscTrans = new TransactionHeaderCollection(fMatchingBase.Factory) { miscTrans };
					controller.SetCollectionForDefaultsAndValidation(defaultMiscTrans);
					controller.SetFormsModalTo(this);
					formToReturn = controller.ShowNewForm();
				}
			}

			return formToReturn;
		}

		#endregion

		#region ShowEditMiscTransactionForm

		void ShowEditMiscTransactionForm(ZString transactionType)
		{
			TransactionHeader header = null;
			switch (transactionType)
			{
				case ZArchitecture.Core.TransactionTypes.Overpayment:
					header = fMatchingBase.OverpaymentCurrent;
					break;
				case ZArchitecture.Core.TransactionTypes.Discount:
					header = fMatchingBase.DiscountCurrent;
					break;
				case ZArchitecture.Core.TransactionTypes.ExchangeDifference:
					header = fMatchingBase.ExchangeDiffCurrent;
					break;
				case ZArchitecture.Core.TransactionTypes.Journal:
					header = fMatchingBase.BankFeeCurrent;
					break;
			}
			if (header != null)
			{
				ZController controller = AccountingControllerCreator.GetNewController(header, ModuleID);
				if (controller != null)
				{
					header.IsMiscellaneousTransaction = true;
					controller.SetFormsModalTo(this);
					controller.ShowEditForm(header);
				}
			}
		}

		#endregion

		ModuleIdentifier fModuleID;
		ModuleIdentifier ModuleID
		{
			get { return fModuleID ?? (fModuleID = ZControllerFactory.Create(ControllerID).ModuleID); }
		}

		#region Miscellaneous Transactions Buttons

		#region Click Handlers

		void NewOverpaymentButton_Click(object sender, EventArgs e)
		{
			HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
		}

		void NewDiscountButton_Click(object sender, EventArgs e)
		{
			HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
		}

		void NewExchangeDiffButton_Click(object sender, EventArgs e)
		{
			HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
		}

		void NewBankFeeButton_Click(object sender, EventArgs e)
		{
			if ((Guid)AccountingConfigurationRegistry.Instance.FinanceChargesAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) == Guid.Empty)
			{
				Globals.Message.ShowError(Res.GetString("bbdf03dd-e200-49da-903d-ab8e79614a2d", @"Cannot create Bank Fee Journal because Finance Charges Account is not set.
	Please set up a correct value of the Registry Item: Accounting > General Ledger Defaults > Link Account > Finance Charges Account."));
			}
			else
			{
				HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Journal);
			}
		}

		#endregion

		#region Changing Button Text

		void fMatchingBase_DiscountBizObjChanged(object sender, EventArgs e)
		{
			SetButtonText(fMatchingBase.DiscountCurrent, DiscountButton);
		}

		void fMatchingBase_OverpaymentBizObjChanged(object sender, EventArgs e)
		{
			SetButtonText(fMatchingBase.OverpaymentCurrent, OverpaymentButton);
		}

		void fMatchingBase_ExchangeDiffBizObjChanged(object sender, EventArgs e)
		{
			SetButtonText(fMatchingBase.ExchangeDiffCurrent, ExchangeDiffButton);
		}

		void fMatchingBase_BankFeeBizObjChanged(object sender, EventArgs e)
		{
			SetButtonText(fMatchingBase.BankFeeCurrent, BankFeeButton);
		}

		void SetButtonText(TransactionHeader header, ZButton button)
		{
			if (header == null)
			{
				button.Text = Res.GetString("Accounting|NewMatchGroupForm|ButtonNew", "New");
			}
			else
			{
				button.Text = Res.GetString("Accounting|NewMatchGroupForm|ButtonEdit", "Edit");
			}
		}

		#endregion

		#region Menu Item Handlers

		void AddOverpayment(object sender, EventArgs e)
		{
			HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
		}

		void AddExchangeDifference(object sender, EventArgs e)
		{
			HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
		}

		void AddDiscount(object sender, EventArgs e)
		{
			HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
		}

		void DeleteMiscTransaction(object sender, EventArgs e)
		{
			if (MatchTransactionsGrid.SelectedElements.Length == 1 &&
				MatchTransactionsGrid.SelectedElements[0] is TransactionHeader)
			{
				fMatchingBase.DeleteMiscTransaction((TransactionHeader)MatchTransactionsGrid.SelectedElements[0]);
			}
		}

		#endregion

		#region Menu Item Properties

		#region OVPMenuItem

		MenuItem OVPMenuItem
		{
			get
			{
				if (fOVPMenuItem == null)
				{
					fOVPMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.MatchGroup.AddOverpayment", "Add Overpayment"), new EventHandler(AddOverpayment));
				}

				return fOVPMenuItem;
			}
		}

		MenuItem fOVPMenuItem;

		#endregion

		#region EXXMenuItem

		MenuItem EXXMenuItem
		{
			get
			{
				if (fEXXMenuItem == null)
				{
					fEXXMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.MatchGroup.AddExchangeDifference", "Add Exchange Difference"), new EventHandler(AddExchangeDifference));
				}

				return fEXXMenuItem;
			}
		}

		MenuItem fEXXMenuItem;

		#endregion

		#region DSCMenuItem

		MenuItem DSCMenuItem
		{
			get
			{
				if (fDSCMenuItem == null)
				{
					fDSCMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.MatchGroup.AddDiscount", "Add Discount"), new EventHandler(AddDiscount));
				}

				return fDSCMenuItem;
			}
		}

		MenuItem fDSCMenuItem;

		#endregion

		#region DeleteMenuItem

		MenuItem DeleteMenuItem
		{
			get
			{
				if (fDeleteMenuItem == null)
				{
					fDeleteMenuItem = new ZMenuItem(ResString.GetMultilingualString("MenuItem.Remove", "Remove"), new EventHandler(DeleteMiscTransaction));
				}

				return fDeleteMenuItem;
			}
		}

		MenuItem fDeleteMenuItem;

		#endregion

		#endregion

		#endregion

		#endregion

		#region Viewing Transactions menuItems

		#region ViewMatchTransactionMenuItem

		MenuItem ViewMatchTransactionMenuItem
		{
			get
			{
				if (fViewMatchTransactionMenuItem == null)
				{
					fViewMatchTransactionMenuItem = new ZMenuItem(ResString.GetMultilingualString("6FB3AC26-3AEF-4272-BF1C-F46FC64194BF", "View"), new EventHandler(ViewMatchGridTransaction));
				}

				return fViewMatchTransactionMenuItem;
			}
		}

		MenuItem fViewMatchTransactionMenuItem;

		#endregion

		#region ViewUnMatchTransactionMenuItem

		MenuItem ViewUnMatchTransactionMenuItem
		{
			get
			{
				if (fViewUnMatchTransactionMenuItem == null)
				{
					fViewUnMatchTransactionMenuItem = new ZMenuItem(ResString.GetMultilingualString("6FB3AC26-3AEF-4272-BF1C-F46FC64194BF", "View"), new EventHandler(ViewUnMatchGridTransaction));
				}

				return fViewUnMatchTransactionMenuItem;
			}
		}

		MenuItem fViewUnMatchTransactionMenuItem;

		#endregion

		#region Menu Item Handlers

		void ViewMatchGridTransaction(object sender, EventArgs e)
		{
			if (MatchTransactionsGrid.SelectedElements.Length == 1)
			{
				var selectedElement = MatchTransactionsGrid.SelectedElements[0];
				var transactionHeader = selectedElement as AccTransactionHeader;
				var paymentApproval = selectedElement as PaymentApprovalBase;

				if (transactionHeader != null)
				{
					if (transactionHeader.IsInDatabase)
					{
						ZController newController = AccountingControllerCreator.GetNewController(transactionHeader);
						newController.ShowViewForm(transactionHeader);
					}
					else if (transactionHeader.AH_TransactionType == TransactionTypes.Journal)
					{
						Globals.Message.ShowInformation(Res.GetString("e67e1343-bd25-4bfd-9317-3c4162d475b8", "Please use the 'AR Journals' and 'AP Journals' tab to view the details for this journal."));
					}
				}
				else if (paymentApproval != null)
				{
					Globals.Message.ShowInformation(Res.GetString("7bb6f7cc-a10d-433a-a4ff-03c6b0778596", "Please use the 'Change Payment Amount' button to view the details for this payment."));
				}
			}
		}

		void ViewUnMatchGridTransaction(object sender, EventArgs e)
		{
			TransactionHeader header = UnmatchedTransactionsGrid.SelectedElements.FirstOrDefault() as TransactionHeader;
			if (header != null)
			{
				ZController newController = AccountingControllerCreator.GetNewController(header);
				newController.ShowViewForm(header);
			}
		}

		#endregion

		#endregion

		#region AR/AP Journal Menu Item

		#region Menu Item

		protected MenuItem EditARJournalMenuItem
		{
			get
			{
				if (fEditARJournalMenuItem == null)
				{
					fEditARJournalMenuItem = new ZMenuItem(ResString.GetMultilingualString("49266ebf-0703-42e9-8ee4-ee9f7c9185ca", "Edit"), new EventHandler(EditARJournal));
				}

				return fEditARJournalMenuItem;
			}
		}

		MenuItem fEditARJournalMenuItem;

		protected MenuItem EditAPJournalMenuItem
		{
			get
			{
				if (fEditAPJournalMenuItem == null)
				{
					fEditAPJournalMenuItem = new ZMenuItem(ResString.GetMultilingualString("49266ebf-0703-42e9-8ee4-ee9f7c9185ca", "Edit"), new EventHandler(EditAPJournal));
				}

				return fEditAPJournalMenuItem;
			}
		}

		MenuItem fEditAPJournalMenuItem;

		#endregion

		#region Menu Item Handlers

		void EditARJournal(object sender, EventArgs e)
		{
			HandleEditARAPJournal(ARJournalsGrid);
		}

		void EditAPJournal(object sender, EventArgs e)
		{
			HandleEditARAPJournal(APJournalsGrid);
		}

		protected virtual IZForm HandleEditARAPJournal(ZGrid grid)
		{
			IZForm form = null;
			if (grid.SelectedElements.Length == 1)
			{
				var controllerID = grid.SelectedElements[0] is ARJournal ? ControllerIDs.ARBalancingJournal : ControllerIDs.APBalancingJournal;
				var journal = grid.SelectedElements[0] as Journal;
				journal.EnableCheckSubAccountsForGLHeader = false;

				var controller = ZControllerFactory.Create(controllerID);
				controller.SetFormsModalTo(this);
				form = controller.ShowEditForm(journal);
				form.Closed += EditARAPJournalForm_Closed;
			}
			return form;
		}

		void EditARAPJournalForm_Closed(object sender, EventArgs e)
		{
			var businessEntity = ((ZForm)sender).BusinessEntity as Journal;
			businessEntity.EnableCheckSubAccountsForGLHeader = true;
			businessEntity.RunPreSaveValidation();
		}

		#endregion

		#endregion

		#region Searching And Selecting Buttons - clickHandlers

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			HandleSelectAll();
		}

		void UnselectAllButton_Click(object sender, EventArgs e)
		{
			HandleUnselectAll();
		}

		#endregion

		#region Handle Methods

		protected void CashAdvanceFilterControl_PerformSearch(object sender, EventArgs e)
		{
			fMatchingBase.ValidatePrimaryOrganization();

			if (fMatchingBase.PrimaryOrganizationInfo.HasErrors())
			{
				ShowErrorMessageToUser();
			}
			else
			{
				fMatchingBase.LoadCashAdvanceRequests();
			}
		}

		protected void CashAdvanceFilterControl_ClearButtonClicked(object sender, EventArgs e)
		{
			fMatchingBase.ClearCashAdvanceRequests();
		}

		void ShowErrorMessageToUser()
		{
			if (!Globals.IsTest)
			{
				using (var form = new ZErrorMessageBox(fMatchingBase, Res.GetString("a25831cc-81d5-4582-a082-b0624f7ac1d0", "search"), Res.GetString("b48329d8-e65c-4bb7-90b2-5c0710f75719", "perform"), Res.GetString("a28d06f7-7f7c-42c3-a250-0901158cdf28", "performed")))
				{
					ZFormModaliser.ShowMessageBoxWithoutDispose(form);
				}
			}
			else
			{
				Globals.Message.ShowError(String.Join(System.Environment.NewLine, fMatchingBase.Notifications.GetFatalNotifications().GetUniqueMessageList()));
			}
		}

		void HandleFind(object obj = null, EventArgs e = null)
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				return;
			}

			if (fMatchingBase.MatchingFilterBizO.HasErrors)
			{
				FilterResultsLabel.Text = Res.GetString("Accounting|NewMatchGroupForm|Error", "Fix all errors before running this operation");
				return;
			}

			Cursor.Current = Cursors.WaitCursor;

			FilterResultsLabel.Text = ZString.Empty;

			fMatchingBase.ReloadSettlementOrgTransactions();

			if (fMatchingBase.UnmatchedTransactions.Count > 0 &&
				!fMatchingBase.MatchingFilterBizO.HasWarnings &&
				!fMatchingBase.MatchingFilterBizO.Filter.IsEmpty)
			{
				UnmatchedTransactionsGrid.SelectAllElements();
				FilterResultsLabel.Text = fMatchingBase.FoundMoreThanMaxResultRows ?
					Res.GetString("Accounting|NewMatchGroupForm|MoreTransactionsFound", "More than {0} transactions were found. {0} are listed per 'Max Results in Matching Search' registry", fMatchingBase.UnmatchedTransactions.Count) :
					Res.GetString("Accounting|NewMatchGroupForm|TransactionsFound", "{0} Transactions were found", fMatchingBase.UnmatchedTransactions.Count);
			}
			else if (fMatchingBase.UnmatchedTransactions.Count == 0)
			{
				FilterResultsLabel.Text = Res.GetString("Accounting|NewMatchGroupForm|NoTransactionsWereFound", "No Transactions were found");
			}

			Cursor.Current = Cursors.Arrow;
		}

		void LoadFilterWithTooManyParameters(object sender, UserMessageEventArgs e)
		{
			Globals.Message.ShowError(e.Message);
		}

		void HandleClear()
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				return;
			}
			TransactionFilterControl.ResetFiltersLayout();
		}

		void HandleSelectAll()
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				return;
			}
			Cursor.Current = Cursors.WaitCursor;
			if (IsTransactionsTabSelected)
			{
				fMatchingBase.MoveAllFromUnmatchToMatch();
			}
			else if (IsCashAdvanceTabSelected)
			{
				fMatchingBase.MoveAllCashAdvanceFromUnmatchToMatch();
			}
			Cursor.Current = Cursors.Arrow;
		}

		void HandleUnselectAll()
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				return;
			}
			Cursor.Current = Cursors.WaitCursor;
			MatchTransactionsGrid.SelectAllElements();
			HandleMoveUp();
			Cursor.Current = Cursors.Arrow;
		}

		void HandleAutoSelect()
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				return;
			}

			if (fMatchingBase.MatchingFilterBizO.HasErrors)
			{
				FilterResultsLabel.Text = Res.GetString("Accounting|NewMatchGroupForm|Error", "Fix all errors before running this operation");
				return;
			}

			Cursor.Current = Cursors.WaitCursor;
			fMatchingBase.LoadTransactionsMatchingTheFilter();
			fMatchingBase.MoveFilterMatchingTransactionsToSelected();
			Cursor.Current = Cursors.Arrow;
		}

		#endregion

		#region Grid Clicks And Key Press

		void UnmatchedTransactionsGrid_DoubleClick(object sender, EventArgs e)
		{
			HandleMoveDown();
		}

		void MatchTransactionsGrid_DoubleClick(object sender, EventArgs e)
		{
			HandleMoveUp();
		}

		protected void UnmatchedCashAdvanceRequestsGrid_DoubleClick(object sender, EventArgs e)
		{
			HandleMoveDown();
		}

		#endregion

		#region Move Up and Down

		void HandleMoveDown()
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				return;
			}

			if (IsTransactionsTabSelected)
			{
				if (UnmatchedTransactionsGrid.ListManager.Position >= 0)
				{
					Cursor.Current = Cursors.WaitCursor;
					fMatchingBase.MoveFromUnmatchToMatch(UnmatchedTransactionsGrid.SelectedElements);
					if (UnmatchedTransactionsGrid.ListManager.Position != -1 &&
						UnmatchedTransactionsGrid.ListManager.Position < UnmatchedTransactionsGrid.List.Count)
					{
						UnmatchedTransactionsGrid.Select(UnmatchedTransactionsGrid.ListManager.Position);
					}
					Cursor.Current = Cursors.Arrow;
				}
			}
			else if (IsCashAdvanceTabSelected)
			{
				if (UnmatchedCashAdvanceRequestsGrid.ListManager.Position >= 0)
				{
					Cursor.Current = Cursors.WaitCursor;
					try
					{
						fMatchingBase.MoveCashAdvanceFromUnmatchToMatch(UnmatchedCashAdvanceRequestsGrid.SelectedElements.Cast<CashAdvanceRequestHeader>().ToArray());
					}
					catch (CannotGenerateCashAdvanceJournalException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
					if (UnmatchedCashAdvanceRequestsGrid.ListManager.Position != -1 &&
						UnmatchedCashAdvanceRequestsGrid.ListManager.Position < UnmatchedCashAdvanceRequestsGrid.List.Count)
					{
						UnmatchedCashAdvanceRequestsGrid.Select(UnmatchedCashAdvanceRequestsGrid.ListManager.Position);
					}
					Cursor.Current = Cursors.Arrow;
				}
			}
		}

		void HandleMoveUp()
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				return;
			}
			if (MatchTransactionsGrid.ListManager.Position >= 0)
			{
				Cursor.Current = Cursors.WaitCursor;

				List<Journal> journals = new List<Journal>();
				List<BusinessObject> transactions = new List<BusinessObject>();
				foreach (BusinessObject element in MatchTransactionsGrid.SelectedElements)
				{
					Journal journal = element as Journal;
					if (fMatchingBase.IsNewBalancingJournal(journal))
					{
						if (journal == fMatchingBase.BankFeeCurrent)
						{
							fMatchingBase.DeleteMiscTransaction(journal);
						}
						else
						{
							journals.Add(journal);
						}
					}
					else
					{
						transactions.Add(element);
						InvoicingBase invoice = element as InvoicingBase;
						if (invoice != null)
						{
							journals.AddRange(fMatchingBase.GetLinkedToInvoiceBalancingJournals(invoice));
						}
					}
				}

				journals = journals.Distinct().ToList();
				DialogResult dialogResult = DialogResult.Yes;
				if (journals.Count > 0)
				{
					dialogResult = Globals.Message.Show(Res.GetString("5e917240-7a37-480e-8966-90c9b6655040", @"This action will delete balancing journals.
Do you want to continue?"), Res.GetString("996C3FC5-EA7B-4db8-941D-71EB358F41A9", "Matching"), MessageBoxButtons.YesNo, DialogResult.Yes);
					if (dialogResult == DialogResult.Yes)
					{
						fMatchingBase.RemoveAndDeleteBalancingJournalsFromMatchingTransactions(journals);
					}
				}

				if (dialogResult != DialogResult.No)
				{
					foreach (IMatching element in transactions)
					{
						TransactionHeader header = element as TransactionHeader;
						if (header != null)
						{
							PayLineMediators.Remove(header);
						}
					}

					fMatchingBase.MoveFromMatchToUnmatch(transactions.ToArray());

					if (MatchTransactionsGrid.ListManager.Position != -1 &&
						MatchTransactionsGrid.ListManager.Position < MatchTransactionsGrid.List.Count)
					{
						MatchTransactionsGrid.Select(MatchTransactionsGrid.ListManager.Position);
					}
				}
				Cursor.Current = Cursors.Arrow;
			}
		}

		void MoveDownButton_Click(object sender, EventArgs e)
		{
			HandleMoveDown();
		}

		void MoveUpButton_Click(object sender, EventArgs e)
		{
			HandleMoveUp();
		}

		#endregion

		#region Primary Org Changed

		void PrimaryOrganizationInfo_ValueChanged(object sender, EventArgs e)
		{
			IsOrgInfoGridReadOnlyWhenFirstShown = !fMatchingBase.IsPrimaryOrgValidAndNonEmpty;
			OrgInfoGrid.ReadOnly = IsOrgInfoGridReadOnlyWhenFirstShown;
		}

		void PrimaryOrganisationForGUINotificationInfo_ValueChanged(object sender, EventArgs e)
		{
			if (PrimaryOrgInitialised)
			{
				if (!SuspendPrimaryOrgNotification)
				{
					string question = Res.GetString("68963ce2-7e55-429a-80b0-741c367453f0", "Changing the Primary Organization will reset the current matching session.\r\nDo you want to continue?");
					DialogResult cancelOptions = Globals.Message.Show(question, Res.GetString("f31955ed-b398-44d1-83c8-55b02aba83a0", "Warning"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
					switch (cancelOptions)
					{
						case DialogResult.Yes:
							fMatchingBase.PrimaryOrganization = fMatchingBase.PrimaryOrganisationForGUINotification;
							FilterResultsLabel.Text = ZString.Empty;
							break;
						case DialogResult.Cancel:
							SuspendPrimaryOrgNotification = true;
							fMatchingBase.PrimaryOrganisationForGUINotification = fMatchingBase.PrimaryOrganization;
							SuspendPrimaryOrgNotification = false;
							break;
						case DialogResult.No:
							SuspendPrimaryOrgNotification = true;
							fMatchingBase.PrimaryOrganisationForGUINotification = fMatchingBase.PrimaryOrganization;
							SuspendPrimaryOrgNotification = false;
							break;
					}
				}
			}
			else
			{
				PrimaryOrgInitialised = true;
				fMatchingBase.PrimaryOrganization = fMatchingBase.PrimaryOrganisationForGUINotification;
				FilterResultsLabel.Text = ZString.Empty;
			}
		}

		ZBool SuspendPrimaryOrgNotification = false;
		ZBool PrimaryOrgInitialised = false;

		#endregion

		#region Match / Match & Continue Button EventHandlers

		#region On Successful Matching

		void fMatchingBase_MatchingSuccessful(object sender, EventArgs e)
		{
			MatchAndCloseButton.Enabled = false;
			MatchAndContinueButton.Enabled = false;
		}

		#endregion

		#region Transactions Selected Changed

		void fMatchingBase_TransactionsSelectedChanged(object sender, EventArgs e)
		{
			if (MatchTransactionsGrid.List.Count != 0)
			{
				MatchAndCloseButton.Enabled = true;
				if (MatchAndContinueButton.Visible && !MatchAndContinueButton.Enabled)
				{
					MatchAndContinueButton.Enabled = true;
				}
			}
		}

		#endregion

		public void HideMatchAndContinueButtonForReceiptPayment()
		{
			MatchAndContinueButton.Enabled = false;
			MatchAndContinueButton.Visible = false;
		}

		#endregion

		#region Top Level Key EventHandlers

		#region TestMatchingForm_KeyDown

		void TestMatchingForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == (Keys.Control | Keys.F))
			{
				HandleFind();
				e.Handled = true;
			}
			else if (e.KeyData == (Keys.Control | Keys.U))
			{
				HandleUnselectAll();
				e.Handled = true;
			}
			else if (e.KeyData == (Keys.Control | Keys.A))
			{
				HandleAutoSelect();
				e.Handled = true;
			}
			else if (e.KeyData == (Keys.Control | Keys.C))
			{
				HandleClear();
				e.Handled = true;
			}
			else if (e.KeyData == (Keys.Control | Keys.S))
			{
				HandleAutoSelect();
				e.Handled = true;
			}
		}

		#endregion

		#region ProcessCmdKey

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Control | Keys.Add:
					HandleMoveDown();
					return true;
				case Keys.Control | Keys.Subtract:
					HandleMoveUp();
					return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		#endregion

		#endregion

		#region Match Transactions Grid Context Menu

		void SetupMatchTransactionsGridContextMenu()
		{
			MatchTransactionsGrid.ContextMenu.MenuItems.Add(0, PayLinesMenuItem);
			MatchTransactionsGrid.ContextMenu.MenuItems.Add(1, PayLinesMenuSeparator);
		}

		#region AR/AP JournalsGridMenu_Popup

		public void ARJournalsGridMenu_Popup(object sender, EventArgs e)
		{
			ARAPJournalsGridMenu_Popup(ARJournalsGrid, EditARJournalMenuItem);
		}

		public void APJournalsGridMenu_Popup(object sender, EventArgs e)
		{
			ARAPJournalsGridMenu_Popup(APJournalsGrid, EditAPJournalMenuItem);
		}

		void ARAPJournalsGridMenu_Popup(ZGrid grid, MenuItem menuItem)
		{
			if (grid.SelectedElements.Length == 1)
			{
				if (!grid.ContextMenu.MenuItems.Contains(menuItem))
				{
					grid.ContextMenu.MenuItems.Add(0, menuItem);
				}
			}
			else
			{
				grid.ContextMenu.MenuItems.Remove(menuItem);
			}
		}

		#endregion

		#region MatchTransactionsGridMenu_Popup

		void MatchTransactionsGridMenu_Popup(object sender, EventArgs e)
		{
			// Remove the OVP, DSC and EXX Menu Items
			RemoveMatchTransactionGridMenuItems();

			PayLinesMenuItem.Visible = MatchTransactionsGrid.CurrentRowIndex != -1;
			PayLinesMenuSeparator.Visible = MatchTransactionsGrid.CurrentRowIndex != -1;

			// Add the items if Balance != 0, etc.
			if (fMatchingBase.Balance != 0)
			{
				if (fMatchingBase.OverpaymentCurrent == null)
				{
					MatchTransactionsGrid.ContextMenu.MenuItems.Add(OVPMenuItem);
				}
				if (fMatchingBase.DiscountCurrent == null)
				{
					MatchTransactionsGrid.ContextMenu.MenuItems.Add(DSCMenuItem);
				}
				if (fMatchingBase.ExchangeDiffCurrent == null)
				{
					MatchTransactionsGrid.ContextMenu.MenuItems.Add(EXXMenuItem);
				}
			}
			// Delete Menu Item
			if (MatchTransactionsGrid.SelectedElements.Length == 1 &&
				(MatchTransactionsGrid.SelectedElements[0] == fMatchingBase.DiscountCurrent ||
				MatchTransactionsGrid.SelectedElements[0] == fMatchingBase.OverpaymentCurrent ||
				MatchTransactionsGrid.SelectedElements[0] == fMatchingBase.ExchangeDiffCurrent ||
				MatchTransactionsGrid.SelectedElements[0] == fMatchingBase.BankFeeCurrent))
			{
				if (!MatchTransactionsGrid.ContextMenu.MenuItems.Contains(DeleteMenuItem))
				{
					MatchTransactionsGrid.ContextMenu.MenuItems.Add(DeleteMenuItem);
				}
			}
			else
			{
				if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(DeleteMenuItem))
				{
					MatchTransactionsGrid.ContextMenu.MenuItems.Remove(DeleteMenuItem);
				}
			}
			// Viewing Transactions
			if (MatchTransactionsGrid.SelectedElements.Length == 1)
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Add(ViewMatchTransactionMenuItem);
			}
		}

		#endregion

		#region RemoveMatchTransactionGridMenuItems

		void RemoveMatchTransactionGridMenuItems()
		{
			if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(OVPMenuItem))
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Remove(OVPMenuItem);
			}
			if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(DSCMenuItem))
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Remove(DSCMenuItem);
			}
			if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(EXXMenuItem))
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Remove(EXXMenuItem);
			}
			if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(ViewMatchTransactionMenuItem))
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Remove(ViewMatchTransactionMenuItem);
			}
		}

		#endregion

		#endregion

		#region Unmatch Transactions Grid Context Menu

		void UnmatchTransactionsGridMenu_Popup(object sender, EventArgs e)
		{
			// remove the view menu item first (code readbility)
			if (UnmatchedTransactionsGrid.ContextMenu.MenuItems.Contains(ViewUnMatchTransactionMenuItem))
			{
				UnmatchedTransactionsGrid.ContextMenu.MenuItems.Remove(ViewUnMatchTransactionMenuItem);
			}
			if (UnmatchedTransactionsGrid.SelectedElements.Length == 1 &&
				UnmatchedTransactionsGrid.SelectedElements[0] is TransactionHeader)
			{
				UnmatchedTransactionsGrid.ContextMenu.MenuItems.Add(ViewUnMatchTransactionMenuItem);
			}
		}

		#endregion

		#region Reload From DB

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (IsTransactionsTabSelected)
			{
				Cursor.Current = Cursors.WaitCursor;
				FilterResultsLabel.Text = ZString.Empty;
				fMatchingBase.ReloadSettlementOrgTransactions();
				Cursor.Current = Cursors.Arrow;

				ProcessJournalsWhileMovingToMatchingTab();

				fMatchingBase.BalancingAPJournals.SetUseJournalValidation(false);
				fMatchingBase.BalancingARJournals.SetUseJournalValidation(false);
			}
			if (IsCashAdvanceTabSelected)
			{
				Cursor.Current = Cursors.WaitCursor;
				fMatchingBase.ReloadSettlementOrgTransactions();
				Cursor.Current = Cursors.Arrow;
			}
			else if (IsAPJournalsTabPageSelected)
			{
				APJournalsGrid.Visible = fMatchingBase.LedgerType == LedgerTypes.AccountsPayable
					|| IfExistAtLeastOneTransactionInMatchedGridWithSettedLedger(LedgerTypes.AccountsPayable);

				fMatchingBase.BalancingAPJournals.SetUseJournalValidation(true);
			}
			else if (IsARJournalsTabPageSelected)
			{
				ARJournalsGrid.Visible = fMatchingBase.LedgerType == LedgerTypes.AccountsReceivable
					|| IfExistAtLeastOneTransactionInMatchedGridWithSettedLedger(LedgerTypes.AccountsReceivable);

				fMatchingBase.BalancingARJournals.SetUseJournalValidation(true);
			}
		}

		void ProcessJournalsWhileMovingToMatchingTab()
		{
			foreach (Journal journal in fMatchingBase.BalancingARJournals)
			{
				ProcessJournal(journal);
			}

			foreach (Journal journal in fMatchingBase.BalancingAPJournals)
			{
				ProcessJournal(journal);
			}

			List<Journal> journalsForDelete = new List<Journal>();
			foreach (IMatching transaction in fMatchingBase.MatchedTransactions)
			{
				Journal journal = transaction as Journal;
				if (journal != null && journal.RelatedJournal != null && journal.RelatedJournal.IsDeleted)
				{
					journalsForDelete.Add(journal);
				}
			}

			fMatchingBase.RemoveAndDeleteBalancingJournalsFromMatchingTransactions(journalsForDelete);
		}

		void ProcessJournal(Journal journal)
		{
			if (journal.AH_TransactionCategory == Constants.TransactionCategory.Codes.TransactionAlreadyPaid ||
						journal.AH_TransactionCategory == Constants.TransactionCategory.Codes.TransactionNotFound)
			{
				if (journal.RelatedJournal == null)
				{
					if (fMatchingBase.MatchedTransactions.Contains(journal))
					{
						fMatchingBase.MoveFromMatchToUnmatch(new BusinessObject[] { journal });
					}
					fMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { fMatchingBase.CopyJournalWithOppositeAmount(journal) });
				}
			}
			else
			{
				if (!fMatchingBase.MatchedTransactions.Contains(journal))
				{
					if (journal.RelatedJournal != null)
					{
						fMatchingBase.MoveFromMatchToUnmatch(new BusinessObject[] { journal.RelatedJournal });
						journal.RelatedJournal.Delete();
						journal.RelatedJournal = null;
					}
					fMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { journal });
				}
			}
		}

		bool IfExistAtLeastOneTransactionInMatchedGridWithSettedLedger(string ledger)
		{
			foreach (IMatching matchingBizO in fMatchingBase.MatchedTransactions)
			{
				if (matchingBizO.Ledger == ledger)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		MatchingBase fMatchingBase;

		protected bool IsTransactionsTabSelected => MatchingTabControl.SelectedTab == GridsTabPage;

		protected bool IsCashAdvanceTabSelected => MatchingTabControl.SelectedTab == CashAdvanceTabPage;

		protected bool IsAPJournalsTabPageSelected => MatchingTabControl.SelectedTab == APJournalsTabPage;

		protected bool IsARJournalsTabPageSelected => MatchingTabControl.SelectedTab == ARJournalsTabPage;

		#region Currency Summary

		readonly string CurrencySummaryFormKey = "CurrencySummary";

		void ChangePaymentAmountButton_Click(object sender, EventArgs e)
		{
			if (fMatchingBase is PaymentApprovalMatchingBase)
			{
				new AlterPaymentManager((PaymentApprovalMatchingBase)fMatchingBase).ShowForm(this);
			}
			else
			{
				new AlterReceiptManager(fMatchingBase).ShowForm(this);
			}
		}

		void CurrencySummaryButton_Click(object sender, EventArgs e)
		{
			if (fMatchingBase.CurrencySummary != null && OpenedFormCache.GetInstance().Contains(fMatchingBase.CurrencySummary.PK.ToGuid(), CurrencySummaryFormKey))
			{
				OpenedFormCache.GetInstance().SwitchToCachedForm(fMatchingBase.CurrencySummary.PK.ToGuid(), CurrencySummaryFormKey);
			}
			else
			{
				CurrencySummaryForm form = new CurrencySummaryForm(fMatchingBase.CurrencySummary);
				OpenedFormCache.GetInstance().Add(fMatchingBase.CurrencySummary.PK.ToGuid(), form, CurrencySummaryFormKey);
				form.Show();
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			CurrencySummaryForm form = OpenedFormCache.GetInstance().GetForm(fMatchingBase.CurrencySummary.PK.ToGuid(), CurrencySummaryFormKey) as CurrencySummaryForm;
			if (form != null)
			{
				form.Close();
			}
			var settlementOrganisationsForm = OpenedFormCache.GetInstance().GetForm(fMatchingBase.SettlementOrganisaion.PK.ToGuid(), SettlementOrganisationsFormKey) as SettlementOrganisationsForm;
			if (settlementOrganisationsForm != null)
			{
				settlementOrganisationsForm.Close();
			}
		}

		#endregion

		readonly string SettlementOrganisationsFormKey = "SettlementOrganisation";

		void ExpandViewButton_Click(object sender, EventArgs e)
		{
			if (fMatchingBase != null && OpenedFormCache.GetInstance().Contains(fMatchingBase.SettlementOrganisaion.PK.ToGuid(), SettlementOrganisationsFormKey))
			{
				OpenedFormCache.GetInstance().SwitchToCachedForm(fMatchingBase.SettlementOrganisaion.PK.ToGuid(), SettlementOrganisationsFormKey);
			}
			else
			{
				var form = new SettlementOrganisationsForm(fMatchingBase.SettlementOrganisaion);
				OpenedFormCache.GetInstance().Add(fMatchingBase.SettlementOrganisaion.PK.ToGuid(), form, SettlementOrganisationsFormKey);
				form.Show();
			}
		}
	}
}

