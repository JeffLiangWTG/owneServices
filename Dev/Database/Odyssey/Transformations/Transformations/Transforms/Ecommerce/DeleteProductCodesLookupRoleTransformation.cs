using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce
{
	public class DeleteProductCodesLookupRoleTransformation : DataTransformation
	{
		public override string UserDescription => "Remove ProductCodesLookup role from database";

		protected override void OfflinePostUpgradeTransform()
			=> Db.Connection.ExecuteNonQuery(@"
			DELETE dbo.GlbGroupRole Where GGR_RoleName = 'ProductCodesLookup';");
	}
}
