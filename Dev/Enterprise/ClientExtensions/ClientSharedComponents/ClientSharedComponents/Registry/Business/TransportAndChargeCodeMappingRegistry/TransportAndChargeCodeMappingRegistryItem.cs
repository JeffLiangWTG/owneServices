using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class TransportAndChargeCodeMappingRegistryItem : StronglyTypedRegistryItem<TransportAndChargeCodeMappingRegistryBusinessObjectCollection>
	{
		public TransportAndChargeCodeMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new TransportAndChargeCodeMappingRegistryDataType(), storage, RegistryOptions.NotCached))
		{
		}
	}

	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.TransportAndChargeCodeMappingRegistryItemEditor,  Enterprise.ClientSharedComponents.GUI")]
	public class TransportAndChargeCodeMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TransportAndChargeCodeMappingRegistryBusinessObjectCollection>
	{
		public TransportAndChargeCodeMappingRegistryDataType()
		{
		}
	}
}
