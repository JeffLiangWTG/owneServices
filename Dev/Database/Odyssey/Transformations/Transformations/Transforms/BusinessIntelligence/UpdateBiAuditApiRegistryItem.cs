using System.Text;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BusinessIntelligence
{
	class UpdateBiAuditApiRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Update BiAuditAPI value to true to enable Replication API by default.";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateDatabaseValue("BiAuditAPI", Encoding.Unicode.GetBytes("True"));
		}
	}
}
