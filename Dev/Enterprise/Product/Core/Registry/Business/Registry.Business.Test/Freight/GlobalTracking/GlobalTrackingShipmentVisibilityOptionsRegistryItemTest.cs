using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlobalTrackingShipmentVisibilityOptionsRegistryItem))]
	sealed class GlobalTrackingShipmentVisibilityOptionsRegistryItemTest : StronglyTypedRegistryItemTestCase<GlobalTrackingShipmentVisibilityOptions>
	{
		protected override StronglyTypedRegistryItem<GlobalTrackingShipmentVisibilityOptions, GlobalTrackingShipmentVisibilityOptions> GetNewRegistryItem()
		{
			return new GlobalTrackingShipmentVisibilityOptionsRegistryItem(
					"GlobalTrackingShipmentVisibility",
					FreightDataRegistry.Categories.Freight_GlobalTracking,
					(NoResString)"Shipment Visibility",
					(NoResString)"Enable Shipment Visibility?",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					new GlobalTrackingShipmentVisibilityOptions { IsActive = false, IsDefaultContainerAutomation = false });
		}
	}
}
