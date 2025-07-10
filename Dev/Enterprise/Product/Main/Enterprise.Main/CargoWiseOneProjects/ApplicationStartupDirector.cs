using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using CargoWise.Async;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Windows.UI.Controls.Internal;
#if NET
using CargoWise;
#endif
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.Startup.Tasks;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Startup
{
	public class ApplicationStartupDirector
	{
		#region Startup Tasks

		/// <summary>
		/// Specifies all tasks that run during Enterprise startup.
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		protected virtual IApplicationStartupTask[] GetEnterpriseStartupTasks(CommandLineArguments arguments)
		{
			if (IsNonInteractiveExecution(arguments))
			{
				return GetNonInteractiveUpgradeStartupTasks(arguments);
			}

			var result = new IApplicationStartupTask[] {
				new ProductBrandingCommandLineDeterminerTask(() =>
				{
					if (!System.Environment.UserInteractive)
					{
						return;
					}

					#if DEBUG
					if ((bool)arguments[ApplicationArguments.OptionNoSplash])
					{
						return;
					}
					#endif
					splash = new ZProcessStatusFormManager<StartupSplashForm>() { DisposeOnFormDispose = true };
					splash.EnableHideOnModal();
					splash.Start();
				}),
				new StartupNotification.InitializationTask(),
				new EnterStartupErrorHandler(),
				new StartupInitEnableMemoryManager(),
				new Initialiser.InitialiseWinFormsTask(),
				new ValidateDbArguments(),
				new SetupDbConnection(),								// Tasks that hit the DB, directly or indirectly, must come after SetupDbConnection
				new ProductBrandingRegistryDeterminerTask(UpdateBranding),
				new ValidatePurgeDataRunStatus(),
				new StartupCheckAvailableDiskSpace(),

				#if DEBUG
				new StartupInitAutoTesting(),
				#endif

				new StartupCheckRunWithoutLoader(),
				new InstallCurrentVersionTask(),
				new StartupCheckClientDll(),
				DbUpgraderDirector.New(arguments),
				new LogUsageOfApplicationTask(),
				new RemoveOldUpgradePackagesTask(),
				new RemoveOldInstallationsTask(),
				new TempFileCleanupTask(),
				new ResourceStringsUpdaterTask(),
				new StartupDocManagerCommandLineHandler(),
				new StartupInitShowInSystray(),
				new StartupInitEnterpriseUrlHandlerService(),
				new RemoteDesktopServicesInitializationTask(),
				LoginDirector.Instance,
				new EndStartupErrorHandler(),
				DotNetRecorderTaskScheduler.Instance,
				#if DEBUG
				new StartDat(this),
				#endif

				new ScreenResolutionChecker(),
				new StartupOpenMainFormTask(),

				#if DEBUG
				new StartBlazorWinFormsInterop(),
				#endif
				new ServiceManagerCommunicationMonitoringTask(),
				new DbConnectionCleanerTask(),
				#if DEBUG
				new ConcludeStartupTask(),
				#endif
			};

			return result.Where(x => x != null).ToArray();
		}

		IApplicationStartupTask[] GetNonInteractiveUpgradeStartupTasks(CommandLineArguments arguments)
		{
			return new IApplicationStartupTask[] {
				new StartupNotification.InitializationTask(),
				new EnterStartupErrorHandler(),
				new EnterScheduledUpgraderErrorHandler(),
				new Initialiser.InitialiseWinFormsTask(),
				new ValidateDbArguments(),
				new SetupDbConnection(),
				new ValidatePurgeDataRunStatus(),
				new ProductBrandingRegistryDeterminerTask(UpdateBranding),
				new InstallCurrentVersionTask(),
				new StartupCheckClientDll(),
				DbUpgraderDirector.New(arguments),
				new RemoveOldUpgradePackagesTask(),
				new RemoveOldInstallationsTask(),
				new TempFileCleanupTask(),
				new ResourceStringsUpdaterTask(),
#if DEBUG
				new SlowBackgroundApplicationStartupTask(),
#endif
				new ExitApplicationTask(),
			};
		}

		static bool IsNonInteractiveExecution(CommandLineArguments arguments)
		{
			if ((bool)arguments[ApplicationArguments.OptionScheduledDbUpgrader])
			{
				return true;
			}

#if DEBUG
			if ((bool)arguments[ApplicationArguments.OptionConsoleUpgrader])
			{
				return true;
			}
#endif

			return false;
		}

#if DEBUG
		public void SetProductBranding_ForTest()
		{
			((IApplicationStartupTask)new ProductBrandingCommandLineDeterminerTask(UpdateBranding)).Execute(new ApplicationArguments(new[] { ApplicationArguments.OpenRecordBaseline }));
		}

		[ThreadStatic]
		public static EventHandler MainFormShownActionForTest;
#endif

		#endregion

		#region Start Enterprise

#if !DEBUG // This breaks Edit & Continue on developer machines
		[LoaderOptimization(LoaderOptimization.MultiDomain)]
#endif
		[STAThread]
		public static int Main(string[] args)
		{
#if NET
			// In NetCore we need to execute the assembly resolver before any WiseTech lib is loaded.
			// So in Main we only have call to assembly resolver and all actual startup code
			// is executed in MainInternal after the resolver is set up.
			NetCoreAssemblyResolver.Setup();
#endif
			return MainInternal(args);
		}

		static int MainInternal(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

#if !NETFRAMEWORK
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
			ExceptionReporter.UIHooks = new ExceptionReporterUIHooks();
			ExceptionReporter.Instance.Enable();
			WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
			DbCommand.DisableAsync();

			// Do not access to non-system classes here
			// If you need to do something during application startup, it should be added as a task in GetEnterpriseStartupTasks()
			return new ApplicationStartupDirector().StartEnterprise(args);
		}

		public int StartEnterprise(string[] args)
		{
			var result = ExitCodes.Success;
			if (args.Length == 1 && args[0].StartsWith(UrlHandler.EdiUrlPrefix))
			{
				new EnterpriseUrlHandlerClient().ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew(args[0]);
			}

#if DEBUG
			else if (Dat.Integration.TestClient.IsTestClientCommand(args))
			{
				result = Dat.Integration.TestClient.Execute(Dat.Integration.CurrentContextExtensionLoader.Instance, args);
			}
#endif
			else
			{
				var parsedArguments = ParseArguments(args);
				if (parsedArguments != null)
				{
					var okToRun = RunEnterpriseStartupTasks(parsedArguments);
					if (okToRun)
					{
						try
						{
							Application_Run(StartupOpenMainFormTask.MainFormInstance);
						}
						catch (ObjectDisposedException) when (StartupOpenMainFormTask.MainFormInstance.IsDisposed)
						{
							// The Main Form is closed for any reason. We just need to exit the application.
						}
					}
					else
					{
						result = ExitErrorCode;
					}

					BackgroundApplicationStartupTask.WaitAll();
				}
			}

			return result;
		}

		internal bool RunEnterpriseStartupTasks(CommandLineArguments arguments)
		{
			IApplicationStartupTaskExceptionHandler exceptionHandler = null;
			var ok = true;
			Exception thrown = null;
			ApplicationDispatcher.Current = new ZDispatcherSynchronizationContext(Dispatcher.CurrentDispatcher);

			try
			{
				var tasks = GetEnterpriseStartupTasks(arguments);
				for (var taskNumber = 0; taskNumber < tasks.Length; taskNumber++)
				{
					var task = tasks[taskNumber];
					if (ok)
					{
						try
						{
							if (task.ShouldExecute(arguments))
							{
								OnProgress(task.TaskDescription, ProgressPercentageComplete(taskNumber, tasks.Length));
								var withProgress = task as IApplicationStartupTaskProgress;
								if (withProgress != null)
								{
									withProgress.Progress += new Progress(OnProgress);
								}

								var statisticsName = string.IsNullOrEmpty(task.TaskDescription) ? task.GetType().FullName : task.TaskDescription;

								using (PerformanceStatisticsCollector.StartMonitoring(statisticsName))
								{
									if (!task.Execute(arguments))
									{
										ok = false;
									}
								}

								var withExceptionHander = task as IApplicationStartupTaskExceptionHandler;
								if (withExceptionHander != null)
								{
									exceptionHandler = withExceptionHander;
								}
							}
						}
						catch (Exception e)
						{
							if (exceptionHandler != null)
							{
								try
								{
									ok = exceptionHandler.HandleException(e, arguments);
								}
								catch
								{
									ok = false;
									thrown = e; // exception handler failed, just rethrow original exception
								}
							}
							else
							{
								ok = false;
								thrown = e;
							}
						}
						if (!ok)
						{
							ExitErrorCode = task.FailureExitCode;
						}
					}
					else
					{
						try
						{
							var withOnFailure = task as IApplicationStartupTaskExecuteOnFailure;
							if (withOnFailure != null && task.ShouldExecute(arguments))
							{
								withOnFailure.ExecuteOnFailure(arguments);
							}
						}
						catch { } // after failure, suppress exceptions during cleanup
					}
				}
			}
			finally
			{
				DisposeSplash();
			}

			if (thrown != null)
			{
				throw new InvalidOperationException("Failed to start " + BrandingFactory.Instance.ProductName + ": " + thrown.Message, thrown);
			}

			if (StartupOpenMainFormTask.MainFormInstance != null && StartupOpenMainFormTask.MainFormInstance.IsDisposed)
			{
				ok = false;
			}
			return ok;
		}

		internal static int ProgressPercentageComplete(int taskNumber, decimal totalNumberOfTasks) => decimal.ToInt32(taskNumber / totalNumberOfTasks * 100);

		internal protected virtual void DisposeSplash()
		{
			if (splash != null)
			{
				if (StartupOpenMainFormTask.MainFormInstance != null)
				{
					splash.ChangeThreadFocus();
					StartupOpenMainFormTask.MainFormInstance.Shown += SplashOnMainFormShown;
				}
				else
				{
					splash.Dispose();
					splash = null;
				}
			}
		}

		void OnProgress(string status, int percentComplete)
		{
			if (splash != null)
			{
				splash.UpdateStatus(status, percentComplete);
			}
		}

		void Application_Run(Form mainForm)
		{
			RegisterDispatcher.Register();
			Application.SafeTopLevelCaptionFormat = BrandingFactory.Instance.ProductName;
			Application_RunCore(mainForm);
		}

#if DEBUG
		protected virtual
#endif
		void Application_RunCore(Form mainForm)
		{
#if DEBUG
			if (MainFormShownActionForTest != null)
			{
				mainForm.Shown += MainFormShownActionForTest;
			}
#endif
			Application.ThreadExit += Application_ThreadExit;
			Application.Run(mainForm);
		}

		protected void SplashOnMainFormShown(object sender, EventArgs e)
		{
			StartupOpenMainFormTask.MainFormInstance.Shown -= SplashOnMainFormShown;
			if (splash != null)
			{
				splash.Dispose();
				splash = null;
			}
		}

		internal protected ProcessStatusFormManager<StartupSplashForm> splash;

		internal void UpdateBranding()
		{
			splash?.InvokeOnForm(form => form?.UpdateBranding());
		}

		#endregion

		#region Error Handling

#if DEBUG
		public
#endif
		class EnterStartupErrorHandler : InitializingApplicationStartupTask, IApplicationStartupTaskExceptionHandler
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Before App Initialize")]
			public override string TaskDescription => "Startup Error Handler";

			public override int FailureExitCode => ExitCodes.EnterStartupErrorHandlerError;

			protected override bool DoExecute(CommandLineArguments arguments)
			{
				NotificationHandler.Instance = new ZGUINotificationHandler();
				ExceptionReporter.Instance.SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions = true;
				return true;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Before App Initialize")]
			public bool HandleException(Exception e, CommandLineArguments arguments)
			{
				var handled = false;

				if (e is FileLoadException)
				{
					throw e;
				}
				else if (e is DatabaseUpgradeException)
				{
					DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException((DatabaseUpgradeException)e);
					handled = true;
				}
				else if (e is SqlException sqlException)
				{
					switch (new DbErrorMatch(sqlException).ExceptionType)
					{
						case DbErrorType.ModuleBeingExecutedIsNotTrusted:
							using (var conn = Db.NewAdminConnection())
							{
								DataUtils.SetTrustworthyOn(conn, Db.DatabaseName);
							}
							handled = true;
							break;

						case DbErrorType.CannotExecuteAsDatabasePrincipal:
							using (var conn = Db.NewAdminConnection())
							{
								DataUtils.AlterDbAuthorisation(conn, Db.DatabaseName, sqlException);
							}
							handled = true;
							break;

						default:
							break;
					}
				}

				if (!handled)
				{
					StartupNotification.ShowError(FormattableString.Invariant($"{BrandingFactory.Instance.ProductName} failed to start: {e.Message}\r\nException details will be written to the Windows Event Log."), BrandingFactory.Instance.ProductName + " failed to start");
					SafeEventLogExtensions.SafeWriteEntryToApplicationLog(e.ToString(), EventLogEntryType.Error);
				}

				return handled;
			}
		}

		internal class EndStartupErrorHandler : InitializingApplicationStartupTask, IApplicationStartupTaskExecuteOnFailure, IApplicationStartupTaskExceptionHandler
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Before App Initialize")]
			public override string TaskDescription => "Startup Error Handler";

			public override int FailureExitCode => ExitCodes.EndStartupErrorHandlerError;

			protected override bool DoExecute(CommandLineArguments arguments)
			{
				ExceptionReporter.Instance.SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions = false;
				ExceptionReporter.Instance.SendAllUnsentDeveloperExceptions();
				return true;
			}

			public void ExecuteOnFailure(CommandLineArguments arguments)
			{
				Execute(arguments);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Show Error")]
			public bool HandleException(Exception e, CommandLineArguments arguments)
			{
				var handled = ExceptionReporter.Instance.TryHandleWithoutReporting(e);
				if (!handled)
				{
					StartupNotification.ShowError(FormattableString.Invariant($"{BrandingFactory.Instance.ProductName} failed to start: {e.Message}\r\nException details will be written to the Windows Event Log."), BrandingFactory.Instance.ProductName + " failed to start");
					SafeEventLogExtensions.SafeWriteEntryToApplicationLog(e.ToString(), EventLogEntryType.Error);
				}
				return handled;
			}
		}

#if DEBUG
		internal
#endif
		class EnterScheduledUpgraderErrorHandler : InitializingApplicationStartupTask, IApplicationStartupTaskExceptionHandler
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Before App Initialize")]
			public override string TaskDescription => "Scheduled Upgrade Error Handler";

			public override int FailureExitCode => ExitCodes.EnterScheduledUpgraderErrorHandlerError;

			public bool HandleException(Exception e, CommandLineArguments arguments)
			{
				StartupNotification.ScheduledUpgradeLogger.Log(Enterprise.Integration.LogType.Error, e.Message, e);
				return false;
			}

			protected override bool DoExecute(CommandLineArguments arguments)
			{
				return true;
			}
		}

		#endregion

		#region Command Line Arguments

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Nothing has been initialized")]
		protected CommandLineArguments ParseArguments(string[] args)
		{
			CommandLineArguments arguments = null;

			try
			{
				string argsFilePath = "";
				arguments = new ApplicationArguments(args);
				if (arguments[ApplicationArguments.OptionMoreArgs].ToString() != false.ToString())
				{
					argsFilePath = arguments[ApplicationArguments.OptionMoreArgs].ToString();
					CommandLineArguments newArguments = new CommandLineArguments(argsFilePath, arguments.OptionalArgs);
					newArguments.DatabaseName = arguments.DatabaseName;
					newArguments.ServerName = arguments.ServerName;
					arguments = newArguments;

					if (File.Exists(argsFilePath))
					{
						File.Delete(argsFilePath);
					}
				}
			}
			catch (ArgumentException e)
			{
				StartupNotification.ShowError(e.Message, "Argument Error");
				return null;
			}

			CommandLineArguments.UsedToLaunchApplication = arguments;

#if DEBUG
			NUnit.Framework.MainArgs.Args = args;
#endif

			return arguments;
		}

		#endregion

		#region Exit

		protected static void Application_ThreadExit(object sender, EventArgs e)
		{
			try
			{
				if (ApplicationDispatcher.MainThread == Thread.CurrentThread)
				{
					GCTracker.StopTracking();
					if (!Db.DatabaseUpgradedExceptionHasBeenThrownInConnection)
					{
						Env.LoginController.Logout();
					}
				}
			}
			catch (Exception)
			{
				// By this stage the exception handler has been disconnected, so all exceptions should be eaten
				// An example where this happens is during system upgrade if the user attempts to close the app
			}
		}

		int ExitErrorCode
		{
			get { return exitErrorCode; }
			set { exitErrorCode = value; }
		}
		int exitErrorCode = ExitCodes.Unspecified;

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Startup
{
	public class SlowBackgroundApplicationStartupTask : BackgroundApplicationStartupTask
	{
		public override int FailureExitCode => ExitCodes.SlowBackgroundApplicationStartupTaskError;

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return (bool)arguments[ApplicationArguments.OptionSlowBackgroundApplicationStartupTask];
		}

		public override void DoExecute()
		{
			Thread.Sleep(TimeSpan.FromSeconds(10));
			File.WriteAllText(Path.Combine(Temp.TempPath, "SlowBackgroundApplicationStartupTask"), "Finished");
		}
	}
}

#endif
#endregion
