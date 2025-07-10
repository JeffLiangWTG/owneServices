using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryModeRegistryItem))]
	sealed class HBLDeliveryModeRegistryItemTest : StronglyTypedRegistryItemTestCase<HBLDeliveryModes>
	{
		protected override StronglyTypedRegistryItem<HBLDeliveryModes, HBLDeliveryModes> GetNewRegistryItem()
		{
			return new HBLDeliveryModeRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.BranchDepartment, RegistryOptions.Default, new HBLDeliveryModes());
		}
	}
}
