using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchDepartmentCodeMappingRegistryItem))]
	class BranchDepartmentCodeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<BranchDepartmentCodeMappingRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<BranchDepartmentCodeMappingRegistryBusinessObjectCollection,
			BranchDepartmentCodeMappingRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new BranchDepartmentCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
