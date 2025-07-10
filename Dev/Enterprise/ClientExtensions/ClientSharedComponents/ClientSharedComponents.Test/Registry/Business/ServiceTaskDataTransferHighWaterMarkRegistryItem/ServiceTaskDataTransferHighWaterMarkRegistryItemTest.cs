using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceTaskDataTransferHighWaterMarkRegistryItem))]
	public class ServiceTaskDataTransferHighWaterMarkRegistryItemTest : DataTransferRegistryItemTest
	{
		protected override StronglyTypedRegistryItem<DataTransferRegistryBusinessObject, DataTransferRegistryBusinessObject> GetNewRegistryItem()
		{
			return new ServiceTaskDataTransferHighWaterMarkRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
