using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Schema.Synchronisers;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Startup
{
	public class TransactionalAfterUpgradeTasks
	{
		public TransactionalAfterUpgradeTasks(AdminConnection connection, IUpgradeTaskWorkflowControl logger, bool isRunningSchemaOrScriptUpgrade)
		{
			this.connection = connection;
			this.logger = logger;
			this.isRunningSchemaOrScriptUpgrade = isRunningSchemaOrScriptUpgrade;
		}

		readonly AdminConnection connection;
		readonly IUpgradeTaskWorkflowControl logger;
		readonly bool isRunningSchemaOrScriptUpgrade;

		public void RunTasks()
		{
			EnsureCdcConfiguration();

			var auditTriggerFactory = new AuditTriggerFactory(connection, logger);
			var allSchemas = Db.CW1AdditionalSchemas.Prepend(WellKnownSqlNames.DbOwnerSchema);

			foreach (var schema in allSchemas)
			{
				EnsureAutoVersionTriggers(schema);

				if (ShouldAddAuditTriggers())
				{
					logger?.StartNonEstimatedTask($"Check audit trigger for schema: {schema}");
					auditTriggerFactory.EnsureAuditDetailTriggers(schema);
				}
				else
				{
					auditTriggerFactory.DeleteAuditTriggers(schema);
				}
			}

			SetupSystemLastEditTrigger(allSchemas);

			if (isRunningSchemaOrScriptUpgrade)
			{
				SynchroniseSynonyms();
			}

			RemoveRegistryIndex();
		}

		void SetupSystemLastEditTrigger(IEnumerable<string> schemas)
		{
			logger?.StartNonEstimatedTask($"Synchronising SystemLastEditAuditInfo update triggers");
			var systemLastEditTriggerSynchronizer = new SystemLastEditTriggerSynchronizer(connection, logger, schemas);

			if (RunningEnvironment.IsDebugMode)
			{
				systemLastEditTriggerSynchronizer.EnsureTriggers();
			}
			else
			{
				systemLastEditTriggerSynchronizer.DropAllTriggers();
			}
		}

		// Only adding these triggers in DEBUG mode. Most of the benefit of these triggers will come with failing tests
		// BAS to consider adding them in RELEASE as well.
		bool ShouldAddAuditTriggers()
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}

		protected void RemoveRegistryIndex()
		{
			// RegistryIndex is an XML cache of static registry items. After upgrade it must always be removed (and later populated on demand).
			connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName} WHERE {StmDataSchema.Constants.SD_Name} = 'RegistryIndex'");
		}

		void SynchroniseSynonyms()
		{
			logger.StartNonEstimatedTask("Synchronise database synonyms");
			new DbUserRepository().SynchroniseSynonyms(connection);
		}

		protected void EnsureCdcConfiguration()
		{
			if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.DisableAndStopJobsAndTrigger(connection, Db.DatabaseName);
			}
		}

		protected void EnsureAutoVersionTriggers(string schema)
		{
			logger?.StartNonEstimatedTask($"Check autoversion trigger for schema: {schema}");

			var dbConnectionInternals = (IDbConnectionInternals)connection;
			var autoVersionTriggerFactory = new AutoVersionTriggerFactory(
				logger,
				dbConnectionInternals.InternalDbConnection,
				dbConnectionInternals.InternalDbTransaction);
			autoVersionTriggerFactory.Setup(schema);
		}
	}
}
