using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;
using ResString = Enterprise.Accounting.Module.ResString;

namespace Enterprise.Accounting.Business
{
	public class CreditControlledDocumentsApprovalFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddDateFilters(filters);
			AddReferenceFilters(filters);
			AddOtherFilters(filters);

			return filters;
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Approval Date", GenApprovalRequestSchema.XP_ApprovalDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|CreditControlledDocumentsApprovalFilter|ApprovalDate", "Approval Date");
		}

		void AddReferenceFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNkFilter("Approving User", GenApprovalRequestSchema.XP_GS_NKApprovingUser1, ModuleIDs.GlbStaff, StaffList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CreditControlledDocumentsApprovalFilter|ApprovingUser", "Approving User");
		}

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddTextFilter("Approval Status", GetApprovalStatusQuery, ApprovalStatusList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CreditControlledDocumentsApprovalFilter|ApprovalStatus", "Approval Status");

			ModuleFilter approvalRequiredFilter = filters.AddTextFilter("Approval Required", GetApprovalRequiredQuery, ApprovalRequiredList);
			approvalRequiredFilter.Category = FilterCategories.StatusAndFlags;
			approvalRequiredFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CreditControlledDocumentsApprovalFilter|ApprovalRequired", "Approval Required");

			ModuleFilter requestIdFilter = filters.AddTextFilter("Reference Number", GetRequestIDQuery);
			requestIdFilter.Category = FilterCategories.NumbersAndReferences;
			requestIdFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|CreditControlledDocumentsApprovalFilter|RequestID", "Reference Number");
		}

		ZQuery GetRequestIDQuery(SQLComparisonOperator sqlOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				return new ZQuery(GenApprovalRequestSchema.XP_RequestID, sqlOperator, value);
			}
			else
			{
				return null;
			}
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

		ZQuery GetApprovalRequiredQuery(ZString value)
		{
			if (!value.IsEmpty)
			{
				var authorizationWeight = AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(value);

				return new ZQuery(GenApprovalRequestSchema.XP_PrivledgeRequired, SQLComparisonOperator.Contains, authorizationWeight.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				return null;
			}
		}

		CodeDescriptionPairList ApprovalStatusList
		{
			get
			{
				if (ApprovalStatusList_cached == null)
				{
					ApprovalStatusList_cached = new CodeDescriptionPairList();
					ApprovalStatusList_cached.AddPair(Constants.GenApprovalRequestApprovalStatus.Requested, ResString.GetMultilingualString("56cdefe8-3c00-490b-9f51-499949a36280", "Requested"));
					ApprovalStatusList_cached.AddPair(Constants.GenApprovalRequestApprovalStatus.Cancelled, ResString.GetMultilingualString("22692318-739c-403c-909c-a67347b9a7f1", "Canceled"));
					ApprovalStatusList_cached.AddPair(Constants.GenApprovalRequestApprovalStatus.Rejected, ResString.GetMultilingualString("237bcd81-dccb-437e-8cbc-f7ac22060ba0", "Rejected"));
					ApprovalStatusList_cached.AddPair(Constants.GenApprovalRequestApprovalStatus.Approved, ResString.GetMultilingualString("3f41b44a-3b69-4dfe-93eb-d3d5582d5f15", "Approved"));
				}
				return ApprovalStatusList_cached;
			}
		}
		CodeDescriptionPairList ApprovalStatusList_cached;

		CodeDescriptionPairList ApprovalRequiredList
		{
			get
			{
				if (ApprovalRequiredList_cached == null)
				{
					ApprovalRequiredList_cached = new CodeDescriptionPairList();
					ApprovalRequiredList_cached.AddPair(AuthorisationRequirementCodes.FirstApprovalRequiredOnly, AuthorisationRequirementDescriptions.FirstApprovalRequiredOnly);
					ApprovalRequiredList_cached.AddPair(AuthorisationRequirementCodes.SecondApprovalRequiredOnly, AuthorisationRequirementDescriptions.SecondApprovalRequiredOnly);
					ApprovalRequiredList_cached.AddPair(AuthorisationRequirementCodes.ThirdApprovalRequiredOnly, AuthorisationRequirementDescriptions.ThirdApprovalRequiredOnly);
				}
				return ApprovalRequiredList_cached;
			}
		}
		CodeDescriptionPairList ApprovalRequiredList_cached;

		GlbStaffCollection StaffList
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
