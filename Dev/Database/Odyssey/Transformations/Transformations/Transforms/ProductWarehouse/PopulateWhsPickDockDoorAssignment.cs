using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class PopulateWhsPickDockDoorAssignment : DataTransformation
	{
		public override string UserDescription => "Populate the Dock Door Assignment links for Warehouse Picks.";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
BEGIN TRY

	IF OBJECT_ID('TG_WhsPick_DockDoorLocationIsCorrect', 'TR') IS NOT NULL DISABLE TRIGGER TG_WhsPick_DockDoorLocationIsCorrect ON dbo.WhsPick;
	IF OBJECT_ID('TG_WhsPick_UpdateAutoVersion', 'TR') IS NOT NULL DISABLE TRIGGER TG_WhsPick_UpdateAutoVersion ON dbo.WhsPick;

	EXEC dbo.SuspendTrigger 'TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick';
	EXEC dbo.SuspendTrigger 'TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick';
	
	CREATE TABLE #UpdateDDAInformation
	(
		PickPK uniqueidentifier,
		DockDoorPK uniqueidentifier,
		INDEX ix_UpdateDDAInformation CLUSTERED (DockDoorPK)
	)
	
	INSERT INTO #UpdateDDAInformation
	SELECT
			WP_PK AS PickPK,
			WP_WL_DockDoor AS DockDoorPK 
	FROM
		dbo.WhsPick
	WHERE
		EXISTS
		(
			SELECT NULL
			FROM dbo.WhsDocket
			WHERE
				WD_WP = WP_PK AND WD_DocketType = 'ORD'
				AND WD_DocketStatus IN ('ATP', 'PIC')
		)
		AND WP_PickStatus NOT IN ('FIN', 'CAN')
		AND WP_WL_DockDoor IS NOT NULL
	
	;WITH
		DDAValues AS (
			SELECT DISTINCT DockDoorPK FROM #UpdateDDAInformation
	)
	INSERT INTO dbo.WhsDockDoorAssignment (WDA_PK, WDA_WL_AssignedDockDoor, WDA_FirstPutawayToDockDoorUtc, WDA_SystemCreateTimeUtc, WDA_SystemCreateUser, WDA_SystemLastEditTimeUtc, WDA_SystemLastEditUser)
	SELECT
		NEWID(),
		DockDoorPK,
		GETUTCDATE(),
		GETUTCDATE(),
		'~BP',
		GETUTCDATE(),
		'~BP'
	FROM
		DDAValues
	
	;WITH
		PickValues AS (
			SELECT PickPK, DockDoorPK FROM #UpdateDDAInformation
	)
	UPDATE
		dbo.WhsPick
	SET
		WP_WDA_DockDoorAssignment = WDA_PK,
		WP_WL_DockDoor = NULL,
		WP_AutoVersion = (WP_AutoVersion + 1) % 32768,
		WP_SystemLastEditTimeUtc = GETUTCDATE(),
		WP_SystemLastEditUser = '~BP'
	FROM
		PickValues
		JOIN dbo.WhsDockDoorAssignment ON WDA_WL_AssignedDockDoor = DockDoorPK
	WHERE
		WP_PK = PickPK
	
	DECLARE @StoredProcedureReturnValue int;
	DECLARE @DockDoorAssignmentPKsToCheck dbo.TVP_uniqueidentifier;
	
	INSERT INTO @DockDoorAssignmentPKsToCheck
	SELECT WDA_PK FROM dbo.WhsDockDoorAssignment
	
	IF EXISTS(SELECT 1 FROM @DockDoorAssignmentPKsToCheck)
	BEGIN
		EXEC @StoredProcedureReturnValue = dbo.WhsCheckForUnreferencedWhsDockDoorAssignment @DockDoorAssignmentPKsToCheck
	END
	
	DROP TABLE #UpdateDDAInformation;
	
	IF OBJECT_ID('TG_WhsPick_UpdateAutoVersion', 'TR') IS NOT NULL ENABLE TRIGGER TG_WhsPick_UpdateAutoVersion ON dbo.WhsPick;
	IF OBJECT_ID('TG_WhsPick_DockDoorLocationIsCorrect', 'TR') IS NOT NULL ENABLE TRIGGER TG_WhsPick_DockDoorLocationIsCorrect ON dbo.WhsPick; 
	
	EXEC dbo.ResumeTrigger 'TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick';
	EXEC dbo.ResumeTrigger 'TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick';

END TRY
BEGIN CATCH
	THROW
END CATCH
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
