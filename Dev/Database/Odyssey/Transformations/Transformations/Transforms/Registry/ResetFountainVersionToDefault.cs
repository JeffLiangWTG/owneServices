using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	class ResetFountainVersionToDefault : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "NumberFountainVersion" };
		}
	}
}
