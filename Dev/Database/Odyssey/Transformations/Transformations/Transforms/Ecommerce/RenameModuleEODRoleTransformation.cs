using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce
{
	public class RenameModuleEODRoleTransformation : DataTransformation
	{
		public override string UserDescription => "Rename module-EOD role to eCommerceOriginDepotViewer.";

		protected override void OfflinePostUpgradeTransform()
			=> Db.Connection.ExecuteNonQuery(@"
			UPDATE G1
			SET G1.GGR_RoleName = 'eCommerceOriginDepotViewer',
				G1.GGR_SystemLastEditTimeUtc = CURRENT_TIMESTAMP,
				G1.GGR_SystemLastEditUser = 'E'
			FROM dbo.GlbGroupRole AS G1
			WHERE G1.GGR_RoleName = 'module-EOD' AND NOT EXISTS (
				SELECT 1
				FROM dbo.GlbGroupRole AS G2
				WHERE G2.GGR_RoleName = 'eCommerceOriginDepotViewer' AND G1.GGR_GG_Group = G2.GGR_GG_Group
			);
			");
	}
}
