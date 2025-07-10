using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow
{
	public class UpdateGridSettingsRevertAddingIndexRegistryItem : DataTransformation
	{
		public override string UserDescription => "Transform value from Glow GridSettings with Index_ to remove the Index_ text.";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
BEGIN TRY
	-- get rid of this table in case it exists
	DROP TABLE IF EXISTS #columnSettingsToProcess;

	-- get all the column settings that need to be renamed, also use the new name to find the PKs of any possible collisions
	SELECT recordsToUpdate.SD_PK, recordsToUpdate.SD_Name, recordsToUpdate.newName, recordsToUpdate.SD_Owner, existingData.SD_PK collisionPK
	INTO #columnSettingsToProcess
	FROM (
		SELECT SD_PK, SD_Name, SD_Owner, CONCAT('GridSettings_RDT', SUBSTRING(SD_Name, LEN('GridSettings_RDT_Index_'), 300)) newName
			FROM StmData
			WHERE SD_Name LIKE 'GridSettings_RDT_Index_%'
		UNION 
		SELECT SD_PK, SD_Name, SD_Owner, CONCAT('GridSettings_SEP', SUBSTRING(SD_Name, LEN('GridSettings_SEP_Index_'), 300)) newName
			FROM StmData
			WHERE SD_Name LIKE 'GridSettings_SEP_Index_%'
		UNION 
		SELECT SD_PK, SD_Name, SD_Owner, CONCAT('GridSettings_SLS', SUBSTRING(SD_Name, LEN('GridSettings_SEP_Index_'), 300)) newName
			FROM StmData
			WHERE SD_Name LIKE 'GridSettings_SLS_Index_%'
		UNION 
		SELECT SD_PK, SD_Name, SD_Owner, CONCAT('GridSettings_SDT', SUBSTRING(SD_Name, LEN('GridSettings_SDT_Index_'), 300)) newName
			FROM StmData
			WHERE SD_Name LIKE 'GridSettings_SDT_Index_%'
		UNION 
		SELECT SD_PK, SD_Name, SD_Owner, CONCAT('GridSettings_Search', SUBSTRING(SD_Name, LEN('GridSettings_Search_Index_'), 300)) newName
			FROM StmData
			WHERE SD_Name LIKE 'GridSettings_Search_Index_%'
	) recordsToUpdate
		LEFT JOIN StmData existingData ON recordsToUpdate.newName = existingData.SD_Name AND recordsToUpdate.SD_Owner = existingData.SD_Owner;

	-- delete collisions
	DELETE FROM StmData
	WHERE SD_PK IN (
		SELECT collisionPK FROM #columnSettingsToProcess
	);

	-- update the names 
	UPDATE StmData 
		SET SD_Name = newName,
		SD_SystemLastEditTimeUtc = GETUTCDATE(),
		SD_SystemLastEditUser = '~BP'
	FROM StmData 
		INNER JOIN #columnSettingsToProcess ON StmData.SD_PK = #columnSettingsToProcess.SD_PK;

	DROP TABLE IF EXISTS #columnSettingsToProcess;
END TRY
BEGIN CATCH
	DROP TABLE IF EXISTS #columnSettingsToProcess; 
	THROW;
END CATCH;


BEGIN TRY
	-- get rid of this table in case it exists
	DROP TABLE IF EXISTS #filterSettingsToProcess;

	-- get all the filter settings that need to be renamed, also use the new name to find the PKs of any possible collisions
	SELECT filterRecordsToUpdate.S9_PK, filterRecordsToUpdate.S9_ModuleID, filterRecordsToUpdate.newName, filterRecordsToUpdate.S9_RelatedEntityID, existingFilter.S9_PK collisionPK
	INTO #filterSettingsToProcess
	FROM (
		SELECT S9_PK, S9_ModuleID, S9_RelatedEntityID, CONCAT('RDT', SUBSTRING(S9_ModuleID, LEN('RDT_Index_'), 300)) newName
			FROM StmModuleFilter
			WHERE S9_ModuleID LIKE 'RDT_Index_%'
		UNION 
		SELECT S9_PK, S9_ModuleID, S9_RelatedEntityID, CONCAT('SEP', SUBSTRING(S9_ModuleID, LEN('SEP_Index_'), 300)) newName
			FROM StmModuleFilter
			WHERE S9_ModuleID LIKE 'SEP_Index_%'
		UNION 
		SELECT S9_PK, S9_ModuleID, S9_RelatedEntityID, CONCAT('SLS', SUBSTRING(S9_ModuleID, LEN('SEP_Index_'), 300)) newName
			FROM StmModuleFilter
			WHERE S9_ModuleID LIKE 'SLS_Index_%'
		UNION 
		SELECT S9_PK, S9_ModuleID, S9_RelatedEntityID, CONCAT('SDT', SUBSTRING(S9_ModuleID, LEN('SDT_Index_'), 300)) newName
			FROM StmModuleFilter
			WHERE S9_ModuleID LIKE 'SDT_Index_%'
		UNION 
		SELECT S9_PK, S9_ModuleID, S9_RelatedEntityID, CONCAT('Search', SUBSTRING(S9_ModuleID, LEN('Search_Index_'), 300)) newName
			FROM StmModuleFilter
			WHERE S9_ModuleID LIKE 'Search_Index_%'
	) filterRecordsToUpdate
		LEFT JOIN StmModuleFilter existingFilter ON filterRecordsToUpdate.newName = existingFilter.S9_ModuleID AND filterRecordsToUpdate.S9_RelatedEntityID = existingFilter.S9_RelatedEntityID;

	-- delete collisions
	DELETE FROM StmModuleFilter
	WHERE S9_PK IN (
		SELECT collisionPK FROM #filterSettingsToProcess
	);

	-- update the names 
	UPDATE StmModuleFilter 
		SET S9_ModuleID = newName,
		S9_SystemLastEditTimeUtc = GETUTCDATE(),
		S9_SystemLastEditUser = '~BP'
	FROM StmModuleFilter 
		INNER JOIN #filterSettingsToProcess ON StmModuleFilter.S9_PK = #filterSettingsToProcess.S9_PK;

	DROP TABLE IF EXISTS #filterSettingsToProcess;
END TRY
BEGIN CATCH
	DROP TABLE IF EXISTS #filterSettingsToProcess; 
	THROW;
END CATCH;
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
