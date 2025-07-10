using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public sealed class DeleteEmptyCusEntryNumbers : DataTransformation
	{
		public override string UserDescription => "Delete empty customs entry number records in CusEntryNum.";

		const string LastProcessedChunkPKName = "DeleteEmptyCusEntryNumbers.LastProcessedChunkPK";
		const int BatchSize = 10000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);

			var chunkingOperation = new GuidChunkingOperation(manager, BatchSize, Math.Max(GetNumberOfRowsFast(), 1), ProcessChunk, LastProcessedChunkPKName);
			chunkingOperation.DoChunking();
		}

		void ProcessChunk(Guid fromPK, Guid toPK)
		{
			using (var cmd = Db.Connection.Command(DeleteEmptyCusEntryNumberChunkSQL))
			{
				cmd.AddParameter("@fromShipmentPK", SqlDbType.UniqueIdentifier, fromPK);
				cmd.AddParameter("@toShipmentPK", SqlDbType.UniqueIdentifier, toPK);
				cmd.ExecuteNonQuery();
			}
		}

		long GetNumberOfRowsFast()
		{
			return DataUtils.GetApproximateRowCountForTable(Db.Connection, CusEntryNumSchema.Constants.TableName);
		}

		const string DeleteEmptyCusEntryNumberChunkSQL = @"
-- Create temporary table for entries we might delete because CE_EntryNum is empty
SELECT CEN.CE_PK, CEN.CE_ParentID, CEN.CE_EntryType,
       ROW_NUMBER() OVER (PARTITION BY CEN.CE_ParentID, CEN.CE_EntryType ORDER BY CEN.CE_PK) AS RowNum,
       CASE WHEN CEN2.CE_PK IS NOT NULL THEN 1 ELSE 0 END AS HasNonEmptyEntryNum
INTO #FilteredEntries
FROM dbo.CusEntryNum CEN
LEFT JOIN dbo.CusEntryNum CEN2 
    ON CEN2.CE_ParentID = CEN.CE_ParentID
    AND CEN2.CE_EntryType = CEN.CE_EntryType
    AND CEN2.CE_EntryNum <> ''
WHERE
    CEN.CE_ParentID BETWEEN @fromShipmentPK AND @toShipmentPK
    AND CEN.CE_ParentTable = 'JobShipment'
    AND CEN.CE_Category = 'CUS'
    AND CEN.CE_EntryNum = ''
OPTION (MAXDOP 1);

-- Delete all entries with empty CE_EntryNum that has another non-empty CE_EntryNum with the same CE_EntryType
-- ... and delete all but one duplicate entry with blank CE_EntryNum if there are no non-empty CE_EntryNum entries
DELETE FROM dbo.CusEntryNum
WHERE CE_PK IN (
    SELECT CE_PK
    FROM #FilteredEntries
    WHERE HasNonEmptyEntryNum = 1 OR RowNum > 1
)
OPTION (MAXDOP 1);

DROP TABLE #FilteredEntries;
";
	}
}
