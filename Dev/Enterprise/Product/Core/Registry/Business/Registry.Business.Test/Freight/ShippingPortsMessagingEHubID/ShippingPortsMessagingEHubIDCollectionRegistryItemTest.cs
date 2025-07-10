using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShippingPortsMessagingEHubIDCollectionRegistryItem))]
	public class ShippingPortsMessagingEHubIDCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<ShippingPortsMessagingEHubIDCollection>
	{
		protected override StronglyTypedRegistryItem<ShippingPortsMessagingEHubIDCollection, ShippingPortsMessagingEHubIDCollection> GetNewRegistryItem()
		{
			return new ShippingPortsMessagingEHubIDCollectionRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}
}
