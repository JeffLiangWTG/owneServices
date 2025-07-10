using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class ServiceLevelRegistryItem : StronglyTypedRegistryItem<ServiceLevelRegistryBusinessObjectCollection>
	{
		public ServiceLevelRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ServiceLevelRegistryDataType(), storage))
		{
		}
	}
}
