using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	class UpdateNRAMetrics : DataTransformation
	{
		public override string UserDescription => "Update Net Resolution Age Metrics";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentMain")
				&& DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentMetrics"))
			{
				var sql = @"UPDATE IME
SET IME_CalculatedMetric = TRA.IME_CalculatedMetric - COALESCE(ACT.ACT_Total, 0),
	IME_SystemLastEditTimeUtc = GETUTCDATE(),
	IME_SystemLastEditUser = '~BP'
FROM IncidentMetrics IME
INNER JOIN IncidentMain IM
    ON IM.IM_IncidentNumber = IME.IME_IncidentNumber
INNER JOIN IncidentMetrics TRA
    ON IM.IM_IncidentNumber = TRA.IME_IncidentNumber
LEFT JOIN (
    SELECT 
        IME_IncidentNumber,
        SUM(IME_CalculatedMetric) AS ACT_Total
    FROM IncidentMetrics
    WHERE IME_MetricCode = 'ACT'
    GROUP BY IME_IncidentNumber
) ACT
    ON IM.IM_IncidentNumber = ACT.IME_IncidentNumber
WHERE 
    IM.IM_ResolutionCode = 'CLS'
    AND IME.IME_MetricCode = 'NRA'
    AND IME.IME_CalculatedMetric <= 0
    AND TRA.IME_MetricCode = 'TRA';
";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
