using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;

public class InsertStmALogFromDTUReturnHistory : DataTransformation
{
	public override string UserDescription => "Insert DTU Return history into StmALog From WhsItemDTUReturnDetail";

	protected override void OfflinePreUpgradeTransform()
	{
		var sql = @$"
INSERT INTO dbo.StmALog
(
	SL_PK,
	SL_Table,
	SL_Parent,
	SL_IsEstimate,
	SL_IsCancelled,
	SL_Reference,
	SL_PostedTimeUtc,
	SL_EventTime,
	SL_GS_NKUser,
	SL_SE_NKEvent,
	SL_GB_NKBranch,
	SL_GE_NKDepartment,
	SL_EventTimeUtc)
SELECT 
	SL_PK = NEWID(),
	SL_Table = 'WhsItemDispatchTransportationUnit',
	SL_Parent = WDH_PK,
	SL_IsEstimate = 'N',
	SL_IsCancelled = 'N',
	SL_Reference = '|FAC=TW|EQN=' + WDH_VehicleReference + '|RES=' + WDR_Reason + '|FRN=' + WDH_ReferenceNumber + '|',
	SL_PostedTimeUtc = CAST(SWITCHOFFSET(WDR_ReturnTime, '+00:00') AS datetime),
	SL_EventTime = CAST(WDR_ReturnTime AS datetime),
	SL_GS_NKUser = WDR_SystemCreateUser,
	SL_SE_NKEvent = 'RET',
	SL_GB_NKBranch = GB_Code,
	SL_GE_NKDepartment = ISNULL(GE_Code, ''),
	SL_EventTimeUtc = CAST(SWITCHOFFSET(WDR_ReturnTime, '+00:00') AS datetime)
FROM
	dbo.WhsItemDTUReturnDetail
INNER JOIN
	dbo.WhsItemDispatchTransportationUnit ON WDR_WDH_TransitDispatchTransportationUnit = WDH_PK
INNER JOIN
	dbo.WhsWarehouse ON WDH_WW_Warehouse = WW_PK
INNER JOIN
	dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
LEFT JOIN
	dbo.GlbStaff ON WDR_SystemCreateUser = GS_Code
LEFT JOIN
	dbo.GlbDepartment ON GS_GE_HomeDepartment = GE_PK
WHERE NOT EXISTS(SELECT 1 FROM dbo.StmALog WHERE SL_Parent = WDH_PK AND SL_SE_NKEvent = 'RET')";
		Db.Connection.ExecuteNonQuery(sql);
	}
}
