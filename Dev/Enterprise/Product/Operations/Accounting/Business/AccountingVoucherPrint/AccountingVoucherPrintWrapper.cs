using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	public class AccountingVoucherPrintWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string AccountingVoucherName = "AccountingVoucher";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded Document Menu Names")]
		public const string DocBuilderAccountingVoucherName = "DocBuilder Accounting Voucher";

		public AccountingVoucherPrintWrapper()
			: base(new BusinessObjectFactory())
		{
		}

		public AccountingVoucherPrintWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Lists

		#region Ledger List

		public OptionalFilterCriteriaList LedgerTypeList
		{
			get
			{
				if (fLedgerTypeList == null)
				{
					fLedgerTypeList = LedgerTransactionAssociator.GetLedgerList();
				}
				return fLedgerTypeList;
			}
		}

		OptionalFilterCriteriaList fLedgerTypeList;

		#endregion

		#region Transaction Type List

		public OptionalFilterCriteriaList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new OptionalFilterCriteriaList();
				}
				return fTransactionTypeList;
			}
		}

		OptionalFilterCriteriaList fTransactionTypeList;

		public void ReloadTransactionList()
		{
			fTransactionTypeList = LedgerTransactionAssociator.GetTransactionListByLedger(LedgerTypeList, true);
		}

		#endregion

		#region Branch List

		public GlbBranchCollection BranchesList
		{
			get
			{
				if (fBranchesList == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					fBranchesList = new GlbBranchCollection(Factory, filter);
				}
				return fBranchesList;
			}
		}

		public GlbBranchCollection fBranchesList;

		#endregion

		#endregion

		#region Public Properties

		#region Selected Branches

		public GlbBranchCollection SelectedBranches
		{
			get
			{
				if (fSelectedBranches == null)
				{
					fSelectedBranches = new GlbBranchCollection(Factory);
					RegisterEditableChildObject(fSelectedBranches);
				}

				return fSelectedBranches;
			}
		}

		GlbBranchCollection fSelectedBranches;

		#endregion

		#region From Period

		public ZInt FromPeriod
		{
			get { return fromPeriod; }
			set
			{
				SetNonPersistentPropertyValue(FromPeriodInfo, ref fromPeriod, value);
				SetDatesFromFromPeriod();
				FromDateInfo.RefreshBinding();
				EndDateInfo.RefreshBinding();
			}
		}

		ZInt fromPeriod;

		public ZPropertyInfo FromPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(FromPeriod)); }
		}

		void SetDatesFromFromPeriod()
		{
			if (!fromPeriod.IsEmpty)
			{
				fromDate = PeriodCalculator.GetFirstDayForPeriod(FromPeriod);
				endDate = PeriodCalculator.GetLastDayForPeriod(FromPeriod);
			}
		}

		#endregion

		#region End Date

		public ZDateTime EndDate
		{
			get { return endDate; }
			set { SetNonPersistentPropertyValue(EndDateInfo, ref endDate, value); }
		}

		public ZPropertyInfo EndDateInfo
		{
			get { return GetZPropertyInfo(nameof(EndDate)); }
		}

		ZDateTime endDate;

		#endregion

		#region From Date

		public ZDateTime FromDate
		{
			get { return fromDate; }
			set { SetNonPersistentPropertyValue(FromDateInfo, ref fromDate, value); }
		}

		ZDateTime fromDate;

		public ZPropertyInfo FromDateInfo
		{
			get { return GetZPropertyInfo(nameof(FromDate)); }
		}

		#endregion

		#region Include Organisation Code

		public ZBool IncludeOrganisationCode
		{
			get { return includeOrganisationCode; }
			set { SetNonPersistentPropertyValue(IncludeOrganisationCodeInfo, ref includeOrganisationCode, value); }
		}

		ZBool includeOrganisationCode;

		public ZPropertyInfo IncludeOrganisationCodeInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeOrganisationCode)); }
		}

		#endregion

		#region Include Job Number

		public ZBool IncludeJobNumber
		{
			get { return includeJobNumber; }
			set { SetNonPersistentPropertyValue(IncludeJobNumberInfo, ref includeJobNumber, value); }
		}

		ZBool includeJobNumber;

		public ZPropertyInfo IncludeJobNumberInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeJobNumber)); }
		}

		#endregion

		#endregion

		#region Validation

