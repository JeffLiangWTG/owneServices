using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceTaskDataTransferSwitchRegistryItem))]
	public class ServiceTaskDataTransferSwitchRegistryItemTest : DataTransferSwitchRegistryItemTest
	{
		protected override StronglyTypedRegistryItem<DataTransferSwitchRegistryBusinessObject, DataTransferSwitchRegistryBusinessObject> GetNewRegistryItem()
		{
			return new ServiceTaskDataTransferSwitchRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
