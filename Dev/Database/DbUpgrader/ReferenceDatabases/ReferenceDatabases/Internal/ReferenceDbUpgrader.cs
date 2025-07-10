using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	abstract class ReferenceDbUpgrader
	{
		public ReferenceDbUpgrader(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
		{
			this.upgradeContext = upgradeContext ?? throw new ArgumentNullException(nameof(upgradeContext));
			this.upgradeConnection = upgradeConnection;
			this.logger = logger;
#if DEBUG
			DbPreparationStrategyForTest = null;
#endif
		}

		public void CreateAndUpgradeIfRequired()
		{
			if (ShouldUpgrade(VersionBeforeUpgrade))
			{
				DbPreparationStrategy.PrepareAndUpgradeDatabase(c => UpgradeDatabase(c, VersionBeforeUpgrade));
				CopyDataFromVersionBeforeUpgrade(VersionBeforeUpgrade);
				versionBeforeUpgradeCached = null;
			}

			SynchroniseSynonyms();
		}

		void CopyDataFromVersionBeforeUpgrade(int versionBeforeUpgrade)
		{
			if (EnableCopyingOfPreviousData && DbPreparationStrategy is ISharedRefDbStrategy strategy)
			{
				var previousRefDbName = strategy.CurrentDatabaseName;

				if (!string.IsNullOrEmpty(previousRefDbName) && upgradeConnection.DatabaseExists(previousRefDbName))
				{
					var currentRefDbName = DbPreparationStrategy.RefDbName;
					if (!string.IsNullOrEmpty(currentRefDbName) && upgradeConnection.DatabaseExists(currentRefDbName))
					{
						CopyDataFromVersionBeforeUpgradeCore(versionBeforeUpgrade, previousRefDbName, currentRefDbName);
					}
				}
			}
		}

		protected void AcquireUpdateLockForSharedDatabaseAndCopyData(string refDbName, string lockType, Action copyData)
		{
			while (!HasAcquiredUpdateLockForSharedDatabaseAndCopyDataCore(refDbName, lockType, copyData))
			{
			}
		}

		bool HasAcquiredUpdateLockForSharedDatabaseAndCopyDataCore(string refDbName, string lockType, Action copyData)
		{
			var result = false;
			using (var lockRefDbConnetion = Db.NewAdminConnection(refDbName))
			{
				lockRefDbConnetion.DefaultCommandTimeOutInSeconds = DbCommand.Timeout.Infinite;
				using (var transactionManager = lockRefDbConnetion.BeginTransactionWithManager())
				{
					if (ReferenceFileUpdateMutex.CanAquireLock(lockRefDbConnetion, refDbName, refDbName) && // check to ensure the db is not locked for upgrade
						ReferenceFileUpdateMutex.AcquireUpdateLock(lockRefDbConnetion, TimeSpan.Zero, refDbName, lockType))
					{
						copyData();
						result = true;
					}
				}
			}
			if (!result)
			{
				System.Threading.Thread.Sleep(10000);
			}
			return result;
		}

		protected static List<string> GetColumnNames(string currentRefDbName, string exclusiveRefDbName, DbConnection conn, string tableName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"SELECT ISNULL(String_Agg(name, ', '), '') AS ColumnNames
FROM [{0}].sys.columns
WHERE object_id = OBJECT_ID('[{0}].[].[{2}]', 'U')
AND name IN (SELECT name
			FROM [{1}].sys.columns
			WHERE object_id = OBJECT_ID('[{1}].[].[{2}]', 'U'))", currentRefDbName, exclusiveRefDbName, tableName);

			return new List<string>(((string)conn.ExecuteScalar(sqlText)).Replace(", ", "|").Split('|'));
		}

		protected virtual void CopyDataFromVersionBeforeUpgradeCore(int versionBeforeUpgrade, string previousRefDbName, string currentRefDbName)
		{
			throw new NotImplementedException();
		}

		protected virtual bool EnableCopyingOfPreviousData
		{ get { return false; } }

		bool ShouldUpgrade(int baseVersion)
		{
			bool result =
				(baseVersion < LatestVersion)
				|| (
					RefDbTableNameResolver.IsSharedDatabase(DbName)
					&& (
						!IsCurrentlyUsingASharedDbForThisReferenceDatabase()
						|| !ReferenceDatabaseExists()
					)
				);
			return result;
		}

		bool ReferenceDatabaseExists()
		{
			return upgradeConnection.DatabaseExists(DbName);
		}

		protected delegate void VersionDataUpgrade();

		protected void RunUpgradeStep(int currentVersion, int stepVersion, VersionDataUpgrade upgradeMethod)
		{
			if (currentVersion < stepVersion && stepVersion <= LatestVersion)
			{
				upgradeMethod();
			}
		}

		/// <summary>
		/// Receives a database connection because in the case of shared reference databases
		/// the upgrade is done with a separate connection, not the main database upgrade connection.
		/// </summary>
		/// <param name="conn">Specific reference database upgrade connection</param>
		/// <param name="versionBeforeUpgrade">version of the database before the upgrade</param>
		protected
#if DEBUG
		virtual
#endif
		void UpgradeDatabase(DbConnection conn, int versionBeforeUpgrade)
		{
			DoDataUpgrade(conn, versionBeforeUpgrade);
			EnsureColumnSchemaRequirements(conn);
			UpdateVersionToLatest(conn);
		}

		/// <summary>
		/// Receives a database connection because in the case of shared reference databases
		/// the upgrade is done with a separate connection, not the main database upgrade connection.
		/// </summary>
		/// <param name="conn">Specific reference database upgrade connection</param>
		protected void UpdateVersionToLatest(DbConnection conn)
		{
			DataUtils.SaveDbExtendedProperty(conn, VersionPropertyName, LatestVersion.ToString(), DbName);
		}

		public string DbName
		{
			get { return DbPreparationStrategy.RefDbName; }
		}

		public string ExclusiveRefDbNameSuffix => RefDbTableNameResolver.GetExclusiveRefDbNameSuffix(DatabaseType, CountryCode);

		internal IRefDbPreparationStrategy DbPreparationStrategy
		{
			get
			{
				return dbPreparationStrategy ?? (dbPreparationStrategy = GetNewDbPreparationStrategy());
			}
		}
		protected IRefDbPreparationStrategy dbPreparationStrategy;

#if DEBUG
		internal Func<string, RefDbTypeEnum, string, DbConnection, IRefDbPreparationStrategy> DbPreparationStrategyForTest { get; set; }
#endif

		IRefDbPreparationStrategy GetNewDbPreparationStrategy()
		{
#if DEBUG
			var testDbPreparationStrategy = DbPreparationStrategyForTest?.Invoke(Db.DatabaseName, DatabaseType, CountryCode, upgradeConnection);
			if (testDbPreparationStrategy != null)
			{
				return testDbPreparationStrategy;
			}
#endif
			var ag = AvailabilityGroupInfo.GetFirstGroupInfoOnServer((AdminConnection)upgradeConnection);
			var serverEnabledAOAG = ag.GroupId != Guid.Empty;

			if (IsUsingVersionSharedReferenceDatabases && SupportsVersionSharedDatabase)
			{
				if (serverEnabledAOAG)
				{
					return new SharedAvailabilityGroupRefDbPreparationStrategy(upgradeContext, (AdminConnection)upgradeConnection, Db.DatabaseName, DatabaseType, CountryCode, LatestVersion, ag.GroupName);
				}

				return new SharedDatabasePreparationStrategy(upgradeContext, Db.DatabaseName, DatabaseType, CountryCode, upgradeConnection, LatestVersion);
			}

			return new StandardRefDbPreparationStrategy(Db.DatabaseName, DatabaseType, CountryCode, upgradeConnection);
		}

		internal int VersionBeforeUpgrade
		{
			get
			{
				if (versionBeforeUpgradeCached == null)
				{
					versionBeforeUpgradeCached = DbPreparationStrategy.GetVersionFromDatabase();
				}
				return versionBeforeUpgradeCached.Value;
			}
		}
		int? versionBeforeUpgradeCached;

		protected DbConnection upgradeConnection;
		protected readonly IUpgradeTaskWorkflowLogger logger;
		protected readonly IUpgradeContext upgradeContext;

		public abstract string ReferenceName { get; }
		public abstract RefDbTypeEnum DatabaseType { get; }
		public abstract string CountryCode { get; }
		public abstract int LatestVersion { get; }
		protected abstract void DoDataUpgrade(DbConnection conn, int versionBeforeUpgrade);

		internal const string VersionPropertyName = "RefDbVersion";

		internal protected virtual IEnumerable<ITableScript> Tables => Enumerable.Empty<ITableScript>();

		protected void CreateIndexesIfNotExist(DbConnection connection)
		{
			foreach (var (script, index) in from ITableScript script in Tables
											from IndexScript index in script.CreateIndexScripts
											select (script, index))
			{
				CreateIndexIfNotExists(connection, script.TableName, index.IndexName, index.CreateIndexScript);
			}
		}

		#region Version Shared Databases

		internal virtual bool SupportsVersionSharedDatabase
		{
			get { return true; }
		}

		#endregion

		#region Synonyms

		internal void SynchroniseSynonyms()
		{
			string sqlText = SynchroniseSynonymScript;
			var batchRunner = new BatchRunner();

			try
			{
				batchRunner.RunCommandsGeneratedByQuery(upgradeConnection, sqlText);
			}
			catch (SqlException ex)
			{
				if (HandleLoginNotMappedToSharedDatabaseError(ex))
				{
					batchRunner.RunCommandsGeneratedByQuery(upgradeConnection, sqlText);
				}
				else
				{
					throw;
				}
			}
		}

		string SynchroniseSynonymScript
		{
			get
			{
				return string.Format(@"
					-- Drop synonyms pointing to an invalid object
					SELECT 'DROP SYNONYM [' + sn.name + '];'
						FROM sys.synonyms sn
						WHERE sn.name like '{0}%'
						AND
						(
							sn.base_object_name not like '[[]{1}].%.[[]' + replace(sn.name, '{0}', '') + ']'
							OR NOT EXISTS
							(
								SELECT NULL
								FROM [{1}].sys.objects o
								INNER JOIN sys.schemas sc ON sc.schema_id = o.schema_id
								WHERE o.type in ('U', 'V', 'P', 'IF', 'TF', 'FN')
								AND o.name = replace(replace(sn.base_object_name, '[{1}].[dbo].[', ''), ']', '')
							)
						)
					UNION ALL
					-- Create synonyms for supported objects which do not have one
					SELECT 'CREATE SYNONYM [{0}' + o.name + '] FOR [{1}].[' + sc.name + '].[' + o.name + '];'
						FROM [{1}].sys.objects o
						INNER JOIN sys.schemas sc ON sc.schema_id = o.schema_id
						LEFT JOIN sys.synonyms sn ON sn.base_object_name like '[[]{1}].%.[[]' + o.name + ']'
						WHERE o.type in ('U', 'V', 'P', 'IF', 'TF', 'FN')
						AND o.name not in ('RefDbVersion', 'StmData', 'SchemaVersion')
						AND sn.name is null
					ORDER BY 1 DESC",
					SynonymPrefix,
					DbName);
			}
		}

		/// <summary>
		/// It requires special handling here when it is changing the referenced database used (from exclusive to shared or from an older version of shared to a new one).
		/// The reason for that is because the new database is not yet in the list of databases: DbConnection.GetAllDatabases() - the old one is instead -
		/// and the low level handling only fixes the user mapping for databases in the list for safety reasons (to ensure it stays within the client scope).
		/// </summary>
		bool HandleLoginNotMappedToSharedDatabaseError(SqlException ex)
		{
			if (
				RefDbTableNameResolver.IsSharedDatabase(DbName)
				&& new DbErrorMatch(ex).ExceptionType == DbErrorType.LoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext)
			{
				using (var adminConnection = Db.NewAdminConnection(upgradeConnection.ServerName, ((ICurrentDbControl)upgradeConnection).InitialDatabase))
				using (((ICurrentDbControl)adminConnection).UseDatabase(DbName))
				{
					((IDbLoginRepair)adminConnection).EnsureDbLoginsHaveRightsToCurrentDatabase();
				}

				return true;
			}

			return false;
		}

		public string SynonymPrefix
		{
			get { return RefDbTableNameResolver.GetRefDbSynonymPrefix(DatabaseType, CountryCode); }
		}

		bool IsUsingVersionSharedReferenceDatabases
		{
			get
			{
				if (isUsingVersionSharedReferenceDatabases == null)
				{
					isUsingVersionSharedReferenceDatabases =
						DoesDbHaveSynonymsToSharedDatabases(upgradeConnection)
						|| RefDbTableNameResolver.ShouldUseSharedDatabases(upgradeConnection);
				}

				return isUsingVersionSharedReferenceDatabases.Value;
			}
		}

		bool? isUsingVersionSharedReferenceDatabases;

#if DEBUG
		internal
#endif
		static bool DoesDbHaveSynonymsToSharedDatabases(DbConnection connection)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				IF exists(SELECT null FROM sys.synonyms	WHERE name like '{0}%' AND (base_object_name like '[[]{1}{0}-___-__-%].%' OR base_object_name like '[[]{2}%].%'))
					SELECT 1;
				ELSE
					SELECT 0;",
				RefDbTableNameResolver.RefDbAffix,
				RefDbTableNameResolver.SharedDbPrefix,
				RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix);
			return (Convert.ToInt32(connection.ExecuteScalar(sqlText)) == 1);
		}

		bool IsCurrentlyUsingASharedDbForThisReferenceDatabase()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				IF exists(SELECT null FROM sys.synonyms	WHERE name like '{0}%' AND (base_object_name like '[[]{1}%].%' OR base_object_name like '[[]{2}%].%'))
					SELECT 1;
				ELSE
					SELECT 0;",
				SynonymPrefix,
				SharedDatabasePreparationStrategy.GetVersionSharedRefDbPrefix(DatabaseType, CountryCode),
				RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix);
			return (Convert.ToInt32(upgradeConnection.ExecuteScalar(sqlText)) == 1);
		}

		#endregion

		#region Upgrade Helper Methods

		#region DDL - schema changes

		protected void EnsureColumnSchemaRequirements(DbConnection connection)
		{
			FixColumnCollation(connection);
			FixDefaultConstraintsOnColumnMismatch(connection);
			AddDefaultConstraints(connection);
			FormatNonDeterminsticDefaultConstraintNames(connection);
		}

		void FixColumnCollation(DbConnection connection)
		{
			try
			{
				DbSchemaChange.EnsureCorrectColumnCollation(connection);
			}
			catch (SqlException ex)
			{
				throw new SchemaChangeException("Attempt to fix column collation failed.\r\n\tThis attempt does not cater for dependent objects.\r\n\tIn such cases a manual data fix is required.", ex);
			}
		}

		void FixDefaultConstraintsOnColumnMismatch(DbConnection connection)
		{
			const string sqlText = @"
				DECLARE @SqlText nvarchar(max);

				SELECT @SqlText = ISNULL(@SqlText, '')
					  + 'ALTER TABLE [dbo].[' + tab.name + '] DROP CONSTRAINT [' + def.name + '];'
				FROM
					  sys.tables tab
					  INNER JOIN sys.columns col ON col.object_id = tab.object_id
					  INNER JOIN sys.types typ ON typ.system_type_id = col.system_type_id
					  INNER JOIN sys.default_constraints def
							ON def.name = 'DF_' + tab.name + '_' + col.name
							AND (def.parent_object_id <> tab.object_id OR def.parent_column_id <> col.column_id)
				WHERE
					  tab.is_ms_shipped = 0
				;

				IF (@SqlText is not null) EXEC (@SqlText);
				";

			connection.ExecuteNonQuery(sqlText);
		}

		void AddDefaultConstraints(DbConnection connection)
		{
			const string sqlText = @"
				DECLARE @SqlText nvarchar(max);

				SELECT @SqlText = ISNULL(@SqlText, '')
					+ 'ALTER TABLE dbo.' + tab.name
					+ ' ADD CONSTRAINT DF_' + tab.name + '_' + col.name
					+ ' DEFAULT {0} FOR ' + col.name + ';'
				FROM
					sys.tables tab
					INNER JOIN sys.columns col ON col.object_id = tab.object_id
					INNER JOIN sys.types typ ON typ.system_type_id = col.system_type_id
					LEFT JOIN sys.default_constraints def ON def.parent_object_id = tab.object_id AND def.parent_column_id = col.column_id
				WHERE
					tab.is_ms_shipped = 0
					AND (col.name LIKE '__[_]%' OR col.name LIKE '___[_]%')
					AND col.is_nullable = 0
					AND col.is_identity = 0
					AND col.is_computed = 0
					AND typ.name in ({1})
					AND def.object_id is null;

				IF (@SqlText is not null) EXEC (@SqlText);
				";

			connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, sqlText, "''''", "'char', 'varchar', 'nvarchar', 'nchar'"));
			connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, sqlText, "0", "'int', 'decimal', 'tinyint', 'smallint'"));
		}

		internal protected void CreateTableIfNotExists(DbConnection conn, string tableName, string createScript)
		{
			string sqlText = DbSchemaChange.GetCreateTableIfNotExistsScript(tableName, createScript);
			conn.ExecuteNonQuery(sqlText);
		}

		internal protected void CreateIndexIfNotExists(DbConnection conn, string tableName, string indexName, string createScript)
		{
			var sqlText = DbSchemaChange.GetCreateIndexIfNotExistsScript(tableName, indexName, createScript);
			conn.ExecuteNonQuery(sqlText);
		}

		internal protected void CreateTriggerIfNotExists(DbConnection conn, string tableName, string triggerName, string createScript)
		{
			string checkIfExistsSql = $@"
				IF NOT EXISTS(
					SELECT * FROM sys.triggers _trigger 
					INNER JOIN sys.tables _table ON _table.object_id = _trigger.parent_id
					WHERE _table.name = '{tableName}' AND _trigger.name = '{triggerName}')
				BEGIN
					SELECT 1
				END
				ELSE 
				BEGIN
					SELECT 0
				END";

			if (conn.ExecuteScalar<int>(checkIfExistsSql) == 1)
			{
				conn.ExecuteNonQuery(createScript);
			}
		}

		internal protected void CreateTables(DbConnection connection, ITableScript[] scripts)
		{
			foreach (ITableScript script in scripts)
			{
				CreateTable(connection, script);
			}
		}

		internal protected void CreateTable(DbConnection connection, ITableScript script)
		{
			DbObjectCreator.CreateTableIfNotExists(connection, script.TableName, script.CreateTableScript);
			foreach (IndexScript index in script.CreateIndexScripts)
			{
				CreateIndexIfNotExists(connection, script.TableName, index.IndexName, index.CreateIndexScript);
			}
		}

		protected void UpgradeBlobToVarMaxDataType(DbConnection conn)
		{
			var sqlText = @"
				SELECT
					isnull('ALTER TABLE [' + tab.name + '] DROP CONSTRAINT [' + def.name + ']; ', '') +
					'ALTER TABLE [' + tab.name + '] ALTER COLUMN [' + col.name + '] ' +
					CASE typ.name WHEN 'TEXT' THEN 'VARCHAR' WHEN 'NTEXT' THEN 'NVARCHAR' WHEN 'IMAGE' THEN 'VARBINARY' END +
					'(max) ' + CASE col.is_nullable WHEN 0 THEN 'NOT ' ELSE '' END + 'NULL;' +
					isnull(' ALTER TABLE [' + tab.name + '] ADD CONSTRAINT [' + def.name + '] DEFAULT ' + def.definition + ' FOR [' + col.name + '];', '')
				FROM
					sys.tables tab
					INNER JOIN sys.columns col ON col.object_id = tab.object_id
					INNER JOIN sys.types typ ON typ.system_type_id = col.system_type_id
					LEFT JOIN sys.default_constraints def ON def.parent_object_id = col.object_id AND def.object_id = col.default_object_id
				WHERE
					tab.is_ms_shipped = 0
					AND typ.name in ('TEXT', 'NTEXT', 'IMAGE');
				";
			new BatchRunner().RunCommandsGeneratedByQuery(conn, sqlText);
		}

		#endregion

		#region Data Load

		internal protected void ClearAndRePopulateTables(DbConnection connection, IPopulateData[] scripts, PreprocessLine preprocessFunc = null)
		{
			ClearAndRePopulateTables(connection, ',', scripts, preprocessFunc);
		}

		protected void ClearAndRePopulateTables(DbConnection connection, char delimiter, IPopulateData[] scripts, PreprocessLine preprocessFunc = null)
		{
			foreach (IPopulateData data in scripts)
			{
				PopulateTableFromCsvFile(connection, delimiter, data.CsvFileName, data.TableName, data.ColumnSqlList, data.ColumnTypes, true, null, false, preprocessFunc);
			}
		}

		protected void RePopulateTables(DbConnection connection, char delimiter, IPopulateData[] scripts, bool clearExistingData = false, bool ignoreDuplicates = true)
		{
			foreach (IPopulateData data in scripts)
			{
				RePopulateTable(connection, delimiter, data, clearExistingData, ignoreDuplicates);
			}
		}

		protected void RePopulateTable(DbConnection connection, char delimiter, IPopulateData data, bool clearExistingData = false, bool ignoreDuplicates = true)
		{
			PopulateTableFromCsvFile(connection, delimiter, data.CsvFileName, data.TableName, data.ColumnSqlList, data.ColumnTypes, clearExistingData, ignoreDuplicates: ignoreDuplicates);
		}

		public delegate string PreprocessLine(string line);

		protected void PopulateTableFromCsvFile(DbConnection connection, char delimiter, string csvFileName, string tableName, string columnList, IReadOnlyList<SqlDbType> columnTypes, bool clearExistingData = true, string multiLineFieldDelimiter = null, bool ignoreDuplicates = false, PreprocessLine preprocessFunc = null)
		{
			var columns = columnList.Split(',');
			var parameterList = columnList.Replace(columnList.Substring(0, 3), "@");
			var parameters = parameterList.Split(',');
			if (parameters.Length != columnTypes.Count)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Count of {0} table columns ({1}) and count of column type list ({2}) don't match", tableName, parameters.Length, columnTypes.Count));
			}

			Stream csvStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(CsvFileStoragePath + csvFileName);
			if (csvStream != null)
			{
				using (var reader = new StreamReader(csvStream))
				{
					if (clearExistingData)
					{
						connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "DELETE {0}", tableName));
					}

					int count = 0;
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						if (preprocessFunc != null)
						{
							line = preprocessFunc(line);
						}
						line = line.TrimEnd();
						var csvLine = new OCsvLine(line, delimiter);
						count++;
						string sqlText;
						if (csvLine.FieldValues.Length == parameters.Length)
						{
							if (ignoreDuplicates)
							{
								sqlText = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS(
   SELECT 1 FROM {0} WHERE {3} = {4})
BEGIN
   INSERT {0} ({1}) VALUES ({2})
END", tableName, columnList, parameterList, columns[0], parameters[0]);
							}
							else
							{
								sqlText = string.Format(CultureInfo.InvariantCulture, "INSERT {0} ({1}) VALUES ({2})", tableName, columnList, parameterList);
							}
							DbCommand command = connection.Command(sqlText);

							for (int i = 0; i < parameters.Length; i++)
							{
								string fieldValue = csvLine.FieldValues[i];

								if (columnTypes[i] == SqlDbType.SmallDateTime)
								{
									if (fieldValue.Length == 8)
									{
										fieldValue = fieldValue.Substring(0, 4) + "-" + fieldValue.Substring(4, 2) + "-" + fieldValue.Substring(6, 2) + " 12:00:00";
									}
									else if (fieldValue.Length == 14)
									{
										fieldValue = fieldValue.Substring(0, 4) + "-" + fieldValue.Substring(4, 2) + "-" + fieldValue.Substring(6, 2) + " " + fieldValue.Substring(8, 2) + ":" + fieldValue.Substring(10, 2) + ":" + fieldValue.Substring(12, 2);
									}
								}

								if (columnTypes[i] == SqlDbType.Decimal)
								{
									command.AddParameter(parameters[i].Trim(), columnTypes[i], decimal.Parse(fieldValue, CultureInfo.InvariantCulture));
								}
								else if (columnTypes[i] == SqlDbType.UniqueIdentifier)
								{
									object guidValue = string.IsNullOrWhiteSpace(fieldValue) ? DBNull.Value : new Guid(fieldValue);
									command.AddParameter(parameters[i].Trim(), columnTypes[i], guidValue);
								}
								else if (columnTypes[i] == SqlDbType.Bit)
								{
									AddBitParameterToCommand(command, parameters[i], fieldValue);
								}
								else
								{
									if (multiLineFieldDelimiter != null)
									{
										fieldValue = fieldValue.Replace(multiLineFieldDelimiter, "\r\n");
									}
									var type = columnTypes[i];
									command.AddParameter(parameters[i].Trim(), type, fieldValue);
								}
							}
							command.ExecuteNonQuery();
						}
