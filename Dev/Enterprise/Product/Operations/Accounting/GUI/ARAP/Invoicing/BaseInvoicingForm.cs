#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.Business.Presentation.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.InvoicingApproval;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI
{
	/// <summary>
	/// Summary description for BaseInvoicingForm.
	/// 
	/// Receipt/Payment details are on ReceiptPaymentPanel
	/// which is only visible when entering a new AR/AP Invoice. ie. not visible in View mode
	/// 
	/// Header and Line Details are on MainPanel which stretches to fit available space when
	/// Receipt/Payment details are not visible.
	/// </summary>
#if DEBUG
	// This ZForm serves as base class only. Its parent partial class is concreate. Hence it has TestExcludeZWinFormsAllHaveFormBashers attribute applied.
	[TestExcludeZWinFormHasTypedConstructor()]
	[TestExcludeZWinFormsAllHaveFormBashers()]
#endif
	public partial class BaseInvoicingForm : AccountingZForm, IDataGridLayoutIdentifierRoot
	{
		#region Controls

		public ZTabControl ChargesAndApportionmentsTabControl;
		public ZTabPage LineChargesTabPage;
		public ZTabPage PeriodApportionmentTabPage;
		public ZTemplateTabControl MainTabControl;
		public ZTabPage InvoiceDetailsTabPage;
		public ZPanel ReceiptPaymentOuterPanel;
		public ZPanel MainPanel;
		public ZPanel ButtonsPanel;
		public ZPostingButtonsUserControl PostingButtonsUserControl;
		public InvoiceUserControl InvoiceDetails;
		public ZGrid LineChargesGrid;
		ZCalcFindBox LocalSubTotalExTaxAmountCalcEdit;
		ZCalcFindBox OSSubTotalExTaxAmountCalcEdit;
		public ZCalcFindBox AH_LocalTotalAmountCalcEdit;
		public ZCalcFindBox AH_LocalTaxAmountCalcEdit;
		public ZCalcFindBox AH_OSTotalAmountCalcEdit;
		public ZCalcFindBox AH_OSTaxAmountCalcEdit;
		public ZCalcFindBox AH_LocalWHTAmountCalcEdit;
		public ZCalcFindBox AH_OSWHTAmountCalcEdit;
		public ZPanel ChargeDetailsPanel;
		public ZCalcFindBox AH_LocalExtraTaxAmountCalcEdit;
		ZLogsTabPage zEventTabPage1;
		ZCalcFindBox AH_OSExtraTaxAmountCalcEdit;
		ZTabPage RelatedInvoicesTabPage;
		ZGrid RelatedInvoicesGrid;
		internal MenuItem SaveAsIncompleteMenuItem;
		internal MenuItem OverrideBranchAndDepartmentMenuItem;
		readonly MenuItem PreviewInvoiceMenuItem;
		internal MenuItem PreviewCostMenuItem;
		internal MenuItem ResetStatusToQueuedMenuItem;
		internal MenuItem SetStatusToAwaitMenuItem;
		internal MenuItem AuthorizeAndSendMenuItem;
		internal MenuItem AutoAllocateDiscrepancyMenuItem;
		internal MenuItem RequestSalesTaxCalculationMenuItem;
		internal MenuItem RequestSalesTaxSubmissionMenuItem;
		ZWorkflowTabPage zWorkflowTabPage1;
		ZLabel RestrictedLineChargesLabel;
		ZTabPage sourceXmlTabPage;
		ImportedInvoiceXMLControl importedInvoiceXMLControl;
		ZButton CalculateTaxTransactionsButton;
		ZGrid PeriodApportionmentGrid;
		ZTabPage SubAccountsTabPage;
		SubAccountsControl SubAccountsControl;
		KTableLayoutPanel ButtonsLayoutPanel;
		ZCalcFindBox AH_OSSubTotalAmountCalcEdit;
		ZCalcFindBox AH_LocalSubTotalAmountCalcEdit;
		ZCalcFindBox AH_LocalOtherTaxesAmountCalcEdit;
		ZCalcFindBox AH_OSOtherTaxesAmountCalcEdit;
		CollapsibleTableLayoutPanel InvoiceTotalsCollapsibleLayoutPanel;
		ZPanel TaxAmountsPanel;
		ZPanel OtherTaxesAmountsPanel;
		ZPanel TotalInvoiceAmountsPanel;
		ZPanel SubTotalAmountsPanel;
		ZPanel SubTotalExTaxPanel;
		ZTabControl TotalAndUnallocatedTabControl;
		ZTabPage TotalTabPage;
		ZPanel ExtraTaxPanel;
		ZPanel WHTPanel;
		ZTabPage UnallocatedTabPage;
		ZCalcFindBox OSExpectedTotalExTaxCalcEdit;
		ZPanel IncludingTaxTotalPanel;
		ZCalcFindBox OSInvoiceTotalIncTaxCalcEdit;
		ZCalcFindBox OSExpectedTotalIncTaxCalcEdit;
		ZCalcFindBox OSUnallocatedTotalIncTaxCalcEdit;
		ZPanel TaxTotalPanel;
		ZCalcFindBox OSInvoiceTotalTaxCalcEdit;
		ZCalcFindBox OSExpectedTotalTaxCalcEdit;
		ZCalcFindBox OSUnallocatedTotalTaxCalcEdit;
		ZPanel ExcludingTaxPanel;
		ZCalcFindBox OSInvoiceTotalExTaxCalcEdit;
		ZCalcFindBox OSUnallocatedTotalExTaxCalcEdit;
		ZPanel UnallocatedCaptionPanel;
		ZLabel ExpectedTotalCaption;
		ZLabel InvoiceTotalCaption;
		ZLabel UnallocatedCaption;
		KFlowLayoutPanel UnallocatedFlowLayoutPanel;
		IContainer components;

		#endregion

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public BaseInvoicingForm()
		{
		}

		public BaseInvoicingForm(InvoicingBase businessEntity)
			: base(businessEntity)
		{
			if (businessEntity == null)
			{
				throw new ArgumentNullException(nameof(businessEntity));
			}

			InvoicingBase invoice = Invoice ?? throw new InvalidOperationException("The property \"Invoice\" happens to be null.");

			PlugIns plugIns = PlugIns ?? throw new InvalidOperationException("The property \"PlugIns\" happens to be null.");

			ZWorkflowTabPage workflowTabPage = zWorkflowTabPage1 ?? throw new InvalidOperationException("The control \"zWorkflowTabPage1\" happens to be null.");

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			SetDataBinding(businessEntity, "");

			invoice.AH_RX_NKTransactionCurrencyInfo.ValueChanged += AH_RX_NKTransactionCurrencyInfo_ValueChanged;
			invoice.MutexError += Invoice_MutexError;
			plugIns.Add(ControllerIDs.DocumentUDFPlugIn);
			plugIns.Add(ControllerIDs.eDocsPlugIn);

			IWorkflowProvider provider = businessEntity;
			IsPostOnly = invoice.IsReversing;
			DisplayModeChanged += BaseInvoicingForm_DisplayModeChanged;
			InvoiceDetailsTabPage.RunWhenTabInitialized((sender, args) =>
			{
				InvoiceDetails.ApportionChargesButton.Click += ApportionChargesButton_Click;
				InvoiceDetails.BulkChargeImportButton.Click += BulkChargeImportButton_Click;
				InvoiceDetails.LineSummaryTabPage.RunWhenTabInitialized((sender2, e) =>
				{
					InvoiceDetails.TransactionLinesGrid.CurrentCellChanged += TransactionLinesGrid_CurrentCellChanged;

					if (!businessEntity.SupportMultiPeriodApportionment)
					{
						PeriodApportionmentTabPage.TabVisible = false;
					}
				});

				SubTotalAmountsPanel.AllowOverlap(OtherTaxesAmountsPanel);
				SubTotalAmountsPanel.AllowOverlap(TotalInvoiceAmountsPanel);
				OtherTaxesAmountsPanel.AllowOverlap(TotalInvoiceAmountsPanel);
			});

			if (!businessEntity.IsReverseTransaction)
			{
				SecurityOverrideProviderSource.Get(businessEntity).Provider = ApprovalGUIProvider.GetNewSecurityOverrideProviderForPosting(businessEntity);
			}

			var isBizEntInDB = businessEntity.IsInDatabase;
			var bizEntLedger = businessEntity.AH_Ledger;

			IsApprovingSelfBillingInvoice = isBizEntInDB &&
				 bizEntLedger == LedgerTypes.AccountsPayable &&
				(ZString)businessEntity.AH_LedgerInfo.OriginalValue == LedgerTypes.UnapprovedPayableTransactions &&
				businessEntity.IsSelfBillingInvoice;

			if (!IsUATransaction)
			{
				IsPostOnly = businessEntity.IsReverseTransaction;
			}

			if (businessEntity is APInvoice || businessEntity is APCreditNote)
			{
				businessEntity.Lines.ShowJobChargesForImportEvent += InvoiceForm_ShowJobChargesForImportEvent;
				businessEntity.Lines.ApportionedInvoiceLineModified += InvoiceForm_ApportionedInvoiceLineModified;
				InvoiceDetailsTabPage.RunWhenTabInitialized((sender, args) =>
				{
					InvoiceDetails.LineSummaryTabPage.RunWhenTabInitialized((sender2, e) =>
					{
						InvoiceDetails.TransactionLinesGrid.RowsDeleting += TransactionLinesGrid_RowDeleting;
						InvoiceDetails.TransactionLinesGrid.RowsDeleted += TransactionLinesGrid_RowDeleted;
						InvoiceDetails.TransactionLinesGrid.ContextMenu.Popup += TransactionLinesGridContextMenu_Popup;
					});
				});
				businessEntity.Factory.Saved += Factory_Saved;
				invoice.OnJobChanged += Invoice_OnJobChanged;
			}

			if (businessEntity.IsAPTransaction && !invoice.IsInDatabase)
			{
				invoice.EnableValidationOfValidateExpectedInvoiceTotal();
			}

			ActionsMenuItem.Popup += ActionsMenuItem_Popup;

			if (!businessEntity.Factory.HasContext(BusinessContext.PayableOrder) && (bizEntLedger == LedgerTypes.AccountsPayable || bizEntLedger == LedgerTypes.IncompleteTransactions))
			{
				SaveAsIncompleteMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("Accounting|BaseInvoicingForm|SaveAsIncompleteMenuName", "Save As 'Incomplete'"), SaveAsIncomplete_Click);
			}

			if (!isBizEntInDB)
			{
				if (bizEntLedger == LedgerTypes.AccountsPayable || bizEntLedger == LedgerTypes.UnapprovedPayableTransactions)
				{
					PreviewCostMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, MenuNameConstants.PreviewCosts, PreviewInvoice_Click);
				}
				else if (bizEntLedger == LedgerTypes.AccountsReceivable)
				{
					PreviewInvoiceMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("Accounting|BaseInvoicingForm|PreviewInvoiceMenuName", "Preview Invoice"), PreviewInvoice_Click);
				}
			}

			if (isBizEntInDB && businessEntity.IsEligibleToCreateEInvoicingTransactionPivot)
			{
				var registry = AccountingMasterFilesRegistry.Instance;
				var currCompany = GlbCompany.CurrentCompany;

				if (InvoiceFormPresentationProvider.IsEInvoicingColumnsAvailable(bizEntLedger))
				{
					ResetStatusToQueuedMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("f30e178c-f1dc-4dea-96f6-84422e6d35f0", "Reset Status to Queued"), ResetStatusToQueued_Click);
				}
				if (currCompany.GC_RN_NKCountryCode == CountryCodes.Italy &&
					(bizEntLedger == LedgerTypes.AccountsReceivable &&
						registry.EnableEInvoicingFunctionality.Value && registry.EReportingSubmitPivotDefaultStatus.Value == EInvoicingPivotState.Pending ||
					bizEntLedger == LedgerTypes.AccountsPayable &&
						registry.EnableEInvoicingFunctionalityForPayables.Value && registry.EReportingSubmitPivotDefaultStatusForPayables.Value == EInvoicingPivotState.Pending))
				{
					var subMenu = new ZMenuItem(Res.GetString("2C19F75E-632C-4304-8D72-F40E0A1A399C", "E-Reporting Authorization"));
					SetStatusToAwaitMenuItem = new ZMenuItem(Res.GetString("8A341A71-73F3-4DE2-82C3-C50019AEBDB0", "Awaiting Review"), SetStatusToAwait_Click);
					subMenu.MenuItems.Add(SetStatusToAwaitMenuItem);
					AuthorizeAndSendMenuItem = new ZMenuItem(Res.GetString("679EFDE5-7EB1-406B-A9D2-618B931281CE", "Authorize And Send"), AuthorizeAndSend_Click);
					subMenu.MenuItems.Add(AuthorizeAndSendMenuItem);
					ZFormMenuStrategy.AddActionsMenuItem(this, subMenu);
				}
			}

			OverrideBranchAndDepartmentMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("43a343ab-cbbc-45e8-9995-66f4f084ab6e", "Override Transaction Branch / Department"), OverrideBranchAndDepartmentButton_Click);
			AutoAllocateDiscrepancyMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("Accounting|BaseInvoicingForm|AutoAllocateDiscrepancyMenuName", "Auto-Allocate Discrepancy"), AutoAllocateDiscrepancy_Click);

			using (var salesTaxCalculator = ObjectFactory.Get<IUSSalesTaxCalculator>())
			{
				if (salesTaxCalculator.IsEnabled(invoice.Branch)
					&& salesTaxCalculator.ShouldShowMenuItemsOnInvoiceForm(invoice))
				{
					var textForCalculation = salesTaxCalculator.CalculateMenuItemText
											?? ResString.GetMultilingualString("Accounting|BaseInvoicingForm|RequestSalesTaxCalculationMenuName", "Request Sales Tax Calculation");
					var textForSubmission = salesTaxCalculator.SubmitMenuItemText
											?? ResString.GetMultilingualString("Accounting|BaseInvoicingForm|RequestSalesTaxSubmissionMenuName", "Submit Sales Tax for Transaction");
					RequestSalesTaxCalculationMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, textForCalculation, RequestSalesTaxCalculationMenuItem_Click);
					RequestSalesTaxSubmissionMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, textForSubmission, RequestSalesTaxSubmissionMenuItem_Click);
				}
			}

			ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, ResString.GetMultilingualString("BaseInvoicingForm|aea86484-3F54-4352-b9bf-6C247a5bfad2", "S&ave"));
			businessEntity.ShowError = ShowError;

			businessEntity.AH_OHInfo.ValueChanged += new EventHandler(AH_OHInfo_ValueChanged);

			invoice.ValidateExpectedInvoiceTotalInfo.ValueChanged += ValidateExpectedInvoiceTotalInfo_ValueChanged;

			if (provider != null && !provider.WorkflowType.IsEmpty)
			{
				workflowTabPage.Initialize(businessEntity);
			}
			else
			{
				workflowTabPage.TabVisible = false;
			}

			guiComponentsStateRestorer_constructorInitializedOnly = new GUIComponentsStateRestorer();

			businessEntity.Lines.ShowGLAccountsForImportAction = ShowGLAccountsForImportAction;
		}

		void ShowGLAccountsForImportAction(AccGLHeaderCollection collection, List<AccGLHeader> glHeaderList)
		{
			ZFormModaliser.ShowDialogAndDispose(new GLAccountSelectionForm(collection, glHeaderList));
		}

		IInvoiceFormPresentationProvider InvoiceFormPresentationProvider => invoiceFormPresentationProvider ?? (invoiceFormPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetInvoiceFormPresentationProvider());
		IInvoiceFormPresentationProvider invoiceFormPresentationProvider;

		void ActionsMenuItem_Popup(object sender, EventArgs e)
		{
			SetSaveAsIncompleteAccessibility();
			SetAutoAllocateDiscrepancyAccessibility();
			SetOverrideBranchAndDepartmentAccessibility();
			SetPreviewInvoiceAccessibility();
		}

		void SetSaveAsIncompleteAccessibility()
		{
			if (SaveAsIncompleteMenuItem != null)
			{
				SaveAsIncompleteMenuItem.Enabled = IsSaveAsIncompletePossible;
			}
		}

		bool IsSaveAsIncompletePossible
		{
			get
			{
				var result = false;
				if (SaveAsIncompleteMenuItem != null)
				{
					bool isInViewMode = DisplayMode == ODisplayMode.ReadOnly;
					bool isCompleted = !Invoice.IsDeleted && Invoice.AH_Ledger == LedgerTypes.AccountsPayable && (Invoice.IsInDatabase || Invoice.IsReversed) && !Invoice.IsAllocatingInvoice;

					result = (!LastSaveSuccessful && !Invoice.IsDeleted && !Invoice.IsPosted && !Invoice.IsReversed) || !(isInViewMode || IsTransactionWithApprovalRequest || isCompleted)
						|| isInViewMode && IsInCassContext;
				}
				return result;
			}
		}

		void SetOverrideBranchAndDepartmentAccessibility()
		{
			if (OverrideBranchAndDepartmentMenuItem != null)
			{
				bool isInViewMode = DisplayMode == ODisplayMode.ReadOnly;
				bool incompleteOrAllocating = Invoice.IsIncompleteInvoice || Invoice.IsAllocatingInvoice;
				bool isCompleted = !Invoice.IsDeleted && ((Invoice.IsInDatabase && !incompleteOrAllocating) || Invoice.IsReversed);
				bool isEnabled = !(isInViewMode || isCompleted);
				GUIComponentsStateRestorer.DisableMenuItemIfApplicableAndUpdateCurrentState(OverrideBranchAndDepartmentMenuItem, isEnabled);
			}
		}

		void SetAutoAllocateDiscrepancyAccessibility()
		{
			if (AutoAllocateDiscrepancyMenuItem != null)
			{
				if (DisplayMode == ODisplayMode.ReadOnly)
				{
					AutoAllocateDiscrepancyMenuItem.Enabled = false;
				}
				else if (Invoice != null && (Invoice.IsDeleted || Invoice.IsInDatabase && !Invoice.IsAllocatingInvoice && Invoice.AH_Ledger != LedgerTypes.IncompleteTransactions))
				{
					AutoAllocateDiscrepancyMenuItem.Enabled = false;
				}
			}
		}

		void SetPreviewInvoiceAccessibility()
		{
			if (PreviewInvoiceMenuItem != null && CalculateTaxTransactionsButton.Visible)
			{
				PreviewInvoiceMenuItem.Enabled = !CalculateTaxTransactionsButton.Enabled;
			}
		}

		protected readonly bool IsApprovingSelfBillingInvoice;

		public void ShowError(string message, string caption)
		{
			Globals.Message.ShowError(message, caption);
		}

		protected bool ConfirmToPrintDocument(string message)
		{
			return Globals.Message.Show(message, ComplianceDocumentHelper.PrintingCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		protected void ShowPrintingResult(string message, bool isError)
		{
			Globals.Message.Show(message, ComplianceDocumentHelper.PrintingCaption, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
		}

		#region E-Reporting Status change

		SecurityCheckpoint GetSecurityCheckpointForRequeueTransactionsWithSentStatus()
		{
			var securityCheckpoint = Env.Security.None;
			if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				securityCheckpoint = Env.Security.ReceivablesResetTransactionStatus;
			}
			else if (Invoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				securityCheckpoint = Env.Security.PayablesResetTransactionStatus;
			}

			return securityCheckpoint;
		}

		SecurityCheckpoint GetSecurityCheckpointForAwaitTransactionsWithPendingStatus()
		{
			var securityCheckpoint = Env.Security.None;
			if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				securityCheckpoint = Env.Security.ReceivablesSetTransactionStatusToAwait;
			}
			else if (Invoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				securityCheckpoint = Env.Security.PayablesSetTransactionStatusToAwait;
			}

			return securityCheckpoint;
		}

		SecurityCheckpoint GetSecurityCheckpointForAutorizeSetsStatusToQUE()
		{
			var securityCheckpoint = Env.Security.None;
			if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				securityCheckpoint = Env.Security.ReceivablesAuthorizeAndSend;
			}
			else if (Invoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				securityCheckpoint = Env.Security.PayablesAuthorizeAndSend;
			}

			return securityCheckpoint;
		}

		#endregion

		#region ReOpen Closed Job

		bool HasClosedJob
		{
			get { return Invoice != null && Invoice.HasClosedJob; }
		}

		bool AllowedReOpenClosedJob
		{
			get
			{
				return JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck((InvoicingBase)BusinessEntity, ((InvoicingBase)BusinessEntity).RelatedJobsForReversing);
			}
		}

		bool ReOpenClosedJob()
		{
			return ((InvoicingBase)BusinessEntity).ReOpenClosedJob();
		}

		#endregion

		#region Form Overrides

		protected override bool ShowAuditTab => true;

		protected override void HandleSaveWhileClosing(CancelEventArgs e)
		{
			if (IsINTransaction)
			{
				SaveAsIncomplete();
			}
			else
			{
				base.HandleSaveWhileClosing(e);
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			try
			{
				var result = ContinueWithSave.No;
				if (!BusinessEntity.Factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice))
				{
					using (InvoicingBaseTaxFrameworkViewModel.SuspendTrackingHasChanges)
					{
						if (IsINTransaction || IsINTransactionWithApprovalRequest)
						{
							((IBusinessObjectState)Invoice).HasChanges = true;
							Invoice.MoveFromIncompleteToPayableLedger();
						}
						if (!LastSaveSuccessful && Invoice.HasJobChargeTransformerBeenRun && Invoice.ShouldRunJobChargeTransformer)
						{
							Globals.Message.ShowError(Res.GetString("422c53c4-b6db-4908-aab1-445f06ae5463",
							@"The saving process has encountered an unrecoverable error. Please save this transaction as incomplete (Actions > Save as 'Incomplete') and post from the Incomplete Invoices module."));
						}
						else
						{
							var warningMessage = Invoice.AllocateAndLogComplianceSubTypeIfNumberingByPostDate();
							if (!warningMessage.IsEmpty)
							{
								Globals.Message.ShowWarning(warningMessage);
							}

							result = base.ValidateAndSave();

							if (result == ContinueWithSave.Yes && !IsINTransactionWithApprovalRequest)
							{
								SetButtonsVisibility();
								SetComplianceSequenceTextValue();
							}

							if (result == ContinueWithSave.Yes && isBaseApplyButtonClick)
							{
								var controller = TryReopenWithCorrectControllerID();
#if DEBUG
								if (Globals.IsTest)
								{
									ReopenedControllerForTest = controller;
								}
#endif
							}
						}
					}
				}

				return result;
			}
			finally
			{
				isBaseApplyButtonClick = false;
			}
		}

