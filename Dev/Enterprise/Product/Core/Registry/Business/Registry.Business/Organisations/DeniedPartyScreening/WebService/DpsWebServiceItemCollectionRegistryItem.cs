using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DpsWebServiceItemCollectionRegistryItem : StronglyTypedRegistryItem<DpsWebServiceItemCollection>
	{
		public DpsWebServiceItemCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DpsWebServiceItemCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DpsWebServiceItemRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
