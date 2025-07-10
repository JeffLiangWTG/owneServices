using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class StatisticsFoldupInfoRegistryItem : StronglyTypedRegistryItem<StatisticsFoldupInfoCollection>
	{
		// check for examples of other classes that implement this strongly typed registry thing. maybe there's an extremely common pattern (I don't doubt it) and you can get a great refactor happening
		public StatisticsFoldupInfoRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StatisticsFoldupInfoCollection defaultValue)
			: this(name, category, caption, hint, RegistryStorageFlags.Company, RegistryOptions.Default, defaultValue) { }

		public StatisticsFoldupInfoRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOptions, StatisticsFoldupInfoCollection defaultValue)
			: base(new StatisticsFoldupInfoRegistryItemImpl(name, category, caption, hint, storage, registryOptions, defaultValue)) { }

		class StatisticsFoldupInfoRegistryItemImpl : RegistryItemImpl
		{
			public StatisticsFoldupInfoRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOptions, StatisticsFoldupInfoCollection defaultValue)
				: base(name, category, caption, hint, new StatisticsFoldupInfoRegistryDataType(), storage, registryOptions, defaultValue) { }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.StatisticsFoldupInfoRegistryItemEditor, Enterprise.Registry.GUI")]
	public class StatisticsFoldupInfoRegistryDataType : NonPersistentBusinessObjectRegistryDataType<StatisticsFoldupInfoCollection> { }
}
