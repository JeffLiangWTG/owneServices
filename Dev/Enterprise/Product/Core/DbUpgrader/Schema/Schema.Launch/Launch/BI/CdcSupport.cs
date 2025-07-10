namespace Enterprise.DbUpgrader.Schema
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Bi.Common;
	using CargoWise.Common;
	using CargoWise.Data;
	using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.MainDb;
	using CargoWise.Schema;
	using Enterprise.ChangeDataCapture.Common;
	using Enterprise.DbUpgrader.Shared;
	using Enterprise.Integration.Licensing;

	public class CdcSupport
	{
		public CdcSupport(IUpgradeManager manager, string dbName, AdminConnection upgConnection)
		{
			this.manager = manager;
			this.dbName = dbName;
			this.upgConnection = upgConnection;
		}

		public bool AreTablesTrackedByCdc => ShouldSynchroniseCdcTables &&
			CdcTable.GetTablesToEnableCdc(upgConnection)
				.Select(x => new CdcTable(x.SchemaName, x.TableName))
				.All(x => x.IsCdcEnabled(upgConnection));

		#region After Schema Synchronisation

		#region BI registry items

		bool IsAuditServerSet
		{
			get
			{
				if (isAuditServerSet == null)
				{
					isAuditServerSet = !string.IsNullOrEmpty(BiServers.LoadAuditServerUsingCacheIfPossible(upgConnection));
				}
				return isAuditServerSet.Value;
			}
		}
		bool? isAuditServerSet;

		bool IsDataWarehouseServerSet
		{
			get
			{
				if (isDwServerSet == null)
				{
					isDwServerSet = !string.IsNullOrEmpty(BiServers.LoadDataWarehouseServerUsingCacheIfPossible(upgConnection));
				}
				return isDwServerSet.Value;
			}
		}
		bool? isDwServerSet;

		#endregion

		bool ShouldSynchroniseCdcTables => CdcDatabase.IsEnabled(upgConnection, dbName) &&
			(IsAuditServerSet || IsDataWarehouseServerSet);

		public void SynchroniseCdcSchema()
		{
			if (ShouldSynchroniseCdcTables)
			{
				manager.StartNonEstimatedTask("CDC - Align capture tables with new schema");

				using (((ICurrentDbControl)upgConnection).UseDatabase(dbName))
				{
					SynchroniseCdcTables();
				}

				manager.ShowInfoMessage(".");
			}
		}

		void SynchroniseCdcTables()
		{
			try
			{
				CheckAlterCdcMetaObjectsTrigger();
				DisableCdcForChangedTables();
				EnableCdcForSelectedTables();
				RecreateCdcTriggers();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string msgHeader = String.Format("Failed to synchronise CDC tables [{0}]", dbName);
				throw new OdysseyDataException(String.Format(CultureInfo.InvariantCulture, "{0}\r\n{1}", msgHeader, ex.Message), ex);
			}
		}

		#region Ensure Allow Alter CDC Trigger is enabled

		internal void CheckAlterCdcMetaObjectsTrigger()
		{
			CreateTriggerIfNotExists();
			EnsureAlterCdcTriggerIsEnabled();
		}

		void CreateTriggerIfNotExists()
		{
			if (!upgConnection.Exists("from sys.triggers where name = 'TG_AllowAlterCDCMetaObjects'"))
			{
				manager.ShowInfoMessage("Creating database trigger [TG_AllowAlterCDCMetaObjects]");
				var scriptText = new TG_AllowAlterCDCMetaObjects().Text;
				upgConnection.ExecuteNonQuery(scriptText);
			}
		}

		void EnsureAlterCdcTriggerIsEnabled()
		{
			if (upgConnection.Exists("from sys.triggers where name = 'TG_AllowAlterCDCMetaObjects' and is_disabled = 1"))
			{
				manager.ShowInfoMessage("Enabling database trigger [TG_AllowAlterCDCMetaObjects]");
				upgConnection.ExecuteNonQuery("ENABLE TRIGGER TG_AllowAlterCDCMetaObjects ON DATABASE");
			}
		}

		#endregion

		#region Disable Changed Tables for CDC

		internal void DisableCdcForChangedTables()
		{
			DisableCaptureInstancesIfCdcTablesIsMissing();
			DisableUnsupportedCdcInstances();
			DisableCdcForChangedColumnListTables();
			DisableCdcForTablesWithModifiedColumns();
			DisableCdcForTablesWithMismatchingColumnIds();
		}

		void DisableCaptureInstancesIfCdcTablesIsMissing()
		{
			var sqlText = "IF EXISTS (select * from sys.tables where name = 'CdcTables' and schema_name(schema_id) = 'cdc') SELECT 1 ELSE SELECT 0";
			var cdcTableListExists = Convert.ToBoolean(upgConnection.ExecuteScalar(sqlText));
			if (!cdcTableListExists)
			{
				manager.ShowInfoMessage("Disabling CDC instances missing in admin table");
				CdcDatabase.DisableCaptureInstances(upgConnection);
			}
			else
			{
				sqlText = "IF EXISTS (select * from cdc.CdcTables) SELECT 1 ELSE SELECT 0";
				var isCdcTableListPopulated = Convert.ToBoolean(upgConnection.ExecuteScalar(sqlText));
				if (!isCdcTableListPopulated)
				{
					manager.ShowInfoMessage("Disabling CDC instances missing in admin table");
					CdcDatabase.DisableCaptureInstances(upgConnection);
				}
			}
		}

		void DisableUnsupportedCdcInstances()
		{
			manager.ShowInfoMessage("Disabling unsupported CDC instances");

			var cwCdcTables =
				CdcConfigurationInfo.GetEligibleCdcTables(upgConnection)
					.Select(cc => String.Format(CultureInfo.InvariantCulture, "('{0}', '{1}')", cc.SchemaName, cc.TableName));

			string cwCdcTabSql = String.Join(",", cwCdcTables);

			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				WITH
				CwCdcCte (SourceSchema, SourceTable, CaptureInstance) AS
				(
					SELECT
						CwCdcTab.SourceSchema,
						CwCdcTab.SourceTable,
						CaptureInstance = CwCdcTab.SourceSchema + '_' + CwCdcTab.SourceTable
					FROM
						(VALUES {0}) CwCdcTab(SourceSchema, SourceTable)
				)

				-- Capture Instances which are not in our configuration
				SELECT
					SourceSchema = SrcSch.name,
					SourceTable = SrcTab.name,
					CaptureInstance = ChgTab.capture_instance
				FROM
					cdc.change_tables ChgTab
					INNER JOIN sys.tables SrcTab ON SrcTab.object_id = ChgTab.source_object_id
					INNER JOIN sys.schemas SrcSch ON SrcSch.schema_id = SrcTab.schema_id
					LEFT JOIN CwCdcCte AS CwCdc
						ON CwCdc.SourceSchema = SrcSch.name
						AND CwCdc.SourceTable = SrcTab.name
						AND CwCdc.CaptureInstance = ChgTab.capture_instance
				WHERE
					CwCdc.CaptureInstance is null",
				cwCdcTabSql
			);

			DisableCdcInstancesFromQuery(sqlText);
		}

		void DisableCdcForChangedColumnListTables()
		{
			manager.ShowInfoMessage("Disabling CDC for tables with added or removed columns");

			var eligibleCdcTables = CdcConfigurationInfo.GetEligibleCdcTables(upgConnection);

			var cwCdcTablesAndColumns = new List<string>();

			foreach (var cdcConfigTable in eligibleCdcTables)
			{
				var cdcTable = new CdcTable(cdcConfigTable.SchemaName, cdcConfigTable.TableName);
				cdcTable.GetEligibleCdcColumns(IsAuditServerSet, IsDataWarehouseServerSet)
					.ForEach(c => cwCdcTablesAndColumns.Add(String.Format(CultureInfo.InvariantCulture, "('{0}', '{1}', '{2}')", cdcConfigTable.SchemaName, cdcConfigTable.TableName, c)));
			}

			string cwCdcTabSql = String.Join(",", cwCdcTablesAndColumns);

			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				WITH
				DbCdcCte (SourceTableId, SourceSchema, SourceTable, CaptureTableId, CaptureInstance) AS
				(
					SELECT
						SourceTableId = SrcTab.object_id,
						SourceSchema = SrcSch.name,
						SourceTable = SrcTab.name,
						CaptureTableId = ChgTab.object_id,
						CaptureInstance = ChgTab.capture_instance
					FROM
						cdc.change_tables ChgTab
						INNER JOIN sys.tables SrcTab ON SrcTab.object_id = ChgTab.source_object_id
						INNER JOIN sys.schemas SrcSch ON SrcSch.schema_id = SrcTab.schema_id
				),
				CwCdcCte (SourceSchema, SourceTable, CaptureInstance, ColumnName) AS
				(
					SELECT
						DbCdcTab.SourceSchema,
						DbCdcTab.SourceTable,
						DbCdcTab.CaptureInstance,
						CwCdcTab.ColumnName
					FROM
						DbCdcCte AS DbCdcTab
						INNER JOIN (VALUES {0}) CwCdcTab(SourceSchema, SourceTable, ColumnName)
							ON CwCdcTab.SourceSchema = DbCdcTab.SourceSchema
							AND CwCdcTab.SourceTable = DbCdcTab.SourceTable
				)

				-- New CDC eligible column added +
				-- CDC column removed or no longer eligible
				SELECT DISTINCT
					SourceSchema = isnull(DbCdcTab.SourceSchema, CwCdcCol.SourceSchema),
					SourceTable = isnull(DbCdcTab.SourceTable, CwCdcCol.SourceTable),
					CaptureInstance = isnull(DbCdcTab.CaptureInstance, CwCdcCol.CaptureInstance)
				FROM
					DbCdcCte AS DbCdcTab
					INNER JOIN cdc.captured_columns AS DbCdcCol
						ON DbCdcCol.object_id = DbCdcTab.CaptureTableId
					FULL OUTER JOIN CwCdcCte AS CwCdcCol
						ON CwCdcCol.SourceSchema = DbCdcTab.SourceSchema
						AND CwCdcCol.SourceTable = DbCdcTab.SourceTable
						AND CwCdcCol.ColumnName = DbCdcCol.column_name
				WHERE 1=2
					OR CwCdcCol.ColumnName is null
					OR DbCdcCol.column_name is null",
				cwCdcTabSql
			);

			DisableCdcInstancesFromQuery(sqlText);
		}

		void DisableCdcForTablesWithModifiedColumns()
		{
			manager.ShowInfoMessage("Disabling CDC for tables with modified columns");

			string sqlText = @"
				-- Column type changed
				SELECT DISTINCT
					SourceSchema = SrcSch.name,
					SourceTable = SrcTab.name,
					CaptureInstance = ChgTab.capture_instance
				FROM
					cdc.change_tables ChgTab
					INNER JOIN sys.tables SrcTab ON SrcTab.object_id = ChgTab.source_object_id
					INNER JOIN sys.schemas SrcSch ON SrcSch.schema_id = SrcTab.schema_id
					INNER JOIN sys.columns AS SrcCol ON SrcCol.object_id = SrcTab.object_id
					INNER JOIN sys.columns AS CdcCol ON CdcCol.object_id = ChgTab.object_id AND CdcCol.name = SrcCol.name
				WHERE 1=2
					OR SrcCol.user_type_id != CdcCol.user_type_id
					OR SrcCol.max_length != CdcCol.max_length
					OR SrcCol.[precision] != CdcCol.[precision]
					OR SrcCol.scale != CdcCol.scale";

			DisableCdcInstancesFromQuery(sqlText);
		}

		void DisableCdcForTablesWithMismatchingColumnIds()
		{
			manager.ShowInfoMessage("Disabling CDC for tables with mismatching column_id's");

			string sqlText = @"
				-- Column type changed
				SELECT DISTINCT
					SourceSchema = SCHEMA_NAME(t.schema_id),
					SourceTable = t.name,
					CaptureInstance = ct.capture_instance
				from sys.tables t
					inner join cdc.change_tables ct on ct.source_object_id = t.object_id
					inner join sys.columns c on c.object_id = t.object_id
					inner join cdc.captured_columns cc on cc.column_name = c.name and cc.object_id = ct.object_id
				Where c.column_id <> cc.column_id";

			DisableCdcInstancesFromQuery(sqlText);
		}

		void DisableCdcInstancesFromQuery(string getCdcInstancesToDisableSql)
		{
			var cdcInstancesToDisable = DataUtils.GetDataTableFromQuery(upgConnection, getCdcInstancesToDisableSql);

			foreach (DataRow row in cdcInstancesToDisable.Rows)
			{
				string schemaName = row[0].ToString();
				string tableName = row[1].ToString();
				string captureInstance = row[2].ToString();
				manager.ShowInfoMessage(String.Format(CultureInfo.InvariantCulture, "(-) {0}.{1} ({2})", schemaName, tableName, captureInstance));

				// Remove existing CDC configuration
				var cdcTable = new CdcTable(schemaName, tableName);
				cdcTable.DisableCdc(upgConnection, captureInstance);
			}
		}

		#endregion

		#region Enable Changed Tables for CDC

		void EnableCdcForSelectedTables()
		{
			try
			{
				EnableCdcForSelectedTablesUnsafe();
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CdcDataOutdated)
			{
				manager.ShowInfoMessage("Upgrading CDC metadata after SQL Server version upgrade");
				CdcDatabase.RunInternalCdcUpgrade(upgConnection, dbName);

				try
				{
					EnableCdcForSelectedTablesUnsafe();
				}
#if DEBUG
				catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.CdcDataOutdated)
				{
					string msgHeader = String.Format(CultureInfo.InvariantCulture, "Failed to synchronise CDC tables [{0}].", dbName);
					throw new OdysseyDataException(String.Format(CultureInfo.InvariantCulture, "{0}\r\nCDC configuration is out of date. Get the latest version and rebuild binaries.", msgHeader), e);
				}
#endif
				catch
				{
					throw;
				}
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CannotAllowAlterCDCMetaObjects)
			{
				var errorMessage = "This database does not permit CDC Allow Alter Meta Data - Please enable TF15006. Please search for Technical Advisory Notes for more information.";
				throw new MandatoryTraceFlagMissingException(errorMessage, ex);
			}
		}

		protected virtual void EnableCdcForSelectedTablesUnsafe()
		{
			manager.ShowInfoMessage("Enabling CDC for selected tables");
			var tablesToBeEnabled = CdcTable.GetTablesToEnableCdc(upgConnection);

			if (!IsWiseTechInternalSystem())
			{
				CdcConfigurationInfo.SupportsNetChangesFlag = IsDataWarehouseServerSet;
			}

			bool isFirstTable = Convert.ToBoolean(upgConnection.ExecuteScalar("IF NOT EXISTS (select null from cdc.change_tables) SELECT 1 ELSE SELECT 0"), CultureInfo.InvariantCulture);
			if (isFirstTable)
			{
				upgConnection.ExecuteNonQuery("insert into cdc.change_tables (object_id, capture_instance, partition_switch) values(1, 'dbo_InitialTableValue', 1)");
			}

			if (tablesToBeEnabled.Any())
			{
				var enabledCdcTables = string.Join(",", tablesToBeEnabled.Select(t => $"{t.SchemaName}.{t.TableName}"));
				DbRegistry.EnabledCdcTables.SaveValue(enabledCdcTables, upgConnection);
			}

			foreach (var table in tablesToBeEnabled)
			{
				manager.ShowInfoMessage(String.Format(CultureInfo.InvariantCulture, "(+) {0}.{1}", table.SchemaName, table.TableName));
				var cdcTable = new CdcTable(table.SchemaName, table.TableName);
				cdcTable.EnableCdc(upgConnection, captureInstance: null, filegroup: null, IsAuditServerSet, IsDataWarehouseServerSet);

				if (isFirstTable)
				{
					upgConnection.ExecuteNonQuery("delete from cdc.change_tables where capture_instance = 'dbo_InitialTableValue'");
					isFirstTable = false;
				}
			}
		}

		static bool IsWiseTechInternalSystem()
		{
			var registration = CargoWise.Application.ObjectFactory.Get<IProductRegistration>();
			return registration.IsWiseTechGlobalInternalSystem();
		}

		#endregion

		#region Recreating CDC Triggers

		void RecreateCdcTriggers()
		{
			manager.ShowInfoMessage("Recreating CDC Triggers");
			CdcTable.RecreateCdcTriggers(upgConnection);
		}

		#endregion

		#endregion

		protected readonly string dbName;
		protected readonly IUpgradeManager manager;
		protected readonly AdminConnection upgConnection;
	}
}
