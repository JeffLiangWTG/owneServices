using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DeliveryModeRegistryItem))]
	sealed class DeliveryModeRegistryItemTest : StronglyTypedRegistryItemTestCase<DeliveryModeCollection>
	{
		protected override StronglyTypedRegistryItem<DeliveryModeCollection, DeliveryModeCollection> GetNewRegistryItem()
		{
			return new DeliveryModeRegistryItem(
				"ContainerDeliveryModeList",
				RawDataRegistry.Categories.Freight_Container,
				(NoResString)"Container Delivery Mode List",
				(NoResString)"A list of container delivery mode types.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				DeliveryModeCollection.GetDefault());
		}
	}
}
