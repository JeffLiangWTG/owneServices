using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.Providers.Common;
using CargoWise.Data.Services;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	#region Interfaces

	public interface IDbConnectionMultiThreadControl
	{
		IDisposable UseMainConnectionAcrossAllThreadsForTests();
	}

	public interface IDbUpgradeSupport
	{
		IDisposable ElevateToAdminConnectionForUpgrade();
		IDisposable SetUpgradeWorkingInProgress();
		void SetDatabaseHasBeenUpgraded(int diffVersionsBinaryVsDatabase);
	}

	#endregion

	public partial class Db : IDbConnectionMultiThreadControl, IDbUpgradeSupport
	{
		Db()
		{
		}

		public static Db Instance
		{
			get
			{
				if (instance == null)
				{
					lock (singletonPadLock)
					{
						if (instance == null)
						{
							var tempInstance = new Db();
							instance = tempInstance;
						}
					}
				}
				return instance;
			}
		}
		static Db instance;

		static readonly object singletonPadLock = new object();

		public static bool InstanceIsNull
		{
			get { return instance == null; }
		}

		#region Published

		#region IsDatabaseUpgraded

		[ThreadSafe]
		static volatile bool isDatabaseUpgraded;

		public static bool IsDatabaseUpgraded => isDatabaseUpgraded;

		void IDbUpgradeSupport.SetDatabaseHasBeenUpgraded(int diffVersionsBinaryVsDatabase)
		{
			isDatabaseUpgraded = diffVersionsBinaryVsDatabase != 0;
		}

		#endregion

		#region Connection Factories

		public static IDataProviderFactory DefaultDataProviderFactory
		{
			get
			{
				if (Instance.defaultDataProviderFactory == null)
				{
					Instance.defaultDataProviderFactory = (SqlProxyClientProvider.IsHttpEnabled) ?
						new HttpClient.HttpDataProviderFactory() :
						new SqlDataProviderFactory();
				}

				return Instance.defaultDataProviderFactory;
			}
		}
		IDataProviderFactory defaultDataProviderFactory;

		public static bool DatabaseUpgradedExceptionHasBeenThrownInConnection
		{
			get
			{
				return currentDbConnection?.DatabaseUpgradedExceptionHasBeenThrown ?? false;
			}
		}

		[ThreadStatic]
		static DbConnection currentDbConnection;

		/// <summary>
		/// Database Connection
		/// </summary>
		public static DbConnection Connection
		{
			get
			{
#if DEBUG
				if (ConnectionOverrideForTest != null)
				{
					currentDbConnection = ConnectionOverrideForTest;
					return currentDbConnection;
				}
#endif

				if (AdminConnection != null && AdminConnection.ThreadSentry.IsOwner)
				{
					currentDbConnection = AdminConnection;
					return currentDbConnection;
				}

				var db = Instance;
				if (db.ShouldUseMainConnection)
				{
					currentDbConnection = db.MainConnectionInstance;
					return currentDbConnection;
				}

				currentDbConnection = GetDisposableExtraConnection();
				return currentDbConnection;
			}
		}

#if DEBUG
		public static DbConnection ConnectionOverrideForTest
		{
			get
			{
				return connectionOverrideForTest;
			}
			set
			{
				connectionOverrideForTest = value;
			}
		}

		[ThreadStatic]
		static DbConnection connectionOverrideForTest;
#endif

		static AdminConnection adminConnection;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static long upgradeInProgressState = 0L;

		public static bool IsUpgradeWorkingInProgress
		{
			get => Interlocked.Read(ref upgradeInProgressState) == 1L;
			private set => Interlocked.Exchange(ref upgradeInProgressState, value ? 1L : 0L);
		}

		IDisposable IDbUpgradeSupport.SetUpgradeWorkingInProgress()
		{
			IsUpgradeWorkingInProgress = true;
			return new DisposableAction(() => IsUpgradeWorkingInProgress = false);
		}

		/// <remarks>
		/// All upgrade scripts use Db.Connection so it should return the admin connection during upgrade
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Very old code. Don't know what potential exceptions to expect.")]
		IDisposable IDbUpgradeSupport.ElevateToAdminConnectionForUpgrade()
		{
			try
			{
				DisposeThreadConnection();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			adminConnection = NewAdminConnection();
			return new DisposableAction(delegate
			{
				try
				{
					if (adminConnection != null)
					{
						adminConnection.Dispose();
					}
				}
				finally
				{
					adminConnection = null;
				}
			});
		}

		public static AdminConnection AdminConnection
		{
			get
			{
				return adminConnection;
			}
		}

		public static AdminConnection NewAdminConnection()
		{
			return AdminConnection.New(ServerName, DatabaseName);
		}

		public static LockoutReason GetLockoutReason()
			=> DbLockout.GetLockoutReason();

		public static bool IsUpgradeLockoutError(DbException sqlException)
			=> DbLockout.IsUpgradeLockoutError(sqlException);

		public static AdminConnection NewAdminConnection(string databaseName)
		{
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			var newAdminConnection = AdminConnection.New(ServerName, DatabaseName);
			((ICurrentDbControl)newAdminConnection).UseDatabase(databaseName);

			return newAdminConnection;
		}

		public static AdminConnection NewAdminConnection(string serverName, string databaseName)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return AdminConnection.New(serverName, databaseName);
		}

		#endregion

		#region Services

		static void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IDatabaseCreationService, DatabaseCreationService>();
		}

		[ThreadSafe]
		static readonly Lazy<DbServiceLocator> globalServiceLocator = new Lazy<DbServiceLocator>(() => new DbServiceLocator(ConfigureServices));
		internal static DbServiceLocator GlobalServiceLocator => globalServiceLocator.Value;
		public static T Service<T>() => GlobalServiceLocator.GetService<T>();

		#endregion

		public static bool ShouldAlwaysBackup(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			return dbName.Equals(fDatabaseName, StringComparison.OrdinalIgnoreCase)
				|| SystemDatabasesForBackup.Any(db => db.Equals(dbName, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Application main database.
		/// MUST NOT be changed after the connection is established.
		/// </summary>
		public static string DatabaseName
		{
			get
			{
				if (string.IsNullOrEmpty(fDatabaseName))
				{
					throw new InvalidOperationException("DatabaseName referenced before initialization");
				}
				return fDatabaseName;
			}
		}
		protected static string fDatabaseName;

		public static void SetCurrentUser(string user)
		{
			currentUser = user;
		}

		public static string GetCurrentUserOrDefault(string defaultUser = "")
		{
			return currentUser ?? defaultUser;
		}

		[ThreadSafe]
		static string currentUser;

#if DEBUG
		public static void ClearServerDetails()
		{
			if (fInitialDatabaseName == null)
			{
				fInitialDatabaseName = fDatabaseName;
			}

			fServerName = null;
			fDatabaseName = null;
			ProtectedDataService.ClearGlobalServices();
		}

		[ThreadSafe]
		static string fInitialDatabaseName;
		public static string InitialDatabaseName => fInitialDatabaseName ?? DatabaseName;

		public static IDisposable ClearServerDetailsTemporarily()
		{
			var serverName = Db.ServerName;
			var databaseName = Db.DatabaseName;

			return new DisposableAction(
				ClearServerDetails,
				() => InitializeDatabaseDetails(serverName, databaseName));
		}
#else
		public static string InitialDatabaseName => DatabaseName;
#endif

		public static bool DatabaseNameIsInitialized => !string.IsNullOrEmpty(fDatabaseName);

		public static string AuditDatabaseName
		{
			get
			{
				return DatabaseName + AuditDatabaseSuffix;
			}
		}

		public static string EdwDatabaseName
		{
			get
			{
				return DatabaseName + EdwDatabaseSuffix;
			}
		}

		/// <summary>
		/// Application database server.
		/// MUST NOT be changed after the connection is established.
		/// </summary>
		public static string ServerName
		{
			get
			{
				if (string.IsNullOrEmpty(fServerName))
				{
					throw new InvalidOperationException("ServerName referenced before initialization");
				}
				return fServerName;
			}
		}

		public static void InitializeDatabaseDetails(string serverName, string databaseName)
		{
			InitializeDatabaseDetails(serverName, databaseName, ApplicationType.Default);
		}

		public static void InitializeDatabaseDetails(
			string serverName,
			string databaseName,
			ApplicationType applicationType)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			if (serverName != fServerName || databaseName != fDatabaseName)
			{
				if (fDatabaseName != null || fServerName != null)
				{
					throw new OdysseyDataException("Server and database are already initialized. Cannot change the initial server or database name.");
				}
				fDatabaseName = databaseName;
				fServerName = serverName;

				ProtectedDataService.ConfigureGlobalServices(serverName, databaseName, (services) =>
				{
					services.ConfigureProtectedDataSqlExtensions(applicationType);
					services.ConfigureProtectedDataAdministrationServices();
					services.ConfigureProtectedDataSqlServerAdministrationServices();
					services.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, CargowisePDSAdministrationSqlContextManager>();
				});
			}
		}

		protected static string fServerName;

		public static bool ServerNameIsInitialized => !string.IsNullOrEmpty(fServerName);

		public static string SqlServerPort
		{
			get
			{
				var port = "";
				var part = fServerName.Split(',');
				if (part.Length == 2)
				{
					port = part[1];
				}

				return port;
			}
		}

		/// <summary>
		/// *DANGEROUS* Temporarily disable schema version checks globally, for all connections and all threads.
		/// A connection opened while checks are disabled will stay open afterwards and there will no version check,
		/// even when the return value is disposed.
		/// The application will not detect if the database is upgraded.
		/// If the application is CargoWise One (or a process controller, etc) it will not upgrade itself or exit.
		/// This risks old software running on a new database for a long time.
		/// </summary>
		/// <returns>disposable to re-enable checks</returns>
		public static IDisposable DisableSchemaVersionCheck()
		{
			if (IsGlobalSchemaVersionCheckDisabled)
			{
				return new DisposableAction(() => { });
			}
			IsGlobalSchemaVersionCheckDisabled = true;
			return new DisposableAction(() => { IsGlobalSchemaVersionCheckDisabled = false; });
		}

		public static IDisposable DisableSchemaVersionCheckOnCurrentThread()
		{
			if (IsThreadSchemaVersionCheckDisabled)
			{
				return new DisposableAction(() => { });
			}

			isThreadSchemaVersionCheckDisabled = true;
			return new DisposableAction(() => { isThreadSchemaVersionCheckDisabled = false; });
		}

		/// <summary>
		/// *EXTREMELY DANGEROUS* Permanently disable schema version checks globally, for all connections and all threads.
		/// For details on the danger <see cref="DisableSchemaVersionCheck"/>
		/// </summary>
		public static void DisableSchemaVersionCheckPermanently()
			=> IsGlobalSchemaVersionCheckDisabled = true;

		[Conditional("DEBUG")]
		public static void EnableSchemaVersionCheckPermanently_ForTest()
			=> IsGlobalSchemaVersionCheckDisabled = false;

		/// <summary>
		/// *DANGEROUS* Permanently disable schema version checks, for all connections opened by this thread.
		/// For details on the danger <see cref="DisableSchemaVersionCheck"/>
		/// </summary>
		public static void DisableThreadSchemaVersionCheckPermanently()
			=> isThreadSchemaVersionCheckDisabled = true;

		[Conditional("DEBUG")]
		public static void EnableThreadSchemaVersionCheckPermanently_ForTest()
			=> isThreadSchemaVersionCheckDisabled = false;

		public static bool IsSchemaVersionCheckDisabled
			=> IsGlobalSchemaVersionCheckDisabled || IsThreadSchemaVersionCheckDisabled;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static bool IsGlobalSchemaVersionCheckDisabled { get; private set; }

		public static bool IsThreadSchemaVersionCheckDisabled => isThreadSchemaVersionCheckDisabled;
		[ThreadStatic]
		static bool isThreadSchemaVersionCheckDisabled;

		public bool? IsDatabaseSecurityOpen()
		{
			var dbSecurity = new DbSecurity();
			return dbSecurity.IsDatabaseSecurityOpen();
		}

		#endregion

		#region FixReaderLogin

		public static void FixReaderLogin(string targetServer)
		{
			Argument.NotNullOrEmpty(targetServer, nameof(targetServer));

			using (var adminConn = NewAdminConnection())
			{
				((IDbLoginRepair)adminConn).EnsureReaderDbLogin();
				((IDbLoginRepair)adminConn).EnsureRestrictedReaderDbLogin();
			}

			if (!targetServer.Equals(ServerName, StringComparison.OrdinalIgnoreCase))
			{
				using (var tgtServerAdminConn = NewAdminConnection(targetServer, DatabaseName))
				{
					var tgtServerLoginReader = new ReaderDatabaseLogin(tgtServerAdminConn);
					tgtServerLoginReader.DisableLogin();
					tgtServerLoginReader.EnableLogin(msg => { });

					var tgtServerLoginRestrictedReader = new RestrictedReaderDatabaseLogin(tgtServerAdminConn);
					tgtServerLoginRestrictedReader.DisableLogin();
					tgtServerLoginRestrictedReader.EnableLogin(msg => { });
				}
			}
		}

		#endregion

		#region Lockout

		public static DbLockoutState ResetLockout()
		{
			var auditServer = DbRegistry.BiAuditServer.LoadValue(Connection);
			var dataWarehouseServer = DbRegistry.BiDataWarehouseServer.LoadValue(Connection);

			using (var auditConnection = (!string.IsNullOrEmpty(auditServer) ?
					Db.NewAdminConnection(auditServer, Db.SqlMasterDb) :
					null))
			using (var dataWarehouseConnection = (!string.IsNullOrEmpty(dataWarehouseServer) ?
					Db.NewAdminConnection(dataWarehouseServer, Db.SqlMasterDb) :
					null))
			{
				return ResetLockout(AdminConnection, auditConnection, dataWarehouseConnection);
			}
		}

		public static DbLockoutState ResetLockout(AdminConnection mainDbConnection, AdminConnection auditConnection, AdminConnection dataWarehouseConnection)
		{
			Argument.NotNull(mainDbConnection, nameof(mainDbConnection));
			var lockoutState = mainDbConnection.ResetLockout();

			if (auditConnection != null &&
				!string.Equals(mainDbConnection.ServerNameReportedByDatabase, auditConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
			{
				((IDbLoginRepair)auditConnection).EnableApplicationDbLogins();
			}

			if (dataWarehouseConnection != null &&
				!string.Equals(mainDbConnection.ServerNameReportedByDatabase, dataWarehouseConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(auditConnection?.ServerNameReportedByDatabase, dataWarehouseConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
			{
				((IDbLoginRepair)dataWarehouseConnection).EnableApplicationDbLogins();
			}
			return lockoutState;
		}

		#endregion

		#region Implementation

		#region DB Connection Thread Control

		bool ShouldUseMainConnection
		{
			get
			{
				bool result =
					!IsServingWebApp
					&&
					(
						ReferenceEquals(Thread.CurrentThread, MainConnectionInstance.InstantiationThread)
						|| SqlProxyClientProvider.IsHttpEnabled
						|| isUsingMainConnectionAcrossAllThreads
					);
				return result;
			}
		}

		MainConnection MainConnectionInstance
		{
			get
			{
				return MainConnection.Instance;
			}
		}

		public static event EventHandler OnMainConnectionOpened
		{
			add
			{
				MainConnection.OnConnectionOpened += value;
			}
			remove
			{
				MainConnection.OnConnectionOpened -= value;
			}
		}

		IDisposable IDbConnectionMultiThreadControl.UseMainConnectionAcrossAllThreadsForTests()
		{
#if DEBUG
			if (!NUnit.Framework.TestingState.IsRunningTests)
			{
				throw new InvalidOperationException("UseMainConnectionAcrossAllThreadsForTests is for tests only!");
			}
#endif

			bool previousValue = isUsingMainConnectionAcrossAllThreads;
			isUsingMainConnectionAcrossAllThreads = true;
			return new DisposableAction(() => isUsingMainConnectionAcrossAllThreads = previousValue);
		}

		bool isUsingMainConnectionAcrossAllThreads;

		#region IsServingWebApp

		bool IsServingWebApp
		{
			get
			{
				bool result = DbEnv.Instance.IsServingWebBasedApp;

#if DEBUG
				if (isWebTestOverride != null)
				{
					result = isWebTestOverride.Value;
				}
#endif

				return result;
			}
		}

#if DEBUG
		internal bool? IsWebTestOverride
		{
			get { return isWebTestOverride; }
			set { isWebTestOverride = value; }
		}
		bool? isWebTestOverride;
#endif

		#endregion

#if DEBUG
		public static ReaderWriterLock DbStateLock
		{
			get
			{
				return dbStateLock;
			}
		}

		static readonly ReaderWriterLock dbStateLock = new ReaderWriterLock();
#endif

		#endregion

		#endregion

		#region Logins

		public static IEnumerable<string> GetAllLoginNames(string dbName) => new[]
		{
			CargoWiseReaderLoginCredentials.UserNameFor(dbName),
			CargoWiseWriterLoginCredentials.UserNameFor(dbName),
			RestrictedReaderLoginCredentials.UserNameFor(dbName),
			RestrictedWriterLoginCredentials.UserNameFor(dbName),
			UnrestrictedWriterLoginCredentials.UserNameFor(dbName),
		};

		#endregion

		public static bool IsSystemDatabase(string dbName)
		{
			return (
				string.Equals(dbName, Db.SqlMasterDb, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(dbName, "msdb", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(dbName, "model", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(dbName, "tempdb", StringComparison.OrdinalIgnoreCase)
			);
		}

		public static string GetMachineNameIfLocal(string serverName)
		{
			return Regex.Replace(serverName, @"^(localhost|\(local\)|127\.0\.0\.1|\.)", System.Environment.MachineName, RegexOptions.IgnoreCase);
		}
	}
}

#region Test
#if DEBUG

namespace CargoWise.Data
{
	using System.Data;
	using CargoWise.Data.Testing;
	using Moq;

	#region Partial class

	public partial class Db
	{
		public static IDisposable DisposableUpgrade_ForTest(bool acquireLockOut = true, bool killOtherConnections = true, bool updateSchemaVersion = true, bool disableLogins = false, bool resetDatabaseUpgradedForTest = true)
		{
			var savedDbEnv = DbEnv.Instance;
			var pluginMock = new Mock<IDbConnectionGuiPlugin>();
			var dbEnvMock = new Mock<BaseDbEnvironment>();
			pluginMock.Setup(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradedException>()));
			dbEnvMock.Setup(x => x.ConnectionGuiPlugin).Returns(pluginMock.Object);
			dbEnvMock.Setup(x => x.ConnectionPooling).Returns(new NoConnectionPooling());
			DbEnv.SetDbEnvironment(dbEnvMock.Object);

			var newAdminConnection = Db.NewAdminConnection();
			if (acquireLockOut)
			{
				newAdminConnection.AcquireLockout(disableLogins: disableLogins);
			}

			if (killOtherConnections)
			{
				DbConnectionKiller.KillOtherConnections(newAdminConnection, DatabaseName);
			}

			var initialSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(newAdminConnection);
			if (updateSchemaVersion)
			{
				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(initialSchemaVersion + 1, newAdminConnection);
			}

			return new DisposableAction(() =>
			{
				DbEnv.SetDbEnvironment(savedDbEnv);

				if (updateSchemaVersion)
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(initialSchemaVersion, newAdminConnection);
				}

				newAdminConnection.ResetLockout();
				newAdminConnection.Dispose();

				using (Db.DisposableActionForDbConnection())
				{
					Db.Connection.EnsureIsOpen();
				}

				if (resetDatabaseUpgradedForTest)
				{
					Db.ResetDatabaseUpgraded_ForTest();
				}
			});
		}

		public static void ResetDatabaseUpgraded_ForTest()
		{
			((IDbUpgradeSupport)Db.Instance).SetDatabaseHasBeenUpgraded(diffVersionsBinaryVsDatabase: 0);
		}
	}

	#endregion // Partial class

	#region AdoTestUtils

	public static class AdoTestUtils
	{
		internal static void KillMainConnection()
		{
			KillConnection(Db.Connection);
		}

		public static void KillConnection(DbConnection connectionToBeKilled)
		{
			int currentProcess = Convert.ToInt32(connectionToBeKilled.ExecuteScalar("SELECT @@spid"));

			using (DbConnection anotherConn = Db.NewAdminConnection())
			{
				string sqlText = "KILL " + currentProcess.ToString();
				anotherConn.ExecuteNonQuery(sqlText);
			}
		}

		public static void CreateDbIfNotExists(AdminConnection adminConnection, string testDbName, string mainDbName = null)
		{
			CreateDbIfNotExists(adminConnection, testDbName, defaultDbRecoveryModel, mainDbName);
		}

		public static void CreateDbIfNotExists(AdminConnection adminConnection, string testDbName, DbRecoveryModel recoveryModel, string mainDbName = null)
		{
			mainDbName ??= GetMainDatabaseNameFromTargetDatabaseName(testDbName);

			// Switch from CW1 main db when creating another. We observed interference when the main db had just had a snapshot operation performed on it.
			using (((ICurrentDbControl)adminConnection).UseDatabase(Db.SqlMasterDb))
			{
				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = @testDbName)
				BEGIN
					CREATE DATABASE [{0}];
					ALTER DATABASE [{0}] COLLATE {1};
					ALTER DATABASE [{0}] SET RECOVERY {2};
				END",
					testDbName, Db.DatabaseCollation, recoveryModel);
				adminConnection.ExecuteNonQuery(sqlText, p => p.AddParameter("@testDbName", SqlDbType.NVarChar, 128, testDbName));

				using (((ICurrentDbControl)adminConnection).UseDatabase(testDbName))
				{
					#region CreateTestStorageDocs
					var sqlCreateStorageDocs = FormattableString.Invariant($@"
							IF OBJECT_ID('{testDbName}..StorageDocs','U') IS NULL
							BEGIN
								CREATE TABLE [{testDbName}].[dbo].[StorageDocs] (
								 [SC_PK] UNIQUEIDENTIFIER NOT NULL,
								 [SC_DataType] VARCHAR(20) NOT NULL DEFAULT 'TIF' ,
								 [SC_DocType] VARCHAR(4) NOT NULL DEFAULT '' ,
								 [SC_Desc] VARCHAR(128) NOT NULL DEFAULT '' ,
								 [SC_Language] VARCHAR(7) NOT NULL DEFAULT '',
								 [SC_FileName] NVARCHAR(256) NOT NULL DEFAULT '' ,
								 [SC_Date] SMALLDATETIME NOT NULL,
								 [SC_IsDeleted] CHAR(1) NOT NULL DEFAULT 'N' ,
								 [SC_IsPublished] CHAR(1) NOT NULL DEFAULT 'N' ,
								 [SC_IsSystemGenerated] CHAR(1) NOT NULL DEFAULT 'N' ,
								 [SC_SaveVersions] CHAR(1) NOT NULL DEFAULT 'N' ,
								 [SC_ImageData] VARBINARY(MAX) NOT NULL,
								 [SC_UncompressedSize] INT NOT NULL DEFAULT 0,
								 [SC_ExternalStorageSize] INT NOT NULL DEFAULT 0,
								 [SC_ImageData_HasValue] AS IIF(DATALENGTH(SC_ImageData) = 0, 0, 1),
								 [SC_VersionID] VARCHAR(1024) NOT NULL DEFAULT '',
								 [SC_ParentID] UNIQUEIDENTIFIER NULL,
								 [SC_SM] UNIQUEIDENTIFIER NOT NULL,
								 [SC_EncryptedDataKey] VARBINARY(64) NULL,
								 [SC_SCK_MasterKey] UNIQUEIDENTIFIER NULL,
								 [SC_GE_Department] UNIQUEIDENTIFIER NULL,
								 [SC_GC_Company] UNIQUEIDENTIFIER NULL,
								 [SC_GB_Branch] UNIQUEIDENTIFIER NULL,
								 [SC_RDS_NKDocSource] VARCHAR(3) NOT NULL DEFAULT '',
								 [SC_SystemCreateTimeUtc] SMALLDATETIME NOT NULL,
								 [SC_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
								 [SC_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL,
								 [SC_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
							);
							ALTER TABLE [{testDbName}].[dbo].[StorageDocs]
							ADD CONSTRAINT [PK_UX__SC_PK] PRIMARY KEY NONCLUSTERED  ([SC_PK] ASC)
							WITH ( IGNORE_DUP_KEY = OFF);

							CREATE NONCLUSTERED INDEX [NR_RX__SC_SM] ON [{testDbName}].[dbo].[StorageDocs] ([SC_SM] ASC)
							WITH ( IGNORE_DUP_KEY = OFF);

							CREATE NONCLUSTERED INDEX [NR_RX__SC_ParentID] ON [{testDbName}].[dbo].[StorageDocs] ([SC_ParentID] ASC)
							WHERE SC_ParentID IS NOT NULL
							WITH ( IGNORE_DUP_KEY = OFF);

							CREATE NONCLUSTERED INDEX [NR_RX__SC_ImageData_HasValue] ON [{testDbName}].[dbo].[StorageDocs] ([SC_ImageData_HasValue] ASC)

							CREATE NONCLUSTERED INDEX [NR_RX__SC_SystemLastEditTimeUtc] ON [{testDbName}].[dbo].[StorageDocs] ([SC_SystemLastEditTimeUtc] ASC)
							INCLUDE ( [SC_SystemLastEditUser] )

							CREATE NONCLUSTERED INDEX [NR_RX__SC_SystemCreateTimeUtc] ON [{testDbName}].[dbo].[StorageDocs] ([SC_SystemCreateTimeUtc] ASC)
							INCLUDE ( [SC_SystemCreateUser] )

							ALTER TABLE [{testDbName}].[dbo].[StorageDocs] WITH NOCHECK
							ADD CONSTRAINT [Constraint_SC_UncompressedSize] CHECK (SC_UncompressedSize >= 0);

							ALTER TABLE [{testDbName}].[dbo].[StorageDocs] WITH NOCHECK
							ADD CONSTRAINT [Constraint_SC_ExternalStorageSize_NOCHECK] CHECK (SC_ExternalStorageSize >= 0);
						END
");
					#endregion

					if (DocManagerUtils.IsDocManagerDatabase(testDbName))
					{
						adminConnection.ExecuteNonQuery(sqlCreateStorageDocs);
					}

					LoginRepairService.Instance.ReviveApplicationLogin<RestrictedWriterLoginCredentials>(adminConnection, mainDbName, adminConnection);

					// The admin connection caches the login names from the first database it is connected to,
					// We need to connect to main database first and then switch to target database for the function to work.
					using (var adminConnectionToTargetDb = Db.NewAdminConnection(adminConnection.ServerName, mainDbName))
					using (((ICurrentDbControl)adminConnectionToTargetDb).UseDatabase(testDbName))
					{
						((IDbLoginRepair)adminConnectionToTargetDb).EnsureWriterDbLoginHasRightsToCurrentDatabase();
					}
				}
			}
		}

		internal static void CreateDbIfNotExists(string testDbName, string mainDbName = null)
		{
			CreateDbIfNotExists(testDbName, defaultDbRecoveryModel, mainDbName);
		}

		internal static void CreateDbIfNotExists(string testDbName, DbRecoveryModel recoveryModel, string mainDbName = null)
		{
			using (AdminConnection adminConnection = Db.NewAdminConnection())
			{
				CreateDbIfNotExists(adminConnection, testDbName, recoveryModel, mainDbName);
			}
		}

		public static void CreateDbDropExisting(string testDbName, string mainDbName = null)
		{
			CreateDbDropExisting(testDbName, defaultDbRecoveryModel, mainDbName);
		}

		public static void CreateDbDropExisting(string testDbName, DbRecoveryModel recoveryModel, string mainDbName = null)
		{
			using (AdminConnection adminConnection = Db.NewAdminConnection())
			{
				CreateDbDropExisting(adminConnection, testDbName, recoveryModel, mainDbName);
			}
		}

		public static IDisposable CreateDbDropExistingDisposable(string testDbName, string mainDbName = null)
		{
			return CreateDbDropExistingDisposable(testDbName, defaultDbRecoveryModel, mainDbName);
		}

		public static IDisposable CreateDbDropExistingDisposable(string testDbName, DbRecoveryModel recoveryModel, string mainDbName = null)
		{
			var disposable = DropDbIfExistsDisposable(testDbName, mainDbName);
			try
			{
				CreateDbDropExisting(testDbName, recoveryModel, mainDbName);
			}
			catch
			{
				disposable.Dispose();
				throw;
			}
			return disposable;
		}

		public static IDisposable CreateDbDropExistingDisposable(AdminConnection adminConnection, string testDbName, string mainDbName = null)
		{
			return CreateDbDropExistingDisposable(adminConnection, testDbName, defaultDbRecoveryModel, mainDbName);
		}

		public static IDisposable CreateDbDropExistingDisposable(AdminConnection adminConnection, string testDbName, DbRecoveryModel recoveryModel, string mainDbName = null)
		{
			CreateDbDropExisting(adminConnection, testDbName, recoveryModel, mainDbName);
			return DropDbIfExistsDisposable(adminConnection, testDbName, mainDbName);
		}

		public static void CreateDbDropExisting(AdminConnection adminConnection, string testDbName, string mainDbName = null)
		{
			CreateDbDropExisting(adminConnection, testDbName, defaultDbRecoveryModel, mainDbName);
		}

		public static void CreateDbDropExisting(AdminConnection adminConnection, string testDbName, DbRecoveryModel recoveryModel, string mainDbName = null)
		{
			DropDbIfExists(adminConnection, testDbName, mainDbName);
			CreateDbIfNotExists(adminConnection, testDbName, recoveryModel, mainDbName);
		}

		public static void DropDbIfExists(AdminConnection adminConnection, string testDbName, string mainDbName = null)
		{
			mainDbName ??= GetMainDatabaseNameFromTargetDatabaseName(testDbName);

			DbConnectionKiller.KillOtherConnections(adminConnection, testDbName);

			var sqlRepairDb = FormattableString.Invariant($@"
IF EXISTS (SELECT NULL FROM sys.databases where name = @name AND state_desc = 'Emergency')
BEGIN
	ALTER DATABASE [{testDbName}] SET SINGLE_USER;
	DBCC CHECKDB ([{testDbName}], REPAIR_ALLOW_DATA_LOSS);
	ALTER DATABASE [{testDbName}] SET ONLINE;
END"
			);
			adminConnection.ExecuteNonQuery(sqlRepairDb, p => p.AddParameter("@name", SqlDbType.NVarChar, 128, testDbName));

			var sqlDropDb = FormattableString.Invariant($@"
IF EXISTS (SELECT name FROM sys.databases WHERE name = @name) DROP DATABASE [{testDbName}]
");
			adminConnection.ExecuteNonQuery(sqlDropDb, p => p.AddParameter("@name", SqlDbType.NVarChar, 128, testDbName));

			DbConnection.RemoveCacheEntryFromSnapshotEnabledDictionary(testDbName);

			if (testDbName == mainDbName)
			{
				var dropAllLoginsSql = @"
SET xact_abort OFF
DECLARE @loginName nvarchar(128)
WHILE (1 = 1) 
BEGIN  
  SELECT TOP 1 @loginName = [name] FROM sys.server_principals WHERE [name] like @loginNamePattern
  IF @@ROWCOUNT = 0 BREAK;
  EXEC sp_droplogin @loginName
END";
				adminConnection.ExecuteNonQuery(dropAllLoginsSql,
					cmd => cmd.AddParameter("@loginNamePattern", SqlDbType.NVarChar, 128, $"{mainDbName}[_]%"));
			}
		}

		public static void SetDbRecoveryModelForTest(AdminConnection connection, string dbName, DbRecoveryModel recoveryModel)
		{
			connection.ExecuteNonQuery($"ALTER DATABASE {dbName.QuoteName()} SET RECOVERY {recoveryModel};");
		}

		internal static void DropDbIfExists(string testDbName, string mainDbName = null)
		{
			using (AdminConnection adminConnection = Db.NewAdminConnection())
			{
				DropDbIfExists(adminConnection, testDbName, mainDbName);
			}
		}

		public static IDisposable DropDbIfExistsDisposable(string testDbName, string mainDbName = null)
		{
			return new DisposableAction(() => DropDbIfExists(testDbName, mainDbName));
		}

		public static IDisposable DropDbIfExistsDisposable(AdminConnection adminConnection, string testDbName, string mainDbName = null)
		{
			return new DisposableAction(() => DropDbIfExists(adminConnection, testDbName, mainDbName));
		}

		public static DateTime DbServerTime(DbConnection connection = null)
		{
			if (connection == null)
			{
				connection = Db.Connection;
			}

			return (DateTime)connection.ExecuteScalar("SELECT CURRENT_TIMESTAMP");
		}

		public static void DropDbLoginIfExists(AdminConnection adminConnection, string loginName)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, "IF exists(SELECT null FROM sys.server_principals WHERE name = @name) DROP LOGIN [{0}]", loginName);
			adminConnection.ExecuteNonQuery(sqlText, p => p.AddParameter("@name", System.Data.SqlDbType.NVarChar, 128, loginName));
		}

		public static void DropDbRoleIfExists(AdminConnection adminConnection, string dbName, string roleName)
		{
			using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
			{
				var sqlText = $@"
IF exists(SELECT null FROM sys.database_principals WHERE name = @roleName)
BEGIN
DECLARE @cmd AS NVARCHAR(MAX) = N'';

SELECT @cmd = @cmd + '
	ALTER ROLE ' + QUOTENAME(@rolename) + ' DROP MEMBER ' + QUOTENAME(members.[name]) + ';'
FROM sys.database_role_members AS rolemembers
	JOIN sys.database_principals AS roles
		ON roles.[principal_id] = rolemembers.[role_principal_id]
	JOIN sys.database_principals AS members
		ON members.[principal_id] = rolemembers.[member_principal_id]
WHERE roles.[name]=@rolename
EXEC(@cmd);

DROP ROLE {roleName.QuoteName()};

END";

				adminConnection.ExecuteNonQuery(sqlText, p => p.AddParameter("@roleName", SqlDbType.NVarChar, 128, roleName));
			}
		}

		public static void ModifyLoginPasswordIfExists(AdminConnection adminConnection, string loginName, string password)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, "IF exists(SELECT null FROM sys.server_principals WHERE name = '{0}') ALTER LOGIN [{0}] WITH PASSWORD = '{1}'", loginName, password.QuoteEscapedName('\''));
			adminConnection.ExecuteNonQuery(sqlText);
		}

		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed for DbException under Common namespace")]
		public static System.Data.Common.DbException GetSqlException(int errorCode, string errorMessage, DbConnection connection)
		{
			SqlError error = SqlExceptionBuilder.CreateSqlError(errorCode, byte.MaxValue, byte.MinValue, connection.ServerName, errorMessage, "@@NoProceedure", 0);
			SqlErrorCollection errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			return SqlExceptionBuilder.CreateSqlException(errors);
		}

		static string GetMainDatabaseNameFromTargetDatabaseName(string targetDatabaseName)
		{
			if (targetDatabaseName.StartsWith(RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix) ||
				targetDatabaseName.StartsWith(RefDbTableNameResolver.SharedDbPrefix) ||
				targetDatabaseName.StartsWith(RefDbTableNameResolver.SharedRefDbPrefix))
			{
				throw new ArgumentException("Cannot infer the main database name from the provided database name, please input the main database name when creating or deleting database for extracting the login name");
			}
			return targetDatabaseName.Split('_')[0];
		}

		static readonly DbRecoveryModel defaultDbRecoveryModel = DbRecoveryModel.Simple;
	}

	#endregion
}

#endif
#endregion
