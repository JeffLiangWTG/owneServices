using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class BranchDepartmentCodeMappingRegistryItem : StronglyTypedRegistryItem<BranchDepartmentCodeMappingRegistryBusinessObjectCollection>
	{
		public BranchDepartmentCodeMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new BranchDepartmentCodeMappingRegistryDataType(), storage, RegistryOptions.NotCached))
		{
		}
	}

	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.BranchDepartmentCodeMappingRegistryItemEditor,  Enterprise.ClientSharedComponents.GUI")]
	public class BranchDepartmentCodeMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BranchDepartmentCodeMappingRegistryBusinessObjectCollection>
	{
		public BranchDepartmentCodeMappingRegistryDataType()
		{
		}
	}
}
