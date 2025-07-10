using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Asycuda
{
	public sealed class UpdateAsycudaBillPersonTypes_SystemDefinedValues : DataTransformation
	{
		public override string UserDescription => "Migrate AsycudaBill ShipperPersonType,ConsigneePersonType,NotifyPartyPersonType from User-Defined to System-Defined values storage";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
BEGIN TRY

DECLARE @UserDefinedAblPersonTypeValue TABLE(
	XV_PK uniqueidentifier PRIMARY KEY,
	XV_ParentTableCode varchar(3),
	XV_ParentID uniqueidentifier,
	XV_Name varchar(60),
	XV_Type varchar(3),
	XV_Data nvarchar(100),
	XV_AutoVersion smallint,
	XV_SystemCreateTimeUtc smalldatetime,
	XV_SystemCreateUser varchar(3),
	XV_SystemLastEditTimeUtc smalldatetime,
	XV_SystemLastEditUser varchar(3))

INSERT INTO @UserDefinedAblPersonTypeValue(
	XV_PK,
	XV_ParentTableCode,
	XV_ParentID,
	XV_Name,
	XV_Type,
	XV_Data,
	XV_AutoVersion,
	XV_SystemCreateTimeUtc,
	XV_SystemCreateUser,
	XV_SystemLastEditTimeUtc,
	XV_SystemLastEditUser)
	SELECT
		XV_PK,
		XV_ParentTableCode,
		XV_ParentID,
		XV_Name,
		XV_Type,
		XV_Data,
		XV_AutoVersion,
		XV_SystemCreateTimeUtc,
		XV_SystemCreateUser,
		XV_SystemLastEditTimeUtc,
		XV_SystemLastEditUser
	FROM dbo.GenCustomAddOnValue c WITH (INDEX(NR_RX__XV_Name_XV_Data))
	WHERE
		XV_ParentTableCode = 'ABL'
		AND XV_Name IN ('ShipperPersonType', 'ConsigneePersonType', 'NotifyPartyPersonType')
		AND NOT EXISTS (SELECT 1 FROM dbo.GenAddOnColumn s WHERE s.XA_ParentTableCode = 'ABL' AND s.XA_ParentID = c.XV_ParentID AND s.XA_Name = c.XV_Name)

INSERT INTO dbo.GenAddOnColumn(
	XA_PK,
	XA_ParentTableCode,
	XA_ParentID,
	XA_Name,
	XA_Type,
	XA_Data,
	XA_AutoVersion,
	XA_SystemCreateTimeUtc,
	XA_SystemCreateUser,
	XA_SystemLastEditTimeUtc,
	XA_SystemLastEditUser)
	SELECT
		NEWID(),
		XV_ParentTableCode,
		XV_ParentID,
		XV_Name,
		XV_Type,
		XV_Data,
		XV_AutoVersion,
		XV_SystemCreateTimeUtc,
		XV_SystemCreateUser,
		XV_SystemLastEditTimeUtc,
		XV_SystemLastEditUser
	FROM @UserDefinedAblPersonTypeValue

DELETE FROM dbo.GenCustomAddOnValue
WHERE XV_PK IN (SELECT t.XV_PK FROM @UserDefinedAblPersonTypeValue t)

END TRY
BEGIN CATCH
    THROW;
END CATCH
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
