using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CalculateDeliveryDueDateOptionsRegistryItem))]
	sealed class CalculateDeliveryDueDateOptionsRegistryItemTest : StronglyTypedRegistryItemTestCase<CalculateDeliveryDueDateOptions>
	{
		protected override StronglyTypedRegistryItem<CalculateDeliveryDueDateOptions, CalculateDeliveryDueDateOptions> GetNewRegistryItem()
		{
			var defaultList = new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = false };
			return new CalculateDeliveryDueDateOptionsRegistryItem(
						"CalculateDeliveryDueDateByTransportMode",
						FreightDataRegistry.Categories.Freight_Shipment_DeliveryDueDate,
						(NoResString)"Calculate Delivery Due Date",
						(NoResString)"Enable Delivery Due Date calculation on Bookings and Shipments.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList);
		}
	}
}
