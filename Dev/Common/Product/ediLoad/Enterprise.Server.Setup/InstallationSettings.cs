using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.Loader.Common;
using Microsoft.Win32;

namespace Enterprise.Server.Setup
{
	public class InstallationSettings : INotifyPropertyChanged
	{
		#region Fields

		public const string UseDefaultPropertyPrefix = "UseDefault";

		bool registerInstance;
		bool useDefaultDbName;
		bool useDefaultSQLServerMachineName;
		bool useDefaultLicenseCode;

		string instanceName;
		string dbName;
		string sqlServerMachineName;
		string userNominatedInstance;
		string licenseCode;

		Dictionary<string, bool> commandLineProperties;
		List<DatabaseChoice> databaseList;
		PropertyDescriptorCollection properties;
		DatabaseChoice selectedDatabase;

		#endregion

		public InstallationSettings(Configuration configuration)
		{
			UseDefaultDbName = true;
			UseDefaultSQLServerMachineName = true;
			RegisterInstance = true;

			CDPath = configuration.StartupPath;
			DataPath = Path.GetFullPath(Path.Combine(new Configuration().BaseTargetPath, "..\\Databases\\Data"));
			LogPath = Path.GetFullPath(Path.Combine(new Configuration().BaseTargetPath, "..\\Databases\\Log"));
		}

		public string CDPath { get; private set; }

		public string GetCDInstallPath(string file)
		{
			return Path.Combine(Path.Combine(CDPath, "Install"), file);
		}

		public Dictionary<string, bool> CommandLineProperties
		{
			get
			{
				if (commandLineProperties == null)
				{
					commandLineProperties = new Dictionary<string, bool>();
					foreach (PropertyDescriptor property in Properties)
					{
						CommandLineArgumentAttribute attribute = property.Attributes[typeof(CommandLineArgumentAttribute)] as CommandLineArgumentAttribute;
						if (attribute != null)
						{
							commandLineProperties[property.Name] = attribute.HasUseDefaultProperty;
						}
					}
				}
				return commandLineProperties;
			}
		}

