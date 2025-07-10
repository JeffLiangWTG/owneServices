using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE
{
	public class UPEBranchIDsRegistryItem : StronglyTypedRegistryItem<UPEBranchIDsRegistryObjectCollection>
	{
		public UPEBranchIDsRegistryItem(string name, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, UPEBranchIDsRegistryObjectCollection defaultValue, MultilingualString[] categories)
			: base(new RegistryItemImpl(name, caption, hint, new UPEBranchIDsRegistryDataType(), null, storage, RegistryOptions.Default, defaultValue, false, categories))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.UPE.UPEBranchIDsRegistryItemEditor, ZClientUPE")]
	class UPEBranchIDsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<UPEBranchIDsRegistryObjectCollection>
	{
		public UPEBranchIDsRegistryDataType()
		{
		}
	}
}
