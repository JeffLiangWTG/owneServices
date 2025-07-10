using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Loader.Common.Exceptions;

namespace CargoWise.Loader.Common
{
	public abstract class StartupDirector
	{
		public const string BreakIntoDebuggerArgument = "-EdiLoadBreakIntoDebugger";

		protected StartupDirector()
		{
		}

		protected Configuration config;

		protected Configuration Configuration
		{
			get
			{
				if (config == null)
				{
					throw new ConfigurationNotInitializedException("Should have the Configuration initialized first.");
				}

				return config;
			}
		}

		protected static string CurrentProcessPath
		{
			get
			{
				var mainModule = Process.GetCurrentProcess().MainModule;
				return mainModule.FileName;
			}
		}

		public InstallationItem TopLevelItem { get; protected set; }

		protected abstract Configuration GetNewConfiguration();

		public bool Initialize(string[] args)
		{
			config = GetNewConfiguration();
			return config.Initialize(args) && InitializeInstallationItems();
		}

		protected abstract bool InitializeInstallationItems();

		protected virtual void Run()
		{
			if (TopLevelItem == null)
			{
				throw new InvalidOperationException("Should have TopLevelItem evaluated first.");
			}

			TopLevelItem.Installation.Progress += new EventHandler(TopLevelItem_Progress);
			TopLevelItem.Installation.CurrentTaskDescriptionChanged += new EventHandler<TaskDescriptionChangedEventArgs>(TopLevelItem_CurrentTaskDescriptionChanged);
			if (Configuration.UILevel == UILevel.AutomatedWithNoUI)
			{
				StartInstall();
			}
			else
			{
				StartUserInterface();
			}
		}

		public int StartApplication(string[] args)
		{
			args = ParseArgs(args);
			startupArgs = string.Join(" ", args);

			Application.EnableVisualStyles();
			try
			{
				Application.SetCompatibleTextRenderingDefault(false);
			}
			catch (InvalidOperationException)
			{
				// Won't work in a unit test - ignore it.
			}
			if (Initialize(args))
			{
				if (args != null && args.Any(a => a == "-PW"))
				{
					BrandingFactory.Configure(BrandingFactory.BrandingType.ProductivityWise);
				}
				else
				{
					BrandingFactory.Configure(Configuration.BrandingType);
				}

				Run();
			}
			return (int)Configuration.ReturnCode;
		}

		protected virtual void StartInstall()
		{
			if (TopLevelItem != null)
			{
				InstallationResultCollection availabilityResults = new InstallationResultCollection();
				TopLevelItem.CheckAvailability(availabilityResults);
				ShowResultsDialogIfNecessary(availabilityResults);

				if (availabilityResults.ErrorCount == 0)
				{
					InstallationResultCollection installResults = new InstallationResultCollection();
					TopLevelItem.Install(installResults);
					ShowResultsDialogIfNecessary(installResults);
					Configuration.ReturnCode = (installResults.ErrorCount > 0) ? ReturnCode.Failure : ReturnCode.Success;
				}
				else
				{
					Configuration.ReturnCode = ReturnCode.Failure;
				}
			}

			if (splash != null)
			{
				splash.Dispose();
			}
		}

		protected virtual void StartUserInterface()
		{
			using (splash = CreateSplash())
			{
				splash.Start();
				splash.UpdateBranding();
				StartInstall();
			}
		}

		ISplash CreateSplash() => BrandingFactory.Instance is CargoWiseNextBranding ? new CWNextSplash(Configuration) : new CW1Splash(Configuration);

		string[] ParseArgs(string[] args)
		{
			if (args != null)
			{
				int debugArgumentIndex = Array.IndexOf(args, BreakIntoDebuggerArgument);
				if (debugArgumentIndex >= 0)
				{
					List<string> argsCopy = new List<string>(args);
					argsCopy.RemoveAt(debugArgumentIndex);
					args = argsCopy.ToArray();
				}
			}
			return args;
		}

		void ShowResultsDialogIfNecessary(InstallationResultCollection results)
		{
			Argument.NotNull(results, nameof(results));
			if (Configuration.UILevel == UILevel.AutomatedWithNoUI)
			{
				string errors = results.GetErrorMessages();
				string warnings = results.GetWarningMessages();
				if (errors.Length > 0)
				{
					Configuration.Services.EventLog.WriteEntry(Configuration.ApplicationName, $"{startupArgs}{Environment.NewLine}{errors}", EventLogEntryType.Error);
					Console.Error.WriteLine($"{errors}");
				}
				if (warnings.Length > 0)
				{
					Configuration.Services.EventLog.WriteEntry(Configuration.ApplicationName, $"{startupArgs}{Environment.NewLine}{warnings}", EventLogEntryType.Warning);
				}
			}
			else
			{
				if ((results.ErrorCount > 0) || (results.WarningCount > 0))
				{
					using (InstallationResultsForm resultForm = InstallationResultsForm.New())
					{
						resultForm.LoadInstallationResults(results);
						resultForm.ShowDialog();
					}
				}
			}
		}

		void TopLevelItem_CurrentTaskDescriptionChanged(object sender, TaskDescriptionChangedEventArgs e)
		{
			if (splash != null)
			{
				splash.UpdateStatus(e.TaskDescription, splash.ProgressValue);
			}
		}

		void TopLevelItem_Progress(object sender, EventArgs e)
		{
			if (splash != null)
			{
				taskNumber++;
				splash.UpdateStatus(splash.Status, Decimal.ToInt32(taskNumber / ((decimal)TopLevelItem.TotalProgressCount) * 100));
			}
		}

		ISplash splash;
		int taskNumber;
		string startupArgs;
	}
}
