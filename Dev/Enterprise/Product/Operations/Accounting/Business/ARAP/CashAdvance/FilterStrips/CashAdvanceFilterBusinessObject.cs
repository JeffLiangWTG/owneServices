using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public abstract class CashAdvanceFilterBusinessObject : FilterStripBusinessObject
	{
		protected abstract ZString LedgerType { get; }

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.AddToFilter(AccCashAdvanceRequestHeaderSchema.CAH_GC_Company, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccCashAdvanceRequestHeaderSchema.CAH_Ledger, LedgerType);
				return query;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddOrganisationFilter(filters);

			var currencyFilter = filters.AddNkFilter("Currency", AccCashAdvanceRequestHeaderSchema.CAH_RX_NKTransactionCurrency, ModuleIDs.RefCurrency, CAH_RXList);
			currencyFilter.Category = FilterCategories.FinancialDetails;
			currencyFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashAdvanceFilter|Currency", "Currency");

			var osAmountfilter = filters.AddNumberRangeFilter("OS Amount", AccCashAdvanceRequestHeaderSchema.CAH_OSAmount);
			osAmountfilter.Category = FilterCategories.FinancialDetails;
			osAmountfilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashAdvanceFilter|OSAmount", "OS Amount");

			AddStatusFilter(filters);

			var cahNumberfilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.CashAdvanceNumber, AccCashAdvanceRequestHeaderSchema.CAH_RequestReferenceNumber);
			cahNumberfilter.Category = FilterCategories.NumbersAndReferences;
			cahNumberfilter.MaxLength = AccCashAdvanceRequestHeaderSchema.CAH_RequestReferenceNumber.MaxLength;
			cahNumberfilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashAdvanceFilter|CashAdvanceReferenceRenamed", "Advance Payment #");

			var jobNumberfilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.JobNumber, JobHeaderSchema.JH_JobNum);
			jobNumberfilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberfilter.MaxLength = JobHeaderSchema.JH_JobNum.MaxLength;
			jobNumberfilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashAdvanceFilter|JobNumberReference", "Job #");
			jobNumberfilter.SubGroup = new JobNumberSubGroup();

			var jobBranchFilter = filters.AddGuidFilter("Job Branch", ModuleIDs.GlbBranch, GetJobBranch, JobBranchCollection);
			jobBranchFilter.Category = FilterCategories.Organisations;
			jobBranchFilter.MaxLength = JobHeaderSchema.JH_GB.MaxLength;
			jobBranchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashAdvanceFilter|JobBranch", "Job Branch");

			var jobDepartmentFilter = filters.AddGuidFilter("Job Department", ModuleIDs.GlbDepartment, GetJobDepartment, JobDepartmentCollection);
			jobDepartmentFilter.Category = FilterCategories.Organisations;
			jobDepartmentFilter.MaxLength = JobHeaderSchema.JH_GE.MaxLength;
			jobDepartmentFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashAdvanceFilter|JobDepartment", "Job Department");

			return filters;
		}

		void AddOrganisationFilter(ModuleFilterCollection moduleFilters)
		{
			var filter = moduleFilters.AddGuidFilter("Organisation", ModuleIDs.Organisation, AccCashAdvanceRequestHeaderSchema.CAH_OH_Organization, CAH_OH_OrganizationList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashAdvanceFilter|Organisation", "Organization");
		}

		protected virtual void AddStatusFilter(ModuleFilterCollection moduleFilters)
		{
			var statusFilter = moduleFilters.AddTextFilter("Status", AccCashAdvanceRequestHeaderSchema.CAH_Status, () => CAH_StatusList);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CashAdvanceFilter|Status", StatusFilterDescription);
		}

		public AccCashAdvanceRequestHeaderCollection RequestCashAdvanceCollection => fCashAdvanceRequestCollection ?? (fCashAdvanceRequestCollection = new AccCashAdvanceRequestHeaderCollection(Factory));
		AccCashAdvanceRequestHeaderCollection fCashAdvanceRequestCollection;

		public AccCashAdvanceRequestLineCollection CashAdvanceLines => fCashAdvanceLines ?? (fCashAdvanceLines =  new AccCashAdvanceRequestLineCollection(Factory));
		AccCashAdvanceRequestLineCollection fCashAdvanceLines;

		public OrgHeaderCollection CAH_OH_OrganizationList => fCAH_OH_OrganizationList ?? (new OrgHeaderCollection(Factory));
		protected OrgHeaderCollection fCAH_OH_OrganizationList;

		public RefCurrencyCollection CAH_RXList => fCAH_RXList ?? (new RefCurrencyCollection(Factory));
		protected RefCurrencyCollection fCAH_RXList;

		public CodeDescriptionPairList CAH_StatusList => fCAH_StatusList ?? (fCAH_StatusList = GetStatusCodesList());
		protected CodeDescriptionPairList fCAH_StatusList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter")]
		protected const string StatusFilterDescription = "Status";

		public GlbBranchDependentCollection JobBranchCollection => FindboxLookupCollections.GetCompanyBranchesCollection(Factory);

		protected ZQuery GetJobBranch(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(AccCashAdvanceRequestHeader));
			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccCashAdvanceRequestHeaderSchema.CAH_JH_Job);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GB, value);
			query.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);
			return query;
		}

		public GlbDepartmentCollection JobDepartmentCollection => FindboxLookupCollections.GetDepartmentCollection(Factory);

		protected ZQuery GetJobDepartment(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(AccCashAdvanceRequestHeader));
			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccCashAdvanceRequestHeaderSchema.CAH_JH_Job);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GE, value);
			query.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);
			return query;
		}

		CodeDescriptionPairList GetStatusCodesList()
		{
			var statusCode = CashAdvanceStatusCodes.RequestHeader.CodesList;

			statusCode.RemoveCode(CashAdvanceStatusCodes.RequestHeader.PartiallyPaid);
			statusCode.RemoveCode(CashAdvanceStatusCodes.RequestHeader.Pending);

			return statusCode;
		}
	}

	public class JobNumberSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var query = new ZDBOnlyQuery(typeof(AccCashAdvanceRequestHeader));
			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccCashAdvanceRequestHeaderSchema.CAH_JH_Job);
			jobHeaderSubQuery.AddToFilter(filter);
			query.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);
			return query;
		}
	}
}
