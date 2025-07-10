using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise
{
	sealed class UpdateEnableCompliancePotentialRiskMessageBannerRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Update EnableCompliancePotentialRiskMessageBanner registry item name.";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName(PreviousRegistryItemName, NewRegistryItemName);
		}

		public const string PreviousRegistryItemName = "EnableCompliancePotentialRiskMessageBanner";
		public const string NewRegistryItemName = "EnableComplianceWarningMessage";
	}
}
