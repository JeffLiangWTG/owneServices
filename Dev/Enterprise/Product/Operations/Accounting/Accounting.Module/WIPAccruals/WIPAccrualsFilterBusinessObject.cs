using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class WIPAccrualsFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public WIPAccrualsFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddReferenceFilters(filters);
			AddOtherFilters(filters);
			AddBranchManagementCodeFilter(filters);
			return filters;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var filter = new ModuleNumberFilter(AccountingUtils.NumberFilterTypes.JobNumber, QueryJobNumberDelegate)
			{
				MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|Job", AccountingUtils.NumberFilterTypes.JobNumber)
			};

			return filter;
		}

		ZQuery QueryJobNumberDelegate(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(BaseWIPAccrual));
			var subQueryJobNumber = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionLinesSchema.AL_JH);
			subQueryJobNumber.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobNum, comparisonOperator, value);

			// This condition is a workaround for "exclusive" filter missing the default "Not Reversed" filter. This condition should be removed.
			// See: "WI00696867 - DHL: New filter to search for multiple jobs (2 filter)". Task: "Apply a workaround for WIPs 'Job #' filter"
			// Details: "Not Reversed" is added implicitly to all filters,  see method AddOtherFilters() line "showReversedTransactionsFilter.Visibility = FilterVisibility.AlwaysApplied;".
			// "exclusive" filter overwrites all other filters, so all Jobs (Reversed and Not Reversed) are included into the result if using "Job #".
			subQueryJobNumber.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.Equal, null);

			query.AddSubQuery(subQueryJobNumber, JoinCondition.And);

			return query;
		}

		#region FinancialDetails Category

		FilterCategory fFinancialDetailsCategory;
		protected FilterCategory FinancialDetailsCategory
		{
			get
			{
				if (fFinancialDetailsCategory == null)
				{
					fFinancialDetailsCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|FinancialDetails", "Financial Details"));
				}
				return fFinancialDetailsCategory;
			}
		}

		#endregion

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter("Consol #", GetConsolNumberQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|Consol", "Consol #");
			filters.AddTextRangeFilter("Job Number Range", GetJobNumberRangeQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|JobNumberRange", "Job Number Range");
			filters.AddNumberFilter("Job Local Reference", GetLocalJobReferenceQuery)
				.WithMaxLengthOf(JobHeaderSchema.JH_JobLocalReference)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|LocalJobReference", "Job Local Reference");
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetJobNumberRangeQuery(ZString fromNumber, ZString toNumber)
		{
			var query = new ZDBOnlyQuery(typeof(BaseWIPAccrual));

			if (!fromNumber.IsEmpty || !toNumber.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionLinesSchema.AL_JH);

				if (!fromNumber.IsEmpty)
				{
					subQuery.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.GreaterThanOrEqualTo, fromNumber);
				}

				if (!toNumber.IsEmpty)
				{
					subQuery.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.LessThanOrEqualTo, toNumber);
				}

				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionToFilter", Justification = "Baseline")]
		ZQuery GetConsolNumberQuery(SQLComparisonOperator @operator, ZString consolID)
		{
			var consolQuery = new ZDBOnlyQuery(typeof(BaseWIPAccrual));

			var parameters = AccountingUtils.GetParametersForMultiSearchOptimizationIfApplicable(@operator, consolID);

			foreach (var singleNumber in parameters.multiNumber)
			{
				var innerQuery = new ZDBOnlyQuery(typeof(BaseWIPAccrual));
				var operatorString = parameters.usingOptimization ? parameters.operatorString : @operator.ComparisonText(singleNumber);
				var valueString = parameters.usingOptimization ? singleNumber : (ZString)"@ConsolID";

				FormattableString formattableString = $@"{AccTransactionLinesSchema.AL_JH.Name} in 
							(
							Select {JobHeaderSchema.PK.Name}
								From {JobConsolSchema.Constants.SqlSchemaName}.{JobConsolSchema.Constants.TableName}
								Inner Join {JobConShipLinkSchema.Constants.SqlSchemaName}.{JobConShipLinkSchema.Constants.TableName} on {JobConShipLinkSchema.JN_JK.Name} = {JobConsolSchema.PK.Name}
								Inner Join {JobShipmentSchema.Constants.SqlSchemaName}.{JobShipmentSchema.Constants.TableName} on {JobConShipLinkSchema.JN_JS.Name} = {JobShipmentSchema.PK.Name}
								Inner Join {JobHeaderSchema.Constants.SqlSchemaName}.{JobHeaderSchema.Constants.TableName} on {JobShipmentSchema.PK.Name} = {JobHeaderSchema.JH_ParentID.Name}
								Where {JobConsolSchema.JK_UniqueConsignRef.Name} {operatorString} {valueString} AND {JobHeaderSchema.JH_GC.Name} = @CompanyPK )";  // This is Direct SQL hit to the DB, and used string in order to help with readablity. Failure will be manifested through Unit Test
				ZString sQL = formattableString.ToString(CultureInfo.InvariantCulture);
				var @params = new ZSqlParameterCollection();
				if (!parameters.usingOptimization)
				{
					@params.Add("@ConsolID", @operator.EscapedSqlValue(consolID), JobConsolSchema.JK_UniqueConsignRef);
				}
				@params.Add("@CompanyPK", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);
				consolQuery.AddFilterAndZSQLParameterCollection(sQL, @params);

				consolQuery.AddToFilter(innerQuery, @operator.IsNegativeSQLOperator() ? JoinCondition.And : JoinCondition.Or);
			}
			return consolQuery;
		}

		ZQuery GetLocalJobReferenceQuery(SQLComparisonOperator @operator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(BaseWIPAccrual));
			var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionLinesSchema.AL_JH);
			subQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobLocalReference, @operator, value);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.PostDate, GetAL_PostDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|PostDate", "Post Date");
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.ReverseDate, GetAL_ReverseDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|ReverseDate", "Reverse Date");
		}

		#endregion

		#region Date Filters Querys

		ZQuery GetAL_PostDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetZQueryFor2DateTime(comparisonOperator, AccTransactionLinesSchema.AL_PostDate, date1, date2);
		}

		ZQuery GetAL_ReverseDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetZQueryFor2DateTime(comparisonOperator, AccTransactionLinesSchema.AL_ReverseDate, date1, date2);
		}

		ZQuery GetZQueryFor2DateTime(DateComparisonOperator comparisonOperator, SchemaDateTimeColumn column, ZDateTime date1, ZDateTime date2)
		{
			var filter = new ZQuery();
			AddDateTimeRange(filter, comparisonOperator, JoinCondition.And, column, date1, date2);
			return filter;
		}

		#endregion

		#region Reference Filter

		void AddReferenceFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionLinesSchema.AL_GB, BranchCollection);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|Branch", "Branch");

			filter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionLinesSchema.AL_GE, DepartmentCollection);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|Department", "Department");

			filter = filters.AddGuidFilter("Account", ModuleIDs.Organisation, AccTransactionLinesSchema.AL_OH, OrganisationCollection);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|Account", "Account");

			filter = filters.AddGuidFilter("Charge Code", ModuleIDs.AccChargeCode, AccTransactionLinesSchema.AL_AC, ChargeCodeCollection);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|ChargeCode", "Charge Code");
		}

		#endregion

		#region Reference Filter Delegates

		#region GetBranchManagementCodeQuery

		protected override ZQuery GetBranchManagementCodeQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionLines));

			if (!value.IsEmpty)
			{
				var branchManagementCodeQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchManagementCodeQuery.AddToFilter(GlbBranchSchema.GB_AccountingGroupCode, value);
				result.AddSubQuery(AccTransactionLinesSchema.AL_GB, branchManagementCodeQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#endregion

		#region Other Filters

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddTextFilter("Transaction Type", GetTransactionTypeQuery, TransactionTypeList);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|TransactionType", "Transaction Type");

			filter = filters.AddNumberRangeFilter("Amount", GetAmountQuery);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|Amount", "Amount");

			var showReversedTransactionsFilter = filters.AddTextFilter("Show Reversed Transactions", GetShowReversedTransactionsQuery, ShowReversedTransactionsList);
			showReversedTransactionsFilter.Category = FilterCategories.ModesAndTypes;
			showReversedTransactionsFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|ShowReversedTransactions", "Show Reversed Transactions");
			showReversedTransactionsFilter.DefaultProperty = (NoResString)"Not Reversed";
			showReversedTransactionsFilter.Visibility = FilterVisibility.AlwaysApplied;

			if (AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				var orphanTransactionsFilter = filters.AddFlagsFilter("Show Only Orphan Transactions", new string[] { Res.GetString("Accounting|WIPAccrualsFilter|ShowOrphanTransactionOnly", "Yes") }, new GetFlagsQuery[] { GetOrphanTransactionsOnlyQuery });
				orphanTransactionsFilter.Category = FilterCategories.ModesAndTypes;
				orphanTransactionsFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|WIPAccrualsFilter|ShowOnlyOrphanTransactions", "Show Only Orphan Transactions");
			}

			filters.AddFilter(new BillingJobParentModuleFilter(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_JH, Factory));
		}

		#endregion

		#region Other Filter Delegates

		ZQuery GetTransactionTypeQuery(ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetAmountQuery(INumericZType value1, INumericZType value2)
		{
			return ModuleNumberRangeFilter.AddToFilters(new ZQuery(), AccTransactionLinesSchema.AL_LineAmount, (ZDecimal)(-1 * (ZDecimal)value2), (ZDecimal)(-1 * (ZDecimal)value1));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		ZQuery GetShowReversedTransactionsQuery(ZString value)
		{
			var query = new ZQuery();
			if (value == "Not Reversed")
			{
				query.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.Equal, null);
			}
			if (value == "Reversed")
			{
				query.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.NotEqual, null);
			}
			return query;
		}

		ZQuery GetOrphanTransactionsOnlyQuery(ZBool thisOptionChecked)
		{
			var filter = new ZQuery();
			if (thisOptionChecked)
			{
				filter.AddToFilter(AccountingUtils.GetOrphanWIPsOrAccrualsFilterQuery());
			}

			return filter;
		}

		#endregion

		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;
				var wIPFilter = new ZQuery(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionLineTypes.WIP);
				var aCRFilter = new ZQuery(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionLineTypes.Accrual);
				filter.AddToFilter(new ZQuery(wIPFilter, JoinCondition.Or, aCRFilter));
				filter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
				return filter;
			}
		}

		#region Lookups

		protected CodeDescriptionPairList fShowReversedTransactionsList;
		public CodeDescriptionPairList ShowReversedTransactionsList
		{
			get
			{
				if (fShowReversedTransactionsList == null)
				{
					fShowReversedTransactionsList = new CodeDescriptionPairList();
					fShowReversedTransactionsList.AddPair((NoResString)"Not Reversed", Res.GetString("Accounting|WIPAccrualsFilter|ShowNotReversedTransactionsOnly", "Show not reversed transactions only"));
					fShowReversedTransactionsList.AddPair((NoResString)"Reversed", Res.GetString("Accounting|WIPAccrualsFilter|ShowReversedTransactionsOnly", "Show reversed transactions only"));
					fShowReversedTransactionsList.AddPair((NoResString)"All", Res.GetString("Accounting|WIPAccrualsFilter|DisplayAllTransactions", "Display all transactions"));
				}
				return fShowReversedTransactionsList;
			}
		}

		public GlbBranchDependentCollection BranchCollection
		{
			get { return FindboxLookupCollections.GetCompanyBranchesCollection(Factory); }
		}

		public GlbDepartmentCollection DepartmentCollection
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		public OrgHeaderCollection OrganisationCollection
		{
			get { return FindboxLookupCollections.GetOrgHeaderCollection(Factory); }
		}

		public AccChargeCodeCollection ChargeCodeCollection
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		public CodeDescriptionPairList TransactionTypeList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.WIPAccrualTransactionTypes); }
		}

		#endregion
	}
}
