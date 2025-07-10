using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Warehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(RemoveEnableOrderLinePalletIDEntryRegistryItem))]
	class RemoveEnableOrderLinePalletIDEntryRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => new[] { "EnableOrderLinePalletIDEntry" };
	}
}
