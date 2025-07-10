
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class RatesPrioritiesRegistryItem : StronglyTypedRegistryItem<RatesPrioritiesCollection>
	{
		public RatesPrioritiesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, RatesPrioritiesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RatesPrioritiesRegistryDataType(), storage, options, defaultValue))
		{
		}

		public RatesPrioritiesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RatesPrioritiesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RatesPrioritiesRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.RatePrioritiesRegistryItemEditor, Enterprise.Registry.GUI")]
	public class RatesPrioritiesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<RatesPrioritiesCollection>
	{
	}
}
