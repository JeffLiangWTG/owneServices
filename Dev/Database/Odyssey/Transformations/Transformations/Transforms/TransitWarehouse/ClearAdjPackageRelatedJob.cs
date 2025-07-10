using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class ClearAdjPackageRelatedJob : DataTransformation
	{
		public override string UserDescription => "Clear adjust package's dispatch consignment and load list and transfer through cycle count.";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
DECLARE @AdjPackage TABLE(
    AdjPackageStatePK uniqueidentifier,
    TransferLinePK uniqueidentifier,
    TransferHeaderPK uniqueidentifier
);

INSERT INTO @AdjPackage (AdjPackageStatePK, TransferLinePK, TransferHeaderPK)
SELECT
    WPS_PK,
    WTF_PK,
    WTF_WTH_TransitTransferHeader
FROM dbo.WhsItemPackageState
LEFT JOIN WhsItemTransferLine ON WTF_WPS_PackageState = WPS_PK AND WTF_PutTime IS NULL
WHERE WPS_Status = 'ADJ' AND WPS_AdjustedOut = 'LCC';

UPDATE
    WhsItemTransferLine
SET 
    WTF_WL_To = WTF_WL_From,
    WTF_PutTime = GETDATE(),
    WTF_GS_NKPutUser = '~BP',
    WTF_SystemLastEditTimeUtc = GETDATE(),
    WTF_SystemLastEditUser = '~BP'
WHERE
    EXISTS (
        SELECT NULL
        FROM @AdjPackage
        WHERE WhsItemTransferLine.WTF_PK = TransferLinePK
    ) AND WTF_PickTime IS NOT NULL AND WTF_PutTime IS NULL AND WTF_WL_To != WTF_WL_From;

DELETE FROM
    WhsItemTransferLine
WHERE
    EXISTS (
        SELECT NULL
        FROM @AdjPackage
        WHERE WhsItemTransferLine.WTF_PK = TransferLinePK
    ) AND WTF_PickTime IS NULL;

UPDATE
	WhsItemTransferHeader
SET
	WTH_IsFinalised = 1,
	WTH_SystemLastEditTimeUtc = GETDATE(),
	WTH_SystemLastEditUser = '~BP'
FROM @AdjPackage
WHERE
	WTH_PK = TransferHeaderPK AND NOT EXISTS (SELECT NULL FROM WhsItemTransferLine WHERE WTF_PutTime IS NULL AND WTF_WTH_TransitTransferHeader = WTH_PK);

UPDATE
    dbo.WhsItemPackageState
SET
    WPS_WDC_TransitDispatchConsignment = NULL,
    WPS_WDL_LoadList = NULL,
    WPS_SystemLastEditTimeUtc = GETDATE(),
    WPS_SystemLastEditUser = '~BP'
WHERE
    EXISTS (
        SELECT NULL
        FROM @AdjPackage
        WHERE dbo.WhsItemPackageState.WPS_PK = AdjPackageStatePK
    ) AND (WPS_WDC_TransitDispatchConsignment IS NOT NULL OR WPS_WDL_LoadList IS NOT NULL);

";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
