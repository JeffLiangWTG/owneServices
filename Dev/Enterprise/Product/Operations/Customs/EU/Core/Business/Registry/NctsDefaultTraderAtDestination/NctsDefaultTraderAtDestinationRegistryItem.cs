using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Registry
{
	public class NctsDefaultTraderAtDestinationRegistryItem : StronglyTypedRegistryItem<NctsDefaultTraderAtDestination>
	{
		public NctsDefaultTraderAtDestinationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, NctsDefaultTraderAtDestination defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new NctsDefaultTraderAtDestinationRegistryDataType(), storage, defaultValue))
		{
		}
	}
}
