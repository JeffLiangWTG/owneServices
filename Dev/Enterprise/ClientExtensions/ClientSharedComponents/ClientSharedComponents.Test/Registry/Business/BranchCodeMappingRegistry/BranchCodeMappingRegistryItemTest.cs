using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchCodeMappingRegistryItem))]
	class BranchCodeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<BranchCodeMappingRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<BranchCodeMappingRegistryBusinessObjectCollection,
			BranchCodeMappingRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new BranchCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
