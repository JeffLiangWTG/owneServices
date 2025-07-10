using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class KafkaSecurityRegistryItem : StronglyTypedRegistryItem<KafkaSecurity>
	{
		public KafkaSecurityRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new KafkaSecurityRegistryDataType(), storage, RegistryOptions.Default)) { }

		public KafkaSecurityRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, KafkaSecurity defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new KafkaSecurityRegistryDataType(), storage, defaultValue)) { }

		public KafkaSecurityRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, KafkaSecurity defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new KafkaSecurityRegistryDataType(), storage, options, defaultValue)) { }
	}
}
