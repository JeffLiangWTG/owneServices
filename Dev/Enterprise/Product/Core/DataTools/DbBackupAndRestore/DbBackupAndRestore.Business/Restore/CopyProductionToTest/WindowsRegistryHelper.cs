using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace Enterprise.DataTools.DbBackupAndRestore.Business;

public interface IWindowsRegistryHelper
{
	string GetAWSAccessKeyFromLocalHostWindowsRegistry();
}
public class WindowsRegistryHelper : IWindowsRegistryHelper
{
	public string GetAWSAccessKeyFromLocalHostWindowsRegistry()
	{
		const string keyPath = @"SOFTWARE\WiseTech Global\CargoWise\Database Backup and Restore\AWS Credentials";
		const string valueName = "S3 Read-Only Access Key Generator";

		using var key = Registry.LocalMachine.OpenSubKey(keyPath);
		var value = key?.GetValue(valueName);

		if (value != null)
		{
			var encryptedData = Convert.FromBase64String(value.ToString());
			var decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.LocalMachine);
			var decryptedString = Encoding.UTF8.GetString(decryptedData);

			return decryptedString;
		}

		throw new KeyNotFoundException("No access keys found in Registry");
	}
}

