[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(Enterprise.ZArchitecture.Schema.GlbStaffSchema))]

namespace Enterprise.ChangeDataCapture.Common
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using CargoWise.Bi.Common;
	using CargoWise.Bi.ConfigLoader;
	using CargoWise.Data;
	using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

	#region SuppressResourceStringsCheckRegion

	public class CdcTable
	{
		public CdcTable(string schemaName, string tableName)
		{
			this.schemaName = schemaName;
			this.tableName = tableName;
		}

		protected readonly string schemaName;
		protected readonly string tableName;

		#region Enable CDC

		public bool IsCdcEnabled(DbConnection conn)
		{
			string sqlText = @"
				SELECT is_tracked_by_cdc
				FROM sys.schemas s
				INNER JOIN sys.tables t ON t.schema_id = s.schema_id
				WHERE s.name = @SchemaName
				AND t.name = @TableName";

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@SchemaName", SqlDbType.NVarChar, 128, schemaName);
				cmd.AddParameter("@TableName", SqlDbType.NVarChar, 128, tableName);
				object objResult = cmd.ExecuteScalar();
				return !(objResult == null || objResult == DBNull.Value) && Convert.ToBoolean(objResult, CultureInfo.InvariantCulture);
			}
		}

		public void EnableCdc(DbConnection conn, string captureInstance = null, string filegroup = null, bool includeAuditColumns = true, bool includeEdwColumns = true, bool includeAllColumns = false)
		{
			if (string.IsNullOrWhiteSpace(filegroup))
			{
				filegroup = FileGroupName;
			}

			string capturedColumnList = String.Empty;
			if (!includeAllColumns)
			{
				var cdcColumns = GetEligibleCdcColumns(includeAuditColumns, includeEdwColumns);
				capturedColumnList = (cdcColumns.Any())
					? ("[" + string.Join("],[", cdcColumns) + "]")
					: null;
			}

			using (var cmd = conn.Command("sys.sp_cdc_enable_table"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@source_schema", SqlDbType.NVarChar, schemaName);
				cmd.AddParameter("@source_name", SqlDbType.NVarChar, tableName);
				cmd.AddParameter("@role_name", SqlDbType.NVarChar, new CwUnrestrictedWriterRole().Name);

				if (CdcConfigurationInfo.SupportsNetChangesFlag != null)
				{
					cmd.AddParameter("@supports_net_changes", SqlDbType.Bit, CdcConfigurationInfo.SupportsNetChangesFlag);
				}

				if (!string.IsNullOrWhiteSpace(capturedColumnList))
				{
					cmd.AddParameter("@captured_column_list", SqlDbType.NVarChar, capturedColumnList);
				}

				if (!string.IsNullOrWhiteSpace(filegroup))
				{
					cmd.AddParameter("@filegroup_name", SqlDbType.NVarChar, filegroup);
				}

				if (!string.IsNullOrWhiteSpace(captureInstance))
				{
					this.CaptureInstance = captureInstance;
					cmd.AddParameter("@capture_instance", SqlDbType.NVarChar, captureInstance);
				}

				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CdcSourceTableHasNeitherPrimaryKeyNorUniqueIndex)
				{
					cmd.SetParameterValue("@supports_net_changes", 0);
					cmd.ExecuteNonQuery();
				}
			}

			if (string.IsNullOrEmpty(captureInstance) || captureInstance.Equals($"{schemaName}_{tableName}", StringComparison.OrdinalIgnoreCase))
			{
				InsertToCdcTableList(conn);
			}
		}

		public void InsertToCdcTableList(DbConnection conn)
		{
			var sqlText = @"
			IF OBJECT_ID('cdc.CdcTables') IS NULL
			BEGIN
				CREATE TABLE cdc.CdcTables (CaptureInstance SYSNAME NOT NULL, Lsn BINARY(10) NOT NULL, LastDUProcessedLsn BINARY(10) NOT NULL DEFAULT 0x00000000000000000000, PkColumnList VARCHAR(MAX) DEFAULT NULL)

				ALTER TABLE cdc.CdcTables
				ADD CONSTRAINT PK_UX__CdcTables_CaptureInstance PRIMARY KEY CLUSTERED (CaptureInstance)
			END

			IF NOT EXISTS (SELECT * FROM cdc.CdcTables WHERE CaptureInstance = @InstanceName)
				INSERT INTO cdc.CdcTables (CaptureInstance, Lsn) VALUES(@InstanceName, 0x0)";

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@InstanceName", SqlDbType.VarChar, CaptureInstance);
				cmd.ExecuteNonQuery();
			}
		}

		protected virtual string FileGroupName
		{
			get
			{
				return CdcDatabase.FileGroup;
			}
		}

		#endregion

		#region Enable Selected Tables and Columns

		public static List<CdcConfigurationInfo> GetTablesToEnableCdc(DbConnection connection)
		{
			var tablesFromCdcConfig = CdcConfigurationInfo.GetEligibleCdcTables(connection);

			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT distinct sch.name, tab.name
				FROM
					sys.tables tab
					INNER JOIN sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN (VALUES {0}) CdcTable([schema], [name]) ON CdcTable.[schema] = sch.name AND CdcTable.[name] = tab.name
					LEFT JOIN cdc.change_tables chg ON chg.source_object_id = tab.object_id
				WHERE
					chg.source_object_id is null
				ORDER BY
					sch.name,
					tab.name;",
				string.Join(",", tablesFromCdcConfig.Select(t => string.Format(CultureInfo.InvariantCulture, "('{0}', '{1}')", t.SchemaName, t.TableName)))
			);

			var tablesToEnableCdc = DataUtils.GetDataTableFromQuery(connection, sqlText);
			var enabledTableList = new List<CdcConfigurationInfo>();

			foreach (DataRow row in tablesToEnableCdc.Rows)
			{
				string schemaName = row[0].ToString();
				string tableName = row[1].ToString();

				enabledTableList.Add(new CdcConfigurationInfo(schemaName, tableName));
			}

			return enabledTableList;
		}

		public virtual IEnumerable<string> GetEligibleCdcColumns(bool includeAuditColumns = true, bool includeEdwColumns = true)
		{
			var cdcTable = CdcConfigTables.FirstOrDefault(t =>
				string.Equals(t.SourceSchema, schemaName, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(t.SourceTable, tableName, StringComparison.OrdinalIgnoreCase));

			var cdcColumnList = new List<string>();

			if (cdcTable != null)
			{
				foreach (var column in cdcTable.GetOrderedCdcColumnConfigRows())
				{
					if ((includeAuditColumns &&
							(column.ColumnInAudit ||
								(!string.IsNullOrEmpty(cdcTable.AuditFilter) &&
								 new Regex($"\\b{column.SourceColumn}\\b", RegexOptions.IgnoreCase).IsMatch(cdcTable.AuditFilter))))
						||
						(includeEdwColumns &&
							(column.ColumnInEdw ||
								(!string.IsNullOrEmpty(cdcTable.EdwFilter) &&
								 new Regex($"\\b{column.SourceColumn}\\b", RegexOptions.IgnoreCase).IsMatch(cdcTable.EdwFilter)))))
					{
						cdcColumnList.Add(column.SourceColumn);
					}
				}
			}

			return cdcColumnList.AsEnumerable();
		}

		#endregion

		#region Disable CDC

		public void DisableCdc(DbConnection conn, string captureInstance)
		{
			using (var cmd = conn.Command("sys.sp_cdc_disable_table"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@source_schema", SqlDbType.NVarChar, schemaName);
				cmd.AddParameter("@source_name", SqlDbType.NVarChar, tableName);
				cmd.AddParameter("@capture_instance", SqlDbType.NVarChar, captureInstance);
				cmd.ExecuteNonQuery();
			}

			RemoveFromCdcTableList(conn, captureInstance);
		}

		void RemoveFromCdcTableList(DbConnection conn, string captureInstance)
		{
			var sqlText = $"IF EXISTS (select * from sys.tables where name = 'CdcTables' and schema_name(schema_id) = 'cdc') DELETE FROM cdc.CdcTables WHERE CaptureInstance = '{captureInstance}'";
			conn.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region Capture Instances

		public void DisableCdcInstances(DbConnection conn)
		{
			var captureInstances = new List<string>();

			string sqlText = @"
				SELECT ChgTab.capture_instance
				FROM
					cdc.change_tables ChgTab
					INNER JOIN sys.tables SrcTab ON SrcTab.object_id = ChgTab.source_object_id
					INNER JOIN sys.schemas SrcSch ON SrcSch.schema_id = SrcTab.schema_id
				WHERE
					SrcSch.name = @SchemaName
					AND SrcTab.name = @TableName";

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@SchemaName", SqlDbType.NVarChar, 128, schemaName);
				cmd.AddParameter("@TableName", SqlDbType.NVarChar, 128, tableName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						captureInstances.Add(reader[0].ToString());
					}
				}
			}

			if (captureInstances.Count > 0)
			{
				foreach (string captureInstance in captureInstances)
				{
					DisableCdc(conn, captureInstance);
				}
			}
		}

		#endregion

		#region CDC Triggers

		public static void RecreateCdcTriggers(DbConnection connection)
		{
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				try
				{
					DropCdcTriggers(connection);
					CreateCdcTriggers(connection);

					transactionManager.CommitTransaction();
				}
				catch
				{
					transactionManager.RollbackTransaction();
					throw;
				}
			}
		}

		#region Create CDC Triggers

		static void CreateCdcTriggers(DbConnection connection)
		{
			var triggerFilterDictionary = GetCaptureInstancesToCreateCdcTriggers(connection);
			foreach (var triggerFilter in triggerFilterDictionary)
			{
				CreateCdcTriggerForCaptureInstance(connection, triggerFilter.Key, triggerFilter.Value);
			}
		}

		static Dictionary<string, string> GetCaptureInstancesToCreateCdcTriggers(DbConnection connection)
		{
			var isAuditEnabled = !string.IsNullOrEmpty(BiServers.LoadAuditServerUsingCacheIfPossible(connection));
			var isEdwEnabled = !string.IsNullOrEmpty(BiServers.LoadDataWarehouseServerUsingCacheIfPossible(connection));

			var triggerFilterDictionary = new Dictionary<string, string>();
			foreach (var cdcTable in BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig)
			{
				var filter = GetCdcTableTriggerFilter(cdcTable, isAuditEnabled, isEdwEnabled);
				if (!string.IsNullOrEmpty(filter))
				{
					triggerFilterDictionary[$"{cdcTable.SourceSchema}_{cdcTable.SourceTable}"] = filter;
				}
			}
			return triggerFilterDictionary;
		}

		public static string GetCdcTableTriggerFilter(CdcTableConfigRow cdcTable, bool isAuditEnabled, bool isEdwEnabled)
		{
			string auditFilter = null;
			var shouldIncludeAuditFilter = isAuditEnabled && cdcTable.TableInAudit;
			if (shouldIncludeAuditFilter)
			{
				auditFilter = cdcTable.AuditFilter;
			}

			string edwFilter = null;
			var shouldIncludeEdwFilter = isEdwEnabled && cdcTable.TableInEdw;
			if (shouldIncludeEdwFilter)
			{
				edwFilter = cdcTable.EdwFilter;
			}

			if (shouldIncludeAuditFilter && shouldIncludeEdwFilter)
			{
				if (!string.IsNullOrEmpty(auditFilter) && auditFilter.Equals(edwFilter, StringComparison.OrdinalIgnoreCase))
				{
					return auditFilter;
				}
				else if (!string.IsNullOrEmpty(auditFilter) && !string.IsNullOrEmpty(edwFilter))
				{
					return $"({auditFilter}) OR ({edwFilter})";
				}
				else
				{
					return null;
				}
			}
			else if (!shouldIncludeAuditFilter && shouldIncludeEdwFilter)
			{
				return edwFilter;
			}
			else if (!shouldIncludeEdwFilter && shouldIncludeAuditFilter)
			{
				return auditFilter;
			}
			else
			{
				return null;
			}
		}

		public static void CreateCdcTriggerForCaptureInstance(DbConnection connection, string captureInstance, string filter)
		{
			var triggerName = $"TR_II_{captureInstance}_CT";
			var cdcTableName = $"{captureInstance}_CT";

			var createTriggerScript = string.Format(CultureInfo.InvariantCulture,
@"CREATE TRIGGER [{0}] ON [cdc].[{1}]
INSTEAD OF INSERT AS
BEGIN
	INSERT INTO [cdc].[{1}]
	SELECT *
	FROM inserted i
	WHERE ({2})
END", triggerName, cdcTableName, filter);

			connection.ExecuteNonQuery(createTriggerScript);
		}

		#endregion

		#region Drop CDC Triggers

		static void DropCdcTriggers(DbConnection connection)
		{
			var triggersToDrop = GetCdcTriggersFromDatabase(connection);
			foreach (var triggerName in triggersToDrop)
			{
				connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "DROP TRIGGER [cdc].[{0}]", triggerName));
			}
		}

		static IEnumerable<string> GetCdcTriggersFromDatabase(DbConnection connection)
		{
			var sqlText = @"select tr.name
from sys.triggers tr
inner join sys.tables t
	on tr.parent_id = t.object_id
where schema_name(t.schema_id) = 'cdc' and tr.is_instead_of_trigger = 1";

			return DataUtils.GetListOfValuesFromQuery(connection, sqlText);
		}

		#endregion

		#endregion

		#region CDC Configuration

		CdcTableConfigDataTable CdcConfigTables => cdcTables ?? (cdcTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig);

		CdcTableConfigDataTable cdcTables;
		string captureInstance;
		public string CaptureInstance
		{
			get
			{
				if (string.IsNullOrEmpty(captureInstance))
				{
					captureInstance = $"{schemaName}_{tableName}";
				}
				return captureInstance;
			}
			private set
			{
				captureInstance = value;
			}
		}

		#endregion
	}

	#endregion
}
