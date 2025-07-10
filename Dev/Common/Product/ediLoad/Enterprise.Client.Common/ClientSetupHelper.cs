using System;
using CargoWise.Common;
using Microsoft.Win32;

namespace Enterprise.Client.Common
{
	public class ClientSetupHelper
	{
		public const string ShortcutsInstalledValueName = "ShortcutsInstalled";
		public const string ShortcutPathsValueName = "ShortcutPaths";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		public virtual string KeyName
		{
			get { return @"SOFTWARE\WiseTech Global\CargoWise One"; }
		}
	}

	public class LegacyClientSetupHelper
	{
		public const string ClientProgramFilesDirValueName = "ClientProgramFilesDir";
		public const string DefaultInstanceName = "ediEnterprise";
		public const string InstanceKeyNameHeader = "Instance.";
		public const string InstanceNameValueName = "InstanceName";
		public const string ProductCodeValueName = "ProductCode";
		public const string ServerProgramFilesDirValueName = "ServerProgramFilesDir";
		public const string ShortcutsInstalledValueName = "ShortcutsInstalled";
		public const string ShortcutPathsValueName = "ShortcutPaths";
		public const int MaxInstances = 50;

		public LegacyClientSetupHelper()
		{
		}

		public virtual string KeyName
		{
			get { return @"SOFTWARE\CargoWise edi\ediEnterprise"; }
		}

		public static string GetInstanceDescription(string instanceName)
		{
			Argument.NotNull(instanceName, nameof(instanceName));

			string result = "ediEnterprise Client";
			if (!instanceName.Equals(DefaultInstanceName, StringComparison.OrdinalIgnoreCase))
			{
				result += " (" + instanceName + ')';
			}

			return result;
		}

		public static string GetInstanceName(string instanceName, string targetDirectoryName)
		{
			string result = instanceName;
			if (string.IsNullOrEmpty(result))
			{
				result = targetDirectoryName;
				if (string.IsNullOrEmpty(result))
				{
					result = DefaultInstanceName;
				}
			}

			return result;
		}

		public RegistryKey GetInstanceSubKey(RegistryKey rootKey, string instanceName)
		{
			RegistryKey result = null;
			if (rootKey != null)
			{
				var subKeyNames = rootKey.GetSubKeyNames();

				foreach (string subKeyName in subKeyNames)
				{
					if (IsInstanceSubKey(subKeyName))
					{
						RegistryKey subKey = rootKey.OpenSubKey(subKeyName, false);
						if (subKey != null)
						{
							try
							{
								string registryInstanceName = subKey.GetValue(InstanceNameValueName) as string;
								if (string.Equals(registryInstanceName, instanceName, StringComparison.OrdinalIgnoreCase))
								{
									result = subKey;
									break;
								}
							}
							finally
							{
								//Have to move this logic to a stub method to prevent 'reference use unreached' on 'if (result == null)' in code contracts static analyzer.
								SubKeyClose_Stub(result, subKey);
							}
						}
					}
				}
			}

			return result;
		}

		void SubKeyClose_Stub(RegistryKey result, RegistryKey subKey)
		{
			Argument.NotNull(subKey, nameof(subKey));

			if (result == null)
			{
				subKey.Close();
			}
		}

		public static bool IsInstanceSubKey(string subKeyName)
		{
			if (!string.IsNullOrEmpty(subKeyName) && subKeyName.StartsWith(InstanceKeyNameHeader, StringComparison.OrdinalIgnoreCase))
			{
				string tail = subKeyName.Substring(InstanceKeyNameHeader.Length);
				if (tail.Length > 0)
				{
					foreach (char c in tail)
					{
						if (!char.IsDigit(c))
						{
							return false;
						}
					}

					return true;
				}
			}

			return false;
		}
	}
}

