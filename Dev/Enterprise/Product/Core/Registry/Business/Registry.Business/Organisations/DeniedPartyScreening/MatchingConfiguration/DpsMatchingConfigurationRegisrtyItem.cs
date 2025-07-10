using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DpsMatchingConfigurationRegisrtyItem : StronglyTypedRegistryItem<DpsMatchingConfigurationBusinessObject>
	{
		public DpsMatchingConfigurationRegisrtyItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DpsMatchingConfigurationBusinessObject defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DpsMatchingConfigurationRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
