using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class ServiceTaskDataTransferSwitchRegistryItem : AutomaticProcessRegistryItemBase<DataTransferSwitchRegistryBusinessObject>
	{
		public ServiceTaskDataTransferSwitchRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(name, category, caption, hint, new ServiceTaskDataTransferSwitchRegistryDataType(), storage)
		{
		}

		public ServiceTaskDataTransferSwitchRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new ServiceTaskDataTransferSwitchRegistryDataType(), storage, options)
		{
		}

		public ServiceTaskDataTransferSwitchRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string minimumIntervalType, int minimumInterval)
			: base(name, category, caption, hint, new ServiceTaskDataTransferSwitchRegistryDataType(minimumIntervalType, minimumInterval), storage, options)
		{
		}
	}

	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.ServiceTaskDataTransferSwitchRegistryItemEditor, Enterprise.ClientSharedComponents.GUI")]
	class ServiceTaskDataTransferSwitchRegistryDataType : AutomaticProcessRegistryDataType<DataTransferSwitchRegistryBusinessObject>
	{
		public ServiceTaskDataTransferSwitchRegistryDataType() : base()
		{
		}

		public ServiceTaskDataTransferSwitchRegistryDataType(bool shouldValidate) : base(shouldValidate)
		{
		}

		public ServiceTaskDataTransferSwitchRegistryDataType(string minimumIntervalType, int minimumInterval) : base(minimumIntervalType, minimumInterval)
		{
		}
	}
}
