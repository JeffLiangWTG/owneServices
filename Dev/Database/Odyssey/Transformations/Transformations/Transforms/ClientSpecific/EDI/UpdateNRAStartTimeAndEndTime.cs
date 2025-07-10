using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	class UpdateNRAStartTimeAndEndTime : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update Net Resolution Age StartTime And EndTime";

		protected override void OfflinePostUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentMetrics"))
			{
				return;
			}

			var sql = @"
UPDATE IM
SET
	IM.IME_StartTimeUtc = IM2.IME_StartTimeUtc,
	IM.IME_EndTimeUtc = IM2.IME_EndTimeUtc,
	IME_SystemLastEditTimeUtc = GETUTCDATE(),
	IME_SystemLastEditUser = '~BP'
FROM dbo.IncidentMetrics IM
JOIN dbo.IncidentMetrics IM2 
	ON IM.IME_IncidentNumber = IM2.IME_IncidentNumber
	AND IM2.IME_MetricCode = 'TRA'
WHERE IM.IME_MetricCode = 'NRA'
	AND IM.IME_CalculatedMetric != 0
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New("dbo", "IncidentMetrics")
					.Key("IME_MetricCode")
					.Key("IME_IncidentNumber")
					.Include("IME_CalculatedMetric")
					.Include("IME_StartTimeUtc")
					.Include("IME_EndTimeUtc")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
