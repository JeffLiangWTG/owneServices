using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Glow;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.Glow
{
	[TestedType(typeof(RemoveEnforceContentSecurityPolicy))]
	class RemoveEnforceContentSecurityPolicyTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { RemoveEnforceContentSecurityPolicy.GlowEnforceContentSecurityPolicy };
		}
	}
}
