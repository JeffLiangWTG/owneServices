using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DirectorySearchRegistryItem : StronglyTypedRegistryItem<IDirectorySearch, DirectorySearch>
	{
		public DirectorySearchRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DirectorySearchRegistryDataType(), storage))
		{
		}

		public DirectorySearchRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new DirectorySearchRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.DirectorySearchRegistryItemEditor, Enterprise.Registry.GUI")]
	class DirectorySearchRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DirectorySearch>
	{
		public DirectorySearchRegistryDataType()
		{
		}
	}
}