#if DEBUG
		virtual
#endif
 public void ValidateFromPeriod()
		{
			FromPeriodInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FromPeriodInfo);
			if (IsWIPSelected || IsAccrualSelected || IsGeneralJournalSelected)
			{
				if (!PeriodCalculator.IsPeriodGLClosed(FromPeriod))
				{
					FromPeriodInfo.AddError(Res.GetString("9fe76c8f-1806-4eb7-9c7f-ce442dbf7ba8", "This period is not closed."));
				}
			}
		}

#if DEBUG
		virtual
#endif
 public void ValidateFromDate()
		{
			FromDateInfo.ClearAllNotifications();

			TypeValidation.CheckValidZDateTimeWithoutRange(FromDateInfo);
			TypeValidation.CheckValidZDateTimeRange(FromDateInfo);
			MandatoryValidation.CheckEntered(FromDateInfo);
			if (!FromPeriod.IsEmpty)
			{
				if ((PeriodCalculator.GetFirstDayForPeriod(FromPeriod) > FromDate) || (PeriodCalculator.GetLastDayForPeriod(FromPeriod) < FromDate))
				{
					FromDateInfo.AddError(Res.GetString("26876afc-c88f-44e9-81fd-75dc33a31bdc", "From Date must be within the Period specified."));
				}
			}
		}

#if DEBUG
		virtual
#endif
 public void ValidateEndDate()
		{
			EndDateInfo.ClearAllNotifications();

			TypeValidation.CheckValidZDateTimeWithoutRange(EndDateInfo);
			TypeValidation.CheckValidZDateTimeRange(EndDateInfo);
			MandatoryValidation.CheckEntered(EndDateInfo);
			if (!FromPeriod.IsEmpty)
			{
				if ((PeriodCalculator.GetFirstDayForPeriod(FromPeriod) > EndDate) || (PeriodCalculator.GetLastDayForPeriod(FromPeriod) < EndDate))
				{
					EndDateInfo.AddError(Res.GetString("20d4bde7-26d1-4b74-b6e4-592c589b3ae2", "End Date must be within the Period specified."));
				}
			}
		}

		public void ValidateBranchWithJobCostingOptionSelected()
		{
			foreach (GlbBranch branch in SelectedBranches)
			{
				branch.ClearAllNotifications();
			}

			if ((IsAccrualSelected || IsWIPSelected) && SelectedBranches.Count > 0 && !AccountingMasterFilesRegistry.Instance.PrintGLVoucherBasedOnTransactionLineBranch.Value)
			{
				SelectedBranches[0].AddRowError(Res.GetString("ffe1eaa8-512d-4307-9daf-86abc1937d51", "You cannot select Branch Filter if you are printing Job Costing Voucher."));
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateFromDate();
			ValidateEndDate();
			ValidateFromPeriod();
			ValidateBranchWithJobCostingOptionSelected();
		}

		#endregion

		#region Tranaction Filter

		ZQuery TransactionFilter
		{
			get
			{
				fTransactionFilter = new ZQuery();

				if (TransactionTypeList.SelectedItemCount > 0)
				{
					AddExcludedTransactionType(fTransactionFilter);
					AddDateFilter(fTransactionFilter);
					AddTransactionTypeFilter(fTransactionFilter);
					AddBranchesFilter(fTransactionFilter);

					fTransactionFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				}
				else
				{
					fTransactionFilter = ZQuery.NoResultQuery;
				}

				return fTransactionFilter;
			}
		}

		ZQuery fTransactionFilter;

		void AddTransactionTypeFilter(ZQuery transactionFilter)
		{
			ZQuery ledgerAndTypeFilter = new ZQuery();

			ledgerAndTypeFilter.AddToFilter(TransactionTypeList.GetFilterFromSelectedItems());
			ledgerAndTypeFilter.AddToFilter(LedgerTypeList.GetFilterFromSelectedItems());

			if (IsContraSelected)
			{
				ZQuery contraFilter = new ZQuery();
				contraFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				contraFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra);
				ledgerAndTypeFilter.AddToFilter(contraFilter, JoinCondition.Or);
			}

			transactionFilter.AddToFilter(ledgerAndTypeFilter);
		}

		void AddDateFilter(ZQuery filter)
		{
			if (FromDate.IsValid)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, FromDate);
			}

			if (EndDate.IsValid)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, EndDate);
			}
		}

		void AddExcludedTransactionType(ZQuery filter)
		{
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, TransactionTypes.ReceiptBatch);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, TransactionTypes.OpeningPayment);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, TransactionTypes.OpeningReceipt);
		}

		internal void AddBranchesFilter(ZQuery filter)
		{
			if (SelectedBranches.Count > 0)
			{
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GB, SelectedBranches.GetPKs());
			}
		}

		#endregion

		#region Voucher Document Printing Related

		public ZInt GetWrapperCount()
		{
			return (AccoutingVoucherDocWrappers == null ? 0 : AccoutingVoucherDocWrappers.Length);
		}

		public void GenerateVoucherDocWrapper()
		{
			PrintUtil = new AccPrintingUtility(Factory, Constants.DataContext.AccountingVoucher);
			AccoutingVoucherDocWrappers = GenerateDocWrapper();
		}

		public void PrintVoucherDocument()
		{
			if (PrintUtil != null & GetWrapperCount() > 0)
			{
				PrintCore();
			}
		}

