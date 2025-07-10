using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class APTransactionModuleStrip : TransactionModuleStrip, IDocumentBusinessContext
	{
		public APTransactionModuleStrip() : this(CreateWithholdingDependecies())
		{ }

		public APTransactionModuleStrip
			(
				(IWithholdingJournalCreatorForMultipleInvoices withholdingJournalCreatorForMultipleInvoices, IWithholdingJournalRealizerForMultipleInvoices withholdingJournalRealizerForMultipleInvoices) withholdingJournaldependecies
			)
		{
			Argument.NotNull(withholdingJournaldependecies, nameof(withholdingJournaldependecies));

			WithholdingJournalCreatorForMultipleInvoices = Argument.NotNull(withholdingJournaldependecies.withholdingJournalCreatorForMultipleInvoices, nameof(withholdingJournaldependecies.withholdingJournalCreatorForMultipleInvoices));
			WithholdingJournalRealizerForMultipleInvoices = Argument.NotNull(withholdingJournaldependecies.withholdingJournalRealizerForMultipleInvoices, nameof(withholdingJournaldependecies.withholdingJournalRealizerForMultipleInvoices));
		}

		public IWithholdingJournalRealizerForMultipleInvoices WithholdingJournalRealizerForMultipleInvoices { get; }
		public IWithholdingJournalCreatorForMultipleInvoices WithholdingJournalCreatorForMultipleInvoices { get; }

		protected static (IWithholdingJournalCreatorForMultipleInvoices withholdingJournalCreatorForMultipleInvoices, IWithholdingJournalRealizerForMultipleInvoices withholdingJournalRealizerForMultipleInvoices) CreateWithholdingDependecies()
		{
			var withholdingJournalCreationManager = new WithholdingJournalCreationManager();

			return
				(
					new WithholdingJournalCreatorForMultipleInvoices(new WithholdingJournalCreatorForSingleInvoice(withholdingJournalCreationManager)),
					new WithholdingJournalRealizerForMultipleInvoices(new WithholdingJournalRealizerForSingleInvoice(withholdingJournalCreationManager))
				);
		}

		#region Constants

		internal static string SettlementGroupWithMultiReferenceMessageHeader => Res.GetString("62e1922b-ef8c-4b0c-96e5-017b233f3df9", "The following organizations have wrong Settlement Group setup.");

		internal static string SettlementGroupWithMultiReferenceMessageEnd => Res.GetString("a5d637b9-e029-4191-8814-8e89e2ba506d", @"These organizations should not have Settlement Group set to another organization as they are Settlement Groups by themselves.
You may change the Settlement Group for these Organizations by accessing the Organization Setup screen.");

		internal static string CannotPrintAsASelfBilledTransactionMessage => Res.GetString("0dfa6732-cc44-4da1-9ba3-d0e9b2ccbbfb", "This transaction was not posted as a self billing transaction. Please print the Cost Confirmation Document instead.");

		protected static MultilingualString ReprintMenuItemText => ResString.GetMultilingualString("b6b0fdcd-3ac3-4ddc-9ff9-09f91b4523c7", "Reprint Check/s");

		#endregion

		#region Override

		protected override ControllerID ExpectedOpenedMatchFormID
		{
			get { return ControllerIDs.ZAPMatching; }
		}

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.APInvoiceCode;

		public override ModuleIdentifier ID => ModuleIDs.APTransaction;

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var aPTransactionHeaders = new APTransactionHeaderCollection(Factory);
			return new FilteredTransactionHeaderCollectionView(aPTransactionHeaders);
		}

		protected override void PushItemsIntoCollectionCore(IBusinessObjectCollection collection, PerformSearchResult searchResult, SortInfo sort)
		{
			var filteredCollection = collection as FilteredTransactionHeaderCollectionView;
			IBusinessObjectCollection originalCollection;

			if (filteredCollection == null)
			{
				originalCollection = collection;
			}
			else
			{
				filteredCollection.CollectionToFilter.Factory.ClearQueryCache();
				originalCollection = filteredCollection.CollectionToFilter;
			}
			base.PushItemsIntoCollectionCore(originalCollection, searchResult, sort);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new APTransactionFilterStripBusinessObject();
		}

		protected override bool CanAddAuditAndCashMenus => true;

		protected override bool CanCreateComplianceDocuemntsMenus => AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value;

		protected override string Ledger => LedgerTypes.AccountsPayable;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.PayablesTransactions;

		protected override SecurityCheckpoint MarkAsNotPrintedSecurityItem => Env.Security.PayablesMarkInvoiceCreditAsNotPrinted;

		protected override SecurityCheckpoint ImportRemittanceFileSecurityCheckpoint => Env.Security.ImportRemittanceFilePayables;

		protected override SecurityCheckpoint AllocateComplianceNumberCheckpoint => Env.Security.PayablesAllocateComplianceNumber;

		protected override SecurityCheckpoint UpdateComplianceSubTypeOrNumberCheckpoint => Env.Security.PayablesModifyComplianceSubTypeOrNumber;

		protected override SecurityCheckpoint ModifyTransactionDescriptionsSecurity => Env.Security.APOverrideTransactionDescription;

		protected override SecurityCheckpoint AuditSecurityCheckpoint => Env.Security.PayablesAuditTransaction;

		protected override SecurityCheckpoint UndoAuditSecurityCheckpoint => Env.Security.PayablesUndoAuditTransaction;

		protected override SecurityCheckpoint RecordCashierSecurityCheckpoint => Env.Security.PayablesRecordCashier;

		protected override SecurityCheckpoint ClearCashierSecurityCheckpoint => Env.Security.PayablesClearCashier;

		protected override SecurityCheckpoint ModifyMatchStatusAndReasonSecurity => Env.Security.PayablesModifyMatchStatusAndReason;

		protected SecurityCheckpoint ModifyGovernmentAllocatedNumberSecurity => Env.Security.PayablesModifyGovernmentAllocatedNumber;

		protected MultilingualString OverrideGovernmentAllocatedNumberMenuText => ResString.GetMultilingualString("D5613491-FC8E-45F5-AD26-D8C763E398E9", "Override Government Allocated Number");

		protected override IFilterControl GetNewFilterControl()
		{
			TransactionFilterStripControl controller = new TransactionFilterStripControl(GridCollection, (APTransactionFilterStripBusinessObject)FilterBusinessObject);
			controller.FilteredGrid.ColorContextKey = "APTransactionFilterStripControl";
			return controller;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return selectedBusinessObject != null ? base.GetNewController(selectedBusinessObject) : ZControllerFactory.Create(ControllerIDs.APInvoice);
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
			{
				AddImportDataMenuItem(ResString.GetMultilingualString("50e0e22e-b6fc-4512-b86d-baaec11db85d", "NZ Customs CSV Transaction"), new EventHandler(ImportNZCustomsAPInvoiceEventHandler));
			}
		}

		protected override void DisplayImportedTransactionForm(InvoicingBase importedTransaction)
		{
			InvoicingBaseController controller = null;

			if (importedTransaction.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				switch (importedTransaction.AH_TransactionType)
				{
					case TransactionTypes.Invoice:
						controller = (APInvoiceController)ZControllerFactory.Create(ControllerIDs.APInvoice);
						break;

					case TransactionTypes.CreditNote:
						controller = (APCreditNoteController)ZControllerFactory.Create(ControllerIDs.APCreditNote);
						break;

					case TransactionTypes.AdjustmentNote:
						controller = (APAdjustmentNoteController)ZControllerFactory.Create(ControllerIDs.APAdjustmentNote);
						break;
				}
			}

			if (controller != null)
			{
				ShowImportedDataForm(controller, importedTransaction);
			}
			else
			{
				Globals.Message.Show(Res.GetString("6471f711-8081-494c-82e2-56ce512efc3b", "Only Accounts Payable Invoices, Credit Notes and Adjustment Notes can be imported from this menu"));
			}
		}

		protected override void OnBeforePerformSearchCore()
		{
			ResetRelatedTransactionsAndClaimsOnExistingCollectionEntriesBeforeReload();
		}

		protected override void OnAfterPerformSearchCore()
		{
			if (GridCollection is FilteredTransactionHeaderCollectionView && ((FilteredTransactionHeaderCollectionView)GridCollection).CollectionToFilter.Count > GridCollection.Count)
			{
				Globals.Message.ShowWarning(ViewingRestrictionOutsideLoginWarningMessage, Caption);
			}
			base.OnAfterPerformSearchCore();
		}

		void ResetRelatedTransactionsAndClaimsOnExistingCollectionEntriesBeforeReload()
		{
			var collection = GetTransactionCollection();
			if (collection != null)
			{
				collection.ResetRelatedTransactionsCollection();
				collection.ResetRelatedClaimsCollection();
			}
		}

		protected override void PrintInvoices(TransactionHeader[] headers)
		{
			PrintCostConfirmationDocument(headers);
		}

		protected override SecurityCheckpoint GetSecurityForPrintTransaction(ZString transactionType)
		{
			return Env.Security.PrintPayableTransactions;
		}

		protected virtual void ShowImportedDataForm(InvoicingBaseController controller, InvoicingBase importedTransaction)
		{
			controller.ShowImportedDataForm(importedTransaction);
		}

		protected override MenuItem[] GetActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>
			{
				new ZMenuItem("-"),
				new ZMenuItem(ResString.GetMultilingualString("Accounting.APTransaction.PayInvoices", "Pay Invoices"), new EventHandler(HandlePayInvoices)),
				new ZMenuItem(ResString.GetMultilingualString("Accounting.APTransaction.BulkAPInvoicePosting", "Bulk AP Invoice Posting"), new EventHandler(HandleBulkAPInvoicePosting))
			};
			ZFormMenuStrategy.AddInterfaceConnectorMenuItem(menuItems, new ZMenuItem(ResString.GetMultilingualString("Accounting.APTransaction.ExportRemittance", "Export Remittance To XML"), new EventHandler(HandleExportRemittanceInfo)));
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.APTransaction.EditRequisition", "Edit Requisition Date and Status"), new EventHandler(HandleEditRequisition)));
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.APTransaction.ReallocateCheckNumber", "Re-Allocate Check Number"), new EventHandler(HandleReallocateCheckNumber)));

			if (GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled())
			{
				menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.APTransaction.RealisePaymentsBasisWithholdingTax", "Realize Payments Basis Withholding Tax"), new EventHandler(HandleWHTRealization)));
			}

			return menuItems.ToArray();
		}

		protected override MenuItem[] GetEInvoicingActionMenuItems()
		{
			return EInvoicingGUIActionHelper.GetActionMenuItems(isAPTransaction: true);
		}

		EInvoicingGUIActionHelper EInvoicingGUIActionHelper
		{
			get => eInvoicingGUIActionHelper ?? (eInvoicingGUIActionHelper = new EInvoicingGUIActionHelper(() => SelectedBusinessObjects.OfType<TransactionHeader>()));
		}
		EInvoicingGUIActionHelper eInvoicingGUIActionHelper;

		protected override MenuItem[] GetOverrideDetailsMenuItems()
		{
			var menuItems = new List<MenuItem>();
			menuItems.AddRange(base.GetOverrideDetailsMenuItems());
			menuItems.Add(new ZMenuItem(OverrideInvoiceReferencesMenuText, new EventHandler(HandleOverrideInvoiceReference)));
			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.Value)
			{
				menuItems.Add(new ZMenuItem(OverrideGovernmentAllocatedNumberMenuText, new EventHandler(HandleOverrideGovernmentAllocatedNumber)));
			}
			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			PrintMenuItem.MenuItems.Add(new ZMenuItem("-"));
			PrintSelfBillingInvoiceMenuItem = new ZMenuItem(PrintSelfBillingInvoiceMenuText, new EventHandler(HandlePrintSelfBillingInvoice));
			PrintMenuItem.MenuItems.Add(PrintSelfBillingInvoiceMenuItem);
			PrintMenuItem.MenuItems.Add(new ZMenuItem(ReprintMenuItemText, HandleReprint));
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Italy)
			{
				PrintMenuItem.MenuItems.Add(new ZMenuItem(PrintAutofatturaInvoiceMenuText, new EventHandler(HandlePrintAutofatturaInvoice)));
			}
			PrintMenuItem.Popup += new EventHandler(printMenuItem_Popup);
			return result.ToArray();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			NewMenuItem.MenuItems.Add(new ZMenuItem("-"));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewIntercompanyCostsApportionmentMenuText, new EventHandler(HandleNewIntercompanyCostsApportionment)));

			return menuItems.ToArray();
		}

		protected MultilingualString NewIntercompanyCostsApportionmentMenuText => f_NewIntercompanyCostsApportionmentMenuText ?? (f_NewIntercompanyCostsApportionmentMenuText = ResString.GetMultilingualString("8e8867db-f277-4a14-bfbc-657f6c417997", "New Overhead Costs &Apportionment"));
		MultilingualString f_NewIntercompanyCostsApportionmentMenuText;

		#region Overrides for Matching

		protected override SecurityCheckpoint MatchCheckpoint => Env.Security.MatchPayablesTransactions;

		protected override MatchingBase GetMatchingObject(BusinessObjectFactory factory)
		{
			return new APMatchingBase(factory);
		}

		protected override MatchingModule CreateMatchingModuleForViewMatchedTransactions()
			=> new APMatchingModule();

		#endregion

		#endregion

		#region Event Handlers

		protected void HandleOverrideInvoiceReference(object sender, EventArgs e)
		{
			if (Env.Security.PayablesModifyInvoiceRemittanceReference.IsAllowed || Env.Security.PayablesModifyInvoiceDateNumOrSupplierCostRef.IsAllowed)
			{
				if (CurrentBusinessObjectInGrid != null)
				{
					bool canReferenceBeChanged = true;
					foreach (TransactionHeader bizo in SelectedBusinessObjects)
					{
						if (!(bizo is InvoicingBase))
						{
							canReferenceBeChanged = false;
							break;
						}
					}
					if (!canReferenceBeChanged)
					{
						Globals.Message.ShowInformation(Res.GetString("0BAD3CAD-CE72-47B4-9F24-485DD537A280", "You can only override invoice remittance reference for AP Invoices, Credit and Adjustment Notes."));
					}
					else
					{
						var helper = new OverrideInvoiceReferenceHelper(new BusinessObjectFactory(), SelectedBusinessObjects.Select(bizo => bizo.PK).ToArray());
						ZFormModaliser.Show(new OverrideInvoiceReferenceForm(helper), Grid.FindForm());
					}
				}
				else
				{
					ShowNoSelectedMessage();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("04DF9D0F-7B29-4FA0-82BA-9DFD91EA28CE", "{0}\r\n\r\n{1}\r\nand/or\r\n{2}",
					SecurityCore.SecurityErrorMessage,
					Env.Security.PayablesModifyInvoiceRemittanceReference.DisplayTextPathToSecurityRight,
					Env.Security.PayablesModifyInvoiceDateNumOrSupplierCostRef.DisplayTextPathToSecurityRight));
			}
		}

		void HandleOverrideGovernmentAllocatedNumber(object sender, EventArgs e)
		{
			if (ModifyGovernmentAllocatedNumberSecurity.IsAllowed)
			{
				if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
				{
					foreach (var element in Grid.SelectedElements)
					{
						if (element is TransactionHeader transactionHeader && (transactionHeader.AH_TransactionType != TransactionTypes.Invoice &&
																				transactionHeader.AH_TransactionType != TransactionTypes.CreditNote &&
																				transactionHeader.AH_TransactionType != TransactionTypes.AdjustmentNote))
						{
							Globals.Message.ShowError(Res.GetString("ad103d39-842c-4027-86cf-4d0a18ef70c4", "The 'Override Government Allocated Number' Action menu can only be run for INV, CRD and ADJ transaction types only."));
							return;
						}
					}

					var selectedPKs = Grid.SelectedElements.Select(x => x.PK);

					var helper = new OverrideGovernmentAllocatedNumberHelper(new BusinessObjectFactory(), selectedPKs.ToArray());
					ZFormModaliser.Show(new OverrideGovernmentAllocatedNumberForm(helper), Grid.FindForm());
				}
				else
				{
					ShowNoSelectedMessage();
				}
			}
			else
			{
				ModifyGovernmentAllocatedNumberSecurity.ShowError();
			}
		}

		protected MultilingualString PrintSelfBillingInvoiceMenuText => ResString.GetMultilingualString("faeb764c-9d3b-4936-9264-9cb419889f76", "Print &Self Billing Invoice");
		protected MultilingualString PrintAutofatturaInvoiceMenuText => ResString.GetMultilingualString("6A81775C-12DF-412B-959F-E55F86D923FC", "Print Autofattura (IT)");
		public MultilingualString GetPrintAutofatturaInvoiceMenuText => PrintAutofatturaInvoiceMenuText;

		MenuItem PrintSelfBillingInvoiceMenuItem;

		protected void HandleReprint(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(ChequeNumberReallocator.NoTransactionIsSelectedToPrint);
			}
			else if (!Env.Security.APPaymentProcessingReprint.IsAllowed)
			{
				Env.Security.APPaymentProcessingReprint.ShowError();
			}
			else
			{
				ReallocateCheckNumbers(true);
			}
		}

		protected override SecurityCheckpoint ModifyAddressContactForPostedTransactionsSecurity => Env.Security.PayablesModifyAddressContactForPosted;

		protected override SecurityCheckpoint ModifyCashFlowCategoryForPostedTransactionsSecurity => Env.Security.PayablesModifyCashFlowCategoryForPosted;

		protected override SecurityCheckpoint ModifyAgreedPaymentMethodForPostedTransactionsSecurity => Env.Security.PayablesModifyAgreedPaymentMethodForPosted;

		protected override SecurityCheckpoint ModifyDueDateForPostedTransactionsSecurity => Env.Security.PayablesModifyDueDateForPosted;

		protected void HandleReallocateCheckNumber(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(ChequeNumberReallocator.NoTransactionIsSelectedToReAllocate);
			}
			else if (!Env.Security.APPaymentProcessingReprint.IsAllowed)
			{
				Env.Security.APPaymentProcessingReprint.ShowError();
			}
			else
			{
				ReallocateCheckNumbers(false);
			}
		}

		void ReallocateCheckNumbers(bool withReprint)
		{
			var headerCollection = new List<TransactionHeader>();

			foreach (TransactionHeader businessObject in SelectedBusinessObjects)
			{
				headerCollection.Add(businessObject);
			}

			if (ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(headerCollection))
			{
				AccChequeBook chequeBook = ((APPayment)headerCollection[0]).ChequeBookBizO;
				chequeBook.Reload();
				ChequeNumberReallocator reallocator = new ChequeNumberReallocator(Factory, chequeBook, headerCollection, withReprint);

				StmPrintJob[] printJobs = reallocator.GetExistingPrintJobs();
				if (printJobs.Length > 0)
				{
					if (QueryUserAboutDeletingExistingPrintJobs())
					{
						reallocator.DeletePrintJobs(printJobs);
					}
					else
					{
						return;
					}
				}

				using (ChequeNumberReallocationForm form = new ChequeNumberReallocationForm(reallocator))
				{
					DialogResult result = ZFormModaliser.ShowDialogAndDispose(form);

					if (result == DialogResult.OK)
					{
						ZString errorMessage = reallocator.Process(transactions => new PaymentBatchPrintManager(transactions, null, TransactionTypes.Payment, Factory).Print());
						if (errorMessage != ZString.Empty)
						{
							Globals.Message.ShowError(errorMessage);
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(withReprint ? ChequeNumberReallocator.TransactionCannotBeReprinted : ChequeNumberReallocator.ChequeNumberCannotBeReAllocated);
			}
		}

		bool QueryUserAboutDeletingExistingPrintJobs()
		{
			return Globals.Message.Show(ChequeNumberReallocator.DeleteExistingPrintJobs, ChequeNumberReallocator.DeleteExistingPrintJobsFormCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
		}

		void printMenuItem_Popup(object sender, EventArgs e)
		{
			PrintSelfBillingInvoiceMenuItem.Visible = false;
			TransactionHeader currentHeader = CurrentBusinessObjectInGrid as TransactionHeader;
			if (currentHeader != null)
			{
				if (currentHeader.Header != null && currentHeader.Header.OH_IsActive)
				{
					if (currentHeader.AH_TransactionType == TransactionTypes.Invoice || currentHeader.AH_TransactionType == TransactionTypes.CreditNote)
					{
						PrintSelfBillingInvoiceMenuItem.Visible = true;
					}
				}
			}
		}

		void PrintCostConfirmationDocument(TransactionHeader[] headers)
		{
			InvoicePrintHelper.PrintCostConfirmationDocument(headers);
		}

		void HandleBulkAPInvoicePosting(object sender, EventArgs e)
		{
			ZController postingController = ZControllerFactory.Create(ControllerIDs.APBulkInvoicePosting);
			var form = postingController.ShowNewForm();

#if DEBUG
			if (Globals.IsTest)
			{
				LastShownBulkAPInvoicePostingForm_ForTestOnly = form;
			}
#endif
		}

		void HandleExportRemittanceInfo(object sender, EventArgs e)
		{
			var exporter = new XmlDataTransferExporter(new ARAPPaymentDataAdapter(), true);
			if (Grid.SelectedElements.Length > 0)
			{
				var paymentFactory = new BusinessObjectFactory();
				if (Grid.SelectedElements[0] is Payment)
				{
					var payment = paymentFactory.Load<APPayment>(Grid.SelectedElements[0].PK);
					exporter.PromptUserAndExport(new BusinessObject[] { payment });
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("a7d05bfa-257d-4e14-9a87-7aca28b1c326", "Please select a payment to use this menu item"));
				}
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("1b9c698a-f3af-4256-8117-c95caa27b359", "There are no outstanding invoices, credit notes or adjustment notes to export."));
			}
		}

		protected virtual void HandlePrintSelfBillingInvoice(object sender, EventArgs e)
		{
			base.HandlePrint(sender, e);
			InvoicePrintHelper.PrintSelfBillingInvoice(CurrentBusinessObjectInGrid as TransactionHeader);
		}

		protected virtual void HandlePrintAutofatturaInvoice(object sender, EventArgs e)
		{
			var gridTransactions = Grid.SelectedElements;
			var canPrint = true;
			if (gridTransactions != null && gridTransactions.Length > 0)
			{
				TransactionHeaderCollection selectedTransactions = new TransactionHeaderCollection(Factory);
				foreach (BusinessObject bizo in gridTransactions)
				{
					TransactionHeader transaction = bizo as TransactionHeader;
					if (!((transaction.AH_TransactionType == TransactionTypes.Invoice ||
							transaction.AH_TransactionType == TransactionTypes.CreditNote ||
							transaction.AH_TransactionType == TransactionTypes.AdjustmentNote) &&
							((InvoicingBase)transaction).SupportPrintAutofatturaDocument()))
					{
						var errorMessage = Res.GetString("5903571F-2E08-4D6F-8A29-2000606065E2", "Autofattura (IT) can only be printed for Invoice, Credit Note and Adjustment transactions that have a Compliance Sub Type = APS and allocated Compliance Number");
						Globals.Message.ShowError(errorMessage);
						canPrint = false;
						break;
					}
					selectedTransactions.Add(transaction);
				}
				if (canPrint)
				{
					InvoicePrintHelper.PrintITAutoFattura(selectedTransactions.ToArray<TransactionHeader>());
				}
			}
			else
			{
				Globals.Message.Show(NothingSelectedMessage);
			}
		}

		protected void ImportNZCustomsAPInvoiceEventHandler(object sender, EventArgs e)
		{
			NZCustomsAPInvoiceImporter importer = new NZCustomsAPInvoiceImporter();
			DataImporterBusinessObject importerBusinessObject = new DataImporterBusinessObject(new BusinessObjectFactory());

			using (DataImporterForm importForm = new DataImporterForm(importerBusinessObject, Res.GetString("4659254f-d31b-4e67-ab5e-8c00b791e74e", "Import NZ Customs CSV Transaction"), BillingInterfaceName.APInvoiceNZCustomsImport))
			{
				importer.ImportingSingleTransaction = ZBool.True;
				importer.RunExtraValidation = ZBool.False;
				importForm.Importer = importer;
				ZFormModaliser.ShowDialogWithoutDispose(importForm);
			}

			if (importer.ImportedInvoice != null && !importer.ImportedInvoice.IsDeleted)
			{
				DisplayImportedTransactionForm(importer.ImportedInvoice);
			}
		}

		void HandleNewIntercompanyCostsApportionment(object sender, EventArgs e)
		{
			HandleShowingFormSafely(() => HandleNewIntercompanyCostsApportionmentCore());
		}

		void HandleNewIntercompanyCostsApportionmentCore()
		{
			ZControllerFactory.Create(ControllerIDs.APIntercompanyCostsApportionment).ShowNewForm();
		}

		protected void HandleEditRequisition(object sender, EventArgs e)
		{
			new RequisitionEditHelper().HandleEditRequisition(Grid, Factory, Env.Security.PayablesRequisitionEdit);
		}

		protected void HandleWHTRealization(object sender, EventArgs e)
		{
			var errorMessage = NothingSelectedMessage;
			var areAnyJournalsToRealize = false;
			IReadOnlyCollection<WithholdingJournalsPerInvoice> journalCollectionPerInvoice = null;

			if (SelectedBusinessObjects != null)
			{
				var transactionKPs = SelectedBusinessObjects.OfType<InvoicingBase>().Select(bizo => bizo.PK).ToList();

				if (transactionKPs.Any())
				{
					var factory = new BusinessObjectFactory();
					var apTransactionsInNewFactory = factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, transactionKPs));

					(journalCollectionPerInvoice, errorMessage) = WithholdingJournalCreatorForMultipleInvoices.Create(apTransactionsInNewFactory);

					areAnyJournalsToRealize = journalCollectionPerInvoice.Count > 0;
					if (areAnyJournalsToRealize && !errorMessage.IsNullOrEmpty())
					{
						errorMessage +=
@$"

{Res.GetString("476FB14F-20C1-4ECF-986D-22EC5F110058", "Successfully generated journals will be shown after this message is closed.")}";
					}
				}
			}

			if (!errorMessage.IsNullOrEmpty())
			{
				Globals.Message.Show(errorMessage);
			}
			if (areAnyJournalsToRealize)
			{
				ZFormModaliser.ShowDialogAndDispose(new WithholdingJournalParentPivotForm(journalCollectionPerInvoice, WithholdingJournalRealizerForMultipleInvoices));
			}
		}

		#endregion

		#region Pay Invoices Members

		protected virtual void HandlePayInvoices(object sender, EventArgs e)
		{
			var transactionsWithNotionalWHT = FindTransactionsWithNotionalWHT(Grid.SelectedElements);
			if (transactionsWithNotionalWHT.Any())
			{
				var transactionList = transactionsWithNotionalWHT
									  .OrderBy(transaction => transaction.AH_TransactionNum)
									  .Select(transaction => $"{transaction.AH_TransactionType} {transaction.AH_TransactionNum}");
				var transactionListString = string.Join(", ", transactionList);

				Globals.Message.ShowError(CannotIncludeInvoicesWithNotionalWHTInPaymentBatchMessage + $"\n• {transactionListString}");
				return;
			}

			TransactionHeaderCollection filteredTransactions = FilterSelectedTransactions(Grid.SelectedElements);

			if (filteredTransactions.Count > 0)
			{
				IZForm form = null;
				RunOnMainThread(() => filteredTransactions.GetPKs(), (pks) =>
				{
					var postingController = (APPaymentBatchPostingController)ZControllerFactory.Create(ControllerIDs.APPaymentBatchPosting);
					postingController.SetCollectionForDefaultsAndValidation(new APPaymentBatchPosterCollection(postingController.Factory, pks));
					form = postingController.ShowNewForm();
				});

#if DEBUG
				if (Globals.IsTest)
				{
					LastShownAPPaymentBatchPostingForm_ForTestOnly = (ZForm)form;
				}
#endif
			}
		}

		IEnumerable<InvoicingBase> FindTransactionsWithNotionalWHT(BusinessObject[] gridTransactions)
		{
			return gridTransactions
				.Where(transaction => transaction is InvoicingBase inv
								   && inv.AH_NotionalWHTTax != 0)
				.Cast<InvoicingBase>();
		}

		TransactionHeaderCollection FilterSelectedTransactions(BusinessObject[] gridTransactions)
		{
			TransactionHeaderCollection selectedTransactions = new TransactionHeaderCollection(Factory);
			if (gridTransactions != null && gridTransactions.Length > 0)
			{
				ZBool needToCheckForSettlementGroups = ZBool.False;
				foreach (BusinessObject transaction in gridTransactions)
				{
					if (transaction is TransactionHeader && ((TransactionHeader)transaction).AH_TransactionType != ZArchitecture.Core.TransactionTypes.InvoiceBatch)
					{
						if (((TransactionHeader)transaction).AH_FullyPaidDate.IsEmpty)
						{
							selectedTransactions.Add(transaction);
							if (((TransactionHeader)transaction).Header.APSettlementGroupPK.IsValid)
							{
								needToCheckForSettlementGroups = ZBool.True;
							}
						}
					}
				}
				if (selectedTransactions.Count > 0)
				{
					if (needToCheckForSettlementGroups)
					{
						CheckCollectionContainAtleastOneTransactionForEachSettlementGroup(selectedTransactions);
					}
				}
				else
				{
					Globals.Message.Show(NothingToPostAmongSelectedTransactionsMessage);
				}
			}
			else
			{
				Globals.Message.Show(NothingSelectedMessage);
			}
			return selectedTransactions;
		}

		void CheckCollectionContainAtleastOneTransactionForEachSettlementGroup(TransactionHeaderCollection transactionsToPay)
		{
			List<ZGuid> uniqueSettlementGroups = new List<ZGuid>();
			foreach (TransactionHeader header in transactionsToPay)
			{
				if (header.Header.APSettlementGroupPK.IsValid &&
							!uniqueSettlementGroups.Contains(header.Header.APSettlementGroupPK))
				{
					uniqueSettlementGroups.Add(header.Header.APSettlementGroupPK);
				}
			}

			ZString settlemntGroupsWithMultiReference = ZString.Empty;
			foreach (ZGuid settlementGroup in uniqueSettlementGroups)
			{
				ZBool removeTransactionsRelatedToCurrentSettlementGroup = ZBool.False;
				BusinessObject[] transactions = transactionsToPay.Find(new ZQuery(AccTransactionHeaderSchema.AH_OH, settlementGroup));

				foreach (BusinessObject transaction in transactions)
				{
					if (((TransactionHeader)transaction).OH_APSettlementGroup.IsValid && ((TransactionHeader)transaction).OH_APSettlementGroup != settlementGroup)
					{
						settlemntGroupsWithMultiReference = settlemntGroupsWithMultiReference + System.Environment.NewLine + GetOH_CodeByPK(settlementGroup);
						removeTransactionsRelatedToCurrentSettlementGroup = ZBool.True;
					}
				}
				if (removeTransactionsRelatedToCurrentSettlementGroup)
				{
					RemoveAllTransactionsForSpecifiedSettlementGroup(transactionsToPay, settlementGroup);
				}
			}
			if (!settlemntGroupsWithMultiReference.IsEmpty)
			{
				Globals.Message.Show(SettlementGroupWithMultiReferenceMessageHeader + settlemntGroupsWithMultiReference + System.Environment.NewLine + SettlementGroupWithMultiReferenceMessageEnd);
			}
		}

		void RemoveAllTransactionsForSpecifiedSettlementGroup(TransactionHeaderCollection transactions, ZGuid settlementGroup)
		{
			List<TransactionHeader> transactionsToRemove = new List<TransactionHeader>();
			foreach (TransactionHeader transaction in transactions)
			{
				if (transaction.OH_APSettlementGroup == settlementGroup || transaction.AH_OH == settlementGroup)
				{
					transactionsToRemove.Add(transaction);
				}
			}
			foreach (TransactionHeader transaction in transactionsToRemove)
			{
				transactions.Remove(transaction);
			}
		}

		ZString GetOH_CodeByPK(ZGuid organisationPK)
		{
			OrgHeader organisation = Factory.Load<OrgHeader>(organisationPK);
			return organisation != null ? organisation.OH_Code : ZString.Empty;
		}

		#endregion

		#region IDocumentBusinessContext members

		CargoWise.Definitions.BusinessContext IDocumentBusinessContext.BusinessContext => CargoWise.Definitions.BusinessContext.APTransaction;

		#endregion
	}
}
