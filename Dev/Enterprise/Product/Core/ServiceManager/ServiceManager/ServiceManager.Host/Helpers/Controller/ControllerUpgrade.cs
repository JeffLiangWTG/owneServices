using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using ServiceManager.Logging.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class ControllerUpgrade : IControllerUpgrade
	{
		public ControllerUpgrade(IServiceStopRequestConsumer serviceStopRequestConsumer, IApplicationEmergencyExit applicationEmergencyExit, IEventLogger eventLogger)
		{
			this.serviceStopRequestConsumer = serviceStopRequestConsumer;
			this.applicationEmergencyExit = applicationEmergencyExit;
			this.eventLogger = eventLogger;
			this.upgradeCheckStopwatch = new Stopwatch();
			upgradeCheckStopwatch.Start();
		}

		readonly IServiceStopRequestConsumer serviceStopRequestConsumer;
		readonly IApplicationEmergencyExit applicationEmergencyExit;
		readonly IEventLogger eventLogger;
		readonly Stopwatch upgradeCheckStopwatch;

		internal string lastReportedErrorMessage;
		DateTime lastReportedErrorMessageTime = DateTime.MinValue;

		public TimeSpan TimeSinceLastUpgradeCheck => upgradeCheckStopwatch.Elapsed;

		public void UpgradeSoftwareIfNeeded()
		{
#if DEBUG
			if (!Globals.IsTest && !AssemblyLoader.GetBinPath().StartsWith(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86)))
			{
				return;
			}
#endif

			var upgradeCheckSucceded = false;

			do
			{
				upgradeCheckStopwatch.Restart();
				try
				{
					try
					{
						var connection = Db.Connection;
						connection.IsUpgradeCheckDisabled = true;
						using (new DisposableAction(() => connection.IsUpgradeCheckDisabled = false))
						{
							// For an upgrade in progress...
							// - If logins were disabled then this connection will throw DatabaseUpgradeInProgressException when opened.
							// - If logins are enabled, then since the schema check is disabled this connection will not throw.
							//   However all the tables would normally be locked out so the SQL further down will fail with a lock timeout.
							//   Since we don't want to do anything if an upgrade is in progress anyway we have to do our own lockout check.
							//   Ugly, but this self updating code is going away soon.
							connection.EnsureIsOpen();
							if (DbLockout.HasLockout(connection))
							{
								throw new DatabaseUpgradeInProgressException();
							}

							var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
							var currentVersionInDatabase = upgradeManager.QueryCurrentVersion();

							if (currentVersionInDatabase != null && !ReleaseInfo.Instance.VersionNumber.ToVersion().Equals(currentVersionInDatabase.Version))
							{
								InstallCurrentVersion(currentVersionInDatabase);
								// we give a generous 2 minutes for CurrentVersionWriter to take the UpgraderMutex and issue the first service stop request before upgrading the ImagePath.
								// if CurrentVersionWriter will be unable to issue first stop request to our service within 2 minute period, the upgradeCheckSucceded will be false and we will repeat the upgrade loop in this method
								// invocation of CurrentVersionWriter is necessary to re-point Service ImagePath to the new binaries
								upgradeCheckSucceded = serviceStopRequestConsumer.WaitForServiceStopRequest(TimeSpan.FromMinutes(2));
							}
							else if (!DbVersionIsTheSame())
							{
								throw new DatabaseVersionException(
									new VersionLabel(Env.Registry.DatabaseMajorSchemaVersion, Env.Registry.DatabaseMinorSchemaVersion),
									new VersionLabel(Env.Registry.DatabaseMajorScriptVersion, Env.Registry.DatabaseMinorScriptVersion),
									new VersionLabel(Env.Registry.DatabaseMajorTransformationVersion, Env.Registry.DatabaseMinorTransformationVersion),
									SchemaVersion.Application,
									ScriptVersion.Application,
									TransformationVersion.ApplicationNumber);
							}
							else
							{
								upgradeCheckSucceded = true;
							}
						}
					}
					catch (DatabaseUpgradeInProgressException)
					{
						using (var adminConnection = Db.NewAdminConnection())
						{
							adminConnection.IsUpgradeCheckDisabled = true;
							if (adminConnection.CheckLockoutState() == DbLockoutState.InvalidLockout)
							{
								adminConnection.ResetLockout();
								continue;
							}
						}

						upgradeCheckSucceded = serviceStopRequestConsumer.WaitForServiceStopRequest(TimeSpan.FromSeconds(30));
					}
				}
				catch (ThreadAbortException)
				{
					upgradeCheckSucceded = true;
				}
				catch (SqlException ex)
				{
					if (new DbErrorMatch(ex).ExceptionType == DbErrorType.GeneralNetworkError)
					{
						//. SQL Server SHUTDOWN is in progress. 
						Log(ex, LogLevel.Information);
						upgradeCheckSucceded = true;
					}
					else if (new DbErrorMatch(ex).ExceptionType == DbErrorType.CannotOpenDbRequestedInLogin)
					{
						//. Cannot open database error 4060
						Log(ex, LogLevel.Information);
						upgradeCheckSucceded = true;
					}
					else
					{
						Log(ex, LogLevel.Error);
						upgradeCheckSucceded = serviceStopRequestConsumer.WaitForServiceStopRequest(TimeSpan.FromSeconds(30));
					}
				}
				catch (DatabaseVersionException versionException)
				{
					upgradeCheckSucceded = true;
					applicationEmergencyExit.ExitApplicationUnsafe(versionException.Message);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Log(ex, LogLevel.Error);
					upgradeCheckSucceded = serviceStopRequestConsumer.WaitForServiceStopRequest(TimeSpan.FromSeconds(30));
				}
			}
			while (!upgradeCheckSucceded);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void Log(Exception ex, LogLevel logLevel)
		{
			if (string.IsNullOrEmpty(lastReportedErrorMessage) || ex.Message != lastReportedErrorMessage || DateTime.UtcNow.Subtract(lastReportedErrorMessageTime).TotalMinutes > 30)
			{
				eventLogger.Log(logLevel, "Error checking current software version. " + ex.Message + "\r\n\r\n" + ex.StackTrace);
				lastReportedErrorMessage = ex.Message;
				lastReportedErrorMessageTime = DateTime.UtcNow;
			}
		}

		public bool IsUpgrading
		{
			get;
			private set;
		}

		/// <summary>
		/// Install the current version.
		/// Launches a separate process to do the work.
		/// That process should shut down this service, and eventually restart it.
		/// </summary>
		void InstallCurrentVersion(UpgradeInfo upgrade)
		{
			eventLogger.LogInformation(string.Format("Installing current software version {0}", upgrade.Version));
			IsUpgrading = true;
			var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			var targetPath = InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber(upgrade.Version));
			using (upgradeManager.InstallUpgradePackage(upgrade, targetPath))
			{
				upgradeManager.StartCurrentVersionWriter(targetPath);
			}
		}

		bool DbVersionIsTheSame()
		{
#if DEBUG
			if (SkipUpgrade())
			{
				return true;
			}
#endif

			return !((IDbReconnectionHandling)Db.Connection).HasDbSchemaOrScriptOrTransformationVersionChanged();
		}

#if DEBUG
		bool SkipUpgrade()
		{
			var result = false;
			if (CargoWise.BuildTools.BuildConstants.LocalSourcePathAvailable)
			{
				var checkerType = Type.GetType("Enterprise.DbUpgrader.Shared.DbFileChecker, Enterprise.DbUpgrader.Shared");
				var checker = Activator.CreateInstance(checkerType, Path.Combine(CargoWise.BuildTools.BuildConstants.LocalEnterprisePath, "DBUPG_SkipUpgradeDbs.txt"));
				result = (bool)checker.GetType().GetMethod("IsDbInFile").Invoke(checker, new object[] { Db.ServerName, Db.DatabaseName });
			}
			return result;
		}
#endif
	}
}