#if DEBUG
		virtual
#endif
 public PrintTask GetAccountingVoucherPrintTask(AccTransactionHeader[] transactionsToPrint)
		{
			var task = (PrintTask)null;
			var voucherDocWrappers = GenerateDocWrapper(transactionsToPrint, true);
			var stmMenuItem = GetAccountingVoucherDocumentCommand(voucherDocWrappers);
			var pack = new DocumentPack(stmMenuItem);

			if (voucherDocWrappers.Length > 0)
			{
				task = new PrintTask();
				foreach (var voucherWrapper in voucherDocWrappers)
				{
					var voucherProvider = (VoucherProvider)voucherWrapper.WrappedObject;
					pack.AddReportsToPack(stmMenuItem, null, (IDocumentSupportable)voucherWrapper.WrappedObject, null);
					pack.ForceBusinessObjectToLogAgainst(voucherProvider.Transaction);
					if (pack.DocumentSupporter == null)
					{
						pack.DocumentSupporter = voucherProvider.DocumentSupporter;
					}
				}
				task.Add(pack);
			}
			return task;
		}

		DocumentCommand GetAccountingVoucherDocumentCommand(DocumentWrapper[] accoutingVoucherDocWrappers)
		{
			var menuItem = (DocumentCommand)null;
			if (accoutingVoucherDocWrappers.Length > 0)
			{
				var voucher = (VoucherProvider)accoutingVoucherDocWrappers[0].WrappedObject;
				var menuName = UseDocBuilderAccountingVoucher ? DocBuilderAccountingVoucherName : AccountingVoucherName;
				var commandFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsPublished, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueChar);
				var documentCommands = new DocumentCommandCollection(voucher);
				documentCommands.Load();
				var commands = documentCommands.Find(commandFilter);
				if (commands.Length < 1)
				{
					throw new ARAP.Invoicing.PublishedARInvoiceDocumentNotFoundException();
				}
				menuItem = (DocumentCommand)commands[0];
			}
			return menuItem;
		}

#if DEBUG
		virtual
#endif
 public void PrintCore()
		{
			if (UseDocBuilderAccountingVoucher)
			{
				using (var task = GetAccountingVoucherPrintTask(Transactions))
				{
					if (task != null)
					{
						task.Run(AllowedDeliveryOptions.All, Env.Security.None);
					}
				}
			}
			else
			{
				PrintUtil.PrintDocuments(AccoutingVoucherDocWrappers, AccountingVoucherName, AllowedDeliveryOptions.All);
			}
		}

#if DEBUG
		public DocumentWrapper[] GenerateDocWrapper_ForTestOnly()
		{
			return GenerateDocWrapper();
		}
