using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class NewsSectionRegistryItem : StronglyTypedRegistryItem<NewsSectionCollection>
	{
		public NewsSectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new NewsSectionRegistryDataType(), storage, RegistryOptions.NotCached))
		{
		}

		public NewsSectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, NewsSectionCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new NewsSectionRegistryDataType(), storage, RegistryOptions.NotCached, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.NewsSectionRegistryItemEditor, Enterprise.Registry.GUI")]
	public class NewsSectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<NewsSectionCollection>
	{
	}
}
