using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class LastDbRestoreRegistryItem : StronglyTypedRegistryItem<LastDbRestoreInfo, LastDbRestoreInfo>
	{
		public LastDbRestoreRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, LastDbRestoreInfo defaultValue)
			: base(new LastDbRestoreRegistryItemImpl(name, category, caption, hint, storage, options, defaultValue)) { }

		class LastDbRestoreRegistryItemImpl : RegistryItemImpl
		{
			public LastDbRestoreRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, LastDbRestoreInfo defaultValue)
				: base(name, category, caption, hint, new LastDbRestoreInfoRegistryDataType(), storage, options, defaultValue) { }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.LastDbRestoreRegistryItemEditor, Enterprise.Registry.GUI")]
	public class LastDbRestoreInfoRegistryDataType : NonPersistentBusinessObjectRegistryDataType<LastDbRestoreInfo> { }
}
