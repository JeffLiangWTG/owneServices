using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	class RemoveDGThresholdFeatureFlagRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "EnableUNDGThresholdFunctionality" };
	}
}
