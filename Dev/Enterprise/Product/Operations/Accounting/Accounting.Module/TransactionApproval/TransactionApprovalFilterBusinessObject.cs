using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class TransactionApprovalFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddDateFilters(filters);
			AddReferenceFilters(filters);
			AddOtherFilters(filters);
			AddBranchManagementCodeFilter(filters);
			AddJobBranchCodeFilter(filters);
			AddJobDepartmentCodeFilter(filters);
			return filters;
		}

		protected virtual void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Approval Date", GenApprovalRequestSchema.XP_ApprovalDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|ApprovalDate", "Approval Date");
		}

		protected virtual void AddReferenceFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNkFilter("Approving User", GenApprovalRequestSchema.XP_GS_NKApprovingUser1, ModuleIDs.GlbStaff, StaffList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|ApprovingUser", "Approving User");
		}

		GlbBranchCollection Branches
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

		GlbDepartmentCollection Departments
		{
			get { return DepartmentCollection_cached ?? (DepartmentCollection_cached = new GlbDepartmentCollection(Factory)); }
		}
		GlbDepartmentCollection DepartmentCollection_cached;

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleFilter statusfilter = filters.AddTextFilter("Approval Status", GetApprovalStatusQuery, ApprovalStatusList);
			statusfilter.Category = FilterCategories.StatusAndFlags;
			statusfilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|ApprovalStatus", "Approval Status");

			ModuleFilter reasonCodefilter = filters.AddTextFilter("Reason Code", GetReasonCodeQuery, ReasonCodeList);
			reasonCodefilter.Category = FilterCategories.Other;
			reasonCodefilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|ReasonCode", "Reason Code");
		}

		#region Add JobBranch JobDepartment Filter
		protected void AddJobBranchCodeFilter(ModuleFilterCollection filters)
		{
			var jobBranchCodeFilter = filters.AddGuidFilter("Job Branch Code", ModuleIDs.GlbBranch, GenApprovalRequestSchema.XP_GB_JobBranch, Branches);
			jobBranchCodeFilter.Category = FilterCategories.Other;
			jobBranchCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|JobBranchCode", "Job Branch Code");
		}

		protected void AddJobDepartmentCodeFilter(ModuleFilterCollection filters)
		{
			var jobDepartmentCodeFilter = filters.AddGuidFilter("Job Department Code", ModuleIDs.GlbDepartment, GenApprovalRequestSchema.XP_GE_JobDepartment, Departments);
			jobDepartmentCodeFilter.Category = FilterCategories.Other;
			jobDepartmentCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|JobDepartmentCode", "Job Department Code");
		}
		#endregion

		#region GetBranchManagementCodeQuery

		protected override ZQuery GetBranchManagementCodeQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			if (!value.IsEmpty)
			{
				var branchManagementCodeQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchManagementCodeQuery.AddToFilter(GlbBranchSchema.GB_AccountingGroupCode, value);
				result.AddSubQuery(GenApprovalRequestSchema.XP_GB_RequestingBranch, branchManagementCodeQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		ZQuery GetReasonCodeQuery(ZString value)
		{
			if (!value.IsEmpty)
			{
				return new ZQuery(GenApprovalRequestSchema.XP_ReasonCode, SQLComparisonOperator.Equal, value);
			}
			else
			{
				return null;
			}
		}

		ReadOnlyCodeDescriptionPairList ReasonCodeList
		{
			get { return ReasonCodeList_cached ?? (ReasonCodeList_cached = GetReasonCodeList()); }
		}
		ReadOnlyCodeDescriptionPairList ReasonCodeList_cached;

		protected virtual ReadOnlyCodeDescriptionPairList GetReasonCodeList()
		{
			return GenApprovalRequestHelper.GetAllReasonCodeList;
		}

		ZQuery GetApprovalStatusQuery(ZString value)
		{
			if (!value.IsEmpty)
			{
				return new ZQuery(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.Equal, value);
			}
			else
			{
				return null;
			}
		}

		CodeDescriptionPairList ApprovalStatusList
		{
			get { return ApprovalStatusList_cached ?? (ApprovalStatusList_cached = GetApprovalStatusList()); }
		}
		CodeDescriptionPairList ApprovalStatusList_cached;

		protected virtual CodeDescriptionPairList GetApprovalStatusList()
		{
			var list = GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList;
			list.RemoveCode(Constants.GenApprovalRequestApprovalStatus.Error);

			return list;
		}

		protected GlbStaffCollection StaffList
		{
			get
			{
				if (StaffList_cached == null)
				{
					StaffList_cached = new GlbStaffCollection(Factory);
				}
				return StaffList_cached;
			}
		}
		GlbStaffCollection StaffList_cached;
	}
}
