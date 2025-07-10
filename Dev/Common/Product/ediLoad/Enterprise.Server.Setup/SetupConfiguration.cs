using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Loader.Common;
using Microsoft.Win32;

namespace Enterprise.Server.Setup
{
	sealed class SetupConfiguration : Configuration
	{
		const string HelpArgument = "-?";
		List<string> handledPropertyArguments;
		InstallationSettings installationSettings;
		bool sqlInstanceInvalid;

		public override string ApplicationName
		{
			get { return BrandingFactory.Instance.ProductName + " Server Setup"; }
		}

		public bool HasErrors { get; private set; }

		public InstallationSettings InstallationSettings
		{
			get { return installationSettings ?? (installationSettings = new InstallationSettings(this)); }
		}

		public string GetHelpMessage(bool error)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(
@"Switches:

{0}
    Displays this help message.
{1}
    Installation will be automated but the progress bar and error
    messages will be shown.
{2}
    Installation will be fully automated with no user interface. Errors
    and warnings will be logged in the Application log in the Windows
    event viewer. Implies Automated.

Properties:

",
				HelpArgument, AutomatedArgument, NoUIArgument);

			foreach (string propertyName in InstallationSettings.CommandLineProperties.Keys)
			{
				sb.AppendLine('-' + propertyName + ':');
			}

			sb.AppendLine(
@"-SqlInstance:
    Use MSSQLSERVER for the default SQL Server instance that is
    already installed.

Specify the value after the argument. If a property or value isn't specified, the default will be used.");

			if (error)
			{
				sb.Insert(0, "One or more arguments you have specified are invalid. The following arguments are supported:" + Environment.NewLine + Environment.NewLine);
			}

			return sb.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		protected override void InitializeCore(string[] args)
		{
			base.InitializeCore(args);

			bool continuationFileLoaded = LoadContinuationFile();
			installationSettings.DeleteContinuationFile();

			if (!HasErrors)
			{
				bool isAdmin = AdministratorChecker.UserIsAdministrator();
				if (isAdmin)
				{
					HasErrors = !Directory.Exists(StartupPath);
					if (HasErrors)
					{
						string fullUserName = Environment.UserDomainName + '\\' + Environment.UserName;
						Notifier.ShowError("Could not find '" + StartupPath + "'. Please make sure that your user account (" + fullUserName + ") has permissions to access that directory.", "Error");
					}
					else
					{
						if (UILevel != UILevel.Normal)
						{
							if (sqlInstanceInvalid)
							{
								Notifier.ShowError("The specified SQL Server instance does not exist.");
								HasErrors = true;
							}
							else
							{
								InstallationSettingsValidator validator = new InstallationSettingsValidator(this, null);
								CancelEventArgs e = new CancelEventArgs(false);
								validator.Validate(e);
								HasErrors = e.Cancel;
							}
						}
					}
				}
				else
				{
					Notifier.ShowError("You must be an administrator on this computer to install " + BrandingFactory.Instance.ProductName + ".");
					HasErrors = true;
				}
			}
			if (!HasErrors && continuationFileLoaded && UILevel == UILevel.Normal)
			{
				if (MessageBox.Show("Would you like to continue the previous installation process?", ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					UILevel = UILevel.AutomatedWithUI;
				}
			}
		}

		internal bool LoadContinuationFile()
		{
			bool continuationFileLoaded = false;
			if (File.Exists(InstallationSettings.ContinuationFile))
			{
				continuationFileLoaded = true;
				foreach (string line in File.ReadAllLines(InstallationSettings.ContinuationFile, Encoding.UTF8))
				{
					ParseCommandLineArgument(line);
				}
			}
			return continuationFileLoaded;
		}

		protected override void ParseCommandLineArgument(string arg)
		{
			if (!HasErrors)
			{
				switch (arg)
				{
					case "-?":
					case "/?":
						ShowHelpMessage(false);
						break;
					default:
						HandlePropertyArgument(arg);
						break;
				}
			}
		}

		List<string> HandledPropertyArguments
		{
			get { return handledPropertyArguments ?? (handledPropertyArguments = new List<string>()); }
		}

		void HandlePropertyArgument(string arg)
		{
			bool valid = arg.StartsWith("-");
			if (valid)
			{
				int separatorIndex = arg.IndexOf(':');
				valid = (separatorIndex != -1);
				if (valid)
				{
					string propertyName = arg.Substring(1, separatorIndex - 1);
					string propertyValue = arg.Substring(separatorIndex + 1);
					if (valid = (propertyName.Length > 0) && (propertyValue.Length > 0))
					{
						if (HandledPropertyArguments.Contains(propertyName))
						{
							HasErrors = true;
							Notifier.ShowError("The argument '" + propertyName + "' has been specified more than once.", "Duplicate Argument");
						}
						else
						{
							HandledPropertyArguments.Add(propertyName);
							switch (propertyName)
							{
								case "SqlInstance":
									HandleSqlInstanceArgument(propertyValue);
									break;
								default:
									bool hasUseDefaultProperty;
									if (InstallationSettings.CommandLineProperties.TryGetValue(propertyName, out hasUseDefaultProperty))
									{
										if (hasUseDefaultProperty)
										{
											InstallationSettings.SetPropertyValue("UseDefault" + propertyName, false);
										}
										InstallationSettings.SetPropertyValue(propertyName, propertyValue);
									}
									else
									{
										valid = false;
									}
									break;
							}
						}
					}
				}
			}
			if (!valid)
			{
				ShowHelpMessage(true);
			}
		}

		void HandleSqlInstanceArgument(string instanceName)
		{
			foreach (DatabaseChoice dbChoice in InstallationSettings.DatabaseList)
			{
				if (string.Equals(dbChoice.InstanceName, instanceName, StringComparison.OrdinalIgnoreCase))
				{
					InstallationSettings.SelectedDatabase = dbChoice;
					return;
				}
			}
			sqlInstanceInvalid = true;
		}

		void ShowHelpMessage(bool error)
		{
			HasErrors = true;
			ShowHelp = true;
			string message = GetHelpMessage(error);
			if (error)
			{
				Notifier.ShowError(message, "Invalid Arguments");
			}
			else
			{
				Services.MessageBox.Show(message, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		public void RegisterRunOnReboot()
		{
			using (var runOnceKey = Registry.CurrentUser.CreateSubKey(RunOnceRegistryKey))
			{
				runOnceKey.SetValue(this.ApplicationName, InstallationSettings.GetCDInstallPath("setup.exe") + " " + AutomatedArgument);
			}
		}

		public void UnRegisterRunOnReboot()
		{
			using (var runOnceKey = Registry.CurrentUser.OpenSubKey(RunOnceRegistryKey, true))
			{
				if (runOnceKey != null)
				{
					runOnceKey.DeleteValue(this.ApplicationName);
				}
			}
		}

		internal const string RunOnceRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\RunOnce";
	}
}

