using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Database.ExtendedProperties;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.Integration;
using CargoWise.Shared;
using Enterprise.Core;
using Enterprise.Core.Environment;
using Enterprise.DbUpgrader.Assemblies;
using Enterprise.DbUpgrader.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Semaphores.Common;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Startup
{
	partial class UpgradeManager : BaseUpgradeManager
	{
		public UpgradeManager(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade)
		{
			this.schemaVersionBeforeUpgrade = versionInfo.DbReferenceVersion_Schema;
			this.transformationVersionBeforeUpgrade = versionInfo.DbReferenceVersion_Transformation;
			this.versionInfo = versionInfo;
			this.softwareUpgrade = softwareUpgrade;
		}

		readonly UpgradeInfo softwareUpgrade;
		readonly IVersionChangeInfo versionInfo;
		AdminConnection mainDbConnection;

		#region BI Server Connections

		AdminConnection AuditConnection
		{
			get
			{
				if (!isAuditConnectionLoaded)
				{
					LoadAuditConnection();
					isAuditConnectionLoaded = true;
				}
				return auditConnection;
			}
		}
		AdminConnection auditConnection;
		bool isAuditConnectionLoaded;

		protected virtual bool IsCdcDisabledOnRegistry()
		{
			return DbRegistry.BiDisableChangeDataCapture.LoadValue(mainDbConnection);
		}

		internal void SetAuditServerIfRequired()
		{
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);
			var isCdcDisabledOnRegistry = IsCdcDisabledOnRegistry();
			if (ShouldSetAuditServerToDefault(auditServer) && !isCdcDisabledOnRegistry)
			{
				((IUpgradeManager)this).ShowInfoMessage("Setting default value for Audit Server");
				BiServers.SaveAuditServer(mainDbConnection, mainDbConnection.ServerName);
				BiServers.ClearBiServersCache();
				isAuditConnectionLoaded = false;
			}
			else if (isCdcDisabledOnRegistry)
			{
				RemoveAuditServer();
			}
		}

		void RemoveAuditServer()
		{
			BiServers.ResetBiServers(mainDbConnection);
			auditConnection = null;
			isAuditConnectionLoaded = false;
		}

		internal bool ShouldSetAuditServerToDefault(string auditServer)
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			return (string.IsNullOrEmpty(auditServer)
				&& EnvProxy.IsHostedWithCargowise
				&& registration.LocalVerify() == ProductRegistrationVerifyResult.OK);
		}

		void LoadAuditConnection()
		{
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);
			if (!string.IsNullOrEmpty(auditServer))
			{
				auditConnection = LoadBiServerConnectionIfProductRegistrationValid(auditServer, isAudit: true);
			}
			else
			{
				auditConnection = LoadBiServerConnectionDebugMode(auditServer);
			}
		}

		/// <summary>
		/// Return an open connection to the given server.
		/// Returns the given main DB connection if it's to the same server,
		/// otherwise creates and opens a new connection to the master db.
		/// The new connection will have a default command timeout of zero (no timeout).
		/// </summary>
		/// <exception cref="OdysseyException">thrown if connection failed to open</exception>
		static AdminConnection GetOrCreateServerConnection(AdminConnection connectionToMainDbServer, string server)
		{
			AdminConnection connection;
			if (connectionToMainDbServer.ServerName.Equals(server, StringComparison.OrdinalIgnoreCase))
			{
				connection = connectionToMainDbServer;
			}
			else
			{
				connection = Db.NewAdminConnection(server, Db.SqlMasterDb);
				connection.IsUpgradeCheckDisabled = true;

				try
				{
					connection.EnsureIsOpen();
				}
				catch (SqlException ex)
				{
					throw new OdysseyException(
						string.Format(CultureInfo.InvariantCulture,
							"Could not connect to BI server [{0}]. Please check if server exists and security rights have been administered.",
							server),
						ex);
				}

				connection.DefaultCommandTimeOutInSeconds = 0;
			}
			return connection;
		}

		AdminConnection DataWarehouseConnection
		{
			get
			{
				if (!isDataWarehouseConnectionLoaded)
				{
					LoadDataWarehouseConnection();

					if (dataWarehouseConnection != null &&
						dataWarehouseConnection.ServerNameReportedByDatabase.Equals(AuditConnection?.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase) &&
						!dataWarehouseConnection.ServerNameReportedByDatabase.Equals(mainDbConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
					{
						dataWarehouseConnection.Dispose();
						dataWarehouseConnection = AuditConnection;
					}

					isDataWarehouseConnectionLoaded = true;
				}
				return dataWarehouseConnection;
			}
		}
		AdminConnection dataWarehouseConnection;
		bool isDataWarehouseConnectionLoaded;

		void LoadDataWarehouseConnection()
		{
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(mainDbConnection);
			if (!string.IsNullOrEmpty(dwServer))
			{
				dataWarehouseConnection = LoadBiServerConnectionIfProductRegistrationValid(dwServer, isAudit: false);
			}
			else
			{
				dataWarehouseConnection = LoadBiServerConnectionDebugMode(dwServer);
			}
		}

		bool IsProductRegistrationValid()
			=> (lazyIsProductRegistrationValid ?? (lazyIsProductRegistrationValid = VerifyRegistration())).Value;
		bool? lazyIsProductRegistrationValid;
		static bool VerifyRegistration()
			=> ObjectFactory.Get<IProductRegistration>().LocalVerify() == ProductRegistrationVerifyResult.OK;

		AdminConnection LoadBiServerConnectionIfProductRegistrationValid(string biServer, bool isAudit)
		{
			if (IsProductRegistrationValid())
			{
				return GetOrCreateServerConnection(mainDbConnection, biServer);
			}
			else if (ShouldLoadBiServerIfNotRegistered)
			{
				return LoadBiServerConnectionDebugMode(biServer);
			}
			else
			{
				var errorMsg = $"Product Key verification failed. It’s not safe to deploy {(isAudit ? "Audit" : "EDW")} database to BI server [{biServer}]. Remove {(isAudit ? "BiAuditServer" : "BiDataWarehouseServer")} value from registry and try again.";
				throw new OdysseyException(errorMsg);
			}
		}

		internal bool ShouldLoadBiServerIfNotRegistered
		{
			get
			{
				if (!shouldLoadBiServerIfNotRegistered.HasValue)
				{
#if DEBUG
					shouldLoadBiServerIfNotRegistered = true;
#else
					shouldLoadBiServerIfNotRegistered = false;
#endif
				}
				return shouldLoadBiServerIfNotRegistered.Value;
			}
#if DEBUG
			set
			{
				shouldLoadBiServerIfNotRegistered = value;
			}
#endif
		}
		bool? shouldLoadBiServerIfNotRegistered;

		AdminConnection LoadBiServerConnectionDebugMode(string biServer)
		{
			AdminConnection connection = null;
#if DEBUG
			if (string.IsNullOrEmpty(biServer))
			{
				connection = mainDbConnection;
			}
			else if (biServer == mainDbConnection.ServerName && biServer.StartsWith(System.Environment.MachineName, StringComparison.OrdinalIgnoreCase))
			{
				((IUpgradeManager)this).ShowInfoMessage("Running BI Server locally in debug mode.");
				connection = mainDbConnection;
			}
			else
			{
				((IUpgradeManager)this).ShowInfoMessage("Resetting BI registry items in debug mode as it may be a copy of a production system.");
				BiServers.ResetBiServers(mainDbConnection);
				connection = mainDbConnection;
			}
#endif
			return connection;
		}

		#endregion

		#region InitializeTasks

		void InitializeTasks()
		{
			InitialiseRequiredUpgraders();
			requiresLockout = offlineActionList.RequiresApplicationLockout;
			SetTotalNumberOfTasks();
		}

		enum UpgradeStep
		{
			// on-line upgrade
			CreateTemporaryResources

			// off-line upgrade
			, CLR
			, SchemaUpgrade
			, DatabaseScriptObjectUpgrade
			, DataTransformations
			, AutomatedTransformations
			, DataUpgrade
			, ClientSpecificDocumentsUpgrade
			, DbObjectsSynchronising
			, CycleCryptographicKeys
			, CleanupTemporaryResources
			, PTKey
			, OFXKey
		}

		protected void InitialiseRequiredUpgraders()
		{
			onlineActionList = new UpgradeActionList();
			offlineActionList = new UpgradeActionList();

			// CLR Assembly Deployment
			var assembliesUpgrader = GetAssembliesUpgrader();
			if (assembliesUpgrader.IsUpgradeRequired || ForceBiDatabaseUpgrade)
			{
				offlineActionList.Add(UpgradeStep.CLR, assembliesUpgrader.UpgradeAction);
			}

			var schemaUpgrader = new SchemaUpgrader(this, mainDbConnection, AuditConnection, DataWarehouseConnection);

			// Schema Upgrade
			if (versionInfo.IsRequired_Schema || versionInfo.IsRequired_Transformation || ForceBiDatabaseUpgrade)
			{
				onlinePreSchemaUpgrader = new OnlinePreSchemaUpgrader(this, mainDbConnection, AuditConnection, DataWarehouseConnection, versionInfo.DbReferenceVersion_Schema);

				onlineActionList.Add(UpgradeStep.CreateTemporaryResources, "Create temporary resources", schemaUpgrader.SetSelectiveXMLIndexesChangedFilter);
				onlineActionList.Add(UpgradeStep.CreateTemporaryResources, "Create temporary resources", schemaUpgrader.CreateTemporaryTransformationIndexes);

				offlineActionList.Add(UpgradeStep.SchemaUpgrade, schemaUpgrader.UpgradeAction);
				PrepareRefreshDependentScriptsAfterOfflineUpgradeTransaction(schemaUpgrader);

				offlineActionList.Add(UpgradeStep.DbObjectsSynchronising, "Database objects synchronising", () => schemaUpgrader.CreateAndValidateCheckConstraints());
				offlineActionList.Add(UpgradeStep.CleanupTemporaryResources, "Cleanup temporary resources", schemaUpgrader.DropTemporaryTransformationIndexes);
			}

			var checkTriggers = false;

			var transformationUpgrader = new DataTransformationUpgrader(this, schemaUpgrader.TransformationDirector, mainDbConnection);
			var scriptUpgrader = new Script.ScriptUpgrader(this, mainDbConnection, AuditConnection, DataWarehouseConnection, versionInfo.DbReferenceVersion_Script
				, transformationUpgrader.GetTriggerTransformations().Select(x => x.TriggerName));

			// Database Script Object Upgrade (views, procedures, functions, etc.)
			if (versionInfo.IsRequired_Script || assembliesUpgrader.IsUpgradeRequired || ForceBiDatabaseUpgrade || versionInfo.IsRequired_Transformation)
			{
				onlineActionList.Add(UpgradeStep.CreateTemporaryResources, "Create temporary resources", scriptUpgrader.CreateTemporaryScriptIndexes);

				offlineActionList.Add(UpgradeStep.DatabaseScriptObjectUpgrade, scriptUpgrader.UpgradeAction);

				checkTriggers = true;
			}

			// Data Transformations
			if (versionInfo.IsRequired_Transformation)
			{
				offlineActionList.Add(UpgradeStep.DataTransformations, transformationUpgrader.UpgradeAction);
				checkTriggers = true;
			}

			if (checkTriggers)
			{
				offlineActionList.Add(UpgradeStep.DbObjectsSynchronising, "Database objects synchronising", scriptUpgrader.CheckTriggersAreEnabledAndSync);
			}

			// Automated Data Transformations
			var isRequired_AutoTransformations =
					versionInfo.IsRequired_Schema
					|| versionInfo.IsRequired_Transformation
					|| versionInfo.IsRequired_Data
					;
			if (isRequired_AutoTransformations)
			{
				offlineActionList.Add(UpgradeStep.AutomatedTransformations, transformationUpgrader.AutomatedTransformations);
			}

			// Data Upgrade
			if (versionInfo.IsRequired_Data)
			{
				var dataUpgFactory = new DataUpgraderFactory();
				var dataUpgrader = dataUpgFactory.New(this, mainDbConnection, versionInfo.DbReferenceVersion_Data);

				offlineActionList.Add(UpgradeStep.DataUpgrade, dataUpgrader.UpgradeAction);
			}

			var clientUpgrade = softwareUpgrade;
			var upgradeClientDocumentsInCurrentVersion = false;
			if (clientUpgrade == null)
			{
				var curUpgradeVersion = UpgradeManagerFactory.NewUpgradeManager().QueryCurrentVersion();
				if (curUpgradeVersion != null && new VersionNumber(curUpgradeVersion.Version) == ReleaseInfo.Instance.VersionNumber && !string.IsNullOrEmpty(DataRegistry.Instance.ClientDocumentName))
				{
					upgradeClientDocumentsInCurrentVersion = true;
					clientUpgrade = curUpgradeVersion;
				}
			}

			// Client Specific Documents Upgrade
			if (versionInfo.IsRequired_ClientDocuments || upgradeClientDocumentsInCurrentVersion)
			{
				var clientDocumentsUpgrader = new ClientDocumentsUpgrader(this, mainDbConnection, new VersionLabel(0, 0), clientUpgrade);

				onlineActionList.Add(UpgradeStep.CreateTemporaryResources, "Prepare client documents for upgrade", () => clientDocumentsUpgrader.FetchAndExtractDocuments());
				offlineActionList.Add(UpgradeStep.ClientSpecificDocumentsUpgrade, clientDocumentsUpgrader.UpgradeAction);
			}

			offlineActionList.Add(UpgradeStep.CycleCryptographicKeys, new KeyCyclingUpgrader(this, mainDbConnection).UpgradeAction);

			offlineActionList.Add(UpgradeStep.PTKey, new PTKeyUpgrader(this, mainDbConnection).UpgradeAction);
			offlineActionList.Add(UpgradeStep.OFXKey, new OFXKeyUpgrader(this, mainDbConnection).UpgradeAction);
		}

		protected virtual BaseUpgrader GetAssembliesUpgrader() => new AssembliesUpgrader(this, mainDbConnection, AuditConnection, DataWarehouseConnection, versionInfo.DbReferenceVersion_Clr);

		protected virtual bool ForceBiDatabaseUpgrade
		{
			get
			{
				if (forceBiDatabaseUpgrade == null)
				{
					forceBiDatabaseUpgrade =
						new BiDatabaseChecker(AuditConnection, Db.AuditDatabaseName).ForceBiDatabaseUpgrade ||
						new BiDatabaseChecker(DataWarehouseConnection, Db.EdwDatabaseName).ForceBiDatabaseUpgrade;
				}
				return forceBiDatabaseUpgrade.Value;
			}
		}
		bool? forceBiDatabaseUpgrade;

		#endregion // InitializeTasks

		#region Run

		string extraInformationForEmail;

		public override bool IsHosted => EnvProxy.IsHostedWithCargowise;

		public override bool? IsInternalSystem => EnvProxy.IsInternalSystem;

		public override bool? IsUATSystem => EnvProxy.IsUATSystem;

		public override ValidationResponse Run()
		{
			extraInformationForEmail = null;
			var result = new ValidationResponse
			{
				Successful = false,
			};

			try
			{
				// RUNS UPGRADE HERE
				RunUpgradeOnAdminConnectionWithNoTimeout();
				result.Successful = true;
			}
			catch (Exception ex) when (HandleLogOnlyException(ex, this))
			{
				//do nothing
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ReportException(ex, this);
			}

			result.Information = extraInformationForEmail;
			return result;
		}

		const int Win32_WAIT_TIMEOUT = 258;

		internal static bool HandleLogOnlyException(Exception e, IUpgradeTaskWorkflowLogger logger)
		{
			if (e is UpgradeBlockedException)
			{
				logger.ShowTaskError(e.Message);
			}
			else if (e is ServerRequirementsNotMetException
				|| IsSqlErrorNotReportedAsIssue(e))
			{
				logger.ShowTaskError(e.Message);
				ShowExceptionErrorDetails(e, logger);
			}
			else if (e is DeniedByUserException)
			{
				var message = "Upgrade canceled by user. Failed to lock application access to the database [" + Db.DatabaseName + "] because applications are still running by other user.";
				logger.ShowInfoMessage(message);
				Globals.Message.ShowWarning(message);
			}
			else if (e.FlattenInnerExceptions().Any(ex => ex is SqlLockLostException)
				|| e is InvalidOperationException && e.Message.StartsWith("ExecuteReader requires an open and available Connection. The connection's current state is closed."))
			{
				logger.ShowTaskError("Connection to the database was lost while running the upgrade, please try the upgrade again later.");
				ShowExceptionErrorDetails(e, logger);
			}
			else if ((e.GetBaseException() is TimeoutException exKillConnections && exKillConnections.Message.StartsWith(DbConnectionKiller.TimeoutExceptionMessagePrefix))
				|| e.FlattenInnerExceptions().Any(ex => ex is OnlinePreSchemaUpgraderEnvironmentException)
				|| (e.GetAllExceptions().Any(ex => ex is Win32Exception win32Exception && win32Exception.NativeErrorCode == Win32_WAIT_TIMEOUT)))
			{
				logger.ShowTaskError(e.Message);
				ShowExceptionErrorDetails(e, logger);
			}
			else if (e is OdysseyException && e.Message.StartsWith("Product Key verification failed. It’s not safe to deploy"))
			{
				logger.ShowTaskError(e.Message);
				ShowExceptionErrorDetails(e, logger);
			}
			else if (e is SqlException sqlEx && new DbErrorMatch(sqlEx).IsInfrastructureDbError)
			{
				logger.ShowTaskError(e.Message);
			}
			else
			{
				logger.ShowTaskError(e.Message);
				ShowExceptionErrorDetails(e, logger);
				return false;
			}
			return true;
		}

		static void ReportException(Exception e, IUpgradeTaskWorkflowLogger logger)
		{
			try
			{
				ErrorReporter.ReportOnce("Failed to upgrade database", new UpgradeManagerException(e));
			}
			catch (Exception reportOnceException) when (!reportOnceException.IsCriticalException())
			{
				var errMsg = "\r\n\r\nError happened when reporting the failure of upgrading database. The error message is as following:\r\n";
				errMsg += reportOnceException.Message;
				logger.ShowTaskError(errMsg);
				ShowExceptionErrorDetails(reportOnceException, logger);
			}
		}

		void LogUnhandledException(Exception ex)
		{
			var logger = (IUpgradeTaskWorkflowLogger)this;
			logger.ShowTaskError(ex.Message);
			ShowExceptionErrorDetails(ex, logger);
		}

		static bool IsSqlErrorNotReportedAsIssue(Exception e)
		{
			if (e.GetBaseException() is SqlException sqlException)
			{
				switch (new DbErrorMatch(sqlException).ExceptionType)
				{
					case DbErrorType.CannotRenameTableBecauseItIsPublishedForReplication:
					case DbErrorType.ModifyFileEncounteredOperatingSystemError:
					case DbErrorType.CouldNotObtainExclusiveLock:
					case DbErrorType.DbFilegroupIsFull:
					case DbErrorType.LogIsFull:
						return true;
				}

				if (sqlException.Number == 18461)
				{
					// "Login failed for user. Reason: Server is in single user mode. Only one administrator can connect at this time."
					return true;
				}
			}

			return false;
		}

		internal void RunUpgradeOnAdminConnectionWithNoTimeout()
		{
			((IUpgradeManager)this).ShowInfoMessage("Connecting with administrator account");

			var dbUpgradeSupport = Db.Instance as IDbUpgradeSupport;

			using (dbUpgradeSupport.SetUpgradeWorkingInProgress())
			using (dbUpgradeSupport.ElevateToAdminConnectionForUpgrade())
			{
				mainDbConnection = Db.AdminConnection;
				mainDbConnection.DefaultCommandTimeOutInSeconds = 0;

				var sqlLock = TryGetUpgradeLockWithErrorHandling();

				if (sqlLock != null)
				{
					using (sqlLock)
					using (Db.DisableSchemaVersionCheck())
					{
						WriteDbExtPtyLockInfo(mainDbConnection);

						CheckExistenceOfUpgradeInfo(mainDbConnection);

						SetAutoStatisticsForDatabase(mainDbConnection);

						((IUpgradeManager)this).ShowInfoMessage("Acquired lock for upgrade");

						var clientHookLoader = ObjectFactory.Get<IClientHookLoader>();
						var clientHook = (IClientHook)InvokeOrExecute(() => clientHookLoader.ClientHook);
						if (clientHook != null && clientHook.HasCompanySpecificOverrides)
						{
							clientHook.IsUpgrading = true;
							if (!clientHook.IsInitialised)
							{
								InvokeOrExecute(() => clientHook.Initialise(true));
							}
						}

						try
						{
							SetupOtherConnections();

							ResetCdcIfRequired();

							InitializeTasks();

							((IUpgradeManager)this).ShowInfoMessage(
								string.Format(CultureInfo.InvariantCulture, "Database Server: {0} - Database Name: {1}{2}{3}{4}\r\n\r\n",
								mainDbConnection.ServerNameReportedByDatabase,
								Db.DatabaseName,
								(AuditConnection != null) ?
									string.Format(CultureInfo.InvariantCulture, " - Audit Server: {0}", AuditConnection.ServerNameReportedByDatabase)
									: "",
								(DataWarehouseConnection != null) ?
									string.Format(CultureInfo.InvariantCulture, " - Data Warehouse Server: {0}", DataWarehouseConnection.ServerNameReportedByDatabase)
									: "",
								string.Format(" - Machine Name: {0}", System.Environment.MachineName)));

							LogUpgradeProcessInfo();

							((IUpgradeManager)this).StartTask("Checking SQL Server requirements");
							CheckServerRequirements();
							CheckBiServersRequirements();

							// RUNS ON-LINE UPGRADE HERE
							RunOnlinePreLockoutTasks();

							RunUserAction_BeforeOffline_ForTest();
							((IUpgradeManager)this).StartTask("Switching Offline LockTimeout to Infinite");
							using (ErrorReporter.DeferReportingTemporarily())
							using (mainDbConnection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
							using (((IDbRecoveryModelManagerInternals)DbEnv.Instance.DbRecoveryModelManager).LoadAndCacheRequiredDbValues(mainDbConnection))
							{
								RunOfflineUpgrade();

								// RUNS FINALISING TASKS
								AfterSuccessfulRun();
							}
						}
						finally
						{
							RunUserAction_AfterOffline_ForTest();
							((IUpgradeManager)this).StartTask("Restoring Offline LockTimeout");
							if (clientHook != null && clientHook.HasCompanySpecificOverrides)
							{
								clientHook.IsUpgrading = false;
								if (clientHook.IsInitialised)
								{
									InvokeOrExecute(() => clientHook.Uninitialise());
								}
							}
						}
					}
					((IUpgradeManager)this).ShowInfoMessage("Released lock for upgrade");
				}
				else
				{
					const string message = "Database is locked for upgrade";
					((IUpgradeManager)this).ShowInfoMessage(GetDbExtPtyLockInfo(mainDbConnection, message));
					extraInformationForEmail = message;
					throw new UpgradeBlockedException(message, null);
				}
			}
			// Disposing ElevateToAdminConnectionForUpgrade will have disposed this
			mainDbConnection = null;
		}

		void CheckExistenceOfUpgradeInfo(DbConnection connection)
		{
			if (softwareUpgrade != null && !connection.Exists($"FROM dbo.StmUpgrade WHERE SZ_PK = '{softwareUpgrade.PK}'"))
			{
				throw new InvalidPackageException(Invariant($"Failed to apply the upgrade '{softwareUpgrade.PK}' because it does not exist."));
			}
		}

		public bool DisableAutoStatistics { get; set; }

		void SetAutoStatisticsForDatabase(DbConnection connection)
		{
			DisableAutoStatistics = DataUtils.ShouldDisableAutoStatisticsDuringUpgrade(connection, Db.DatabaseName);
			var isAutoCreateStatsOnForDb = connection.Exists($"FROM sys.databases WHERE name = N'{Db.DatabaseName.QuoteEscapedName('\'')}' AND is_auto_create_stats_on = 1");

			if (!DisableAutoStatistics && isAutoCreateStatsOnForDb)
			{
				((IUpgradeManager)this).ShowInfoMessage("Switching off AUTO_CREATE_STATISTICS");
				connection.ExecuteNonQuery($@"ALTER DATABASE {Db.DatabaseName.QuoteName()} SET AUTO_CREATE_STATISTICS OFF;");
			}
			else if (DisableAutoStatistics && !isAutoCreateStatsOnForDb)
			{
				((IUpgradeManager)this).ShowInfoMessage("Switching on AUTO_CREATE_STATISTICS");
				connection.ExecuteNonQuery($@"ALTER DATABASE {Db.DatabaseName.QuoteName()} SET AUTO_CREATE_STATISTICS ON;");
			}
		}

		void InvokeOrExecute(Action action)
		{
			if (SyncInvoke != null)
			{
				SyncInvoke.Invoke(action, Array.Empty<object>());
			}
			else
			{
				action();
			}
		}

		object InvokeOrExecute(Func<object> action)
		{
			if (SyncInvoke != null)
			{
				return SyncInvoke.Invoke(action, Array.Empty<object>());
			}
			else
			{
				return action();
			}
		}

		SqlApplicationLock TryGetUpgradeLockWithErrorHandling()
		{
			try
			{
				return TryGetUpgradeLock();
			}
			catch (SqlException ex) when (ex.Number == -2) // -2 is Execution Timeout Expired
			{
				const string message = "Database is busy";
				((IUpgradeManager)this).ShowInfoMessage(message);
				extraInformationForEmail = message;
				throw new UpgradeBlockedException(message, ex);
			}
		}

		protected virtual SqlApplicationLock TryGetUpgradeLock()
			=> mainDbConnection.TryGetLock(Constants.SystemUpgrade.DbUpgradeLock, out var sqlLock) ? sqlLock : null;

		internal static string GetDbExtPtyLockInfo(DbConnection connection, string message)
		{
			var lockInfo = DataUtils.LoadDbExtendedPropertyWithNoLock(connection, Constants.SystemUpgrade.DbUpgradeLockExtPty);
			return string.IsNullOrEmpty(lockInfo)
				? message
				: Invariant($"{message}{System.Environment.NewLine}Current lock holder:{System.Environment.NewLine}{lockInfo}");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:Do not use System.DateTime.UtcNow Rule", Justification = "Historical code")]
		internal static void WriteDbExtPtyLockInfo(DbConnection connection)
		{
			DataUtils.SaveDbExtendedProperty(
				connection,
				Constants.SystemUpgrade.DbUpgradeLockExtPty,
				Invariant($"User: '{Env.CurrentUser?.FullName}' started a system upgrade at utc time: {DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture)}"));
		}

		void ResetCdcIfRequired()
		{
			var cdcManager = new ChangeDataCaptureManager(mainDbConnection, AuditConnection, DataWarehouseConnection, this);
			cdcManager.DisableCdcAndCleanupBiDatabases();
		}

		void SetupOtherConnections()
		{
			SetAuditServerIfRequired();
			_ = AuditConnection;
			_ = DataWarehouseConnection;
		}

		/// <summary>
		/// Controls lockout out application connections and running DbUpgraders
		/// </summary>
		internal virtual void RunOfflineUpgrade()
		{
			var runFailed = true;
			var shouldAcquireLockoutInTransaction = ShouldAcquireLockoutInTransaction();
			IDisposable statisticsCollectorSuspender = null;
			UnhandledExceptionsAggregator
				.WithFinally(() => CleanupAfterOfflineUpgrade(statisticsCollectorSuspender, shouldAcquireLockoutInTransaction, runFailed))
				.OnUnhandledExceptions(LogUnhandledException)
				.Execute(() =>
				{
					if (requiresLockout)
					{
						PauseForOfflineUpgradeWarningPeriodAndConfirmContinue();

						if (!shouldAcquireLockoutInTransaction)
						{
							AcquireLegacyLockout();
						}
					}

					if (!shouldAcquireLockoutInTransaction)
					{
						KillUserConnections();
						CleanupAfterFirstKillUserConnections();
					}

					statisticsCollectorSuspender = PerformanceStatisticsCollector.SuspendStatisticsCollector();
					DoOfflinePreparationBeforeMainTransaction();

					if (!shouldAcquireLockoutInTransaction)
					{
						DoNonTransactionalStepsAfterFirstMainDbLockout();
					}

					RunUserAction_ForTest();
					RunRequiredUpgradeProcessesInATransaction(shouldAcquireLockoutInTransaction);
					RefreshDependentScriptsAfterOfflineUpgradeTransaction();

					new ChangeTrackingStatusManager().ReEnableChangeTrackToEnsureDataCleanup(mainDbConnection, this,
						ref extraInformationForEmail);
					new ServiceTasksSchedulesCleaner().DeleteSchedulesOfDecommissionedServiceTask(this);
					runFailed = false;
				});
		}

		void CleanupAfterOfflineUpgrade(IDisposable statisticsCollectorSuspender, bool shouldAcquireLockoutInTransaction, bool runFailed)
		{
			try
			{
				statisticsCollectorSuspender?.Dispose();
				if (DisableAutoStatistics)
				{
					StatisticsSwitch.Instance.CheckAndTurnStatistics(Db.DatabaseName, isON: true);
				}
				((IDbLoginRepair)mainDbConnection).CheckAndFixDbLoginSids();
				if (requiresLockout)
				{
					try
					{
						if (!shouldAcquireLockoutInTransaction)
						{
							ReleaseLegacyLockout(runFailed);
						}
					}
					finally
					{
						UpgWarning.ClearSystemUpgradeWarningPeriod();
					}
				}
				readonlyEdocDbKeeper?.RestoreReadonlyState(mainDbConnection);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				((IUpgradeManager)this).ShowTaskError(e.Message);
				ShowExceptionErrorDetails(e, this);
			}
		}

		void CleanupSemaphores(AdminConnection connection)
		{
			try
			{
				connection.ExecuteNonQuery("DELETE dbo.StmServiceSemaphore");
			}
			catch (SqlException)
			{
			}
		}

		void CleanupSqlMutexLocks(AdminConnection connection)
		{
			try
			{
				connection.ExecuteNonQuery("DELETE [dbo].[StmServiceMutex]");
			}
			catch (SqlException)
			{
			}
		}

		static void ShowExceptionErrorDetails(Exception e, IUpgradeTaskWorkflowLogger logger)
		{
			string messageDetails = null;
			Exception innerEx = e;
			while (innerEx != null)
			{
				if (innerEx is SqlException sqlEx)
				{
					messageDetails =
						"Error Number: " + sqlEx.Number.ToString() + "\r\n" +
						"Error Stack:\r\n" + sqlEx.ToString();
					break;
				}
				innerEx = innerEx.InnerException;
			}

			if (messageDetails == null)
			{
				messageDetails = e.ToString();
			}

			string message = "\r\n\r\n" +
						"-------------------------------------------------------------------\r\n" +
						"ERROR DETAILS\r\n" +
						"-------------------------------------------------------------------\r\n" +
						messageDetails + "\r\n" +
						"-------------------------------------------------------------------\r\n\r\n";

			logger.ShowInfoMessage(message);
		}

		#endregion // Run

		#region Implementation

		UpgradeActionList onlineActionList;
		protected UpgradeActionList offlineActionList;
		OnlinePreSchemaUpgrader onlinePreSchemaUpgrader;
		int applicationLockTimeoutInMs;

		partial void RunUserAction_ForTest();
		partial void RunUserAction_BeforeOnline_ForTest();
		partial void RunUserAction_Online_ForTest();
		partial void RunUserAction_AfterOnline_ForTest();
		partial void RunUserAction_BeforeOffline_ForTest();
		partial void RunUserAction_Offline_ForTest();
		partial void RunUserAction_AfterOffline_ForTest();

		protected virtual bool GetConfirmationToDisconnectUsers(KeyValuePair<string, string>[] loggedInUsers)
		{
			return (bool)InvokeOrExecute(() => UpgUtils.Instance.ShowConfirmDisconnectUsersMessageBox(loggedInUsers));
		}

		int NumberOfTasksBeforeOfflineActionList()
		{
			// CHECK SERVER REQUIREMENTS + ACQUIRE LOCKOUT + RELEASE LOCKOUT
			int totalTasks = 3;

			// Adds estimate number of tasks used in UpgraderManager
			totalTasks += 16;

			return totalTasks;
		}

		/// <summary>
		/// Calculates the total number of tasks involving all managed Upgraders
		/// </summary>
		void SetTotalNumberOfTasks()
		{
			try
			{
				int totalTasks = NumberOfTasksBeforeOfflineActionList();

				// Adds 2 (START + COMPLETION) from UpgraderManager for each required offline upgrade tasks
				totalTasks += offlineActionList.Count * 2;

				// Adds the number of tasks of each required offline upgrade tasks
				totalTasks += offlineActionList.EstimatedNumberOfTasks;

				// Adds 2 (START + COMPLETION) from UpgraderManager for each required online upgrade tasks
				totalTasks += onlineActionList.Count * 2;

				// Adds the number of tasks of each required online upgrade tasks
				totalTasks += onlineActionList.EstimatedNumberOfTasks;

				// Adds onlinePreSchemaUpgrader estimated number of tasks
				if (onlinePreSchemaUpgrader != null)
				{
					totalTasks += onlinePreSchemaUpgrader.EstimatedNumberOfTasks;
				}

				// Add task for committing software upgrade
				if (softwareUpgrade != null)
				{
					totalTasks++;
				}

				((IUpgradeManager)this).ActivateTaskProgress(totalTasks);
			}
			catch (Exception e)
			{
				throw new Exception("Failed to set number of tasks.\r\n" + e.Message);
			}
		}

		void LogUpgradeProcessInfo(bool afterRestart = false)
		{
			((IUpgradeManager)this).StartTask($"Applying version [{softwareUpgrade?.Version ?? ReleaseInfo.Instance.VersionNumber.ToVersion()}]");

			var currentSpid = 0;
			var hostName = "";
			var hostProcessId = 0;
			var programName = "";
			mainDbConnection.ExecuteReader(@"
SELECT
	session_id,
	host_name       = ISNULL(host_name, N''),
	host_process_id = ISNULL(host_process_id, 0),
	program_name    = ISNULL(program_name, N'')
FROM
	sys.dm_exec_sessions
WHERE 1=1
	AND session_id = @@SPID

"
				, (record) =>
				{
					currentSpid = (short)record["session_id"];
					hostName = (string)record["host_name"];
					hostProcessId = (int)record["host_process_id"];
					programName = (string)record["program_name"];
				});

			((IUpgradeManager)this).ShowInfoMessage($"Upgrade process SPID{(afterRestart ? " after restart" : "")}: [{currentSpid}]");
			((IUpgradeManager)this).ShowInfoMessage($"Client computer: [{hostName}]");
			((IUpgradeManager)this).ShowInfoMessage($"Client process: [{hostProcessId}]");
			((IUpgradeManager)this).ShowInfoMessage($"Client program: [{programName}]");
			((IUpgradeManager)this).ShowInfoMessage(".");
		}

		#region Check Server Requirements

		void CheckServerRequirements()
		{
			CheckMandatoryRequirements();
			CheckDbServerDiskSpace();
		}

		void CheckMandatoryRequirements()
		{
			try
			{
				Checker.CheckServerName(mainDbConnection);
				Checker.CheckServerVersion(mainDbConnection, this);
				Checker.CheckCollation(mainDbConnection);
				Checker.CheckDbNaming();
				Checker.CheckDotNet(mainDbConnection, this);
			}
			catch (Exception e)
			{
				throw new ServerRequirementsNotMetException("Server requirements not met: " + System.Environment.NewLine + e.Message, e.InnerException);
			}
		}

		void CheckDbServerDiskSpace()
		{
			string[] lowDiskSpaceDrives = Checker.GetLowDiskSpaceList(mainDbConnection);

			if (lowDiskSpaceDrives.Length > 0)
			{
				if (!GetConfirmationIfUserAttended("Database server requirements", "Insufficient disk space on database server.\r\nDo you wish to continue anyway?", lowDiskSpaceDrives))
				{
					throw new OdysseyException("Process aborted by the user due to low disk space on database server.");
				}
			}
		}

		protected MainServerRequirementChecker Checker
		{
			get
			{
				if (fChecker == null)
				{
					fChecker = new MainServerRequirementChecker(AllDatabasesBeingUpgraded, AllDatabases);
				}

				return fChecker;
			}
		}

		protected MainServerRequirementChecker fChecker;

		void CheckBiServersRequirements()
		{
			CheckBiServerRequirements(AuditConnection, Db.AuditDatabaseName, "Audit");
			CheckBiServerRequirements(DataWarehouseConnection, Db.EdwDatabaseName, "EDW");
		}

		void CheckBiServerRequirements(AdminConnection biConnection, string biDbName, string biDbType)
		{
			if (biConnection != null &&
				biConnection.ServerName != Db.ServerName)
			{
				((IUpgradeManager)this).StartTask($"Checking {biDbType} SQL Server requirements");

				var biDatabases = new[] { biDbName };
				var biChecker = new BiServerRequirementChecker(biDatabases);

				biChecker.CheckServerVersion(biConnection, this);
				biChecker.CheckCollation(biConnection);
			}
		}

		#endregion // Check Server Requirements

		/// <summary>
		/// Do the offline steps that run before the main db transaction has started.
		/// </summary>
		protected virtual void DoOfflinePreparationBeforeMainTransaction()
		{
			// Set readonly eDocs databases to writable during the upgrade if changes are needed.
			// Will be set back to readonly after.
			readonlyEdocDbKeeper = ReadonlyStoregeDocsKeeperFactory.New(versionInfo, mainDbConnection);
			readonlyEdocDbKeeper.AllowWrite(mainDbConnection);

			var upgradePreparation = CreateUpgradePreparation();
			upgradePreparation.ApplyServerConfigurationSettings();
			upgradePreparation.SetAutoStatsOnModelDatabase();
			upgradePreparation.AlterStorageDocDatabaseAutoCreateStatistics();
			if (DisableAutoStatistics)
			{
				upgradePreparation.TurnOffMainDbStatistics();
			}
			upgradePreparation.EnsureMainDatabaseSettings(this);
			upgradePreparation.AdjustAllDatabasesRecoveryModels(this);
			upgradePreparation.EnsureCargoWiseDatabaseSettings(AllDatabases);
			upgradePreparation.EnableCdc(this);
			upgradePreparation.ManageFilegroups();
		}

		protected virtual IUpgradePreparation CreateUpgradePreparation()
			=> new UpgradePreparation(mainDbConnection);

		/// <summary>
		/// Do the one-time steps that have to run outside a transaction, but after the offline lockout of the main database.
		/// The lockout will ensure there are no other processes changing the main database.
		///
		/// The mainDbConnection will be in the upgrade transaction at this point (except for upgrading from legacy lockout software)
		/// so another connection is used.
		///
		/// BI databases (Audit and EDW)
		/// ============================
		/// This method is called before the DatabasePadlock of the BI databases.
		/// If a BI database is on another server to the main DB, then internally this can use
		/// the AuditConnection/DataWarehouseConnection properties since those connections won't be in a transaction yet.
		/// However, if a BI database is on the same server as the main DB a new connection is needed
		/// since those connections are actually referring to the same connection as mainDbConnection.
		/// </summary>
		protected virtual void DoNonTransactionalStepsAfterFirstMainDbLockout()
		{
			DoNonTransactionalBiPreparation(isAudit: true);
			DoNonTransactionalBiPreparation(isAudit: false);
		}

		void DoNonTransactionalBiPreparation(bool isAudit)
		{
			var biConnection = isAudit ? AuditConnection : DataWarehouseConnection;
			if (biConnection != null && BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(mainDbConnection, biConnection))
			{
				IDisposable disposable = null;
				if (biConnection.IsInTransaction)
				{
					biConnection = Db.NewAdminConnection(biConnection.ServerName, Db.SqlMasterDb);
					disposable = biConnection;
				}

				using (disposable)
				{
					// Note, deliberately passing mainDbConnection even though it is in a transaction and
					// this method is for non-transactional steps, since the Preparation uses it to read registry, etc
					// and only the mainDbConnection has registry access at this point.
					var upgradePreparation = isAudit
						? (BusinessIntelligenceUpgradePreparation)new BiAuditUpgradePreparation(mainDbConnection, biConnection, versionInfo)
						: new BiDataWarehouseUpgradePreparation(mainDbConnection, biConnection, versionInfo);
					var msg = isAudit
						? "Running Audit upgrade preparation"
						: "Running EDW upgrade preparation";
					((IUpgradeManager)this).ShowInfoMessage(msg);
					upgradePreparation.PerformNonTransactionalSettings(this);
				}
			}
		}

		#region After Running Upgrade

		void AfterSuccessfulRun()
		{
			try
			{
				UpdateSoftwareUpgradeVersion();
			}
			finally
			{
				RunUpgradeConclusionSteps();
			}
		}

		void UpdateSoftwareUpgradeVersion()
		{
			if (softwareUpgrade != null)
			{
				((IUpgradeManager)this).StartTask("Updating local current version");
				SoftwareUpgrader.Instance.RunCurrentVersionWriter(softwareUpgrade, this);
			}
		}

		void RunUpgradeConclusionSteps()
		{
			try
			{
				if (AuditConnection != null)
				{
					try
					{
						CreateAuditUpgradeConclusion().RunAfterUpgradeSteps(this, this);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						((IUpgradeManager)this).ShowInfoMessage($"Audit upgrade failed: {ex.Message}");
					}
				}

				if (DataWarehouseConnection != null)
				{
					try
					{
						CreateDataWarehouseUpgradeConclusion().RunAfterUpgradeSteps(this, this);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						((IUpgradeManager)this).ShowInfoMessage($"Data warehouse upgrade failed: {ex.Message}");
					}
				}

				// Ensure UpgradeConclusion always runs
				new UpgradeConclusion(mainDbConnection).RunAfterUpgradeSteps(this, this);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				((IUpgradeManager)this).ShowInfoMessage(ex.Message);
				extraInformationForEmail = ex.ToString();
			}
		}

		IReadonlyStoregeDocsKeeper readonlyEdocDbKeeper;

		#endregion

		#region Run Required Upgrades

		void RunRequiredUpgradeProcessesInATransaction(bool shouldAcquireLockoutInTransaction)
		{
			var transactionManager = shouldAcquireLockoutInTransaction
				? BeginTransactionsAndSetup()
				: LegacyLockout_BeginTransactionAndLockResources();

			UnhandledExceptionsAggregator
				.Using(transactionManager)
				.OnUnhandledExceptions(LogUnhandledException)
				.Execute(() =>
				{
					RunTransactionalUpgradeProcesses();

					if (softwareUpgrade != null)
					{
						SoftwareUpgrader.Instance.CommitSoftwareUpgradeInDatabase(softwareUpgrade, this);
					}

					if (requiresLockout && shouldAcquireLockoutInTransaction)
					{
						DbLockout.ResetTransactionLockout(mainDbConnection);
					}

					transactionManager.CommitTransaction();
				});
		}

		/// <summary>
		/// Begin the upgrade transaction and perform initial setup like locking out other connections.
		///
		/// This is not used for a legacy lockout. That's in <see cref="LegacyLockout_BeginTransactionAndLockResources"/>
		/// </summary>
		internal ITransactionManager BeginTransactionsAndSetup()
		{
			((IUpgradeManager)this).StartTask("Beginning transaction");

			TransactionManager result = null;
			var tryCount = 10;
			var locksTaken = false;
			var firstTry = true;
			while (!locksTaken && tryCount-- > 0)
			{
				locksTaken = true;
				result = new TransactionManager(this);

				try
				{
					result.BeginTransaction(mainDbConnection);
					if (requiresLockout)
					{
						// WARNING: this will lock ALL extended property metadata
						// After this, other connections will not be able set any extended properties
						// and they will only be able to read them WITH(NOLOCK).
						((IUpgradeManager)this).StartTask("Locking application access to the database");
						locksTaken = DbLockout.AcquireTransactionLockout(mainDbConnection);
					}

					if (locksTaken)
					{
						KillUserConnections();
						if (firstTry)
						{
							CleanupAfterFirstKillUserConnections();
							DoNonTransactionalStepsAfterFirstMainDbLockout();
							firstTry = false;
						}

						if (AuditConnection != null)
						{
							result.BeginTransaction(AuditConnection, () => CreateAuditUpgradeConclusion().CleanupBiDatabase(this));
						}
						if (DataWarehouseConnection != null)
						{
							result.BeginTransaction(DataWarehouseConnection, () => CreateDataWarehouseUpgradeConclusion().CleanupBiDatabase(this));
						}

						locksTaken = EnsureExclusiveLockOfResourcesIfRequired();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					try
					{
						result.RollbackTransaction();
					}
					catch (Exception)
					{
						// skip any exceptions here and throw the original exception out
					}

					throw;
				}

				if (!locksTaken)
				{
					result.RollbackTransaction();
					if (tryCount > 0)
					{
						KillUserConnections();
					}
				}
			}

			if (tryCount >= 0)
			{
				return result;
			}
			else
			{
				throw new UpgradeBlockedException("Cannot obtain exclusive locks on database resources");
			}
		}

		BusinessIntelligenceUpgradeConclusion CreateAuditUpgradeConclusion()
			=> new BiAuditUpgradeConclusion(mainDbConnection, AuditConnection, IsProductRegistrationValid());

		BusinessIntelligenceUpgradeConclusion CreateDataWarehouseUpgradeConclusion()
			=> new BiDataWarehouseUpgradeConclusion(mainDbConnection, DataWarehouseConnection);

		#region Legacy Lockout code - for upgrading from old software

		internal ITransactionManager LegacyLockout_BeginTransactionAndLockResources()
		{
			((IUpgradeManager)this).StartTask("Beginning transaction");

			var connections = new List<DbConnection>()
			{
				mainDbConnection
			};

			if (AuditConnection != null)
			{
				connections.Add(AuditConnection);
			}

			if (DataWarehouseConnection != null)
			{
				connections.Add(DataWarehouseConnection);
			}

			ITransactionManager result = null;
			var tryCount = 10;
			var locksTaken = false;
			while (!locksTaken && tryCount-- > 0)
			{
				result = new LegacyLockout_TransactionManager(this, connections);

				try
				{
					locksTaken = EnsureExclusiveLockOfResourcesIfRequired();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					try
					{
						result.RollbackTransaction();
					}
					catch (Exception)
					{
						// skip any exceptions here and throw the original exception out
					}

					throw;
				}

				if (!locksTaken)
				{
					result.RollbackTransaction();
					if (tryCount > 0)
					{
						KillUserConnections();
					}
				}
			}

			if (tryCount >= 0)
			{
				return result;
			}
			else
			{
				throw new UpgradeBlockedException("Cannot obtain exclusive locks on database resources");
			}
		}

		#endregion

		void RunTransactionalUpgradeProcesses()
		{
			RunUserAction_Offline_ForTest();
			RunUpgradeActions(offlineActionList);

			AfterUpgradeTransactionalTasks();
		}

		protected virtual bool EnsureExclusiveLockOfResourcesIfRequired()
		{
			var result = true;

			if (requiresLockout && versionInfo.IsRequired_Schema)
			{
				((IUpgradeManager)this).StartNonEstimatedTask("Putting exclusive locks on database resources");

				result = PutExclusiveLockOnDatabaseResources(mainDbConnection, AllDatabasesBeingUpgraded);

				if (result && AuditConnection != null)
				{
					result = PutExclusiveLockOnDatabaseResources(AuditConnection, new[] { Db.AuditDatabaseName });
				}

				if (result && DataWarehouseConnection != null)
				{
					result = PutExclusiveLockOnDatabaseResources(DataWarehouseConnection, new[] { Db.EdwDatabaseName });
				}
			}

			return result;
		}

		bool PutExclusiveLockOnDatabaseResources(AdminConnection padlockConnection, IEnumerable<string> databasesToLock)
		{
			var logger = (IUpgradeManager)this;
			logger.StartSubtask(string.Join(", ", databasesToLock));

			var waitInMs = 3000;
			var observer = new UpgradeObserver(applicationLockTimeoutInMs, waitInMs, padlockConnection.SPID);
			using (observer.Run(feedbackMethod: (message) => ((IUpgradeManager)this).ShowInfoMessage(message)))
			{
				return new DatabasePadlock().LockDatabaseResources(
					padlockConnection,
					databasesToLock,
					feedbackMethod: (message) => logger.ShowInfoMessage("\t" + message)
				);
			}
		}

		void AfterUpgradeTransactionalTasks()
		{
			var transactionalTaskRunner = new TransactionalAfterUpgradeTasks(
				mainDbConnection,
				logger: this,
				isRunningSchemaOrScriptUpgrade: (versionInfo.IsRequired_Schema || versionInfo.IsRequired_Script));
			transactionalTaskRunner.RunTasks();
		}

		#endregion

		#region Refresh Dependent Scripts

		void PrepareRefreshDependentScriptsAfterOfflineUpgradeTransaction(SchemaUpgrader schemaUpgrader)
			=> RefreshDependentScriptsAction = new Action(() => schemaUpgrader.RefreshDependentScripts());

		void RefreshDependentScriptsAfterOfflineUpgradeTransaction()
		{
			if (!DisableRefreshDependentScripts_ForTest)
			{
				RefreshDependentScriptsAction?.Invoke();
			}
		}

		internal Action RefreshDependentScriptsAction { get; set; }
		internal bool DisableRefreshDependentScripts_ForTest { get; set; }

		#endregion

		#region Online Upgrade Tasks - before upgrade lockout is acquired

		void RunOnlinePreLockoutTasks()
		{
			RunUserAction_BeforeOnline_ForTest();

			applicationLockTimeoutInMs = DbRegistry.LockTimeout.LoadValue(mainDbConnection);

			if (onlinePreSchemaUpgrader != null || onlineActionList.Count > 0)
			{
				((IUpgradeManager)this).StartTask("Switching LockTimeout to Infinite");

				using (mainDbConnection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				using (AuditConnection?.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				using (DataWarehouseConnection?.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				{
					var upgradeObserverMaxWaitInSeconds = Env.Registry.UpgradeObserverMaxWaitInSeconds;

					if (upgradeObserverMaxWaitInSeconds >= 0)
					{
						((IUpgradeManager)this).StartTask("Using On-line Upgrade Observer");

						var currentWaitInMs = upgradeObserverMaxWaitInSeconds * 1000;

						Observer = new UpgradeObserver(applicationLockTimeoutInMs, currentWaitInMs, mainDbConnection.SPID);
						using (Observer.Run(feedbackMethod: (message) => ((IUpgradeManager)this).ShowInfoMessage(message)))
						{
							RunOnlineUpgrade();
						}
					}
					else
					{
						RunOnlineUpgrade();
					}
				}

				((IUpgradeManager)this).StartTask("Restoring LockTimeout");
			}

			RunUserAction_AfterOnline_ForTest();
		}

		void RunOnlineUpgrade()
		{
			((IUpgradeManager)this).ShowInfoMessage(".");

			if (onlinePreSchemaUpgrader != null)
			{
				onlineActionList.ForEach(action => onlinePreSchemaUpgrader.UpgradeActions.Add(action));
			}

			var retryCount = GetOnlineUpgradeRetryCount();
			while (!RunOnlineUpgradeWithRetry(retryCount--))
			{
				// On-line upgrade has been re-started due to exception
				LogUpgradeProcessInfo(afterRestart: true);
			}

			((IUpgradeManager)this).ShowInfoMessage(".");
		}

		bool RunOnlineUpgradeWithRetry(int retryCount)
		{
			try
			{
				RunUserAction_Online_ForTest();

				if (onlinePreSchemaUpgrader != null)
				{
					onlinePreSchemaUpgrader.RunUpgrade();
				}
				else
				{
					RunUpgradeActions(onlineActionList);
				}
			}
			catch (Exception ex) when (retryCount > 0 && ex.IsInnermostDeadlock())
			{
				((IUpgradeManager)this).ShowInfoMessage("On-line upgrade has been re-started due to deadlock");

				return false;
			}
			catch (Exception ex) when (retryCount > 0 && ex.IsInnermostSpecifiedError(DbErrorType.CouldNotObtainExclusiveLock))
			{
				((IUpgradeManager)this).ShowInfoMessage("On-line upgrade has been re-started due to failure to obtain an exclusive lock.");

				return false;
			}

			return true;
		}

		internal int GetOnlineUpgradeRetryCount()
		{
			if (DataUtils.ObjectExists(mainDbConnection, ExtProperty.TableName))
			{
				var value = ExtProperty.Database.Select(mainDbConnection, nameof(OnlineUpgradeRetryCount));
				if (int.TryParse(value, out var result))
				{
					return result;
				}
			}

			return OnlineUpgradeRetryCount;
		}

		internal int OnlineUpgradeRetryCount { get; } = 3;

		UpgradeObserver Observer { get; set; }

		void RunUpgradeActions(UpgradeActionList actionList)
		{
			int progressValue = NumberOfTasksBeforeOfflineActionList();

			foreach (var action in actionList)
			{
				((IUpgradeManager)this).ShowInfoMessage(".");
				((IUpgradeManager)this).StartTask("--- " + action.Name + " - START ---");

				action.Run();

				((IUpgradeManager)this).StartTask("--- " + action.Name + " - END   ---");
				((IUpgradeManager)this).ShowInfoMessage(".");

				progressValue += action.EstimatedNumberOfTasks + 2;
				((IUpgradeManager)this).UpdateCurrentProgress(progressValue);
			}
		}

		#endregion // Online Upgrade Tasks - before upgrade lockout is acquired

		#region Abstract Properties Implementation

		public override VersionLabel SchemaVersionBeforeUpgrade
		{
			get { return schemaVersionBeforeUpgrade; }
		}

		readonly VersionLabel schemaVersionBeforeUpgrade;

		public override VersionLabel TransformationVersionBeforeUpgrade
		{
			get { return transformationVersionBeforeUpgrade; }
		}

		readonly VersionLabel transformationVersionBeforeUpgrade;

		#endregion

		#region Lockout

		internal bool requiresLockout;

		void PauseForOfflineUpgradeWarningPeriodAndConfirmContinue()
		{
			var pausePeriod = UpgWarning.SetSystemUpgradeWarningPeriodAndPause(this);

			if (Globals.IsUserInteractive && pausePeriod == 0 && !GetConfirmationToKillOtherDbConnections())
			{
				throw new DeniedByUserException();
			}
		}

		/// <summary>
		/// Obtain a legacy lockout, which is a lockout taken before the upgrade transaction begins.
		/// It's used when upgrading from older software,
		/// since that can't read the lockout extended property when it's locked inside a transaction.
		/// </summary>
		internal void AcquireLegacyLockout()
		{
			var shouldDisableLogins = ShouldDisableLogins();

			((IUpgradeManager)this).StartTask("Locking application access to the database" + (!shouldDisableLogins ? " with logins enabled" : ""));
			var lockoutState = DbLockout.AcquireLockout(mainDbConnection, LockoutReason.Upgrade, disableLogins: shouldDisableLogins);

			if (lockoutState == DbLockoutState.AquiredLockout && shouldDisableLogins)
			{
				if (AuditConnection != null &&
					!string.Equals(mainDbConnection.ServerNameReportedByDatabase, AuditConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
				{
					((IDbLockout)AuditConnection).DisableApplicationDbLogins();
				}

				if (DataWarehouseConnection != null &&
					!string.Equals(mainDbConnection.ServerNameReportedByDatabase, DataWarehouseConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(DataWarehouseConnection.ServerNameReportedByDatabase, AuditConnection?.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
				{
					((IDbLockout)DataWarehouseConnection).DisableApplicationDbLogins();
				}
			}
			else if (lockoutState != DbLockoutState.AquiredLockout)
			{
				throw new Exception("Failed to lock application access to the database [" + Db.DatabaseName + "].");
			}
		}

		/// <summary>
		/// Cleanup after all the other connections have been killed so even if the upgrade fails,
		/// users and system services can reconnect without problems.
		/// Internally will ensure the cleanup runs in a connection not in a transaction.
		/// Must be called before the padlock is taken so the relevant tables are not locked.
		/// </summary>
		protected virtual void CleanupAfterFirstKillUserConnections()
		{
			var connectionNotInTransaction = mainDbConnection;
			IDisposable disposable = null;
			if (connectionNotInTransaction.IsInTransaction)
			{
				connectionNotInTransaction = Db.NewAdminConnection();
				connectionNotInTransaction.IsUpgradeCheckDisabled = true;
				disposable = connectionNotInTransaction;
			}

			UnhandledExceptionsAggregator
				.Using(disposable)
				.OnUnhandledExceptions(LogUnhandledException)
				.Execute(() =>
				{
					CleanupSemaphores(connectionNotInTransaction);
					CleanupSqlMutexLocks(connectionNotInTransaction);
				});
		}

		protected virtual void KillUserConnections()
		{
			((IUpgradeManager)this).StartSubtask("Killing user connections");
			var loginCutoffTime = DbConnectionKiller.GetSqlServerDateForLoginCutoff(mainDbConnection);
			Thread.Sleep(TimeSpan.FromSeconds(2));
			KillUserConnectionsToSpecificDatabase(mainDbConnection, Db.DatabaseName, loginCutoffTime);

			if (AuditConnection != null)
			{
				KillUserConnectionsToSpecificDatabase(AuditConnection, Db.AuditDatabaseName, loginCutoffTime);
			}

			if (DataWarehouseConnection != null)
			{
				KillUserConnectionsToSpecificDatabase(DataWarehouseConnection, Db.EdwDatabaseName, loginCutoffTime);
			}
		}

		void KillUserConnectionsToSpecificDatabase(AdminConnection adminConnection, string databaseName, DateTime loginCutoffTime)
		{
			var logger = (IUpgradeManager)this;
			logger.ShowInfoMessage("\t" + databaseName);

			var writeLog = true;
			var timer = Stopwatch.StartNew();

			Action<string> feedBackAction = (message) =>
			{
				if (writeLog)
				{
					logger.ShowInfoMessage("\t\t" + message);
					timer.Restart();
				}

				writeLog = (timer.Elapsed >= TimeSpan.FromMinutes(1));
			};

			if (adminConnection == mainDbConnection && !adminConnection.IsInTransaction)
			{
				using (adminConnection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				{
					DbConnectionKiller.KillOtherConnections(adminConnection, databaseName, feedBackAction, loginCutoffTime);
				}
			}
			else
			{
				DbConnectionKiller.KillOtherConnectionsUsingAuxiliaryConnection(databaseName, adminConnection, feedBackAction, loginCutoffTime, Timeout.InfiniteTimeSpan);
			}
		}

		void ReleaseLegacyLockout(bool runFailed)
		{
			((IUpgradeManager)this).StartTask("Unlocking application access to the database");

			int i = 1;
			while (true)
			{
				try
				{
					Db.ResetLockout(mainDbConnection, AuditConnection, DataWarehouseConnection);
					break;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					((IUpgradeManager)this).ShowInfoMessage(string.Format("Could not unlock application access to the database. Attempt {0}. Reason:\r\n{1}", i, e.Message));
					i++;

					if (ShouldRetryToReleaseLockout)
					{
						Thread.Sleep(RetryWaitingPeriod);
					}
					else
					{
						throw;
					}
				}
			}
		}

		SystemUpgradeWarning UpgWarning
		{
			get { return upgWarning_UsePtyInstead ?? (upgWarning_UsePtyInstead = new SystemUpgradeWarning(mainDbConnection)); }
		}
		SystemUpgradeWarning upgWarning_UsePtyInstead;

		protected virtual bool ShouldRetryToReleaseLockout
		{
			get { return UpgUtils.Instance.ShowRetryConfirmationMessageBox("Unsuccessfull lock release", "Application access to the database could not be unlocked. Do you want to retry?"); }
		}

		protected virtual TimeSpan RetryWaitingPeriod
		{
			get { return TimeSpan.Zero; }
		}

		/// <summary>
		/// Gets user confirmation to disconnect other users from the database.
		/// </summary>
		/// <returns>
		///   TRUE  - if confirmed to kill other processes
		///   FALSE - if the user chose to cancel upgrade
		/// </returns>
		internal virtual bool GetConfirmationToKillOtherDbConnections()
		{
			var activeDbUsers = GetActiveDbUsers();

			// If there are other user processes connected to the database
			if (activeDbUsers.Length > 0)
			{
				return GetConfirmationToDisconnectUsers(activeDbUsers);
			}

			return true;
		}

		/// <summary>
		/// Gets active users information.
		/// Note: Caches exception and try use safe method to get active users information.
		/// </summary>
		/// <returns>Array of active users descriptions</returns>
		KeyValuePair<string, string>[] GetActiveDbUsers()
		{
			KeyValuePair<string, string>[] activeDbUsers;

			try
			{
				var userSessions = ActiveUserQuery.GetActiveUserSessions(false, mainDbConnection).Where(x => x.UserType == LogonType.Staff).ToList();
				activeDbUsers = new KeyValuePair<string, string>[userSessions.Count];

				for (int i = 0; i < userSessions.Count; i++)
				{
					var session = userSessions[i];
					string userValue = string.Format("{0} - {1} - {2}", session.FullName, session.ComputerName, session.ProcessId.ToString());
					activeDbUsers[i] = new KeyValuePair<string, string>(session.LogonIdentificationCode, userValue);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				activeDbUsers = GetAlternativeActiveDbUsersInformation();
			}

			return activeDbUsers;
		}

		/// <summary>
		/// Gets active users information using alternative safe way
		/// </summary>
		KeyValuePair<string, string>[] GetAlternativeActiveDbUsersInformation()
		{
			string sqlText = string.Format(@"
				SELECT rtrim(program_name) + ' - ' + rtrim(host_name) + ' - ' + rtrim(login_name)
				FROM sys.dm_exec_sessions
				WHERE host_name != '' AND database_id = db_id('{0}') AND session_id != @@spid",
				Db.DatabaseName);

			DataTable usersTable = Utilities.GetDataTableFromQuery(sqlText);
			KeyValuePair<string, string>[] users = new KeyValuePair<string, string>[usersTable.Rows.Count];
			int i = 0;

			foreach (DataRow row in usersTable.Rows)
			{
				users[i++] = new KeyValuePair<string, string>("", Utilities.GetStringFromObject(row[0]));
			}

			return users;
		}

		internal protected virtual bool ShouldAcquireLockoutInTransaction()
		{
			const int LastMajorSchemaVersionWithLockoutOutsideTransaction = Prod_LastMajorSchemaVersionThatDisabledLogins;
			return schemaVersionBeforeUpgrade.Major > LastMajorSchemaVersionWithLockoutOutsideTransaction;
		}

		// Schema version comes from Enterprise.DbUpgrader.Resource.SchemaVersion.ApplicationMajor
		// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared?path=%2FCargoWise.DbUpgrader%2Fsrc%2FResource%2FResource%2FSchemaVersion.cs&_a=contents&version=GBmaster
		internal const int Test_LastMajorSchemaVersionThatDisabledLogins = 7227;
		internal const int Prod_LastMajorSchemaVersionThatDisabledLogins = 7337;

		internal bool ShouldDisableLogins()
		{
			// For now, only internal test systems don't disable logins.
			var registration = ObjectFactory.Get<IProductRegistration>();
			var isInternalTestSystem = registration.IsWiseTechGlobalInternalSystem() && registration.Key.DatabaseType != DatabaseTypes.Codes.Production;

			var lastMajorSchemaVersionThatDisabledLogins = isInternalTestSystem
				? Test_LastMajorSchemaVersionThatDisabledLogins
				: Prod_LastMajorSchemaVersionThatDisabledLogins;

			return schemaVersionBeforeUpgrade.Major <= lastMajorSchemaVersionThatDisabledLogins;
		}

		#endregion

		#region Database Lists

		/// <summary>
		/// List of all databases in the system, whether or not they are being upgraded.
		/// </summary>
		protected IEnumerable<string> AllDatabases
		{
			get { return mainDbConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW); }
		}

		/// <summary>
		/// List of Databases involved in the upgrade process (including the Main one)
		/// </summary>
		protected List<string> AllDatabasesBeingUpgraded
		{
			get
			{
				if (allDatabasesBeingUpgraded == null)
				{
					allDatabasesBeingUpgraded = new List<string>
					{
						Db.DatabaseName
					};
					allDatabasesBeingUpgraded.AddRange(SecondaryDatabasesBeingUpgraded);
				}

				return allDatabasesBeingUpgraded;
			}
		}
		List<string> allDatabasesBeingUpgraded;

		/// <summary>
		/// List of Databases involved in the upgrade process besides the Main one
		/// </summary>
		protected List<string> SecondaryDatabasesBeingUpgraded
		{
			get
			{
				if (secondaryDatabasesBeingUpgraded == null)
				{
					secondaryDatabasesBeingUpgraded = offlineActionList.SecondaryDatabasesToUpgrade.ToList();
				}

				return secondaryDatabasesBeingUpgraded;
			}
		}
		List<string> secondaryDatabasesBeingUpgraded;

		#endregion

		#endregion

		#region Helper classes

		/// <summary>
		/// A transaction manager that supports adding transactions one at a time.
		/// Each can have it's own rollback action.
		/// </summary>
		sealed class TransactionManager : BaseTransactionManager<UpgradeManager>
		{
			public TransactionManager(UpgradeManager owner) : base(owner)
			{
			}

			readonly List<ITransactionManager> innerTransactions = new List<ITransactionManager>();
			readonly List<Action> rollbackActionList = new List<Action>();

			public void BeginTransaction(ITransactionStarter transaction, Action rollbackAction = null)
			{
				innerTransactions.Add(transaction.BeginTransactionWithManager());
				if (rollbackAction != null)
				{
					rollbackActionList.Add(rollbackAction);
				}
			}

			protected override void Commit()
			{
				if (innerTransactions.Count == 0)
				{
					return;
				}

				((IUpgradeManager)owner).StartTask("Committing transaction");
				try
				{
					foreach (var transaction in innerTransactions)
					{
						transaction.CommitTransaction();
					}
				}
				catch (Exception commitEx)
				{
					InvokeAllAndGatherExceptions(commitEx, innerTransactions.Select(t => new Action(() => t.RollbackTransaction())).ToArray());
				}
			}

			protected override void Rollback()
			{
				if (innerTransactions.Count == 0)
				{
					return;
				}

				((IUpgradeManager)owner).StartNonEstimatedTask("\r\n\r\nUpgrade failed - Rolling back transaction");
				InvokeAllAndGatherExceptions(innerTransactions.Select(t => new Action(() => t.RollbackTransaction())).ToArray());
				foreach (var rollbackAction in rollbackActionList)
				{
					rollbackAction();
				}
			}

			protected override void Dispose(bool isDisposing)
				=> InvokeAllAndGatherExceptions(() => base.Dispose(isDisposing), () => DisposeTransactions(innerTransactions));

			void DisposeTransactions(IEnumerable<ITransactionManager> transactions)
				=> InvokeAllAndGatherExceptions(transactions.Select(t => new Action(() => t.Dispose())).ToArray());
		}

		class LegacyLockout_TransactionManager : MultiTransactionManager<UpgradeManager>
		{
			public LegacyLockout_TransactionManager(UpgradeManager owner, IEnumerable<ITransactionStarter> transactionStarters) : base(owner, transactionStarters)
			{
			}

			protected override void Commit()
			{
				((IUpgradeManager)owner).StartTask("Committing transaction");
				base.Commit();
			}

			protected override void Rollback()
			{
				((IUpgradeManager)owner).StartNonEstimatedTask("\r\n\r\nUpgrade failed - Rolling back transaction");
				base.Rollback();
				if (owner.AuditConnection != null)
				{
					owner.CreateAuditUpgradeConclusion().CleanupBiDatabase(owner);
				}
				if (owner.DataWarehouseConnection != null)
				{
					owner.CreateDataWarehouseUpgradeConclusion().CleanupBiDatabase(owner);
				}
			}
		}

		partial class UpgradeObserver
		{
			internal UpgradeObserver(int applicationLockTimeout, int currentWaitInMs, int mainSPID)
			{
				ApplicationLockTimeout = applicationLockTimeout;
				MaxWait = TimeSpan.FromMilliseconds(currentWaitInMs);

				Interval = (applicationLockTimeout >= 0 && applicationLockTimeout < currentWaitInMs)
					? TimeSpan.FromMilliseconds(applicationLockTimeout)
					: MaxWait
					;

				MainSPID = mainSPID;
			}

			internal IDisposable Run(Action<string> feedbackMethod)
			{
				var cancellationSource = new CancellationTokenSource();
				var cancellationToken = cancellationSource.Token;
				Task task;

				using (var startEvent = new AutoResetEvent(false))
				{
					task = Task.Run(() =>
					{
						using (var observerConnection = Db.NewAdminConnection(Db.SqlMasterDb))
						{
							startEvent.Set();

							var forceKill = false;
							while (forceKill || !cancellationToken.WaitHandle.WaitOne(Interval))
							{
								forceKill = CheckBlockers(observerConnection, forceKill, feedbackMethod);
							}
						}
					});

					startEvent.WaitOne();
				}

				return new DisposableAction(() =>
				{
					cancellationSource.Cancel();
					Task.WaitAll(new[] { task });
					cancellationSource.Dispose();
				});
			}

			#region Implementation

			int ApplicationLockTimeout { get; }
			TimeSpan MaxWait { get; }
			TimeSpan Interval { get; }
			int MainSPID { get; }

			bool CheckBlockers(AdminConnection observerConnection, bool forceKill, Action<string> feedbackMethod)
			{
				var sql = @"-- On-line Upgrade Observer
DECLARE
	@spid          int
	, @kill_spid   nvarchar(1000)
	, @wait_type   nvarchar(60)
	, @kill_reason nvarchar(100)

;WITH
	waits AS (
			SELECT TOP (1)
				*
			FROM
				sys.dm_os_waiting_tasks
			WHERE 1=1
				AND blocking_session_id > 0
				AND blocking_session_id <> session_id
				AND blocking_session_id <> @@spid
				AND session_id = @main_spid
				AND
				(
					CASE
						WHEN @force_kill = 1 THEN 1
						WHEN wait_duration_ms >= @interval_ms THEN 1
						ELSE 0
					END = 1
				)
			ORDER BY
				wait_duration_ms DESC
		)
	, blockers AS (
			SELECT
				w.blocking_session_id
				, w.wait_type
				, kill_reason =
					CASE
						WHEN @force_kill = 1 THEN N'(forced)'
						WHEN w.wait_duration_ms >= @max_wait_ms THEN N'after wait'
						ELSE 'immediately'
					END
			FROM
				waits AS w
			WHERE 1=1
				AND
				(
					CASE
						WHEN @force_kill = 1 THEN 1
						WHEN w.wait_duration_ms >= @max_wait_ms THEN 1
						WHEN EXISTS(SELECT NULL FROM sys.dm_exec_requests AS r WHERE r.blocking_session_id = w.session_id) THEN 1
						ELSE 0
					END = 1
				)
		)
SELECT TOP (1)
	@spid          = b.blocking_session_id
	, @kill_spid   = CONCAT(N'BEGIN TRY KILL ', b.blocking_session_id, N'; END TRY BEGIN CATCH if (ERROR_NUMBER() NOT in (6106, 6107, 6120, 6109)) THROW; END CATCH;')
	, @wait_type   = wait_type
	, @kill_reason = kill_reason
FROM
	blockers                  AS b
	JOIN sys.dm_exec_sessions AS s ON s.session_id = b.blocking_session_id
WHERE 1=1
	AND s.is_user_process = 1

if (@kill_spid is NOT NULL)
begin
	EXEC (@kill_spid);
end

SELECT
	SpidKilled = ISNULL(@spid, 0),
	KillReason = ISNULL(@kill_reason, N''),
	WaitType   = ISNULL(@wait_type, N'')

";

				var spidKilled = 0;
				var killReason = "";
				var waitType = "";

				observerConnection.ExecuteReader(sql
					, cmd =>
					{
						cmd.AddParameter("@force_kill", SqlDbType.Bit, forceKill);
						cmd.AddParameter("@main_spid", SqlDbType.SmallInt, MainSPID);
						cmd.AddParameter("@interval_ms", SqlDbType.BigInt, (int)Interval.TotalMilliseconds);
						cmd.AddParameter("@max_wait_ms", SqlDbType.BigInt, (int)MaxWait.TotalMilliseconds);
					}
					, reader =>
					{
						spidKilled = (int)reader["SpidKilled"];
						killReason = (string)reader["KillReason"];
						waitType = (string)reader["WaitType"];
					});

				if (spidKilled > 0)
				{
					feedbackMethod?.Invoke($"Client SPID {spidKilled} has been killed {killReason} due to blocking upgrade process (WaitType: {waitType}, ApplicationLockTimeout: {ApplicationLockTimeout:N0} millisecond(s), MaxWait: {(int)MaxWait.TotalSeconds:N0} second(s))");

					var blockers = new List<DbConnectionKiller.ConnectionInfo>() { new DbConnectionKiller.ConnectionInfo(spidKilled.ToString()) };
					DbConnectionKiller.PendingRollback(observerConnection, blockers, feedbackMethod);

					Thread.Sleep(100);
					return true;
				}

				return false;
			}

			#endregion // Implementation
		}

		#endregion // Helper classes
	}

	[Serializable]
	class DeniedByUserException : Exception
	{
		public DeniedByUserException()
		{
		}

		public DeniedByUserException(string message) : base(message)
		{
		}

		public DeniedByUserException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DeniedByUserException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

#region Test
#if DEBUG

#region Partial class

namespace Enterprise.DbUpgrader.Startup
{
	public partial class UpgradeManager
	{
		public Action UserAction_ForTest { get; set; }
		partial void RunUserAction_ForTest()
		{
			UserAction_ForTest?.Invoke();
		}

		public void SetConnection_ForTest(AdminConnection testConnection)
		{
			mainDbConnection = testConnection;
		}

		public Action UserAction_BeforeOffline_ForTest { get; set; }
		partial void RunUserAction_BeforeOffline_ForTest()
		{
			UserAction_BeforeOffline_ForTest?.Invoke();
		}

		public Action UserAction_Offline_ForTest { get; set; }
		partial void RunUserAction_Offline_ForTest()
		{
			UserAction_Offline_ForTest?.Invoke();
		}

		public Action UserAction_AfterOffline_ForTest { get; set; }
		partial void RunUserAction_AfterOffline_ForTest()
		{
			UserAction_AfterOffline_ForTest?.Invoke();
		}

		public Action UserAction_BeforeOnline_ForTest { get; set; }
		partial void RunUserAction_BeforeOnline_ForTest()
		{
			UserAction_BeforeOnline_ForTest?.Invoke();
		}

		public Action UserAction_Online_ForTest { get; set; }
		partial void RunUserAction_Online_ForTest()
		{
			UserAction_Online_ForTest?.Invoke();
		}

		public Action UserAction_AfterOnline_ForTest { get; set; }
		partial void RunUserAction_AfterOnline_ForTest()
		{
			UserAction_AfterOnline_ForTest?.Invoke();
		}

		public bool UseObserver_ForTest => Observer != null;

		public int ObserverInterval_ForTest => (int)Observer.Interval_Exposed.TotalMilliseconds;

		public void SetOnlineActionList_ForTest(UpgradeActionList newList)
		{
			onlinePreSchemaUpgrader = new OnlinePreSchemaUpgraderForTest(this, mainDbConnection, null, null, versionInfo.DbReferenceVersion_Schema);
			onlineActionList = newList;
		}

		public void SetOffLineActionList_ForTest(UpgradeActionList newOfflineList, bool offlineLock)
		{
			offlineActionList = newOfflineList;
			requiresLockout = offlineLock;
		}

		#region Helper classes

		partial class UpgradeObserver
		{
			public TimeSpan Interval_Exposed => Interval;
		}

		sealed class OnlinePreSchemaUpgraderForTest : OnlinePreSchemaUpgrader
		{
			public OnlinePreSchemaUpgraderForTest(IUpgradeManager manager, DbConnection upgConnection, DbConnection auditConnection, DbConnection dataWarehouseConnection, VersionLabel versionBeforeUpgrade)
				: base(manager, upgConnection, auditConnection, dataWarehouseConnection, versionBeforeUpgrade)
			{
			}

			protected override void UpgradeMainDbSchema()
			{
			}
		}

		#endregion // Helper classes
	}
}

#endregion // Partial class

#endif
#endregion
