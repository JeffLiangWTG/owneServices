using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ChargeCodeMappingRegistryItem))]
	class ChargeCodeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<ChargeCodeMappingRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<ChargeCodeMappingRegistryBusinessObjectCollection,
			ChargeCodeMappingRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new ChargeCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
