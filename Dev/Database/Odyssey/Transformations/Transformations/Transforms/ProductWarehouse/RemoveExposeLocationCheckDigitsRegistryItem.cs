using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	class RemoveExposeLocationCheckDigitsRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "ExposeLocationCheckDigits" };
	}
}
