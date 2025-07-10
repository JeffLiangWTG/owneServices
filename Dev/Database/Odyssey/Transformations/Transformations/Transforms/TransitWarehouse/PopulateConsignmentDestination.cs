using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.TransitWarehouse
{
	public class PopulateConsignmentDestination : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Populate RCN and DCN Destination from AdditionalReference and Delete used AdditionalReference.";

		protected override void OfflinePostUpgradeTransform() {
			var sql = @"
BEGIN TRY

IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_WhsItemDispatchConsignment_UpdateAutoVersion' AND type = 'TR')
BEGIN
	DISABLE TRIGGER TG_WhsItemDispatchConsignment_UpdateAutoVersion ON dbo.WhsItemDispatchConsignment
END;

IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_WhsItemReceiveConsignment_UpdateAutoVersion' AND type = 'TR')
BEGIN
	DISABLE TRIGGER TG_WhsItemReceiveConsignment_UpdateAutoVersion ON dbo.WhsItemReceiveConsignment
END;

UPDATE
	dbo.WhsItemDispatchConsignment
SET
	WDC_RL_NKDestination = CE_EntryNum,
	WDC_SystemLastEditTimeUtc = GETUTCDATE(),
	WDC_SystemLastEditUser = '~BP',
	WDC_AutoVersion = (WDC_AutoVersion + 1) % 32768
FROM
	dbo.WhsItemDispatchConsignment JOIN 
	(
		SELECT 
			CE_ParentID,
			CE_EntryNum,
			ROW_NUMBER() OVER (PARTITION BY CE_ParentID ORDER BY CE_SystemCreateTimeUtc DESC) AS TimeOrder
		FROM
			dbo.CusEntryNum
		WHERE CE_ParentTable = 'WhsItemDispatchConsignment' AND CE_EntryType = 'DPT' AND CE_Category = 'OTH' AND LEN(CE_EntryNum) = 5
	) InnerQuery
ON WDC_PK = CE_ParentID AND TimeOrder = 1

UPDATE
	dbo.WhsItemReceiveConsignment
SET
	WRC_RL_NKDestination = CE_EntryNum,
	WRC_SystemLastEditTimeUtc = GETUTCDATE(),
	WRC_SystemLastEditUser = '~BP',
	WRC_AutoVersion = (WRC_AutoVersion + 1) % 32768
FROM
	dbo.WhsItemReceiveConsignment JOIN 
	(
		SELECT 
			CE_ParentID,
			CE_EntryNum,
			ROW_NUMBER() OVER (PARTITION BY CE_ParentID ORDER BY CE_SystemCreateTimeUtc DESC) AS TimeOrder
		FROM
			dbo.CusEntryNum
		WHERE CE_ParentTable = 'WhsItemReceiveConsignment' AND CE_EntryType = 'DPT' AND CE_Category = 'OTH' AND LEN(CE_EntryNum) = 5
	) InnerQuery
ON WRC_PK = CE_ParentID AND TimeOrder = 1

DELETE FROM
	dbo.CusEntryNum
WHERE
	(CE_ParentTable = 'WhsItemDispatchConsignment' OR CE_ParentTable = 'WhsItemReceiveConsignment') AND CE_EntryType = 'DPT' AND CE_Category = 'OTH'

IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_WhsItemDispatchConsignment_UpdateAutoVersion' AND type = 'TR')
BEGIN
	ENABLE TRIGGER TG_WhsItemDispatchConsignment_UpdateAutoVersion ON dbo.WhsItemDispatchConsignment
END

IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_WhsItemReceiveConsignment_UpdateAutoVersion' AND type = 'TR')
BEGIN
	ENABLE TRIGGER TG_WhsItemReceiveConsignment_UpdateAutoVersion ON dbo.WhsItemReceiveConsignment
END

END TRY
BEGIN CATCH
THROW;
END CATCH
";

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusEntryNumSchema.Instance)
					.Key(CusEntryNumSchema.Constants.CE_ParentTable, CusEntryNumSchema.Constants.CE_EntryType, CusEntryNumSchema.Constants.CE_Category, CusEntryNumSchema.Constants.CE_ParentID).Key(CusEntryNumSchema.Constants.CE_SystemCreateTimeUtc, OrderBys.DESC)
					.Include(CusEntryNumSchema.Constants.CE_EntryNum, CusEntryNumSchema.Constants.CE_EntryStatus)
					.Where($"([{CusEntryNumSchema.Constants.CE_ParentTable}] IN ('{WhsItemReceiveConsignmentSchema.Constants.TableName}', '{WhsItemDispatchConsignmentSchema.Constants.TableName}')) AND [{CusEntryNumSchema.Constants.CE_Category}]='OTH' AND [{CusEntryNumSchema.Constants.CE_EntryType}]='DPT'")
					.GetInfo();

				indexProvider.New(WhsItemReceiveConsignmentSchema.Instance)
					.Key(WhsItemReceiveConsignmentSchema.Constants.PK)
					.Include(WhsItemReceiveConsignmentSchema.Constants.WRC_RL_NKDestination, WhsItemReceiveConsignmentSchema.Constants.WRC_SystemLastEditTimeUtc, WhsItemReceiveConsignmentSchema.Constants.WRC_SystemLastEditUser, "WRC_AutoVersion")
					.GetInfo();

				indexProvider.New(WhsItemDispatchConsignmentSchema.Instance)
					.Key(WhsItemDispatchConsignmentSchema.Constants.PK)
					.Include(WhsItemDispatchConsignmentSchema.Constants.WDC_RL_NKDestination, WhsItemDispatchConsignmentSchema.Constants.WDC_SystemLastEditTimeUtc, WhsItemDispatchConsignmentSchema.Constants.WDC_SystemLastEditUser, "WDC_AutoVersion")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
