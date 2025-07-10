using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class ARCreditNoteApprovalFilterBusinessObject : InvoicingBaseApprovalFilterBusinessObject
	{
		protected override void GetJobFilterMultilingualDescription(ModuleFilter filter)
		{
			filter.MultilingualDescription = ResString.GetMultilingualString("6d43b1a0-09f9-4c1f-b23e-90ba012e1d0c", "Job/Invoice #");
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddModesAndTypesFilters(filters);
			return filters;
		}

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			var approvalTypeFilter = filters.AddTextFilter("Approval Type", GetTextQuery, ApprovalTypeList);
			approvalTypeFilter.Category = FilterCategories.ModesAndTypes;
			approvalTypeFilter.MultilingualDescription = ResString.GetMultilingualString("f5010880-87d5-4ba6-9ddf-4c59b809784d", "Approval Type");
			approvalTypeFilter.MaxLength = GenApprovalRequestSchema.XP_ApprovalType.MaxLength;
		}

		ZQuery GetTextQuery(ZString value)
		{
			var result = new ZQuery();
			result.AddToFilter(GenApprovalRequestSchema.XP_ApprovalType, value);
			return result;
		}

		CodeDescriptionPairList fApprovalTypeList;
		CodeDescriptionPairList ApprovalTypeList
		{
			get
			{
				if (fApprovalTypeList == null)
				{
					fApprovalTypeList = new CodeDescriptionPairList();
					fApprovalTypeList.AddPair(Core.Constants.GenApprovalRequestApprovalType.ARCreditNote, Res.GetString("a68bb567-cd94-41e9-bbf5-a0fdf0ae4fba", "AR Credit Note"));
					fApprovalTypeList.AddPair(Core.Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal, Res.GetString("04ebab70-488d-4e35-9516-a429857bf232", "AR Credit Note for reversal"));
				}
				return fApprovalTypeList;
			}
		}

		protected override void AddReferenceFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter("Approving User", GetApprovingUserQuery, ModuleIDs.GlbStaff, StaffList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARCreditNoteApprovalFilterBusinessObject|ApprovingUser", "Approving User");

			if (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.Value.AuthorizationMode == AuthorizationMode.Codes.TwoApprovers)
			{
				filter = filters.AddNkFilter("Approving User 1", GenApprovalRequestSchema.XP_GS_NKApprovingUser1, ModuleIDs.GlbStaff, StaffList);
				filter.Category = FilterCategories.Organisations;
				filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARCreditNoteApprovalFilterBusinessObject|ApprovingUser1", "Approving User 1");

				filter = filters.AddNkFilter("Approving User 2", GenApprovalRequestSchema.XP_GS_NKApprovingUser2, ModuleIDs.GlbStaff, StaffList);
				filter.Category = FilterCategories.Organisations;
				filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARCreditNoteApprovalFilterBusinessObject|ApprovingUser2", "Approving User 2");
			}
		}

		ZQuery GetApprovingUserQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));
			query.AddToFilter(GenApprovalRequestSchema.XP_GS_NKApprovingUser1, SQLComparisonOperator.Equal, value);
			query.AddToFilter(JoinCondition.Or, GenApprovalRequestSchema.XP_GS_NKApprovingUser2, SQLComparisonOperator.Equal, value);
			return query;
		}
	}
}
