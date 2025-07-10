using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	class UpdateARTMetricCount : DataTransformation
	{
		public override string UserDescription => "Update Additional Response Time MetricCount";

		protected override void OfflinePostUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentMetrics"))
			{
				return;
			}

			var sql = @"
DELETE FROM dbo.IncidentMetrics
WHERE IME_MetricCode = 'ART' AND IME_EndTimeUtc is null AND IME_CalculatedMetric = 0 AND IME_MetricCount = 0;

UPDATE dbo.IncidentMetrics
SET IME_MetricCount = 1, IME_SystemLastEditTimeUtc = GETUTCDATE(), IME_SystemLastEditUser = '~BP'
WHERE IME_MetricCode = 'ART' AND IME_MetricCount = 0;
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
