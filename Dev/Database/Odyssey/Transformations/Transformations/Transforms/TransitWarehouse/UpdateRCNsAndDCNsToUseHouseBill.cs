using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class UpdateRCNsAndDCNsToUseHouseBill : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Add CusEntryNum for relating HSB rows To House Bill Number Column in RCN And DCN";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_WhsItemReceiveConsignment_UpdateAutoVersion')
BEGIN
    DISABLE TRIGGER TG_WhsItemReceiveConsignment_UpdateAutoVersion ON dbo.WhsItemReceiveConsignment
END;

IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_WhsItemDispatchConsignment_UpdateAutoVersion')
BEGIN
    DISABLE TRIGGER TG_WhsItemDispatchConsignment_UpdateAutoVersion ON dbo.WhsItemDispatchConsignment
END;

;WITH CusEntryNumView AS (
    SELECT CE_EntryNum, CE_ParentID, ROW_NUMBER() OVER (PARTITION BY CE_ParentID ORDER BY CE_SystemLastEditTimeUtc DESC, CE_SystemCreateTimeUtc DESC, CE_EntryNum DESC) AS RowNumber
    FROM dbo.CusEntryNum
    WHERE CE_EntryType = 'HSB' AND CE_ParentTable = 'WhsItemReceiveConsignment' AND CE_IsValid = 0
)
UPDATE dbo.WhsItemReceiveConsignment 
SET
    WRC_HouseBillNumber = CE_EntryNum,
    WRC_SystemLastEditTimeUtc = GetUtcDate(),
    WRC_SystemLastEditUser = '~BP',
    WRC_AutoVersion = WRC_AutoVersion + 1 % 32768
FROM CusEntryNumView
WHERE WRC_HouseBillNumber = '' AND WRC_PK = CE_ParentID AND RowNumber = 1

;WITH CusEntryNumView AS (
    SELECT CE_EntryNum, CE_ParentID, ROW_NUMBER() OVER (PARTITION BY CE_ParentID ORDER BY CE_SystemLastEditTimeUtc DESC, CE_SystemCreateTimeUtc DESC, CE_EntryNum DESC) AS RowNumber
    FROM dbo.CusEntryNum
    WHERE CE_EntryType = 'HSB' AND CE_ParentTable = 'WhsItemDispatchConsignment' AND CE_IsValid = 0
)
UPDATE dbo.WhsItemDispatchConsignment 
SET
    WDC_HouseBillNumber = CE_EntryNum,
    WDC_SystemLastEditTimeUtc = GetUtcDate(),
    WDC_SystemLastEditUser = '~BP',
    WDC_AutoVersion = WDC_AutoVersion + 1 % 32768
FROM CusEntryNumView
WHERE WDC_HouseBillNumber = '' AND WDC_PK = CE_ParentID AND RowNumber = 1;

IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_WhsItemReceiveConsignment_UpdateAutoVersion')
BEGIN
    ENABLE TRIGGER TG_WhsItemReceiveConsignment_UpdateAutoVersion ON dbo.WhsItemReceiveConsignment
END;

IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_WhsItemDispatchConsignment_UpdateAutoVersion')
BEGIN
    ENABLE TRIGGER TG_WhsItemDispatchConsignment_UpdateAutoVersion ON dbo.WhsItemDispatchConsignment
END;
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusEntryNumSchema.Instance)
					.Key(CusEntryNumSchema.Constants.CE_ParentTable)
					.Key(CusEntryNumSchema.Constants.CE_EntryType)
					.Key(CusEntryNumSchema.Constants.CE_IsValid)
					.Include(CusEntryNumSchema.Constants.CE_EntryNum)
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
