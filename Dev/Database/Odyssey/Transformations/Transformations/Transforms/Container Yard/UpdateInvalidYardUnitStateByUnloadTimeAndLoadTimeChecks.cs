using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class UpdateInvalidYardUnitStateByUnloadTimeAndLoadTimeChecks : DataTransformation
	{
		public override string UserDescription => "Update invalid Yard Unit State by Load/Unload Time checks";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
BEGIN TRY
    DECLARE @CurrentUtcDate DATETIME;
    SET @CurrentUtcDate = GETUTCDATE();
 
    -- Delete loaded units missing dispatch information
    DELETE FROM dbo.CYDMovement
    WHERE 
        YML_YTU_ToTransportationUnit IS NOT NULL
        AND YML_YUS_YardUnitState IN (
            SELECT YUS_PK 
            FROM dbo.CYDYardUnitState 
            WHERE 
                YUS_LoadTime IS NOT NULL
                AND (
                    YUS_YPL_Pickup IS NULL
                    OR YUS_YTU_DispatchTransportationUnit IS NULL
                    OR YUS_YEL_ReleaseLine IS NULL
                )
        );
 
    -- Clear load data
    UPDATE dbo.CYDYardUnitState
    SET
        YUS_LoadTime = NULL,
        YUS_GS_NKLoadUser = '',
        YUS_UnloadTime = NULL,
        YUS_GS_NKUnloadUser = '',
        YUS_WL_CurrentYardLocation = NULL,
        YUS_SystemLastEditTimeUtc = @CurrentUtcDate,
        YUS_SystemLastEditUser = '~BP'
    WHERE 
        YUS_LoadTime IS NOT NULL
        AND (
            YUS_YPL_Pickup IS NULL
            OR YUS_YTU_DispatchTransportationUnit IS NULL
            OR YUS_YEL_ReleaseLine IS NULL
        );
 
    -- Delete unloaded units missing receiving information
    DELETE FROM dbo.CYDMovement
    WHERE 
        YML_YTU_FromTransportationUnit IS NOT NULL
        AND YML_YUS_YardUnitState IN (
            SELECT YUS_PK 
            FROM dbo.CYDYardUnitState 
            WHERE 
                YUS_UnloadTime IS NOT NULL
                AND (
                    YUS_YDL_Delivery IS NULL
                    OR YUS_YTU_ReceiveTransportationUnit IS NULL
                    OR YUS_YRL_ReceiveLine IS NULL
                )
        );
 
    -- Clear unload data
    UPDATE dbo.CYDYardUnitState
    SET
        YUS_UnloadTime = NULL,
        YUS_GS_NKUnloadUser = '',
        YUS_LoadTime = NULL,
        YUS_GS_NKLoadUser = '',
        YUS_WL_CurrentYardLocation = NULL,
        YUS_SystemLastEditTimeUtc = @CurrentUtcDate,
        YUS_SystemLastEditUser = '~BP'
    WHERE 
        YUS_UnloadTime IS NOT NULL
        AND (
            YUS_YDL_Delivery IS NULL
            OR YUS_YTU_ReceiveTransportationUnit IS NULL
            OR YUS_YRL_ReceiveLine IS NULL
        );
END TRY
BEGIN CATCH
    THROW;
END CATCH;";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
