using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.DataTransfer.EInvoicing.Egypt;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.CountryCompliance.ChinaComplianceInfo;

namespace Enterprise.Accounting.Module
{
	public partial class ARTransactionModuleStrip : TransactionModuleStrip
	{
		#region Overrides

		protected override ControllerID ExpectedOpenedMatchFormID
		{
			get { return ControllerIDs.ZARMatching; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ARTransaction; }
		}

		protected override SecurityCheckpoint MarkAsNotPrintedSecurityItem
		{
			get { return Env.Security.MarkInvoiceCreditAsNotPrinted; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ReceivablesTransactions; }
		}

		protected override SecurityCheckpoint AllocateComplianceNumberCheckpoint
		{
			get { return Env.Security.ReceivablesAllocateComplianceNumber; }
		}

		protected override SecurityCheckpoint UpdateComplianceSubTypeOrNumberCheckpoint
		{
			get { return Env.Security.ReceivablesModifyComplianceSubTypeOrNumber; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.ARInvoiceCode; }
		}

		protected override SecurityCheckpoint ImportRemittanceFileSecurityCheckpoint
		{
			get { return Env.Security.ImportRemittanceFileReceivables; }
		}

		protected override SecurityCheckpoint ModifyMatchStatusAndReasonSecurity => Env.Security.ReceivablesModifyMatchStatusAndReason;

