using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class IndexDurationRegistryItem : StronglyTypedRegistryItem<IndexDurationList>
	{
		public IndexDurationRegistryItem(string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new IndexDurationListRegistryDataType(), storage, options, new IndexDurationList()))
		{
		}

		public IndexDurationRegistryItem(string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options,
			IndexDurationList list)
			: base(new RegistryItemImpl(name, category, caption, hint, new IndexDurationListRegistryDataType(), storage, options, list))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.IndexDurationRegistryItemEditor, Enterprise.Registry.GUI")]
	public class IndexDurationListRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IndexDurationList>
	{
	}
}
