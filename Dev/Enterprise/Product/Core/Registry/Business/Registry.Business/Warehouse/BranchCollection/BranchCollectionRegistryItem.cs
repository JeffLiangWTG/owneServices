using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class BranchCollectionRegistryItem : StronglyTypedRegistryItem<BranchProxyMaster>
	{
		public BranchCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new BranchCollectionRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.BranchControlRegistryItemEditor, Enterprise.Registry.GUI")]
	class BranchCollectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BranchProxyMaster>
	{
	}
}
