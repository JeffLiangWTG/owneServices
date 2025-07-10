using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	public class UPEGlbGroupsRegistryItem : StronglyTypedRegistryItem<UPEGlbGroupsRegistryObjectCollection>
	{
		public UPEGlbGroupsRegistryItem(string name, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, UPEGlbGroupsRegistryObjectCollection defaultValue, params MultilingualString[] categories)
			: base(new RegistryItemImpl(name, caption, hint, new UPEGlbGroupsRegistryDataType(), null, storage, RegistryOptions.Default, defaultValue, false, categories))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.UPE.Registry.GUI.UPEGlbGroupsRegistryItemEditor, ZClientUPE")]
	class UPEGlbGroupsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<UPEGlbGroupsRegistryObjectCollection>
	{
		public UPEGlbGroupsRegistryDataType() { }
	}
}
