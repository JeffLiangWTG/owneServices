using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class ServiceTaskDataTransferRegistryItem : AutomaticProcessRegistryItemBase<DataTransferRegistryBusinessObject>
	{
		public ServiceTaskDataTransferRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(name, category, caption, hint, new ServiceTaskDataTransferRegistryDataType(), storage)
		{
		}

		public ServiceTaskDataTransferRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new ServiceTaskDataTransferRegistryDataType(), storage, options)
		{
		}

		public ServiceTaskDataTransferRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string minimumIntervalType, int minimumInterval)
			: base(name, category, caption, hint, new ServiceTaskDataTransferRegistryDataType(minimumIntervalType, minimumInterval), storage, options)
		{
		}
	}

	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.ServiceTaskDataTransferRegistryItemEditor, Enterprise.ClientSharedComponents.GUI")]
	class ServiceTaskDataTransferRegistryDataType : AutomaticProcessRegistryDataType<DataTransferRegistryBusinessObject>
	{
		public ServiceTaskDataTransferRegistryDataType() : base()
		{
		}

		public ServiceTaskDataTransferRegistryDataType(bool shouldValidate) : base(shouldValidate)
		{
		}

		public ServiceTaskDataTransferRegistryDataType(string minimumIntervalType, int minimumInterval) : base(minimumIntervalType, minimumInterval)
		{
		}
	}
}
