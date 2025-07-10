using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class BranchCodeMappingRegistryItem : StronglyTypedRegistryItem<BranchCodeMappingRegistryBusinessObjectCollection>
	{
		public BranchCodeMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new BranchCodeMappingRegistryDataType(), storage, RegistryOptions.NotCached))
		{
		}
	}

	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.BranchCodeMappingRegistryItemEditor,  Enterprise.ClientSharedComponents.GUI")]
	public class BranchCodeMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BranchCodeMappingRegistryBusinessObjectCollection>
	{
		public BranchCodeMappingRegistryDataType()
		{
		}
	}
}
