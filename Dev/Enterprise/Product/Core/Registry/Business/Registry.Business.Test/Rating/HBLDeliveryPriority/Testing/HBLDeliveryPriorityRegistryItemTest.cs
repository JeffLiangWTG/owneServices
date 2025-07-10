using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryPriorityRegistryItem))]
	sealed class HBLDeliveryPriorityRegistryItemTest : StronglyTypedRegistryItemTestCase<HBLDeliveryPriorityConfigCollection>
	{
		protected override StronglyTypedRegistryItem<HBLDeliveryPriorityConfigCollection, HBLDeliveryPriorityConfigCollection> GetNewRegistryItem()
			=> new HBLDeliveryPriorityRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
	}
}
