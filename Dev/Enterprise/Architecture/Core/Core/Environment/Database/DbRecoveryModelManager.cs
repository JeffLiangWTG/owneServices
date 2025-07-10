using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using WTG.StaticAnalysis.Annotation;
using static CargoWise.Data.RefDbTableNameResolver;

namespace Enterprise.ZArchitecture.Environment
{
	class DbRecoveryModelManager : IDbRecoveryModelManagerInternals
	{
		public void AdjustDatabase(DbConnection connection, string mainDbName, string databaseName, string tempDbNameWhileCreating = null)
		{
			using (UseMainDbOrProvideDefaultValues(connection, mainDbName))
			{
				_ = AdjustDatabaseCore(connection, mainDbName, databaseName, tempDbNameWhileCreating);
			}
		}

		public string[] AdjustAllDatabases(DbConnection connection, string mainDbName)
		{
			var changedRecoveryModels = new List<string>();

			using (UseMainDbOrProvideDefaultValues(connection, mainDbName))
			{
				foreach (var databaseName in GetAllDatabases(connection.ServerName, mainDbName))
				{
					var (oldValue, newValue) = AdjustDatabaseCore(connection, mainDbName, databaseName);
					if (oldValue != newValue)
					{
						changedRecoveryModels.Add($"Recovery model of database {databaseName} was changed from {oldValue} to {newValue}.");
					}
				}
			}

			return changedRecoveryModels.ToArray();
		}

		public DbRecoveryModel GetActual(DbConnection connection, string databaseName)
		{
			using (connection.UseMasterDb())
			{
				var recoveryModel = connection.ExecuteScalar(
					"SELECT recovery_model FROM sys.databases WHERE name = @DbName"
					, cmd =>
					{
						cmd.AddParameter("@DbName", System.Data.SqlDbType.NVarChar, 128, databaseName);
					})
					?? throw new KeyNotFoundException($"Database does not exist: [{databaseName}]");

				return DbRecoveryModel.Get(Convert.ToInt32(recoveryModel));
			}
		}

		public DbRecoveryModel? GetDesired(DbConnection connection, string mainDbName, string databaseName)
		{
			using (UseMainDbOrProvideDefaultValues(connection, mainDbName))
			{
				return GetDesiredCore(connection, mainDbName, databaseName);
			}
		}

		IDisposable UseMainDbOrProvideDefaultValues(DbConnection connection, string mainDbName)
		{
			if (connection.DatabaseExists(mainDbName))
			{
				return ((ICurrentDbControl)connection).UseDatabase(mainDbName);
			}

			var currentDatabase = connection.CurrentDatabase;
			return CacheTemporaryDbValues(currentDatabase, new CachedValues(currentDatabase, DbRecoveryModel.Full, DefaultSingleRefDbName));
		}

		DbRecoveryModel? GetDesiredCore(DbConnection connection, string mainDbName, string databaseName)
		{
			var dbType = GetDbType(connection, mainDbName, databaseName);
			switch (dbType)
			{
				case DbType.Unknown:
					return DbRecoveryModel.Simple;
				case DbType.Main:
				case DbType.SD:
				case DbType.Audit:
				case DbType.EDW:
				case DbType.UserRepository:
				case DbType.ExclusiveRefDb:
				case DbType.SharedRefDb:
				case DbType.SharedAvailabilityGroupRefDb:
					if (Globals.IsDebugMode && !Globals.IsTest)
					{
						// Developer PCs
						return null;
					}

					if (IsAlwaysOnEnabled(connection, mainDbName, databaseName))
					{
						return DbRecoveryModel.Full;
					}

					if (Globals.IsTest)
					{
						return GetRegistryValue(connection);
					}

					if (IsWiseTechGlobalDatabaseServer(connection))
					{
						return IsSandEnvironment(connection) ? GetRegistryValue(connection) : DbRecoveryModel.Full;
					}

					// Self-Hosted
					return IsProductionSystem ? DbRecoveryModel.Full : GetRegistryValue(connection);

				case DbType.SingleSharedRefDb:
					return DbRecoveryModel.Simple;

				default:
					throw new ArgumentOutOfRangeException(nameof(databaseName));
			}
		}

		(DbRecoveryModel? oldValue, DbRecoveryModel? newValue) AdjustDatabaseCore(DbConnection connection, string mainDbName, string databaseName, string tempDbNameWhileCreating = null)
		{
			var desired = GetDesiredCore(connection, mainDbName, databaseName);
			if (desired == null)
			{
				return (null, null);
			}

			if (tempDbNameWhileCreating != null)
			{
				databaseName = tempDbNameWhileCreating;
			}

			var actual = GetActual(connection, databaseName);

			if (desired != actual)
			{
				connection.ExecuteNonQueryWithRetry($"ALTER DATABASE {databaseName.QuoteName()} SET RECOVERY {desired}");
			}

			return (actual, desired);
		}

		IEnumerable<string> GetAllDatabases(string serverName, string mainDbName)
		{
			using var connection = Db.NewAdminConnection(serverName, mainDbName);
			return connection.GetDatabases(DatabaseType.All);
		}

