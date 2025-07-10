using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceTaskDataTransferRegistryItem))]
	public class ServiceTaskDataTransferRegistryItemTest : DataTransferRegistryItemTest
	{
		protected override StronglyTypedRegistryItem<DataTransferRegistryBusinessObject, DataTransferRegistryBusinessObject> GetNewRegistryItem()
		{
			return new ServiceTaskDataTransferRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
