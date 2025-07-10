using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	class RemoveEnableRfServicesRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "EnableRFServices" };
	}
}