		DbType GetDbType(DbConnection connection, string mainDbName, string databaseName)
		{
			if (databaseName.StartsWith(SharedRefDbPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return DbType.SharedRefDb;
			}

			if (databaseName.StartsWith(SharedAvailabilityGroupRefDbPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return DbType.SharedAvailabilityGroupRefDb;
			}

			if (databaseName.StartsWith(mainDbName, StringComparison.OrdinalIgnoreCase))
			{
				var suffix = databaseName.Substring(mainDbName.Length);
				if (string.IsNullOrEmpty(suffix))
				{
					return DbType.Main;
				}

				if (suffix.Equals(AuditDbSuffix, StringComparison.OrdinalIgnoreCase))
				{
					return DbType.Audit;
				}

				if (suffix.Equals(EdwDbSuffix, StringComparison.OrdinalIgnoreCase))
				{
					return DbType.EDW;
				}

				if (suffix.Equals(UserRepositoryDbSuffix, StringComparison.OrdinalIgnoreCase))
				{
					return DbType.UserRepository;
				}

				if (suffix.StartsWith(ExclusiveRefDbAffix, StringComparison.OrdinalIgnoreCase))
				{
					return DbType.ExclusiveRefDb;
				}

				if (storageDatabaseSuffixRegex.IsMatch(suffix))
				{
					return DbType.SD;
				}
			}

			if (databaseName.Equals(GetSingleRefDbName(connection), StringComparison.OrdinalIgnoreCase))
			{
				return DbType.SingleSharedRefDb;
			}

			return DbType.Unknown;
		}

		static bool IsWiseTechGlobalDatabaseServer(DbConnection connection)
		{
			return DataUtils.IsWiseTechGlobalDatabaseServer(connection);
		}

		static bool IsAlwaysOnEnabled(DbConnection connection, string mainDbName, string databaseName)
		{
			if (AlwaysOn.IsDbPartOfAlwaysOn(connection, databaseName))
			{
				return true;
			}

			if (!connection.DatabaseExists(databaseName))
			{
				return AlwaysOn.IsDbPartOfAlwaysOn(connection, mainDbName);
			}

			return false;
		}

		static bool IsSandEnvironment(DbConnection connection)
		{
			string serveDomain;
#if DEBUG
			if (ServerDomain_ForTest.IsOverriden)
			{
				serveDomain = ServerDomain_ForTest.Value;
			}
			else
#endif
			{
				serveDomain = connection.ServerDomain;
			}

			return SandDomainName.Equals(serveDomain, StringComparison.OrdinalIgnoreCase);
		}

		internal DbRecoveryModel GetRegistryValue(DbConnection connection)
		{
			if (cachedDbValues.TryGetValue(connection.CurrentDatabase, out var cachedValues) && cachedValues != null)
			{
				return cachedValues.RecoveryModel;
			}

			return DbRecoveryModel.Get(DbRegistry.DatabaseRecoveryModel.LoadValue(connection));
		}

		internal string GetSingleRefDbName(DbConnection connection)
		{
			if (cachedDbValues.TryGetValue(connection.CurrentDatabase, out var cachedValues) && cachedValues != null)
			{
				return cachedValues.SingleRefDatabaseName;
			}

			return GetSingleRefDatabaseName(connection);
		}

		static bool IsProductionSystem => EnvProxy.Instance.IsProductionSystem;

#if DEBUG
		public static Overridable<string> ServerDomain_ForTest { get; } = new Overridable<string>(null);
#endif

		const string AuditDbSuffix = Db.AuditDatabaseSuffix;
		const string EdwDbSuffix = Db.EdwDatabaseSuffix;
		const string UserRepositoryDbSuffix = DbUserRepository.RepositoryDbSuffix;
		const string ExclusiveRefDbAffix = "_" + RefDbAffix + "_";
		const string SandDomainName = "sand.wtg.zone";
		static readonly Regex storageDatabaseSuffixRegex = new Regex(@"^_SD\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public IDisposable LoadAndCacheRequiredDbValues(DbConnection mainDbConnection)
		{
			var dbName = mainDbConnection.CurrentDatabase;
			var dbValues = new CachedValues(dbName, GetRegistryValue(mainDbConnection), GetSingleRefDbName(mainDbConnection));
			return CacheTemporaryDbValues(dbName, dbValues);
		}

		static IDisposable CacheTemporaryDbValues(string dbName, CachedValues dbValues)
		{
			cachedDbValues.AddOrUpdate(dbName, dbValues, (key, prev) => dbValues);
			return dbValues;
		}

		[ThreadSafe]
		static readonly ConcurrentDictionary<string, CachedValues> cachedDbValues = new ConcurrentDictionary<string, CachedValues>();

		[Immutable]
		class CachedValues : IDisposable
		{
			public CachedValues(string dbName, DbRecoveryModel recoveryModel, string singleRefDatabaseName)
			{
				this.dbName = dbName;
				RecoveryModel = recoveryModel;
				SingleRefDatabaseName = singleRefDatabaseName;
			}

			public DbRecoveryModel RecoveryModel { get; }
			public string SingleRefDatabaseName { get; }

			public void Dispose()
			{
				cachedDbValues.TryUpdate(dbName, null, this);
			}

			readonly string dbName;
		}

		enum DbType
		{
			Unknown,
			Main,
			SD,
			UserRepository,
			Audit,
			EDW,
			ExclusiveRefDb,
			SharedRefDb,
			SharedAvailabilityGroupRefDb,
			SingleSharedRefDb,
		}
	}
}
