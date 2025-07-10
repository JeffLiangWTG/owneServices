using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels.Testing
{
	sealed class OverrideLevelSystemDepartmentTest : OverrideLevelTestCase<IOverrideLevel>
	{
		IDepartment department;

		protected override IOverrideLevel GetNewLevel()
		{
			return new DepartmentOverrideLevel(new SystemOverrideLevel(), department, Guid.Empty, Guid.Empty, RegistryStorageFlags.SystemDepartment);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var department = (IGlbDepartment)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());
			department.GE_Desc = "Description";

			this.department = (IDepartment)department;
		}

		protected override void SetAtValue(IRegistryItem item, object value)
		{
			item.SetValue(Guid.Empty, Guid.Empty, department.PK, value);
		}

		protected override IEnumerable<IOverrideLevel> ExpectedChildren(IOverrideLevel level)
		{
			return Enumerable.Empty<IOverrideLevel>();
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatApplyAtThisLevel
		{
			get { return new[] { RegistryItemWithStorage(RegistryStorageFlags.SystemDepartment), RegistryItemWithStorage(RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company) }; }
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatDoNotAtThisLevel
		{
			get
			{
				return AllStorageFlags
					.Where(storage => !storage.HasFlag(RegistryStorageFlags.SystemDepartment))
					.Select(RegistryItemWithStorage);
			}
		}
	}
}
