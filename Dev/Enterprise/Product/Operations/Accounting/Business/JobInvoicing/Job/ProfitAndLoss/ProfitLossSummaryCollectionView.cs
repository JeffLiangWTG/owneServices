using System.Linq;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ProfitLossSummaryCollectionView : ProfitLossSummaryCollectionBaseView
	{
		public ProfitLossSummaryCollectionView(ProfitLossSummaryCollectionBase profitLossSummaryCollectionToFilter, IJobCostingPlugIn plugin)
			: base(profitLossSummaryCollectionToFilter, plugin)
		{
		}

		public override void FilterProfitLossSummaryCollection()
		{
			RemoveAndDeleteAll();
			AddRange(ProfitLossSummaryCollectionToFilter.Where(x => IsAllowedToViewJobFromCurrentBranch(x.Job as Job, x.Branch, x.Department)).Select(e => new ProfitLossSummaryDetailView(e)));
		}

		#region Security Check

		bool IsAllowedToViewJobFromCurrentBranch(Job job, GlbBranch branch, GlbDepartment department)
		{
			var result = true;
			var hasChargeViewingRestriction = true;
			if (Plugin != null && (Plugin as ForwardingConsol) != null)
			{
				hasChargeViewingRestriction = job != null && !job.IsAllowedToViewConsolCostsFromOtherBranchesOrDepartments;
			}
			else
			{
				hasChargeViewingRestriction = job != null && !job.IsAllowedToViewChargesFromOtherBranchesOrDepartments;
			}
			if (hasChargeViewingRestriction && branch != null && department != null)
			{
				if (branch != GlbBranch.CurrentBranch || department != GlbDepartment.CurrentDepartment)
				{
					result = AllowedToLogin(branch, department);
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Combining Cache key, not related to GUI")]
		bool AllowedToLogin(GlbBranch branch, GlbDepartment department)
		{
			return Factory.GetCachedValue("Login BRN:" + branch.GB_Code + " DEP:" + department.GE_Code, delegate
			{
				var security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
				return security.Login.IsAllowed;
			});
		}

		#endregion
	}
}
