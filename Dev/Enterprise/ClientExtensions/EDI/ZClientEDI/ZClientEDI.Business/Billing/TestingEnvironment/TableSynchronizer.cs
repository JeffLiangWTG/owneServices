using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace ZClientEDI.Business.Billing.TestingEnvironment
{
	public abstract class TableSynchronizer
	{
		public TableSynchronizer(ILogger logger)
		{
			Logger = logger;
			TablesToSync = GetTablesToSync();
			TableDependencies = SchemaHelper.GetTableDependencies();
			ReferencedTableFullNames = new HashSet<string>(SchemaHelper.GetReferencedTables().Select(x => x.FullName));
		}

		public virtual void Synchronise()
		{
			try
			{
				using (var mutex = CreateAndLockMutex())
				{
					if (mutex == null)
					{
						Log(LogType.Error, "Unable to obtain the table synchronizer lock as it is currently held by another session.");
						return;
					}

					using (Db.DisposableActionForDbConnection())
					using (SourceServerConnection = NewSourceServerConnection())
					using (TargetServerConnection = NewTargetServerConnection())
					{
						if (!Validate())
						{
							return;
						}

						GenerateStagingTables();
						SynchroniseTargetTables();
					}
				}
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
			finally
			{
				SourceServerConnection = null;
				TargetServerConnection = null;
			}
		}

		protected virtual bool Validate()
		{
			if (string.Equals(SourceServerConnection.ServerNameReportedByDatabase, TargetServerConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(SourceServerConnection.CurrentDatabase, TargetServerConnection.CurrentDatabase, StringComparison.OrdinalIgnoreCase))
			{
				Log(LogType.Error, "The source and destination databases must be different.");
				return false;
			}

			return true;
		}

		void GenerateStagingTables()
		{
			foreach (var table in TablesToSync)
			{
				Log($"Generating Staging Table {table.TableName} ...");
				var stagingTableName = $"{table.SchemaName}.{StagingTableNamePrefix}{table.TableName}";
				var stagingTableDataQuery = GenerateStagingTableDataQuery(table);
				var colNames = SchemaHelper.GetTableColumnsAsString(table);

				//generate empty staging table schema
				TargetServerConnection.ExecuteNonQuery($@"
IF OBJECT_ID('{stagingTableName}', 'U') IS NOT NULL
BEGIN
DROP TABLE {stagingTableName};
END
SELECT
{colNames}
INTO {stagingTableName} FROM {table.FullName} WITH(NOLOCK) WHERE 1 = 0;");

				BulkCopyTable(stagingTableName, stagingTableDataQuery);
			}
		}

		protected void BulkCopyTable(string destinationTableName, string sourceDataQuery)
		{
			using (var dataTable = DataUtils.GetDataTableFromQuery(SourceServerConnection, sourceDataQuery))
			using (var bulkCopy = TargetServerConnection.GetSqlBulkCopy())
			{
				bulkCopy.BulkCopyTimeout = TargetServerConnection.DefaultCommandTimeOutInSeconds;
				bulkCopy.BatchSize = 100000;
				bulkCopy.DestinationTableName = destinationTableName;

				foreach (DataColumn column in dataTable.Columns)
				{
					bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
				}

				using (NewDisableTriggerAction(TargetServerConnection, destinationTableName))
				{
					bulkCopy.WriteToServer(dataTable);
				}
			}
		}

		string GenerateStagingTableDataQuery(DatabaseTable table)
		{
			var query = new StringBuilder();
			query.AppendLine("SELECT");

			var colsAsString = string.Join(",\r\n", SchemaHelper.GetTableColumns(table).Select(x => GetStagingTableColumnSelectQuery(x)));
			query.AppendLine(colsAsString);
			query.AppendLine($"FROM {table.FullName} WITH(NOLOCK)");

			var additionalWhereClause = GetStagingTableAdditionalWhereClause(table);
			if (!string.IsNullOrWhiteSpace(additionalWhereClause))
			{
				query.AppendLine($"WHERE {additionalWhereClause}");
			}

			return query.ToString();
		}

		void SynchroniseTargetTables()
		{
			Log($"Generating Table Dependencies ...");
			var tablesToDelete = new List<DatabaseTable>(TablesToSync);
			using (NewDisableCdcAction(TargetServerConnection))
			{
				OnSynchronisingTargetTables(tablesToDelete);
				foreach (var tableToDelete in TableDependencies.Join(tablesToDelete,
																x => x.FullName,
																y => y.FullName,
																(x, y) => x)
																.OrderByDescending(x => x.Sequence))
				{
					DeleteTable(tableToDelete.FullName);
				}

				foreach (var table in TableDependencies.Join(TablesToSync,
															x => x.FullName,
															y => y.FullName,
															(x, y) => x)
															.OrderBy(x => x.Sequence))
				{
					Log($"Copying {table.TableName} ...");
					var colNames = SchemaHelper.GetTableColumnsAsString(table);
					using (NewDisableTriggerAction(TargetServerConnection, table.FullName))
					{
						TargetServerConnection.ExecuteNonQuery(
$@"INSERT INTO {table.FullName}
(
{colNames}
)
SELECT
{colNames}
FROM {table.SchemaName}.{StagingTableNamePrefix}{table.TableName};");
					}
				}
				OnTargetTablesSynchronised();
			}
		}

		void DeleteTable(string tableFullName)
		{
			for (var i = 1; i <= 100; i++)
			{
				try
				{
					Log($"Deleting {tableFullName} ... ({i})");
					using (NewDisableTriggerAction(TargetServerConnection, tableFullName))
					{
						var sql = $"{(ReferencedTableFullNames.Contains(tableFullName) ? "DELETE" : "TRUNCATE TABLE")} {tableFullName};";
						TargetServerConnection.ExecuteNonQuery(sql);
					}
					return;
				}
				catch (SqlException ex) when (ex.Number == 547) //FK violation
				{
					var pattern = @"table ""(\w\w\w\.[\w\d]+)""";
					var match = Regex.Match(ex.Message, pattern);

					string subTableName;
					if (match.Success)
					{
						subTableName = match.Groups[1].Value;
					}
					else
					{
						throw;
					}

					DeleteTable(subTableName);
				}
			}
			throw new InvalidOperationException($"Failed to delete table named {tableFullName}.");
		}

		static DisposableAction NewDisableTriggerAction(DbConnection targetServerConnection, string tableFullName) =>
			new DisposableAction(() =>
			{
				targetServerConnection.ExecuteNonQuery($"DISABLE TRIGGER ALL ON {tableFullName};");
			},
			() =>
			{
				targetServerConnection.ExecuteNonQuery($"ENABLE TRIGGER ALL ON {tableFullName};");
			});

		static DisposableAction NewDisableCdcAction(DbConnection targetServerConnection)
		{
			var isCdcEnabled = false;
			return new DisposableAction(() =>
			{
				isCdcEnabled = targetServerConnection.ExecuteScalar<int>("SELECT COUNT(1) FROM SYS.Schemas WHERE [Name] = 'CDC';") > 0;
				if (isCdcEnabled)
				{
					targetServerConnection.ExecuteNonQuery("EXEC sys.sp_cdc_disable_db;");
				}
			},
			() =>
			{
				if (isCdcEnabled)
				{
					targetServerConnection.ExecuteNonQuery("EXEC sys.sp_cdc_enable_db;");
				}
			});
		}

		protected virtual DbConnection NewSourceServerConnection() => Db.NewExtraConnectionToMainDb();
		protected abstract DbConnection NewTargetServerConnection();
		protected abstract IEnumerable<DatabaseTable> GetTablesToSync();
		protected virtual string GetStagingTableColumnSelectQuery(TableColumn tableColumn) => tableColumn.ColumnName;
		protected virtual string GetStagingTableAdditionalWhereClause(DatabaseTable table) => "";
		protected virtual void OnSynchronisingTargetTables(List<DatabaseTable> tablesToDelete) { }
		protected virtual void OnTargetTablesSynchronised() { }

		protected void HandleException(Exception ex)
		{
			Log(LogType.Error, ex.Message, ex);
			try
			{
				var mail = new EmailDef();
				mail.Subject = GetType().Name + ' ' + ex.Message;
				mail.Body = ex.ToString();
				Env.OutgoingMailManager.CreateAndSave(mail, EDIDataRegistry.Instance.InternalNotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.InternalNotificationGroup));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}
		}

		ZGlobalMutex CreateAndLockMutex()
		{
			ZGlobalMutex result = null;
			var mutex = new ZGlobalMutex(new MutexID(GetType().FullName, "Ensure only one TableSynchronizer execution"));
			try
			{
				if (mutex.Lock())
				{
					result = mutex;
					mutex = null;
				}
			}
			catch (SqlLockLostException)
			{
			}
			finally
			{
				if (mutex != null)
				{
					try
					{
						((IDisposable)mutex).Dispose();
					}
					catch (Exception ex) when (!ex.IsCriticalException()) { }
				}
			}
 
			return result;
		}

		protected const string StagingTableNamePrefix = "_EBS_Staging_";
		protected void Log(string message) => Log(LogType.Information, message);
		protected void Log(LogType type, string message) => Logger?.Log(type, message);
		protected void Log(LogType type, string message, Exception ex) => Logger?.Log(type, message, ex);
		readonly ILogger Logger;
		readonly IEnumerable<DatabaseTable> TablesToSync;
		readonly IEnumerable<TableDependency> TableDependencies;
		readonly IEnumerable<string> ReferencedTableFullNames;

		DbConnection SourceServerConnection { get; set; }
		protected DbConnection TargetServerConnection { get; private set; }

		protected class DatabaseObject
		{
			public DatabaseObject(string schemaName, string objectName)
			{
				SchemaName = schemaName;
				ObjectName = objectName;
			}

			readonly public string SchemaName;
			readonly public string ObjectName;
			public string FullName => $"{SchemaName}.{ObjectName}";
		}

		protected class DatabaseTable : DatabaseObject
		{
			public DatabaseTable(string tableName)
				: this("dbo", tableName)
			{
			}

			public DatabaseTable(string schemaName, string tableName)
				: base(schemaName, tableName)
			{
			}

			public string TableName => ObjectName;
		}

		protected class TableDependency : DatabaseTable
		{
			public TableDependency(string schemaName, string tableName, int sequence)
				: base(schemaName, tableName)
			{
				Sequence = sequence;
			}

			readonly public int Sequence;
		}

		protected class TableColumn : DatabaseObject
		{
			public TableColumn(
				string schemaName,
				string tableName,
				string columnName,
				int position,
				string dataType,
				int length
				)
				: base(schemaName, columnName)
			{
				TableName = tableName;
				Position = position;
				DataType = dataType;
				Length = length;
			}

			readonly public string TableName;
			public string ColumnName => ObjectName;
			readonly public int Position;
			readonly public string DataType;
			readonly public bool IsNumber;
			readonly public bool IsDate;
			readonly public bool IsString;
			readonly public bool IsBinary;
			readonly public bool IsGuid;
			readonly public int Length;
		}

		protected static class SchemaHelper
		{
			public static IEnumerable<TableDependency> GetTableDependencies()
			{
				var sql = $@"
DECLARE @Dependency TABLE
(
    oType INT,
    oObjName VARCHAR(128),
    oOwner NVARCHAR(128),
    oSequence INT 
);

INSERT INTO @Dependency
EXEC sp_msdependencies @objname = NULL, @objtype = 3; 

SELECT SchemaName = oOwner, TableName = oObjName, Sequence = CAST(ROW_NUMBER() OVER (ORDER BY oSequence, oObjName) AS INT)
FROM @Dependency
ORDER BY oSequence, oObjName;
";
				using (var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql))
				{
					return dataTable.Rows.OfType<DataRow>().Select(x =>
						new TableDependency
						(
							x.Field<string>(nameof(TableDependency.SchemaName)),
							x.Field<string>(nameof(TableDependency.TableName)),
							x.Field<int>(nameof(TableDependency.Sequence))
						)).ToArray();
				}
			}

			public static IEnumerable<DatabaseObject> GetReferencedTables()
			{
				var sql = @"
SELECT DISTINCT SchemaName = s.name, TableName = referenced_table.name
FROM sys.foreign_keys AS fk
JOIN sys.tables AS referenced_table ON fk.referenced_object_id = referenced_table.object_id
JOIN sys.schemas s ON referenced_table.schema_id = s.schema_id;
";
				using (var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql))
				{
					return dataTable.Rows.OfType<DataRow>().Select(x =>
						new DatabaseObject
						(
							x.Field<string>(nameof(TableDependency.SchemaName)),
							x.Field<string>(nameof(TableDependency.TableName))
						)).ToArray();
				}
			}

			public static IEnumerable<TableColumn> GetTableColumns(DatabaseTable table)
			{
				var sql = $@"
SELECT 
SchemaName = C.TABLE_SCHEMA,
TableName = C.TABLE_NAME,
ColumnName = C.COLUMN_NAME,
Position = C.ORDINAL_POSITION,
DataType = C.DATA_TYPE,
Length = ISNULL(C.CHARACTER_MAXIMUM_LENGTH, -1)
FROM INFORMATION_SCHEMA.COLUMNS C
LEFT JOIN sys.computed_columns CC ON CC.name = C.COLUMN_NAME
JOIN INFORMATION_SCHEMA.TABLES T ON 
	T.TABLE_CATALOG	= C.TABLE_CATALOG
	AND T.TABLE_SCHEMA	= C.TABLE_SCHEMA	
	AND T.TABLE_NAME = C.TABLE_NAME	
WHERE CC.name IS NULL
  AND T.TABLE_TYPE = 'BASE TABLE' AND T.TABLE_SCHEMA = '{table.SchemaName}' AND T.TABLE_NAME = '{table.TableName}'
ORDER BY Position;";

				using (var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql))
				{
					if (dataTable.Rows.Count == 0)
					{
						throw new InvalidOperationException($"{table.TableName} DOES NOT exist");
					}

					return dataTable.Rows.OfType<DataRow>().Select(x =>
						new TableColumn
						(
							x.Field<string>(nameof(TableColumn.SchemaName)),
							x.Field<string>(nameof(TableColumn.TableName)),
							x.Field<string>(nameof(TableColumn.ColumnName)),
							x.Field<int>(nameof(TableColumn.Position)),
							x.Field<string>(nameof(TableColumn.DataType)),
							x.Field<int>(nameof(TableColumn.Length))
						)).ToArray();
				}
			}

			public static string GetTableColumnsAsString(DatabaseTable table)
				=> string.Join(",\r\n", GetTableColumns(table).OrderBy(x => x.Position).Select(x => x.ColumnName));
		}
	}
}
