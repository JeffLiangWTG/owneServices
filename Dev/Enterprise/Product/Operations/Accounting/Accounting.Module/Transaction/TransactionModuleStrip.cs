#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.Invoices;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module
{
	public abstract partial class TransactionModuleStrip : FilterGridModuleWithMultipleReversing
	{
#if DEBUG
		public ControllerID ExpectedOpenedMatchFormID_ForTest => ExpectedOpenedMatchFormID;
#endif

		protected abstract ControllerID ExpectedOpenedMatchFormID { get; }

		protected static string NothingSelectedMessage => Res.GetString("23ae666e-c786-4dd9-a3e7-24406144971d", "Please select Invoices first");

		protected static string NothingToPostAmongSelectedTransactionsMessage => Res.GetString("02fc83c2-a6ce-4862-9af4-eadd02b0484e", "You can only use this option with transactions that have not been fully paid.");

		protected static string CannotIncludeInvoicesWithNotionalWHTInPaymentBatchMessage => Res.GetString("e1404e30-e818-410c-9eb7-e19bef65c19a", "AP Invoices with a Notional WHT cannot be included in a payment batch.");

		public static string CopyInactiveDepartment => Res.GetString("efadfcba-79c6-6490-de62-defd0c695643", "The invoice you are trying to copy has an inactive department.\r\n{0} will copy the selected invoice and use the current login department instead of the inactive department.\r\nClick 'Yes' to continue.", Core.Constants.ProductName);

		public static string CopyInactiveLineDepartmentFromHeader => Res.GetString("5cc8981b-99cb-49fd-a775-602766196fec", "The invoice you are trying to copy has inactive line department(s).\r\n{0} will copy the line department from header instead of the inactive department.\r\nClick 'Yes' to continue.", Core.Constants.ProductName);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		protected override ZQuery GetDisplayResultsQuery()
		{
			var query = base.GetDisplayResultsQuery();
			if (GridCollection != null && GridCollection is ISubsetBusinessObjectCollection subsetCollection)
			{
				var collection = subsetCollection.CollectionToFilter;

				if (collection != null && collection.CompleteFilter != null)
				{
					query.AddToFilter(new ZQuery(collection.CompleteFilter), JoinCondition.And);
				}
			}

			return query;
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(Res.GetString("ab5b9f30-21e4-4cb7-b585-bad46e0538dc", "&XML Transaction"), new EventHandler(ImportTransactionXMLEventHandler));
			AddImportDataMenuItem(Res.GetString("46202a21-d29d-4ce5-85db-e285365d9a85", "&CSV Transaction"), new EventHandler(ImportTransactionCSVEventHandler));
			AddImportDataMenuItem(Res.GetString("0c635b41-bbb8-4bcf-a78e-807509b6de62", "&Remittance File"), new EventHandler(ImportRemittanceFileEventHandler));
		}

		protected override ZQuery ExportQuery
		{
			get
			{
				return CreateCollectionExportQuery(GridCollection, FilterBusinessObject);
			}
		}

		protected override void OnAfterPerformSearchCore()
		{
			var collection = GetTransactionCollection();
			SetWHTAmountLoader(collection);
			base.OnAfterPerformSearchCore();
		}

		void SetWHTAmountLoader(TransactionHeaderCollection collection)
		{
#if DEBUG
			//C:\Dev\Enterprise\Architecture\GUI\Shell\Core\Public\Modules\FilteredGridLoader.cs
			//Inside PerformSearch function, Collection's Factory is changed. 
			WHTAmountLoaderSubstituter_ForTestOnly?.Invoke(collection.Factory);
#endif

			if (collection != null && collection.Count > 0)
			{
				collection.Factory.RegisterTransactionsForLoadingWHTAmounts(false, collection.Select(x => x.PK).ToArray());
			}
		}

#if DEBUG
		public Action<BusinessObjectFactory> WHTAmountLoaderSubstituter_ForTestOnly { get; set; }
#endif

		protected TransactionHeaderCollection GetTransactionCollection()
		{
			var collection = GridCollection as TransactionHeaderCollection;
			if (collection == null && GridCollection is FilteredTransactionHeaderCollectionView)
			{
				collection = ((FilteredTransactionHeaderCollectionView)GridCollection).CollectionToFilter as TransactionHeaderCollection;
			}
			return collection;
		}

		internal static ZQuery CreateCollectionExportQuery(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		{
			if (gridCollection is ISubsetBusinessObjectCollection subsetCollection)
			{
				var collection = subsetCollection.CollectionToFilter;
				if (collection != null)
				{
					var query = new ZQuery(collection.CompleteFilter);

					if (filterBusinessObject != null && filterBusinessObject.Filter != null)
					{
						query.AddToFilter(new ZQuery(filterBusinessObject.Filter), JoinCondition.And);
					}

					return query;
				}
			}

			return new ZQuery();
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override IZForm ShowTemplateCopyForm(BusinessObject selectedBusinessObject)
		{
			IZForm result = null;
			TransactionHeader selectedTransaction = selectedBusinessObject as TransactionHeader;
			if (selectedTransaction != null && selectedTransaction.Header != null && !selectedTransaction.Header.OH_IsActive)
			{
				Globals.Message.ShowInformation(Res.GetString("2573bb77-2b66-4cff-9a0e-da5d0c695f3b", "This transaction cannot be copied because it is for an inactive organization"), Res.GetString("e51dfcba-69c6-4490-ae62-7eb106a49a8e", "Copy Transaction"));
			}
			else if (CheckAndConfirmInactiveDeparmentHandling(selectedTransaction))
			{
				result = base.ShowTemplateCopyForm(selectedBusinessObject);
			}
			return result;
		}

		bool CheckAndConfirmInactiveDeparmentHandling(TransactionHeader header)
		{
			if (header.Department == null || !header.Department.GE_IsActive)
			{
				return Globals.Message.Show(CopyInactiveDepartment,
						Res.GetString("e51dfcba-69c6-4490-ae62-7eb106a49a8e", "Copy Transaction"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
			}
			else if (header is TransactionHeaderWithLines headerWithLines && headerWithLines.Lines.Cast<TransactionLine>().Any(x => x.Department == null || !x.Department.GE_IsActive))
			{
				return Globals.Message.Show(CopyInactiveLineDepartmentFromHeader,
					Res.GetString("f8fcf5f0-3af4-43a0-b7cd-9828d0204635", "Copy Transaction"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
			}

			return true;
		}

		#region GetNewMenuItems

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			menuItems.Remove(DeleteMenuItem);
			if (CopyMenuItem != null)
			{
				menuItems.Remove(CopyMenuItem);
			}
			menuItems.Add(DeleteMenuItem);
			if (CopyMenuItem != null)
			{
				menuItems.Add(CopyMenuItem);
			}

			NewMenuItem.MenuItems.Add(new ZMenuItem(NewInvoiceMenuText, new EventHandler(HandleNewInvoice)));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewCreditNoteMenuText, new EventHandler(HandleNewCreditNote)));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewAdjustmentNoteMenuText, new EventHandler(HandleNewAdjustmentNote)));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewJournalMenuText, new EventHandler(HandleNewJournal)));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewTransferMenuText, new EventHandler(HandleNewTransfer)));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewContraMenuText, new EventHandler(HandleNewContra)));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewReceiptMenuText, new EventHandler(HandleNewReceipt)));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewPaymentMenuText, new EventHandler(HandleNewPayment)));

			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			PrintMenuItem = new ZMenuItem(PrintMenuText);
			PrintMenuItem.MenuItems.Add(new ZMenuItem(PrintTransactionMenuText, new EventHandler(HandlePrint)));
			PrintMenuItem.MenuItems.Add(new ZMenuItem(PrintMatchDocMenuText, new EventHandler(HandlePrintMatchingReport)));
			PrintMenuItem.MenuItems.Add(new ZMenuItem(AccountingJournalPrintHelper.PrintAccountingJournalText, new EventHandler(HandlePrintAccountingJournal)));
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
			{
				PrintMenuItem.MenuItems.Add(new ZMenuItem(PrintAccountingVoucherText, new EventHandler(HandlePrintAccountingVoucher)));
			}
			PrintMenuItem.MenuItems.Add(new ZMenuItem("-"));
			PrintMenuItem.MenuItems.Add(new ZMenuItem(MarkAsNotPrintedMenuText, new EventHandler(HandleMarkAsNotPrinted)));
			result.Add(PrintMenuItem);

			var menuItems = new MenuItem[2] { SumSelectedTransactionsMenuItem = new ZMenuItem(SumSelectedTransactionsMenuItemName, new EventHandler(HandleSumSelectedTransactions)), new ZMenuItem("-") };
			result.InsertRange(0, menuItems);
			return result.ToArray();
		}

		ZString CheckTransactionsWithZeroOutstandingAmount(BusinessObject[] selectedBusinessObjects)
		{
			var result = ZString.Empty;
			var query = new ZQuery(AccTransactionHeaderSchema.PK, selectedBusinessObjects.Select(x => x.PK));
			var newSelectedBusinessObjects = new BusinessObjectFactory().Load<TransactionHeader>(query);
			if (newSelectedBusinessObjects.Any(x => x.AH_OutstandingAmount.IsEmpty))
			{
				result = Res.GetString("ADBB8417-CBE4-40BA-814E-D6A5B1F3046D", "One or more transaction(s) has zero outstanding amount. Please re-select transactions.");
			}
			return result;
		}

		void HandleSumSelectedTransactions(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("9F10F315-8AE9-4189-98EE-73BB740689C1", "Please select transaction(s) to sum"));
			}
			else
			{
				var errorMessage = CheckTransactionsWithZeroOutstandingAmount(SelectedBusinessObjects);
				if (!errorMessage.IsEmpty)
				{
					Globals.Message.Show(errorMessage);
				}
				else
				{
					var transactionsForSummary = SelectedBusinessObjects.Cast<AccTransactionHeader>().ToArray();
					var transactionCurrencySummary = new TransactionCurrencySummary(transactionsForSummary, Factory);
					ZFormModaliser.Show(new TransactionCurrencySummaryForm(transactionCurrencySummary), Grid.FindForm());
				}
			}
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.Transaction.Match", "Match"), new EventHandler(HandleMatch)));
			if (ShowViewMatchedTransactionsMenuItem)
			{
				menuItems.Add(new ZMenuItem(ViewMatchedTransactionsMenuItemText, HandleViewMatchedTransactions, IconTypes.MatchButton, IconTypes.MatchButton));
			}

			if (ID == ModuleIDs.APTransaction || ID == ModuleIDs.ARTransaction)
			{
				RegenerateJournalEntriesHelper.AddRegenerateJournalEntriesMenuItemIfAllowed(menuItems, HandleRegenerateJournalEntries);
			}

			menuItems.AddRange(GetActionMenuItems());
			menuItems.Add(new ZMenuItem("-"));
			menuItems.AddRange(GetOverrideDetailsMenuItems());
			menuItems.Add(new ZMenuItem("-"));
			menuItems.AddRange(GetOverrideCashFlowCategoryMenuItems());

			if (CanAddAuditAndCashMenus)
			{
				menuItems.Add(new ZMenuItem("-"));
				menuItems.AddRange(GetAuditTransactionMenuItems());
				menuItems.Add(new ZMenuItem("-"));
				menuItems.AddRange(GetRecordCashierMenuItems());
			}

			var currCompany = GlbCompany.CurrentCompany;
			var registry = AccountingMasterFilesRegistry.Instance;

			if (!registry.EnableComplianceDocumentModule.Value && currCompany.Country.SupportComplianceSubType)
			{
				menuItems.Add(new ZMenuItem("-"));
				menuItems.AddRange(GetEInvoicingActionMenuItems());
				menuItems.Add(new ZMenuItem(Res.GetString("85c6e111-68df-49cf-b676-35ae9c3a5cf5", "Update Compliance Sub Type and/or Number"), new EventHandler(HandleAllocateSubTypeAndNumber)));
				if (currCompany.Country.Code != CountryCodes.China)
				{
					menuItems.Add(new ZMenuItem(Res.GetString("2998fbd8-200e-4482-ade2-76fe518c4401", "Allocate Compliance Number"), new EventHandler(HandleBulkAllocateComplianceNumber)));
				}

				if (currCompany.Country.HasAccComplianceSequence)
				{
					menuItems.Add(new ZMenuItem(Res.GetString("f4d679f7-8e76-41bd-aaf4-332f58ca99dd", "Lock Counter Compliance Book"), new EventHandler(HandleLockComplianceBook)));
					menuItems.Add(new ZMenuItem(Res.GetString("3697cb1b-2896-45e8-8218-f0ebb142df20", "Release Counter Compliance Book"), new EventHandler(HandleReleaseComplianceBook)));
				}
			}
			var supportPendingInvoiceAction = ((ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IEInvoicingActionProvider>)?.Get())?.SupportPendingInvoiceAction ?? false;
			if (supportPendingInvoiceAction)
			{
				menuItems.AddRange(GetQueuePendingInvoiceMenuItems());
			}

			if (CanCreateComplianceDocuemntsMenus)
			{
				menuItems.Add(new ZMenuItem("-"));
				menuItems.AddRange(GetCreateComplianceDocumentsMenuItems());
			}

			ZString ledgerType = (ID == ModuleIDs.APTransaction) ? (ZString)LedgerTypes.AccountsPayable :
								 (ID == ModuleIDs.ARTransaction) ? (ZString)LedgerTypes.AccountsReceivable :
								 ZString.Empty;

			if (TransactionModuleStripPresentationProvider.IsEInvoicingColumnsAvailable(ledgerType))
			{
				menuItems.Add(new ZMenuItem("-"));
				menuItems.Add(new ZMenuItem(ResetStatusToQueuedText, new EventHandler(HandleResetStatusToQueued)));

				var supportResetToDeliveredInstance = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(currCompany.GC_RN_NKCountryCode) as IInstanceProvider<ISupportResetStatusToDelivered>)?.Get();
				if (supportResetToDeliveredInstance != null)
				{
					menuItems.Add(new ZMenuItem(ResetStatusToDeliveredText, new EventHandler(HandleResetStatusToDelivered)));
				}

				menuItems.AddRange(GetLedgerSpecificEInvoicingActionMenuItems());

				var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(currCompany.GC_RN_NKCountryCode) as IInstanceProvider<IQueueInvoiceForTransmissionProvider>)?.Get();
				if (provider?.ShouldShowQueueInvoiceForTransmissionMenuItem(ledgerType) ?? false)
				{
					menuItems.Add(new ZMenuItem(Res.GetString("DE7BFC47-B40E-4D28-9593-A9EF7D655963", "Queue Invoice For Transmission"), new EventHandler(HandleQueueInvoice)));
				}
			}

			var currCountry = currCompany.GC_RN_NKCountryCode;
			if (currCountry == CountryCodes.Italy)
			{
				if (ID == ModuleIDs.ARTransaction &&
					registry.EnableEInvoicingFunctionality.Value && registry.EReportingSubmitPivotDefaultStatus.Value == EInvoicingPivotState.Pending ||
					ID == ModuleIDs.APTransaction &&
					registry.EnableEInvoicingFunctionalityForPayables.Value && registry.EReportingSubmitPivotDefaultStatusForPayables.Value == EInvoicingPivotState.Pending)
				{
					var subMenu = new ZMenuItem(Res.GetString("1558FA30-7CEE-414C-95AB-410B35A5B32B", "E-Reporting Authorization"));
					menuItems.Add(subMenu);
					subMenu.MenuItems.Add(new ZMenuItem(SetStatusToAwaitText, new EventHandler(HandleSetStatusToAwait)));
					subMenu.MenuItems.Add(new ZMenuItem(AuthorizeAndSendText, new EventHandler(HandleSetStatusToQueued)));
				}
			}

			return menuItems.ToArray();
		}

		protected abstract MenuItem[] GetActionMenuItems();

		protected virtual MenuItem[] GetEInvoicingActionMenuItems() => Array.Empty<MenuItem>();

		protected virtual MenuItem[] GetQueuePendingInvoiceMenuItems() => Array.Empty<MenuItem>();

		protected virtual IEnumerable<MenuItem> GetLedgerSpecificEInvoicingActionMenuItems() => Enumerable.Empty<MenuItem>();

		protected virtual MenuItem[] GetOverrideDetailsMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>();
			menuItems.Add(new ZMenuItem(OverrideTransactionDescriptionMenuText, new EventHandler(HandleOverrideTransactionDescription)));
			menuItems.Add(new ZMenuItem(OverrideAddressContactMenuText, new EventHandler(HandleOverrideAddressAndContact)));
			menuItems.Add(new ZMenuItem(OverrideAgreedPaymentMethodMenuText, new EventHandler(HandleOverrideAgreedPaymentMethod)));
			menuItems.Add(new ZMenuItem(OverrideMatchStatusMenuText, new EventHandler(HandleOverrideMatchStatus)));

			return menuItems.ToArray();
		}

		protected virtual MenuItem[] GetAuditTransactionMenuItems()
		{
			return new List<MenuItem>
			{
				new ZMenuItem(AccountingConstants.AuditAndCashActionText.AuditTransactionText, (sender, e) => AccountingAuditHelper.HandleAuditTransaction(this, new AuditAndCashEventArgs(AuditSecurityCheckpoint, SelectedTransactions))),
				new ZMenuItem(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText, (sender, e) => AccountingAuditHelper.HandleUndoAuditTransaction(this, new AuditAndCashEventArgs(UndoAuditSecurityCheckpoint, SelectedTransactions))),
			}.ToArray();
		}

		protected virtual MenuItem[] GetRecordCashierMenuItems()
		{
			return new List<MenuItem>
			{
				new ZMenuItem(AccountingConstants.AuditAndCashActionText.RecordCashierText, (sender, e) => AccountingAuditHelper.HandleRecordCashier(this, new AuditAndCashEventArgs(RecordCashierSecurityCheckpoint, SelectedTransactions))),
				new ZMenuItem(AccountingConstants.AuditAndCashActionText.ClearCashierText, (sender, e) => AccountingAuditHelper.HandleClearCashier(this, new AuditAndCashEventArgs(ClearCashierSecurityCheckpoint, SelectedTransactions))),
			}.ToArray();
		}

		protected virtual MenuItem[] GetCreateComplianceDocumentsMenuItems()
		{
			var mainMenuItem = new ZMenuItem(ResString.GetMultilingualString("128FAF0D-9B1B-4A4A-AF8C-223A6117347F", "Create Compliance Document Records"));
			var rollupMenuItem = new ZMenuItem(ResString.GetMultilingualString("9AE2D420-979B-4CA8-8D00-94EB3C046C8B", "Roll-up by Charge Code"), new EventHandler(HandleRollupByChargeCode));
			var noRollupMenuItem = new ZMenuItem(ResString.GetMultilingualString("AA023497-FFFB-42F0-AB33-08C14794FCC8", "No Roll-up"), new EventHandler(HandleNoRollup));
			mainMenuItem.MenuItems.AddRange(new[] { rollupMenuItem, noRollupMenuItem });
			return new[] { mainMenuItem };
		}

		protected abstract bool CanAddAuditAndCashMenus { get; }

		protected abstract bool CanCreateComplianceDocuemntsMenus { get; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected AccTransactionHeader[] SelectedTransactions => Grid.GetSelectedElements<AccTransactionHeader>();

		protected abstract SecurityCheckpoint AuditSecurityCheckpoint { get; }

		protected abstract SecurityCheckpoint UndoAuditSecurityCheckpoint { get; }

		protected abstract SecurityCheckpoint RecordCashierSecurityCheckpoint { get; }

		protected abstract SecurityCheckpoint ClearCashierSecurityCheckpoint { get; }

		protected SecurityCheckpoint CreateComplianceDocumentsSecurityCheckpoint
		{
			get { return ComplianceDocumentHelper.GetCreateComplianceDocumentsSecurity(Ledger); }
		}

		protected abstract SecurityCheckpoint ModifyAddressContactForPostedTransactionsSecurity { get; }

		protected abstract SecurityCheckpoint ModifyTransactionDescriptionsSecurity { get; }

		protected void HandleOverrideTransactionDescription(object sender, EventArgs e)
		{
			if (ModifyTransactionDescriptionsSecurity.IsAllowed)
			{
				if (CurrentBusinessObjectInGrid != null)
				{
					bool canDescriptionBeChanged = true;
					foreach (TransactionHeader bizo in SelectedBusinessObjects)
					{
						if (!(bizo is InvoicingBase))
						{
							canDescriptionBeChanged = false;
						}
					}
					if (!canDescriptionBeChanged)
					{
						Globals.Message.ShowInformation(Res.GetString("5709842A-FF0A-4993-BF51-D5BBA79F0BF6", "You can only override description for AP/AR Invoices, Credit and Adjustment Notes."));
					}
					else
					{
						OverrideTransactionDescriptionHelper helper = new OverrideTransactionDescriptionHelper(new BusinessObjectFactory(), SelectedBusinessObjects.Select(bizo => bizo.PK).ToArray());
						ZFormModaliser.Show(new OverrideTransactionDescriptionForm(helper), Grid.FindForm());
					}
				}
				else
				{
					ShowNoSelectedMessage();
				}
			}
			else
			{
				ModifyTransactionDescriptionsSecurity.ShowError();
			}
		}

		protected virtual MenuItem[] GetOverrideCashFlowCategoryMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>();
			menuItems.Add(new ZMenuItem(OverrideCashFlowCategoryMenuText, new EventHandler(HandleOverrideCashFlowCategory)));
			return menuItems.ToArray();
		}

		#endregion

		#region EventHandlers

		#region Handle Change Status

		protected void HandleQueueInvoice(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				var guiHelper = new EInvoicingGUIActionHelper(() => SelectedTransactions.OfType<TransactionHeader>());
				guiHelper.QueueInvoiceForTransmission(GetSecurityCheckpointForQueueInvoiceForTransmission());
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected void HandleResetStatusToQueued(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				var guiHelper = new EInvoicingGUIActionHelper(() => SelectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.ResetStatusToQueued(GetSecurityCheckpointForRequeueTransactionsWithSentStatus());
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected void HandleResetStatusToDelivered(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				var guiHelper = new EInvoicingGUIActionHelper(() => SelectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.ResetStatusToDelivered();
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected void HandleSetStatusToAwait(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				var guiHelper = new EInvoicingGUIActionHelper(() => SelectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.SetStatusToAwait(GetSecurityCheckpointForAwaitTransactionsWithPendingStatus());
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected void HandleSetStatusToQueued(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				var guiHelper = new EInvoicingGUIActionHelper(() => SelectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.SetStatusToQueued(GetSecurityCheckpointForAuthorizeSetsStatusToQUE());
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		SecurityCheckpoint GetSecurityCheckpointForAuthorizeSetsStatusToQUE()
		{
			var securityCheckpoint = Env.Security.None;
			if (ID == ModuleIDs.ARTransaction)
			{
				securityCheckpoint = Env.Security.ReceivablesAuthorizeAndSend;
			}
			else if (ID == ModuleIDs.APTransaction)
			{
				securityCheckpoint = Env.Security.PayablesAuthorizeAndSend;
			}

			return securityCheckpoint;
		}

		SecurityCheckpoint GetSecurityCheckpointForRequeueTransactionsWithSentStatus()
		{
			var securityCheckpoint = Env.Security.None;
			if (ID == ModuleIDs.ARTransaction)
			{
				securityCheckpoint = Env.Security.ReceivablesResetTransactionStatus;
			}
			else if (ID == ModuleIDs.APTransaction)
			{
				securityCheckpoint = Env.Security.PayablesResetTransactionStatus;
			}

			return securityCheckpoint;
		}

		SecurityCheckpoint GetSecurityCheckpointForQueueInvoiceForTransmission()
		{
			var securityCheckpoint = Env.Security.None;
			if (ID == ModuleIDs.ARTransaction)
			{
				securityCheckpoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
			}

			return securityCheckpoint;
		}

		SecurityCheckpoint GetSecurityCheckpointForAwaitTransactionsWithPendingStatus()
		{
			var securityCheckpoint = Env.Security.None;
			if (ID == ModuleIDs.ARTransaction)
			{
				securityCheckpoint = Env.Security.ReceivablesSetTransactionStatusToAwait;
			}
			else if (ID == ModuleIDs.APTransaction)
			{
				securityCheckpoint = Env.Security.PayablesSetTransactionStatusToAwait;
			}
			return securityCheckpoint;
		}

		#endregion

		#region show message

		bool ConfirmToPrintDocument(string message)
		{
			return Globals.Message.Show(message, ComplianceDocumentHelper.PrintingCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		void ShowPrintingResult(string message, bool isError)
		{
			Globals.Message.Show(message, ComplianceDocumentHelper.PrintingCaption, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
		}

		#endregion

		#region Compliance Documents

		protected void HandleRollupByChargeCode(object sender, EventArgs e)
		{
			var message = Res.GetString("3E939D47-0861-4647-B6DF-EB681DE523B4", "Compliance Records created successfully with Roll-up by Charge Code.");
			CreateComplianceDocumentRecords(OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge, message);
		}

		protected void HandleNoRollup(object sender, EventArgs e)
		{
			var message = Res.GetString("999AF246-9731-48B8-8B2A-62FD491143A7", "Compliance Records created successfully without Roll-up.");
			CreateComplianceDocumentRecords(OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup, message);
		}

		void ShowTaxRateIsNullMessage()
		{
			Globals.Message.ShowError(Res.GetString("2032EDDF-8906-4D2F-9EE3-6058F8A5C2FA", "Some of selected transaction does not have a tax rate."), Res.GetString("B27C5B3B-3541-466F-B1D3-6AF12C41CBAB", "Tax Rate Null..."));
		}

		void ShowTransactionTypeNotSupportMessage()
		{
			Globals.Message.ShowError(Res.GetString("154808E9-F695-41A2-A113-A1DAEBBBF396", "Selected transactions should be Invoices or Credit Notes."), Res.GetString("067AC9C5-D95B-4BF3-95B9-67E085FDF731", "Not Support..."));
		}

		void ShowComplianceDocumentExistedMessage()
		{
			Globals.Message.ShowError(Res.GetString("6F26E143-34F5-4DB8-A1D9-22B339CA6843", "Some of compliance document records has been created in selected transactions before."), Res.GetString("DBC32CCD-4CF3-41B6-B1EA-E32BD282D5A6", "Records Existed..."));
		}

		void ShowTransactionCancelledMessage()
		{
			Globals.Message.ShowError(Res.GetString("82DB140B-F1CA-4857-8A62-DA7CF4BAB36D", "No compliance document record created. Compliance document record cannot be created for canceled transactions."), Res.GetString("11912E13-5FDE-46A5-8203-AD63A5CC2739", "Transactions Canceled..."));
		}

		bool CheckTransactionIsInvoiceOrCreditNote()
		{
			var invoices = SelectedTransactions.OfType<InvoicingBase>();
			return invoices.Count() == SelectedTransactions.Length && invoices.All(x => x.HasSupportedTransactionTypes);
		}

		bool CheckHasLineWithoutTaxID() => SelectedTransactions.OfType<InvoicingBase>().Any(x => x.LinesContainEmptyTaxID);

		bool CheckHasTransactionCancelled() => SelectedTransactions.OfType<InvoicingBase>().Any(x => x.AH_IsCancelled);

		bool CheckComplianceDocumentRecordExisted()
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentPivot));
			var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			var subQuery1 = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			subQuery1.AddToFilter(AccTransactionHeaderSchema.PK, SelectedTransactions.Select(x => x.PK));
			subQuery.AddSubQuery(AccTransactionLinesSchema.AL_AH, subQuery1, JoinCondition.And);

			var subQuery2 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentLine), AccComplianceDocumentLineSchema.PK);
			var subQuery3 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentHeader), AccComplianceDocumentHeaderSchema.PK);
			subQuery3.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_DocumentStatus, SQLComparisonOperator.NotEqual, ComplianceDocumentStatus.Voided);
			subQuery2.AddSubQuery(AccComplianceDocumentLineSchema.ADL_ADH, subQuery3, JoinCondition.And);

			query.AddSubQuery(AccComplianceDocumentPivotSchema.ADP_AL, subQuery, JoinCondition.And);
			query.AddSubQuery(AccComplianceDocumentPivotSchema.ADP_ADL, subQuery2, JoinCondition.And);

			return Factory.Exists(typeof(AccComplianceDocumentPivot), query);
		}

		bool CheckPromptToPrintComplianceDocumentOnCreation => Ledger == LedgerTypes.AccountsReceivable
			&& AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value
			&& AccountingMasterFilesRegistry.Instance.PromptToPrintComplianceDocumentOnCreation.Value;

		void CreateComplianceDocumentRecords(string createOption, ZString message)
		{
			if (!CreateComplianceDocumentsSecurityCheckpoint.IsAllowed)
			{
				CreateComplianceDocumentsSecurityCheckpoint.ShowError();
				return;
			}

			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				ShowNoSelectedMessage();
				return;
			}

			if (!CheckTransactionIsInvoiceOrCreditNote())
			{
				ShowTransactionTypeNotSupportMessage();
				return;
			}

			if (CheckHasTransactionCancelled())
			{
				ShowTransactionCancelledMessage();
				return;
			}

			if (CheckHasLineWithoutTaxID())
			{
				ShowTaxRateIsNullMessage();
				return;
			}

			if (CheckComplianceDocumentRecordExisted())
			{
				ShowComplianceDocumentExistedMessage();
				return;
			}

			var invoices = SelectedTransactions.OfType<InvoicingBase>().ToArray();
			if (invoices.Any())
			{
				var creator = new ComplianceDocumentCreator(invoices, createOption);
				var compliances = creator.CreateComplianceDocumentRecords();
				var invoicePKs = compliances.SelectMany(x => x.TransactionHeaders).Select(x => x.PK).Distinct().ToArray();
				if (invoicePKs.Length == 0)
				{
					Globals.Message.ShowError(GetComplianceDocumentNegativeMessage(), NegativeComplianceCaption);
				}
				else
				{
					creator.Factory.Save();

					if (invoicePKs.Length < invoices.Length)
					{
						var invoicesStr = string.Join(", ", invoices.Where(x => !invoicePKs.Contains(x.PK)).OrderBy(x => x.AH_TransactionNum).Select(x => x.AH_TransactionNum));

						Globals.Message.ShowWarning(GetComplianceDocumentNegativeMessageWithTransNum(invoicesStr), NegativeComplianceCaption);
					}
					else if (CheckPromptToPrintComplianceDocumentOnCreation)
					{
						ComplianceDocumentHelper.PromptAndPrintComplianceDocument(ConfirmToPrintDocument, ShowPrintingResult, compliances, message);
					}
					else
					{
						Globals.Message.ShowInformation(message);
					}
				}
			}
		}

		#endregion

		protected void HandleOverrideAddressAndContact(object sender, EventArgs e)
		{
			if (ModifyAddressContactForPostedTransactionsSecurity.IsAllowed)
			{
				if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
				{
					if (SelectedBusinessObjects.Any(bizo => !(bizo is InvoicingBase) && !(bizo is Payment)))
					{
						Globals.Message.ShowInformation(Res.GetString("b2fbe35c-1712-42a3-b66c-63f0ff6476e2", "You can only override address and contact for Invoices, Credit Notes, Adjustment Notes and Payments."));
					}
					else
					{
						OverrideInvoiceAddressContactHelper helper = new OverrideInvoiceAddressContactHelper(new BusinessObjectFactory(), SelectedBusinessObjects.Select(bizo => bizo.PK).ToArray());
						ZFormModaliser.Show(new OverrideInvoiceAddressContactForm(helper), Grid.FindForm());
					}
				}
				else
				{
					ShowNoSelectedMessage();
				}
			}
			else
			{
				ModifyAddressContactForPostedTransactionsSecurity.ShowError();
			}
		}

		protected abstract SecurityCheckpoint ModifyAgreedPaymentMethodForPostedTransactionsSecurity { get; }

		protected abstract SecurityCheckpoint ModifyDueDateForPostedTransactionsSecurity { get; }

		protected void HandleOverrideAgreedPaymentMethod(object sender, EventArgs e)
		{
			if (ModifyAgreedPaymentMethodForPostedTransactionsSecurity.IsAllowed || ModifyDueDateForPostedTransactionsSecurity.IsAllowed)
			{
				if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
				{
					if (SelectedBusinessObjects.Any(bizo => !(bizo is InvoicingBase || bizo is Journal)))
					{
						Globals.Message.ShowInformation(Res.GetString("31C2A1C8-A001-4576-B63F-7F9E6D4676DB", "You can only override agreed payment method or due date for Invoices, Credit Notes, Adjustment Notes and Journals."));
					}
					else
					{
						var helper = new OverrideTransactionAgreedPaymentMethodHelper(new BusinessObjectFactory(), SelectedBusinessObjects.Select(bizo => bizo.PK).ToArray());
						ZFormModaliser.Show(new OverrideTransactionAgreedPaymentMethodForm(helper), Grid.FindForm());
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
					ModifyAgreedPaymentMethodForPostedTransactionsSecurity.DisplayTextPathToSecurityRight,
					ModifyDueDateForPostedTransactionsSecurity.DisplayTextPathToSecurityRight));
			}
		}

		protected abstract SecurityCheckpoint ModifyCashFlowCategoryForPostedTransactionsSecurity { get; }

		protected void HandleOverrideCashFlowCategory(object sender, EventArgs e)
		{
			if (ModifyCashFlowCategoryForPostedTransactionsSecurity.IsAllowed)
			{
				if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
				{
					if (SelectedBusinessObjects.Any(bizo => !(bizo is ReceiptPaymentBase)))
					{
						Globals.Message.ShowInformation(Res.GetString("0cc55899-6f97-41ae-9547-529c0aed2a06", "You can only override cash flow category for receipt or payment."));
					}
					else
					{
						OverrideReceiptPaymentCashFlowCategoryHelper helper = new OverrideReceiptPaymentCashFlowCategoryHelper(new BusinessObjectFactory(), SelectedBusinessObjects.Select(bizo => bizo.PK).ToArray());
						ZFormModaliser.Show(new OverrideReceiptPaymentCashFlowCategoryForm(helper), Grid.FindForm());
					}
				}
				else
				{
					ShowNoSelectedMessage();
				}
			}
			else
			{
				ModifyCashFlowCategoryForPostedTransactionsSecurity.ShowError();
			}
		}

		protected abstract SecurityCheckpoint ModifyMatchStatusAndReasonSecurity { get; }

		void HandleOverrideMatchStatus(object sender, EventArgs e)
		{
			if (ModifyMatchStatusAndReasonSecurity.IsAllowed)
			{
				if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
				{
					var helper = new OverrideMatchStatusHelper(new BusinessObjectFactory(), SelectedBusinessObjects.Select(bizo => bizo.PK).ToArray());
					ZFormModaliser.Show(new OverrideMatchStatusForm(helper), Grid.FindForm());
				}
				else
				{
					ShowNoSelectedMessage();
				}
			}
			else
			{
				ModifyMatchStatusAndReasonSecurity.ShowError();
			}
		}

		#region Allocate Compliance Sub-Type and Number

#if DEBUG
		ZForm LastShownInvoicePrintingForm;
#endif

		protected abstract SecurityCheckpoint AllocateComplianceNumberCheckpoint { get; }

		void HandleBulkAllocateComplianceNumber(object sender, EventArgs e)
		{
			if (AllocateComplianceNumberCheckpoint.IsAllowed)
			{
				if (SelectedBusinessObjects.Length > 0)
				{
					HandleBulkAllocateComplianceNumberCore();
				}
				else
				{
					Globals.Message.ShowError(NoItemSelected);
				}
			}
			else
			{
				AllocateComplianceNumberCheckpoint.ShowError();
			}
		}

		List<TransactionHeader> GetInvoicesToUpdate(bool checkForEmptySubType)
		{
			TransactionHeader[] selectedTransactions = Array.ConvertAll(SelectedBusinessObjects, x => (TransactionHeader)x);
			List<TransactionHeader> transactionsToUpdate = new List<TransactionHeader>();
			foreach (TransactionHeader transaction in selectedTransactions)
			{
				if (transaction.AH_TransactionType == TransactionTypes.Invoice ||
					transaction.AH_TransactionType == TransactionTypes.CreditNote ||
					transaction.AH_TransactionType == TransactionTypes.AdjustmentNote)
				{
					if (!checkForEmptySubType ||
						(checkForEmptySubType && !transaction.AH_ComplianceSubType.IsEmpty))
					{
						transactionsToUpdate.Add(transaction);
					}
				}
			}
			return transactionsToUpdate;
		}

		protected virtual void HandleBulkAllocateComplianceNumberCore()
		{
			List<TransactionHeader> transactionsToUpdate = GetInvoicesToUpdate(true);
			for (int index = transactionsToUpdate.Count - 1; index >= 0; index--)
			{
				if (!(transactionsToUpdate[index]).AH_TransactionReference.IsEmpty)
				{
					transactionsToUpdate.RemoveAt(index);
				}
			}

			if (transactionsToUpdate.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("6648b081-1459-49ad-b20c-1b8918900383", "This function will only update INV, CRD and ADJ transactions that already have a Compliance Sub Type and do not already have a number allocated.\r\n\r\nThe transactions you've selected do not satisfy these criteria.\r\n\r\nNo transactions will be updated."));
			}
			else if (transactionsToUpdate.Count < SelectedBusinessObjects.Length)
			{
				Globals.Message.ShowWarning(Res.GetString("c7717cbf-259f-44a6-b91e-e40255f8a121", "This function will only update INV, CRD and ADJ transactions that already have a Compliance Sub Type.\r\n\r\nSome of the transactions you've selected do not satisfy these criteria.\r\n\r\nOnly INV, CRD and ADJ transactions with a Compliance Sub Type will be updated."));
				PrintAndAllocateSequenceNumber(transactionsToUpdate.ToArray());
			}
			else
			{
				PrintAndAllocateSequenceNumber(transactionsToUpdate.ToArray());
			}
		}

		protected void PrintAndAllocateSequenceNumber(TransactionHeader[] transactionsToUpdate)
		{
			new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly).PrintGovtTaxInvoices(transactionsToUpdate);
		}

		protected abstract SecurityCheckpoint UpdateComplianceSubTypeOrNumberCheckpoint { get; }

		void HandleAllocateSubTypeAndNumber(object sender, EventArgs e)
		{
			if (UpdateComplianceSubTypeOrNumberCheckpoint.IsAllowed)
			{
				if (SelectedBusinessObjects.Length > 0)
				{
					HandleAllocateSubTypeAndNumberCore();
				}
				else
				{
					Globals.Message.ShowError(NoItemSelected);
				}
			}
			else
			{
				UpdateComplianceSubTypeOrNumberCheckpoint.ShowError();
			}
		}

		protected virtual void HandleAllocateSubTypeAndNumberCore()
		{
			List<TransactionHeader> transactionsToUpdate = GetInvoicesToUpdate(false);
			if (transactionsToUpdate.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("b27c9cfe-7057-4d83-a851-c5857270e378", @"This function can only be used with INV, CRD and ADJ transactions.
None of the selected transactions fit this criteria.
Please select at least one INV, CRD or ADJ transaction."));
			}
			else if (transactionsToUpdate.Count < SelectedBusinessObjects.Length)
			{
				Globals.Message.ShowWarning(Res.GetString("236611b9-4452-460e-b66c-7a5d1c90bf78", @"This function can only be used with INV, CRD and ADJ transactions.
Some of the transactions selected do not fit this criteria and will not be updated."));
			}

			if (transactionsToUpdate.Count > 0)
			{
				HandlePrintingInvoices(transactionsToUpdate.ToArray(), SelectedBusinessObjects.First());
			}
		}

		protected void HandlePrintingInvoices(TransactionHeader[] transactionsToUpdate, BusinessObject invoice)
		{
			List<ZGuid> transactionPKs = transactionsToUpdate.Select(transaction => transaction.PK).ToList();
			ZPKCollection results = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.InvoicePrinting);
			results.Rebuild(transactionPKs);
#if DEBUG
			LastShownInvoicePrintingForm = (ZForm)ZControllerFactory.Create(ControllerIDs.InvoicePrinting).ShowEditForm(invoice);
#else
			ZControllerFactory.Create(ControllerIDs.InvoicePrinting).ShowEditForm(invoice);
#endif
		}

		protected string NoItemSelected => Res.GetString("79d1b652-522b-4135-855b-a9ebe51a3d48", "Please select an item in the grid.");

		protected virtual void HandleLockComplianceBook(object sender, EventArgs e)
		{
			if (Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed)
			{
				ZController lockComplianceBookController = (MasterFiles.Module.LockComplianceBookController)ZControllerFactory.Create(ControllerIDs.LockComplianceBook);

				var lockForm = (MasterFiles.GUI.LockReleaseComplianceBookForm)lockComplianceBookController.ShowNewForm();

#if DEBUG
				LastShownInvoicePrintingForm = lockForm;
#endif
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("482162e0-8e0d-4fa5-ae75-5b7761e6dfe9", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", Env.Security.ComplianceSequencesModifyLockRelease.DisplayTextPathToSecurityRight));
			}
		}

		protected virtual void HandleReleaseComplianceBook(object sender, EventArgs e)
		{
			if (Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed || Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed)
			{
				ZController releaseComplianceBookController = (MasterFiles.Module.ReleaseComplianceBookController)ZControllerFactory.Create(ControllerIDs.ReleaseComplianceBook);
#if DEBUG
				LastShownInvoicePrintingForm = (ZForm)releaseComplianceBookController.ShowNewForm();
#else
				releaseComplianceBookController.ShowNewForm();
#endif
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("3668228f-35b8-4d87-a96a-bf52110e331b", @"You do not have the appropriate security rights to run this function. 

If you require access to lock/release ‘CTR’ Allocation Level Compliance Invoice Book, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}

If you require access to release ‘CTR’ Allocation Level Compliance Invoice Book locked by other staff, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{1}", Env.Security.ComplianceSequencesModifyLockRelease.DisplayTextPathToSecurityRight, Env.Security.ComplianceSequencesModifyReleaseOtherStaff.DisplayTextPathToSecurityRight));
			}
		}

		#endregion

		#region Handle Match

		protected virtual void HandleMatch(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("77C9085C-3945-43ea-8974-B56DCC086862", "Please select transaction(s) to match"));
			}
			else if (!MatchCheckpoint.IsAllowed)
			{
				MatchCheckpoint.ShowError();
			}
			else
			{
				BusinessObjectFactory matchingFactory = new BusinessObjectFactory();
				TransactionHeaderCollection transactionsForMatching = new TransactionHeaderCollection(matchingFactory);
				bool containsTransactionsWithZeroOutstandingAmount = false;
				foreach (BusinessObject businessobject in SelectedBusinessObjects)
				{
					transactionsForMatching.Add(matchingFactory.ImportFromAnotherFactory(businessobject));
					if (((TransactionHeader)businessobject).AH_OutstandingAmount.IsEmpty)
					{
						containsTransactionsWithZeroOutstandingAmount = true;
						break;
					}
				}

				if (containsTransactionsWithZeroOutstandingAmount)
				{
					Globals.Message.Show(Res.GetString("11BBB25A-2031-4272-9E2E-F56BC5D153C5", "One or more transaction(s) has zero outstanding amount. Matching process terminated"));
				}
				else
				{
					PrimaryOrgSelectorCollection primaryOrgs = new PrimaryOrgSelectorCollection(matchingFactory);
					foreach (TransactionHeader transaction in transactionsForMatching)
					{
						OrgHeader org = matchingFactory.Load<OrgHeader>(transaction.AH_OH);
						if (org == null)
						{
							Globals.Message.Show(Res.GetString("0DC0265B-93AB-4049-A1ED-E977473CD037", "The transaction you are trying to match is missing an organization and cannot be matched."));
							return;
						}
						PrimaryOrgSelector primaryOrg = primaryOrgs.FindByOrgCode(org.OH_Code);
						if (primaryOrg == null)
						{
							primaryOrg = new PrimaryOrgSelector(matchingFactory, org);
							primaryOrgs.Add(primaryOrg);
						}
						primaryOrg.Transactions.Add(transaction);
					}

					ZGuid primaryOrgPK = ZGuid.Empty;
					primaryOrgPK = GetPrimaryOrgFromForm(primaryOrgs);

					if (primaryOrgPK.IsValid)
					{
						MatchingBase matching = GetMatchingObject(matchingFactory);
						matching.PrimaryOrganisationForGUINotification = primaryOrgPK;
						matching.PrimaryOrganization = primaryOrgPK;

						var maxPostDate = transactionsForMatching.Select(t => ((TransactionHeader)t).AH_PostDate.Date).Max();

						if (maxPostDate > ZDateTime.Today)
						{
							matching.MatchDate = maxPostDate;
						}

						if (primaryOrgs.Count > 1)
						{
							foreach (PrimaryOrgSelector primaryOrg in primaryOrgs)
							{
								if (!matching.MatchingFilterBizO.SettlementOrgInfos.ContainsOrgPK(primaryOrg.OrganisationPK))
								{
									OrgLedgerFilter filter = matching.MatchingFilterBizO.SettlementOrgInfos.AddNew();
									filter.Organization = primaryOrg.OrganisationPK;
								}
							}
						}

						matching.MoveFromUnmatchToMatch(transactionsForMatching.ToArray());

						bool success = false;

						if (matching.Balance == 0m)
						{
							matching.RunPreSaveValidation();

							success = !matching.HasErrors && matching.MatchAndClearTransactionsWithSaveErrorHandling();
						}

						if (success)
						{
							Globals.Message.Show(Res.GetString("45E40B79-11EF-42ae-8487-EA6D6055694F",
							"Transactions Matched Successfully: Match Group Number {0}", matching.MatchGroupNumber));
						}
						else if (!matching.Corrupted)
						{
							ShowMatchGroup(matching);
						}
						else
						{
							Globals.Message.ShowError(Res.GetString("2E14721F-7714-4BBA-9674-8CF7880E89A7",
								"This match could not be saved due to another user accessing transactions included in the match. Please close and re-open the module before trying to match again."));
						}
					}
				}
			}
		}
		void ShowMatchGroup(MatchingBase matching)
		{
			var form = ZControllerFactory
				.Create(ExpectedOpenedMatchFormID)
				.ShowFormForNewEntity(matching);
#if DEBUG
			if (Globals.IsTest)
			{
				MatchingFormShown_ForTestOnly = form;
			}
#endif
		}
		ZGuid GetPrimaryOrgFromForm(PrimaryOrgSelectorCollection primaryOrgs)
		{
			ZGuid primaryOrgPK = ZGuid.Empty;
			if (primaryOrgs.Count > 1)
			{
				PrimaryOrgSelectorForm selectorForm = new PrimaryOrgSelectorForm(primaryOrgs);
				ZFormModaliser.ShowDialogAndDispose(selectorForm);
				primaryOrgPK = selectorForm.SelectedOrgPK;
			}
			else if (primaryOrgs.Count == 1)
			{
				primaryOrgPK = primaryOrgs[0].OrganisationPK;
			}
			return primaryOrgPK;
		}

#if DEBUG
		internal IZForm MatchingFormShown_ForTestOnly;
#endif

		protected abstract SecurityCheckpoint MatchCheckpoint { get; }
		protected abstract MatchingBase GetMatchingObject(BusinessObjectFactory factory);
		protected abstract MatchingModule CreateMatchingModuleForViewMatchedTransactions();
		protected bool ShowViewMatchedTransactionsMenuItem =>
			this.GetType() == typeof(APTransactionModuleStrip) || this.GetType() == typeof(ARTransactionModuleStrip);

		#endregion

		#region HandleViewMatchedTranasactions

		void HandleViewMatchedTransactions(object sender, EventArgs e)
		{
			if (!MatchCheckpoint.IsAllowed)
			{
				MatchCheckpoint.ShowError();
				return;
			}
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length != 1)
			{
				Globals.Message.Show(Res.GetString("c0fcc3f7-3620-4742-a035-2de5d52d4acb", "Please select one transaction to search for matches."));
				return;
			}

			var invoice = (TransactionHeader)SelectedBusinessObjects[0];
			var matchingModule = CreateMatchingModuleForViewMatchedTransactions();
			var defaultFilter = new FilterBusinessObjectDefaults();
			defaultFilter.Add(new FilterBusinessObjectDefault(AccountingUtils.NumberFilterTypes.TransactionNumber, "Property", invoice.AH_TransactionNum, false));
			defaultFilter.Add(new FilterBusinessObjectDefault(AccountingUtils.ModesAndTypesFilterTypes.TransactionType, "Property", invoice.AH_TransactionType, false));
			matchingModule.FilterBusinessObject.SetExternalDefaults(defaultFilter);
			var filterObject = (MatchingBaseFilterBusinessObject)matchingModule.FilterBusinessObject;
			filterObject.LedgerFilterValue = invoice.AH_Ledger;

			var popupForm = new ZArchitecture.GUI.Internal.EmbeddedModulePopup(matchingModule);
			popupForm.RequireAtLeastOneItemToBeSelected = false;
			ZFormModaliser.Show(popupForm, Grid.FindForm());
		}

		#endregion

		protected virtual void HandlePrintAccountingVoucher(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("aea0f3ea-2930-40f4-bd42-8e6bdf1a2ab7", "Please select transaction(s) to print"));
			}
			else
			{
				AccountingVoucherPrintHelper accountingVoucherPrintHelper = new AccountingVoucherPrintHelper();
				TransactionHeader[] gridSelections = Grid.GetSelectedElements<TransactionHeader>();
				TransactionHeader[] transactions = TransactionHeaderHelper.ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher(gridSelections);

				PrintTask task = accountingVoucherPrintHelper.GetAccountingVoucherPrintTask(transactions);
				if (task != null)
				{
					task.Run(Env.Security.None);
				}
			}
		}

		protected abstract SecurityCheckpoint MarkAsNotPrintedSecurityItem { get; }

		protected void HandleMarkAsNotPrinted(object sender, EventArgs args)
		{
			if (MarkAsNotPrintedSecurityItem.IsAllowed)
			{
				if (Grid.SelectedElements.Length > 0)
				{
					List<ZGuid> invoicesAndCreditPKs = new List<ZGuid>();
					foreach (TransactionHeader invoice in Grid.SelectedElements)
					{
						if ((invoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice ||
							invoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote)
							&& invoice.AH_InvoicePrinted)
						{
							invoicesAndCreditPKs.Add(invoice.PK);
						}
					}

					if (invoicesAndCreditPKs.Count > 0)
					{
						BusinessObjectFactory newFactory = new BusinessObjectFactory();
						var invoicesAndCredits = newFactory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, invoicesAndCreditPKs));
						var invoicesThatCannotBeMarkedAsNotPrintedAndMessage = invoicesAndCredits.OfType<InvoicingBase>().GetTransactionsThatCannotBeMarkedAsNotPrintedDueToCompliance();
						var transactionsThatCanBeMarkedAsNotPrintedPKs = invoicesAndCredits.Select(x => x.PK).Except(invoicesThatCannotBeMarkedAsNotPrintedAndMessage.Invoices.Select(x => x.PK));

						if (!transactionsThatCanBeMarkedAsNotPrintedPKs.Any())
						{
							Globals.Message.ShowError(!string.IsNullOrEmpty(invoicesThatCannotBeMarkedAsNotPrintedAndMessage.Message)
								? invoicesThatCannotBeMarkedAsNotPrintedAndMessage.Message
								: Res.GetString("cd9e7f6e-0481-445c-b590-70e21669de6f", "There is no transaction that can be marked as not printed"));
						}
						else if (invoicesThatCannotBeMarkedAsNotPrintedAndMessage.Invoices.Any())
						{
							var errMsgBuilder = new ZStringBuilder(invoicesThatCannotBeMarkedAsNotPrintedAndMessage.Message);
							errMsgBuilder.AppendLine(Res.GetString("efc9cff5-f6d7-4eb0-8699-1cb9f601ad55", "Do you want to mark rest of the transactions as not printed?"));

							if (Globals.Message.Show(errMsgBuilder.ToStringWithNewLineBetweenAppends(), Res.GetString("f2775567-5085-48c4-aea4-7eacd961c5b8", "Continue"), MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
							{
								var transactionsThatCanBeMarkedAsNotPrinted = invoicesAndCredits.Where(x => transactionsThatCanBeMarkedAsNotPrintedPKs.Contains(x.PK)).ToArray();
								MarkAsNotPrinted(newFactory, transactionsThatCanBeMarkedAsNotPrinted);
							}
						}
						else
						{
							MarkAsNotPrinted(newFactory, invoicesAndCredits);
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("3dc46091-1944-4801-9e47-c0e44f53ce3f", "There are no invoices or credit notes to mark as not printed in your selection"));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("cfb24306-b31d-4587-bc4a-5e3c53679ecf", "You have not selected any invoices"));
				}
			}
			else
			{
				MarkAsNotPrintedSecurityItem.ShowError();
			}

			void MarkAsNotPrinted(BusinessObjectFactory localFactory, TransactionHeader[] transactions)
			{
				foreach (TransactionHeader invoiceOrCredit in transactions)
				{
					invoiceOrCredit.AH_InvoicePrinted = false;
				}
				localFactory.Save();

				Globals.Message.Show(Res.GetString("94593428-fd95-42bb-9cd5-55f0b0d667d3", "Invoice/Credit Note marked as not printed"));
			}
		}

		protected virtual void HandlePrintAccountingJournal(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("ebb3f7af-2024-49e1-a232-7dac83a9d2ee", "Please select transaction(s) to print."));
			}
			else
			{
				var gridSelections = Grid.GetSelectedElements<TransactionHeader>();
				AccountingJournalPrintHelper.PrintAccountingJournal(gridSelections);
			}
		}

		protected virtual void HandlePrintMatchingReport(object sender, EventArgs e)
		{
			InvoicePrintHelper.PrintMatchingReport(CurrentBusinessObjectInGrid as TransactionHeader);
		}

		protected void HandlePrint(object sender, EventArgs e)
		{
			BusinessObjectFactory transactionPrintingFactory = new BusinessObjectFactory();
			TransactionHeaderCollection transactionCollection = new TransactionHeaderCollection(transactionPrintingFactory);
			var paymentCollection = new List<TransactionHeader>();
			var receiptCollection = new List<TransactionHeader>();

			ZStringBuilder errorMessages = new ZStringBuilder(Res.GetString("b28e43a2-1a5c-4f12-ab7e-cf3e7363de81", "The following transaction(s) cannot be printed"));
			errorMessages.AppendLine();
			bool hasError = false;

			BusinessObject[] transactionsSelected;
			if (Grid.SelectedElements.Length == 0 && CurrentBusinessObjectInGrid != null)
			{
				transactionsSelected = new BusinessObject[] { (TransactionHeader)CurrentBusinessObjectInGrid };
			}
			else
			{
				transactionsSelected = Grid.SelectedElements;
			}

			foreach (TransactionHeader header in transactionsSelected)
			{
				if (!header.Header.OH_IsActive)
				{
					hasError = true;
					errorMessages.AppendLine(Res.GetString("31e5c35b-6b39-4c4c-8515-4e590a971299", "Transaction {0} cannot be printed because it is for an inactive organization", header.AH_TransactionNum));
				}
				else
				{
					ZString transactionType = header.AH_TransactionType;
					SecurityCheckpoint securityCheckPoint = GetSecurityForPrintTransaction(transactionType);

					if (securityCheckPoint == null || securityCheckPoint.IsAllowed)
					{
						switch (transactionType)
						{
							case TransactionTypes.AdjustmentNote:
							case TransactionTypes.CreditNote:
							case TransactionTypes.Invoice:
								transactionCollection.Add(header);
								break;
							case TransactionTypes.Contra:
								hasError = true;
								errorMessages.AppendLine(Res.GetString("ec2db01c-5606-4061-b0d1-440beb992647", "Transaction {0} cannot be printed because it is Contra", header.AH_TransactionNum));
								break;
							case TransactionTypes.Discount:
								hasError = true;
								errorMessages.AppendLine(Res.GetString("985cfb19-e3ea-4aa0-83c3-483d4a2290eb", "Transaction {0} cannot be printed because it is Discount", header.AH_TransactionNum));
								break;
							case TransactionTypes.ExchangeDifference:
								hasError = true;
								errorMessages.AppendLine(Res.GetString("2a13915c-5f27-441e-917b-b8afaaa5cc37", "Transaction {0} cannot be printed because it is Exchange Difference", header.AH_TransactionNum));
								break;
							case TransactionTypes.Journal:
								hasError = true;
								errorMessages.AppendLine(Res.GetString("31093841-a1c0-41d9-851d-4c95d818ff46", "Transaction {0} cannot be printed because it is Journal", header.AH_TransactionNum));
								break;
							case TransactionTypes.Overpayment:
								hasError = true;
								errorMessages.AppendLine(Res.GetString("1e6ea973-f780-4604-905b-1c3f3e82ff49", "Transaction {0} cannot be printed because it is Overpayment", header.AH_TransactionNum));
								break;
							case TransactionTypes.Payment:
								paymentCollection.Add(header);
								break;
							case TransactionTypes.Receipt:
								receiptCollection.Add(header);
								break;
							case TransactionTypes.Transfer:
								hasError = true;
								errorMessages.AppendLine(Res.GetString("b558d12c-da81-4bbe-81fd-b76a0552ffae", "Transaction {0} cannot be printed because it is Transfer", header.AH_TransactionNum));
								break;
							default:
								InvoicePrintHelper.HandleNotImplemented();
								break;
						}
					}
					else
					{
						hasError = true;
						errorMessages.AppendLine(Res.GetString("f02a59f2-1c04-4725-a5eb-d3b1dd025bee", "Transaction {0} cannot be printed because {1}", header.AH_TransactionNum, securityCheckPoint.ErrorMessageForNotAllowed));
						errorMessages.AppendLine();
					}
				}
			}

			if (hasError)
			{
				Globals.Message.ShowInformation(errorMessages.ToString(), Res.GetString("b346d05a-a49e-4c32-9013-2370c5b9e9ae", "Print Transaction"));
			}

			if (transactionCollection.Count > 0)
			{
				PrintInvoices(transactionCollection.ToArray<TransactionHeader>());
			}
			if (paymentCollection.Count > 0)
			{
				paymentCollection.Sort((x, y) => x.AH_ChequeOrReference.CompareTo(y.AH_ChequeOrReference));
				var printManager = new PaymentBatchPrintManager(paymentCollection, null, TransactionTypes.Payment, transactionPrintingFactory);
				printManager.Print();
			}
			if (receiptCollection.Count > 0)
			{
				ReceiptPrint receiptPrintHandler = new ReceiptPrint();
				receiptPrintHandler.Print(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, receiptCollection.Select(x => x.PK.ToGuid()).ToArray());
			}
		}

		protected abstract void PrintInvoices(TransactionHeader[] headers);

		protected abstract SecurityCheckpoint GetSecurityForPrintTransaction(ZString transactionType);

		protected void HandleNewPayment(object sender, EventArgs e)
		{
			HandleNew(ZArchitecture.Core.TransactionTypes.Payment);
		}

		protected void HandleNewReceipt(object sender, EventArgs e)
		{
			HandleNew(ZArchitecture.Core.TransactionTypes.Receipt);
		}

		protected void HandleNewContra(object sender, EventArgs e)
		{
			HandleNew(ZArchitecture.Core.TransactionTypes.Contra);
		}

		protected void HandleNewTransfer(object sender, EventArgs e)
		{
			HandleNew(ZArchitecture.Core.TransactionTypes.Transfer);
		}

		protected void HandleNewJournal(object sender, EventArgs e)
		{
			HandleNew(ZArchitecture.Core.TransactionTypes.Journal);
		}

		protected void HandleNewAdjustmentNote(object sender, EventArgs e)
		{
			HandleNew(ZArchitecture.Core.TransactionTypes.AdjustmentNote);
		}

		protected void HandleNewCreditNote(object sender, EventArgs e)
		{
			HandleNew(ZArchitecture.Core.TransactionTypes.CreditNote);
		}

		protected void HandleNewInvoice(object sender, EventArgs e)
		{
			HandleNew(ZArchitecture.Core.TransactionTypes.Invoice);
		}

		protected void HandleNew(string transactionType)
		{
			HandleShowingFormSafely(() => HandleNewCore(transactionType));
		}

		protected virtual void HandleNewCore(string transactionType)
		{
			ControllerFromTransactionType(transactionType).ShowNewForm();
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController result = null;

			if (selectedBusinessObject != null)
			{
				AccTransactionHeader selectedHeader = selectedBusinessObject as AccTransactionHeader;
				if (selectedHeader != null)
				{
					result = AccountingControllerCreator.GetNewController(selectedHeader);
				}
			}
			return result;
		}

		protected ZController ControllerFromTransactionType(string transactionType)
		{
			return AccountingControllerCreator.GetNewController(transactionType, Ledger);
		}

		protected void ImportTransactionXMLEventHandler(object sender, EventArgs args)
		{
			if (!Env.Security.ImportXmlAccountingTransactions.IsAllowed)
			{
				Env.Security.ImportXmlAccountingTransactions.ShowError();
			}
			else
			{
				ImportSingleInvoiceXmlDataTransferDirector importDirector = new ImportSingleInvoiceXmlDataTransferDirector(true, Ledger);
				importDirector.PromptUserAndImport(BillingInterfaceName.TransactionsXmlImport);

				if (importDirector.ImportedInvoice != null && !importDirector.ImportedInvoice.IsDeleted)
				{
					DisplayImportedTransactionForm(importDirector.ImportedInvoice);
				}
			}
		}

		protected void ImportTransactionCSVEventHandler(object sender, EventArgs args)
		{
			if (!Env.Security.ImportCsvAccountingTransactions.IsAllowed)
			{
				Env.Security.ImportCsvAccountingTransactions.ShowError();
			}
			else
			{
				TxnHeaderFlatFileDataImporter importer = new TxnHeaderFlatFileDataImporter();
				SingleInvoiceDataImporterBusinessObject importerBusinessObject = new SingleInvoiceDataImporterBusinessObject(new BusinessObjectFactory());

				using (DataImporterForm form = new DataImporterForm(importerBusinessObject, Res.GetString("92eba852-18cd-4678-bc8d-a84ca8e3a80f", "Import Accounting Transactions (CSV)"), BillingInterfaceName.AccountingTransactionsCsvImport))
				{
					importer.ImportingSingleTransaction = true;
					importer.RunExtraValidation = true;
					form.Importer = importer;
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}

				if (importer.ImportedInvoice != null && !importer.ImportedInvoice.IsDeleted)
				{
					DisplayImportedTransactionForm(importer.ImportedInvoice);
				}
			}
		}

		#region ImportRemittanceFileEventHandler

		protected void ImportRemittanceFileEventHandler(object sender, EventArgs args)
		{
			if (ImportRemittanceFileSecurityCheckpoint.IsAllowed)
			{
				using (DataImporterForm form = GetDataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), Res.GetString("92f12e59-23a8-4559-99f2-9c6288b73f88", "Import Remittance File"), BillingInterfaceName.ImportRemittance))
				{
					PaymentReceiptRemittanceDataImporter remittanceFileImporter = new PaymentReceiptRemittanceDataImporter();
					remittanceFileImporter.OnSavePaymentWithChequeNumberAllocator += RemittanceFileImporter_OnSavePaymentWithChequeNumberAllocator;
					remittanceFileImporter.OnPrintMatchingDocument += remittanceFileImporter_OnPrintMatchingDocument;
					form.Importer = remittanceFileImporter;
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
			else
			{
				ImportRemittanceFileSecurityCheckpoint.ShowError();
			}
		}

		protected abstract SecurityCheckpoint ImportRemittanceFileSecurityCheckpoint { get; }

		void RemittanceFileImporter_OnSavePaymentWithChequeNumberAllocator(object sender, EventArgs e)
		{
			PaymentApprovalBase paymentApproval = sender as PaymentApprovalBase;
			if (paymentApproval != null)
			{
				var allocator = new PaymentChequeNumberAllocator(paymentApproval, PaymentChequeNumberAllocator.PrintingMode.PaymentApproval, paymentApproval.Factory);
				BusinessObjectFactory.SaveTogether(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving());
			}
		}

		void remittanceFileImporter_OnPrintMatchingDocument(object sender, BoolResponseEventArgs e)
		{
			e.Response = Globals.Message.Show(Res.GetString("2ff6efd3-d2fb-4b0c-bdbd-e86c75d9cc0a", "Do you want to print the match document for imported transaction/s?"), Res.GetString("a32f73e9-5e89-40f8-aeda-8dc895b54db9", "Print Matching Document"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes;
		}

		#endregion

		protected abstract void DisplayImportedTransactionForm(InvoicingBase importedTransaction);

		#region Implementation

		public static MultilingualString PrintMenuText => ResString.GetMultilingualString("81d246bf-10bd-48c7-892b-386ec022f57a", "&Print");

		protected MenuItem PrintMenuItem;

		protected MenuItem SumSelectedTransactionsMenuItem;

		protected MultilingualString SumSelectedTransactionsMenuItemName => ResString.GetMultilingualString("57BAB704-7878-43FE-9CBD-155E04994939", "Sum Of Transactions Selected");

		protected MultilingualString PrintMatchDocMenuText => ResString.GetMultilingualString("7f1ba246-a9d0-4a52-bee8-37faeb1a6f3e", "Print &Match Doc");

		protected MultilingualString PrintTransactionMenuText => ResString.GetMultilingualString("0243dcd5-ea45-494e-bff3-4080e7de33ba", "Print Transaction");

		protected MultilingualString MarkAsNotPrintedMenuText => ResString.GetMultilingualString("68830389-e126-4378-ac07-96ef23ee9202", "Mark as Not Printed");

		protected MultilingualString PrintAccountingVoucherText => ResString.GetMultilingualString("2b7278c3-f058-47dd-b9b1-f44b87aff5d5", "Print Accounting Voucher");

		protected MultilingualString CopyTextWithoutAmpersand => f_CopyTextWithoutAmpersand ?? (f_CopyTextWithoutAmpersand = ResString.GetMultilingualString("a2db209a-0b1b-4fc7-836c-5d08d4a06454", "Copy"));
		MultilingualString f_CopyTextWithoutAmpersand;

		protected MultilingualString NewContraMenuText => ResString.GetMultilingualString("222365d5-f707-4f06-bc4e-677fb2a7c77c", "New C&ontra");

		protected MultilingualString NewTransferMenuText => ResString.GetMultilingualString("0dcb7514-05c2-424d-b222-45ba0bd97391", "New &Transfer");

		protected MultilingualString NewInvoiceMenuText => ResString.GetMultilingualString("9b0e6820-9688-42fd-bf99-0ba2ace128af", "New I&nvoice");

		protected MultilingualString NewCreditNoteMenuText => ResString.GetMultilingualString("e003a353-c885-4b8e-baa8-176faceb426b", "New Cre&dit Note");

		protected MultilingualString NewAdjustmentNoteMenuText => ResString.GetMultilingualString("9893d918-1f53-4f60-909a-5df5a8ac4671", "New &Adjustment Note");

		protected MultilingualString NewJournalMenuText => ResString.GetMultilingualString("092d1264-885a-4e99-8736-79ab89ffb26b", "New &Journal");

		protected MultilingualString NewPaymentMenuText => ResString.GetMultilingualString("c5428618-e480-4e0f-844c-8125ffc56223", "New Pa&yment");

		protected MultilingualString NewReceiptMenuText => ResString.GetMultilingualString("25ab6cd5-46a0-4cba-a2af-08bbc7482477", "New R&eceipt");

		protected MultilingualString OverrideAddressContactMenuText => ResString.GetMultilingualString("cca681e3-be2a-4c61-902c-c7f711545da9", "Override Address and Contact");

		protected MultilingualString OverrideTransactionDescriptionMenuText => ResString.GetMultilingualString("197d23f1-421a-4703-8ac6-447f30c06502", "Override Transaction Description");

		protected MultilingualString OverrideInvoiceReferencesMenuText => ResString.GetMultilingualString("1F4116BA-043C-4BEC-B73A-F81D8436E65F", "Override Invoice References");

		protected MultilingualString OverrideAgreedPaymentMethodMenuText => ResString.GetMultilingualString("F21589CE-4406-4FCC-BD34-9C09D333DCAF", "Override Agreed Payment Method and Due Date");

		protected MultilingualString OverrideCashFlowCategoryMenuText => ResString.GetMultilingualString("4e137356-3f54-47d5-bdaf-4140916c6dcd", "Override Cash Flow Category");

		protected MultilingualString ViewingRestrictionOutsideLoginWarningMessage => ResString.GetMultilingualString("18472059-80DE-4DEA-9E8A-24B0C041A64C", "Transaction created in branch / dept outside your login permission are not listed.");

		protected MultilingualString Caption => ResString.GetMultilingualString("a282dffe-956f-457d-bcdb-6f6c067ec08e", "Search Results");

		protected MultilingualString ResetStatusToQueuedText => ResString.GetMultilingualString("9255a192-dda7-4a63-b03a-fa3ad76f9055", "Reset Status to Queued");
		protected MultilingualString ResetStatusToDeliveredText => ResString.GetMultilingualString("73ff7386-bef4-4b1a-a2cd-70aa2d351e77", "Reset Status to Delivered");
		protected MultilingualString SetStatusToAwaitText => ResString.GetMultilingualString("EE23B803-6E9D-43FA-BCA2-233A438FB8E0", "Awaiting Review");

		protected MultilingualString AuthorizeAndSendText => ResString.GetMultilingualString("D66D6125-2E32-49D9-996D-9E21F8F74C1E", "Authorize and Send");

		protected ResourceStringData ViewMatchedTransactionsMenuItemText => Res.GetData("c63b86ce-e848-44ca-afc6-cb55e0c3e8fe", "View Matched Transactions");

		protected ResourceStringData OverrideMatchStatusMenuText => Res.GetData("c8793d99-1728-4a78-b20d-51446fd759cc", "Override Match Status and Reason");

		protected abstract string Ledger { get; }

		DataImporterForm GetDataImporterForm(DataImporterBusinessObject businessEntity, string formCaption, BillingInterfaceName interfaceName)
		{
#if DEBUG
			return ObjectFactory.New<IDataImporterFormForTestOnly>(businessEntity, formCaption, interfaceName).ConvertToDataImporterForm();
#else
			return new DataImporterForm(businessEntity, formCaption, interfaceName);
#endif
		}

		ITransactionModuleStripPresentationProvider TransactionModuleStripPresentationProvider => transactionModuleStripPresentationProvider ?? (transactionModuleStripPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetTransactionModuleStripPresentationProvider());
		ITransactionModuleStripPresentationProvider transactionModuleStripPresentationProvider;

#if DEBUG
		#region Test Only Interface

		public interface IDataImporterFormForTestOnly
		{
			void ImportFromFileExposed(ZString fileName);
			DataImporterForm ConvertToDataImporterForm();
		}

		#endregion
#endif

		#endregion
	}
}
