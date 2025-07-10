using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShipmentInspectionTypeRegistryItem))]
	sealed class ShipmentInspectionTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<ShipmentInspectionTypes>
	{
		protected override StronglyTypedRegistryItem<ShipmentInspectionTypes, ShipmentInspectionTypes> GetNewRegistryItem()
		{
			return new ShipmentInspectionTypeRegistryItem("Hello", (NoResString)"Hello", (NoResString)"Hello", (NoResString)"hello", RegistryStorageFlags.Company, new ShipmentInspectionTypes());
		}
	}
}
