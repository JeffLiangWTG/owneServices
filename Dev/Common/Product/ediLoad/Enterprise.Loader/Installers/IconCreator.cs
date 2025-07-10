using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Native;
using Enterprise.Client.Common;
using Enterprise.RemoteDesktopServices;
using Microsoft.Win32;

namespace Enterprise.Loader
{
	class IconCreator : AppManagerInvoker, IAppManagerInvocable
	{
		const string Hkcu = "HKEY_CURRENT_USER";
		const string Hklm = "HKEY_LOCAL_MACHINE";
		ClientSetupHelper setupHelper;
		readonly Type typeOfIconCreationDialog;

		public IconCreator(ClientInstallation installation, Type typeOfIconCreationDialog)
			: base(installation)
		{
			this.typeOfIconCreationDialog = typeOfIconCreationDialog;
		}

		IconCreator()
			: base(null)
		{
		}

		public bool AlreadyAskedAboutIcons()
		{
			try
			{
				if (HasRegistryValue(Hkcu) || HasRegistryValue(Hklm))
				{
					return true;
				}
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (SecurityException)
			{
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		public string GetIconName(IconCreationOptions options)
		{
			return "CargoWise One" + (string.IsNullOrEmpty(options.InstanceDescription) ? "" : " (" + options.InstanceDescription + ")") + ".lnk";
		}

		protected virtual ClientSetupHelper GetNewSetupHelper()
		{
			return new ClientSetupHelper();
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			IconCreationOptions options;
			using (IIconCreationDialog dialog = (IIconCreationDialog)Activator.CreateInstance(typeOfIconCreationDialog))
			{
				if (!string.IsNullOrEmpty(Configuration.InstanceName))
				{
					dialog.SetInstanceDescription(Configuration.DatabaseName, true);
				}
				else
				{
					dialog.SetInstanceDescription(Configuration.DatabaseName, false);
				}
				dialog.ShowDialog();
				options = dialog.IconCreationOptions;
			}

			List<string> iconPaths = GetIconPaths(options);
			if (options.AllUsers)
			{
				object[] state = { iconPaths, Path.Combine(Configuration.BaseTargetPath, EnterpriseConfiguration.StartupExeFileName), InstanceSubKeyName };
				return InvokeAppManager<IconCreator>(state, null);
			}
			else
			{
				CreateIcons(iconPaths, Path.Combine(Configuration.BaseTargetPath, EnterpriseConfiguration.StartupExeFileName), Registry.CurrentUser, InstanceSubKeyName);
				return InstallationResult.OK();
			}
		}

		protected override bool NeedsToInstallCore()
		{
			return Configuration.UILevel != UILevel.AutomatedWithNoUI && !IsRemoteAppSession && !AlreadyAskedAboutIcons();
		}

		protected virtual bool IsRemoteAppSession => new TerminalService().IsRemoteAppSession;

		EnterpriseConfiguration Configuration
		{
			get { return (EnterpriseConfiguration)Installation.Configuration; }
		}

		string InstanceSubKeyName
		{
			get
			{
				return string.IsNullOrEmpty(Configuration.InstanceName) ? Configuration.ServerName + " " + Configuration.DatabaseName : "-Instance:" + Configuration.InstanceName;
			}
		}

		ClientSetupHelper SetupHelper
		{
			get { return setupHelper ?? (setupHelper = GetNewSetupHelper()); }
		}

		static void CreateIcon(string iconPath, string startupExePath, string arguments)
		{
			string iconDirectory = Path.GetDirectoryName(iconPath);
			if (!Directory.Exists(iconDirectory))
			{
				Directory.CreateDirectory(iconDirectory);
			}
			using (ShellShortcut shortcut = new ShellShortcut(iconPath))
			{
				shortcut.Path = startupExePath;
				shortcut.WorkingDirectory = Path.GetDirectoryName(startupExePath); // Not required for the shortcut to work, but makes it look neat.
				shortcut.Arguments = arguments;
				shortcut.Save();
			}
		}

		void CreateIcons(List<string> iconPaths, string startupExePath, RegistryKey rootKey, string instanceSubKeyName)
		{
			foreach (string iconPath in iconPaths)
			{
				CreateIcon(iconPath, startupExePath, instanceSubKeyName);
			}
			SetRegistry(iconPaths, rootKey, instanceSubKeyName);
		}

		string GetFolderPath(int csidl)
		{
			StringBuilder sb = new StringBuilder(NativeMethods.MAX_PATH);
			Installation.Configuration.Services.NativeMethods.SHGetFolderPath(IntPtr.Zero, csidl | NativeMethods.CSIDL_FLAG_CREATE, IntPtr.Zero, NativeMethods.SHGFP_TYPE_CURRENT, sb);
			string result = sb.ToString();
			if ((csidl == NativeMethods.CSIDL_COMMON_PROGRAMS) || (csidl == NativeMethods.CSIDL_PROGRAMS))
			{
				result = Path.Combine(Path.Combine(result, CargoWise.Loader.Common.Configuration.CompanyName));
			}
			return result;
		}

		List<string> GetIconPaths(IconCreationOptions options)
		{
			List<string> result = new List<string>();
			if (options.Desktop)
			{
				int csidl = options.AllUsers ? NativeMethods.CSIDL_COMMON_DESKTOPDIRECTORY : NativeMethods.CSIDL_DESKTOPDIRECTORY;
				result.Add(Path.Combine(GetFolderPath(csidl), GetIconName(options)));
			}
			if (options.Programs)
			{
				int csidl = options.AllUsers ? NativeMethods.CSIDL_COMMON_PROGRAMS : NativeMethods.CSIDL_PROGRAMS;
				result.Add(Path.Combine(GetFolderPath(csidl), GetIconName(options)));
			}
			return result;
		}

		bool HasRegistryValue(string root)
		{
			return (Registry.GetValue(root + '\\' + SetupHelper.KeyName + '\\' + InstanceSubKeyName, ClientSetupHelper.ShortcutsInstalledValueName, null) != null);
		}

		void SetRegistry(List<string> iconPaths, RegistryKey rootKey, string instanceSubKeyName)
		{
			using (RegistryKey subKey = rootKey.CreateSubKey(SetupHelper.KeyName + '\\' + instanceSubKeyName))
			{
				subKey.SetValue(ClientSetupHelper.ShortcutsInstalledValueName, 1);
				subKey.SetValue(ClientSetupHelper.ShortcutPathsValueName, iconPaths.ToArray());
			}
		}

		#region IAppManagerInvocable Members

		AppManagerResult IAppManagerInvocable.Invoke(bool waitedForMutex, object state)
		{
			object[] stateAsArray = (object[])state;
			CreateIcons(
				(List<string>)stateAsArray[0],
				(string)stateAsArray[1],
				Registry.LocalMachine,
				(string)stateAsArray[2]);
			return new AppManagerResult(AppManagerResultStatus.Success);
		}

		#endregion
	}
}

