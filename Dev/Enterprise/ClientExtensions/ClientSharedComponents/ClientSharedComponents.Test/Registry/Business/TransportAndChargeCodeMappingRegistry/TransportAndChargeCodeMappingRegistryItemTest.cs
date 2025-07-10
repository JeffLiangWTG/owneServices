using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(TransportAndChargeCodeMappingRegistryItem))]
	class TransportAndChargeCodeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<TransportAndChargeCodeMappingRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<TransportAndChargeCodeMappingRegistryBusinessObjectCollection,
			TransportAndChargeCodeMappingRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new TransportAndChargeCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
