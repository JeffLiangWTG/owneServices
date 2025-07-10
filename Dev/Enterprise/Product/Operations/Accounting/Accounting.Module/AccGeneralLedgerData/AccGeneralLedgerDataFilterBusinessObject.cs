using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class AccGeneralLedgerDataFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddTextFilters(filters);
			AddDateFilters(filters);
			AddGuidFilters(filters);
			AddNumberFilters(filters);
			AddFinancialDetailsFilters(filters);
			return filters;
		}

		#region DefaultFilters

		public override ZQuery Filter
		{
			get
			{
				var query = new ZQuery(AccGeneralLedgerDataSchema.GLD_GC_Company, Env.CurrentCompany.PK);
				var result = base.Filter;
				result.AddToFilter(query);
				return result;
			}
		}

		#endregion

		#region TextFilters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var accountFilter = new AccGLHeaderRangeFilter("GL Account", GetGLAccountQuery, GLHeaders, GLHeaders);
			accountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|GLAccount", "GL Account");
			filters.AddFilter(accountFilter);

			var periodFilter = new ModuleTextRangeFilter("Accounting Period", GetPeriodQuery);
			periodFilter.Property1Validation = ValidateAccountingPeriod;
			periodFilter.Property2Validation = ValidateAccountingPeriod;
			periodFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|PostPeriod", "Post Period");
			filters.AddFilter(periodFilter);

			var typeFilter = filters.AddTextFilter("Journal Entries Type", AccGeneralLedgerDataSchema.GLD_Type, AccountingConstants.GLDTypeList);
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|GLD_Type", "Journal Entries Type");
			typeFilter.MaxLength = AccGeneralLedgerDataSchema.GLD_Type.MaxLength;

			var ledgerAndTransactionTypeFilter = new DependentListFilter("Ledger/Transaction Type", GetLedgerAndTransactionTypeQuery, LedgerList, AccountingUtils.GetTransactionTypeList);
			ledgerAndTransactionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericTransactionFilter|ledgerAndTransactionTypeFilter", "Ledger/Transaction Type");
			filters.AddCustomFilter(ledgerAndTransactionTypeFilter);

			var presentationCategoryFilter = new PresentationCategoryFilter("Presentation Category", GetPresentationCategoryQuery, PresentationCategoryList);
			presentationCategoryFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|PresentationCategory", "Presentation Category");
			presentationCategoryFilter.MaxLength = AccTransactionHeaderSchema.AH_TransactionCategory.MaxLength;
			filters.AddFilter(presentationCategoryFilter);

			var headerDescriptionFilter = filters.AddTextFilter("Header Description", GetHeaderDescription);
			headerDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|HeaderDescription", "Header Description");
			headerDescriptionFilter.MaxLength = AccTransactionHeaderSchema.AH_Desc.MaxLength;
			RemoveUnexpectedComparisonOperators(headerDescriptionFilter);

			var lineDescriptionFilter = filters.AddTextFilter("Line Description", GetLineDescription);
			lineDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|LineDescription", "Line Description");
			lineDescriptionFilter.MaxLength = AccTransactionLinesSchema.AL_Desc.MaxLength;
			RemoveUnexpectedComparisonOperators(lineDescriptionFilter);

			var accountTypeFilter = filters.AddTextFilter("Account Type", GetAccountTypeFilter, AccountTypes);
			accountTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|AccountType", "Account Type");
			accountTypeFilter.MaxLength = AccGLHeaderSchema.AG_AccountType.MaxLength;
		}

		#endregion

		#region GuidFilters

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			var organisationFilter = filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, GetOrganisationQuery, Organisations);
			organisationFilter.Category = FilterCategories.Organisations;
			organisationFilter.IsCommon = true;
			organisationFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			organisationFilter.RemoveComparisonOperatorsLeavingOne(ModuleNumberFilter.ComparisonConstants.Exact);
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|OrganizationPK", "Organization Code");
			organisationFilter.SupportsFiltersMatchComparisonOperator = false;

			var branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccGeneralLedgerDataSchema.GLD_GB_Branch, Branches);
			branchFilter.SupportsFiltersMatchComparisonOperator = false;
			branchFilter.Category = FilterCategories.Other;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|BranchCode", "Branch");
			if (!Env.Security.AccountingJournalsViewJournalEntriesNotRelatedToLoginBranch.IsAllowed)
			{
				branchFilter.DefaultProperty = GlbBranch.CurrentBranch.PK;
				branchFilter.Visibility = FilterVisibility.AlwaysVisible;
				branchFilter.ReadOnly = true;
			}

			var taxBranchFilter = filters.AddGuidFilter("TaxBranch", ModuleIDs.GlbBranch, AccGeneralLedgerDataSchema.GLD_GB_TaxBranch, Branches);
			taxBranchFilter.SupportsFiltersMatchComparisonOperator = false;
			taxBranchFilter.Category = FilterCategories.Other;
			taxBranchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|TaxBranchCode", "Tax Branch");

			var departmentFilter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccGeneralLedgerDataSchema.GLD_GE_Department, Departments);
			departmentFilter.SupportsFiltersMatchComparisonOperator = false;
			departmentFilter.Category = FilterCategories.Other;
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|Department", "Department");

			var chargeCodeFilter = filters.AddGuidFilter("Charge Code", ModuleIDs.AccChargeCode, GetChargeCode, ChargeCodeCollection, AccTransactionLinesSchema.AL_AC);
			chargeCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|ChargeCode", "Charge Code");
			RemoveUnexpectedComparisonOperators(chargeCodeFilter);

			var subAccountFilter = new AccGeneralLedgerDataSubAccountFilter(SubAccountFilterText);
			subAccountFilter.Category = FilterCategories.Organisations;
			subAccountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|SubAccountFilter", SubAccountFilterText);
			filters.AddFilter(subAccountFilter);

			var subAccountWithoutValueFilter = new ModuleTextFilter(SubAccountWithoutValueFilterText, GetSubAccountWithoutValueQuery, SubAccountTypeList);
			subAccountWithoutValueFilter.Category = FilterCategories.Organisations;
			subAccountWithoutValueFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|SubAccountFilterWithoutValue", SubAccountWithoutValueFilterText);
			filters.AddFilter(subAccountWithoutValueFilter);
		}

		#endregion

		#region DateFilters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("PostDate", AccGeneralLedgerDataSchema.GLD_PostDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|PostDate", "Post Date");
			filters.AddDateFilter("Transaction Date", GetTransactionDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|TransactionDate", "Transaction Date");
		}

		#endregion

		#region NumberFilters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var numberFilter = filters.AddNumberFilter("Journal Entries Number", AccGeneralLedgerDataSchema.GLD_JournalEntriesNumber);
			numberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|JournalEntriesNumber", "Journal Entries Number");
			numberFilter.MaxLength = AccGeneralLedgerDataSchema.GLD_JournalEntriesNumber.MaxLength;

			var transactionNumberFilter = filters.AddNumberFilter("Transaction Number", GetTransactionNumberQuery);
			transactionNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|TransactionNumber", "Transaction Number");
			transactionNumberFilter.MaxLength = AccTransactionHeaderSchema.AH_TransactionNum.MaxLength;
			RemoveUnexpectedComparisonOperators(transactionNumberFilter);

			var jobNumberFilter = filters.AddTextRangeFilter("Job Number", GetJobNumber);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|JobNumber", "Job Number Range");
			jobNumberFilter.MaxLength = JobHeaderSchema.JH_JobNum.MaxLength;
		}

		#endregion

		#region FinancialDetailsFilters

		void AddFinancialDetailsFilters(ModuleFilterCollection filters)
		{
			var osDebitAmountFilter = filters.AddNumberRangeFilter("OS Debit Amount", AccGeneralLedgerDataSchema.GLD_OSDebitAmount);
			osDebitAmountFilter.Category = FinancialDetailsCategory;
			osDebitAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|OSDebitAmount", "OS Debit Amount");

			var osCreditAmountFilter = filters.AddNumberRangeFilter("OS Credit Amount", AccGeneralLedgerDataSchema.GLD_OSCreditAmount);
			osCreditAmountFilter.Category = FinancialDetailsCategory;
			osCreditAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|OSCreditAmount", "OS Credit Amount");

			var localDebitAmountFilter = filters.AddNumberRangeFilter("Local Debit Amount", AccGeneralLedgerDataSchema.GLD_LocalDebitAmount);
			localDebitAmountFilter.Category = FinancialDetailsCategory;
			localDebitAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|LocalDebitAmount", "Local Debit Amount");

			var localCreditAmountFilter = filters.AddNumberRangeFilter("Local Credit Amount", AccGeneralLedgerDataSchema.GLD_LocalCreditAmount);
			localCreditAmountFilter.Category = FinancialDetailsCategory;
			localCreditAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|LocalCreditAmount", "Local Credit Amount");

			var currencyFilter = filters.AddNkFilter("Currency", AccGeneralLedgerDataSchema.GLD_Currency, ModuleIDs.RefCurrency, CurrencyList);
			currencyFilter.Category = FinancialDetailsCategory;
			currencyFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|Currency", "Currency");
		}

		#endregion

		#region Query

		ZQuery GetOrganisationQuery(ZGuid value)
		{
			if (value.IsValid)
			{
				var transactionHeaderSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
				transactionHeaderSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, value);

				var transactionLinesSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
				transactionLinesSubQuery.AddToFilter(AccTransactionLinesSchema.AL_OH, value);

				var taxTransactionSubQuery = new ZDBOnlySubQuery(typeof(AccTaxTransaction), AccTaxTransactionSchema.PK);
				taxTransactionSubQuery.AddSubQuery(AccTaxTransactionSchema.ATT_AH, transactionHeaderSubQuery, JoinCondition.And);

				var taxGLMovementSubQuery = new ZDBOnlySubQuery(typeof(AccTaxGLMovement), AccTaxGLMovementSchema.PK);
				taxGLMovementSubQuery.AddSubQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransactionSubQuery, JoinCondition.And);

				var transactionLinesQuery = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));
				transactionLinesQuery.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, transactionLinesSubQuery, JoinCondition.And);

				var transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));
				transactionHeaderQuery.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transactionHeaderSubQuery, JoinCondition.And);

				var taxGLMovementQuery = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));
				taxGLMovementQuery.AddSubQuery(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, taxGLMovementSubQuery, JoinCondition.And);

				taxGLMovementQuery.AddToFilter(transactionLinesQuery, JoinCondition.Or);
				taxGLMovementQuery.AddToFilter(transactionHeaderQuery, JoinCondition.Or);

				return taxGLMovementQuery;
			}

			return null;
		}

		#region Period

		ZQuery GetPeriodQuery(ZString value1, ZString value2)
		{
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var startDate = ZDateTime.Invalid;
			var endDate = ZDateTime.Invalid;

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

		#endregion

		#region Date

		ZQuery GetPostDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, AccGeneralLedgerDataSchema.GLD_PostDate, fromDate.Date, toDate.Date);
			return query;
		}

		#endregion

		#region Transaction Date

		ZQuery GetTransactionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var result = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));

			var headerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			AddDateRange(headerSubQuery, comparisonOperator, JoinCondition.And, AccTransactionHeaderSchema.AH_InvoiceDate, fromDate.Date, toDate.Date);

			var lineSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			lineSubQuery.AddToFilter(JoinCondition.Or, AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			lineSubQuery.AddToFilter(JoinCondition.Or, AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Accrual);
			AddDateRange(lineSubQuery, comparisonOperator, JoinCondition.And, AccTransactionLinesSchema.AL_PostDate, fromDate.Date, toDate.Date);

			var taxGLMovementSubQuery = GetTaxGLMovementSubQueryFromHeader(headerSubQuery);

			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, headerSubQuery, JoinCondition.And);
			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, lineSubQuery, JoinCondition.Or);
			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, taxGLMovementSubQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region Transaction Number

		ZQuery GetTransactionNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));

			var headerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			headerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, comparisonOperator, value);

			var taxGLMovementSubQuery = GetTaxGLMovementSubQueryFromHeader(headerSubQuery);

			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, headerSubQuery, JoinCondition.And);
			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, taxGLMovementSubQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region Job Number

		ZQuery GetJobNumber(ZString fromNumber, ZString toNumber)
		{
			var result = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));

			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);

			if (!fromNumber.IsEmpty)
			{
				jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.GreaterThanOrEqualTo, fromNumber);
			}

			if (!toNumber.IsEmpty)
			{
				jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.LessThanOrEqualTo, toNumber);
			}

			var headerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			headerSubQuery.AddSubQuery(AccTransactionHeaderSchema.AH_JH, jobHeaderSubQuery, JoinCondition.And);

			var lineSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			lineSubQuery.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobHeaderSubQuery, JoinCondition.And);

			var taxGLMovementSubQuery = GetTaxGLMovementSubQueryFromHeader(headerSubQuery);

			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, headerSubQuery, JoinCondition.And);
			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, lineSubQuery, JoinCondition.Or);
			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, taxGLMovementSubQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region Sub Account Type Without Value Query

		ZQuery GetSubAccountWithoutValueQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				query.AddToFilter(AccGeneralLedgerDataSubAccountFilter.GetSubAccountQuery(value, ZGuid.Empty, true));
			}
			return query;
		}

		#endregion

		#region GL Account

		ZQuery GetGLAccountQuery(ZString value1, ZString value2)
		{
			return GetBetweenQuery<AccGLHeader>(value1, value2, AccGeneralLedgerDataSchema.GLD_AG_GLAccount, AccGLHeaderSchema.AG_AccountNum);
		}

		#endregion

		#region Ledger And Transaction Type

		ZQuery GetLedgerAndTransactionTypeQuery(ZString ledger, ZString transactionType)
		{
			var result = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));

			if (transactionType == TransactionLineTypes.WIP || transactionType == TransactionLineTypes.Accrual)
			{
				var subQueryForWIPAndACR = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccGeneralLedgerDataSchema.GLD_AL_TransactionLine);
				subQueryForWIPAndACR.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_LineType, transactionType);

				result.AddSubQuery(subQueryForWIPAndACR, JoinCondition.And);
			}
			else if (ledger == LedgerTypes.JobCosting && transactionType.IsEmpty)
			{
				var subQueryForJC = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader);
				subQueryForJC.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);

				var subQueryForWIPAndACR = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccGeneralLedgerDataSchema.GLD_AL_TransactionLine);
				subQueryForWIPAndACR.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
				subQueryForWIPAndACR.AddToFilter(JoinCondition.Or, AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Accrual);

				result.AddSubQuery(subQueryForJC, JoinCondition.Or);
				result.AddSubQuery(subQueryForWIPAndACR, JoinCondition.Or);
			}
			else
			{
				var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader);

				if (!ledger.IsEmpty)
				{
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);
				}
				if (!transactionType.IsEmpty)
				{
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionType);
				}

				result.AddSubQuery(subQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region Presentation Category

		ZQuery GetPresentationCategoryQuery(SQLComparisonOperator @operator, ZString value)
		{
			var operatorList = new List<SQLComparisonOperator>() { SQLComparisonOperator.Contains, SQLComparisonOperator.NotContains, SQLComparisonOperator.Equal, SQLComparisonOperator.NotEqual };
			var query = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));
			var categorySubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader);
			categorySubQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.General);

			if (string.IsNullOrEmpty(value) || !operatorList.Contains(@operator))
			{
				categorySubQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionCategory, @operator, value);
				query.AddSubQuery(categorySubQuery, JoinCondition.And);
				return query;
			}

			var glPresentationJournalCategoryCollection = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Cast<GLPresentationJournalCategory>();
			var childCategories = glPresentationJournalCategoryCollection.Where(x => x.Bool && x.Code.Contains(value)).Select(x => x.Code).ToList();
			if (@operator == SQLComparisonOperator.Equal || @operator == SQLComparisonOperator.NotEqual)
			{
				childCategories = childCategories.Where(x => x.Equals(value)).ToList();
			}

			if (childCategories.Count == 0)
			{
				categorySubQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionCategory, @operator, value);
				query.AddSubQuery(categorySubQuery, JoinCondition.And);
				return query;
			}
			var realOperator = @operator == SQLComparisonOperator.Contains || @operator == SQLComparisonOperator.Equal ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			categorySubQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionCategory, realOperator, childCategories);
			query.AddSubQuery(categorySubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Header Description

		ZQuery GetHeaderDescription(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));

			var headerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			headerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_Desc, comparisonOperator, value);

			var taxGLMovementSubQuery = GetTaxGLMovementSubQueryFromHeader(headerSubQuery);

			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, headerSubQuery, JoinCondition.And);
			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, taxGLMovementSubQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region Line Description

		ZQuery GetLineDescription(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));

			var lineSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			lineSubQuery.AddToFilter(AccTransactionLinesSchema.AL_Desc, comparisonOperator, value);

			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, lineSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Account Type

		ZQuery GetAccountTypeFilter(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));
			var lineSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeader), AccGLHeaderSchema.PK);

			if (value == "ALL")
			{
				lineSubQuery.AddToFilter(AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Contains, new[] { Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.AccountType.Note });
			}
			else
			{
				lineSubQuery.AddToFilter(AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, value);
			}
			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AG_GLAccount, lineSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Charge Code

		ZQuery GetChargeCode(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, object value)
		{
			var result = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));

			var lineSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);

			if (comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				lineSubQuery.AddToFilter(AccTransactionLinesSchema.AL_AC, SQLComparisonOperator.NotEqual, null);
			}
			else if (filtersMatchQuery != null)
			{
				lineSubQuery.AddSubQuery(filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				lineSubQuery.AddToFilter(AccTransactionLinesSchema.AL_AC, (ZGuid)value);
			}

			result.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, lineSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Query Helper

		ZQuery GetBetweenQuery<T>(ZString value1, ZString value2, SchemaGuidColumn schemaColumn, SchemaColumn subCchemaColumn) where T : BusinessObject
		{
			var query = string.IsNullOrEmpty(value1) ? new ZQuery() : new ZQuery(subCchemaColumn, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
			if (!string.IsNullOrEmpty(value2))
			{
				query.AddToFilter(new ZQuery(subCchemaColumn, SQLComparisonOperator.LessThanOrEqualTo, value2));
			}

			var bizos = Factory.Load<T>(query);

			var filterQuery = new ZQuery();
			filterQuery.FilterByForeignKey(schemaColumn, bizos);

			return filterQuery;
		}

		ZDBOnlySubQuery GetTaxGLMovementSubQueryFromHeader(ZDBOnlySubQuery headerSubQuery)
		{
			var taxTransactionSubQuery = new ZDBOnlySubQuery(typeof(AccTaxTransaction), AccTaxTransactionSchema.PK);
			taxTransactionSubQuery.AddSubQuery(AccTaxTransactionSchema.ATT_AH, headerSubQuery, JoinCondition.And);

			var taxGLMovementSubQuery = new ZDBOnlySubQuery(typeof(AccTaxGLMovement), AccTaxGLMovementSchema.PK);
			taxGLMovementSubQuery.AddSubQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransactionSubQuery, JoinCondition.And);

			return taxGLMovementSubQuery;
		}

		#endregion

		#endregion

		#region Filter Helper

		#region Collections

		GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}
				return fDepartments;
			}
		}

		GlbDepartmentCollection fDepartments;

		GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}

		GlbBranchCollection fBranches;

		OrgHeaderCollection Organisations
		{
			get { return organisations_List ?? (organisations_List = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection organisations_List;

		AccGLHeaderCollection GLHeaders
		{
			get { return FindboxLookupCollections.GetGLHeaders(Factory); }
		}

		public AccChargeCodeCollection ChargeCodeCollection
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		#endregion

		#region lists

		#region LedgerList

		protected CodeDescriptionPairList fLedgerList;
		public virtual CodeDescriptionPairList LedgerList
		{
			get
			{
				if (fLedgerList == null)
				{
					fLedgerList = new CodeDescriptionPairList();
					fLedgerList.AddPair(ZString.Empty, Res.GetString("Accounting|AccGeneralLedgerDataFilter|AllLedgers", "All Ledgers"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.AccountsReceivable, Res.GetString("Accounting|AccGeneralLedgerDataFilter|Receivables", "Receivables"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.AccountsPayable, Res.GetString("Accounting|AccGeneralLedgerDataFilter|Payables", "Payables"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.CashBook, Res.GetString("Accounting|AccGeneralLedgerDataFilter|CashBooks", "Cash Books"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.JobCosting, Res.GetString("Accounting|AccGeneralLedgerDataFilter|JobCosting", "Job Costing"));
					fLedgerList.AddPair(ZArchitecture.Core.LedgerTypes.General, Res.GetString("Accounting|AccGeneralLedgerDataFilter|GeneralLedger", "General Ledger"));
				}
				return fLedgerList;
			}
		}

		#endregion

		#region PresentationCategoryList

		protected CodeDescriptionPairList fPresentationCategoryList;
		public CodeDescriptionPairList PresentationCategoryList
		{
			get
			{
				if (fPresentationCategoryList == null)
				{
					fPresentationCategoryList = new CodeDescriptionPairList(AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList());
					fPresentationCategoryList.AddRangeOverwriteIfExists(AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList());
				}

				return fPresentationCategoryList;
			}
		}

		#endregion

		#region AccountTypes

		public CodeDescriptionPairList AccountTypes
		{
			get
			{
				if (fAccountTypes == null)
				{
					fAccountTypes = new CodeDescriptionPairList();
					fAccountTypes.AddPair("ALL", Res.GetString("Accounting|AccGeneralLedgerDataFilter|All", "All"));
					fAccountTypes.AddPair(Core.Constants.AccountType.BalanceSheetAccount, Res.GetString("Accounting|AccGeneralLedgerDataFilter|BalanceSheet", "Balance Sheet"));
					fAccountTypes.AddPair(Core.Constants.AccountType.ProfitAndLossAccount, Res.GetString("Accounting|AccGeneralLedgerDataFilter|ProfitLoss", "Profit & Loss"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Note, Res.GetString("Accounting|AccGeneralLedgerDataFilter|Note", "Note"));
				}
				return fAccountTypes;
			}
		}

		CodeDescriptionPairList fAccountTypes;

		#endregion

		#region SubAccountTypeList

		CodeDescriptionPairList fSubAccountTypeList;
		public CodeDescriptionPairList SubAccountTypeList
		{
			get
			{
				if (fSubAccountTypeList == null)
				{
					fSubAccountTypeList = new AccountingMasterFilesConstants.SubAccountTypeList();
				}

				return fSubAccountTypeList;
			}
		}

		#endregion

		#region

		protected RefCurrencyCollection fCurrencyList;
		public RefCurrencyCollection CurrencyList
		{
			get
			{
				if (fCurrencyList == null)
				{
					fCurrencyList = new RefCurrencyCollection(Factory);
				}
				return fCurrencyList;
			}
		}

		#endregion

		#endregion

		void ValidateAccountingPeriod(ZPropertyInfo info)
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

		void RemoveUnexpectedComparisonOperators<T>(ModuleFilterWithListAndComparisonOperators<T> filter)
			where T : IZType
		{
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
		}

		#endregion

		#region Financial Details Category

		FilterCategory fFinancialDetailsCategory;
		protected FilterCategory FinancialDetailsCategory
		{
			get
			{
				if (fFinancialDetailsCategory == null)
				{
					fFinancialDetailsCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|AccGeneralLedgerDataFilter|FinancialDetails", "Financial Details"));
				}
				return fFinancialDetailsCategory;
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		const string SubAccountFilterText = "Sub Account + Value";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		const string SubAccountWithoutValueFilterText = "Sub Account + No Value";
	}
}
