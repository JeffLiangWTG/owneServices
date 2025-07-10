using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.PostUpgrade.Public.Glow;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.Glow
{
	[TestedType(typeof(RemoveGlowUseDependencyGraphRegistryItem))]
	class RemoveGlowUseDependencyGraphRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "GlowUseDependencyGraph" };
		}
	}
}
