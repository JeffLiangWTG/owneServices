using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	class SharedDatabasePreparationStrategy : StandardRefDbPreparationStrategy, ISharedRefDbStrategy
	{
		public SharedDatabasePreparationStrategy(IUpgradeContext context, string mainDbName, RefDbTypeEnum refDbType, string refDbCountry, DbConnection upgradeConnection, int versionToUpgradTo)
			: base(mainDbName, refDbType, refDbCountry, upgradeConnection)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
			this.versionToUpgradTo = versionToUpgradTo;
		}

		readonly int versionToUpgradTo;

		protected override string GetLatestVersionDatabaseName()
		{
			return string.Format("{0}{1}", VersionSharedRefDbPrefix, versionToUpgradTo.ToString("000000"));
		}

		protected virtual string SharedRefDbPrefix => RefDbTableNameResolver.SharedRefDbPrefix;

		protected virtual string VersionSharedRefDbPrefix
		{
			get { return GetVersionSharedRefDbPrefix(refDbType, refDbCountry); }
		}

		public static string GetVersionSharedRefDbPrefix(RefDbTypeEnum refDbType, string refDbCountry)
		{
			return String.Format(
				"{0}{1}-{2}-{3}-",
				RefDbTableNameResolver.SharedDbPrefix,
				RefDbTableNameResolver.RefDbAffix,
				RefDbTableNameResolver.GetDbType3LetterCode(refDbType),
				refDbCountry);
		}

		protected override void CreateDatabaseIfNotExists()
		{
			while (Db.Connection.RunLocked(string.Format(CultureInfo.InvariantCulture, "CargoWiseOne_SharedDbCreate_{0}", RefDbName), (f) => base.CreateDatabaseIfNotExists(), null, max_tries: 3, Db.SqlMasterDb) != LockedProcessResult.Completed)
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
			}
		}
		/// <summary>
		/// Checks the current reference database version from the synonym base object in the main DB: [CW-RefDb-Ent-ZA-000000].dbo.SomeTable
		/// If it matches the latest version, it means it has already being upgraded => returns this value.
		/// Otherwise it gets the version from the BaseDatabaseForUpgrade, as this is will the base version for the upgrade steps.
		/// </summary>
		protected override int GetVersionFromDatabaseSafe()
		{
			string currentSynonymDb = CurrentDatabaseName;
			int versionFromCurrentSynonymDatabase = GetVersionFromDatabase(currentSynonymDb);

			int version = (versionFromCurrentSynonymDatabase == versionToUpgradTo || currentSynonymDb == BaseDatabaseForUpgrade)
				? versionFromCurrentSynonymDatabase
				: GetVersionFromDatabase(BaseDatabaseForUpgrade);

			return version;
		}

		int GetVersionFromDatabase(string refDbToGetVersionFrom)
		{
			return RefDbTableNameResolver.IsSharedDatabase(refDbToGetVersionFrom)
				? GetSharedDatabaseVersionFromExtendedProperty(mainUpgConnection, refDbToGetVersionFrom)
				: GetVersionFromExtendedPropertyWithFallbackToVersionTable(refDbToGetVersionFrom);
		}

		/// <summary>
		/// Gets the version information which is stored in an extended property in the shared reference database.
		/// When the database is fully upgraded, it should match the version label in the database name [CW-RefDb-Xxx-ZZ-000000].
		/// </summary>
		int GetSharedDatabaseVersionFromExtendedProperty(DbConnection connection, string refDbToGetVersionFrom)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture,
				"SELECT convert(int, value) FROM [{0}].sys.extended_properties WITH (READPAST, READCOMMITTEDLOCK) WHERE class = 0 AND name = '{1}'",
				refDbToGetVersionFrom,
				ReferenceDbUpgrader.VersionPropertyName);

			object objValue = connection.ExecuteScalar(sqlText);
			return (objValue == null || objValue == DBNull.Value) ? 0 : (int)objValue;
		}

		string currentDatabaseName;
		public string CurrentDatabaseName => currentDatabaseName ?? (currentDatabaseName = GetCurrentDatabaseName());

#if DEBUG
		internal void RefreshCurrentDatabaseNameForTesting()
		{
			currentDatabaseName = null;
		}
