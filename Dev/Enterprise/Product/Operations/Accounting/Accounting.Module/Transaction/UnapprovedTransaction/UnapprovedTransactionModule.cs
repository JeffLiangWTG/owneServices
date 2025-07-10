using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.UnapprovedAPTransaction;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class UnapprovedTransactionModule : FilterGridModuleWithMultipleReversing
	{
		#region Overriden methods

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> menus = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			menus.Add(new ZMenuItem(ResString.GetMultilingualString("96f3d947-1354-41b0-b1c7-465b4f167bf5", "Print Transaction"), new EventHandler(HandlePrint)));
			menus.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.TransactionApproval.Approve", "Approve"), new EventHandler(UAInvoicesModule_Approve_Click)));
			return menus.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menus = new List<MenuItem>(base.GetNewActionMenuItems());
			menus.Add(new ZMenuItem("-"));
			menus.Add(new ZMenuItem(ResString.GetMultilingualString("c790d5a9-9c91-48d9-9ffd-bd28cf208f5f", "Already Posted"), new EventHandler(HandleAlreadyPosted)));
			return menus.ToArray();
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("af77930a-d71d-418b-a6d1-798aa482d8dc", "Cancel", "Cancels the selected item after viewing its details read-only (shortcut Del)");
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewInvoiceMenuText, new EventHandler(HandleNewInvoice)));
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewCreditNoteMenuText, new EventHandler(HandleNewCreditNote)));
			}

			return menuItems.ToArray();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var selectedHeader = selectedBusinessObject as AccTransactionHeader;

			if (selectedHeader != null)
			{
				return GetNewControllerFromCreator(selectedHeader);
			}

			return ZControllerFactory.Create(ControllerIDs.UAInvoice);
		}

		protected virtual ZController GetNewControllerFromCreator(AccTransactionHeader selectedHeader)
		{
			return AccountingControllerCreator.GetNewController(selectedHeader);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UnapprovedTransactionFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new UnapprovedTransactionFilterStripControl(GridCollection, (UnapprovedTransactionFilterStripBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new UnApprovedFilteredTransactionHeaderCollectionView(Candidates);
		}

		protected override ZQuery GetQueryForCollection(IBusinessObjectCollection collection)
		{
			return base.GetQueryForCollection(GetFilteredCollection(collection));
		}

		protected override void PushItemsIntoCollectionCore(IBusinessObjectCollection collection, PerformSearchResult searchResult, SortInfo sort)
		{
			base.PushItemsIntoCollectionCore(GetFilteredCollection(collection), searchResult, sort);
		}

		static IBusinessObjectCollection GetFilteredCollection(IBusinessObjectCollection collection)
		{
			var filteredCollection = collection as UnApprovedFilteredTransactionHeaderCollectionView;

			IBusinessObjectCollection originalCollection;
			if (filteredCollection == null)
			{
				originalCollection = collection;
			}
			else
			{
				originalCollection = filteredCollection.CollectionToFilter;
			}

			return originalCollection;
		}

		protected override FilteredGridLoader CreateSearchManager()
			=> new UnapprovedTransactionModuleGridLoader(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

		class UnapprovedTransactionModuleGridLoader : FilteredGridLoader
		{
			public UnapprovedTransactionModuleGridLoader(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
				: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
			{
			}

			public override int GetEstimatedLoadCount(IBusinessObjectCollection collection, ZQuery query)
			{
				var unapprovedTransactionCandidateCollection = collection as UnapprovedTransactionCandidateCollection;

				return collection.Factory.GetDatabaseCount(typeof(TransactionHeader), unapprovedTransactionCandidateCollection.CompleteFilter.AddToFilter(query));
			}
		}

		protected override bool HasTypeErrorForSelectedBusinessObjects(BusinessObject selectedBusinessObject)
		{
			var filteredCollection = GridCollection as UnApprovedFilteredTransactionHeaderCollectionView;
			IBusinessObjectCollection originalCollection = null;
			originalCollection = filteredCollection == null ? GridCollection : filteredCollection.CollectionToFilter;

			var query = new ZQuery(originalCollection.CompleteFilter);
			query.ReLoadExistingRows = true;

			var factory = new BusinessObjectFactory();
			var obj = factory.Load<TransactionPendingAllocation>(selectedBusinessObject.PK);
			return !obj.MatchesFilter(query);
		}

		protected override ZString IncorrectTypeErrorMessage
		{
			get { return Res.GetString("fc8b9672-d6d7-4021-994d-fee19b24cf7d", "The selected transaction is no longer valid. Please refresh the grid and try again."); }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.UnapprovedTransaction; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.APUnapprovedInvoices; }
		}

		protected override void OnBeforePerformSearchCore()
		{
			ResetRelatedTransactionsOnExistingCollectionEntriesBeforeReload();
		}

		protected override void OnAfterPerformSearchCore()
		{
			if (GridCollection is UnApprovedFilteredTransactionHeaderCollectionView && ((UnApprovedFilteredTransactionHeaderCollectionView)GridCollection).CollectionToFilter.Count > GridCollection.Count)
			{
				Globals.Message.ShowWarning(ViewingRestrictionOutsideLoginWarningMessage, Caption);
			}
			base.OnAfterPerformSearchCore();
		}

		protected override ZQuery ExportQuery
		{
			get
			{
				var query = new ZQuery();
				IBusinessObjectCollection originalCollection = null;

				var filteredCollection = GridCollection as UnApprovedFilteredTransactionHeaderCollectionView;
				if (filteredCollection == null)
				{
					originalCollection = GridCollection;
				}
				else
				{
					originalCollection = filteredCollection.CollectionToFilter;
				}
				if (originalCollection != null)
				{
					query = new ZQuery(originalCollection.CompleteFilter);
					if (FilterBusinessObject != null && FilterBusinessObject.Filter != null)
					{
						query.AddToFilter(new ZQuery(FilterBusinessObject.Filter), JoinCondition.And);
					}
				}

				return query;
			}
		}

		protected MultilingualString ViewingRestrictionOutsideLoginWarningMessage
		{
			get { return ResString.GetMultilingualString("18472059-80DE-4DEA-9E8A-24B0C041A64C", "Transaction created in branch / dept outside your login permission are not listed."); }
		}

		protected MultilingualString Caption
		{
			get { return ResString.GetMultilingualString("d438043b-8f59-4e94-8cfe-7c662cab9c38", "Search Results"); }
		}

		void ResetRelatedTransactionsOnExistingCollectionEntriesBeforeReload()
		{
			TransactionHeaderCollection collection = GridCollection as TransactionHeaderCollection;
			if (collection == null && GridCollection is UnApprovedFilteredTransactionHeaderCollectionView)
			{
				collection = ((UnApprovedFilteredTransactionHeaderCollectionView)GridCollection).CollectionToFilter as TransactionHeaderCollection;
			}
			if (collection != null)
			{
				collection.ResetRelatedTransactionsCollection();
			}
		}

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			var errorMessages = PrepareTransactionErrorMessages(selectedBusinessObject);
			if (!string.IsNullOrEmpty(errorMessages))
			{
				ShowModifiedTransactionErrorMessage(errorMessages);
				return null;
			}
			return base.ShowEditForm(selectedBusinessObject);
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			var errorMessages = PrepareTransactionErrorMessages(selectedBusinessObject);
			if (!string.IsNullOrEmpty(errorMessages))
			{
				ShowModifiedTransactionErrorMessage(errorMessages);
				return null;
			}
			return base.ShowDeleteForm(selectedBusinessObject);
		}

		protected override void DeleteMultiple(BusinessObject[] selectedBusinessObjects)
		{
			var errorMessages = PrepareTransactionErrorMessages(selectedBusinessObjects);
			if (!string.IsNullOrEmpty(errorMessages))
			{
				ShowModifiedTransactionErrorMessage(errorMessages);
				return;
			}
			base.DeleteMultiple(selectedBusinessObjects);
		}

		protected override IZForm ShowTemplateCopyForm(BusinessObject selectedBusinessObject)
		{
			var errorMessages = PrepareTransactionErrorMessages(selectedBusinessObject);
			if (!string.IsNullOrEmpty(errorMessages))
			{
				ShowModifiedTransactionErrorMessage(errorMessages);
				return null;
			}
			return base.ShowTemplateCopyForm(selectedBusinessObject);
		}

		#endregion

		#region Event Handlers

		protected void HandleNewCreditNote(object sender, EventArgs e)
		{
			HandleNew(TransactionTypes.UACreditNote);
		}

		protected void HandleNewInvoice(object sender, EventArgs e)
		{
			HandleNew(TransactionTypes.UAInvoice);
		}

		void HandleNew(string transactionType)
		{
			HandleShowingFormSafely(() => HandleNewCore(transactionType));
		}

		protected virtual void HandleNewCore(string transactionType)
		{
			ControllerFromTransactionType(transactionType).ShowNewForm();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionToFilter", Justification = "Baseline")]
		UnapprovedTransactionConverter GetUnapprovedTransactionConverter(BusinessObject[] selectedObjects)
		{
			ZQuery additionalFilter = new ZQuery(AccTransactionHeaderSchema.AH_IsCancelled, false);
			additionalFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.NotEqual, LedgerTypes.UnapprovedPayableTransactions);
			additionalFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			ZQuery aH_PKsFilter = new ZQuery();
			bool areUAClaimCreditNotesSelected = false;
			bool areUACancelledTransactionsSelected = false;
			if (selectedObjects.Length > 0)
			{
				foreach (InvoicingBase transaction in selectedObjects)
				{
					if (transaction.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
					{
						if (IsTransactionAttachedToClaim(transaction))
						{
							areUAClaimCreditNotesSelected = true;
						}
						if (transaction.AH_IsCancelled)
						{
							areUACancelledTransactionsSelected = true;
						}
					}
					aH_PKsFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.PK, transaction.PK);
				}
			}
			else
			{
				foreach (InvoicingBase transaction in GridCollection)
				{
					if (transaction.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
					{
						if (IsTransactionAttachedToClaim(transaction))
						{
							areUAClaimCreditNotesSelected = true;
						}
						if (transaction.AH_IsCancelled)
						{
							areUACancelledTransactionsSelected = true;
						}
						if (areUAClaimCreditNotesSelected && areUACancelledTransactionsSelected)
						{
							break;
						}
					}
				}
			}
			StringBuilder warningMessageBuilder = new StringBuilder();
			if (areUAClaimCreditNotesSelected)
			{
				additionalFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, SQLComparisonOperator.NotEqual, Constants.TransactionCategory.Codes.ClaimRelated);

				warningMessageBuilder.AppendLine(Res.GetString("2f8d74bb-9584-412f-b3a4-9dde76e4c920", @"One or more of the transactions selected for approval are attached to an outstanding claim.
Those transactions will be omitted from this Transaction Approval Session.
Transactions attached to an outstanding claim are approved / rejected through the Claims and Queries module itself."));
			}
			if (areUACancelledTransactionsSelected)
			{
				if (warningMessageBuilder.Length != 0)
				{
					warningMessageBuilder.AppendLine();
				}
				warningMessageBuilder.AppendLine(Res.GetString("f2eb5ad2-78ea-4d16-8f6f-e68865c8a49f", @"One or more of the transactions selected for approval are rejected.
Those transactions will be omitted from this Transaction Approval Session."));
			}
			if (areUAClaimCreditNotesSelected || areUACancelledTransactionsSelected)
			{
				Globals.Message.ShowInformation(warningMessageBuilder.ToString());
			}
			additionalFilter.AddToFilter(aH_PKsFilter, JoinCondition.And);

			return new UnapprovedTransactionConverter(new BusinessObjectFactory(), additionalFilter);
		}

		void UAInvoicesModule_Approve_Click(object sender, EventArgs e)
		{
			BusinessObject[] selectedObjects = GetSelectedBusinessObjects();
			var errorMessages = PrepareTransactionErrorMessages(selectedObjects);
			if (string.IsNullOrEmpty(errorMessages))
			{
				UnapprovedTransactionConverter converter = GetUnapprovedTransactionConverter(selectedObjects);
				ZFormModaliser.Show(new UnapprovedTransactionAuthorisationForm(converter), Grid.FindForm());
			}
			else
			{
				ShowModifiedTransactionErrorMessage(errorMessages);
			}
		}

		bool IsTransactionAttachedToClaim(InvoicingBase transaction)
		{
			return transaction != null && transaction.AH_TransactionCategory == Constants.TransactionCategory.Codes.ClaimRelated && transaction is UACreditNote;
		}

		protected void HandlePrint(object sender, EventArgs e)
		{
			TransactionHeader currentHeader = CurrentBusinessObjectInGrid as TransactionHeader;

			if (currentHeader != null && currentHeader.Header != null)
			{
				var errorMessages = PrepareTransactionErrorMessages(CurrentBusinessObjectInGrid);
				if (!string.IsNullOrEmpty(errorMessages))
				{
					ShowModifiedTransactionErrorMessage(errorMessages);
					return;
				}
				else
				{
					if (currentHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						Globals.Message.ShowInformation(Res.GetString("d64da673-598c-4144-891b-0c7ba3e5bca2", "You cannot print sister company AR transactions"));
						return;
					}
					else if (!currentHeader.Header.OH_IsActive)
					{
						Globals.Message.ShowInformation(Res.GetString("e305d8fd-d225-4cff-9ac1-c3866096c76f", "This transaction cannot be printed because it is for an inactive organization"), Res.GetString("0243dcd5-ea45-494e-bff3-4080e7de33ba", "Print Transaction"));
					}
					else
					{
						SecurityCheckpoint securityCheckPoint = Env.Security.PrintPayableTransactions;
						if (securityCheckPoint == null || securityCheckPoint.IsAllowed)
						{
							InvoicePrintHelper.PrintCostConfirmationDocument(CurrentBusinessObjectInGrid as TransactionHeader);
						}
						else
						{
							Globals.Message.Show(securityCheckPoint.ErrorMessageForNotAllowed, Res.GetString("4c792c14-cb09-4b3d-a471-77ec964ab617", "Access Denied"), MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
				}
			}
		}

		void HandleAlreadyPosted(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Any())
			{
				if (!Env.Security.APUnapprovedInvoicesFlagInvoiceAsAlreadyPosted.IsAllowed)
				{
					Env.Security.APUnapprovedInvoicesFlagInvoiceAsAlreadyPosted.ShowError();
					return;
				}
				BusinessObject[] selectedObjects = GetSelectedBusinessObjects();

				var errorMessages = PrepareTransactionErrorMessages(selectedObjects);
				if (!string.IsNullOrEmpty(errorMessages))
				{
					ShowModifiedTransactionErrorMessage(errorMessages);
					return;
				}
				if (selectedObjects.Any(x => HasTypeErrorForSelectedBusinessObjects(x)))
				{
					ShowIncorrectTypeErrorMessage();
					return;
				}
				var invoicePKs = Grid.SelectedElements.Select(x => x.PK);
				var factoryForMarkingInvoiceAsAlreadyPosted = new BusinessObjectFactory();

				var filter = new ZQuery(AccTransactionHeaderSchema.PK, invoicePKs);
				filter.OrderBy = AccTransactionHeaderSchema.Constants.AH_TransactionNum + " ASC";
				var reloadedInvoices = factoryForMarkingInvoiceAsAlreadyPosted.Load<TransactionHeader>(filter);

				var marker = new AlreadyPostedInterCompanyInvoiceMarker(reloadedInvoices);
				ZString errorMessage = marker.GetErrorMessageForIncorrectSelection();
				if (!errorMessage.IsEmpty)
				{
					Globals.Message.ShowError(errorMessage);
				}
				else
				{
					var result = Globals.Message.Show(marker.GetMessageForConfirmationPrompt(), Res.GetString("8f96a969-ec4c-4bc3-9bb5-b815015f8930", "Already Posted."), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result == DialogResult.Yes)
					{
						marker.MarkAsAlreadyPosted();
						factoryForMarkingInvoiceAsAlreadyPosted.Save();
						PerformSearch();
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("4d457909-2526-4453-9cfe-51cee2e8471f", "No Invoice is selected to mark as Already Posted."), Res.GetString("fc28f0ed-2e6f-4bcd-b1fd-20e753a1fdcd", "No record selected"));
			}
		}

		#endregion

		#region Implementation

		protected ZController ControllerFromTransactionType(string transactionType)
		{
			return AccountingControllerCreator.GetNewController(transactionType, LedgerTypes.UnapprovedPayableTransactions);
		}

		protected MultilingualString NewInvoiceMenuText
		{
			get { return ResString.GetMultilingualString("49f79681-930d-42b1-b4a6-3e28093ec5dc", "New I&nvoice"); }
		}
		protected MultilingualString NewCreditNoteMenuText
		{
			get { return ResString.GetMultilingualString("b532d146-a983-41fe-832a-51b10388b83f", "New Cre&dit Note"); }
		}
		protected string RejectMenuItemText
		{
			get { return Res.GetString("bd4b30a8-3dc0-423f-94ff-e31e02ae4e43", "Reject"); }
		}

		UnapprovedTransactionCandidateCollection Candidates
		{
			get { return candidates ?? (candidates = new UnapprovedTransactionCandidateCollection(Factory)); }
		}
		UnapprovedTransactionCandidateCollection candidates;

		protected string PrepareTransactionErrorMessages(params BusinessObject[] objects)
		{
			ZStringBuilder result = new ZStringBuilder();
			if (objects != null)
			{
				var transactionsErrorMessages = from transaction in objects.Cast<TransactionHeader>()
												where transaction.IsInstanceLedgerModified()
												select string.Format("{0}, {1}, {2}", transaction.AH_TransactionType, transaction.AH_TransactionNum, transaction.Header != null ? transaction.Header.OH_Code : ZString.Empty);
				result = new ZStringBuilder(transactionsErrorMessages);
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		protected void ShowModifiedTransactionErrorMessage(string errorMessages)
		{
			if (!string.IsNullOrEmpty(errorMessages))
			{
				ZStringBuilder errorBuilder = new ZStringBuilder(Res.GetString("d20fcad9-46ab-473c-82ed-5aef8fa3762c", "Some of the selected records were modified so that they can't be processed in this module:"));
				errorBuilder.Append(errorMessages);
				Globals.Message.ShowError(errorBuilder.ToStringWithNewLineBetweenAppends(), Res.GetString("058a246d-8150-4b28-bf11-fe1f79f4f263", "Error"));
			}
		}

		#endregion
	}
}
