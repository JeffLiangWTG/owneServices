using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Registry
{
	public class TriggerPointsConfigurationRegistryItem : StronglyTypedRegistryItem<TriggerPointsConfiguration>
	{
		public TriggerPointsConfigurationRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new TriggerPointsConfigurationRegistryDataType(), storage))
		{
		}

		public TriggerPointsConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, TriggerPointsConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TriggerPointsConfigurationRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}
}
