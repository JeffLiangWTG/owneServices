using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	class UpdateFRTAndACTMetricCount : DataTransformation
	{
		public override string UserDescription => "Update First Response Time And Awaiting Client Time MetricCount";

		protected override void OfflinePostUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentMetrics"))
			{
				return;
			}

			var sql = @"
UPDATE dbo.IncidentMetrics
SET IME_MetricCount = 1, IME_SystemLastEditTimeUtc = GETUTCDATE(), IME_SystemLastEditUser = '~BP'
WHERE IME_MetricCode IN ('FRT', 'ACT') AND IME_CalculatedMetric != 0 AND IME_MetricCount = 0;
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
