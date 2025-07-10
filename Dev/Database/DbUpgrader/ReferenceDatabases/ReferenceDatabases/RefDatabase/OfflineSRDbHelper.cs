#if DEBUG
using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	public static class OfflineSRDbHelper
	{
		public static void ResetSRDbAndPopulateSchema(DbConnection connection, IUpgradeTaskWorkflowLogger logger)
		{
			var sRDbName = RefDbTableNameResolver.DefaultSingleRefDbName;
			var sRDbExist = connection.DatabaseExists(sRDbName);
			using (var adminConnection = Db.NewAdminConnection())
			{
				if (sRDbExist)
				{
					sRDbExist = ResetSRDb(adminConnection, logger);
				}

				if (!sRDbExist)
				{
					logger?.ShowInfoMessage("Begin Create SRDb:");
					RefDatabaseInitialiser.CreateRefDbIfNotExists(adminConnection, sRDbName);
					logger?.ShowInfoMessage("Finish Create SRDb.");
					((ICurrentDbControl)adminConnection).UseDatabase(sRDbName);
				}
				logger?.ShowInfoMessage("Ensure guest permissions on SRDb:");
				EnsureGuestHasAllPermissionsAsCwRestrictedWriterRole(adminConnection);
				logger?.ShowInfoMessage("Begin Populate Schema in SRDb:");
				SRDbOfflineSupport.PopulateOfflineSRDbSchema(((IDbConnectionInternals)adminConnection).ADOConnection);
				logger?.ShowInfoMessage("Finish Populate Schema in SRDb.");
			}

			DataUtils.SaveDbExtendedProperty(connection, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, "Y", sRDbName);
			logger?.ShowInfoMessage("Set extendedproperty SingleRefDatabaseConsumeDATSnapshot to Y");
			logger?.ShowInfoMessage("Populate SRDb Schema successfully");
		}

		public static bool ResetSRDb(AdminConnection adminConnection, IUpgradeTaskWorkflowLogger logger)
		{
			var result = false;
			try
			{
				logger?.ShowInfoMessage("Begin Reset SRDb:");
				((ICurrentDbControl)adminConnection).UseDatabase(RefDbTableNameResolver.DefaultSingleRefDbName);
				var runner = new BatchRunner();
				runner.RunCommandsGeneratedByQuery(adminConnection, DropAllConstraints);
				runner.RunCommandsGeneratedByQuery(adminConnection, DropAllTablesViewsFunctionsProcedures);
				runner.RunCommandsGeneratedByQuery(adminConnection, DropXMLSchemCollection);
				DataUtils.DropDbExtendedProperty(adminConnection, RefDatabaseInitialiser.RefDbVersionProperty);
				logger?.ShowInfoMessage("Finish Reset SRDb.");
				result = true;
			}
			catch (Exception ex)
			{
				logger?.ShowInfoMessage($"Fail to reset SRDb: {ex.Message}");
				ErrorReporter.ReportOnce("Fail to reset SRDb", ex);
				new DbRemover(RefDbTableNameResolver.DefaultSingleRefDbName).Drop(adminConnection);
			}
			return result;
		}

		public static void EnsureGuestHasAllPermissionsAsCwRestrictedWriterRole(DbConnection connection)
		{
			connection.ExecuteNonQuery(GetEnableGuestHasAllPermissionsAsCwRestrictedWriterRole(new CwRestrictedWriterRole()));
		}

		const string DropAllConstraints = @"
   SET NOCOUNT ON
   DECLARE  @SchemaName NVARCHAR(100) = 'dbo'


   SELECT 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id))
   +'.'+ QUOTENAME(OBJECT_NAME(s.parent_object_id))
   + ' DROP CONSTRAINT ' +  QUOTENAME(s.name)   
   FROM sys.foreign_keys s
   JOIN sys.tables t on s.parent_object_id = t.object_id
   JOIN sys.tables t2 on s.referenced_object_id = t2.object_id
   WHERE t2.schema_id = SCHEMA_ID(@SchemaName)";

		const string DropAllTablesViewsFunctionsProcedures = @"
   SET NOCOUNT ON
   DECLARE  @SchemaName NVARCHAR(100) = 'dbo'

   SELECT 'DROP ' +
					CASE WHEN type IN ('P','PC') THEN 'PROCEDURE'
						 WHEN type =  'U' THEN 'TABLE'
						 WHEN type IN ('IF','TF','FN') THEN 'FUNCTION'
						 WHEN type = 'V' THEN 'VIEW'
					 END +
				   ' ' +  QUOTENAME(SCHEMA_NAME(schema_id))+'.'+QUOTENAME(name)  
				   FROM sys.objects
			 WHERE schema_id = SCHEMA_ID(@SchemaName)
	AND type IN('P','PC','U','IF','TF','FN','V')
	ORDER BY  CASE WHEN type IN ('P','PC') THEN 4
						 WHEN type =  'U' THEN 3
						 WHEN type IN ('IF','TF','FN') THEN 1
						 WHEN type = 'V' THEN 2
					   END";

		const string DropXMLSchemCollection = @"
	SET NOCOUNT ON
	DECLARE  @SchemaName NVARCHAR(100) = 'dbo'

	SELECT 'DROP XML SCHEMA COLLECTION '
	+ QUOTENAME(SCHEMA_NAME(schema_id))+'.'+QUOTENAME(name)
	FROM sys.xml_schema_collections
	WHERE schema_id = SCHEMA_ID(@SchemaName)";

		static string GetEnableGuestHasAllPermissionsAsCwRestrictedWriterRole(CwRestrictedWriterRole cwRestrictedWriterRole)
	=> $@"GRANT {string.Join(",", cwRestrictedWriterRole.DbDatabasePermissions)} TO guest;
GRANT {string.Join(",", cwRestrictedWriterRole.DbSchemaPermissions)} ON SCHEMA::dbo TO guest;
";
	}
}
#endif
