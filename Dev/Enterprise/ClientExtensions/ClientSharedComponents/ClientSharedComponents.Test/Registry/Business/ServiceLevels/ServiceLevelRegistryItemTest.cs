using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceLevelRegistryItem))]
	public class ServiceLevelRegistryItemTest : StronglyTypedRegistryItemTestCase<ServiceLevelRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<ServiceLevelRegistryBusinessObjectCollection, ServiceLevelRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new ServiceLevelRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
