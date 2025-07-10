using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class BranchGroupSettingsValidation
	{
		public BranchGroupSettingsValidation(BranchGroupSettings parent)
		{
			Parent = parent;
		}

		readonly BranchGroupSettings Parent;

		public void ValidateBranchPK()
		{
			Parent.BranchPKInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(Parent.BranchPKInfo, (IMultilingualString)ResString.GetMultilingualString("2a91e6d0-c1cf-46d3-b241-782fd3322db8", "Branch"));
			if (Parent.ParentCollection != null)
			{
				var duplicateBranches = Parent.ParentCollection.Cast<BranchGroupSettings>().GroupBy(x => x.BranchPK).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
				if (duplicateBranches.Any() && duplicateBranches.Contains(Parent.BranchPK))
				{
					Parent.BranchPKInfo.AddError(Res.GetString("941db06e-874f-4f9e-852d-7539d6124193", "Same Branch not allowed more than once. Please select another Branch."));
				}
			}
		}

		public void ValidateGroupNumber()
		{
			Parent.GroupNumberInfo.ClearAllNotifications();

			if (Parent.GroupNumber < 0 || Parent.GroupNumber > 9)
			{
				Parent.GroupNumberInfo.AddError(Res.GetString("6c5e4b5d-7d10-422a-96d1-bfd65165e3a7", "Please enter a Posting Group number between 0 to 9."));
			}
		}

		public void ValidateIsParentBranch()
		{
			Parent.IsParentBranchInfo.ClearAllNotifications();

			if (Parent.ParentCollection != null)
			{
				var groups = Parent.ParentCollection.Cast<BranchGroupSettings>()
					.GroupBy(g => new { g.GroupNumber, g.IsParentBranch })
					.Select(y => new { GroupNumber = y.Key.GroupNumber, IsParent = y.Key.IsParentBranch, Groups = y })
					.Where(z => z.GroupNumber == Parent.GroupNumber && z.IsParent);

				if (!groups.Any())
				{
					Parent.IsParentBranchInfo.AddError(Res.GetString("fe45516e-db0b-4e3c-bb9d-9035cb486b51", "A single branch in each Posting Group must be set as the Branch to fallback for Transaction Header"));
				}

				if (Parent.IsParentBranch && groups.Any(x => x.Groups.Count() > 1))
				{
					Parent.IsParentBranchInfo.AddError(Res.GetString("fa2a25fe-d0df-4e28-816f-d7c602ca068b", "Only one branch in a Posting Group can be set as the Branch to fallback for Transaction Header"));
				}
			}
		}
	}
}