#if DEBUG
						else
						{
							throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid number of elements in line {0} of table {1}, was {2}, but was expecting {3}", count, tableName, csvLine.FieldValues.Length, parameters.Length));
						}
#endif
					}
				}
#if DEBUG
				CsvFilePopulationStatisticAction?.Invoke(csvFileName);
#endif
			}
			else
			{
				throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Cannot find source file '{0}'", csvFileName));
			}
		}
#if DEBUG
		internal Action<string> CsvFilePopulationStatisticAction;
#endif

		protected virtual void AddBitParameterToCommand(DbCommand command, string parameter, string fieldValue)
		{
		}

		protected virtual string CsvFileStoragePath
		{
			get { return "Enterprise.DbUpgrader.ReferenceDatabases."; }
		}

		#endregion

		protected void FormatNonDeterminsticDefaultConstraintNames(DbConnection connection)
		{
			string sqlText = $@"
SELECT  dc.name as ConstraintName, t.name as TableName, c.name as ColumnName
FROM
	sys.default_constraints dc
	INNER JOIN sys.tables t ON t.object_id = dc.parent_object_id
	INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
	INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE
	t.is_ms_shipped = 0
	AND dc.name <> 'DF_' + t.name + '_' + c.name
";
			var renameSqlBuilder = new StringBuilder();

			connection.ExecuteReader(sqlText, (reader) =>
			{
				renameSqlBuilder.AppendLine($"EXEC sys.sp_rename '{reader["ConstraintName"]}', 'DF_{reader["TableName"]}_{reader["ColumnName"]}', 'OBJECT';");
			});
			if (renameSqlBuilder.Length > 0)
			{
				connection.ExecuteNonQuery(renameSqlBuilder.ToString());
			}
		}

		#endregion
	}
}
