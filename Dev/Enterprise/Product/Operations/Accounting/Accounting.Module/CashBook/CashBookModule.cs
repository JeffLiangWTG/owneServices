using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.DataExport;
using Enterprise.Accounting.DataTransfer.DirectReceiptPayment;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.DataExport;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module for CashBook.
	/// </summary>
	public partial class CashBookTransactionModule : FilterGridModuleWithMultipleReversing
	{
		static string CannotPrintOpeningReceipt => Res.GetString("d6110405-9d4f-43d2-93db-913f8e16f45e", "You cannot print Opening Receipt transactions.");

		static string CannotPrintOpeningPayment => Res.GetString("643ba27c-e488-4653-a31d-46a6f63c0d1f", "You cannot print Opening Payment transactions.");

		static string CannotPrintExchangeDifference => Res.GetString("ce61dc2a-708b-4147-b013-0a96aa3b15fe", "You cannot print Exchange Difference transactions.");

		static string CannotCopyTransaction => Res.GetString("ccf26749-aba6-496d-918b-78d286b6fa9a", "The copy function is only used for AR Invoice, AP Invoice, Bank Transfer, Direct Receipt and Direct Payment at this point");
		
		#region Overrides

		public override ModuleIdentifier ID => ModuleIDs.CashbookTransaction;

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController controller = null;
			if (selectedBusinessObject != null)
			{
				AccTransactionHeader transaction = selectedBusinessObject as AccTransactionHeader;
				if (transaction != null)
				{
					controller = AccountingControllerCreator.GetNewController(transaction, ID);
				}
			}

			return controller ?? AccountingControllerCreator.GetNewController(TransactionTypes.DirectPayment, LedgerTypes.CashBook);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CashBookFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CashbookTransactionCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CashBookFilterBusinessObject();
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(Res.GetString("000635ed-dd03-47c1-9c4b-6c1402df41fe", "Cashbook Transactions"), ImportTransactionXMLEventHandler);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CashBookTransactions;

		#endregion

		#region Menu Items

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (NewMenuItem != null)
			{
				var newIndex = menuItems.IndexOf(NewMenuItem);
				NewMenuItem = new ZMenuItem(Res.GetData("MenuItem.New.V2", "New", "Creates a new Item"), IconTypes.NewButtonActive, IconTypes.NewButtonRest);
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewOpeningReceiptText, new EventHandler(HandleNewOpeningReceipt)));
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewOpeningPaymentText, new EventHandler(HandleNewOpeningPayment)));
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewBankCurrencyAdjustmentText, new EventHandler(HandleNewBankCurrencyAdjustment)));
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewBankTransferText, new EventHandler(HandleNewBankTransfer)));
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewDirectReceiptText, new EventHandler(HandleNewDirectReceipt)));
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewDirectPaymentText, new EventHandler(HandleNewDirectPayment)));
				menuItems[newIndex] = NewMenuItem;
			}

			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			if (DataTransferMenuItem != null)
			{
				menuItems.Remove(DataTransferMenuItem);
			}

			menuItems.Add(new ZMenuItem(PrintMenuItemText, new EventHandler(HandlePrint)));
			menuItems.Add(new ZMenuItem(ReprintMenuItemText, HandleReprint));
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.CashBook.ReallocateCheckNumber", "Re-Allocate Check Number"), new EventHandler(HandleReallocateCheckNumber)));
			RegenerateJournalEntriesHelper.AddRegenerateJournalEntriesMenuItemIfAllowed(menuItems, HandleRegenerateJournalEntries);
			menuItems.Add(new ZMenuItem("-"));
			menuItems.Add(new ZMenuItem(AccountingConstants.AuditAndCashActionText.AuditTransactionText, (sender, e) => AccountingAuditHelper.HandleAuditTransaction(this, new AuditAndCashEventArgs(AuditSecurityCheckpoint, SelectedTransactions))));
			menuItems.Add(new ZMenuItem(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText, (sender, e) => AccountingAuditHelper.HandleUndoAuditTransaction(this, new AuditAndCashEventArgs(UndoAuditSecurityCheckpoint, SelectedTransactions))));
			menuItems.Add(new ZMenuItem("-"));
			menuItems.Add(new ZMenuItem(AccountingConstants.AuditAndCashActionText.RecordCashierText, (sender, e) => AccountingAuditHelper.HandleRecordCashier(this, new AuditAndCashEventArgs(RecordCashierSecurityCheckpoint, SelectedTransactions))));
			menuItems.Add(new ZMenuItem(AccountingConstants.AuditAndCashActionText.ClearCashierText, (sender, e) => AccountingAuditHelper.HandleClearCashier(this, new AuditAndCashEventArgs(ClearCashierSecurityCheckpoint, SelectedTransactions))));

			if (IsCurrentCompanyUSAOrCanada)
			{
				menuItems.Add(new ZMenuItem(ExportPositivePayMenuItemText, new EventHandler(HandleExportPositivePay)));
				menuItems.Add(new ZMenuItem(CustomizePositivePayExportMenuItemText, new EventHandler(HandleCustomizePositivePayExport)));
			}
			else
			{
				menuItems.Add(new ZMenuItem(ExportCheckPaymentsMenuItemText, new EventHandler(HandleExportPositivePay)));
				menuItems.Add(new ZMenuItem(CustomizeCheckPaymentsExportMenuItemText, new EventHandler(HandleCustomizePositivePayExport)));
			}

			menuItems.Add(new ZMenuItem(AccountingJournalPrintHelper.PrintAccountingJournalText, new EventHandler(HandlePrintAccountingJournal)));

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
			{
				menuItems.Add(new ZMenuItem(PrintAccountingVoucherText, new EventHandler(HandlePrintAccountingVoucher)));
			}

			if (DataTransferMenuItem != null)
			{
				menuItems.Add(DataTransferMenuItem);
			}

			return menuItems.ToArray();
		}

		#endregion

		#region Event Handlers

		protected void HandleReprint(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("1d62ef3f-aac2-4bb3-972f-d439e161c2e7", "Please select transaction(s) to print"));
			}
			else if (!Env.Security.ReprintReallocateCheque.IsAllowed)
			{
				Env.Security.ReprintReallocateCheque.ShowError();
			}
			else
			{
				ReallocateCheckNumbers(true);
			}
		}

		protected virtual void HandlePrintAccountingVoucher(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("F5698F27-8E12-45AE-AC83-8DDE240FEE09", "Please select transaction(s) to print"));
			}
			else
			{
				AccountingVoucherPrintHelper accountingVoucherPrintHelper = new AccountingVoucherPrintHelper();
				TransactionHeader[] gridSelections = Grid.GetSelectedElements<TransactionHeader>();
				TransactionHeader[] transactions = ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher(gridSelections);
				PrintTask task = accountingVoucherPrintHelper.GetAccountingVoucherPrintTask(transactions);
				if (task != null)
				{
					task.Run(Env.Security.None);
				}
			}
		}

		static TransactionHeader[] ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher(TransactionHeader[] gridSelections)
		{
			List<TransactionHeader> transactionList = new List<TransactionHeader>(gridSelections);
			var secondTransfers = from x in gridSelections
								  where x.AH_TransactionType == TransactionTypes.Transfer && x.AH_Ledger == LedgerTypes.CashBook && (x.AH_TransactionCount == 2 || x.AH_TransactionCount == 5)
								  select x;
			foreach (var secondTransfer in secondTransfers)
			{
				if (secondTransfer.RelatedTransactions.Count > 0)
				{
					int index = transactionList.IndexOf(secondTransfer);
					TransactionHeader firstTransfer = secondTransfer.RelatedTransactions[0];
					transactionList.Remove(secondTransfer);
					if (!transactionList.Contains(firstTransfer))
					{
						transactionList.Insert(index, firstTransfer);
					}
				}
			}
			TransactionHeader[] transactions = transactionList.ToArray<TransactionHeader>();
			return transactions;
		}

		protected void HandleReallocateCheckNumber(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(ChequeNumberReallocator.NoTransactionIsSelectedToReAllocate);
			}
			else if (!Env.Security.ReprintReallocateCheque.IsAllowed)
			{
				Env.Security.ReprintReallocateCheque.ShowError();
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
				AccChequeBook chequeBook = ((ICanUpdateChequeNumber)headerCollection[0]).ChequeBook;
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
						ZString errorMessage = reallocator.Process(header => new PaymentPrintManager(header.PK.ToGuid(), header.AH_TransactionType, new BusinessObjectFactory()).Print());
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

		protected TransactionHeader[] TransactionsAlreadyExportedForPositivePay(TransactionHeader[] transactionHeaders)
		{
			List<ZGuid> guids = new List<ZGuid>();
			foreach (TransactionHeader header in transactionHeaders)
			{
				guids.Add(header.PK);
			}

			ZQuery query = new ZQuery(GenExportBatchSequenceSchema.XB_ParentID, guids);
			query.AddToFilter(GenExportBatchSequenceSchema.XB_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			query.AddToFilter(GenExportBatchSequenceSchema.XB_Type, Core.Constants.DataExportBatchSubTypes.Codes.PositivePayFile);
			GenExportBatchSequence[] exportBatchSequences = Factory.Load<GenExportBatchSequence>(query);

			List<TransactionHeader> exportedHeaders = new List<TransactionHeader>();
			foreach (GenExportBatchSequence exportBatchSequence in exportBatchSequences)
			{
				exportedHeaders.AddRange(transactionHeaders.Where((x) => { return x.PK == exportBatchSequence.XB_ParentID; }));
			}

			exportedHeaders.Sort((x, y) => { return x.AH_TransactionNum.CompareTo(y.AH_TransactionNum); });

			return exportedHeaders.ToArray();
		}

		protected ZString CheckSelectedTransactionsForPositivePayExport(TransactionHeader[] transactionHeaders)
		{
			ZString errorMessage = ZString.Empty;
			TransactionHeader[] exportedHeaders = TransactionsAlreadyExportedForPositivePay(transactionHeaders);
			if (exportedHeaders.Length > 0)
			{
				errorMessage = IsCurrentCompanyUSAOrCanada ? Res.GetString("1b50f284-894d-4a1d-bb16-65f0a143bcbf", "The following transaction(s) have already been exported for Positive Pay:")
														: Res.GetString("36a5c33b-46f6-4108-8e64-fc823c2a3d39", "The following transaction(s) have already been exported for Check Payment:");
				errorMessage += "\r\n\r\n";
				int count = 1;
				foreach (TransactionHeader header in exportedHeaders)
				{
					if (count <= 10)
					{
						errorMessage += header.AH_TransactionType + " " + header.AH_TransactionNum + "\r\n";
					}
					else
					{
						errorMessage += "\r\n" + Res.GetString("8187cd2f-9992-4c29-afa4-853c0546031a", "Too many transactions to display.  Listing only the first ten transactions.  There are more transactions that have already been exported.") + "\r\n";
						break;
					}
					count++;
				}
				errorMessage += "\r\n" + Res.GetString("01895321-1296-4140-ae71-05006a84b53e", "Please only select transaction(s) to be exported that have not yet been exported.");
			}
			else if (transactionHeaders.Any((x) => { return x.AH_AB != transactionHeaders[0].AH_AB; }) || !transactionHeaders[0].AH_AB.IsValid)
			{
				errorMessage = Res.GetString("3375b0cb-dabb-4f4f-b789-a6d70d18073e", "All selected transactions must have the same Bank Account.");
			}
			else if (transactionHeaders.Any((x) => { return x.AH_TransactionType != TransactionTypes.DirectPayment && x.AH_TransactionType != TransactionTypes.Payment; }))
			{
				errorMessage = Res.GetString("f117af45-ba27-4be7-818e-3e6b74ceee50", "All selected transactions must be Payments or Direct Payments.");
			}
			else if (transactionHeaders.Any((x) => { return x.AH_ReceiptType != ReceiptTypes.Cheque; }))
			{
				errorMessage = Res.GetString("1c5c3239-deac-40b8-9d46-98a10b254a2e", "All selected transactions must have a Receipt / Payment Method of Check ({0}).", ReceiptTypes.Cheque);
			}
			return errorMessage;
		}

		protected void HandleExportPositivePay(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length <= 0)
			{
				Globals.Message.ShowError(Res.GetString("b1591925-6b60-4319-84de-940e22c1d16e", "Select at least one transaction."));
			}
			else
			{
				TransactionHeader[] transactionHeaders = Array.ConvertAll(SelectedBusinessObjects, new Converter<BusinessObject, TransactionHeader>((x) => { return (TransactionHeader)x; }));

				ZString errorMessage = CheckSelectedTransactionsForPositivePayExport(transactionHeaders);

				if (!errorMessage.IsEmpty)
				{
					Globals.Message.ShowError(errorMessage);
				}
				else
				{
					AccBankAccount bankAccount = transactionHeaders[0].BankAccount;
					string unmappedFilePath = string.Empty;
					bool ok = true;
					bool prompt = true;
					PositivePayDataExportAdapter adapter = null;
					BusinessObjectFactory factory = new BusinessObjectFactory();
					adapter = new PositivePayDataExportAdapter(factory, transactionHeaders, bankAccount);
					prompt = !adapter.IsFileNameExpressionUsed;

					if (prompt)
					{
						var saveDialog = new ZSaveFileDialog();
						DialogResult selection = saveDialog.ShowDialog();
						unmappedFilePath = saveDialog.UnmappedFileName;
						if (selection != DialogResult.OK)
						{
							ok = false;
						}
					}
					if (ok)
					{
						try
						{
							string exportErrorMessage = string.Empty;
							if (adapter != null && adapter.CreateFile(unmappedFilePath, out exportErrorMessage))
							{
								Globals.Message.ShowInformation(Res.GetString("d718d7a3-351d-479b-b835-e5dc19fcd96d", "File successfully created at {0}", adapter.LastExportedFullFileName));
							}
							else
							{
								Globals.Message.ShowError(exportErrorMessage);
							}
						}
						catch (UnauthorizedAccessException)
						{
							Globals.Message.ShowError(Res.GetString("0a8f820b-f756-4d1c-8347-169b608b60ff", "Cannot write the file to the disk. Please check with your system administrator."));
						}
					}
				}
			}
		}

		protected void HandleCustomizePositivePayExport(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length != 1)
			{
				Globals.Message.Show(Res.GetString("38504b7a-b5bc-4700-9b1d-1783f80bb879", "Please select only one transaction."));
			}
			else
			{
				TransactionHeader transactionHeader = (TransactionHeader)SelectedBusinessObjects[0];
				TransactionHeader[] transactionHeaders = new TransactionHeader[] { transactionHeader };
				try
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					PositivePayDataExportAdapter adapter = new PositivePayDataExportAdapter(factory, transactionHeaders, transactionHeader.BankAccount);
					adapter.Sort(transactionHeaders);
					IExportCollectionInfo collectionInfo = adapter.GetMultiTypeCollectionInfo(adapter.GetBusinessObjectsForExport(transactionHeaders));

					var key = IsCurrentCompanyUSAOrCanada ? "PositivePay" : "CheckPayment";
					using (DataExportWizardForm form = new DataExportWizardForm(collectionInfo, key))
					{
						ExportWizard exportWizard = form.BusinessEntity as ExportWizard;
						ZQuery query = new ZQuery(StmDataSchema.SD_Owner, transactionHeader.AH_AB);
						query.AddToFilter(StmDataSchema.SD_Name, "PositivePayDataExportSetting");
						StmData data = factory.LoadTop1<StmData>(query);

						if (data != null)
						{
							exportWizard.Setting = data.SD_BinaryValue.ToAscii();
						}

						exportWizard.FileNameExpressionObject = new DataExportPositivePayHeader(factory, transactionHeaders, transactionHeader.BankAccount);

#if DEBUG
						if (Globals.IsTest && IsMockTestCustomizePositivePayExport)
						{
							CustomizePositivePayExportWizardEncodingForTest = exportWizard.FileExportEncoding;
							return;
						}
#endif

						form.ShowDialog();

						if (form.DialogResult == DialogResult.OK)
						{
							if (data == null)
							{
								data = factory.New<StmData>();
							}
							data.SD_Name = "PositivePayDataExportSetting";
							data.SD_Owner = transactionHeader.AH_AB;
							ZBlob blob = ZBlob.FromAscii(exportWizard.Setting);
							data.SD_BinaryValue = blob;
							factory.Save();
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var transactionType = IsCurrentCompanyUSAOrCanada ? Res.GetString("24cfcbae-e13b-4701-9675-73cc83e11a80", "Positive Pay") : Res.GetString("7640058a-71b8-43f3-83a0-096e0393aa5c", "Check Payment");
					Globals.Message.ShowError(Res.GetString("a3094e4d-dc44-4da2-84fa-b0620a85f850", "Error customizing the {0} export of Bank Account: {1}", transactionType, transactionHeader != null && transactionHeader.BankAccount != null ? transactionHeader.BankAccount.AB_Code : ZString.Empty));
				}
			}
		}

#if DEBUG
		internal Encoding CustomizePositivePayExportWizardEncodingForTest;
		internal bool IsMockTestCustomizePositivePayExport;
#endif

		void HandleNewOpeningReceipt(object sender, EventArgs e)
		{
			HandleNew(TransactionTypes.OpeningReceipt);
		}

		void HandleNewOpeningPayment(object sender, EventArgs e)
		{
			HandleNew(TransactionTypes.OpeningPayment);
		}

		void HandleNewBankCurrencyAdjustment(object sender, EventArgs e)
		{
			HandleNew(TransactionTypes.ExchangeDifference);
		}

		void HandleNewBankTransfer(object sender, EventArgs e)
		{
			HandleNew(TransactionTypes.Transfer);
		}

		void HandleNewDirectReceipt(object sender, EventArgs e)
		{
			HandleNew(TransactionTypes.DirectReceipt);
		}

		void HandleNewDirectPayment(object sender, EventArgs e)
		{
			HandleNew(TransactionTypes.DirectPayment);
		}

		void HandlePrint(object sender, EventArgs e)
		{
			TransactionHeader currentHeader = CurrentBusinessObjectInGrid as TransactionHeader;
			if (currentHeader != null && CheckSecurity(currentHeader))
			{
				ZString transactionType = currentHeader.AH_TransactionType;

				switch (transactionType)
				{
					case TransactionTypes.Payment:
					case TransactionTypes.DirectPayment:
					case TransactionTypes.Transfer:
						PaymentPrintManager printManager = new PaymentPrintManager(currentHeader.PK.ToGuid(), transactionType, new BusinessObjectFactory());
						printManager.Print();
						break;

					case TransactionTypes.DirectReceipt:
					case TransactionTypes.Receipt:
						Business.ARAP.ReceiptPayment.ReceiptPrint printHandler = new Business.ARAP.ReceiptPayment.ReceiptPrint();
						printHandler.PrintReceiptMatchingReport(currentHeader, AccountingUtils.AccountingDocumentTitles.ReceiptJournal);
						break;

					case TransactionTypes.OpeningReceipt:
						Globals.Message.ShowInformation(CannotPrintOpeningReceipt);
						break;
					case TransactionTypes.OpeningPayment:
						Globals.Message.ShowInformation(CannotPrintOpeningPayment);
						break;
					case TransactionTypes.ExchangeDifference:
						Globals.Message.ShowInformation(CannotPrintExchangeDifference);
						break;
				}
			}
		}

		void HandlePrintAccountingJournal(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("50d6c0a3-c219-44a1-b328-1105b6307da7", "Please select transaction(s) to print"));
			}
			else
			{
				var gridSelections = SelectedBusinessObjects.Cast<TransactionHeader>();
				AccountingJournalPrintHelper.PrintAccountingJournal(gridSelections);
			}
		}

		void HandleNew(string transactionType)
		{
			HandleShowingFormSafely(() => HandleNewCore(transactionType));
		}

		void HandleNewCore(string transactionType)
		{
			AccountingControllerCreator.GetNewController(transactionType, LedgerTypes.CashBook).ShowNewForm();
		}

		#region Audit & Undo Audit

		AccTransactionHeader[] SelectedTransactions => Grid.GetSelectedElements<AccTransactionHeader>();

		SecurityCheckpoint AuditSecurityCheckpoint => Env.Security.CashBookAuditTransaction;

		SecurityCheckpoint UndoAuditSecurityCheckpoint => Env.Security.CashBookUndoAuditTransaction;

		SecurityCheckpoint RecordCashierSecurityCheckpoint => Env.Security.CashBookRecordCashier;

		SecurityCheckpoint ClearCashierSecurityCheckpoint => Env.Security.CashBookClearCashier;

		#endregion
		#endregion

		protected void ImportTransactionXMLEventHandler(object sender, EventArgs args)
		{
			if (!Env.Security.ImportXmlAccountingTransactions.IsAllowed)
			{
				Env.Security.ImportXmlAccountingTransactions.ShowError();
			}
			else
			{
				DirectReceiptPaymentDataAdapter dataAdapter = new DirectReceiptPaymentDataAdapter();
				XmlDataTransferDirector transferDirector = new XmlDataTransferDirector(dataAdapter, true);
				transferDirector.PromptUserAndImport(BillingInterfaceName.CashbookTransactionsXmlImport);
			}
		}

		bool CheckSecurity(TransactionHeader currentHeader)
		{
			bool result = true;
			string errorMessage = "";

			if (currentHeader != null)
			{
				ZString transactionType = currentHeader.AH_TransactionType;

				switch (transactionType)
				{
					case TransactionTypes.Receipt:
						result = Env.Security.PrintCashBookReceipt.IsAllowed;
						if (!result)
						{
							errorMessage = Env.Security.PrintCashBookReceipt.ErrorMessageForNotAllowed;
						}
						break;
					case TransactionTypes.Payment:
						result = Env.Security.PrintCashBookPayment.IsAllowed;
						if (!result)
						{
							errorMessage = Env.Security.PrintCashBookPayment.ErrorMessageForNotAllowed;
						}
						break;
					case TransactionTypes.DirectReceipt:
						result = Env.Security.PrintCashBookDirectReceipt.IsAllowed;
						if (!result)
						{
							errorMessage = Env.Security.PrintCashBookDirectReceipt.ErrorMessageForNotAllowed;
						}
						break;
					case TransactionTypes.DirectPayment:
						result = Env.Security.PrintCashBookDirectPayment.IsAllowed;
						if (!result)
						{
							errorMessage = Env.Security.PrintCashBookDirectPayment.ErrorMessageForNotAllowed;
						}
						break;
					case TransactionTypes.Transfer:
						result = Env.Security.PrintCashBookBankTransfer.IsAllowed;
						if (!result)
						{
							errorMessage = Env.Security.PrintCashBookBankTransfer.ErrorMessageForNotAllowed;
						}
						break;
					case TransactionTypes.OpeningReceipt:
						result = Env.Security.PrintCashBookOpeningReceipt.IsAllowed;
						if (!result)
						{
							errorMessage = Env.Security.PrintCashBookOpeningReceipt.ErrorMessageForNotAllowed;
						}
						break;
					case TransactionTypes.OpeningPayment:
						result = Env.Security.PrintCashBookOpeningPayment.IsAllowed;
						if (!result)
						{
							errorMessage = Env.Security.PrintCashBookOpeningPayment.ErrorMessageForNotAllowed;
						}
						break;
					case TransactionTypes.ExchangeDifference:
						result = Env.Security.PrintCashBookBankCurrencyAdjustment.IsAllowed;
						if (!result)
						{
							errorMessage = Env.Security.PrintCashBookBankCurrencyAdjustment.ErrorMessageForNotAllowed;
						}
						break;
				}
			}

			if (!result)
			{
				Globals.Message.Show(errorMessage, Res.GetString("b861fc52-3e1a-4d72-b246-5831814012a4", "Access Denied"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return result;
		}

		static bool IsCurrentCompanyUSAOrCanada
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada;
			}
		}

		protected MultilingualString PrintMenuItemText = ResString.GetMultilingualString("7fd964e2-8cf6-45c3-a40d-ed72c17461e7", "&Print");
		protected MultilingualString ReprintMenuItemText = ResString.GetMultilingualString("863f340f-a2ae-4967-b82c-6aee39ae8de7", "&Reprint Check/s");
		protected MultilingualString PrintAccountingVoucherText = ResString.GetMultilingualString("4d5fc04c-c175-4169-971b-a0736dcd5068", "Print Accounting Voucher");
		protected MultilingualString ExportPositivePayMenuItemText = ResString.GetMultilingualString("d3a1ef6d-eae2-4e1e-90f7-0250b26360ae", "&Export Positive Pay");
		protected MultilingualString CustomizePositivePayExportMenuItemText = ResString.GetMultilingualString("6746e934-210a-4652-bf71-fbef851b0160", "&Customize Positive Pay Export");
		protected MultilingualString ExportCheckPaymentsMenuItemText = ResString.GetMultilingualString("230c075a-963b-4396-9e12-c7ef3139ac86", "&Export Check Payments");
		protected MultilingualString CustomizeCheckPaymentsExportMenuItemText = ResString.GetMultilingualString("6b94b5b2-9e5d-4b66-80b5-808d57ca740e", "&Customize Export Check Payments");
		protected MultilingualString NewOpeningReceiptText = ResString.GetMultilingualString("ebacb5a7-7b3a-409e-867b-1824c358de96", "New Opening Receipt");
		protected MultilingualString NewOpeningPaymentText = ResString.GetMultilingualString("f448d0af-8e77-4a6d-afba-2cac5a1a8772", "New Opening Payment");
		protected MultilingualString NewBankCurrencyAdjustmentText = ResString.GetMultilingualString("5f409fc9-ed5a-4078-9d88-23e7319ca5f5", "New Bank Currency Adjustment");
		protected MultilingualString NewBankTransferText = ResString.GetMultilingualString("6870a0d6-2349-40c5-a373-e3c3e72410b5", "New Bank Transfer");
		protected MultilingualString NewDirectReceiptText = ResString.GetMultilingualString("82c39f63-36f9-4116-a887-64bd78e7af2c", "New Direct Receipt");
		protected MultilingualString NewDirectPaymentText = ResString.GetMultilingualString("1a06ad27-b840-45b1-b4dd-40923b89468c", "New Direct Payment");
	}
}
