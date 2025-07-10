using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business;

public class NctsFallbackConfigurationRegistryItem : StronglyTypedRegistryItem<NctsFallbackConfiguration>
{
	public NctsFallbackConfigurationRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage)
		: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new NctsFallbackConfigurationRegistryDataType(), storage))
	{
	}

	public NctsFallbackConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOptions, NctsFallbackConfiguration defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new NctsFallbackConfigurationRegistryDataType(), storage, registryOptions, defaultValue))
	{
	}
}
