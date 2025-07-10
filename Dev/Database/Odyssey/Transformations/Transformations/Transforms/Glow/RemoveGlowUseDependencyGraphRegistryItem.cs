using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.PostUpgrade.Public.Glow
{
	class RemoveGlowUseDependencyGraphRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "GlowUseDependencyGraph" };
		}
	}
}
