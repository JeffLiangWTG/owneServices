using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MailManager.FileDownload;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Tasks.ScheduledUpgrader;
using Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ServiceTasks;
using Enterprise.Upgrades;
using Enterprise.Upgrades.UpgradePackageServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using static System.FormattableString;

#if NETFRAMEWORK
#endif

[assembly: HostedService(UpgraderServiceTask.Code, UpgraderServiceTask.ServiceTaskDescription, "SYS", typeof(UpgraderServiceTask),
	IsMandatory = false,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1hour",
	ConfigControlType = typeof(ScheduledUpgraderConfigControl),
	MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.Upgrade,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	DefaultScheduleDoNotRunTillNextDueTimeIfOverdue = "24hours")
]

namespace Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ServiceTasks
{
	public class UpgraderServiceTask : ServiceProviderImpl, IServiceTaskConfigurationUser
	{
		public const string Code = "UPG";
		public const string ServiceTaskDescription = "System Upgrade Service";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Information(Invariant($"Auto-Download is set to '{(Config.AutoDownload ? "true" : "false")}'."));
			ServiceLogger.Information(Invariant($"Patch-Only is set to '{(Config.PatchOnly ? "true" : "false")}'."));

			var upgradeToApply = FindUpgradeToApply(token, out var currentVersion);
			if (Config.AutoDownload)
			{
				var downloaded = DownloadLatestPackage(currentVersion, upgradeToApply);
				if (downloaded != null)
				{
					upgradeToApply = downloaded;
				}
			}
			if (upgradeToApply != null)
			{
				DoUpgrade(upgradeToApply);
			}
		}

