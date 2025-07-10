using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Warehouse.Testing
{
	[TestedType(typeof(RemoveDGThresholdFeatureFlagRegistryItem))]
	class RemoveDGThresholdFeatureFlagRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => new[] { "EnableUNDGThresholdFunctionality" };
	}
}
