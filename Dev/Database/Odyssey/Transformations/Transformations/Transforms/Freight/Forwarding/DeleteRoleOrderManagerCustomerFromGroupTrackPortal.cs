using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class DeleteRoleOrderManagerCustomerFromGroupTrackPortal : DataTransformation
	{
		public override string UserDescription => "Delete the Role OrderManagerCustomer from the Group TRACKPORTAL";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			Db.Connection.ExecuteNonQuery("DELETE dbo.GlbGroupRole WHERE GGR_GG_Group IN (SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = 'TRACKPORTAL') AND GGR_RoleName = 'OrderManagerCustomer'");
		}
	}
}
