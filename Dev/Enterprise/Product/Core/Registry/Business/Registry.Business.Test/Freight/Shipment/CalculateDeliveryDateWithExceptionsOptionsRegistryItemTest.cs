using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CalculateDeliveryDateWithExceptionsOptionsRegistryItem))]
	sealed class CalculateDeliveryDateWithExceptionsOptionsRegistryItemTest : StronglyTypedRegistryItemTestCase<CalculateDeliveryDateWithExceptionsOptions>
	{
		protected override StronglyTypedRegistryItem<CalculateDeliveryDateWithExceptionsOptions, CalculateDeliveryDateWithExceptionsOptions> GetNewRegistryItem()
		{
			return new CalculateDeliveryDateWithExceptionsOptionsRegistryItem(
					"CalculateDeliveryDateWithExceptions",
					FreightDataRegistry.Categories.Freight_Shipment_DeliveryDueDate,
					(NoResString)"Calculate Delivery Due Date with Exceptions",
					(NoResString)"Override this registry to specify the maximum combined delay duration per calendar day.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 24 });
		}
	}
}
