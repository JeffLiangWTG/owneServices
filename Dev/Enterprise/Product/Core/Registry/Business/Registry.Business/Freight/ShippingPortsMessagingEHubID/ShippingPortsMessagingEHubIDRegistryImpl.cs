using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	class ShippingPortsMessagingEHubIDRegistryImpl : RegistryItemImpl
	{
		public ShippingPortsMessagingEHubIDRegistryImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
			: base(name, category, caption, hint, dataType, storage)
		{
		}

		public ShippingPortsMessagingEHubIDRegistryImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, dataType, storage, options)
		{
		}
	}
}