		public List<DatabaseChoice> DatabaseList
		{
			get
			{
				if (UseDefaultSQLServerMachineName)
				{
					if (databaseList == null)
					{
						databaseList = new List<DatabaseChoice>();

						// How to find existing instances: http://support.microsoft.com/kb/257716/en-us
						using (RegistryKey sqlServer = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server"))
						{
							if (sqlServer != null)
							{
								string[] installedInstances = sqlServer.GetValue("InstalledInstances") as string[];
								if (installedInstances != null)
								{
									foreach (string instanceName in installedInstances)
									{
										databaseList.Add(new DatabaseChoice(instanceName));
									}
								}
							}
						}

						if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432") != null)
						{
							foreach (string instanceName in GetSQLServerX64Instances())
							{
								databaseList.Add(new DatabaseChoice(instanceName));
							}
						}
					}
				}
				else
				{
					databaseList = null;
					databaseList = new List<DatabaseChoice>();
					databaseList.Add(new DatabaseChoice(userNominatedInstance));
				}

				return databaseList;
			}
		}

		[CommandLineArgument]
		public string DataPath { get; set; }

		[CommandLineArgument(true)]
		public string DbName
		{
			get { return dbName; }
			set
			{
				dbName = value.Replace("'", string.Empty).Trim();
				OnPropertyChanged(nameof(DbName));
			}
		}

		[CommandLineArgument]
		public string LogPath { get; set; }

		public DatabaseChoice SelectedDatabase
		{
			get
			{
				if (selectedDatabase == null)
				{
					selectedDatabase = DatabaseList[0];
				}
				return selectedDatabase;
			}
			set { selectedDatabase = value; }
		}

		[CommandLineArgument(true)]
		public string SQLServerMachineName
		{
			get { return sqlServerMachineName; }
			set
			{
				sqlServerMachineName = value;
				OnPropertyChanged(nameof(SQLServerMachineName));
			}
		}

		public string ServerName
		{
			get
			{
				return string.IsNullOrWhiteSpace(SelectedDatabase.InstanceName) ?
					SQLServerMachineName :
					$@"{SQLServerMachineName}\{SelectedDatabase.InstanceName}";
			}
		}

		public string UserNominatedInstance
		{
			get { return userNominatedInstance; }
			set
			{
				userNominatedInstance = value;
				databaseList = null;
				selectedDatabase = null;
				OnPropertyChanged(nameof(UserNominatedInstance));
			}
		}

		[CommandLineArgument(true)]
		public string LicenseCode
		{
			get { return licenseCode; }
			set
			{
				licenseCode = value;
				UpdateDefaultDbName();
				OnPropertyChanged("licenseCode");
			}
		}

		public string LicenseNineCode
		{
			get { return licenseCode.Length == 6 ? licenseCode.Substring(0, 3) + "___" + licenseCode.Substring(3, 3) : licenseCode; }
		}

		public bool UseDefaultDbName
		{
			get { return useDefaultDbName; }
			set
			{
				useDefaultDbName = value;
				UpdateDefaultDbName();
				OnPropertyChanged(nameof(UseDefaultDbName));
			}
		}

		public bool UseDefaultSQLServerMachineName
		{
			get { return useDefaultSQLServerMachineName; }
			set
			{
				useDefaultSQLServerMachineName = value;
				UpdateSQLServerMachineName();
				OnPropertyChanged(nameof(UseDefaultSQLServerMachineName));
			}
		}

		public bool UseDefaultLicenseCode
		{
			get { return useDefaultLicenseCode; }
			set { useDefaultLicenseCode = value; }
		}

		public bool RegisterInstance
		{
			get { return registerInstance; }
			set
			{
				registerInstance = value;
				OnPropertyChanged(nameof(RegisterInstance));
				OnPropertyChanged(nameof(NotRegisterInstance));
			}
		}

		public bool NotRegisterInstance
		{
			get { return !registerInstance; }
		}

		public string InstanceName
		{
			get { return instanceName; }
			set
			{
				instanceName = value;
				OnPropertyChanged(nameof(InstanceName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public object GetPropertyValue(string propertyName)
		{
			return Properties[propertyName].GetValue(this);
		}

		public void SetPropertyValue(string propertyName, object value)
		{
			Properties[propertyName].SetValue(this, value);
		}

		PropertyDescriptorCollection Properties
		{
			get { return properties ?? (properties = TypeDescriptor.GetProperties(this)); }
		}

		void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = PropertyChanged;
			if (propertyChanged != null)
			{
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		void UpdateDefaultDbName()
		{
			if (UseDefaultDbName)
			{
				DbName = "CargoWiseOne" + LicenseCode;
			}
		}

		void UpdateSQLServerMachineName()
		{
			if (UseDefaultSQLServerMachineName)
			{
				SQLServerMachineName = Environment.MachineName;
				UserNominatedInstance = string.Empty;
			}
			else
			{
				selectedDatabase = null;
			}
		}

		public string UpgradePackageFile
		{
			get;
			set;
		}

		public void WriteContinuationFile()
		{
			using (var writer = new StreamWriter(ContinuationFile, false, Encoding.UTF8))
			{
				foreach (PropertyDescriptor property in Properties)
				{
					if (!property.Name.StartsWith("UseDefault"))
					{
						bool hasUseDefaultProperty;
						if (CommandLineProperties.TryGetValue(property.Name, out hasUseDefaultProperty))
						{
							if ((!hasUseDefaultProperty || !(bool)Properties["UseDefault" + property.Name].GetValue(this))
								&& !string.IsNullOrEmpty(property.GetValue(this) as string))
							{
								writer.WriteLine("-" + property.Name + ":" + property.GetValue(this));
							}
						}
					}
				}
				if (!string.IsNullOrEmpty(SelectedDatabase.InstanceName))
				{
					writer.WriteLine("-SqlInstance:" + SelectedDatabase.InstanceName);
				}
			}
		}

		public void DeleteContinuationFile()
		{
			if (File.Exists(ContinuationFile))
			{
				File.Delete(ContinuationFile);
			}
		}

		public string ContinuationFile
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CargoWiseServerSetup.ini"); }
		}

		#region x64 Instances

		static string[] GetSQLServerX64Instances()
		{
			List<string> result = new List<string>();
			IntPtr sqlServerKey;
			if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Microsoft SQL Server", 0, RegSam.KEY_READ | RegSam.KEY_WOW64_64KEY, out sqlServerKey) == WinError.ERROR_SUCCESS)
			{
				try
				{
					uint type;
					uint cbData = 0;
					if (RegQueryValueEx(sqlServerKey, "InstalledInstances", IntPtr.Zero, out type, Array.Empty<byte>(), ref cbData) == WinError.ERROR_MORE_DATA)
					{
						byte[] data = new byte[cbData];
						if (RegQueryValueEx(sqlServerKey, "InstalledInstances", IntPtr.Zero, out type, data, ref cbData) == WinError.ERROR_SUCCESS)
						{
							StringBuilder instance = new StringBuilder();
							for (int i = 0; i < cbData; i += 2)
							{
								char c = (char)((data[i + 1] << 8) + data[i]);
								if (c == 0)
								{
									if (instance.Length > 0)
									{
										result.Add(instance.ToString());
										instance.Length = 0;
									}
								}
								else
								{
									instance.Append(c);
								}
							}
						}
					}
				}
				finally
				{
					RegCloseKey(sqlServerKey);
				}
			}

			return result.ToArray();
		}

		static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));

		[Flags]
		enum RegSam
		{
			/// <summary>
			/// Combines the STANDARD_RIGHTS_REQUIRED, KEY_QUERY_VALUE, KEY_SET_VALUE, KEY_CREATE_SUB_KEY, KEY_ENUMERATE_SUB_KEYS, KEY_NOTIFY, and KEY_CREATE_LINK access rights.
			/// </summary>
			KEY_ALL_ACCESS = 0xF003F,
			/// <summary>
			///  Reserved for system use.
			/// </summary>
			KEY_CREATE_LINK = 0x0020,
			/// <summary>
			/// Required to create a subkey of a registry key.
			/// </summary>
			KEY_CREATE_SUB_KEY = 0x0004,
			/// <summary>
			/// Required to enumerate the subkeys of a registry key. 
			/// </summary>
			KEY_ENUMERATE_SUB_KEYS = 0x0008,
			/// <summary>
			/// Equivalent to KEY_READ. 
			/// </summary>
			KEY_EXECUTE = 0x20019,
			/// <summary>
			///  Required to request change notifications for a registry key or for subkeys of a registry key.
			/// </summary>
			KEY_NOTIFY = 0x0010,
			/// <summary>
			/// Required to query the values of a registry key.
			/// </summary>
			KEY_QUERY_VALUE = 0x0001,
			/// <summary>
			/// Combines the STANDARD_RIGHTS_READ, KEY_QUERY_VALUE, KEY_ENUMERATE_SUB_KEYS, and KEY_NOTIFY values.
			/// </summary>
			KEY_READ = 0x20019,
			/// <summary>
			/// Required to create, delete, or set a registry value.
			/// </summary>
			KEY_SET_VALUE = 0x0002,
			/// <summary>
			///	Indicates that an application on 64-bit Windows should operate on the 32-bit registry view. For more information, see Accessing an Alternate Registry View.
			/// </summary>
			KEY_WOW64_32KEY = 0x0200,
			/// <summary>
			///	Indicates that an application on 64-bit Windows should operate on the 64-bit registry view. For more information, see Accessing an Alternate Registry View. 
			/// </summary>
			KEY_WOW64_64KEY = 0x0100,
			/// <summary>
			/// Combines the STANDARD_RIGHTS_WRITE, KEY_SET_VALUE, and KEY_CREATE_SUB_KEY access rights.
			/// </summary>
			KEY_WRITE = 0x20006,
		}

		enum WinError
		{
			/// <summary>
			/// The operation completed successfully.
			/// </summary>
			ERROR_SUCCESS = 0,
			/// <summary>
			///	More data is available.
			/// </summary>
			ERROR_MORE_DATA = 234,
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
		static extern WinError RegOpenKeyEx(
			IntPtr hKey,
			string subKey,
			uint options,
			[MarshalAs(UnmanagedType.U4)]
			RegSam sam,
			out IntPtr phkResult
		);

		[DllImport("advapi32.dll")]
		static extern WinError RegCloseKey(
			IntPtr hKey
		);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
		static extern WinError RegQueryValueEx(
			IntPtr hKey,
			string valueName,
			IntPtr lpReserved,
			out uint type,
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]
			byte[] data,
			ref uint cbData
		);

		#endregion
	}
}

