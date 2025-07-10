using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(DepartmentCodeMappingRegistryItem))]
	class DepartmentCodeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<DepartmentCodeMappingRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<DepartmentCodeMappingRegistryBusinessObjectCollection,
			DepartmentCodeMappingRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new DepartmentCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