		internal UpgradeInfo DownloadLatestPackage(UpgradeInfo currentVersion, UpgradeInfo queryVersion)
		{
			if (queryVersion == null)
			{
				queryVersion = currentVersion;
			}
			if (queryVersion == null)
			{
				return null;
			}

			UpgradeInfoExtended result = null;
			UpgradePackageUrlResponse response = null;
			var originalExpect100Continue = System.Net.ServicePointManager.Expect100Continue;
			System.Net.ServicePointManager.Expect100Continue = false;
			var client = UpgradePackageServiceClient;

			try
			{
				var licenceCode = Env.CurrentCompany.GetLicenceCode();
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				var encoder = ZArchitecture.Core.Encryption.TwoWayEncoder.NewWithStandardInitialisationVector();
				var encryptedMessage = encoder.Encrypt(UpgradePackageServiceFactory.GetRawMessage(licenceCode, registrationKey.DatabaseNumber, queryVersion));
				var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = licenceCode, CurrentVersionNumber = queryVersion.ToString(), IsPatchOnly = Config.PatchOnly };

				ServiceLogger.Information("Checking online for a newer upgrade package...");

				var retryPolicy = GetRetryPolicy();

				try
				{
					retryPolicy.ExecuteAction(() =>
					{
						response = client.GetPackageUrl(request);
						if (response.ResponseClass != UpgradePackageUrlResponse.ResponseClassType.Success)
						{
							throw new UpgradeRetryException(response);
						}
					});
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					response = retryPolicy.Response;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				response = new UpgradePackageUrlResponse() { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Error, ErrorMessage = ex.Message };
			}
			finally
			{
				if (client is UpgradePackageServiceClient upgradePackgeServiceClient)
				{
					try
					{
						upgradePackgeServiceClient.Close();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				}
				System.Net.ServicePointManager.Expect100Continue = originalExpect100Continue;
			}

			if (response.ResponseClass == UpgradePackageUrlResponse.ResponseClassType.Success)
			{
				var availableVersion = new Version(response?.VersionNumber ?? "0.0.0.0");
				if (queryVersion.Version < availableVersion && !string.IsNullOrEmpty(response.URL))
				{
					if (Config.PatchOnly && !(currentVersion != null && currentVersion.Version.Major == availableVersion.Major && currentVersion.Version.Minor == availableVersion.Minor && currentVersion.Version.Build == availableVersion.Build))
					{
						ServiceLogger.Information(Invariant($"Upgrade package {response.VersionNumber} found, not downloading because Patch-Only is set to '{(Config.PatchOnly ? "true" : "false")}'."));
					}
					else
					{
						ServiceLogger.Information(Invariant($"Upgrade package {response.VersionNumber} found, downloading from {response.URL}"));
						var tempDirectory = new TempDirectory();
						try
						{
							GetDownloaderRetryPolicy().ExecuteAction(() =>
							{
								using (var webFileDownloader = new WebFileDownloader(response.URL))
								{
									webFileDownloader.StartFileDownload(tempDirectory.DirectoryName);
									while (!webFileDownloader.FinishedWaitHandle.WaitOne(30 * 1000))
									{
										ServiceLogger.Information(Invariant($"Downloaded {webFileDownloader.TotalDownloaded} of {webFileDownloader.FileSize} bytes"));
									}

									if (webFileDownloader.TotalDownloaded > 0 && webFileDownloader.FileSize > 0)
									{
										ServiceLogger.Information(Invariant($"Downloaded {webFileDownloader.TotalDownloaded} of {webFileDownloader.FileSize} bytes"));
									}

									if (webFileDownloader.HasDownloadCompleted)
									{
										ServiceLogger.Information("Uploading package to database");
										var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
										result = upgradeManager.UploadUpgradePackage(webFileDownloader.TargetFilePath, "RDY", "System Upgrade Service", null);
									}
									else
									{
										ServiceLogger.Error(Invariant($"Error downloading upgrade package: {webFileDownloader.StatusMessage}"));
										throw new UpgradeRetryException(null); //trigger the retry.
									}
								}
							});
						}
						catch (CorruptEdpException ex)
						{
							ServiceLogger.Error(ex.Message);
							ErrorReporter.ReportOnce("CorruptEdp", ex.Message, ex);
						}
						catch (UpgradeRetryException)
						{
							ServiceLogger.Error("Failed to download the upgrade package.");
						}
						finally
						{
							try
							{
								tempDirectory.Dispose();
							}
							catch (IOException ex)
							{
								ServiceLogger.Error(Invariant($"Error deleting temp file: {ex.Message}"));
							}
						}
					}
				}
				else if (queryVersion.Version < availableVersion && string.IsNullOrEmpty(response.URL))
				{
					ServiceLogger.Information(Invariant($"The upgrade package {response.VersionNumber} was found, but downloading skipped: {response.ErrorMessage}"));
				}
				else
				{
					ServiceLogger.Information("No new online upgrade package available.");
				}
			}
			else
			{
				var errorMessge = Invariant($"Error checking for latest upgrade package online: {response.ErrorMessage}");
				ServiceLogger.Error(errorMessge);
			}

			return result;
		}

		internal virtual UpgradeRetryPolicy GetRetryPolicy() => new UpgradeRetryPolicy(ServiceLogger);
		RetryPolicy GetDownloaderRetryPolicy() => new RetryPolicy<DownloaderErrorDetectionStrategy>(new FixedInterval(3, DownloaderRetryInterval) { FastFirstRetry = false });
		protected virtual TimeSpan DownloaderRetryInterval => TimeSpan.FromSeconds(15);

		UpgradeInfo FindUpgradeToApply(CancellationToken token, out UpgradeInfo currentVersion)
		{
			UpgradeInfo upgradeToApply = null;
			currentVersion = null;

			try
			{
				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				currentVersion = upgradeManager.QueryCurrentVersion();

				ServiceLogger.Information(Invariant($"Current version is {(currentVersion != null ? currentVersion.Version.ToString() : "UNKNOWN")}."));

				if (currentVersion != null)
				{
					ServiceLogger.Information("Checking locally for a newer upgrade package...");

					foreach (var availableUpgrade in upgradeManager.QueryRunnablePackages())
					{
						token.ThrowIfCancellationRequested();

						if (availableUpgrade.Status == StmUpgrade.StmUpgradeStatus.Ready
							&& availableUpgrade.Version > currentVersion.Version)
						{
							if (Config.PatchOnly && !(currentVersion.Version.Major == availableUpgrade.Version.Major && currentVersion.Version.Minor == availableUpgrade.Version.Minor && currentVersion.Version.Build == availableUpgrade.Version.Build))
							{
								ServiceLogger.Information(Invariant($"Upgrade package {availableUpgrade.Version} found, not applying because Patch-Only is set to '{(Config.PatchOnly ? "true" : "false")}'."));
							}
							else
							{
								ServiceLogger.Information(Invariant($"Upgrade package {availableUpgrade.Version} found."));
								upgradeToApply = availableUpgrade;
								break;
							}
						}
					}
				}
				if (upgradeToApply == null)
				{
					ServiceLogger.Information("No new local upgrade package available.");
				}
			}
			catch (DatabaseUpgradeInProgressException ex)
			{
				ServiceLogger.Information(ex.Message);
			}

			return upgradeToApply;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, launching Enterprise, not opening a file or url")]
		internal virtual bool DoUpgrade(UpgradeInfo upgradeToApply)
		{
			MultilingualString errorMessage = null;

			ServiceLogger.Information(Invariant($"Applying upgrade {upgradeToApply.Version}."));

			try
			{
				bool hasExited;
				int exitCode;

				#region Test run
#if DEBUG
				if (RunUpgradeProcessForTest != null)
				{
					var testRunResult = RunUpgradeProcessForTest();
					hasExited = testRunResult.Item1;
					exitCode = testRunResult.Item2;
				}
				else
#endif
				#endregion
				{
					var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
					var targetInstallPath = InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber(upgradeToApply.Version));
					using (upgradeManager.InstallUpgradePackage(upgradeToApply, targetInstallPath))
					{
						var sdir = GetSDirArgument() ?? string.Empty;
						var exeFilename = ExeFileNames.GetMainExeFileName(targetInstallPath);
						// Orchestrator does check the order and values of certain arguments of the upgrading process.
						// You have to change the Orchestrator code in conjunction with any changes there.
						string[] arguments =
						{
							CommandLineArgEncoder.EnquoteArgumentIfNeeded(Db.ServerName),
							CommandLineArgEncoder.EnquoteArgumentIfNeeded(Db.DatabaseName),
							"-SDir:" + CommandLineArgEncoder.EnquoteArgumentIfNeeded(sdir),
							"-Upgrade:" + upgradeToApply.PK,
							"-ScheduledDbUpgrader",
							Config.NotifyOnSuccess ? "-NotifyOnSuccessfulUpgrade" : "",
							Config.NotificationGroup_PK.IsValid ? "-NotificationGroupPK:" + Config.NotificationGroup_PK.ToGuid() : "",
							"-ServiceProcess:" + Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(Process.GetCurrentProcess().Id.ToString()), null, DataProtectionScope.LocalMachine)),
						};
						var startInfo = new ProcessStartInfo(Path.Combine(targetInstallPath, exeFilename), string.Join(" ", arguments));
						ServiceLogger.Debug(Invariant($"Upgrade process info: {startInfo.FileName} {startInfo.Arguments}"));
						startInfo.WorkingDirectory = targetInstallPath;
						startInfo.UseShellExecute = false;
						upgradeProcess = Process.Start(startInfo);
					}

					while (!stop && !upgradeProcess.WaitForExit(1000))
					{ }
					hasExited = upgradeProcess.HasExited;
					exitCode = hasExited ? upgradeProcess.ExitCode : 0;
				}

				if (hasExited && exitCode != 0)
				{
					errorMessage = ResString.GetMultilingualString("34137da5-d073-4302-bf46-0ec0149d0763",
						"Upgrade process exited with error code {0}. Check the log of service task UPG for upgrade errors.", exitCode);
				}
			}
#if DEBUG
			catch (ThreadAbortException)
			{
				throw;
			}
#endif
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errorMessage = MultilingualString.Join("\r\n", ResString.GetMultilingualString("89054683-7584-4d90-bcb8-e1e6ef1dd271", "Exception occurred while applying upgrade:"), (NoResString)ex.ToString());
			}

			if (errorMessage != null)
			{
				try
				{
					ServiceLogger.Log(LogType.Error, errorMessage.GetUnresolvedString());
				}
				catch (Exception ex) when (ex.Find<SqlLockLostException>() != null)
				{
				}
				return false;
			}

			return true;
		}

		#region Test stuff
