using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels.Testing
{
	sealed class CompanyOverrideLevelTest : OverrideLevelTestCase<CompanyOverrideLevel>
	{
		ICompany company;

		public void TestHideInactive()
		{
			var company = CreateCompanyWithBranches("Company NAme", new[] { "InactiveBranch" }, new[] { "ActiveBranch" });

			var departmentsCount = Factory.GetDatabaseCount(ObjectFactory.GetType<IGlbDepartment>());

			var level = new CompanyOverrideLevel(company, hideInactiveChildren: true);
			AssertEquals(1, level.Children.Count() - departmentsCount);
			AssertEquals("ActiveBranch", level.Children.First().Description);

			//Rule should follow through to child branches
			var departmentToMakeInactive = (IDepartment)Factory.LoadTop1(ObjectFactory.GetType<IGlbDepartment>(), new ZQuery());
			((BusinessObject)departmentToMakeInactive)["GE_IsActive"] = false;
			Factory.Save();

			var subChildren = level.Children.First().Children;
			AssertEquals("Should be one less because we made one inactive", departmentsCount - 1, subChildren.Count());
			Assert("The inactive department should not have made it", subChildren.All(child => child.Description != departmentToMakeInactive.Description));
		}

		protected override void SetUp()
		{
			base.SetUp();

			company = CreateCompanyWithBranches("Test Company", "Test Branch 1");
		}

		protected override CompanyOverrideLevel GetNewLevel()
		{
			return new CompanyOverrideLevel(company);
		}

		protected override void SetAtValue(IRegistryItem item, object value)
		{
			item.SetValue(company.PK, Guid.Empty, Guid.Empty, value);
		}

		protected override IEnumerable<IOverrideLevel> ExpectedChildren(CompanyOverrideLevel level)
		{
			return new[] { new BranchOverrideLevel(company.Branches.First()), };
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatApplyAtThisLevel
		{
			get { return AllStorageFlags.Select(flag => flag | RegistryStorageFlags.Company).Select(RegistryItemWithStorage); }
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatDoNotAtThisLevel
		{
			get { return AllStorageFlags.Where(flag => !flag.HasFlag(RegistryStorageFlags.Company)).Select(RegistryItemWithStorage); }
		}
	}
}
