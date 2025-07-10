using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class CashBookFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public CashBookFilterBusinessObject()
			: base()
		{
		}

		FilterCategory fFinancialDetailsCategory;
		protected FilterCategory FinancialDetailsCategory
		{
			get
			{
				if (fFinancialDetailsCategory == null)
				{
					fFinancialDetailsCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|CashBookFilter|FinancialDetails", "Financial Details"));
				}
				return fFinancialDetailsCategory;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddTypeFilters(filters);
			AddReferenceFilters(filters);
			AddDateFilters(filters);
			AddOtherFilters(filters);
			return filters;
		}

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Business.AccountingUtils.NumberFilterTypes.TransactionNumber, GetTransactionNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_TransactionNum)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|TransactionNumber", "Transaction #");
			filters.AddNumberFilter(Business.AccountingUtils.NumberFilterTypes.ChequeReferenceNumber, GetChequeReferenceNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ChequeOrReference)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|ChequeReferenceNumber", "Check/Reference #");
			filters.AddNumberFilter(Business.AccountingUtils.NumberFilterTypes.DepositBatchNumber, GetDepositBatchNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ReceiptBatchNo)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|DepositBatchNumber", "Deposit Batch #");
			filters.AddNumberFilter(Business.AccountingUtils.NumberFilterTypes.DDRBatchNumber, GetDirectDebitNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ReceiptBatchNo)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|DDRBatchNumber", "DDR Batch #");
		}

		#endregion

		#region Number Filters Delegates

		ZQuery GetTransactionNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddTransactionNumberToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetChequeReferenceNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddChequeReferenceNumberToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetDepositBatchNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddDepositBatchNumberToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetDirectDebitNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddDirectDebitNumberToFilter(query, comparisonOperator, value);
			return query;
		}

		#endregion

		#region Number Filter Implementation

		protected virtual void AddTransactionNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_TransactionNum, @operator, value);
		}

		protected virtual void AddChequeReferenceNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_ChequeOrReference, @operator, value);
		}

		protected virtual void AddDepositBatchNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_ReceiptBatchNo, @operator, value);
		}

		protected virtual void AddDirectDebitNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_ReceiptBatchNo, @operator, value);
		}

		#endregion

		#region Type Filter

		void AddTypeFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddTextFilter("Transaction Type", GetTransactionTypeQuery, TransactionTypes);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|TransactionType", "Transaction Type");

			filter = filters.AddTextFilter("Transaction Category", GetTransactionCategoryQuery, TransactionCategories);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|TransactionCategory", "Transaction Category");

			filter = filters.AddFlagsFilter("Not Batched", new string[] { Res.GetString("Accounting|CashBookFilter|NotBatched", "Not Batched") }, new GetFlagsQuery[] { GetNotBatchedQuery });
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|NotBatched", "Not Batched");

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
			{
				ModuleTextFilter positivePayExportFilter = filters.AddTextFilter(Res.GetString("56bd0449-8d0f-4234-8d29-ed3d7583243e", "Positive Pay Export Status"), GetPositivePayExportStatusQuery, PositivePayExportStatusList);
				positivePayExportFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|PositivePayExportStatus", "Positive Pay Export Status");
				positivePayExportFilter.Category = FilterCategories.ModesAndTypes;
			}
			else
			{
				ModuleTextFilter checkPaymentExportFilter = filters.AddTextFilter("Check Payment Export Status", GetPositivePayExportStatusQuery, CheckPaymentExportStatusList);
				checkPaymentExportFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|CheckPaymentExportStatus", "Check Payment Export Status");
				checkPaymentExportFilter.Category = FilterCategories.ModesAndTypes;
			}
		}

		CodeDescriptionPairList fTransactionTypes;
		public CodeDescriptionPairList TransactionTypes
		{
			get
			{
				if (fTransactionTypes == null)
				{
					fTransactionTypes = new CodeDescriptionPairList(OLookUpEditType.CashBookTransactionTypes);
				}
				return fTransactionTypes;
			}
		}

		CodeDescriptionPairList transactionCategories;
		public CodeDescriptionPairList TransactionCategories
		{
			get
			{
				if (transactionCategories == null)
				{
					transactionCategories = new CodeDescriptionPairList();
					transactionCategories.AddPair("ALL", ResString.GetMultilingualString("Accounting|CashBookFilter|DisplayAllTransactions", "Display all transactions"));
					transactionCategories.AddPair(Core.Constants.TransactionCategory.Codes.UnrealizedExchangeGainLoss, Core.Constants.TransactionCategory.Descriptions.UnrealizedExchangeGainLoss);
					transactionCategories.AddPair(Core.Constants.TransactionCategory.Codes.RealizedExchangeGainLoss, Core.Constants.TransactionCategory.Descriptions.RealizedExchangeGainLoss);
				}
				return transactionCategories;
			}
		}

		protected CodeDescriptionPairList positivePayExportStatusList;
		public CodeDescriptionPairList PositivePayExportStatusList
		{
			get
			{
				if (positivePayExportStatusList == null)
				{
					positivePayExportStatusList = new CodeDescriptionPairList();
					positivePayExportStatusList.AddPair("ALL", Res.GetString("Accounting|CashBookFilter|DisplayAllTransactions", "Display all transactions"));
					positivePayExportStatusList.AddPair("EXP", Res.GetString("Accounting|CashBookFilter|DisplayExportedPositivePayTransactions", "Display exported positive pay transactions"));
					positivePayExportStatusList.AddPair("NOT", Res.GetString("Accounting|CashBookFilter|DisplayUnexportedPositivePayTransactions", "Display un-exported positive pay transactions"));
				}
				return positivePayExportStatusList;
			}
		}

		protected CodeDescriptionPairList checkPaymentExportStatusList;
		public CodeDescriptionPairList CheckPaymentExportStatusList
		{
			get
			{
				if (checkPaymentExportStatusList == null)
				{
					checkPaymentExportStatusList = new CodeDescriptionPairList();
					checkPaymentExportStatusList.AddPair("ALL", Res.GetString("Accounting|CashBookFilter|DisplayAllTransactions", "Display all transactions"));
					checkPaymentExportStatusList.AddPair("EXP", Res.GetString("Accounting|CashBookFilter|DisplayExportedCheckPaymentTransactions", "Display exported Check Payments transactions"));
					checkPaymentExportStatusList.AddPair("NOT", Res.GetString("Accounting|CashBookFilter|DisplayUnexportedCheckPaymentTransactions", "Display un-exported Check Payments transactions"));
				}
				return checkPaymentExportStatusList;
			}
		}

		#endregion

		#region Type Filter Delegates

		ZQuery GetTransactionTypeQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddTransactionTypeToFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetTransactionCategoryQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddTransactionCategoryToFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetNotBatchedQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			AddNotBatchedToFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetPositivePayExportStatusQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			AddPositivePayExportStatusToFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		#endregion

		#region Types Filter Implementation

		protected virtual void AddTransactionTypeToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZString transactionType = (ZString)value;
			if (transactionType != "ALL")
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, @operator, value);
			}
		}

		protected virtual void AddTransactionCategoryToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZString transactionCategory = (ZString)value;
			if (transactionCategory != "ALL")
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, @operator, value);
			}
		}

		protected virtual void AddNotBatchedToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZBool notBatched = (ZBool)value;
			if (notBatched)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, ZString.Empty);
				query.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
			}
		}

		protected virtual void AddPositivePayExportStatusToFilter(ZDBOnlyQuery query, SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value == "EXP" || value == "NOT")
			{
				ZBool notExported = true;
				if (value == "EXP")
				{
					notExported = false;
				}
				else if (value == "NOT")
				{
					notExported = true;
				}

				ZQuery queryTransactionType = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, Enterprise.ZArchitecture.Core.TransactionTypes.Payment);
				queryTransactionType.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, Enterprise.ZArchitecture.Core.TransactionTypes.DirectPayment);

				query.AddToFilter(queryTransactionType);
				query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_ReceiptType, Enterprise.ZArchitecture.Core.ReceiptTypes.Cheque);

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GenExportBatchSequence), GenExportBatchSequenceSchema.XB_ParentID, notExported);
				subQuery.AddToFilter(GenExportBatchSequenceSchema.XB_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
				subQuery.AddToFilter(GenExportBatchSequenceSchema.XB_Type, Core.Constants.DataExportBatchSubTypes.Codes.PositivePayFile);
				query.AddSubQuery(AccTransactionHeaderSchema.PK, GenExportBatchSequenceSchema.XB_ParentID, subQuery, JoinCondition.And);
			}
		}

		#endregion

		#region References Filter

		void AddReferenceFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNkFilter("Currency", AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ModuleIDs.RefCurrency, Currencies);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|Currency", "Currency");

			filter = filters.AddGuidFilter("Bank Account", ModuleIDs.AccBankAccount, AccTransactionHeaderSchema.AH_AB, BankAccounts);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|BankAccount", "Bank Account");

			filter = filters.AddGuidFilter("Debitor/Creditor", ModuleIDs.Organisation, AccTransactionHeaderSchema.AH_OH, Organisations);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|DebitorCreditor", "Debtor/Creditor");

			filter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, Departments);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|Department", "Department");

			filter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, Branches);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|Branch", "Branch");

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				filter = filters.AddGuidFilter("Tax Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB_TaxBranch, Branches);
				filter.Category = FilterCategories.Organisations;
				filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|TaxBranch", "Tax Branch");
			}
		}

		public GlbBranchDependentCollection Branches
		{
			get { return FindboxLookupCollections.GetCompanyBranchesCollection(Factory); }
		}

		public GlbDepartmentCollection Departments
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		public RefCurrencyCollection Currencies
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		public AccBankAccountCollection BankAccounts
		{
			get { return FindboxLookupCollections.GetBankAccounts(Factory); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.PostDate, AccTransactionHeaderSchema.AH_PostDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|PostDate", "Post Date");
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.TransactionDate, AccTransactionHeaderSchema.AH_InvoiceDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|TransactionDate", "Transaction Date");
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.DateShownInStatement, AccTransactionHeaderSchema.AH_DateClearedInCashbook).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|DateShownInStatement", "Shown In Statement");
		}

		#endregion

		#region Others Filter

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNumberRangeFilter("Amount", GetAmountQuery);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|Amount", "Amount");

			filter = filters.AddTextFilter("Payment/Receipt Method", GetPaymentReceiptMethodQuery, PaymentReceiptMethods);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashBookFilter|PaymentReceiptMethod", "Payment/Receipt Method");
		}

		public CodeDescriptionPairList PaymentReceiptMethods
		{
			get
			{
				if (fPaymentReceiptMethods == null)
				{
					fPaymentReceiptMethods = new CodeDescriptionPairList(OLookUpEditType.PaymentOrReceiptMethod);
				}
				return fPaymentReceiptMethods;
			}
		}

		CodeDescriptionPairList fPaymentReceiptMethods;

		#endregion

		#region Others Filter Implementation

		ZQuery GetAmountQuery(INumericZType fromAmount, INumericZType toAmount)
		{
			ZDBOnlyQuery amountQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			var queryText = string.Empty;
			if (!fromAmount.Equals(0m))
			{
				@params.Add("@FromAmount", fromAmount, AccTransactionHeaderSchema.AH_OSTotal);
				queryText = (NoResString)"Abs(" + AccTransactionHeaderSchema.AH_OSTotal.Name + (NoResString)") >= @FromAmount";
			}
			if (!fromAmount.Equals(0m) && !toAmount.Equals(0m))
			{
				queryText = $"{queryText} AND ";
			}
			if (!toAmount.Equals(0m))
			{
				@params.Add("@ToAmount", toAmount, AccTransactionHeaderSchema.AH_OSTotal);
				queryText += (NoResString)"Abs(" + AccTransactionHeaderSchema.AH_OSTotal.Name + (NoResString)") <= @ToAmount";
			}
			amountQuery.AddFilterAndZSQLParameterCollection(queryText, @params);
			return new ZQuery(amountQuery);
		}

		ZQuery GetPaymentReceiptMethodQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (value == "DCR/DDR")
			{
				query = new ZQuery(AccTransactionHeaderSchema.AH_ReceiptType, ZArchitecture.Core.ReceiptTypes.DirectCredit);
				query.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ReceiptType, SQLComparisonOperator.Equal, ZArchitecture.Core.ReceiptTypes.DirectDebit);
			}
			else
			{
				query = new ZQuery(AccTransactionHeaderSchema.AH_ReceiptType, value);
			}

			return query;
		}

		#endregion
	}
}
