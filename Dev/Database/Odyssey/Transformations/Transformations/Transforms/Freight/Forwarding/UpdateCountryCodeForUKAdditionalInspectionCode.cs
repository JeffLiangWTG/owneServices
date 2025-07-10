using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public sealed class UpdateCountryCodeForUKAdditionalInspectionCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update Country Code For UK Additional Inspection Code.";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
DECLARE @Now SMALLDATETIME = GetUtcDate();

WITH HighRiskShipmentsAndPacklines AS (
	SELECT 
		JS_PK,
		JL_PK
	FROM 
		dbo.JobShipment
	LEFT JOIN
		dbo.JobPackLines ON JL_JS = JS_PK
	WHERE
		(JL_IsHighRisk = 1 OR JS_IsHighRisk = 1) AND JS_RL_NKOrigin LIKE 'GB%' 
)

UPDATE
	dbo.CusEntryNum
SET
	CE_RN_NKCountryCode = 'GB',
	CE_SystemLastEditTimeUtc = @Now,
	CE_SystemLastEditUser = 'E'
WHERE
	CE_PK IN (
		SELECT DISTINCT
			CE_PK
		FROM
			dbo.CusEntryNum
		INNER JOIN
			HighRiskShipmentsAndPacklines ON JS_PK = CE_ParentID OR JL_PK = CE_ParentID
		WHERE
			CE_EntryType = 'AIN' AND CE_RN_NKCountryCode = 'EU' AND CE_Category = 'INS' AND (CE_ParentTable = 'JobPackLines' OR CE_ParentTable = 'JobShipment')
	)";

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(JobShipmentSchema.Instance)
					.Key(JobShipmentSchema.Constants.JS_RL_NKOrigin)
					.Include(JobShipmentSchema.Constants.PK, JobShipmentSchema.Constants.JS_IsHighRisk)
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
