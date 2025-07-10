using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow
{
	sealed class RemoveEnforceContentSecurityPolicy : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { GlowEnforceContentSecurityPolicy };
		}

		public const string GlowEnforceContentSecurityPolicy = "GlowEnforceContentSecurityPolicy";
	}
}
