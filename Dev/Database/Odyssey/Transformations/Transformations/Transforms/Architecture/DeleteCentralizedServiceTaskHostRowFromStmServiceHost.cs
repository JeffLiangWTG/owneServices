using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture
{
	public class DeleteCentralizedServiceTaskHostRowFromStmServiceHost : DataTransformation
	{
		public override string UserDescription => "Delete outdated CentralizedServiceTaskHost";

		protected override void OfflinePostUpgradeTransform()
		{
			var sqlText = @"
DELETE FROM dbo.StmServiceHost
WHERE SH_HostName = 'CentralizedServiceTaskHost'
";
			Db.Connection.ExecuteNonQuery(sqlText);
		}
	}
}
