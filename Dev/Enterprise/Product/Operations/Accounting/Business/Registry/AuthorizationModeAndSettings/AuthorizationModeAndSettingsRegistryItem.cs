using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class AuthorizationModeAndSettingsRegistryItem :  StronglyTypedRegistryItem<AuthorizationModeAndSettings>
	{
		public AuthorizationModeAndSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new AuthorizationModeAndSettingsRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.AuthorizationModeAndSettingsEditor, Enterprise.Accounting.GUI")]
	public class AuthorizationModeAndSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AuthorizationModeAndSettings>
	{
	}
}