using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(RemoveShowUNDGVariantInDangerousGoodsDescriptionRegistryItem))]
	class RemoveShowUNDGVariantInDangerousGoodsDescriptionRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "ShowUNDGVariantInDangerousGoodsDescription" };
		}
	}
}
