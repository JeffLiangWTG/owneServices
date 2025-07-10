using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Warehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(RemoveEnablePackingConsolidationFunctionalityRegistryItem))]
	class RemoveEnablePackingConsolidationFunctionalityRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => new[] { "EnablePackingConsolidationFunctionality" };
	}
}
