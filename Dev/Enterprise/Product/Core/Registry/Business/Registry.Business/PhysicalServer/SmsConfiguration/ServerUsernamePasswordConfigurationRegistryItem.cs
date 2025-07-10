using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ServerUsernamePasswordConfigurationRegistryItem : StronglyTypedRegistryItem<ServerUsernamePasswordConfiguration>
	{
		public ServerUsernamePasswordConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ServerUsernamePasswordConfigurationRegistryDataType(), storage))
		{
		}

		public ServerUsernamePasswordConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ServerUsernamePasswordConfiguration defaultConfiguration)
			: base(new RegistryItemImpl(name, category, caption, hint, new ServerUsernamePasswordConfigurationRegistryDataType(), storage, defaultConfiguration))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ServerUsernamePasswordConfigurationRegistryItemEditor, Enterprise.Registry.GUI")]
	class ServerUsernamePasswordConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ServerUsernamePasswordConfiguration>
	{
		public ServerUsernamePasswordConfigurationRegistryDataType()
		{
		}
	}
}
