using System;
using System.IO;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;
using CargoWise.Loader.Common;
using Microsoft.Win32;

namespace Enterprise.Upgrades.Postinstall
{
	public class EdiUrlRegistration : AppManagerInvoker, IAppManagerInvocable
	{
		EdiUrlRegistration()
			: this(null)
		{ }

		public EdiUrlRegistration(Installation installation)
			: base(installation)
		{
		}

		protected override bool NeedsToInstallCore()
		{
			bool needsToInstall;
			using (var hlkm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			{
				using (var command = hlkm.OpenSubKey(@"SOFTWARE\Classes\edient\shell\open\command", false))
				{
					needsToInstall = command == null || !PathIsGood(command.GetValue(null) as string);
				}
				if (!needsToInstall)
				{
					using (var command = hlkm.OpenSubKey(@"SOFTWARE\Classes\EdiEnterprise.edient\shell\open\command", false))
					{
						needsToInstall = command == null || !PathIsGood(command.GetValue(null) as string);
					}
				}
			}

			return needsToInstall;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			var result = InstallationResult.OK();
			var appManagerResult = InvokeAppManager(this.GetType(), null, null);
			if (appManagerResult.IsError)
			{
				if (!HasShownRegistryKeyException)
				{
					result = InstallationResult.Warning(
						"Could not install the 'edient' url protocol. Hyperlinks and shortcuts will not work correctly. (" + appManagerResult.Message + ").\r\n" +
						"This is an informational message only. If you would like hyperlinks and shortcuts to work correctly,\r\n" +
						"ask your system administrator to provide write access to the HKEY_CURRENT_USER registry hive on this computer.");
					HasShownRegistryKeyException = true;
				}
			}
			return result;
		}

		protected override InstallationResult HandleAppManagerException(Exception ex)
		{
			return InstallationResult.Error(ex == null ? "" : ex.Message);
		}

		public AppManagerResult Invoke(bool waitedForMutex, object state)
		{
			var path = Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WiseTech Global", "CargoWise", "CargoWise.Start.exe");

			//windows 7 protocol handler (edient) (in windows 7 protocol handlers can't chain so we do need to fill it out twice)
			using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			using (RegistryKey software = CreateSubKey(hklm, "SOFTWARE"))
			using (RegistryKey classes = CreateSubKey(software, "Classes"))
			using (RegistryKey edient = CreateSubKey(classes, "edient"))
			using (RegistryKey defaultIcon = CreateSubKey(edient, "DefaultIcon"))
			using (RegistryKey shell = CreateSubKey(edient, "shell"))
			using (RegistryKey open = CreateSubKey(shell, "open"))
			using (RegistryKey command = CreateSubKey(open, "command"))
			{
				edient.SetValue(null, "URL:edient Protocol");
				edient.SetValue("URL Protocol", "");
				defaultIcon.SetValue(null, "\"" + path + "\",0");
				command.SetValue(null, "\"" + path + "\" \"%1\"");
			}

			//windows 8 Default Programs registration
			using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			using (RegistryKey software = CreateSubKey(hklm, "SOFTWARE"))
			using (RegistryKey edient = CreateSubKey(software, "EdiEnterprise"))
			using (RegistryKey capabilities = CreateSubKey(edient, "Capabilities"))
			using (RegistryKey urlAssociations = CreateSubKey(capabilities, "UrlAssociations"))
			using (RegistryKey fileAssociations = CreateSubKey(capabilities, "FileAssociations"))
			using (RegistryKey registeredApplications = CreateSubKey(software, "RegisteredApplications"))
			{
				capabilities.SetValue("ApplicationName", "EdiEnterprise");
				capabilities.SetValue("ApplicationIcon", path + ",0");
				capabilities.SetValue("ApplicationDescription", "EdiEnterprise");
				fileAssociations.SetValue(".edient", "EdiEnterprise.edient");
				urlAssociations.SetValue("edi", "EdiEnterprise.edient");
				urlAssociations.SetValue("edient", "EdiEnterprise.edient");
				registeredApplications.SetValue("Enterprise", @"Software\EdiEnterprise\Capabilities");
			}

			//windows 8 protocol handler (EdiEnterprise.edient)
			using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			using (RegistryKey software = CreateSubKey(hklm, "SOFTWARE"))
			using (RegistryKey classes = CreateSubKey(software, "Classes"))
			using (RegistryKey edient = CreateSubKey(classes, "EdiEnterprise.edient"))
			using (RegistryKey defaultIcon = CreateSubKey(edient, "DefaultIcon"))
			using (RegistryKey shell = CreateSubKey(edient, "shell"))
			using (RegistryKey open = CreateSubKey(shell, "open"))
			using (RegistryKey command = CreateSubKey(open, "command"))
			{
				edient.SetValue(null, "URL:edient Protocol");
				edient.SetValue("URL Protocol", "");
				defaultIcon.SetValue(null, "\"" + path + "\",0");
				command.SetValue(null, "\"" + path + "\" \"%1\"");
			}

			return new AppManagerResult(AppManagerResultStatus.Success);
		}

		bool PathIsGood(string path)
		{
			return !string.IsNullOrEmpty(path) && (path.IndexOf("CargoWiseRDPLoad.exe", StringComparison.OrdinalIgnoreCase) > 0 || path.IndexOf("CargoWise.Start.exe", StringComparison.OrdinalIgnoreCase) > 0);
		}

		protected virtual RegistryKey CreateSubKey(RegistryKey parentKey, string subKeyName)
		{
			return parentKey.CreateSubKey(subKeyName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool HasShownRegistryKeyException
		{
			get
			{
				try
				{
					using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\" + CargoWiseSubkey))
					{
						return key != null && (string)key.GetValue(ErrorShownValueName) == "true";
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return false;
				}
			}
			set
			{
				try
				{
					using (RegistryKey software = Registry.CurrentUser.OpenSubKey("Software", true))
					using (RegistryKey key = software.CreateSubKey(CargoWiseSubkey))
					{
						if (value)
						{
							key.SetValue(ErrorShownValueName, "true");
						}
						else
						{
							key.DeleteValue(ErrorShownValueName, false);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		const string CargoWiseSubkey = "CargoWise edi\\ediEnterprise";
		const string ErrorShownValueName = "EdiUrlRegistrationFailureMessageShown";
	}
}