#endif

		string GetCurrentDatabaseName()
		{
			string objValue = ((IPhysicalRefDbLocation)mainUpgConnection).LoadRefDbNameFromSynonym(mainUpgConnection.CurrentDatabase, refDbType, refDbCountry);
			return objValue ?? CalculatedPrivateRefDbName;
		}

		/// <summary>
		/// Looks for highest version shared database with version smaller than the latest (versionToUpgradTo)
		/// Must check the version in the extended property to ensure the base database is reliable
		/// </summary>
		protected string BaseDatabaseForUpgrade
		{
			get
			{
				if (baseDatabaseForUpgrade_UsePropertyInstead == null)
				{
					var currentDatabaseVersion = GetVersionFromDatabase(CurrentDatabaseName);
					if (currentDatabaseVersion == versionToUpgradTo)
					{
						baseDatabaseForUpgrade_UsePropertyInstead = CurrentDatabaseName;
					}
					else
					{
						var (baseDbName, baseDbVersion) = GetBaseDatabaseForUpgrade(currentDatabaseVersion);

						string candidateName = (baseDbVersion == 0)
							? CurrentDatabaseName
							: baseDbName;

						baseDatabaseForUpgrade_UsePropertyInstead = mainUpgConnection.DatabaseExists(candidateName)
							? candidateName
							: "";
					}
				}

				return baseDatabaseForUpgrade_UsePropertyInstead;
			}
		}

		protected virtual (string DbName, int DbVersion) GetBaseDatabaseForUpgrade(int minimumVersion)
		{
			return GetBaseDatabaseForUpgradeByPrefix(VersionSharedRefDbPrefix, minimumVersion);
		}

		protected (string DbName, int DbVersion) GetBaseDatabaseForUpgradeByPrefix(string sharedDbPrefix, int minimumVersion)
		{
			var sql = Invariant($@"
DECLARE @DbNameAndVersion TABLE (dbname sysname, dbversion int);

INSERT @DbNameAndVersion
	SELECT name, convert(int, substring(name, {sharedDbPrefix.Length + 1}, 10))
		FROM sys.databases
		WHERE state = 0 and name like '{sharedDbPrefix}%'
		AND isnumeric(substring(name, {sharedDbPrefix.Length + 1}, 10)) = 1;

DELETE @DbNameAndVersion WHERE dbversion <= {minimumVersion} OR dbversion >= {versionToUpgradTo};

DECLARE @HighestVersionDb sysname;
DECLARE @HighestVersionNumber int;
DECLARE @DbVersionFromExtPty int;

WHILE (@HighestVersionDb is null AND exists(SELECT null FROM @DbNameAndVersion))
BEGIN
	SELECT TOP (1)
		@HighestVersionDb = dbname,
		@HighestVersionNumber = dbversion
	FROM
		@DbNameAndVersion
	ORDER BY
		dbversion desc

	DELETE @DbNameAndVersion WHERE dbname = @HighestVersionDb;

	DECLARE @Cmd nvarchar(1000) =
		'SELECT @DbVersion = convert(int, value)' +
		' FROM [' + @HighestVersionDb + '].sys.extended_properties WITH (READPAST, READCOMMITTEDLOCK)' +
		' WHERE class = 0 AND name = ''{ReferenceDbUpgrader.VersionPropertyName}''';
	EXEC sp_executesql @Cmd, N'@DbVersion INT OUTPUT', @DbVersionFromExtPty OUTPUT;

	IF (@DbVersionFromExtPty is null OR @DbVersionFromExtPty <> @HighestVersionNumber)
	BEGIN
		SET @HighestVersionDb = NULL
		SET @HighestVersionNumber = NULL
	END
END

SELECT
	HighestVersionDb     = ISNULL(@HighestVersionDb, ''),
	HighestVersionNumber = ISNULL(@HighestVersionNumber, 0)

"
						);

			var highestVersionDb = "";
			var highestVersionNumber = 0;
			mainUpgConnection.ExecuteReader(sql
				, (reader) =>
				{
					highestVersionDb = (string)reader["HighestVersionDb"];
					highestVersionNumber = Convert.ToInt32(reader["HighestVersionNumber"], CultureInfo.InvariantCulture);
				}
				);

			return (highestVersionDb, highestVersionNumber);
		}

		string baseDatabaseForUpgrade_UsePropertyInstead;

		protected override void GetPhysicalDatabaseSettings(out string dataPath, out string logPath, out int? dataSizeMb, out int? logSizeMb, out int? dataGrowthMb, out int? logGrowthMb)
		{
			dataPath = logPath = null;
			dataSizeMb = dataGrowthMb = null;
			logSizeMb = logGrowthMb = null;

			if (!String.IsNullOrWhiteSpace(BaseDatabaseForUpgrade))
			{
				var sql = String.Format(CultureInfo.CurrentCulture, @"
					SELECT
						mf.physical_name AS path,
						CEILING(mf.size/128.0) AS size,
						mf.is_percent_growth AS isPercentGrowth,
						CEILING(mf.growth/128.0) AS growth,
						mf.type AS type
					FROM sys.master_files AS mf
					WHERE DB_NAME(mf.database_id) = '{0}';",
					BaseDatabaseForUpgrade);

				using (var cmd = mainUpgConnection.Command(sql))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						if (dataPath == null && reader.GetByte(reader.GetOrdinal("type")) == 0)
						{
							dataPath = Path.GetDirectoryName(reader.GetString(reader.GetOrdinal("path")));
							dataSizeMb = (int)reader.GetDecimal(reader.GetOrdinal("size"));

							if (!reader.GetBoolean(reader.GetOrdinal("isPercentGrowth")))
							{
								dataGrowthMb = (int)reader.GetDecimal(reader.GetOrdinal("growth"));
							}
						}
						else if (logPath == null && reader.GetByte(reader.GetOrdinal("type")) == 1)
						{
							logPath = Path.GetDirectoryName(reader.GetString(reader.GetOrdinal("path")));
							logSizeMb = (int)reader.GetDecimal(reader.GetOrdinal("size"));

							if (!reader.GetBoolean(reader.GetOrdinal("isPercentGrowth")))
							{
								logGrowthMb = (int)reader.GetDecimal(reader.GetOrdinal("growth"));
							}
						}
					}
				}
			}

			logSizeMb = RefDbLogFileIntialSizeMb;
			logGrowthMb = RefDbLogFileGrowthMb;
		}

		protected virtual void GetDataAndLogPath(string sqlText, out string dataPath, out string logPath, AdminConnection adminConnection)
		{
			dataPath = logPath = null;
			using (var dbCmd = adminConnection.Command(sqlText))
			using (var reader = dbCmd.ExecuteReader())
			{
				while (reader.Read())
				{
					if (String.Equals(reader["name"].ToString(), @"ReferenceDataFile", StringComparison.OrdinalIgnoreCase))
					{
						dataPath = reader["value"].ToString();
					}
					else if (String.Equals(reader["name"].ToString(), @"ReferenceLogFile", StringComparison.OrdinalIgnoreCase))
					{
						logPath = reader["value"].ToString();
					}
				}
			}
		}

		protected virtual void GetPhysicalDatabaseSettingsFromMasterDB(out string dataPath, out string logPath, AdminConnection adminConnection)
		{
			dataPath = logPath = null;
			if (context.IsHosted)
			{
				var sqlText = "SELECT name, value FROM [master].[sys].[extended_properties] WHERE name in ('ReferenceDataFile', 'ReferenceLogFile')";
				GetDataAndLogPath(sqlText, out dataPath, out logPath, adminConnection);
			}
		}

		protected virtual void GetPhysicalDatabaseSettingsFromPreviousRefDB(out string dataPath, out string logPath, AdminConnection adminConnection)
		{
			var sqlText = Invariant($@"
declare @RefDBName as nvarchar(max) = '{VersionSharedRefDbPrefix}%'
select  top 2 case when type = 0 then 'ReferenceDataFile'
when type = 1 then 'ReferenceLogFile' end as name,
left(physical_name,len(physical_name) - charindex('\',reverse(physical_name))) as value
from sys.master_files
where DB_NAME(database_id) like @RefDBName
order by DB_NAME(database_id) desc
");

			GetDataAndLogPath(sqlText, out dataPath, out logPath, adminConnection);
		}

		protected virtual void GetPhysicalDatabaseSettingsFromFirstRefDB(out string dataPath, out string logPath, AdminConnection adminConnection)
		{
			var sqlText = Invariant($@"
declare @RefDBName as nvarchar(max) = '{SharedRefDbPrefix}%'
select  top 2 case when type = 0 then 'ReferenceDataFile'
when type = 1 then 'ReferenceLogFile' end as name,
left(physical_name,len(physical_name) - charindex('\',reverse(physical_name))) as value
from sys.master_files
where DB_NAME(database_id) like @RefDBName
order by DB_NAME(database_id)
");

			GetDataAndLogPath(sqlText, out dataPath, out logPath, adminConnection);
		}

		protected virtual void GetPhysicalDatabaseSettingsFromMainDB(out string dataPath, out string logPath, AdminConnection adminConnection)
		{
			var sqlText = Invariant($@"
select  top 2 case when type = 0 then 'ReferenceDataFile'
when type = 1 then 'ReferenceLogFile' end as name,
left(physical_name,len(physical_name) - charindex('\',reverse(physical_name))) as value
from sys.master_files
where DB_NAME(database_id) = '{Db.DatabaseName}'
order by DB_NAME(database_id) desc
");

			GetDataAndLogPath(sqlText, out dataPath, out logPath, adminConnection);
		}

		protected override void DoPrepareAndUpgradeDatabase()
		{
			UpgradeNewSharedDatabaseIfRequired();
		}

		#region Latest Version Shared Database Status

		/// <summary>
		/// If new shared database is not fully upgraded => upgrade it.
		/// </summary>
		void UpgradeNewSharedDatabaseIfRequired()
		{
			if (IsNewSharedRefDbPendingUpgrade(mainUpgConnection))
			{
				try
				{
					AcquireLockAndUpgradeNewSharedDatabase();
				}
				catch (FailedToAcquireSharedRefDbUpgradeLockException)
				{
					// Back to wait and check if new DB is fully upgraded
					System.Threading.Thread.Sleep(1000);
					UpgradeNewSharedDatabaseIfRequired();
				}
			}
		}

		bool IsNewSharedRefDbPendingUpgrade(DbConnection connection)
		{
			return (GetSharedDatabaseVersionFromExtendedProperty(connection, RefDbName) != versionToUpgradTo);
		}

		#endregion

		#region Acquire Shared DB Upgrade Lock

		/// <summary>
		/// Uses a new connection so it does not hold a lock to the shared database until the end of the upgrade.
		/// </summary>
		void AcquireLockAndUpgradeNewSharedDatabase()
		{
			using (var sharedDbUpgradeCnx = Db.NewAdminConnection(mainUpgConnection.ServerName, RefDbName))
			{
				sharedDbUpgradeCnx.DefaultCommandTimeOutInSeconds = DbCommand.Timeout.Infinite;

				using (var manager = sharedDbUpgradeCnx.BeginTransactionWithManager())
				{
					DoAcquireLockAndUpgradeNewSharedDatabase(sharedDbUpgradeCnx);
					manager.CommitTransaction();
				}
				createSnapshot?.Dispose();
			}
		}
		IDisposable createSnapshot;

		void DoAcquireLockAndUpgradeNewSharedDatabase(AdminConnection sharedDbUpgradeCnx)
		{
			if (ReferenceFileUpdateMutex.AcquireUpdateLock(sharedDbUpgradeCnx, TimeSpan.Zero, RefDbName, RefDbName))
			{
				if (IsNewSharedRefDbPendingUpgrade(sharedDbUpgradeCnx))
				{
					UpgradeNewSharedDatabase(sharedDbUpgradeCnx);
				}

				UpgradeStmExtendedPropertyTableIfNeeded(sharedDbUpgradeCnx);
			}
			else
			{
				// Back to wait and check if new DB is fully upgraded
				throw new FailedToAcquireSharedRefDbUpgradeLockException();
			}
		}

		void UpgradeStmExtendedPropertyTableIfNeeded(DbConnection connection)
		{
			ExtProperty.UpgradeDatabase(connection);
		}

		#endregion

		#region Perform Upgrade

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Not using ZorKArchitectures")]
		void UpgradeNewSharedDatabase(AdminConnection sharedDbUpgradeCnx)
		{
			if (!string.IsNullOrWhiteSpace(BaseDatabaseForUpgrade))
			{
				var snapshotManager = new SharedRefDbSnapshotManager(BaseDatabaseForUpgrade, DateTime.Now);// Not using ZorKArchitectures
				createSnapshot = snapshotManager.Create(sharedDbUpgradeCnx);
				OnSnapshotCreated?.Invoke(sharedDbUpgradeCnx);
				CopyObjectsFromPreviousToNewDatabase(sharedDbUpgradeCnx, snapshotManager.SnapshotRefDb);
			}

			performUpgradeTasks(sharedDbUpgradeCnx);
		}

		public event Action<AdminConnection> OnSnapshotCreated;

		void CopyObjectsFromPreviousToNewDatabase(AdminConnection sharedDbUpgradeCnx, string sourceDb)
		{
			DbSchemaChange.DropAllTables(sharedDbUpgradeCnx);
			DbSchemaChange.CopyAllTablesAndData(sharedDbUpgradeCnx, sourceDb);
			DbSchemaChange.EnsureCorrectColumnCollation(sharedDbUpgradeCnx);
			DbSchemaChange.CopyDatabaseExtendedProperties(sharedDbUpgradeCnx, sourceDb);

			var schemaSynchroniser = new Schema.SchemaSynchroniser(
				sharedDbUpgradeCnx,
				new SharedRefDbUpgradeLogger(),
				RefDbName,
				sourceDb);
			schemaSynchroniser.SynchroniseDatabaseSchema();
			schemaSynchroniser.CreateAndValidateCheckConstraints();

			if (!RefDbTableNameResolver.IsSharedDatabase(sourceDb))
			{
				// Drops legacy version table in case copied from an old exclusive database
				DropLegacyVersionTableIfExists(sharedDbUpgradeCnx);
			}
		}

		#endregion

		#region Logging and Exception Classes

		[Serializable]
		internal class FailedToAcquireSharedRefDbUpgradeLockException : ApplicationException
		{
			public FailedToAcquireSharedRefDbUpgradeLockException()
			{
			}

#if NETFRAMEWORK
			protected FailedToAcquireSharedRefDbUpgradeLockException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion

		readonly IUpgradeContext context;
	}
}
