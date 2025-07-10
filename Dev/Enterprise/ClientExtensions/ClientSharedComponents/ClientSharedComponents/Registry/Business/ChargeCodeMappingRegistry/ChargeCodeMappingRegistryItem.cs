using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class ChargeCodeMappingRegistryItem : StronglyTypedRegistryItem<ChargeCodeMappingRegistryBusinessObjectCollection>
	{
		public ChargeCodeMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ChargeCodeMappingRegistryDataType(), storage, RegistryOptions.NotCached))
		{
		}
	}

	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.ChargeCodeMappingRegistryItemEditor,  Enterprise.ClientSharedComponents.GUI")]
	public class ChargeCodeMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ChargeCodeMappingRegistryBusinessObjectCollection>
	{
		public ChargeCodeMappingRegistryDataType()
		{
		}
	}
}
