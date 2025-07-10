using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class RemoveGenAddonColumnShortfallStatus : DataTransformation
	{
		public override string UserDescription => "Delete GenAddonColumn data for WhsOrder.PrioritizedShortfallStatusCode";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var deletedRowCount = 0;
			do
			{
				var sql = @"
DELETE TOP (1000)
	GenAddOnColumn
WHERE
	XA_Name = 'PrioritizedShortfallStatusCode'
	AND XA_Type = 'STR'
	AND XA_ParentTableCode = 'WD'

SELECT @@ROWCOUNT
";
				deletedRowCount = Db.Connection.ExecuteScalar<int>(sql);
			}
			while (deletedRowCount > 0 && !token.IsCancellationRequested);
		}
	}
}
