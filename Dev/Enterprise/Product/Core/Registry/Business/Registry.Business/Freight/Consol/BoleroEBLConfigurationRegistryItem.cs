using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class BoleroEBLConfigurationRegistryItem : StronglyTypedRegistryItem<BoleroEBLConfiguration>
	{
		public BoleroEBLConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new BoleroEBLConfigurationRegistryDataType(), storage))
		{
		}

		public BoleroEBLConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, BoleroEBLConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new BoleroEBLConfigurationRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
