using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eHub
{
	public class ScavengingSettingCollectionRegistryItem : StronglyTypedRegistryItem<ScavengingSettingCollection>
	{
		public ScavengingSettingCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new ScavengingSettingRegistryDataType(), storage, options))
		{
		}

		[RegistryEditor("Enterprise.Registry.GUI.eHub.ScavengingSettingsListControlItemEditor, Enterprise.Registry.GUI")]
#if DEBUG
		public
#else
		internal
#endif
		class ScavengingSettingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ScavengingSettingCollection>
		{
		}
	}
}
