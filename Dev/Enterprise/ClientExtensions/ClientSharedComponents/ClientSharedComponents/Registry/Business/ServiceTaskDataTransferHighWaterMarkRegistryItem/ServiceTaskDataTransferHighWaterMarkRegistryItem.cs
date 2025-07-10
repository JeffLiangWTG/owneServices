using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class ServiceTaskDataTransferHighWaterMarkRegistryItem : AutomaticProcessRegistryItemBase<DataTransferRegistryBusinessObject>
	{
		public ServiceTaskDataTransferHighWaterMarkRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(name, category, caption, hint, new ServiceTaskDataTransferHighWaterMarkRegistryDataType(), storage)
		{
		}

		public ServiceTaskDataTransferHighWaterMarkRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new ServiceTaskDataTransferHighWaterMarkRegistryDataType(), storage, options)
		{
		}
	}

	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.ServiceTaskDataTransferHighWaterMarkRegistryItemEditor, Enterprise.ClientSharedComponents.GUI")]
	class ServiceTaskDataTransferHighWaterMarkRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DataTransferRegistryBusinessObject>
	{
	}
}
