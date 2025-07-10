using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels
{
	public class BranchOverrideLevel : OverrideLevelWithDepartments
	{
		readonly IBranch branch;

		public BranchOverrideLevel(IBranch branch, bool hideInactiveChildren = false)
			: base(
				description: branch.HumanReadableNameForRegistry,
				pathRelativeToParent: Res.GetString("427CCF43-060C-45ED-BF6A-2E345A45B148", "Branches"),
				companyPK: branch.CompanyPK,
				branchPK: branch.PK,
				departmentStorageFlag: RegistryStorageFlags.BranchDepartment,
				hideInactiveChildren: hideInactiveChildren)
		{
			this.branch = branch;
		}

		public override bool CanSetValueOf(IRegistryItem item)
		{
			return item.Storage.HasFlag(RegistryStorageFlags.Branch);
		}

		public override IOverrideLevel Parent
		{
			get { return new CompanyOverrideLevel(branch.Company); }
		}

		protected override IEnumerable<IOverrideLevel> OtherChildren { get { return Enumerable.Empty<IOverrideLevel>(); } }
	}
}
