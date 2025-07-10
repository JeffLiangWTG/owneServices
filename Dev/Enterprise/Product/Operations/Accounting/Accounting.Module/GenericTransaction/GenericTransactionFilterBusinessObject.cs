using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class GenericTransactionFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			DependentListFilter transactionTypeFilter = new DependentListFilter("Ledger/Transaction Type", GetTransactionTypeQuery, LedgerList, AccountingUtils.GetTransactionTypeList);
			transactionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|LedgerTransactionType", "Ledger/Transaction Type");
			filters.AddCustomFilter(transactionTypeFilter);

			ModuleTextRangeFilter periodFilter = new ModuleTextRangeFilter("Accounting Period", GetPeriodQuery);
			periodFilter.Property1Validation = ValidateAccountingPeriod;
			periodFilter.Property2Validation = ValidateAccountingPeriod;
			periodFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|AccountingPeriod", "Accounting Period");
			filters.AddCustomFilter(periodFilter);

			ModuleGuidFilter organisationFilter = filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, GenericTransactionSchema.VT_OH, Organisations);
			organisationFilter.Category = FilterCategories.Organisations;
			organisationFilter.IsCommon = true;
			organisationFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			organisationFilter.RemoveComparisonOperatorsLeavingOne(ModuleNumberFilter.ComparisonConstants.Exact);
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|Organisation", "Organization");
			organisationFilter.SupportsFiltersMatchComparisonOperator = false; // There's no way of knowing which type of object we're getting.

			SubAccountFilter subAccountFilter = new SubAccountFilter(SubAccountFilterText);
			subAccountFilter.Category = FilterCategories.Organisations;
			subAccountFilter.MultilingualDescription = ResString.GetMultilingualString("1f42938a-bd8d-4850-a6c0-e7fef7c5c707", SubAccountFilterText);
			filters.AddFilter(subAccountFilter);

			ModuleTextFilter subAccountWithoutValueFilter = new ModuleTextFilter(SubAccountFilterWithoutValueFilterText, GetSubAccountTypeWithoutValueQuery, SubAccountTypeList);
			subAccountWithoutValueFilter.Category = FilterCategories.Organisations;
			subAccountWithoutValueFilter.MultilingualDescription = ResString.GetMultilingualString("595a399a-231e-4dd5-a95b-67db4c69f3ed", SubAccountFilterWithoutValueFilterText);
			filters.AddFilter(subAccountWithoutValueFilter);

			filters.AddDateFilter(AccountingConstants.DateFilterTypes.PostingDate, GetPostDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|PostingDate", "Posting Date");
			filters.AddDateFilter(AccountingConstants.DateFilterTypes.DocumentDate, GetDocumentDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|DocumentDate", "Document Date");

			ModuleNumberRangeFilter exTaxAmountFilter = filters.AddNumberRangeFilter(AccountingConstants.AmountFilterTypes.ExTaxAmount, GenericTransactionSchema.VT_Amount);
			exTaxAmountFilter.Category = FinancialDetailsCategory;
			exTaxAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|ExTaxAmount", "Ex Tax Amount");

			ModuleNumberRangeFilter taxAmountFilter = filters.AddNumberRangeFilter(AccountingConstants.AmountFilterTypes.TaxAmount, GenericTransactionSchema.VT_GST);
			taxAmountFilter.Category = FinancialDetailsCategory;
			taxAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|TaxAmount", "Tax Amount");

			ModuleNumberRangeFilter totalAmountFilter = filters.AddNumberRangeFilter(AccountingConstants.AmountFilterTypes.TotalAmount, GenericTransactionSchema.VT_Total);
			totalAmountFilter.Category = FinancialDetailsCategory;
			totalAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|TotalAmount", "Total Amount");

			AccGLHeaderRangeFilter accountFilter = new AccGLHeaderRangeFilter("GL Account", GetGLAccountQuery, GLHeaders, GLHeaders);
			accountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|GLAccount", "GL Account");
			filters.AddCustomFilter(accountFilter);

			return filters;
		}

		protected override bool ShouldAddUserDefinedFiltersCore => false;

		protected override bool ShouldAddCustomSqlFilter => false;

		#region Filter Delegates

		ZQuery GetPostDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, GenericTransactionSchema.VT_PostDate, fromDate.Date, toDate.Date);
			return query;
		}

		ZQuery GetDocumentDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, GenericTransactionSchema.VT_InvoiceDate, fromDate.Date, toDate.Date);
			return query;
		}

		ZQuery GetTransactionTypeQuery(ZString ledger, ZString transactionType)
		{
			ZQuery query = new ZQuery();

			if (!ledger.IsEmpty)
			{
				query.AddToFilter(GenericTransactionSchema.VT_Ledger, SQLComparisonOperator.Equal, ledger);
			}

			if (!transactionType.IsEmpty)
			{
				query.AddToFilter(GenericTransactionSchema.VT_Type, SQLComparisonOperator.Equal, transactionType);
			}

			return query;
		}

		ZQuery GetPeriodQuery(ZString value1, ZString value2)
		{
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			ZDateTime startDate = ZDateTime.Invalid;
			ZDateTime endDate = ZDateTime.Invalid;

			if (!value1.IsEmpty)
			{
				if (ZInt.CanParse(value1))
				{
					startDate = periodCalculator.GetFirstDayForPeriod(ZInt.Parse(value1));
				}
			}

			if (!value2.IsEmpty)
			{
				if (ZInt.CanParse(value2))
				{
					endDate = periodCalculator.GetLastDayForPeriod(ZInt.Parse(value2));
				}
			}

			return GetPostDateQuery(DateComparisonOperator.HasDateInRange, startDate, endDate);
		}

		ZQuery GetGLAccountQuery(ZString value1, ZString value2)
		{
			ZQuery query = new ZQuery();
			ZQuery secondGlAccountQuery = new ZQuery();
			ZQuery gstGlAccountQuery = new ZQuery();

			if (!value1.IsEmpty)
			{
				query.AddToFilter(GenericTransactionSchema.VT_GLAccount, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
				secondGlAccountQuery.AddToFilter(GenericTransactionSchema.VT_2ndGLAccount, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
				gstGlAccountQuery.AddToFilter(GenericTransactionSchema.VT_GSTGLAccount, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
			}

			if (!value2.IsEmpty)
			{
				query.AddToFilter(GenericTransactionSchema.VT_GLAccount, SQLComparisonOperator.LessThanOrEqualTo, value2);
				secondGlAccountQuery.AddToFilter(GenericTransactionSchema.VT_2ndGLAccount, SQLComparisonOperator.LessThanOrEqualTo, value2);
				gstGlAccountQuery.AddToFilter(GenericTransactionSchema.VT_GSTGLAccount, SQLComparisonOperator.LessThanOrEqualTo, value2);
			}

			query.AddToFilter(secondGlAccountQuery, JoinCondition.Or);
			query.AddToFilter(gstGlAccountQuery, JoinCondition.Or);

			return query;
		}

		ZQuery GetSubAccountTypeWithoutValueQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				query.AddToFilter(SubAccountHelper.GetSubAccountsQuery(value, ZGuid.Empty, true));
			}
			return query;
		}

		#endregion

		#region Lists

		#region LedgerList

		protected CodeDescriptionPairList fLedgerList;
		public virtual CodeDescriptionPairList LedgerList
		{
			get
			{
				if (fLedgerList == null)
				{
					fLedgerList = new CodeDescriptionPairList();
					fLedgerList.AddPair(String.Empty, Res.GetString("Accounting|GenericTransactionFilter|AllLedgers", "All Ledgers"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.AccountsReceivable, Res.GetString("Accounting|GenericTransactionFilter|Receivables", "Receivables"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.AccountsPayable, Res.GetString("Accounting|GenericTransactionFilter|Payables", "Payables"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.CashBook, Res.GetString("Accounting|GenericTransactionFilter|CashBooks", "Cash Books"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.JobCosting, Res.GetString("Accounting|GenericTransactionFilter|JobCosting", "Job Costing"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.General, Res.GetString("Accounting|GenericTransactionFilter|GeneralLedger", "General Ledger"));
				}
				return fLedgerList;
			}
		}

		#endregion

		public CodeDescriptionPairList SubAccountTypeList
		{
			get
			{
				if (subAccountTypes == null)
				{
					subAccountTypes = new AccountingMasterFilesConstants.SubAccountTypeList();
				}

				return subAccountTypes;
			}
		}

		CodeDescriptionPairList subAccountTypes;

		#endregion

		#region Collections

		public OrgHeaderCollection Organisations
		{
			get { return organisations_List ?? (organisations_List = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection organisations_List;

		public AccGLHeaderCollection GLHeaders
		{
			get { return FindboxLookupCollections.GetGLHeaders(Factory); }
		}

		#endregion

		#region Implementation

		FilterCategory fFinancialDetailsCategory;
		protected FilterCategory FinancialDetailsCategory
		{
			get
			{
				if (fFinancialDetailsCategory == null)
				{
					fFinancialDetailsCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|GenericTransactionFilter|FinancialDetails", "Financial Details"));
				}
				return fFinancialDetailsCategory;
			}
		}

		protected void ValidateAccountingPeriod(ZPropertyInfo info)
		{
			var valueAsString = (ZString)info.Value;
			if (!valueAsString.IsEmpty)
			{
				if (ZInt.CanParse(valueAsString))
				{
					var period = ZInt.Parse(valueAsString);
					if (!new AccountingPeriodCalculator(Factory).IsPeriodValid(period))
					{
						info.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(period));
					}
				}
				else
				{
					info.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(valueAsString));
				}
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string SubAccountFilterText = "Sub Account + Value";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string SubAccountFilterWithoutValueFilterText = "Sub Account + No Value";
	}
}