		protected override string Ledger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			TransactionFilterStripControl controller = new TransactionFilterStripControl(GridCollection, (ARTransactionFilterStripBusinessObject)FilterBusinessObject);
			controller.FilteredGrid.ColorContextKey = "ARTransactionFilterStripControl";
			return controller;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ARTransactionFilterStripBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var aRTransactionHeaders = new ARTransactionHeaderCollection(Factory);
			return new FilteredTransactionHeaderCollectionView(aRTransactionHeaders);
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
				originalCollection = filteredCollection.CollectionToFilter;
			}
			base.PushItemsIntoCollectionCore(originalCollection, searchResult, sort);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			if (selectedBusinessObject != null)
			{
				return base.GetNewController(selectedBusinessObject);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.ARInvoice);
			}
		}

		protected override void DisplayImportedTransactionForm(InvoicingBase importedTransaction)
		{
			InvoicingBaseController controller = null;

			if (importedTransaction.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
			{
				switch (importedTransaction.AH_TransactionType)
				{
					case TransactionTypes.Invoice:
						controller = (ARInvoiceController)ZControllerFactory.Create(ControllerIDs.ARInvoice);
						break;

					case TransactionTypes.CreditNote:
						controller = (ARCreditNoteController)ZControllerFactory.Create(ControllerIDs.ARCreditNote);
						break;

					case TransactionTypes.AdjustmentNote:
						controller = (ARAdjustmentNoteController)ZControllerFactory.Create(ControllerIDs.ARAdjustmentNote);
						break;
				}
			}

			if (controller != null)
			{
				ShowImportedDataForm(controller, importedTransaction);
			}
			else
			{
				Globals.Message.Show(Res.GetString("11c11fe5-9676-40b8-b743-8aded697195d", "Only Accounts Receivable Invoices, Credit Notes and Adjustment Notes can be imported from this menu"));
			}
		}

		protected override void OnBeforePerformSearchCore()
		{
			ResetRelatedTransactionsOnExistingCollectionEntriesBeforeReload();
		}

		protected override void OnAfterPerformSearchCore()
		{
			if (GridCollection is FilteredTransactionHeaderCollectionView && ((FilteredTransactionHeaderCollectionView)GridCollection).CollectionToFilter.Count > GridCollection.Count)
			{
				Globals.Message.ShowWarning(ViewingRestrictionOutsideLoginWarningMessage, Caption);
			}
			base.OnAfterPerformSearchCore();
		}

		void ResetRelatedTransactionsOnExistingCollectionEntriesBeforeReload()
		{
			TransactionHeaderCollection collection = GridCollection as TransactionHeaderCollection;
			if (collection == null && GridCollection is FilteredTransactionHeaderCollectionView)
			{
				collection = ((FilteredTransactionHeaderCollectionView)GridCollection).CollectionToFilter as TransactionHeaderCollection;
			}
			if (collection != null)
			{
				collection.ResetRelatedTransactionsCollection();
			}
		}

		protected override void PerformLevelAuthorizationForReversal(MultipleReversingProviderForHeader multipleReversingProvider)
		{
			var bizObjNotReversed = SelectedBusinessObjects.Cast<TransactionHeader>().Where(x => !x.AH_IsCancelled);
			var arInvoices = bizObjNotReversed.Where(x => x is ARInvoice);
			var positiveARAdjNotes = bizObjNotReversed.Where(x => x is ARAdjustmentNote).Cast<ARAdjustmentNote>().Where(x => x.AH_LocalTotalAmount > 0);

			if (arInvoices.Any() || positiveARAdjNotes.Any())
			{
				var transactions = arInvoices.Concat(positiveARAdjNotes).Cast<InvoicingBase>().ToArray();
				var approvalGUIProvider = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, null, null).ARCreditNoteApprovalGUIProvider;
				var levelAuthorization = new ARCreditNoteForReversalLevelAuthorizationWithApprovalRequest(approvalGUIProvider, true);
				var authorizationResult = levelAuthorization.PerformLevelAuthorizationForReversing(transactions);
				multipleReversingProvider.Factory.ChildFactories.Add(approvalGUIProvider.FactoryForApprovalRequests);
				multipleReversingProvider.ShouldSaveApprovalFactory = authorizationResult.Item2;
				multipleReversingProvider.ApprovalFactory = approvalGUIProvider.FactoryForApprovalRequests;
				multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.UnionWith(levelAuthorization.TransactionsWithoutApprovedRequest);
			}
		}

		#region Override for Audit Transaction & Record Cashier

		protected override bool CanAddAuditAndCashMenus => true;

		protected override bool CanCreateComplianceDocuemntsMenus => AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value;

		protected override SecurityCheckpoint AuditSecurityCheckpoint => Env.Security.ReceivablesAuditTransaction;

		protected override SecurityCheckpoint UndoAuditSecurityCheckpoint => Env.Security.ReceivablesUndoAuditTransaction;

		protected override SecurityCheckpoint RecordCashierSecurityCheckpoint => Env.Security.ReceivablesRecordCashier;

		protected override SecurityCheckpoint ClearCashierSecurityCheckpoint => Env.Security.ReceivablesClearCashier;

		#endregion

		#region Overrides for Matching

		protected override SecurityCheckpoint MatchCheckpoint
		{
			get { return Env.Security.MatchReceivablesTransactions; }
		}

		protected override MatchingBase GetMatchingObject(BusinessObjectFactory factory)
		{
			return new ARMatchingBase(factory);
		}

		protected override MatchingModule CreateMatchingModuleForViewMatchedTransactions()
			=> new ARMatchingModule();

		#endregion

		#endregion

		#region GetNewActionMenu

		protected override MenuItem[] GetEInvoicingActionMenuItems()
		{
			return EInvoicingGUIActionHelper.GetActionMenuItems();
		}

		protected override MenuItem[] GetQueuePendingInvoiceMenuItems()
		{
			var result = new List<MenuItem>();
			var queuePendingInvoiceMenuItem = EInvoicingGUIActionHelper.GetQueuePendingInvoiceActionMenuItem();
			if (queuePendingInvoiceMenuItem != null)
			{
				result.Add(queuePendingInvoiceMenuItem);
			}
			return result.ToArray();
		}

		protected override IEnumerable<MenuItem> GetLedgerSpecificEInvoicingActionMenuItems()
		{
			if (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) is IEInvoicingSignatureBuilderProvider
				&& AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				yield return new ZMenuItem(SignElectronicInvoiceMenuText, new EventHandler(HandleSignElectronicInvoice));
			}

			var penaltyTaxInfoActionMenuItem = EInvoicingGUIActionHelper.GetPenaltyTaxInfoActionMenuItem();
			if (penaltyTaxInfoActionMenuItem != null)
			{
				yield return penaltyTaxInfoActionMenuItem;
			}
		}

		protected override MenuItem[] GetActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>
			{
				new ZMenuItem("-"),
				new ZMenuItem(NewBulkReceiptsMenuText, HandlePostReceipts),
				new ZMenuItem(BadDebtWriteOffMenuText, HandleWriteOffBadDebt)
			};

			var createCollectionBatchMenuItem = new ZMenuItem(NewCollectionBatchMenuText);
			var groupByDebtorAndDueDateMenuItem = new ZMenuItem(ResString.GetMultilingualString("E1BCF2CF-5631-4E6D-8155-DE34079B866A", "Group By Debtor and Due Date"), new EventHandler(HandleCreateBatchGroupByDebtorAndDueDate));
			var groupByDebtorMenuItem = new ZMenuItem(ResString.GetMultilingualString("F34441BA-BC00-4582-93C7-183FEEACCA4A", "Group By Debtor"), new EventHandler(HandleCreateBatchGroupByDebtor));
			var groupAllSelectedMenuItem = new ZMenuItem(ResString.GetMultilingualString("784FE21B-AC51-4EE2-AE56-0F94E4421478", "Group All Selected"), new EventHandler(HandleCreateBatchGroupAllSelected));
			createCollectionBatchMenuItem.MenuItems.AddRange(new[] { groupByDebtorAndDueDateMenuItem, groupByDebtorMenuItem });
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Brazil)
			{
				createCollectionBatchMenuItem.MenuItems.Add(groupAllSelectedMenuItem);
			}
			menuItems.Add(createCollectionBatchMenuItem);

			if (AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.Value == AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code
				&& Env.Security.ReinstateDailyInvoiceDateIncrementing.IsAllowed
				&& ARDefaultInvoiceAndPostDateCalculator.CheckSuspensionNeedsToBeLifted(ZDateTime.Now, GlbCompany.CurrentCompany))
			{
				menuItems.Add(new ZMenuItem(ReinstateDailyInvoiceDateIncrementingMenuText, new EventHandler(HandleReinstateDailyInvoiceDateIncrementing)));
			}

			if (GlbCompany.CurrentCompany.Country.SupportDocumentSigning)
			{
				menuItems.Add(new ZMenuItem(SignInvoiceWithDigitalSignatureMenuText, new EventHandler(HandleSignInvoiceWithDigitalSignature)));
			}

			if (Equals(ID, ModuleIDs.ARTransaction)
				&& Env.CurrentUser.IsSupportUser
				&& ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) is IMarkIssuedInvoiceAsReversed
				&& AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
			{
				menuItems.Add(new ZMenuItem(MarkIssuedInvoiceAsReversedText, new EventHandler(MarkIssuedInvoiceAsReversed)));
			}

			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			NewMenuItem.MenuItems.Add(new ZMenuItem("-"));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewPeriodicInvoiceMenuText, new EventHandler(HandleNewPeriodicInvoice)));
			NewMenuItem.MenuItems.Add(new ZMenuItem(NewPeriodicInvoiceBulkMenuText, new EventHandler(HandleNewPeriodicInvoiceBulk)));

			return menuItems.ToArray();
		}

		protected void MarkIssuedInvoiceAsReversed(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				var selectedTransactions = SelectedBusinessObjects.OfType<TransactionHeader>();
				if (selectedTransactions.Any())
				{
					var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_AH, selectedTransactions.Select(x => x.PK))
						.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS)
						.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Reference, ComplianceDocumentStatusTypes.CDI.Code);
					var references = Factory.Load<AccTransactionHeaderReference>(query);

					if (references.Length > 0)
					{
						var succeedTransactions = string.Join(", ", references.Select(x => x.TransactionHeader).Select(x => x.AH_TransactionNum.ToString()).OrderBy(x => x));

						var caption = Res.GetString("8A2B08A5-BF6A-45E0-9ABA-94330A1AF9D5", "Confirmation");
						var confirmationString = Res.GetString("6FF04DC9-AED3-4CBD-8D83-97A0CADB7C76", "Yes");
						var question = Res.GetString("D22B943F-7079-4518-9DC6-C0E60158F18F", @"You are trying to reset the compliance document status to CDR, do you want to proceed?

Transaction number: {0}

Do you wish to continue?", succeedTransactions);

						if (Globals.Message.ShowConfirmation(question, caption, confirmationString, MessageBoxIcon.Question, MessageBoxButtons.OKCancel) == DialogResult.OK)
						{
							foreach (var reference in references)
							{
								reference.AH1_Reference = ComplianceDocumentStatusTypes.CDR.Code;
								reference.AH1_Amount = reference.TransactionHeader.AH_InvoiceAmount + reference.TransactionHeader.AH_GSTAmount;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
								reference.TransactionHeader.Logs.AddNew(AutoEvents.EditedARecord, "Compliance document status was manually set to 'CDR', origin value was 'CDI'.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							}

							Factory.Save();

							Globals.Message.ShowInformation(Res.GetString("B58D2C9A-CEA4-4B50-82BB-F57935EE2445", @"The following transaction(s) has been reset to CDR.

Transaction number: {0}", succeedTransactions));
						}
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("B8EEF793-208E-4655-8C46-D0DD634053D2", "No eligible transaction(s) can be mark as reversed."));
					}
				}
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		void HandleNewPeriodicInvoice(object sender, EventArgs e)
		{
			HandleShowingFormSafely(() => HandleNewPeriodicInvoiceCore());
		}

		void HandleNewPeriodicInvoiceCore()
		{
			ZControllerFactory.Create(ControllerIDs.PeriodicInvoice).ShowNewForm();
		}

		#region HandleNewPeriodicInvoiceBulk

		void HandleNewPeriodicInvoiceBulk(object sender, EventArgs e)
		{
			HandleShowingFormSafely(() => HandleNewPeriodicInvoiceBulkCore());
		}

		void HandleNewPeriodicInvoiceBulkCore()
		{
			var periodicInvoiceBulkForm = ZControllerFactory.Create(ControllerIDs.PeriodicInvoiceBulk).ShowNewForm();
			if (periodicInvoiceBulkForm != null)
			{
				periodicInvoiceBulkForm.Closed += new EventHandler(PeriodicInvoiceBulkForm_Closed);
			}
#if DEBUG
			if (Globals.IsTest)
			{
				LastPeriodicInvoiceBulkForm = (PeriodicInvoicingBulkForm)periodicInvoiceBulkForm;
			}
#endif
		}

		void PeriodicInvoiceBulkForm_Closed(object sender, EventArgs e)
		{
			var periodicInvoiceBulkForm = (PeriodicInvoicingBulkForm)sender;
			periodicInvoiceBulkForm.Closed -= new EventHandler(PeriodicInvoiceBulkForm_Closed);
			if (periodicInvoiceBulkForm.DialogResult == DialogResult.OK)
			{
				var periodicInvoiceBulkPoster = periodicInvoiceBulkForm.GeneratePoster();
				var postController = new PeriodicInvoiceBulkPosterController(periodicInvoiceBulkPoster, ParentModalForm ?? LocateMainForm());
				postController.StartPosting();
#if DEBUG
				if (Globals.IsTest)
				{
					LastPostController = postController;
				}
#endif
			}
		}

#if DEBUG
		internal PeriodicInvoicingBulkForm LastPeriodicInvoiceBulkForm;
		internal PeriodicInvoiceBulkPosterController LastPostController;
#endif

		#endregion

		protected MultilingualString NewPeriodicInvoiceMenuText
		{
			get { return ResString.GetMultilingualString("782fa8a5-9983-4e5e-80a8-ff16f31c47d7", "New &Periodic Invoice"); }
		}
		protected MultilingualString NewPeriodicInvoiceBulkMenuText
		{
			get { return ResString.GetMultilingualString("47002fbf-4c94-450f-999a-84f53bf7ce21", "New &Bulk Periodic Invoices"); }
		}

		EInvoicingGUIActionHelper EInvoicingGUIActionHelper
		{
			get => eInvoicingGUIActionHelper ?? (eInvoicingGUIActionHelper = new EInvoicingGUIActionHelper(() => SelectedBusinessObjects.OfType<TransactionHeader>()));
		}
		EInvoicingGUIActionHelper eInvoicingGUIActionHelper;

		#endregion

		#region GetOverrideMenu

		protected override MenuItem[] GetOverrideDetailsMenuItems()
		{
			var menuItems = base.GetOverrideDetailsMenuItems().ToList();
			menuItems.Add(new ZMenuItem(OverrideInvoicePaymentReferenceCodeText, new EventHandler(HandleOverrideInvoiceRemittanceType)));
			return menuItems.ToArray();
		}

		protected MultilingualString OverrideInvoicePaymentReferenceCodeText
		{
			get { return ResString.GetMultilingualString("66465f3e-134b-415f-98f4-1b26cc4ae2b4", "Override Invoice Remittance Type"); }
		}

		protected void HandleOverrideInvoiceRemittanceType(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length > 0)
			{
				if (!Env.Security.ReceivablesModifyInvoiceRemittanceType.IsAllowed)
				{
					Globals.Message.ShowError(Env.Security.ReceivablesModifyInvoiceRemittanceType.ErrorMessageForNotAllowed);
					return;
				}

				foreach (var element in Grid.SelectedElements)
				{
					if (element is TransactionHeader transactionHeader && (transactionHeader.AH_TransactionType != TransactionTypes.Invoice &&
												transactionHeader.AH_TransactionType != TransactionTypes.CreditNote &&
												transactionHeader.AH_TransactionType != TransactionTypes.AdjustmentNote))
					{
						Globals.Message.ShowError(Res.GetString("18850277-5982-490C-B0BC-E12F8A91E966", "The 'Override Invoice Remittance Type' Action menu can only be run for INV, CRD and ADJ transaction types only."));
						return;
					}
				}

				var selectedPKs = Grid.SelectedElements.Select(x => x.PK);
				var selectedInvoice = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, selectedPKs));

				using (var manager = ((IDbConnected)Factory).Connection.BeginTransactionWithManager())
				{
					foreach (var invoicingBase in selectedInvoice)
					{
						invoicingBase.SetInvoiceTransactionReference();

						var orgHeader = Factory.Load<OrgHeader>(invoicingBase.AH_OH);
						orgHeader.CompanyData.GenerateARClientNumber();
					}

					Factory.Save();
					manager.CommitTransaction();
				}

				var helper = new OverrideInvoiceRemittanceTypeHelper(new BusinessObjectFactory(), selectedPKs.ToArray());
				ZFormModaliser.Show(new OverrideInvoiceRemittanceTypeForm(helper), Grid.FindForm());
			}

			else
			{
				Globals.Message.ShowError(Res.GetString("D4F0E019-E133-4EA6-9357-8BDC96089693", "You have not selected any Transactions."));
			}
		}

		#endregion

		#region Event Handlers

		protected override void HandleAllocateSubTypeAndNumberCore()
		{
			var messageForUpdateActionPermissions = ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberManualUpdateToAnyValue(Ledger, GlbCompany.CurrentCompany);
			var errorMessageForARComplianceSubTypeAndNumberUpdate = ElectronicInvoicingUpdateActionPermissions.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(SelectedBusinessObjects.Cast<AccTransactionHeader>().ToArray());

			if (Equals(ID, ModuleIDs.ARTransaction) && !messageForUpdateActionPermissions.IsEmpty)
			{
				Globals.Message.ShowInformation(messageForUpdateActionPermissions);
			}
			else if (!errorMessageForARComplianceSubTypeAndNumberUpdate.IsEmpty)
			{
				Globals.Message.ShowError(errorMessageForARComplianceSubTypeAndNumberUpdate);
			}
			else
			{
				base.HandleAllocateSubTypeAndNumberCore();
			}
		}

		protected override void HandleBulkAllocateComplianceNumberCore()
		{
			if (GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.VietNam && Equals(ID, ModuleIDs.ARTransaction))
			{
				var transactionsToUpdate = SelectedBusinessObjects.Cast<TransactionHeader>().Where(x => x is InvoicingBase invoicingBase && invoicingBase.IsValidTransactionToAllocateInVietnam).ToArray();

				if (transactionsToUpdate.Length == 0)
				{
					Globals.Message.ShowError(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToAllocate);
				}
				else if (transactionsToUpdate.Length < SelectedBusinessObjects.Length)
				{
					Globals.Message.ShowWarning(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToAllocate);
					PrintAndAllocateSequenceNumber(transactionsToUpdate.ToArray());
				}
				else
				{
					PrintAndAllocateSequenceNumber(transactionsToUpdate.ToArray());
				}
			}
			else
			{
				base.HandleBulkAllocateComplianceNumberCore();
			}
		}

		protected virtual void HandlePostReceipts(object sender, EventArgs e)
		{
			ZController postingController = (ARReceiptBatchPostingController)ZControllerFactory.Create(ControllerIDs.ARReceiptBatchPosting);
			postingController.SetCollectionForDefaultsAndValidation(new ARReceiptBatchPosterCollection(postingController.Factory));
			postingController.ShowNewForm();
		}

		protected override void PrintInvoices(TransactionHeader[] headers)
		{
			var transactionsEligibleToPrint = InvoicePrintHelper.GetEligibleForPrintingTransactions(headers);

			if (transactionsEligibleToPrint.Any())
			{
				Print(transactionsEligibleToPrint);
			}

			void Print(IEnumerable<TransactionHeader> headersToPrint)
			{
				try
				{
					new InvoicePrintTask(new InvoicePrintTask.Configuration(headersToPrint.ToArray())).Run();
				}
				catch (UnableToFindInvoiceDocumentCommandException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		protected override SecurityCheckpoint GetSecurityForPrintTransaction(ZString transactionType)
		{
			SecurityCheckpoint securityCheckPoint;

			switch (transactionType)
			{
				case TransactionTypes.AdjustmentNote:
					securityCheckPoint = Env.Security.PrintReceivablesAdjustmentNote;
					break;
				case TransactionTypes.CreditNote:
					securityCheckPoint = Env.Security.PrintReceivablesCreditNote;
					break;
				case TransactionTypes.Invoice:
					securityCheckPoint = Env.Security.PrintReceivablesInvoice;
					break;
				case TransactionTypes.Payment:
					securityCheckPoint = Env.Security.PrintReceivablesPayment;
					break;
				case TransactionTypes.Receipt:
					securityCheckPoint = Env.Security.PrintReceivablesReceipt;
					break;
				default:
					securityCheckPoint = null;
					break;
			}

			return securityCheckPoint;
		}

		protected virtual void HandleSignInvoiceWithDigitalSignature(object sender, EventArgs e)
		{
			var transaction = Grid.SelectedElements.Cast<InvoicingBase>().FirstOrDefault();

			if (Grid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(Res.GetString("5eb4bd3b-da37-4347-a765-914f765f8d18", "Please choose one transaction to sign."));
				return;
			}
			else if (!(transaction is ARInvoice || transaction is ARCreditNote))
			{
				Globals.Message.ShowError(Res.GetString("4672bb93-180c-4cda-9491-4aab534eb071", "Signature is only applicable for AR INV and AR CRD."));
				return;
			}
			else
			{
				if (transaction.IsDigitallySigned)
				{
					Globals.Message.ShowError(Res.GetString("ee97b91f-567f-42a7-b9d0-3f2a2aa9eb62", "This transaction has been signed already, please choose another one."));
				}
				else if (transaction.AH_TransactionReference.IsEmpty)
				{
					Globals.Message.ShowError(Res.GetString("1856d20e-3950-4fe5-81df-3c63e16b2b63", "This transaction does not have a compliance number, please choose another one."));
				}
				else
				{
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);

					(int count, ZString errorMessage) = transactionInNewFactory.SignInvoicesRecursivelyInBackwardDirection();
					if (!errorMessage.IsEmpty)
					{
						Globals.Message.ShowError(errorMessage);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("f291829b-259d-4ac6-ae89-f1a8baefd50c", "Signed {0} invoice(s) successfully.", count));
					}
				}
			}
		}

		protected void HandleSignElectronicInvoice(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("5c30a1ac-98e2-4f19-8312-95720f3f80bd", "Please choose one or more transactions to sign."), SignElectronicInvoiceMenuText);
				return;
			}

			if (Globals.Message.Show(Res.GetString("09ea2f84-59a7-4d4f-9e75-a9beda60c688", @"About to sign {0:N0} transaction(s) for Electronic Invoicing.
Please insert your hardware token now.

Do you wish to continue?", Grid.SelectedElements.Length), SignElectronicInvoiceMenuText, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			(DialogResult result, int successCount, int errorCount, Exception ex) = SignElectronicInvoiceCore(sender, e);
			if (result == DialogResult.OK)
			{
				if (ex == null)
				{
					PerformSearch();
					Globals.Message.ShowInformation(Res.GetString("208abdf2-eedb-4cee-b488-5928749674cb", @"Processing complete.
{0:N0} transaction(s) were signed successfully.
{1:N0} transaction(s) had errors.", successCount, errorCount), SignElectronicInvoiceMenuText);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("48b32eaf-1724-47b6-8039-f666598fdfd9", @"Error signing transactions using hardware token: {0} - {1}

Please refer to Help > Diagnostics > Test Hardware Token Signature.", ex.GetType().Name, ex.Message), SignElectronicInvoiceMenuText);
				}
			}
		}

		void HandleStatusRequest(object sender, EventArgs e)
		{
			var guiActionHelper = new EInvoicingGUIActionHelper(() => SelectedBusinessObjects.OfType<TransactionHeader>());
			guiActionHelper.RequestInvoiceStatusUpdateHandlerAR(sender, e);
		}

		protected virtual (DialogResult result, int successCount, int errorCount, Exception ex) SignElectronicInvoiceCore(object sender, EventArgs e)
		{
			var selectedTransactions = Grid.SelectedElements.Cast<TransactionHeader>();
			using (var progressForm = new ProgressForm())
			{
				progressForm.ShowCancelButton = false;
				progressForm.Status = Res.GetString("470108bb-255a-486f-b510-ca62f8900d68", "Initializing transactions...");
				progressForm.ShowProgressBar = true;
				var eSigningBussinessObject = new ESigningBusinessObject(selectedTransactions, new InvoicingBaseToXmlConverter(), progressForm);
				var result = ZFormModaliser.ShowDialogAndDispose(new GUI.EInvoicing.ESigningForm(eSigningBussinessObject), ParentModalForm ?? LocateMainForm());

				return (result, eSigningBussinessObject.SuccessCount, eSigningBussinessObject.ErrorCount, eSigningBussinessObject.Exception);
			}
		}

		protected virtual void HandleCreateBatchGroupByDebtorAndDueDate(object sender, EventArgs e)
		{
			CreateNewCollectionBatch(AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
		}

		protected virtual void HandleCreateBatchGroupByDebtor(object sender, EventArgs e)
		{
			CreateNewCollectionBatch(AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtor);
		}

		protected virtual void HandleCreateBatchGroupAllSelected(object sender, EventArgs e)
		{
			CreateNewCollectionBatch(AccountingConstants.CreateColletionOrdersBatchOption.GroupAllSelected);
		}

		void CreateNewCollectionBatch(string createOption)
		{
			SecurityCheckpoint securityCheckPoint = Env.Security.CreateCollectionOrderBatch;
			if (securityCheckPoint != null && securityCheckPoint.IsAllowed)
			{
				TransactionHeaderCollection filteredTransactions = FilterSelectedTransactions(Grid.SelectedElements);
				if (filteredTransactions.Count > 0)
				{
					if (!TransactionsValidationsSupportCollectionOrderBatchCreation(filteredTransactions, createOption))
					{
						return;
					}
					ZController postingController = (AccCollectionBatchPostingController)ZControllerFactory.Create(ControllerIDs.AccCollectionBatchPosting);
					postingController.SetCollectionForDefaultsAndValidation(new AccCollectionBatchPosterCollection(postingController.Factory, filteredTransactions, createOption));
#if DEBUG
					formShownInTest =
#endif
					postingController.ShowNewForm();
				}
			}
			else
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
		}

		bool TransactionsValidationsSupportCollectionOrderBatchCreation(TransactionHeaderCollection filteredTransactions, string createOption)
		{
			var listDebtorsNoBank = new Dictionary<string, string>();
			var listPaymentMethodCodes = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetActiveCodeDescriptionPairList().GetAllCodes().ToList();
			var batchCurrency = AccCollectionBatchPoster.CalculateCurrencyToUse(filteredTransactions);

			if (!listPaymentMethodCodes.Any())
			{
				Globals.Message.Show(Res.GetString("0242B7A2-CC43-4184-9CF1-22D3D7E33E7C",
					@"There are no valid agreed payment methods configured for Collection Batches, it is not possible to add transactions to the batch.

Please check the registry setting at {0}.", OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.GetLocation()));

				return false;
			}

			foreach (TransactionHeader transaction in filteredTransactions)
			{
				if (transaction.IsUsedByActiveCollectionOrderLine)
				{
					Globals.Message.Show(Res.GetString("ec56a42f-e852-47f9-9825-8d3d62ba51e8", "Some transactions are already included in active batches. Please revise transactions selection before creating the batch."));
					return false;
				}
				else if (createOption == AccountingConstants.CreateColletionOrdersBatchOption.GroupAllSelected && !IsTransactionINVTypeCRQMethodWithInvRemittanceRefValidForGroupAllSelectedOption(transaction))
				{
					Globals.Message.Show(Res.GetString("F2611AE9-E211-4ED1-B75A-BA31D480BDD1", "Collection Orders in Brazil only support INV Transactions with ‘CRQ’ agreed payment method and with an allocated Invoice Remittance Reference. Please review selected transactions before creating the order."));
					return false;
				}
				else if (createOption != AccountingConstants.CreateColletionOrdersBatchOption.GroupAllSelected)
				{
					if (transaction.AH_TransactionType != TransactionTypes.Invoice
							&& transaction.AH_TransactionType != TransactionTypes.CreditNote
							&& transaction.AH_TransactionType != TransactionTypes.AdjustmentNote
							&& transaction.AH_TransactionType != TransactionTypes.Journal)
					{
						Globals.Message.Show(Res.GetString("CBB6C6A3-F42B-4EA8-8E5C-F2827A82FAF3", "Collection batch only supports type INV, CRD, ADJ, JNL. Please revise transactions selection before creating the batch."));
						return false;
					}
					else if (!listPaymentMethodCodes.Any(x => x == transaction.AH_AgreedPaymentMethodOverride))
					{
						listPaymentMethodCodes.Sort();
						var collectionBatchPaymentMethodList = String.Join("', '", listPaymentMethodCodes);
						var singleAgreedPaymentMethod = Res.GetString("BACA6128-26AC-4D5A-BE4F-CB852765FAD6", "Collection batch only supports INV, CRD, ADJ, JNL with ‘{0}’ agreed payment method. Please revise transactions selection before creating the batch.", collectionBatchPaymentMethodList);
						var manyAgreedPaymentsMethod = Res.GetString("1CBBFB97-8DCB-4E0A-9113-0AAF82AA0940", "Collection batch only supports INV, CRD, ADJ, JNL with ‘{0}’ agreed payment methods. Please revise transactions selection before creating the batch.", collectionBatchPaymentMethodList);
						Globals.Message.Show(listPaymentMethodCodes.Count > 1 ? manyAgreedPaymentsMethod : singleAgreedPaymentMethod);
						return false;
					}
					else
					{
						if (!transaction.OrgHeaderHasPaymentMethod(AccARAccountDetails.ARCollectionRequest, batchCurrency))
						{
							if (!listDebtorsNoBank.ContainsKey(transaction.GovtTaxInvoiceDisplay_OrganisationCode))
							{
								listDebtorsNoBank.Add(transaction.GovtTaxInvoiceDisplay_OrganisationCode, transaction.HeaderFullName);
							}
						}
					}
				}
			}
			if (listDebtorsNoBank.Count > 0)
			{
				int numberDebtorsToBeDisplayed = 5;

				int iDebtorsToDisplayed = listDebtorsNoBank.Count;
				iDebtorsToDisplayed = Math.Min(iDebtorsToDisplayed, numberDebtorsToBeDisplayed);

				var messageListDebtorsNoBank = "";
				var firstFiveSorted = listDebtorsNoBank.OrderBy(pair => pair.Value, StringComparer.Ordinal).Take(numberDebtorsToBeDisplayed);
				foreach (var item in firstFiveSorted)
				{
					messageListDebtorsNoBank += string.Format(System.Globalization.CultureInfo.InvariantCulture, "\r\n{0}\t{1}", item.Key, item.Value);
				}
				ZString messageToBeView = Res.GetString("E00B36FD-8F14-422D-B71E-FD1578848BDC", @"The following debtors do not have a bank account setup with payment method 'CRQ-Collection Request' for the collection batch currency 'EUR'.

Please revise transactions selection or setup bank account against the debtor's organizations record before creating the collection batch.

Code        	Name") + messageListDebtorsNoBank;

				if (listDebtorsNoBank.Count > numberDebtorsToBeDisplayed)
				{
					int numberDebutsNoVisualized = listDebtorsNoBank.Count - numberDebtorsToBeDisplayed;
					ZString middleWhitespace = "\r\n\r\n";
					ZString messageDebtorsOverLimit = middleWhitespace + Res.GetString("E304D2D3-ADD9-485F-9049-0341790B50B2", "Note:\tUnable to display the full list as it contains more than {0} organizations.\r\n\tThere are others {1} organizations outside the list.", numberDebtorsToBeDisplayed, numberDebutsNoVisualized);
					messageToBeView += messageDebtorsOverLimit;
				}
				Globals.Message.Show(messageToBeView);
				return false;
			}
			return true;
		}

		protected virtual void HandleReinstateDailyInvoiceDateIncrementing(object sender, EventArgs e)
		{
			var currentBranchStandardZoneUTCOffset = GlbBranch.CurrentBranch.HomePort.StandardZoneUTCOffset;

			var branchesInDiffTimezone = GlbCompany.CurrentCompany.Branches.Where((branch) => branch.HomePort.StandardZoneUTCOffset != currentBranchStandardZoneUTCOffset);

			var userQuestion = new ZStringBuilder();
			userQuestion.AppendLine(Res.GetString("a09b6061-66cb-4ddc-ac1f-9ef55757cffc", "You are about to Reinstate Daily Invoice Date Incrementing. Reinstating Daily Invoice Dates cannot be reversed."));
			if (branchesInDiffTimezone.Any())
			{
				userQuestion.AppendLine();
				userQuestion.AppendLine(Res.GetString("E5D773D9-5486-4742-9703-1D95557DF5ED", "Note: {0} is currently in a different time zone.", string.Join(",", branchesInDiffTimezone.Select(c => c.GB_BranchName))));
			}
			string caption = Res.GetString("22FC246E-F4F1-4BD6-ADDC-4140F1751E70", "Reinstate Daily Invoice Date Incrementing");
			string confirmationString = Res.GetString("EAEA631A-0F36-40CA-B7D6-BF6096B4FD95", "yes");

			if (Globals.Message.ShowConfirmation(userQuestion.ToString(), caption, confirmationString, MessageBoxIcon.Exclamation) != DialogResult.OK)
			{
				return;
			}

			var logsFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var currentPeriodManagement = new AccountingPeriodCalculator(logsFactory).GetPeriodManagementFromDate(ZDateTime.Now, Env.CurrentCompany.PK);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			currentPeriodManagement?.Logs.AddNew(Events.EditedARecord, "Daily Invoice Date Incrementing Has Been Re-instated");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			logsFactory.Save();

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime());
			(new InvoiceDateIncrementingSuspensionEmail(InvoiceDateIncrementingSuspensionEmail.EmailType.SuspensionLiftedByUser)).Send();
			Globals.Message.Show(Res.GetString("ab889b9b-a656-479b-8d44-10e2b9e0e458", "Daily Invoice Date Incrementing reinstated."));
		}

#if DEBUG
		IZForm formShownInTest;
#endif

		TransactionHeaderCollection FilterSelectedTransactions(BusinessObject[] gridTransactions)
		{
			TransactionHeaderCollection selectedTransactions = new TransactionHeaderCollection(Factory);
			if (gridTransactions.Length > 0)
			{
				foreach (BusinessObject transaction in gridTransactions)
				{
					if (transaction is TransactionHeader && ((TransactionHeader)transaction).AH_TransactionType != ZArchitecture.Core.TransactionTypes.InvoiceBatch)
					{
						if (((TransactionHeader)transaction).AH_FullyPaidDate.IsEmpty)
						{
							selectedTransactions.Add(transaction);
						}
					}
				}
				if (selectedTransactions.Count == 0)
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

		protected virtual void HandleWriteOffBadDebt(object sender, EventArgs e)
		{
			if (CurrentBusinessObjectInGrid != null)
			{
				bool canAllTransactionBeWriteOff = true;
				foreach (BusinessObject bizo in SelectedBusinessObjects)
				{
					if (!(bizo is IBadDebtWritingOff))
					{
						canAllTransactionBeWriteOff = false;
						break;
					}
				}
				if (!canAllTransactionBeWriteOff)
				{
					Globals.Message.ShowInformation(Res.GetString("beab7188-582c-4340-b47c-67280ad7aa97", "You can only post a bad debt write off for Invoices, Credit Notes and Journals."));
				}
				else
				{
					foreach (BusinessObject bizo in SelectedBusinessObjects)
					{
						((IBadDebtWritingOff)bizo).IsWritingOff = true;
					}
					try
					{
						HandleDeleteClick(sender, e);
					}
					finally
					{
						foreach (BusinessObject bizo in SelectedBusinessObjects)
						{
							((IBadDebtWritingOff)bizo).IsWritingOff = false;
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("48f29429-59c4-4efb-a96f-8f64569df57f", "Please select a record in the grid."));
			}
		}

		protected override SecurityCheckpoint ModifyAddressContactForPostedTransactionsSecurity => Env.Security.ReceivablesModifyAddressContactForPosted;

		protected override SecurityCheckpoint ModifyTransactionDescriptionsSecurity => Env.Security.AROverrideTransactionDescription;

		protected override SecurityCheckpoint ModifyCashFlowCategoryForPostedTransactionsSecurity => Env.Security.ReceivablesModifyCashFlowCategoryForPosted;

		protected override SecurityCheckpoint ModifyAgreedPaymentMethodForPostedTransactionsSecurity => Env.Security.ReceivablesModifyAgreedPaymentMethodForPosted;

		protected override SecurityCheckpoint ModifyDueDateForPostedTransactionsSecurity => Env.Security.ReceivablesModifyDueDateForPosted;

		#endregion

		bool IsTransactionINVTypeCRQMethodWithInvRemittanceRefValidForGroupAllSelectedOption(TransactionHeader transactionHeader) =>
			transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable
			&& transactionHeader.AH_TransactionType == TransactionTypes.Invoice
			&& transactionHeader.AH_AgreedPaymentMethodOverride == OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest
			&& !transactionHeader.InvoiceRemittanceReference.IsEmpty;

		#region Implementation

		protected virtual void ShowImportedDataForm(InvoicingBaseController controller, InvoicingBase importedTransaction)
		{
			controller.ShowImportedDataForm(importedTransaction);
		}

		protected static string NewDataToShipnet
		{
			get { return Res.GetString("3dc26d63-e80a-49bb-90d4-e64de6c04a5f", "Data To Shipnet"); }
		}
		protected MultilingualString NewBulkReceiptsMenuText
		{
			get { return ResString.GetMultilingualString("f918b32d-789f-4e8d-974f-8127928510a9", "Bulk Re&ceipts"); }
		}
		protected MultilingualString BadDebtWriteOffMenuText
		{
			get { return AccountingConstants.WriteOffAsBadDebtText; }
		}
		protected MultilingualString NewCollectionBatchMenuText
		{
			get { return ResString.GetMultilingualString("1b24742d-c605-4a21-ae6e-1bcec0a01371", "Create Collection Orders Batch"); }
		}
		protected MultilingualString ReinstateDailyInvoiceDateIncrementingMenuText
		{
			get { return ResString.GetMultilingualString("22ea64a4-9fd1-4dee-a0f5-63a8835298aa", "Reinstate Daily Invoice Date Incrementing"); }
		}
		protected MultilingualString SignInvoiceWithDigitalSignatureMenuText
		{
			get { return ResString.GetMultilingualString("ad129132-c8ba-4805-8edb-9ff14956c72e", "Allocate Digital Signature"); }
		}
		protected MultilingualString SignElectronicInvoiceMenuText
		{
			get { return ResString.GetMultilingualString("709c255c-3a6b-4afe-90b1-891347d2c520", "Sign Electronic Invoice"); }
		}

		protected MultilingualString MarkIssuedInvoiceAsReversedText
		{
			get { return ResString.GetMultilingualString("A8117AD4-B7DC-4760-8112-1DC55C8A49BF", "Mark Issued Invoice as Reversed"); }
		}

		#endregion
	}
}
