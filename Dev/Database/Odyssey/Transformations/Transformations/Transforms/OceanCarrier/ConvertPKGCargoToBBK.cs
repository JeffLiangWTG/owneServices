using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier
{
	public class ConvertPKGCargoToBBK : DataTransformation
	{
		public override string UserDescription =>
			"Converting PKG (Package) type cargo to BBK (BreakBulk)";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"UPDATE dbo.CarrierShipmentCargo
SET	CSC_CargoType = 'BBK',
	CSC_F3_NKPackType = 'PKG',
	CSC_SystemLastEditTimeUtc = GetUtcDate(),
	CSC_SystemLastEditUser = '~BP'
WHERE CSC_CargoType = 'PKG'";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
