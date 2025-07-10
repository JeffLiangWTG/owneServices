using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class EBookingAirCarrierConfigurationRegistryItem : StronglyTypedRegistryItem<EBookingCarrierConfiguration>
	{
		public EBookingAirCarrierConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new EBookingAirCarrierConfigurationRegistryDataType(), storage))
		{
		}

		public EBookingAirCarrierConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, EBookingCarrierConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new EBookingAirCarrierConfigurationRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