#if DEBUG
		public ZController ReopenedControllerForTest;
#endif

		ZController TryReopenWithCorrectControllerID()
		{
			ZController result = null;

			var invoice = (InvoicingBase)BusinessEntity;
			var tmpController = ZControllerFactory.Create(ControllerID) as INavigationControllerIDProvider;

			if (tmpController != null && invoice != null)
			{
				var correctID = tmpController.GetValidControllerID(invoice);

				if (ShouldReopenWithCorrectControllerID(correctID))
				{
					result = ZControllerFactory.Create(correctID);
					var settings = result as IBusinessEntityFactorySettings;

					if (settings != null)
					{
						settings.ShouldUseSourceEntityFactory = true;
					}

					if (result != null)
					{
						Close();
						result.ShowViewForm(invoice);
					}
				}
			}

			return result;
		}

		bool isBaseApplyButtonClick;

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			isBaseApplyButtonClick = true;

			base.OnApplyButtonClick(sender, e);
		}

		protected override void SaveCore(ITransactionParticipant[] factories)
		{
			var factoryList = new List<ITransactionParticipant>(new[] { ApprovalGUIProvider.FactoryForApprovalRequests, ARCreditNoteForAmendingApprovalGUIProvider.FactoryForApprovalRequests });
			if (!ApprovalGUIProvider.IsPostingCanceled && !ARCreditNoteForAmendingApprovalGUIProvider.IsPostingCanceled)
			{
				factoryList.AddRange(factories);
			}
			else if (ApprovalGUIProvider.IsPostingCanceled)
			{
				var transactionPendingAllocationRequestFactories = from factory in factories
																   from childFactory in factory.ChildParticipants
																   where childFactory is TransactionPendingAllocationApprovalRequestFactory
																   select childFactory;
				factoryList.AddRange(transactionPendingAllocationRequestFactories);
			}

			if (!Invoice.IsDeleted
				&& !Invoice.IsPosted
				&& !Invoice.IsIncompletInvoiceOrCreditNote)
			{
				Invoice.ResetMultiPeriodApportionmentJournals();
				Invoice.MultiPeriodApportionmentJournals.ForEach(journal => factoryList.Add(new AggregateWrapper(journal, journal)));
			}

			base.SaveCore(factoryList.ToArray());
		}

		protected override void HandleSaveException(Exception ex)
		{
			var saveException = ex as ZSaveException;
			if (saveException != null)
			{
				if (saveException.IsExceptionPresentIncludingInner<System.Data.Common.DbException>(matchExactType: false))
				{
					LastSaveSuccessful = false;
					this.DisplayMode = ODisplayMode.ReadOnly;
					SetReadOnlyIncludingChildren();
				}
			}
			try
			{
				base.HandleSaveException(ex);
			}
			catch (RethrownByExceptionHandlerException e) when (e.InnerException != null && e.InnerException is OnSavingCriticalCheckException)
			{
				FormNotificationHandler.ReportError(e.Message, Res.GetString("a2f3a9e4-a083-46d6-979f-117b84890788", "Data Validation Error"));
			}
		}

		protected virtual bool ShouldEnableApportionChargesButton
		{
			get { return ShowChargesButtons || IsInCassContext; }
		}

		bool ShouldEnableBulkChargeImportButton
		{
			get { return ShowChargesButtons; }
		}

		bool ShowChargesButtons
		{
			get
			{
				InvoicingBase formInvoice = BusinessEntity as InvoicingBase;

				bool isCorrectType = formInvoice is APInvoice || formInvoice is APCreditNote;
				bool notSaved = !BusinessEntity.IsInDatabaseIncludingChildren;
				bool isEditableMode = !IsFormOpenedAsReadOnly && !IsViewOrDeleteMode;
				bool isReversing = Invoice.IsReversing || Invoice.IsReverseTransaction;
				bool isApproving = Invoice.IsInvoiceApproving && notSaved;
				bool isIncomplete = formInvoice.AH_Ledger == LedgerTypes.IncompleteTransactions;

				return formInvoice != null
					&& isCorrectType && isEditableMode
					&& !(isReversing || formInvoice.IsApprovingInvoice)
					&& (notSaved || isIncomplete || isApproving || Invoice.IsAllocatingInvoice);
			}
		}

		bool ShowCalculateOtherTaxesButton
		{
			get
			{
				return TaxRecordParent != null
					&& DisplayMode != ODisplayMode.ReadOnly
					&& TaxRecordParent.ShouldCalculateTaxTransactions;
			}
		}

		InvoicingBaseTaxRecordParent TaxRecordParent => TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(Invoice);

		bool IsInCassContext
		{
			get { return Invoice.Factory.HasContext(BusinessContext.CASS); }
		}

		protected virtual bool ShouldShowConsolIDColumn
		{
			get { return false; }
		}

		void Invoice_MutexError(InvoicingLineBase invoiceLine, MutexErrorEventArgs e)
		{
			Globals.Message.ShowError(e.ErrorMessage, Res.GetString("BaseInvoicingForm|4905D327-C79B-4e08-ACE7-15E9C08DBE6F", "Cannot Create Job"));
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			/***********************************************************************************************************

			 Do NOT Add any code inside this method unless absolutely sure that it is to be used ONLY in this method.
			 ALL relevant code should be moved inside the method InvoicingPreSaveHelper.PreSaveActions.
			 Otherwise the other callers to InvoicingPreSaveHelper.PreSaveActions will miss out on the code you add here.
			 This can lead to inconsistencies in implementations.

			 ***********************************************************************************************************/

			var result = PreSaveActionsResult.Success();
			string validateForPostingError = InvoicingBaseTaxFrameworkViewModel.ValidateForPosting();
			if (!validateForPostingError.IsNullOrEmpty())
			{
				result = PreSaveActionsResult.Failure(validateForPostingError);
			}

			if (result.CanProceed)
			{
				ApprovalGUIProvider.InitializeNewPosting();
				ARCreditNoteForAmendingApprovalGUIProvider.InitializeNewPosting();

				if (base.ShowPreSaveDialogs() == ContinueWithSave.No)
				{
					return ContinueWithSave.No;
				}

				var alwaysCreateApprovalRequest = IsTransactionWithApprovalRequestEditing;
#if DEBUG
				if (Globals.IsTest && AlwaysCreateApprovalRequest_ForTestOnly)
				{
					alwaysCreateApprovalRequest = true;
				}
#endif

				var invoicingbase = BusinessEntity as InvoicingBase;

				ApprovalGUIProvider.AlwaysCreateApprovalRequest = alwaysCreateApprovalRequest;
				result = new InvoicingPreSaveHelper().PreSaveActions(invoicingbase, approvalGUIProvider, false, ARCreditNoteForAmendingApprovalGUIProvider, IsTransactionWithApprovalRequest, alwaysCreateApprovalRequest, true, UpdateSecurityProviderMode);
			}

			if (result.HasErrorForGUI)
			{
				Globals.Message.ShowError(result.ErrorMessage);
			}
			else if (result.CanProceed && !string.IsNullOrEmpty(result.WarningMessage))
			{
				Globals.Message.ShowWarning(result.WarningMessage);
			}

			return result.CanProceed ? ContinueWithSave.Yes : ContinueWithSave.No;
		}
		
		IInvoicingBaseTaxFrameworkViewModel InvoicingBaseTaxFrameworkViewModel => TaxFrameworkObjectFactory.GetInvoicingBaseTaxFrameworkViewModel(Invoice);

		void UpdateSecurityProviderMode(InvoicingBase creditNote)
		{
			var invoicingSecurityOverrideProvider = creditNote.SecurityOverrideProvider as InvoicingSecurityOverrideProvider;
			if (invoicingSecurityOverrideProvider != null)
			{
				invoicingSecurityOverrideProvider.RequiresTwoApprovers = creditNote.TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation.Any(x => x.EnforceTwoApproversWhenPostingARCredit);
				invoicingSecurityOverrideProvider.RequiresSequentialApprovals = creditNote.TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation.Any(x => x.EnforceSequentialApproversWhenPostingARCredit);
			}
		}

		protected BaseInvoicingFormApprovalGUIProvider ApprovalGUIProvider
		{
			get { return approvalGUIProvider ?? (approvalGUIProvider = new BaseInvoicingFormApprovalGUIProvider(this)); }
		}
		BaseInvoicingFormApprovalGUIProvider approvalGUIProvider;

		ARCreditNoteForAmendingApprovalGUIProvider ARCreditNoteForAmendingApprovalGUIProvider
		{
			get { return arCreditNoteForAmendingApprovalGUIProvider ?? (arCreditNoteForAmendingApprovalGUIProvider = new ARCreditNoteForAmendingApprovalGUIProvider()); }
		}
		ARCreditNoteForAmendingApprovalGUIProvider arCreditNoteForAmendingApprovalGUIProvider;

