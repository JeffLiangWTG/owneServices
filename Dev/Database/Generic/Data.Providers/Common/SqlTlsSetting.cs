using Microsoft.Win32;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data.Providers.Common
{
	public static class SqlTlsSetting
	{
		[ThreadSafe]
		static readonly bool? encryptionSetting = null;

		const string registryKeyLocation = @"SOFTWARE\WiseTech Global\SqlTlsSettings";
		const string globalRegKey = "Global_EncryptSQLConnection";

		/// <summary>
		/// Determines whether we should encrypt connections to SQL server
		/// </summary>
		/// <param name="serverName">Server name is passed in as Enviornment variables are not yet available at this stage</param>
		/// <returns>true if encryption is required.</returns>
		public static bool ShouldEncryptSqlConnection(string serverName)
		{
			if (encryptionSetting.HasValue)
			{
				return encryptionSetting.Value;
			}

			bool? globalSetting = GetRegistryValueFor(globalRegKey);
			bool? customerSetting = GetRegistryValueFor(GetRegKeyFrom(serverName));

			return GetEncryptionSetting(globalSetting, customerSetting);
		}

		/// <summary>
		/// Get customer specific registry key from server name. 
		/// </summary>
		/// <param name="serverName"></param>
		/// <returns>The full server name. Logic updated to not return 6 letter customer code as self-hosted customers
		/// can use domain name or database name</returns>
		internal static string GetRegKeyFrom(string serverName)
		{
			return serverName.Replace('\\', '$');
		}

		/// <summary>
		///	Windows API used to get registry key value
		/// </summary>
		/// <param name="registryKey">Either the global key which applies to all DBs on that instance or key specific to a customer</param>
		/// <returns>null if key not present. bool otherwise </returns>
		static bool? GetRegistryValueFor(string registryKey)
		{
			using (RegistryKey hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
			{
				var subKey = hklm32.OpenSubKey(registryKeyLocation);
				if (subKey != null)
				{
					var registryKeyValue = subKey.GetValue(registryKey);
					if (registryKeyValue == null)
					{
						return null;
					}
					return (int)registryKeyValue == 1;
				}
			}
			return null;
		}

		/// <summary>
		/// Customer setting takes precedence over global setting
		/// </summary>
		/// <param name="globalSetting">The value for all DBs in the SQL instance</param>
		/// <param name="customerSetting">The value for the customers DB</param>
		/// <returns>true if encryption is required</returns>
		internal static bool GetEncryptionSetting(bool? globalSetting, bool? customerSetting)
		{
			if (customerSetting.HasValue)
			{
				return customerSetting.Value;
			}
			if (globalSetting.HasValue)
			{
				return globalSetting.Value;
			}
			return false;
		}
	}
}
