using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Environment;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Initialisation
{
	/// <summary>
	/// Initialisation code that must run immediately on application startup.
	/// This code must always succeed. It must not show UI, it must not depend on the database, it must not depend
	/// on command line options, and it must not throw unhandled exceptions.
	/// Use InitialiseAfterDbConnection() instead if your code can wait until after the database is available.
	/// </summary>
	public sealed class Initialiser : Integration.Initialisation.IInitialiser
	{
		Initialiser()
		{
		}

		public static void InitialiseConsoleApp()
		{
			InitialiseBase(isUserInteractive: true);
		}

		public static void InitialiseWeb(bool? enableErrorReport, BaseExceptionReporter baseExceptionReporter, EnvProvider envProvider)
		{
			InitialiseCommon(false, enableErrorReport, baseExceptionReporter, envProvider);
		}

		public static void InitialiseWebForms()
		{
			InitialiseBase(isUserInteractive: true);
		}

		public class InitialiseWinFormsTask : AbstractApplicationStartupTask
		{
			public override string TaskDescription
			{
				get { return (NoResString)"Initializing"; }
			}

			public override int FailureExitCode => ExitCodes.InitialiseWinFormsTaskError;

			protected override bool GetShouldExecute(CommandLineArguments arguments)
			{
				return true;
			}

			protected override bool DoExecute(CommandLineArguments arguments)
			{
				if ((bool)arguments["-ScheduledDbUpgrader"])
				{
					InitialiseCommon(false);
				}
				else
				{
					InitialiseWinForms();
				}
				return true;
			}
		}

		public static void InitialiseBatchProcessor()
		{
			InitialiseCommon(isUserInteractive: false);
		}

		public static void InitialiseServiceManager(BaseExceptionReporter exceptionReporter, bool usePooledConnection)
		{
			InitialiseCommon(isUserInteractive: false, exceptionReporter: exceptionReporter, envProvider: new ServiceManagerEnvProvider(usePooledConnection));
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Calling getter")]
		public static void InitialiseWinForms(Guid? appServerCorrelationId = null)
		{
			InitialiseCommon(isUserInteractive: true, null, null, null, appServerCorrelationId);

			Enterprise.ZArchitecture.GUI.ZFormGlobalStrategies.Initialize();
			ThreadSentry.IsPostableProcess = true;
			ZGridContextMenuHelper.Instance.AddProvider(new DocEngineDynamicMenuItemProvider());
			NotificationIconSchemeRegistration.EnsureRegistered();
			UIResources.Instance.MonitorUIResourcesInBackground(UIRessourcesNearlyOverflow);
			Env.Instance.UserContextChanging += (sender, args) => { ModuleFilter.ClearSelectedFiltersCache(); };
			#if !WINZOR
			var dummy = System.Windows.Forms.ImeModeConversion.ImeModeConversionBits; //fixes multithreaded-access causing an exception: Work Item WI00261579
			#endif
		}

		static void InitialiseCommon(bool isUserInteractive, bool? enableErrorReport = null, BaseExceptionReporter exceptionReporter = null, EnvProvider envProvider = null, Guid? appServerCorrelationId = null)
		{
			InitialiseBase(isUserInteractive);

			envProvider = envProvider ?? new WinFormsEnvironmentProvider();
			envProvider.Enable();

			if (!enableErrorReport.HasValue || enableErrorReport.Value)
			{
				Globals.InteractiveNotification = UserNotification.Instance;
				if (exceptionReporter != null)
				{
					exceptionReporter.Enable();
				}
				else
				{
					ExceptionReporter.SessionId = appServerCorrelationId ?? Guid.NewGuid();
					ExceptionReporter.Instance.Enable();
				}
			}
		}

		static void InitialiseBase(bool isUserInteractive)
		{
			Globals.IsUserInteractive = isUserInteractive;

			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
			Enterprise.ZArchitecture.Business.CustomNotesProvider.Instance = new CustomNotesProvider();
			Enterprise.MasterFiles.Business.PropertyChangeLogger.Initialize();
			Res.SetResourceStringsGetter(ResourceStringsGetter);
			ZrsFile.ZrsFileDirectoryLocator = AssemblyLoader.GetBinPath;
		}

		static IResourceStrings ResourceStringsGetter()
		{
			// ObjectFactory should be accessed from this assembly
			return GetResourceStringsInstance();
		}

		static IResourceStrings GetResourceStringsInstance()
		{
			return DesignerSafeObjectFactory.GetDesignerSafe<IResourceStrings>();
		}

		static void UIRessourcesNearlyOverflow(object sender, EventArgs e)
		{
			Globals.Message.ShowWarning((NoResString)"The application has nearly run out of UI Ressource. Consider closing one or more windows to prevent the application from closing.");
		}
	}
}

#region Test
#if DEBUG
#if !WINZOR

#endif
#endif
#endregion
