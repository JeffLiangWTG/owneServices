using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransitReferenceMappingRegistryItem))]
	sealed class TransitReferenceMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<TransitReferenceMappingConfiguration>
	{
		protected override StronglyTypedRegistryItem<TransitReferenceMappingConfiguration, TransitReferenceMappingConfiguration> GetNewRegistryItem()
		{
			return new TransitReferenceMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
