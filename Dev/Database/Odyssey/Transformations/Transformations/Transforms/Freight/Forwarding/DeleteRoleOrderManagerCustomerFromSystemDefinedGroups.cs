using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class DeleteRoleOrderManagerCustomerFromSystemDefinedGroups : DataTransformation
	{
		public const string OrderManagerCustomerRoleName = "OrderManagerCustomer";

		public override string UserDescription => $"Delete the Role {OrderManagerCustomerRoleName} from all system defined groups";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.GlbGroupRole WHERE GGR_GG_Group IN (SELECT GG_PK FROM dbo.GlbGroup WHERE GG_IsSystemDefined = 1) AND GGR_RoleName = '{OrderManagerCustomerRoleName}'");
		}
	}
}
