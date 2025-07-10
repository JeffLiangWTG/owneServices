using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels.Testing
{
	sealed class OverrideLevelBranchDepartmentTest : OverrideLevelTestCase<IOverrideLevel>
	{
		ICompany company;
		IBranch branch;
		IDepartment department;

		protected override IOverrideLevel GetNewLevel()
		{
			return new DepartmentOverrideLevel(new BranchOverrideLevel(branch), department, branch.CompanyPK, branch.PK, RegistryStorageFlags.BranchDepartment);
		}

		protected override void SetUp()
		{
			base.SetUp();

			company = CreateCompanyWithBranches("Test Company", "Test Branch 1");
			branch = company.Branches.Single();

			var department = (IGlbDepartment)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());
			department.GE_Desc = "Description";

			this.department = (IDepartment)department;
		}

		protected override void SetAtValue(IRegistryItem item, object value)
		{
			item.SetValue(Guid.Empty, branch.PK, department.PK, value);
		}

		protected override IEnumerable<IOverrideLevel> ExpectedChildren(IOverrideLevel level)
		{
			return Enumerable.Empty<IOverrideLevel>();
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatApplyAtThisLevel
		{
			get { return new[] { RegistryItemWithStorage(RegistryStorageFlags.BranchDepartment), RegistryItemWithStorage(RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Company) }; }
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatDoNotAtThisLevel
		{
			get
			{
				return AllStorageFlags
					.Where(storage => !storage.HasFlag(RegistryStorageFlags.BranchDepartment))
					.Select(RegistryItemWithStorage);
			}
		}
	}
}
