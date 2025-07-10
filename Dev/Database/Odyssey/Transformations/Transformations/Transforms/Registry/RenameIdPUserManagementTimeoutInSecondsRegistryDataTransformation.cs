using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	public class RenameIdPUserManagementTimeoutInSecondsRegistryDataTransformation : RegistryDataTransformation
	{
		public override string UserDescription => "Rename registry from IdPUserManagementTimeoutInSeconds to IdentityProviderTimeoutInSeconds";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName("IdPUserManagementTimeoutInSeconds", "IdentityProviderTimeoutInSeconds");
		}
	}
}