#if DEBUG
		internal bool AlwaysCreateApprovalRequest_ForTestOnly { get; set; }
#endif

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			var result = ContinueWithDelete.Yes;

			if (ReverseTransaction != null)
			{
				if (HasClosedJob && !IsINTransaction && !AllowedReOpenClosedJob)
				{
					result = ContinueWithDelete.No;
				}

				if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType
					&& Invoice != null
					&& Invoice.IsComplianceNumberAllocationMandatory
					&& Invoice.ShouldAllocateComplianceNumberOnPosting())
				{
					var complianceError = Invoice.AssignComplianceSubTypeAndCheckComplianceErrors();
					if (!complianceError.IsEmpty)
					{
						Globals.Message.ShowError(complianceError);
						result = ContinueWithDelete.No;
					}
				}
			}

			if (result == ContinueWithDelete.Yes)
			{
				result = base.ShowPreDeleteDialogs();
			}

			if (result == ContinueWithDelete.Yes && HasClosedJob && !IsINTransaction)
			{
				result = ReOpenClosedJob() ? ContinueWithDelete.Yes : ContinueWithDelete.No;
			}

			if (result == ContinueWithDelete.Yes && !IsINTransaction && (Invoice?.OriginalTransaction is InvoicingBase origTrans) && origTrans.ShouldConfirmComplianceSubTypeCanReverse)
			{
				result = Globals.Message.Show(AccountingConstants.ReverseConfirmComplianceSubTypeMessage, AccountingConstants.ReverseConfirmationCaptionText, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes ? ContinueWithDelete.Yes : ContinueWithDelete.No;
			}

			return result;
		}

		public override string FormVerb
		{
			get
			{
				string result;

				IBadDebtWritingOff badDebt = Invoice as IBadDebtWritingOff;
				if (badDebt != null)
				{
					result = base.FormVerb;
				}
				else if (Invoice.IsReversing)
				{
					result = IsUATransaction ? Res.GetString("BaseInvoicingForm|F33C1D1E-B525-41fb-A98C-CB929EA2951F", "Rejecting")
						: Res.GetString("BaseInvoicingForm|B72077DF-53D7-428c-86BB-B0A1243D15A1", "Reversing");
				}
				else if (IsTransactionWithApprovalRequestPosting)
				{
					result = Res.GetString("732f0c4f-1731-4c84-88f6-01386c71fa8f", "Post");
				}
				else if ((IsINTransaction || IsINTransactionWithApprovalRequest) && DisplayMode != ODisplayMode.Delete)
				{
					result = IsFormOpenedAsReadOnly ? FormVerbs.View : FormVerbs.Edit;
				}
				else
				{
					result = base.FormVerb;
				}

				return result;
			}
		}

		protected string GetCaption()
		{
			return Res.GetString("BaseInvoicingForm|1C313022-4D9F-495f-A105-8948AAEF2B97", "Print Invoice");
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				if (InvoiceDetails != null)
				{
					SetControlDefaults();
				}
				else if (!isRunWhenTabInitializedCalledInSetDataBinding)
				{
					InvoiceDetailsTabPage.RunWhenTabInitialized((sender, args) =>
					{
						SetControlDefaults();
					});
					isRunWhenTabInitializedCalledInSetDataBinding = true;
				}
			}
		}

		bool isRunWhenTabInitializedCalledInSetDataBinding;

		protected override void OnShown(EventArgs e)
		{
			if (!this.IsDesignMode())
			{
				WHTPanel.Visible = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
				if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
				{
					AH_OSExtraTaxAmountCalcEdit.CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
				}
				else
				{
					ExtraTaxPanel.Visible = false;
				}

				if (Invoice.AH_Ledger != LedgerTypes.AccountsReceivable)
				{
					InvoiceDetails.TransactionLinesGrid.RemoveFromAvailableColumns(InvoiceLine.Schema.AL_PreventInvoicePrintGrouping);
				}

				if (!Invoice.IsSettingLineExchangeRateSupported)
				{
					InvoiceDetails.TransactionLinesGrid.RemoveFromAvailableColumns(InvoiceLine.Schema.AL_ExchangeRate);
				}

				AH_RX_NKTransactionCurrencyInfo_ValueChanged(Invoice, EventArgs.Empty);
				ValidateExpectedInvoiceTotalInfo_ValueChanged(Invoice, EventArgs.Empty);
				SetInvoiceHeaderDefaults();

				if ((Invoice is APInvoice || Invoice is APCreditNote) && (Invoice.IsInDatabase || Invoice.IsReverseTransaction))
				{
					foreach (ZGridColumnInfo columnStyle in InvoiceDetails.TransactionLinesGrid.ColumnStyles)
					{
						if (columnStyle.ColumnName == InvoiceLine.Schema.AL_IsFinalCharge)
						{
							columnStyle.IsVisible = false;
							break;
						}
					}
				}

				if (ShouldShowRelatedInvoicesTab)
				{
					RelatedInvoicesTabPage.RunWhenTabInitialized((sender, args) =>
					{
						RelatedInvoicesGrid.ContextMenu.Popup += RelatedInvoicesGridContextMenu_Popup;
					});
				}
				else
				{
					RelatedInvoicesTabPage.TabVisible = false;
				}

				if ((IsINTransaction || IsTransactionWithApprovalRequestPosting) && (DisplayMode == ODisplayMode.Browse || DisplayMode == ODisplayMode.New || DisplayMode == ODisplayMode.NewSaved))
				{
					DisplayMode = ODisplayMode.Edit;
				}

				InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("InvoiceForm|0118B140-A258-44e3-838C-FFD026624F90", "View Job Details"), new EventHandler(ViewJobDetails)));
				if (ShouldEnableApportionChargesButton)
				{
					InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.Add(0, EditApportionmentMenuItem = new ZMenuItem(EditApportionmentMenuItemName, new EventHandler(HandleEditApportionment)));
				}
				SetButtonsVisibility();
				AH_OSSubTotalAmountCalcEdit.Visible = ShowOtherTaxesInInvoiceTotals;
				AH_OSOtherTaxesAmountCalcEdit.Visible = ShowOtherTaxesInInvoiceTotals;
				InvoiceTotalsCollapsibleLayoutPanel.Collapse();

				if (IsTransactionWithApprovalRequest)
				{
					DisableNewAction();
				}

				if (Invoice.IsAllocatingInvoice)
				{
					Invoice.AddWritableProperties(new string[] { "InvoiceRemittanceReference" });
				}
			}

#if DEBUG
			if (!DesignMode)
			{
#endif
				ChangeTabControlHeightAccordingToChildControlsVisible();
#if DEBUG
			}