#if DEBUG

		internal Func<Tuple<bool, int>> RunUpgradeProcessForTest { get; set; }

#endif
		#endregion

		public Process UpgradeProcess
		{
			get { return upgradeProcess; }
		}
		Process upgradeProcess;

		protected virtual string GetSDirArgument()
		{
			var sdir = System.Environment.GetCommandLineArgs().Where(x => x.StartsWith("-SDir:")).FirstOrDefault();
			return sdir != null ? sdir.Substring("-SDir:".Length) : null;
		}

		public void Stop()
		{
			stop = true;
		}

		bool stop;

		public string ConfigString { get; set; }

		public ScheduledUpgraderConfig Config
		{
			get { return new ScheduledUpgraderConfig(ConfigString); }
		}

		internal virtual IUpgradePackageService UpgradePackageServiceClient
		{
			get { return UpgradePackageServiceFactory.GetUpgradePackageServiceClient(); }
		}

		public class UpgradeRetryPolicy : RetryPolicy
		{
			public UpgradeRetryPolicy(ILogger logger) : base(new UpgradeErrorStrategy(logger), new UpgradeRetryStrategy())
			{
				(RetryStrategy as UpgradeRetryStrategy).UpdateErrorStrategyRetryCount = UpdateErrorStrategyRetryCount;
			}
			public UpgradeRetryPolicy(RetryStrategy strategy, ILogger logger) : base(new UpgradeErrorStrategy(logger), strategy) { }

			internal UpgradeRetryStrategy UpgradeRetryStrategy => RetryStrategy as UpgradeRetryStrategy;

			public Action<int> UpdateErrorStrategyRetryCount => retryCount => (ErrorDetectionStrategy as UpgradeErrorStrategy).RetryCount = retryCount;

			public UpgradePackageUrlResponse Response => (ErrorDetectionStrategy as UpgradeErrorStrategy).Response;
		}

		public class UpgradeRetryStrategy : RetryStrategy
		{
			public UpgradeRetryStrategy() : base("UpgradeRetryStrategy", false)
			{
			}

			public override ShouldRetry GetShouldRetry() => ShouldRetry;

			public Action<int> UpdateErrorStrategyRetryCount { get; set; }

			bool ShouldRetry(int retryCount, Exception exception, out TimeSpan delay)
			{
				delay = SleepDependingOnRetryCount(retryCount);
				UpdateErrorStrategyRetryCount(retryCount + 1); // this includes this retry.
				return retryCount < 3;
			}

			TimeSpan SleepDependingOnRetryCount(int retries)
			{
				if (IsTest)
				{
					return TimeSpan.Zero;
				}

				switch (retries)
				{
					case 0:
						return TimeSpan.FromMinutes(1);
					case 1:
						return TimeSpan.FromMinutes(5);
					default:
						return TimeSpan.FromMinutes(10);
				}
			}

			protected virtual bool IsTest
			{
				get { return Globals.IsTest; }
			}
		}

		public class UpgradeErrorStrategy : ITransientErrorDetectionStrategy
		{
			public UpgradeErrorStrategy(ILogger logger)
			{
				Logger = logger;
			}
			ILogger Logger { get; }
			public int RetryCount { get; set; }

			public bool IsTransient(Exception exception)
			{
				Response = exception is UpgradeRetryException upgradeRetry
					? upgradeRetry.Response
					: new UpgradePackageUrlResponse() { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Error, ErrorMessage = exception.Message };
				if (RetryCount < 3)
				{
					Logger.Warning(Invariant($"Getting package url failed with '{Response.ErrorMessage}'. Retrying..."));
				}

				return !exception.IsCriticalException();
			}
			public UpgradePackageUrlResponse Response { get; private set; }
		}

		class DownloaderErrorDetectionStrategy : ITransientErrorDetectionStrategy
		{
			public bool IsTransient(Exception ex) => true;
		}
	}

	[Serializable]
	public class UpgradeRetryException : Exception
	{
		public UpgradeRetryException(UpgradePackageUrlResponse response)
		{
			Response = response;
		}

#if NETFRAMEWORK
		public UpgradeRetryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif

		public UpgradePackageUrlResponse Response { get; }
	}
}
