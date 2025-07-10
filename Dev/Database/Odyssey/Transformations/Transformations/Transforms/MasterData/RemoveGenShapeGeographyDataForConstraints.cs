using System.Threading;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.MasterData
{
	public class RemoveGenShapeGeographyDataForConstraints : DataTransformation
	{
		public override string UserDescription => "Cleanup data for new constraint on column ParentTableCode of table GenShapeGeography'";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var sql = @$"
DELETE FROM dbo.[GenShapeGeography]
WHERE SHG_ParentTableCode NOT IN ('RN', 'R9', 'FZ', '');
";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
