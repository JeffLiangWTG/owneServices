using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class UpdateWhsDocketTrimExternalReferenceLeadingWhitespace : DataTransformation
	{
		public override string UserDescription => "Update WD_ExternalReference to not have leading whitespace.";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			const int BatchSize = 1000;

			do
			{
				if (token.IsCancellationRequested)
				{
					token.ThrowIfCancellationRequested();
				}
			} while (UpdateChunk(BatchSize) >= BatchSize);
		}

		int UpdateChunk(int batchSize)
		{
			var sql = $@"
WITH DUPES AS
(
	SELECT
		LTRIM(WD_ExternalReference) Trimmed,
		WD_OH_Client Client,
		WD_DocketType DocketType,
		WD_ExternalReferenceSplit ExternalReferenceSplit
	FROM
		dbo.WhsDocket
	GROUP BY
		LTRIM(WD_ExternalReference),
		WD_OH_Client,
		WD_DocketType,
		WD_ExternalReferenceSplit
	HAVING COUNT(*) > 1
)
SELECT
	TOP {batchSize} WD_PK
INTO
	#ValuesToUpdate
FROM
	dbo.WhsDocket
WHERE
	WD_ExternalReference LIKE ' %'
	AND NOT EXISTS
	(
		SELECT NULL
		FROM 
			DUPES
		WHERE
			LTRIM(WD_ExternalReference) = Trimmed
			AND WD_OH_Client = Client
			AND WD_DocketType = DocketType
			AND WD_ExternalReferenceSplit = ExternalReferenceSplit
	)

UPDATE dbo.WhsDocket
SET
	WD_ExternalReference = LTRIM(WD_ExternalReference),
	WD_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	WD_SystemLastEditUser = '~BP'
WHERE 
	WD_PK IN (SELECT WD_PK FROM #ValuesToUpdate);

DECLARE @NumRowsUpdated int = (SELECT COUNT(*) FROM #ValuesToUpdate)

-- Drop the temp tables
DROP TABLE #ValuesToUpdate

SELECT @NumRowsUpdated
";
			var numProcessed = (int)Db.Connection.ExecuteScalar(sql);
			manager?.ShowInfoMessage($"Trimmed External Reference for {numProcessed} rows.");

			return numProcessed;
		}
	}
}
