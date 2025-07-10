using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class DeleteTransitWarehouseCustomerRoleTransformation : DataTransformation
	{
		public override string UserDescription => "Remove TransitWarehouseCustomer role from database";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
			=> Db.Connection.ExecuteNonQuery(@"
			DELETE dbo.GlbGroupRole WHERE GGR_RoleName = 'TransitWarehouseCustomer';");
	}
}
