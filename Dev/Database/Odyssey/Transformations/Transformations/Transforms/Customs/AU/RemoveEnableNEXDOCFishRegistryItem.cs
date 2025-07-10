using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.AU
{
	sealed class RemoveEnableNEXDOCFishRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "EnableNEXDOCFish", "EnableNEXDOCStaging" };
		}
	}
}
