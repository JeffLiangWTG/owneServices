using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class JobRevenueJournalFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public JobRevenueJournalFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddReferenceFilters(filters);

			return filters;
		}

		#region Number Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddTextRangeFilter(AccountingUtils.NumberFilterTypes.JobNumber, GetJobNumberQuery)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobRevenueJournalFilter|JobNumber", "Job Number Range");
			filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.TransactionNumber, AccTransactionHeaderSchema.AH_TransactionNum)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobRevenueJournalFilter|TransactionNumber", "Transaction #");
			filters.AddNumberFilter("Job Local Reference", GetLocalJobReferenceQuery)
				.WithMaxLengthOf(JobHeaderSchema.JH_JobLocalReference)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|LocalJobReference", "Job Local Reference");
		}

		ZQuery GetLocalJobReferenceQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobRevenueJournal));
			ZDBOnlySubQuery transactionLinesSubQuery = new ZDBOnlySubQuery(typeof(JobRevenueJournalLine), AccTransactionLinesSchema.AL_AH);
			ZDBOnlySubQuery jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			jobHeaderSubQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobLocalReference, @operator, value);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			transactionLinesSubQuery.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobHeaderSubQuery, JoinCondition.And);
			result.AddSubQuery(transactionLinesSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetJobNumberQuery(ZString fromNumber, ZString toNumber)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobRevenueJournal));

			if (!fromNumber.IsEmpty || !toNumber.IsEmpty)
			{
				ZDBOnlySubQuery transactionLinesSubQuery = new ZDBOnlySubQuery(typeof(JobRevenueJournalLine), AccTransactionLinesSchema.AL_AH);
				ZDBOnlySubQuery jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
				jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

				if (!fromNumber.IsEmpty)
				{
					jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.GreaterThanOrEqualTo, fromNumber);
				}

				if (!toNumber.IsEmpty)
				{
					jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.LessThanOrEqualTo, toNumber);
				}

				transactionLinesSubQuery.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobHeaderSubQuery, JoinCondition.And);
				result.AddSubQuery(transactionLinesSubQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(AccountingUtils.DateFilterTypes.PostDate, GetAH_PostDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|JobRevenueJournalFilter|PostDate", "Post Date");
			filters.AddDateFilter(AccountingUtils.DateFilterTypes.TransactionDate, GetAH_InvoiceDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|JobRevenueJournalFilter|TransactionDate", "Transaction Date");
		}

		ZQuery GetAH_PostDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetZQueryFor2DateTime(comparisonOperator, AccTransactionHeaderSchema.AH_PostDate, date1, date2);
		}

		ZQuery GetAH_InvoiceDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetZQueryFor2DateTime(comparisonOperator, AccTransactionHeaderSchema.AH_InvoiceDate, date1, date2);
		}

		ZQuery GetZQueryFor2DateTime(DateComparisonOperator comparisonOperator, SchemaDateTimeColumn column, ZDateTime date1, ZDateTime date2)
		{
			ZQuery filter = new ZQuery();
			AddDateTimeRange(filter, comparisonOperator, JoinCondition.And, column, date1, date2);
			return filter;
		}

		#endregion

		#region Reference Filter

		void AddReferenceFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, BranchCollection);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobRevenueJournalFilter|Branch", "Branch");
			if (!Env.Security.JobRevenueJournalViewingNonLoginBranchTransactions.IsAllowed)
			{
				branchFilter.DefaultProperty = GlbBranch.CurrentBranch.PK;
				branchFilter.Visibility = FilterVisibility.AlwaysVisible;
				branchFilter.ReadOnly = true;
			}

			ModuleGuidFilter departmentFilter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, DepartmentCollection);
			departmentFilter.Category = FilterCategories.Organisations;
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobRevenueJournalFilter|Department", "Department");
		}

		#endregion

		#region Lookups

		public GlbBranchCollection BranchCollection
		{
			get
			{
				if (BranchCollection_cached == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					BranchCollection_cached = new GlbBranchCollection(Factory, filter);
				}

				return BranchCollection_cached;
			}
		}
		GlbBranchCollection BranchCollection_cached;

		public GlbDepartmentCollection DepartmentCollection
		{
			get { return DepartmentCollection_cached ?? (DepartmentCollection_cached = new GlbDepartmentCollection(Factory)); }
		}
		GlbDepartmentCollection DepartmentCollection_cached;

		#endregion
	}
}
