using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion
{
	public class CargoImpVersionRegistryItem : StronglyTypedRegistryItem<CargoImpVersionConfiguration>
	{
		public CargoImpVersionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new CargoImpVersionRegistryDataType(), storage, options, new CargoImpVersionConfiguration()))
		{
		}
	}
}