#endif

		DocumentWrapper[] GenerateDocWrapper()
		{
			Transactions = Factory.Load(typeof(AccTransactionHeader), TransactionFilter) as AccTransactionHeader[];
			return GenerateDocWrapper(Transactions);
		}

		public DocumentWrapper[] GenerateDocWrapper(AccTransactionHeader[] transactionHeaders, bool includeAllOptions = false)
		{
			var arrayTransactionHeaders = new ArrayList();
			arrayTransactionHeaders.AddRange((from transaction in transactionHeaders
											  where transaction.AH_Ledger == LedgerTypes.AccountsPayable &&
												  (transaction.AH_TransactionType == TransactionTypes.Invoice ||
												  transaction.AH_TransactionType == TransactionTypes.CreditNote ||
												  transaction.AH_TransactionType == TransactionTypes.AdjustmentNote)
											  orderby transaction.AH_Ledger, transaction.AH_TransactionType, transaction.AH_ConsolidatedInvoiceRef
											  select transaction).ToArray());
			arrayTransactionHeaders.AddRange((from transaction in transactionHeaders
											  where !(transaction.AH_Ledger == LedgerTypes.AccountsPayable &&
												  (transaction.AH_TransactionType == TransactionTypes.Invoice ||
												  transaction.AH_TransactionType == TransactionTypes.CreditNote ||
												  transaction.AH_TransactionType == TransactionTypes.AdjustmentNote))
											  orderby transaction.AH_Ledger, transaction.AH_TransactionType, transaction.AH_TransactionNum
											  select transaction).ToArray());

			ArrayList wrappersToReturn = new ArrayList();

			foreach (AccTransactionHeader transaction in arrayTransactionHeaders)
			{
				if (transaction.AH_TransactionType == TransactionTypes.Transfer && transaction.AH_Ledger == LedgerTypes.CashBook && transaction.AH_TransactionCount != 1 && transaction.AH_TransactionCount != 4)
				{
					continue;
				}

				VoucherProvider voucher = VoucherFactory.GetProvider(transaction);

				if (voucher != null)
				{
					if (includeAllOptions)
					{
						voucher.IncludeOrganisationCode = true;
						voucher.IncludeJobNumber = true;
					}
					else
					{
						voucher.IncludeOrganisationCode = IncludeOrganisationCode;
						voucher.IncludeJobNumber = IncludeJobNumber;
					}

					DocumentWrapper wrapper = DocumentWrapperFactory.CreateWrapper(Constants.DataContext.AccountingVoucher, voucher);
					if (wrapper != null)
					{
						wrappersToReturn.Add(wrapper);
					}
				}
			}

			ProcessWIPandAccrual(wrappersToReturn);

			AccoutingVoucherDocWrappers = (DocumentWrapper[])wrappersToReturn.ToArray(typeof(DocumentWrapper));
			return AccoutingVoucherDocWrappers;
		}

		void ProcessWIPandAccrual(ArrayList wrappersToReturn)
		{
			if (IsWIPSelected || IsAccrualSelected)
			{
				ArrayList wIPAccrualWrappers = GetWIPsAccrualVoucherWrapperList();
				if (wIPAccrualWrappers != null && wIPAccrualWrappers.Count > 0)
				{
					wrappersToReturn.AddRange(wIPAccrualWrappers);
				}
			}
		}

		VoucherProviderFactory fVoucherFactory;
		VoucherProviderFactory VoucherFactory
		{
			get
			{
				if (fVoucherFactory == null)
				{
					fVoucherFactory = new VoucherProviderFactory(Factory);
				}
				return fVoucherFactory;
			}
		}

		ArrayList GetWIPsAccrualVoucherWrapperList()
		{
			fWIPsAccrualVoucherProvider = new ArrayList();
			var periodList = DataInterfaceUtils.GetPeriodRange(FromPeriod, FromPeriod, Factory);
			var isByBranch = AccountingMasterFilesRegistry.Instance.PrintGLVoucherBasedOnTransactionLineBranch.Value;
			foreach (var period in periodList)
			{
				if (IsWIPSelected)
				{
					ProcessWIPAccrualWrapperList(period, TransactionLineTypes.WIP, isByBranch);
				}

				if (IsAccrualSelected)
				{
					ProcessWIPAccrualWrapperList(period, TransactionLineTypes.Accrual, isByBranch);
				}
			}

			return fWIPsAccrualVoucherProvider;
		}

		void ProcessWIPAccrualWrapperList(AccPeriodManagement period, string lineType, bool isByBranch)
		{
			if (isByBranch)
			{
				CreateWIPAccrualWrapperByBranch(period, lineType);
			}
			else
			{
				CreateWIPAccrualWrapper(period, lineType);
			}
		}

		void CreateWIPAccrualWrapper(AccPeriodManagement period, string lineType)
		{
			var wIPAccrualCollection = new WIPAccrualDataSourceCollection(Factory);
			wIPAccrualCollection.LoadCollection(period.AM_Period, lineType);
			if (wIPAccrualCollection.Any(acc => acc.Amount != 0m))
			{
				AddWIPsAccrualVoucherWrapperToProvider(wIPAccrualCollection, lineType, period);
			}
		}

		void CreateWIPAccrualWrapperByBranch(AccPeriodManagement period, string lineType)
		{
			var wIPAccrualCollection = new WIPAccrualDataSourceCollection(Factory);
			wIPAccrualCollection.LoadCollection(period.AM_Period, lineType, true);
			var wIPAccrualListByBranchGroups = wIPAccrualCollection.GroupBy(voucherDataSource => voucherDataSource.BranchCode)
				.Where(group => !SelectedBranches.Any() || SelectedBranches.Any(branch => ((GlbBranch)branch).GB_Code == group.Key))
				.Select(voucherDataSourceCollection => voucherDataSourceCollection.ToList());
			foreach (var wIPAccrualListByBranchGroup in wIPAccrualListByBranchGroups)
			{
				if (wIPAccrualListByBranchGroup.Any(voucherDataSource => voucherDataSource.Amount != 0m))
				{
					AddWIPsAccrualVoucherWrapperToProvider(new TransformedWIPAccrualDataSourceCollectionForBranch(wIPAccrualListByBranchGroup), lineType, period);
				}
			}
		}

		void AddWIPsAccrualVoucherWrapperToProvider(WIPAccrualDataSourceCollection wIPAccrualCollection, string lineType, AccPeriodManagement period)
		{
			TransactionLineOnlyVoucherProvider voucherProvider;
			if (TransactionLineTypes.Accrual == lineType)
			{
				voucherProvider = new AccrualVoucherProvider(wIPAccrualCollection, period.AM_Period);
			}
			else
			{
				voucherProvider = new WIPVoucherProvider(wIPAccrualCollection, period.AM_Period);
			}
			fWIPsAccrualVoucherProvider.Add(DocumentWrapperFactory.CreateWrapper(Constants.DataContext.AccountingVoucher, voucherProvider));
		}

		ArrayList fWIPsAccrualVoucherProvider;

		#endregion

		#region Helper Objects

		AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalculator;
			}
		}

		AccountingPeriodCalculator fPeriodCalculator;

		LedgerTransactionAssociator LedgerTransactionAssociator
		{
			get
			{
				if (fLedgerTransactionAssociator == null)
				{
					fLedgerTransactionAssociator = new LedgerTransactionAssociator();
				}
				return fLedgerTransactionAssociator;
			}
		}

		LedgerTransactionAssociator fLedgerTransactionAssociator;

		#endregion

		#region Private boolean Properties

		bool IsWIPSelected
		{
			get
			{
				return (TransactionTypeList.FindSelectedValueByDescription(TransactionDescription.WIP));
			}
		}

		bool IsAccrualSelected
		{
			get
			{
				return (TransactionTypeList.FindSelectedValueByDescription(TransactionDescription.Accrual));
			}
		}

#if DEBUG
		public bool IsWIPSelected_ForTestOnly
		{
			get { return IsWIPSelected; }
		}

		public bool IsAccrualSelected_ForTestOnly
		{
			get { return IsAccrualSelected; }
		}
#endif

		bool IsContraSelected
		{
			get
			{
				return (TransactionTypeList.FindSelectedValueByDescription(TransactionDescription.Contra));
			}
		}

		bool IsGeneralJournalSelected
		{
			get
			{
				return (TransactionTypeList.FindSelectedValueByDescription(TransactionDescription.GeneralJournal));
			}
		}

		bool UseDocBuilderAccountingVoucher
		{
			get
			{
				return DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.Value;
			}
		}

		#endregion

		AccTransactionHeader[] Transactions;
		DocumentWrapper[] AccoutingVoucherDocWrappers;

		AccPrintingUtility PrintUtil;
	}
}
