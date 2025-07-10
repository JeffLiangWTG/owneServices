using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow
{
	sealed class UpdateGlowPortalsContentSecurityPolicyModeRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Update GlowPortalsContentSecurityPolicyMode registry item name.";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName(PreviousRegistryItemName, NewRegistryItemName);
		}

		public const string PreviousRegistryItemName = "GlowPortalsContentSecurityPolicyMode";
		public const string NewRegistryItemName = "GlowContentSecurityPolicyMode";
	}
}
