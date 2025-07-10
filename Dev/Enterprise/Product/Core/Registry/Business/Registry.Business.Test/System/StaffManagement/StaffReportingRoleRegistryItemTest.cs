using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StaffReportingRoleRegistryItem))]
	sealed class StaffReportingRoleRegistryItemTest : StronglyTypedRegistryItemTestCase<StaffReportingRoleCollection>
	{
		protected override StronglyTypedRegistryItem<StaffReportingRoleCollection, StaffReportingRoleCollection> GetNewRegistryItem()
		{
			var defaultRoles = new StaffReportingRoleCollection();
			defaultRoles.Add("AAA", (NoResString)"AAA Description", true);
			defaultRoles.Add("ZZZ", (NoResString)"ZZZ Description", false);
			return new StaffReportingRoleRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, defaultRoles);
		}
	}
}