#endif
			InvoiceDetails.CollapseTableWhenBecomeVisible();

			base.OnShown(e);

			/*
			 * THE LINE BELOW MUST BE MAINTAINED AS THE LAST LINE IN THIS METHOD. 
			 * So, please do not add any line in the method after this line as it handles the setting of critical controls. 
			 */
			UpdateFormControlsWhenTaxTransactionsCalculationChanges(isFormInitialization: true);
		}

		void ChangeTabControlHeightAccordingToChildControlsVisible()
		{
			var extendHeight = 0;
			var singleBoxHeight = 22;

			if (ShowOtherTaxesInInvoiceTotals)
			{
				extendHeight += singleBoxHeight * 2;
			}

			if (GlbCompany.CurrentCompany.GC_IsWHTRegistered)
			{
				extendHeight += singleBoxHeight;
			}

			if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				extendHeight += singleBoxHeight;
			}

			if (extendHeight != 0)
			{
				TotalAndUnallocatedTabControl.Size = ControlDpiScalingHelper.NewScaledSize(TotalAndUnallocatedTabControl.Size.Width, TotalAndUnallocatedTabControl.Size.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(extendHeight), false);
			}
		}

		bool ShowOtherTaxesInInvoiceTotals => ShowCalculateOtherTaxesButton || ((Invoice.IsInDatabase || Invoice.IsReverseTransaction) && OtherTaxesDisplayObbj.TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection.Any());

		InvoicingBaseForDisplayOtherTaxes OtherTaxesDisplayObbj => TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(Invoice);

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				LineChargesTabPage.RunWhenTabInitialized((sender, args) =>
				{
					bool hideLineChargesGridAndShowRestrictionLabel =
					Invoice.AH_Ledger == LedgerTypes.AccountsReceivable &&
					Invoice.AH_GC != GlbCompany.CurrentCompany.PK &&
					!Env.Security.APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcs.IsAllowed;
					this.LineChargesGrid.Visible = !hideLineChargesGridAndShowRestrictionLabel;
					this.RestrictedLineChargesLabel.Visible = hideLineChargesGridAndShowRestrictionLabel;
					if (this.RestrictedLineChargesLabel.Visible)
					{
						this.RestrictedLineChargesLabel.Text = Env.Security.APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcs.ErrorMessageForNotAllowed;
					}
				});

				sourceXmlTabPage.TabVisible = Invoice.IsImportedFromUniversalXML;
			}
		}

		protected void ViewJobDetails(object sender, EventArgs e)
		{
			if (InvoiceDetails.TransactionLinesGrid.SelectedElements.Length > 0)
			{
				Job job = ((InvoicingLineBase)InvoiceDetails.TransactionLinesGrid.SelectedElements[0]).InvoicingJob;
				if (job != null)
				{
					if (GlbCompany.CurrentCompany.Branches.Contains(Invoice.Branch))
					{
						ZControllerFactory.Create(ControllerIDs.JobHeader).ShowViewForm(job);
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("ae913224-a6c0-4839-98a3-0897646457d4", "You cannot view job details when the transaction is from another company."));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("01970eb4-d2b9-4711-8d6c-4cebe7ceb84c", "This invoice line doesn't have a job."));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("6ab3856f-2648-445f-b157-49092f65e910", "Please select an invoice line."));
			}
		}

		void SetComplianceSequenceTextValue()
		{
			if (InvoiceDetails != null && InvoiceDetails.ComplianceSequenceTextBox.Visible)
			{
				try
				{
					InvoiceDetails.ComplianceSequenceTextBox.Text = Invoice.ComplianceSequenceWithCodeAndDesc;
				}
				catch (ComplianceSequenceRelatedException)
				{
					InvoiceDetails.ComplianceSequenceTextBox.Text = string.Empty;
				}
			}
		}

		void SetButtonsVisibility()
		{
			if (InvoiceDetails != null)
			{
				InvoiceDetails.ApportionChargesButton.Visible = ShouldEnableApportionChargesButton;
				InvoiceDetails.BulkChargeImportButton.Visible = ShouldEnableBulkChargeImportButton;
				SetCalculateOtherTaxesButtonVisibility();

				var linesGrid = InvoiceDetails.TransactionLinesGrid;
				if (linesGrid.AllowSorting && linesGrid.ListManager != null && linesGrid.Sort != AccTransactionLinesSchema.Constants.AL_Sequence)
				{
					linesGrid.Sort = AccTransactionLinesSchema.Constants.AL_Sequence;
				}
				linesGrid.AllowSorting = !IsViewOrDeleteMode && (!Invoice.IsInDatabase || Invoice.AH_Ledger == LedgerTypes.IncompleteTransactions || Invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions);
				if (linesGrid.AllowSorting && linesGrid.ListManager != null && linesGrid.Sort != AccTransactionLinesSchema.Constants.AL_Sequence)
				{
					linesGrid.Sort = AccTransactionLinesSchema.Constants.AL_Sequence;
				}
			}
		}

		void SetCalculateOtherTaxesButtonVisibility()
		{
			TaxRecordParent.OnOtherTaxesCalculatedBeforePosting_Changed -= OnOtherTaxesCalculatedBeforePosting_Changed;
			CalculateTaxTransactionsButton.Visible = ShowCalculateOtherTaxesButton;
			if (CalculateTaxTransactionsButton.Visible)
			{
				TaxRecordParent.OnOtherTaxesCalculatedBeforePosting_Changed += OnOtherTaxesCalculatedBeforePosting_Changed;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (BusinessEntity != null)
			{
				if (BusinessEntity.Factory.HasContext(BusinessContext.AllocatingTransaction))
				{
					BusinessEntity.Factory.RemoveContext(BusinessContext.AllocatingTransaction);
				}
			}

			base.OnClosing(e);
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			if (BusinessEntity != null && BusinessEntity.HasChanges)
			{
				base.ZForm_Closing(sender, e);
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			var invoicingBase = BusinessEntity as InvoicingBase;
			if (invoicingBase != null)
			{
				//Notice that there is no need to call RemoveNotRequiredJobsAndChargesFromCostsAndLines here, because the form is closed and there is no further save operation.
				invoicingBase.ReleaseAllMutexOnInvoice();
			}
		}

		protected virtual string CaptionForInsertingIntoLabels
		{
			get
			{
				return FormCaption;
			}
		}

		protected override string FormClosingQuestion
		{
			get
			{
				if (IsINTransaction)
				{
					return Res.GetString("aaf5d8b1-60ba-49c3-b651-ef4a0eddb61b", @"This record has been modified.
Would you like to save the changes?

'Yes' will save your changes as incomplete.
'No' will discard your changes.
'Cancel' will return you to the form.");
				}
				return base.FormClosingQuestion;
			}
		}

		public override string FormCaption
		{
			get
			{
				if (IsINTransaction)
				{
					return IncompleteFormCaption;
				}
				else if (IsTransactionWithApprovalRequestEditing)
				{
					return UnapprovedFormCaption;
				}
				else
				{
					return NormalFormCaption;
				}
			}
		}

		protected virtual string IncompleteFormCaption
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0} ", Res.GetString("Accounting|BaseInvoicingForm|Incomplete", "Incomplete")); }
		}
		protected virtual string UnapprovedFormCaption
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0} ", Res.GetString("Accounting|BaseInvoicingForm|Unapproved", "Unapproved")); }
		}

		protected virtual string NormalFormCaption
		{
			get { return string.Empty; }
		}

		#region Handle Invalid Controller ID

		protected virtual bool ShouldReopenWithCorrectControllerID(ControllerID controllerId)
		{
			return false;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				DisplayModeChanged -= BaseInvoicingForm_DisplayModeChanged;
				if (InvoiceDetails != null)
				{
					InvoiceDetails.ApportionChargesButton.Click -= ApportionChargesButton_Click;
					InvoiceDetails.BulkChargeImportButton.Click -= BulkChargeImportButton_Click;
				}
				if (components != null)
				{
					components.Dispose();
				}
				if (Invoice != null)
				{
					Invoice.AH_RX_NKTransactionCurrencyInfo.ValueChanged -= AH_RX_NKTransactionCurrencyInfo_ValueChanged;
					Invoice.ValidateExpectedInvoiceTotalInfo.ValueChanged -= ValidateExpectedInvoiceTotalInfo_ValueChanged;
					Invoice.MutexError -= Invoice_MutexError;
					Invoice.ShowError = null;
					Invoice.AH_OHInfo.ValueChanged -= new EventHandler(AH_OHInfo_ValueChanged);
					TaxRecordParent.OnOtherTaxesCalculatedBeforePosting_Changed -= OnOtherTaxesCalculatedBeforePosting_Changed;
				}
				if (BusinessEntity != null)
				{
					SecurityOverrideProviderSource.Get(BusinessEntity).Provider = null;
					if (BusinessEntity is APInvoice || BusinessEntity is APCreditNote)
					{
						Invoice.OnJobChanged -= Invoice_OnJobChanged;
						Invoice.Lines.ShowJobChargesForImportEvent -= InvoiceForm_ShowJobChargesForImportEvent;
						Invoice.Lines.ApportionedInvoiceLineModified -= InvoiceForm_ApportionedInvoiceLineModified;
						if (InvoiceDetails != null)
						{
							InvoiceDetails.TransactionLinesGrid.RowsDeleting -= TransactionLinesGrid_RowDeleting;
							InvoiceDetails.TransactionLinesGrid.RowsDeleted -= TransactionLinesGrid_RowDeleted;
							InvoiceDetails.TransactionLinesGrid.ContextMenu.Popup -= TransactionLinesGridContextMenu_Popup;
						}
						BusinessEntity.Factory.Saved -= Factory_Saved;
					}
					if (ShouldShowRelatedInvoicesTab && RelatedInvoicesGrid != null)
					{
						RelatedInvoicesGrid.ContextMenu.Popup -= RelatedInvoicesGridContextMenu_Popup;
					}
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Test Methods

#if DEBUG

		public void SetReversingReason_ForTestOnly()
		{
			fReversingCode = "TST";
			fReversingReason = "Test Reversing Reason";
		}

#endif

		#endregion

		#endregion

		#region Implementation

		protected virtual bool ShouldShowRelatedInvoicesTab
		{
			get { return true; }
		}

		protected bool ShouldShowOriginalInvoiceReferenceFields => BusinessEntity is InvoicingBase transactionHeader && transactionHeader.ShouldShowOriginalInvoiceReferenceFields;

		protected virtual void SetInvoiceHeaderDefaults()
		{
			SetVisibilityDefaults();
			SetOriginalInvoiceReferenceFieldsVisibility(ShouldShowOriginalInvoiceReferenceFields);

			var currCompany = GlbCompany.CurrentCompany;

			if (currCompany.Country.SupportComplianceSubType && !AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
			{
				var subType = InvoiceDetails.AH_ComplianceSubTypeDropEdit;
				subType.Visible = true;
				subType.GetExtension<LabelCaptionRenderer>().Visible = true;

				var complNr = InvoiceDetails.AH_TransactionReferenceTextBox;
				complNr.Visible = true;
				complNr.ReadOnly = true;

				var complSeq = InvoiceDetails.ComplianceSequenceTextBox;
				complSeq.Visible = true;
				complSeq.ReadOnly = true;
				SetComplianceSequenceTextValue();
			}

			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				InvoiceDetails.AH_GB_TaxBranchFindBox.Visible = false;
			}

			InvoiceDetails.SourceReferenceTextBox.Visible = Invoice.IsSourceReferenceEnabled;

			switch (Invoice.AH_Ledger)
			{
				case LedgerTypes.AccountsPayable:
				case LedgerTypes.UnapprovedPayableTransactions:
				case LedgerTypes.IncompleteTransactions:
					{
						if (ShouldShowExpectedTotal)
						{
							SetExpectedInvoiceTotal();
						}

						InvoiceDetails.GovernmentAllocatedIDTextBox.Visible = AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.Value;

						var disbInv = InvoiceDetails.IsDisbursementInvoiceCheckBox;
						disbInv.Text = Res.GetString("BaseInvoicingForm|3CA23977-D000-4628-B26B-20FD38E27C3A", "Is Self Billing Invoice");
						disbInv.BindTo = Invoice.IsSelfBillingInvoiceInfo.Name;
						break;
					}
				default:
					{
						if (DisplayMode == ODisplayMode.New)
						{
							Invoice.IsDisbursementOrFinal = Invoice.IsDisbursementOrFinal;
							DisplayMode = ODisplayMode.New;
						}

						var invTerm = InvoiceDetails.AH_InvoiceTermDropEdit;
						invTerm.GetExtension<LabelCaptionRenderer>().Visible = true;
						invTerm.Visible = true;
						var invTermDays = InvoiceDetails.AH_InvoiceTermDaysCalcEdit;
						invTermDays.GetExtension<LabelCaptionRenderer>().Visible = true;
						invTermDays.Visible = true;
						break;
					}
			}

			if (currCompany.GC_RN_NKCountryCode != CountryCodes.Thailand)
			{
				InvoiceDetails.CashBasisVATIndicatorCheckbox.Visible = false;
			}

			AH_OSTaxAmountCalcEdit.Visible = Invoice.IsTaxed || currCompany.GC_IsGSTRegistered;
			if (AH_OSTaxAmountCalcEdit.Visible)
			{
				AH_OSTaxAmountCalcEdit.CaptionResourceString = AH_OSTaxAmountCalcEdit.CaptionResourceString.Format(Invoice.Company.ConsumptionTaxDescriptionForCompanyForm);
			}

			OSSubTotalExTaxAmountCalcEdit.Visible = Invoice.IsTaxed || currCompany.GC_IsGSTRegistered;

			if (Invoice.IsAPInvoiceOrCreditNote)
			{
				SetGSTInclusiveAmountsCheckBoxVisible();
			}
		}

		SecurityCheckpoint OverrideBranchAndDepartmentSecurityCheckpoint
		{
			get
			{
				SecurityCheckpoint result = null;

				switch (Invoice.AH_Ledger)
				{
					case LedgerTypes.AccountsPayable:
					case LedgerTypes.IncompleteTransactions:
					case LedgerTypes.UnapprovedPayableTransactions:
						//case LedgerTypes.TransactionsPendingAllocation:
						result = Env.Security.PayablesOverrideTransactionBranchAndDepartment;
						break;
					case LedgerTypes.AccountsReceivable:
						result = Env.Security.ReceivableOverrideTransactionBranchAndDepartment;
						break;
					default:
						break;
				}

				return result;
			}
		}

		protected bool AllowOverridingTransactionsBranchAndDepartment
		{
			get
			{
				return OverrideBranchAndDepartmentSecurityCheckpoint != null && OverrideBranchAndDepartmentSecurityCheckpoint.IsAllowed;
			}
		}

		void SetGSTInclusiveAmountsCheckBoxVisible()
		{
			if (GlbCompany.CurrentCompany.GC_IsGSTRegistered)
			{
				if ((Invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable || Invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.IncompleteTransactions || Invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions) && DisplayMode == ODisplayMode.New)
				{
					InvoiceDetails.GSTInclusiveAmountsCheckBox.Visible = true;
				}
				else if (Invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable && Invoice.IsConvertedFromARInvoice && DisplayMode == ODisplayMode.Edit)
				{
					InvoiceDetails.GSTInclusiveAmountsCheckBox.Visible = true;
				}
			}
		}

		bool ShouldShowExpectedTotal
		{
			get { return DisplayMode == ODisplayMode.New || DisplayMode == ODisplayMode.Edit || Invoice.IsAllocatingInvoice || Invoice.AH_Ledger == LedgerTypes.IncompleteTransactions || Invoice.IsImportedFromFile || Invoice.IsConvertedFromARInvoice; }
		}

		void SetVisibilityDefaults()
		{
			InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible = false;
			InvoiceDetails.InvoiceTotalValidationCheckBox.Visible = false;

			InvoiceDetails.ExpectedTaxCalcEdit.Visible = false;
			InvoiceDetails.ExpectedExclTaxCalcEdit.Visible = false;

			AH_OHInfo_ValueChanged(null, EventArgs.Empty);

			InvoiceDetails.TransactionGuidFindBox.Visible = BusinessEntity is IAmending && ((IAmending)BusinessEntity).IsAmendingTransaction;
			InvoiceDetails.TransactionGuidFindBox.GetExtension<LabelCaptionRenderer>().Visible = InvoiceDetails.TransactionGuidFindBox.Visible;
			InvoiceDetails.AH_ComplianceSubTypeDropEdit.Visible = false;
			InvoiceDetails.AH_ComplianceSubTypeDropEdit.GetExtension<LabelCaptionRenderer>().Visible = false;
			InvoiceDetails.AH_TransactionReferenceTextBox.Visible = false;
			InvoiceDetails.ComplianceSequenceTextBox.Visible = false;
			InvoiceDetails.AH_InvoiceTermDropEdit.GetExtension<LabelCaptionRenderer>().Visible = false;
			InvoiceDetails.AH_InvoiceTermDaysCalcEdit.GetExtension<LabelCaptionRenderer>().Visible = false;
			InvoiceDetails.AH_InvoiceTermDaysCalcEdit.Visible = false;
			InvoiceDetails.AH_InvoiceTermDropEdit.Visible = false;
			InvoiceDetails.GSTInclusiveAmountsCheckBox.Visible = false;
			InvoiceDetails.IsDisbursementInvoiceCheckBox.Visible = true;
			InvoiceDetails.SourceReferenceTextBox.Visible = false;
		}

		protected void SetOriginalInvoiceReferenceFieldsVisibility(bool visible)
		{
			InvoiceDetails.TransactionGuidFindBox.Visible = visible;
			InvoiceDetails.TransactionGuidFindBox.GetExtension<LabelCaptionRenderer>().Visible = visible;
			InvoiceDetails.AH_OriginalInvoiceDateEdit.Visible = visible;
			InvoiceDetails.AH_OriginalInvoiceDateEdit.GetExtension<LabelCaptionRenderer>().Visible = visible;
			InvoiceDetails.AH_OriginalTransactionNumTextBox.Visible = visible;
			InvoiceDetails.AH_OriginalTransactionNumTextBox.GetExtension<LabelCaptionRenderer>().Visible = visible;
		}

		void SetExpectedInvoiceTotal()
		{
			InvoiceDetails.InvoiceTotalValidationCheckBox.Visible = true;
			InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible = true;

			Invoice.UpdateExpectedAmountFromOSAmount();
		}

		public static string[] GetColumnsToRemoveIfNotGSTRegistered()
		{
			return new string[] {
				InvoicingLineBase.Schema.AL_AT,
				InvoicingLineBase.Schema.AL_TaxDate,
				InvoicingLineBase.Schema.AL_A9_VATClass,
				InvoicingLineBase.Schema.AL_OSTaxAmount,
				InvoicingLineBase.Schema.AL_OSGSTAmount,
				InvoicingLineBase.Schema.GSTInclusiveAmount,
				InvoicingLineBase.Schema.AL_LocalGSTAmount,
				InvoicingLineBase.Schema.AL_LocalTaxAmount,
				InvoicingLineBase.Schema.AL_LocalTotalAmount,
				InvoicingLineBase.Schema.AL_OverseasTotal,
				InvoicingLineBase.Schema.AL_GB_TaxBranch,
				InvoicingLineBase.Schema.TaxBranchName
			};
		}

		#region SetControlDefaults

		void SetControlDefaults()
		{
			if (Invoice == null)
			{
				return;
			}

			SetTransactionLineGridDefaults();

			SetJobSummaryGridDefaults();

			if (!this.IsDesignMode() &&
				(Invoice.AH_Ledger == LedgerTypes.AccountsPayable ||
				Invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
				Invoice.AH_Ledger == LedgerTypes.IncompleteTransactions))
			{
				InvoiceDetails.AddressWithContactControl.CaptionResourceString = Res.GetData("InvoiceUserControl|a505e45d-46f4-4760-b01d-b03fd6ebd105", "Creditor Information");
			}
			InvoiceDetails.SetInvoiceTypeCaptions(FormCaption, CaptionForInsertingIntoLabels);
		}

		void SetTransactionLineGridDefaults()
		{
			InvoiceDetails.LineSummaryTabPage.RunWhenTabInitialized((sender, e) =>
			{
				var shouldKeepTaxColumns = Invoice.IsTaxed || GlbCompany.CurrentCompany.GC_IsGSTRegistered;
				var columnsToRemoveIfNotGSTRegistered = GetColumnsToRemoveIfNotGSTRegistered();
				for (int i = InvoiceDetails.TransactionLinesGrid.ColumnStyles.Count - 1; i >= 0; i--)
				{
					ZGridColumnInfo columnStyle = (ZGridColumnInfo)InvoiceDetails.TransactionLinesGrid.ColumnStyles[i];

					if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable || Invoice is AdjustmentNote)
					{
						if ((!Invoice.IsJobRelated && (columnStyle.ColumnName == InvoicingLineBase.Schema.AL_JH || columnStyle.ColumnName == "JobLocalReference"))
							|| columnStyle.ColumnName == InvoicingLineBase.Schema.AL_IsFinalCharge)
						{
							InvoiceDetails.TransactionLinesGrid.ColumnStyles.Remove(columnStyle);
						}
					}

					if (!ShouldShowConsolIDColumn && columnStyle.ColumnName == "ConsolIDFromApportionedCharge")
					{
						InvoiceDetails.TransactionLinesGrid.ColumnStyles.Remove(columnStyle);
					}

					if (!shouldKeepTaxColumns & columnsToRemoveIfNotGSTRegistered.Contains(columnStyle.ColumnName))
					{
						InvoiceDetails.TransactionLinesGrid.ColumnStyles.Remove(columnStyle);
					}

					if (!GlbCompany.CurrentCompany.GC_IsWHTRegistered &&
						(columnStyle.ColumnName == InvoicingLineBase.Schema.AL_AW || columnStyle.ColumnName == InvoicingLineBase.Schema.AL_OSWHTAmount || columnStyle.ColumnName == InvoicingLineBase.Schema.AL_LocalWHTAmount))
					{
						InvoiceDetails.TransactionLinesGrid.ColumnStyles.Remove(columnStyle);
					}

					if (columnStyle.ColumnName == InvoicingLineBase.Schema.GSTInclusiveAmount && (!(Invoice is APInvoice) && !(Invoice is APCreditNote) || Invoice.IsInDatabase/*for ODisplayMode.New*/))
					{
						InvoiceDetails.TransactionLinesGrid.ColumnStyles.Remove(columnStyle);
					}

					if ((columnStyle.ColumnName == InvoicingLineBase.Schema.AL_GB_TaxBranch || columnStyle.ColumnName == InvoicingLineBase.Schema.TaxBranchName) &&
						!InvoiceFormPresentationProvider.IsTaxBranchColumnVisible())
					{
						InvoiceDetails.TransactionLinesGrid.ColumnStyles.Remove(columnStyle);
					}
				}
			});
		}

		void SetJobSummaryGridDefaults()
		{
			InvoiceDetails.JobSummaryTabPage.RunWhenTabInitialized((sender, e) =>
			{
				for (int i = InvoiceDetails.JobSummaryGrid.ColumnStyles.Count - 1; i >= 0; i--)
				{
					ZGridColumnInfo columnStyle = (ZGridColumnInfo)InvoiceDetails.JobSummaryGrid.ColumnStyles[i];

					if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered &&
						(columnStyle.ColumnName == "JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency"
						|| columnStyle.ColumnName == "JH_RelatedInvoiceLinesCostTaxAmount"
						|| columnStyle.ColumnName == "JH_RelatedInvoiceLinesTotalCost"
						|| columnStyle.ColumnName == "JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency"))
					{
						InvoiceDetails.JobSummaryGrid.ColumnStyles.Remove(columnStyle);
					}
				}
			});
		}

		#endregion

		protected InvoicingBase Invoice
		{
			get { return fInvoice ?? (fInvoice = (InvoicingBase)DataSource); }
		}
		InvoicingBase fInvoice;

		protected virtual JobChargesPopupForm NewJobChargesPopupForm(JobChargesImporter importer)
		{
			return new JobChargesPopupForm(importer);
		}

		protected bool IsFormOpenedAsReadOnly { get; private set; }

		protected override INotificationHandler FormNotificationHandler
		{
			get
			{
				var message = !Invoice.IsContainCashAdvanceCurrencyMismatch && IsSaveAsIncompletePossible ? Res.GetString("2f7117aa-733d-4c00-8e34-51907517621b", @"You can also try to save the transaction as 'incomplete'") : ZString.Empty.ToString();
				return new InvoiceNotificationHandler(message);
			}
		}

		MultilingualString EditApportionmentMenuItemName
		{
			get { return ResString.GetMultilingualString("InvoiceForm|FCA752C3-ED5C-43cf-A081-42765BDED165", "Edit Apportionment"); }
		}

		MenuItem EditApportionmentMenuItem;

		#region class InvoiceNotificationHandler

		class InvoiceNotificationHandler : INotificationHandler
		{
			public InvoiceNotificationHandler(string extraMessage)
			{
				ExtraMessage = extraMessage;
			}

			readonly string ExtraMessage;

			void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
			{
				NotificationHandler.Instance.ReportError(GetFullMessage(message), caption);
			}

			void INotificationHandler.ReportInformation(string message, string caption)
			{
				NotificationHandler.Instance.ReportInformation(GetFullMessage(message), caption);
			}

			string GetFullMessage(string message)
			{
				var messagebuilder = new StringBuilder();
				messagebuilder.AppendLine(message);
				if (!string.IsNullOrEmpty(ExtraMessage))
				{
					messagebuilder.AppendLine();
					messagebuilder.Append(ExtraMessage);
				}
				return messagebuilder.ToString();
			}
		}

		#endregion

		void AH_OHInfo_ValueChanged(object sender, EventArgs e)
		{
			bool visible = false;
			InvoicingBase invoice = BusinessEntity as InvoicingBase;
			if (invoice != null)
			{
				visible = invoice.IsExpectedTaxTotalVisible && ShouldShowExpectedTotal;

				if (!visible)
				{
					invoice.ExpectedInvoiceTaxTotal = ZDecimal.Zero;
					invoice.ExpectedInvoiceExclTaxTotal = ZDecimal.Zero;
				}
			}

			InvoiceDetails.ExpectedTaxCalcEdit.Visible = visible;
			InvoiceDetails.ExpectedExclTaxCalcEdit.Visible = visible;

			ExcludingTaxPanel.Visible = visible;
			TaxTotalPanel.Visible = visible;
		}

		#endregion

		void SaveAsIncomplete()
		{
			var invoicingBase = (InvoicingBase)BusinessEntity;

			using (invoicingBase.Factory.SetTempContext(BusinessContext.SavingIncompleteTransaction))
			using (InvoicingBaseTaxFrameworkViewModel.SuspendTrackingHasChanges)
			{
				invoicingBase.RunPreSaveValidation();

				if (invoicingBase.HasErrors())
				{
					ShowErrorsDialog();
				}
				else
				{
					string extraMsg = null;
					try
					{
						extraMsg = invoicingBase.SaveAsIncomplete();
					}
					catch (OnSavingCriticalCheckException ex)
					{
						Globals.Message.ShowError(ex.Message, Res.GetString("E8778220-1D27-4D39-A75D-7306537AC522", "Critical Validation Error"));
						return;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						HandleSaveException(ex);
						return;
					}

					if (!extraMsg.IsNullOrEmpty())
					{
						extraMsg = "\r\n" + extraMsg;
					}
					Globals.Message.Show(Res.GetString("8946fb47-6cad-49f0-b39d-3579489d82ad", "Save as incomplete successful") + extraMsg,
						Res.GetString("a2177e00-7f3f-4325-9b19-a408e191ecac", "Saved"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		#region Event Handlers

		void ResetStatusToQueued_Click(object sender, EventArgs e)
		{
			if (!Invoice.IsNull)
			{
				BusinessObject[] selectedBusinessObjects = { Invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.ResetStatusToQueued(GetSecurityCheckpointForRequeueTransactionsWithSentStatus());
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("dc7e2925-b8df-463e-9055-3a7691d0aa4c", "You can only reset transactions where E-Reporting status is 'FAL' - Fail or 'BER' - Batched with errors,\r\nor 'SNT' when you have the appropriate security rights.\r\nNo transactions will be reset."));
			}
		}

		void SetStatusToAwait_Click(object sender, EventArgs e)
		{
			if (!Invoice.IsNull)
			{
				BusinessObject[] selectedBusinessObjects = { Invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.SetStatusToAwait(GetSecurityCheckpointForAwaitTransactionsWithPendingStatus());
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("DA69E620-B884-4662-ABA5-9E4CEBF92C0F", "You can only review transactions where E-Reporting status is 'PEN' - Pending, when you have the appropriate security rights.\r\nNo transactions status will be set to Awaiting Review."));
			}
		}

		void AuthorizeAndSend_Click(object sender, EventArgs e)
		{
			if (!Invoice.IsNull)
			{
				BusinessObject[] selectedBusinessObjects = { Invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.SetStatusToQueued(GetSecurityCheckpointForAutorizeSetsStatusToQUE());
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("9C2E161F-9EDF-43BE-8D15-798BC6371015", "You can only Authorize and Send transactions where the E-Reporting status is 'PEN - Pending' or 'AWA - Awaiting Review' and when you have the appropriate security rights.\r\nNo transactions will be set."));
			}
		}

		void SaveAsIncomplete_Click(object sender, EventArgs e)
		{
			SaveAsIncomplete();
		}

		void PreviewInvoice_Click(object sender, EventArgs e)
		{
			InvoicingBase invoicingBase = (InvoicingBase)BusinessEntity;
			invoicingBase.RunPreSaveValidation();
			if (invoicingBase.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				if (invoicingBase.AH_Ledger == LedgerTypes.AccountsPayable
					|| invoicingBase.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					try
					{
						new InvoicePrintTask(new InvoicePrintTask.Configuration((TransactionHeader)BusinessEntity) { MenuNames = [JobInvoicingEDocsProviderSupporter.CostConfirmationDocument] }).Run(false);
					}
					catch (UnableToFindInvoiceDocumentCommandException ex)
					{
						Globals.Message.Show(ex.Message, Res.GetString("7d06e35d-3fb8-47e3-81d8-67635cfbba30", "Preview Invoices"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				else if (invoicingBase.HasApprovalRequestAndNotPosted)
				{
					var newFactory = new BusinessObjectFactory();
					var invoiceReloaded = newFactory.Load<APInvoice>(invoicingBase.PK);
					var restoreResult = invoiceReloaded.RestoreSavedData(false); // deserialize data for incomplete invoice so lines can be included in the document

					if (restoreResult.Result == InvoicingBase.RestoreSavedDataResult.ResultType.Success)
					{
						invoiceReloaded.MoveFromIncompleteToPayableLedger();
						using (var printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoiceReloaded) {  MenuNames = [JobInvoicingEDocsProviderSupporter.CostConfirmationDocument], Factory = newFactory }))
						{
							printTask.Run(false);
						}
					}
					else
					{
						Globals.Message.ShowError(restoreResult.Error, Res.GetString("{950D086E-9E48-4CB5-86D9-3881AFD6E606}", "Preview Invoice"));
					}
				}
				else if (invoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					var helper = GetInvoicingSecurityHelper(invoicingBase);
					var allowedToPreviewInvoice = (helper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PreviewOnly) ?? true);

					if (!allowedToPreviewInvoice)
					{
						helper.ShowError(SecurityCore.PreviewOnly);
					}
					else
					{
						using (var task = new InvoicePrintTask(new InvoicePrintTask.Configuration((InvoicingBase)BusinessEntity)))
						{
							task.Run(false);
						}
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("b30c8959-0d38-4bf5-8dea-4685a31c5ded", "This transaction type is not supported to preview invoices"), Res.GetString("7d06e35d-3fb8-47e3-81d8-67635cfbba30", "Preview Invoices"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		JobInvoicingSecurityHelper GetInvoicingSecurityHelper(InvoicingBase invoicingBase)
		{
			JobInvoicingSecurityHelper helper = null;
			switch (invoicingBase.AH_TransactionType)
			{
				case ZArchitecture.Core.TransactionTypes.Invoice:
					helper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesInvoice, false);
					break;
				case ZArchitecture.Core.TransactionTypes.CreditNote:
					helper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesCreditNote, false);
					break;
				case ZArchitecture.Core.TransactionTypes.AdjustmentNote:
					helper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesAdjustmentNote, false);
					break;
			}
			return helper;
		}

		void AutoAllocateDiscrepancy_Click(object sender, EventArgs e)
		{
			InvoicingBase invoicingBase = (InvoicingBase)BusinessEntity;

			if (TaxRecordParent != null && TaxRecordParent.IsApplicableForTaxTransactions)
			{
				Globals.Message.ShowInformation(Res.GetString("F3F49129-8E99-441A-86B4-A91486B5E5AE", "Auto-allocate Discrepancy feature is disabled because the login company has one or more active {0} Tax Framework Configurations.", invoicingBase.AH_Ledger), Res.GetString("43C58DE8-E2DF-4D1A-B091-FCDC3749405E", "Cannot Auto-allocate Discrepancy"));
				return;
			}

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoicingBase);

			using (invoicingBase.SuspendExpectedInvoiceTotalValidation)
			{
				invoicingBase.RunPreSaveValidation();
			}

			if (invoicingBase.HasErrors())
			{
				if (!Globals.IsTest)
				{
					ZMessageBox messageBox = new ZErrorMessageBox(invoicingBase, invoicingBase.HumanReadableName, "Auto-Allocate", "Auto-Allocated");
					ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
					messageBox.Dispose();
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("449c55ae-616a-4d10-9120-06814ac2cf77", "There are errors - can't auto-allocate."), Res.GetString("387408b7-dfa9-4601-bbf4-a2758ac33482", "Errors!"));
				}
			}
			else
			{
				string errorMessage = allocator.Validate();

				if (!string.IsNullOrEmpty(errorMessage))
				{
					Globals.Message.ShowError(errorMessage, Res.GetString("5b38456a-c48d-4b28-a039-7a59f06d2afd", "Error"));
				}
				else
				{
					DialogResult result = Globals.Message.Show(Res.GetString("64ec2e97-4184-4b66-903c-19541642e75a", "Proceed to auto-allocate the discrepancy values proportionately based on entered/imported amounts?"), Res.GetString("1476787a-89a0-4cb2-821c-ad6cf308a8e7", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					if (result == DialogResult.Yes)
					{
						allocator.Allocate();
					}
				}
			}
		}

		#region Sales Tax Calculation Menu Item Handlers

		void RequestSalesTaxCalculationMenuItem_Click(object sender, EventArgs e)
		{
			using (var calculator = ObjectFactory.Get<IUSSalesTaxCalculator>())
			{
				HandleSalesTaxCalculationMenuItemClick(
					calculator,
					calculator.CheckpointForCalculationMenuItem,
					calculator.CalculateSalesTax,
					ResString.GetMultilingualString("fdb03159-0ca5-4454-9371-6d533615812f", "Sales Tax Calculation"),
					ResString.GetMultilingualString("bdc832ad-9340-444e-be73-7f76f1760f0f", "An error occurred calculating sales tax using {0}:", calculator.Name),
					ResString.GetMultilingualString("514ea611-f905-4899-8f28-b9696066a842", "Calculated Sales Tax Amount")
				);
			}
		}

		void RequestSalesTaxSubmissionMenuItem_Click(object sender, EventArgs e)
		{
			using (var calculator = ObjectFactory.Get<IUSSalesTaxCalculator>())
			{
				HandleSalesTaxCalculationMenuItemClick(
					calculator,
					calculator.CheckpointForSubmitMenuItem,
					calculator.SubmitSalesTax,
					ResString.GetMultilingualString("88e04aed-4826-4bf7-8847-77ba19c19a6b", "Submit Sales Tax"),
					ResString.GetMultilingualString("0025e008-fe78-491a-8830-60b5f2b7c31e", "An error occurred submitting sales tax using {0}:", calculator.Name),
					ResString.GetMultilingualString("9e7b35ff-4bc3-403a-b50e-98e829783523", "Submitted Sales Tax Amount")
				);
			}
		}

		void HandleSalesTaxCalculationMenuItemClick(
			IUSSalesTaxCalculator calculator,
			Func<InvoicingBase, SecurityCheckpoint> getCheckpoint,
			Func<InvoicingBase, (CalculationResult, Exception)> runAction,
			MultilingualString messageCaption,
			MultilingualString errorLine,
			MultilingualString salesTaxAmountCaptionText
		)
		{
			var invoicingBase = (InvoicingBase)BusinessEntity;

			if (!calculator.IsEnabled(invoicingBase.Branch))
			{
				return;
			}

			var checkpoint = getCheckpoint(invoicingBase);
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			var (salesTaxResult, ex) = runAction(invoicingBase);
			if (ex != null)
			{
				var flattenedExceptionsAsString =
					string.Join(System.Environment.NewLine,
								ex.FlattenInnerExceptions().Select(ex2 => FormattableString.Invariant($"  {ex2.GetType().Name}: {ex2.Message}"))
					);
				var message = FormattableString.Invariant($@"{errorLine}
{flattenedExceptionsAsString}

{calculator.MenuItemTroubleshootingHint}");

				Globals.Message.ShowError(message, messageCaption);
				return;
			}

			var (_, chargeCode) = calculator.GetChargeCode(invoicingBase.Branch);
			var currentSalesTaxAmount = calculator.GetCurrentSalesTaxAmount(invoicingBase);
			var salesTaxEnvironment = calculator.GetConfiguration(invoicingBase.Branch);

			var messageText = Res.GetString("8433628a-d050-41c3-ba89-8aa35540a92d", @"{0}: {1:C2}

Current Sales Tax Amount: {2:C2}
Total Invoice Amount Used for Calculation: {3:C2}
Status: {4}

Charge Code: {5}
Environment: {6}
Provider: {7}

WARNING: {8}

{9}",
				salesTaxAmountCaptionText,
				salesTaxResult.TotalSalesTaxAmount,
				currentSalesTaxAmount,
				salesTaxResult.TotalInvoiceAmountExcludingSalesTax,
				salesTaxResult.SubmissionStatus,
				chargeCode,
				salesTaxEnvironment,
				calculator.Name,
				salesTaxResult.WarningMessage,
				calculator.MenuItemTroubleshootingHint
			);
			var messageWithoutOptionalParts = messageText
				.Replace("WARNING: \r\n\r\n", string.Empty)
				.Replace("Status: \r\n", string.Empty)
				.Trim();
			var icon = string.IsNullOrEmpty(salesTaxResult.WarningMessage) ? ZMessageBoxIcon.Information : ZMessageBoxIcon.Warning;
			Globals.Message.Show(messageWithoutOptionalParts, messageCaption, ZMessageBoxButtons.OK, icon);
		}

		#endregion

		void BulkChargeImportButton_Click(object sender, EventArgs e)
		{
			if (CheckIfChargeOrConsolCostImportAvailable())
			{
				InvoicingBaseBulkChargeImportForm form = new InvoicingBaseBulkChargeImportForm(new InvoicingBaseBulkChargeImporter(Invoice));
				ZFormModaliser.Show(form, this);
			}
		}

		void ApportionChargesButton_Click(object sender, EventArgs e)
		{
			if (CheckIfChargeOrConsolCostImportAvailable())
			{
				APInvoiceConsolCostingForm form = new APInvoiceConsolCostingForm(Invoice.ConsolCosting);
				form.DisplayMode = this.DisplayMode;

				ZFormModaliser.Show(form, this);
			}
		}

		void OverrideBranchAndDepartmentButton_Click(object sender, EventArgs e)
		{
			if (AllowOverridingTransactionsBranchAndDepartment)
			{
				ZFormModaliser.Show(new OverrideTransactionBranchAndDepartmentForm(TransactionWithOverriddenBranchAndDepartmentHelper), this);
			}
			else
			{
				Globals.Message.ShowError(OverrideBranchAndDepartmentSecurityCheckpoint.ErrorMessageForNotAllowed);
			}
		}

		TransactionWithOverriddenBranchAndDepartmentAdaptor TransactionWithOverriddenBranchAndDepartmentHelper
		{
			get { return new TransactionWithOverriddenBranchAndDepartmentAdaptor(Invoice); }
		}

		bool CheckIfChargeOrConsolCostImportAvailable()
		{
			bool result = true;

			var isTaxBranchApplicable = AccountingMasterFilesUtils.IsTaxBranchApplicableForTransaction(Invoice);

			if (Invoice == null)
			{
				result = false;
			}
			else if (!Invoice.AH_OH.IsValid || Invoice.AH_RX_NKTransactionCurrency.IsEmpty || (isTaxBranchApplicable && !Invoice.AH_GB_TaxBranch.IsValid) || (Invoice.AH_ExchangeRate.IsEmpty &&
				(!Invoice.AH_PostedToEFT || Env.CurrentCompany.ExchangeRate.TodaysRate(Invoice.AH_RX_NKTransactionCurrency, ExchangeRateType.Buy) == 0M)))
			{
				result = false;

				var errorMessageForPostToEFT = isTaxBranchApplicable
					? Res.GetString("D540CE25-596D-4C59-B0F1-19D4ADC7508F", @"When ""Use Job Exchange Rate"" is ticked the Bulk Charge Import screen can only be accessed when 
- The invoice Creditor, Tax Branch and Currency have been entered; and
- The invoice Currency has a current Buy exchange rate.
Please ensure a Creditor and Transaction Currency have been entered.
Please ensure that the Transaction Currency has a current BUY exchange rate recorded against it.")
					: Res.GetString("37b50692-13e2-4e79-82ca-399fe7516c49", @"When ""Use Job Exchange Rate"" is ticked the Bulk Charge Import screen can only be accessed when 
- The invoice Creditor and Currency have been entered; and
- The invoice Currency has a current Buy exchange rate.
Please ensure a Creditor and Transaction Currency have been entered.
Please ensure that the Transaction Currency has a current BUY exchange rate recorded against it.");

				var errorMessageForNotPostToEFT = isTaxBranchApplicable
					? Res.GetString("01AB0ECD-342E-4FEC-991A-6FEC7821D8EC", @"To access this function, you must fill all of the following fields:

- Creditor
- Currency
- Exchange Rate
- Tax Branch")
					: Res.GetString("b81920a6-b486-4e79-b836-c2d05d3d20cd", @"To access this function, you must fill all of the following fields:

- Creditor
- Currency
- Exchange Rate");

				Globals.Message.ShowError(Invoice.AH_PostedToEFT ? errorMessageForPostToEFT : errorMessageForNotPostToEFT);
			}

			return result;
		}

		void InvoiceForm_ApportionedInvoiceLineModified(InvoicingLineBase sender, EventArgs e)
		{
			DialogResult shouldShowApportionment = Globals.Message.Show(Res.GetString("0bcd40a4-745c-4081-91a0-05ec7a42196d", "Apportioned lines cannot be modified manually. The apportionment must be edited to change these values.\r\nWould you like to modify the apportionment now?"), Res.GetString("2c9934fd-d46d-434c-bd37-a718372d1096", "Modify Apportioned Line"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (shouldShowApportionment == DialogResult.Yes)
			{
				HandleEditApportionment(sender, e);
			}
		}

		void BaseInvoicingForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);

			if (e.ToMode == ODisplayMode.ReadOnly)
			{
				IsFormOpenedAsReadOnly = true;
			}
		}

		protected void HandleEditApportionment(object sender, EventArgs e)
		{
			var currentLine = InvoiceDetails.TransactionLinesGrid.ListManager.GetCurrent() as InvoicingLineBase;

			if (currentLine != null)
			{
				if (currentLine.AL_AC.IsValid)
				{
					if (currentLine.ApportionmentChargeImportedFrom != null)
					{
						ZGuid costPK = currentLine.ApportionmentChargeImportedFrom.JR_E6;
						JobConsolCost cost = (JobConsolCost)Invoice.ConsolCosting.ConsolCosts.FindByPK(costPK);
						if (cost == null)
						{
							var message = GenerateDeveloperMessage(costPK, currentLine, (NoResString)"Cannot find the consol cost.");    // Developer Notification Key not shown to client
							ErrorReporter.ReportOnce("BaseInvoicingForm.HandleEditApportionment|APInvoiceConsolCost", message, null);
							Globals.Message.ShowError(Res.GetString("e1ebfdf0-3ef4-4cb8-b411-1f518585f305", "The consol cost cannot be found"));
						}
						else
						{
							if (cost.ParentAPInvoice == null)
							{
								var message = GenerateDeveloperMessage(costPK, currentLine, (NoResString)"ParentAPInvoice is null.");    // Developer Notification Key not shown to client
								ErrorReporter.ReportOnce("BaseInvoicingForm.HandleEditApportionment|ParentAPInvoice", message, null);

								cost.ParentAPInvoice = Invoice;
							}
							APInvoiceConsolCostingSingleEditForm editCostForm = new APInvoiceConsolCostingSingleEditForm(cost);
							editCostForm.FormClosed += EditCostForm_FormClosed;
							ZFormModaliser.Show(editCostForm, this);
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("6ef38c3b-e90f-4f28-8c56-183a9e1fb0a7", "This line is not part of an apportionment of cost."), EditApportionmentMenuItemName);
					}
				}
				else
				{
					if (CheckIfChargeOrConsolCostImportAvailable())
					{
						APInvoiceConsolCostingForm form = new APInvoiceConsolCostingForm(Invoice.ConsolCosting);
						form.DisplayMode = this.DisplayMode;
						ZFormModaliser.Show(form, this);
					}
				}
			}
		}

		string GenerateDeveloperMessage(ZGuid costPK, InvoicingLineBase currentLine, string title)
		{
			var message = new ZStringBuilder();
			message.Append(title);
			message.AppendFormat((NoResString)"Consol cost PK: '{0}'", costPK.ToString());    // Developer Notification Message not shown to client
			message.Append((NoResString)"Current line:");    // Developer Notification Message not shown to client
			message.Append(currentLine.GetTransactionLineInfo());
			message.Append((NoResString)"Related charge:");    // Developer Notification Message not shown to client
			message.Append(currentLine.ApportionmentChargeImportedFrom.GetJobChargeInfo());
			var relatedCost = Invoice.Factory.Load<JobConsolCost>(costPK);
			if (relatedCost != null)
			{
				message.Append((NoResString)"Related Consol Cost:");    // Developer Notification Message not shown to client
				message.Append(((IJobConsolCost)relatedCost).GetJobConsolCostInfo());
			}

			return message.ToStringWithNewLineBetweenAppends();
		}

		void EditCostForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			var senderForm = sender as APInvoiceConsolCostingSingleEditForm;
			try
			{
				if (senderForm != null && senderForm.FormResult == ConsolCostingSingleEditResult.Apportion)
				{
					using (Invoice.GetReportingDeletedApportionmentChargesSuspender())
					{
						Invoice.ImportSingleCostAndRevalidateLines((JobConsolCost)senderForm.BusinessEntity, null);
					}
				}
			}
			finally
			{
				if (BusinessEntity.Factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost))
				{
					BusinessEntity.Factory.RemoveContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost);
				}

				if (BusinessEntity.Factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost))
				{
					ErrorReporter.ReportOnce("OSTaxAmountModifiedFromCalculatedAmountForConsolCost", "OSTaxAmountModifiedFromCalculatedAmountForConsolCost context should have been removed.");
				}

				if (BusinessEntity.Factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice))
				{
					var stackTraces = new StringBuilder();
					foreach (InvoicingLineBase line in Invoice.Lines)
					{
						foreach (var stackTrace in line.ApportionedLineModifiedStackTrace)
						{
							stackTraces.AppendLine(Res.GetString("cae11bfc-7909-4483-b1c7-01e0d6545bd5", "Line PK:{0}{1}{2}", line.PK, System.Environment.NewLine, stackTrace.ToString()));
						}	
					}
					ErrorReporter.ReportOnce("ModifyingConsolCostDetailsFromAPInvoice", "ModifyingConsolCostDetailsFromAPInvoice context should have been removed already.", new Exception(System.Environment.NewLine + stackTraces));
				}
			}
		}

		void InvoiceForm_ShowJobChargesForImportEvent(object sender, EventArgs e)
		{
			// Workaround for Sequence Number issue caused by uncommitted row in FilteredInvoicingLineBaseCollectionView - change focus to force committing current row.
			InvoicingLineBase invoiceLine = sender as InvoicingLineBase;
			if (Invoice != null && !Invoice.Lines.Contains(invoiceLine))
			{
				var currentControl = this.GetFrontMostActiveControl();
				if (currentControl != null)
				{
					MainTabControl.Focus();
					currentControl.Focus();
				}
			}

			JobChargesImporter chargeImporter = new JobChargesImporter(invoiceLine);
			if (chargeImporter.JobChargesCollection.Count > 0)
			{
				ZFormModaliser.ShowDialogAndDispose(NewJobChargesPopupForm(chargeImporter));
			}

			Invoice.Lines.RefreshBinding();
		}

		IDisposable reportingDeletedApportionmentChargesSuspender;

		void TransactionLinesGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			void selectTabForFocusedCell()
			{
				if (!IsDisposed)
				{
					var grid = InvoiceDetails.TransactionLinesGrid;
					var focusedCell = grid.CurrentCell;
					var focusedCellName = grid.TableStyles[0].GridColumnStyles[focusedCell.ColumnNumber]?.MappingName;

					var provider = TabPageFocusProviders.FirstOrDefault(x => x.IsNeedFocus(focusedCellName));
					var tabToSelect = provider == null ? LineChargesTabPage : provider.TabPageToFocus;

					if (tabToSelect != ChargesAndApportionmentsTabControl.SelectedTab)
					{
						using (grid.SuspendCancelOfNonEditedRowOnLeaving())
						{
							ChargesAndApportionmentsTabControl.SelectedTab = tabToSelect;
							grid.Focus();
							grid.CurrentCell = focusedCell;
						}
					}
				}
			}

			BeginInvoke(new MethodInvoker(selectTabForFocusedCell));
		}

		IEnumerable<TabPageFocusProvider> TabPageFocusProviders
		{
			get
			{
				if (tabPageFocusProviders == null)
				{
					tabPageFocusProviders = new List<TabPageFocusProvider>()
				{
					new PeriodApportionmentTabPageFocusProvider(PeriodApportionmentTabPage),
					new SubAccountsTabPageFocusProvider(SubAccountsTabPage)
				};
				}
				return tabPageFocusProviders;
			}
		}
		List<TabPageFocusProvider> tabPageFocusProviders;

		void TransactionLinesGrid_RowDeleted(object sender, RowsDeletingEventArgs e)
		{
			reportingDeletedApportionmentChargesSuspender?.Dispose();
			reportingDeletedApportionmentChargesSuspender = null;
		}

		void TransactionLinesGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			if (Invoice != null)
			{
				var apportionmentIDs = new HashSet<ZGuid>();
				foreach (InvoicingLineBase line in e.Objects)
				{
					if (line.ApportionmentChargeImportedFrom != null)
					{
						apportionmentIDs.Add(line.ApportionmentChargeImportedFrom.JR_E6);
					}
				}

				bool otherLinesExistForApportionmentIDs = false;
				bool isApportionmentTransaction = false;
				foreach (ZGuid appID in apportionmentIDs)
				{
					var cost = (JobConsolCost)Invoice.ConsolCosting.ConsolCosts.FindByPK(appID);
					if (cost != null)
					{
						var lines = Invoice.GetLinesLinkedToConsolCost(cost);
						if (lines.Count > 0)
						{
							isApportionmentTransaction = true;
							if (lines.Except(e.Objects).Any())
							{
								otherLinesExistForApportionmentIDs = true;
								break;
							}
						}
					}
				}

				if (otherLinesExistForApportionmentIDs)
				{
					reportingDeletedApportionmentChargesSuspender = Invoice.GetReportingDeletedApportionmentChargesSuspender();
					DialogResult result = Globals.Message.Show(Res.GetString("4f1475e8-bb65-4718-8dc4-3006631f8a2c", "This line relates to an apportionment of cost. All lines relating to this apportionment must also be deleted. Would you like to proceed?"), Res.GetString("6d2653c3-b91a-471b-9c36-78e142bdb397", "Delete AP Invoice Line"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, DialogResult.No);
					if (result == DialogResult.No)
					{
						e.Cancel = true;
					}
					else
					{
						try
						{
							foreach (ZGuid appID in apportionmentIDs)
							{
								List<InvoicingLineBase> linesToExclude = new List<InvoicingLineBase>();
								foreach (InvoicingLineBase invoicingLine in e.Objects)
								{
									linesToExclude.Add(invoicingLine);
								}
								Invoice.RemoveAllLinesRelatingToConsolCost(appID, linesToExclude);
#if DEBUG
								if (Globals.IsTest)
								{
									AfterRowDelete_ForTestOnly();
								}
#endif
							}
						}
						catch (CannotDeleteException ex)
						{
							Globals.Message.ShowError(ex.Message);
							e.Cancel = true;
						}
					}
				}

				if (!e.Cancel && isApportionmentTransaction && !otherLinesExistForApportionmentIDs)
				{
					reportingDeletedApportionmentChargesSuspender = Invoice.GetReportingDeletedApportionmentChargesSuspender();
				}
			}
		}

#if DEBUG
		internal bool ThrowExceptionOnRowDelete_ForTestOnly;

		void AfterRowDelete_ForTestOnly()
		{
			if (ThrowExceptionOnRowDelete_ForTestOnly)
			{
				throw new CannotDeleteException("Row Delete Error");
			}
		}
#endif

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (InvoiceDetails != null)
			{
				var shouldEnable = !savedSuccessfully || IsINTransaction || IsINTransactionWithApprovalRequest;
				GUIComponentsStateRestorer.DisableControlIfApplicableAndUpdateCurrentState(InvoiceDetails.ApportionChargesButton, shouldEnable);
			}
		}

		void TransactionLinesGridContextMenu_Popup(object sender, EventArgs e)
		{
			if ((BusinessEntity is APInvoice || BusinessEntity is APCreditNote) && !ShouldEnableApportionChargesButton &&
				InvoiceDetails.TransactionLinesGrid.ContextMenu != null && InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems != null)
			{
				if (EditApportionmentMenuItem != null)
				{
					EditApportionmentMenuItem.Enabled = false;
				}
			}
		}

		void RelatedInvoicesGridContextMenu_Popup(object sender, EventArgs e)
		{
			RemoveDocumentsMenu();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal string constant")]
		void RemoveDocumentsMenu()
		{
			foreach (MenuItem menuItem in RelatedInvoicesGrid.ContextMenu.MenuItems)
			{
				if (menuItem.Text == "Documents")
				{
					RelatedInvoicesGrid.ContextMenu.MenuItems.Remove(menuItem);
					break;
				}
			}
		}

		void AH_RX_NKTransactionCurrencyInfo_ValueChanged(object sender, EventArgs e)
		{
			var isForeignCurrencyInvoice = Invoice.AH_RX_NKTransactionCurrency != Invoice.AH_Calc_LocalRXCode;
			AH_LocalTaxAmountCalcEdit.Visible = isForeignCurrencyInvoice && (Invoice.IsTaxed || GlbCompany.CurrentCompany.GC_IsGSTRegistered);
			LocalSubTotalExTaxAmountCalcEdit.Visible = isForeignCurrencyInvoice && (Invoice.IsTaxed || GlbCompany.CurrentCompany.GC_IsGSTRegistered);
			AH_LocalSubTotalAmountCalcEdit.Visible = isForeignCurrencyInvoice && ShowOtherTaxesInInvoiceTotals;
			AH_LocalOtherTaxesAmountCalcEdit.Visible = isForeignCurrencyInvoice && ShowOtherTaxesInInvoiceTotals;
			if (AH_LocalTaxAmountCalcEdit.Visible)
			{
				AH_LocalTaxAmountCalcEdit.CaptionResourceString = AH_LocalTaxAmountCalcEdit.CaptionResourceString.Format(Invoice.Company.ConsumptionTaxDescriptionForCompanyForm);
			}
			AH_LocalTotalAmountCalcEdit.Visible = isForeignCurrencyInvoice;
			AH_LocalWHTAmountCalcEdit.Visible = isForeignCurrencyInvoice;
			AH_LocalExtraTaxAmountCalcEdit.Visible = isForeignCurrencyInvoice;
			if (AH_LocalExtraTaxAmountCalcEdit.Visible)
			{
				AH_LocalExtraTaxAmountCalcEdit.CaptionResourceString = AH_LocalExtraTaxAmountCalcEdit.CaptionResourceString.Format(AH_OSExtraTaxAmountCalcEdit.CaptionResourceString.ShortCaption);
			}
			InvoiceDetails.SetOverrideExchRateCheckBoxPositionAndVisible(isForeignCurrencyInvoice, Invoice.IsSettingLineExchangeRateSupported);
		}

		void ValidateExpectedInvoiceTotalInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Invoice.AH_Ledger == LedgerTypes.AccountsPayable ||
				Invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
				Invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				UnallocatedTabPage.TabVisible = Invoice.ValidateExpectedInvoiceTotal;
			}
		}

		void Invoice_OnJobChanged(object sender, ChangedBizoEventArgs e)
		{
			InvoicingLineBase line = sender as InvoicingLineBase;
			if (line != null)
			{
				Job job = (Job)e.NewBusinessObject;
				job.ShouldUseImmediateRevenueRecognisedDate += job_ShouldUseImmediateRevenueRecognisedDate;
				try
				{
					job.AskShouldUseImmediateRevenueRecognisedDate(line.ChargeCode);
				}
				finally
				{
					job.ShouldUseImmediateRevenueRecognisedDate -= job_ShouldUseImmediateRevenueRecognisedDate;
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code added for debugging")]
		void job_ShouldUseImmediateRevenueRecognisedDate(object sender, UserQueryEventArgs e)
		{
			e.Response = Globals.Message.Show(e.QueryMessage, "Job " + ((Job)sender).JH_JobNum, MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes;
		}

		#region Tax Framework

		void CalculateTaxTransactionsButton_Click(object sender, EventArgs e)
		{
			var invoicingBase = (InvoicingBase)BusinessEntity;
			using (invoicingBase.SuspendExpectedInvoiceTotalValidation)
			{
				BusinessEntity.MarkAsNeedingValidationIncludingChildren();
				BusinessEntity.RunPreSaveValidation();
			}
			if (BusinessEntity.HasErrors())
			{
				using (var form = new ZErrorMessageBox(BusinessEntity, Res.GetString("5A37F68D-E4EA-43A2-BADF-7703E631946C", "operation"), Res.GetString("b48329d8-e65c-4bb7-90b2-5c0710f75719", "perform"), Res.GetString("a28d06f7-7f7c-42c3-a250-0901158cdf28", "performed")))
				{
					ZFormModaliser.ShowMessageBoxWithoutDispose(form);
				}
			}
			else
			{
				var errorMessage = string.Empty;
				using (this.InvoiceDetails.SuspendTaxRecordsGridTaxTransactionControl())
				{
					errorMessage = ObjectFactory.Get<ITaxProcessor>().ProcessTaxesOnPosting(TaxRecordParent);
				}

				if (!string.IsNullOrEmpty(errorMessage))
				{
					Globals.Message.ShowError(errorMessage);
				}
			}
		}

		void OnOtherTaxesCalculatedBeforePosting_Changed(object sender, EventArgs e)
		{
			UpdateFormControlsWhenTaxTransactionsCalculationChanges(isFormInitialization: false);
		}

		void UpdateFormControlsWhenTaxTransactionsCalculationChanges(bool isFormInitialization)
		{
			var taxRecordParent_IsTaxTransactionsCalculatedBeforePosting = TaxRecordParent.IsTaxTransactionsCalculatedBeforePosting;

			if (isFormInitialization && !taxRecordParent_IsTaxTransactionsCalculatedBeforePosting)
			{
				return;
			}

			CalculateTaxTransactionsButton.Enabled = !taxRecordParent_IsTaxTransactionsCalculatedBeforePosting;

			UpdateEditableIncludingChildrenExceptLineChargesGrid(taxRecordParent_IsTaxTransactionsCalculatedBeforePosting);

			if (taxRecordParent_IsTaxTransactionsCalculatedBeforePosting)
			{
				GUIComponentsStateRestorer.DisableControlsAndMenuItems(ButtonsToDisableAfterOtherTaxesCalculated, MenuItemsToDisableAfterOtherTaxesCalculated);
			}
			else
			{
				GUIComponentsStateRestorer.RestoreControlsAndMenuItems(ButtonsToDisableAfterOtherTaxesCalculated, MenuItemsToDisableAfterOtherTaxesCalculated);
			}

			if (DisplayMode != ODisplayMode.Delete || !IsINTransaction)
			{
				InvoicingBaseTaxFrameworkViewModel.TrackHasChanges(taxRecordParent_IsTaxTransactionsCalculatedBeforePosting);
			}

			if (taxRecordParent_IsTaxTransactionsCalculatedBeforePosting)
			{
				InvoiceDetails.ActivateOtherTaxesTab();
			}
		}

		void UpdateEditableIncludingChildrenExceptLineChargesGrid(bool taxRecordParent_IsTaxTransactionsCalculatedBeforePosting)
		{
			InvoiceDetailsTabPage.UpdateEditableIncludingChildren(!taxRecordParent_IsTaxTransactionsCalculatedBeforePosting, ControlsAllowedToRemainEditableAfterOtherTaxesCalculated);
			LineChargesGrid.ReadOnly = true;
		}

		string[] ControlsAllowedToRemainEditableAfterOtherTaxesCalculated => InvoiceDetails.ControlsAllowedToRemainEditableAfterOtherTaxesCalculated;

		ZButton[] ButtonsToDisableAfterOtherTaxesCalculated => new[] { InvoiceDetails.BulkChargeImportButton, InvoiceDetails.ApportionChargesButton };
		MenuItem[] MenuItemsToDisableAfterOtherTaxesCalculated => new[] { EditApportionmentMenuItem, OverrideBranchAndDepartmentMenuItem };

		IGUIComponentsStateRestorer GUIComponentsStateRestorer => guiComponentsStateRestorer_constructorInitializedOnly;
		IGUIComponentsStateRestorer guiComponentsStateRestorer_constructorInitializedOnly;

#if DEBUG
		public void SubstituteGUIComponentsStateRestorer_ForTestOnly(IGUIComponentsStateRestorer replacement) => guiComponentsStateRestorer_constructorInitializedOnly = replacement;
		public IGUIComponentsStateRestorer GUIComponentsStateRestorer_ExposedForTestOnly => guiComponentsStateRestorer_constructorInitializedOnly;
#endif

		#endregion

		#endregion

		#region IDataGridLayoutIdentifierRoot Members

		string IDataGridLayoutIdentifierRoot.ID
		{
			get
			{
				ZForm form = FindForm() as ZForm;

				string result = string.Empty;

				if (form != null)
				{
					result = form.Name;

					if (form.BusinessEntity != null)
					{
						result += form.BusinessEntity.GetType().Name;
					}
				}

				return result;
			}
		}

		#endregion
	}
}
