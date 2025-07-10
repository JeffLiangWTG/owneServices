using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	public class RemoveUnitAndMovementIfYUS_UnitIDIsEmpty : DataTransformation
	{
		public override string UserDescription => "Remove the yard unit states and movements if YUS_UnitID is empty";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
DELETE FROM dbo.CYDMovement
WHERE YML_YUS_YardUnitState IN (SELECT YUS_PK FROM dbo.CYDYardUnitState WHERE YUS_UnitID = '');

DELETE FROM dbo.CYDYardUnitState
WHERE YUS_UnitID = ''";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
