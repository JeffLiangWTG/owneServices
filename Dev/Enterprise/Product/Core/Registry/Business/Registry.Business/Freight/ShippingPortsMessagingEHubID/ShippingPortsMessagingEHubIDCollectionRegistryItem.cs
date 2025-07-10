using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ShippingPortsMessagingEHubIDCollectionRegistryItem : StronglyTypedRegistryItem<ShippingPortsMessagingEHubIDCollection>
	{
		public ShippingPortsMessagingEHubIDCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ShippingPortsMessagingEHubIDRegistryImpl(name, category, caption, hint, new ShippingPortsMessagingEHubIDCollectionDataType(ShippingPortsMessagingEHubIDCollection.NewWithDefaultValues()), storage, options))
		{
		}
	}
}
