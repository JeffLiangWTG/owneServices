using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	public class RemoveYardUnitStateIfYUS_YRL_ReceiveLineIsEmpty_And_YUS_YPL_Pickup_Or_YUS_YEL_ReleaseLineAreNotEmpty : DataTransformation
	{
		public override string UserDescription => "Remove invalid yard unit states and movement where YUS_YRL_ReceiveLine is blank and YUS_YEL_ReleaseLine or YUS_YPL_Pickup are set";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
DELETE FROM dbo.CYDMovement
WHERE YML_YUS_YardUnitState IN (SELECT YUS_PK FROM dbo.CYDYardUnitState WHERE (YUS_YEL_ReleaseLine IS NOT NULL OR YUS_YPL_Pickup IS NOT NULL) AND YUS_YRL_ReceiveLine IS NULL);

DELETE FROM dbo.CYDYardUnitState
WHERE (YUS_YEL_ReleaseLine IS NOT NULL OR YUS_YPL_Pickup IS NOT NULL) AND YUS_YRL_ReceiveLine IS NULL";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
