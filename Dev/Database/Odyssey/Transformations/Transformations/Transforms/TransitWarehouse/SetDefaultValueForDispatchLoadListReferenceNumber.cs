
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class SetDefaultValueForDispatchLoadListReferenceNumber : DataTransformation
	{
		public override string UserDescription => "Set Default Value For Dispatch Load List Reference Number";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
IF EXISTS(SELECT NULL FROM sys.objects where name = 'TG_WhsItemDispatchLoadList_UpdateAutoVersion' AND type = 'TR')
BEGIN
	DISABLE TRIGGER TG_WhsItemDispatchLoadList_UpdateAutoVersion ON dbo.WhsItemDispatchLoadList
END;

UPDATE
	dbo.WhsItemDispatchLoadList 
SET
	WDL_ReferenceNumber = WDL_JobID,
	WDL_SystemLastEditTimeUtc = GETDATE(),
	WDL_SystemLastEditUser = '~BP',
	WDL_AutoVersion = (WDL_AutoVersion + 1) % 32768

IF EXISTS(SELECT NULL FROM sys.objects where name = 'TG_WhsItemDispatchLoadList_UpdateAutoVersion' AND type = 'TR')
BEGIN
	ENABLE TRIGGER TG_WhsItemDispatchLoadList_UpdateAutoVersion ON dbo.WhsItemDispatchLoadList
END;";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
