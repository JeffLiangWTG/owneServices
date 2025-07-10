using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Customs.Shared
{
	class RemoveDuplicateCusClassPivotsV2 : DataTransformation
	{
		public override string UserDescription => "Remove duplicates from CusClassPartPivot Version 2.";

		const string LastProcessedPKExtendedPropertyString = "RemoveDuplicateCusClassPivotsV2.LastProcessedOrgSupplierPartPK";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var sql = @$"
WITH PivotsPotentiallyShouldBeRemoved AS
(
    SELECT
        CI_PK,
        CI_OP,
        CI_OH,
        CI_CC,
        CI_ChildType,
        CI_TariffNum,
        CI_SystemCreateTimeUtc,
        CI_RN_NKCountry
    FROM 
        dbo.CusClassPartPivot
     WHERE
         CI_ChildType IN ('IMP', 'EXP', 'HTI', 'HTE', 'SHB') AND CI_OH IS NULL AND CI_PK NOT IN (SELECT DISTINCT BG_CI FROM dbo.CusAttributeFilter) AND CI_OP IN (SELECT CI_OP FROM dbo.CusClassPartPivot WHERE (CI_OP BETWEEN @StartGuid AND @EndGuid) AND CI_ChildType IN ('IMP', 'EXP', 'HTI', 'HTE', 'SHB') AND CI_OH IS NULL AND CI_SystemCreateTimeUtc >= '2024-02-28')
),
Ranks AS 
(
    SELECT 
    ROW_NUMBER() OVER(PARTITION BY CI_OP, CI_RN_NKCountry, CI_ChildType, CI_CC, CI_TariffNum ORDER BY CI_SystemCreateTimeUtc) AS RowNumber,
    CI_PK,
    CI_SystemCreateTimeUtc
    FROM PivotsPotentiallyShouldBeRemoved
)
DELETE FROM CusClassPartPivot WHERE CI_PK IN (SELECT CI_PK FROM Ranks WHERE RowNumber > 1 AND CI_SystemCreateTimeUtc >= '2024-02-28')";

			new GuidChunkingOperation(manager,
				chunkSize: 10000,
				tableRowCount: DataUtils.GetApproximateRowCountForTable(Db.Connection, OrgSupplierPartSchema.Constants.TableName),
				processChunk: (startGuid, endGuid) =>
				{
					using (var cmd = Db.Connection.Command(sql))
					{
						cmd.AddParameter("@StartGuid", SqlDbType.UniqueIdentifier, startGuid);
						cmd.AddParameter("@EndGuid", SqlDbType.UniqueIdentifier, endGuid);
						cmd.ExecuteNonQuery();
					}
				},
				lastProcessedPkPropertyName: LastProcessedPKExtendedPropertyString,
				token
				).DoChunking();
		}
	}
}
