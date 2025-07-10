using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.Loader.Common;
using Enterprise.Client.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace Enterprise.Server.Setup
{
	sealed class InstallationSettingsValidator
	{
		readonly SetupConfiguration configuration;
		readonly IWin32Window owner;
		readonly CargowiseSetupSqlContextManager sqlContextManager;

		public InstallationSettingsValidator(SetupConfiguration configuration, IWin32Window owner)
		{
			this.configuration = configuration;
			this.owner = owner;
			this.sqlContextManager = (CargowiseSetupSqlContextManager)Application.ServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();
		}

		public void Validate(CancelEventArgs e)
		{
			CheckLicenseCode(e);
			if (!e.Cancel)
			{
				CheckRegisterInstance(e);
			}

			if (!e.Cancel)
			{
				CheckDirectoryAndOtherNamesAreValid(e);
			}

			if (!e.Cancel)
			{
				CheckSqlVersion(e);
			}

			if (!e.Cancel)
			{
				CheckDbDoesNotExist(e);
			}

			if (!e.Cancel)
			{
				CheckThatLogAndDataAreOnDifferentDriveLetters(e);
			}
		}

		InstallationSettings Settings
		{
			get { return configuration.InstallationSettings; }
		}

		void CheckLicenseCode(CancelEventArgs e)
		{
			if (Settings.LicenseCode == null || Settings.LicenseCode.Length != 6)
			{
				ShowError("Please enter a valid Product Key");
				e.Cancel = true;
			}
		}

		void CheckRegisterInstance(CancelEventArgs e)
		{
			if (Settings.RegisterInstance)
			{
				if (!IsAlphaNumeric(Settings.InstanceName))
				{
					ShowError("The specified Instance Name contains non alphanumeric characters.", "Invalid Instance Name");
					e.Cancel = true;
				}
				else if (!configuration.Services.CargoWiseOneInstanceClass.ExistsInCurrentSchema())
				{
					ShowQuestion(e, CargoWiseOneInstanceManager.SchemaModificationWarning, "Would you like to continue?", "Warning: Active Directory Schema Modification");
				}
			}
		}

		void CheckDbDoesNotExist(CancelEventArgs e)
		{
			try
			{
				sqlContextManager.OpenConnection(Settings.ServerName);

				if (sqlContextManager.CurrentContext.ExecuteScalar("SELECT DB_ID('" + Settings.DbName + "')") != DBNull.Value)
				{
					e.Cancel = true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
			finally
			{
				sqlContextManager.Disconnect();
			}

			if (e.Cancel)
			{
				ShowError("There is already an existing database with the specified Database Name (" + Settings.DbName + ").");
			}
		}

		void CheckDirectoryAndOtherNamesAreValid(CancelEventArgs e)
		{
			bool valid = false;
			if (!IsAlphaNumeric(Settings.DbName))
			{
				ShowError("The specified Database name contains non alphanumeric characters.", "Invalid Database Name");
			}
			else if (Settings.DbName.Length > 35)
			{
				ShowError("The specified Database is longer than 35 characters.", "Invalid Database Name");
			}
			else if (!IsPathValid(Settings.DataPath))
			{
				ShowError("The specified Database Data Files folder path is invalid. All folder paths must be valid.", "Invalid Folder Path");
			}
			else if (!IsPathValid(Settings.LogPath))
			{
				ShowError("The specified Database Log Files folder path is invalid. All folder paths must be valid.", "Invalid Folder Path");
			}
			else
			{
				valid = true;
			}
			e.Cancel = !valid;
		}

		bool IsAlphaNumeric(string dbName)
		{
			return dbName != null && Regex.IsMatch(dbName, @"^[A-Z][A-Z0-9]*$", RegexOptions.IgnoreCase);
		}

		public static object RegistryGetValue(RegistryHive registryHive, string keyName, string valueName, Object defaultValue)
		{
			var registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
			var registryKey = RegistryKey.OpenBaseKey(registryHive, registryView);

			if (registryKey != null)
			{
				registryKey = registryKey.OpenSubKey(keyName);

				if (registryKey != null)
				{
					return registryKey.GetValue(valueName, defaultValue);
				}
			}

			return defaultValue;
		}

		void CheckSqlVersion(CancelEventArgs e)
		{
			try
			{
				sqlContextManager.OpenConnection(Settings.ServerName);

				var instanceName = Settings.SelectedDatabase.InstanceName;
				var baseKeyPath = @"SOFTWARE\Microsoft\Microsoft SQL Server\";
				var keyName = RegistryGetValue(RegistryHive.LocalMachine, baseKeyPath + @"Instance Names\SQL", instanceName, null) as string;
				if (string.IsNullOrEmpty(keyName))
				{
					keyName = instanceName;
				}
				baseKeyPath += keyName;

				var versionString = RegistryGetValue(RegistryHive.LocalMachine, baseKeyPath + @"\Setup", "PatchLevel", null) as string;
				if (string.IsNullOrEmpty(versionString))
				{
					versionString = RegistryGetValue(RegistryHive.LocalMachine, baseKeyPath + @"\MSSQLServer\CurrentVersion", "CurrentVersion", null) as string;
				}

				Exception versionEx = null;
				Version version = null;
				if (string.IsNullOrEmpty(versionString))
				{
					try
					{
						version = GetServerVersion();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						versionEx = ex;
					}
				}
				else
				{
					version = new Version(versionString);
				}

				if (version == null)
				{
					string message =
						"Unable to determine the version of the selected SQL Server instance. Please make sure that it has been installed properly and that the service is running. " + BrandingFactory.Instance.ProductName + " only supports SQL Server 2012 or later." +
						((versionEx == null) ? "" : (Environment.NewLine + versionEx.ToString()));
					ShowError(message);
					e.Cancel = true;
				}
				else if (version.Major < 11)
				{
					var errMsg = string.Format(@"instanceName = {0}
baseKeyPath = {1}
keyName = {2}
versionString = {3}
version = {4}
{5} requires SQL Server 2012 or later.",
								string.IsNullOrEmpty(instanceName) ? "" : instanceName,
								string.IsNullOrEmpty(baseKeyPath) ? "" : baseKeyPath,
								string.IsNullOrEmpty(keyName) ? "" : keyName,
								string.IsNullOrEmpty(versionString) ? "" : versionString,
								version.ToString(),
								BrandingFactory.Instance.ProductName);
					ShowError(errMsg);
					e.Cancel = true;
				}

				Exception editionEx = null;
				SqlServerEdition? edition = null;
				var editionTypeString = RegistryGetValue(RegistryHive.LocalMachine, baseKeyPath + @"\Setup", "EditionType", null) as string;
				if (string.IsNullOrEmpty(editionTypeString))
				{
					try
					{
						edition = GetServerEdition();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						editionEx = ex;
					}
				}
				else if (editionTypeString.StartsWith("Developer") || editionTypeString.StartsWith("Enterprise"))
				{
					edition = SqlServerEdition.EnterpriseDeveloper;
				}
				else
				{
					edition = SqlServerEdition.Other;
				}

				if (!edition.HasValue)
				{
					string message =
						"Unable to determine the edition of the selected SQL Server instance. Please make sure that it has been installed properly and that the service is running. " + BrandingFactory.Instance.ProductName + " only supports MS SQL Server Enterprise Edition." +
						((editionEx == null) ? "" : (Environment.NewLine + editionEx.ToString()));
					ShowError(message);
					e.Cancel = true;
				}
				else if (edition.Value != SqlServerEdition.EnterpriseDeveloper)
				{
					ShowError(BrandingFactory.Instance.ProductName + " only supports MS SQL Server Enterprise Edition.");
					e.Cancel = true;
				}
			}
			finally
			{
				sqlContextManager.Disconnect();
			}
		}

		Version GetServerVersion()
		{
			Version result = null;
			using (var reader = sqlContextManager.CurrentContext.ExecuteReader("EXEC xp_msver 'ProductVersion'"))
			{
				if (reader.Read())
				{
					result = new Version(reader[3].ToString());
				}
			}
			return result;
		}

		SqlServerEdition GetServerEdition()
		{
			var edition = Convert.ToInt32(sqlContextManager.CurrentContext.ExecuteScalar("SELECT CONVERT(int, SERVERPROPERTY('EngineEdition'))"));

			switch (edition)
			{
				case (int)SqlServerEdition.StandardWorkgroup:
					return SqlServerEdition.StandardWorkgroup;
				case (int)SqlServerEdition.EnterpriseDeveloper:
					return SqlServerEdition.EnterpriseDeveloper;
				case (int)SqlServerEdition.Express:
					return SqlServerEdition.Express;
				default:
					return SqlServerEdition.Other;
			}
		}

		public enum SqlServerEdition
		{
			Other = 0,               // (1 = deprecated, 5 = SQL Azure)
			StandardWorkgroup = 2,   // Standard, Web, Business Intelligence, Small Business Server, Workgroup
			EnterpriseDeveloper = 3, // Enterprise (all types), Developer, Evaluation, Data Center
			Express = 4,             // Express (all types), Windows Embedded SQL
		}

		// What we really care about is that they're on different physical drives.
		// To test this properly we would need to look at the logical volumes and the physical devices behind them.
		// We would need to consider things like mounting a volume in a subdirectory of another drive,
		// and a single physical disk partitioned into multiple logical drives.
		// This is beyond our scope at the present time. We'll take the easy way out and just compare drive letters.
		void CheckThatLogAndDataAreOnDifferentDriveLetters(CancelEventArgs e)
		{
			string dataRoot = "data";
			string logRoot = "log";

			try
			{
				dataRoot = Path.GetPathRoot(Settings.DataPath);
				logRoot = Path.GetPathRoot(Settings.LogPath);
			}
			catch (ArgumentException)
			{
				// invalid input is checked later on
			}

			if (string.Equals(dataRoot, logRoot, StringComparison.OrdinalIgnoreCase))
			{
				ShowQuestion(
					e,
					"To improve performance and reliability of " + BrandingFactory.Instance.ProductName + " in live production environments, we strongly recommend that\r\nData and Database Log files are stored on different drives.\r\n\r\nIf you are using this system for testing, demonstration, or educational purposes, you can ignore this warning.",
					"Do you want to continue with these files on the same drive?",
					"Warning: Data and Database Logs");
			}
		}

		static bool IsPathValid(string path)
		{
			bool result =
				path != null &&
				path.Length > 1 &&
				char.IsLetter(path[0]) &&
				path[1] == ':' &&
				Path.IsPathRooted(path) &&
				path.IndexOfAny(Path.GetInvalidPathChars()) < 0;

			if (result)
			{
				string[] parts = path.Substring(2).Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string part in parts)
				{
					if (!FileNameValidator.IsFileNameValid(part))
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		void ShowError(string text)
		{
			ShowError(text, "Error");
		}

		void ShowError(string text, string caption)
		{
			configuration.Notifier.ShowError(owner, text, caption);
		}

		void ShowQuestion(CancelEventArgs e, string info, string question, string caption)
		{
			if (configuration.UILevel == UILevel.AutomatedWithNoUI)
			{
				configuration.Services.EventLog.WriteEntry(configuration.ApplicationName, info, EventLogEntryType.Warning);
			}
			else
			{
				DialogResult result = configuration.Services.MessageBox.Show(
					owner,
					info + Environment.NewLine + Environment.NewLine + question,
					caption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					MessageBoxDefaultButton.Button2);
				if (result != DialogResult.Yes)
				{
					e.Cancel = true;
				}
			}
		}

		#region Native

		[DllImport("Netapi32", CharSet = CharSet.Auto)]
		static extern int NetApiBufferFree(IntPtr buffer);

		[DllImport("Netapi32.dll", SetLastError = true)]
		static extern int NetShareGetInfo(
			[MarshalAs(UnmanagedType.LPWStr)] string servername,
			[MarshalAs(UnmanagedType.LPWStr)] string netname,
			int level,
			out IntPtr bufptr);

		#endregion
	}
}
