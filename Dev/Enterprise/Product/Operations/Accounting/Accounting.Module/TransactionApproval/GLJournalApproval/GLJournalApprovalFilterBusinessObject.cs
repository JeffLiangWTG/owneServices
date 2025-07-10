using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class GLJournalApprovalFilterBusinessObject : TransactionApprovalFilterBusinessObject
	{
		public override ZQuery Filter
		{
			get
			{
				var isShowAllUsersRequestsAllowed = Env.Security.GLJournalApprovalShowAllUsersRequests.IsAllowed;

				if (isShowAllUsersRequestsAllowed != wasShowAllUsersRequestsAllowedWhenLastFilterCreated)
				{
					ModuleFilters.InvalidateCachedQuery();
				}

				wasShowAllUsersRequestsAllowedWhenLastFilterCreated = isShowAllUsersRequestsAllowed;
				var filter = base.Filter;

				if (!isShowAllUsersRequestsAllowed)
				{
					filter.AddToFilter(GenApprovalRequestSchema.XP_SystemCreateUser, GlbStaff.CurrentUser.GS_Code);
				}

				return filter;
			}
		}

		bool wasShowAllUsersRequestsAllowedWhenLastFilterCreated;
	}
}
