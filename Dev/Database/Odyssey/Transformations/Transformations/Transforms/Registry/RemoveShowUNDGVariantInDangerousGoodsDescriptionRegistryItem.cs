using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	class RemoveShowUNDGVariantInDangerousGoodsDescriptionRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "ShowUNDGVariantInDangerousGoodsDescription" };
		}
	}
}
