using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels.Testing
{
	sealed class BranchOverrideLevelTest : OverrideLevelTestCase<BranchOverrideLevel>
	{
		ICompany company;
		IBranch branch;

		protected override void SetUp()
		{
			base.SetUp();

			company = CreateCompanyWithBranches("Test Company", "Test Branch 1");
			branch = company.Branches.Single();
		}

		protected override BranchOverrideLevel GetNewLevel()
		{
			return new BranchOverrideLevel(branch);
		}

		protected override void SetAtValue(IRegistryItem item, object value)
		{
			item.SetValue(Guid.Empty, branch.PK, Guid.Empty, value);
		}

		protected override IEnumerable<IOverrideLevel> ExpectedChildren(BranchOverrideLevel level)
		{
			return Enumerable.Empty<IOverrideLevel>();
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatApplyAtThisLevel
		{
			get { return AllStorageFlags.Select(flag => flag | RegistryStorageFlags.Branch).Select(RegistryItemWithStorage); }
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatDoNotAtThisLevel
		{
			get { return AllStorageFlags.Where(flag => !flag.HasFlag(RegistryStorageFlags.Branch)).Select(RegistryItemWithStorage); }
		}
	}
}
