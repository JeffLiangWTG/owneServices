using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.Client.EDI.DbUpgrader
{
	class EdiDeleteAzureManagementCertificateRegistryTransform : DataTransformation
	{
		public override string UserDescription => "Clean Registry Data for Azure Management Certificate.";

		protected override void OfflinePostUpgradeTransform()
		{
			RegistryDataTransformation.DeleteRegistryItemRows("AzureApplicationManagementCertificate", "AzureApplicationManagementCertificatePassword");
		}
	}
}
