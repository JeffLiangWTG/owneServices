using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class InterfaceConnectorTemporarilyEnabledUntilRegistryItem : StronglyTypedRegistryItem<InterfaceConnectorTemporarilyEnabledUntil>
	{
		public InterfaceConnectorTemporarilyEnabledUntilRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(new RegistryItemImpl(name, category, caption, hint, new InterfaceConnectorTemporarilyEnabledUntilRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.InterfaceConnectorTemporarilyEnabledUntilRegistryItemEditor, Enterprise.Registry.GUI")]
	class InterfaceConnectorTemporarilyEnabledUntilRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InterfaceConnectorTemporarilyEnabledUntil>
	{
		public InterfaceConnectorTemporarilyEnabledUntilRegistryDataType()
		{
		}
	}
}
