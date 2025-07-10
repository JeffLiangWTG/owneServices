using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels
{
	public class CompanyOverrideLevel : OverrideLevelWithDepartments
	{
		readonly ICompany company;

		public CompanyOverrideLevel(ICompany company, bool hideInactiveChildren = false)
			: base(
				description: company.HumanReadableNameForRegistry,
				pathRelativeToParent: Res.GetString("8F43CD2D-4D78-471A-84A8-8BDF159B5FE1", "Companies"),
				companyPK: company.PK,
				branchPK: Guid.Empty,
				departmentStorageFlag: RegistryStorageFlags.CompanyDepartment,
				hideInactiveChildren: hideInactiveChildren)
		{
			this.company = company;
		}

		public override bool CanSetValueOf(IRegistryItem item) => item.Storage.HasFlag(RegistryStorageFlags.Company);
		public override IOverrideLevel Parent => new SystemOverrideLevel();
		protected override IEnumerable<IOverrideLevel> OtherChildren
		{
			get
			{
				var branches = hideInactiveChildren ? company.ActiveBranches : company.Branches;
				return EnvProxy.Instance.Registry.ShowCodeAtCompanyAndBranchName
					? branches?.OrderBy(b => b.Code).Select(b => new BranchOverrideLevel(b, hideInactiveChildren))
					: branches?.OrderBy(b => b.Name).Select(b => new BranchOverrideLevel(b, hideInactiveChildren));
			}
		}
	}
}
