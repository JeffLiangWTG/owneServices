using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class DepartmentCodeMappingRegistryItem : StronglyTypedRegistryItem<DepartmentCodeMappingRegistryBusinessObjectCollection>
	{
		public DepartmentCodeMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DepartmentCodeMappingRegistryDataType(), storage, RegistryOptions.NotCached))
		{
		}
	}

	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.DepartmentCodeMappingRegistryItemEditor,  Enterprise.ClientSharedComponents.GUI")]
	public class DepartmentCodeMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DepartmentCodeMappingRegistryBusinessObjectCollection>
	{
		public DepartmentCodeMappingRegistryDataType()
		{
		}
	}
}
