using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Common
{
	class WindowsRegistryAdapterTest : TestCaseWithFactory
	{
		[TestRequiresAdministrativePrivileges("Registry write")]
		[ExpectNoExceptions]
		public void TestRegistrySettingsAreAdjustedCorrectly()
		{
			using (new DisposableAction(() => DeleteRegistryDwordParameter(TestRegistrySubKey, TestRegistryParameter)))
			{
				//Arrange
				var registryAdapter = new WindowsRegistryAdapter();
				SetRegistryDwordParameter(TestRegistrySubKey, TestRegistryParameter, 4000);

				//Act
				registryAdapter.SetLocalMachineDwordRegistryValue(TestRegistrySubKey, TestRegistryParameter, 65534);

				//Assert
				var newValue = GetRegistryDwordParameter(TestRegistrySubKey, TestRegistryParameter);
				NUnit.Framework.Assert.That(newValue, Is.EqualTo(65534));
			}
		}

		[TestRequiresAdministrativePrivileges("Registry write")]
		[ExpectNoExceptions]
		public void TestRegistrySettingsAreAddedWhenMissing()
		{
			using (new DisposableAction(() => DeleteRegistryDwordParameter(TestRegistrySubKey, TestRegistryParameter)))
			{
				//Arrange
				var registryAdapter = new WindowsRegistryAdapter();
				DeleteRegistryDwordParameter(TestRegistrySubKey, TestRegistryParameter);

				//Act
				registryAdapter.SetLocalMachineDwordRegistryValue(TestRegistrySubKey, TestRegistryParameter, 65534);

				//Assert
				var newValue = GetRegistryDwordParameter(TestRegistrySubKey, TestRegistryParameter);
				NUnit.Framework.Assert.That(newValue, Is.EqualTo(65534));
			}
		}

		[TestRequiresAdministrativePrivileges("Registry write")]
		[ExpectNoExceptions]
		public void TestRegistryIsRead()
		{
			using (new DisposableAction(() => DeleteRegistryDwordParameter(TestRegistrySubKey, TestRegistryParameter)))
			{
				//Arrange
				const int value = 457457;
				var registryAdapter = new WindowsRegistryAdapter();
				SetRegistryDwordParameter(TestRegistrySubKey, TestRegistryParameter, value);

				//Act
				var read = registryAdapter.ReadLocalMachineDwordRegistryValue(TestRegistrySubKey, TestRegistryParameter);

				//Assert
				NUnit.Framework.Assert.That(read, Is.EqualTo(value));
			}
		}

		[TestRequiresAdministrativePrivileges("Registry write")]
		[ExpectNoExceptions]
		public void TestRegistryReadsNullWhenNotFound()
		{
			//Arrange
			var registryAdapter = new WindowsRegistryAdapter();
			DeleteRegistryDwordParameter(TestRegistrySubKey, TestRegistryParameter);

			//Act
			var readValue = registryAdapter.ReadLocalMachineDwordRegistryValue(TestRegistrySubKey, TestRegistryParameter);

			//Assert
			NUnit.Framework.Assert.That(readValue, Is.Null, "Should read no value from registry - should be [null]");
		}

		[TestRequiresAdministrativePrivileges("Registry write")]
		[ExpectNoExceptions]
		public void TestRegistryReadsNullWhenPathIsInvalid()
		{
			//Arrange
			var registryAdapter = new WindowsRegistryAdapter();

			//Act
			var readValue = registryAdapter.ReadLocalMachineDwordRegistryValue("SOME_INVALID_PATH", "SOME_SETTING");

			//Assert
			NUnit.Framework.Assert.That(readValue, Is.Null, "Should read no value from registry - should be [null]");
		}

		int? GetRegistryDwordParameter(string subkey, string parameterName)
		{
			using (var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
			using (var parametersKey = hklm32.OpenSubKey(subkey, true))
			{
				return (int?)parametersKey.GetValue(parameterName);
			}
		}

		void SetRegistryDwordParameter(string subkey, string parameterName, int? parameterValue)
		{
			if (!parameterValue.HasValue)
			{
				DeleteRegistryDwordParameter(subkey, parameterName);
			}
			else
			{
				using (var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				using (var parametersKey = hklm32.OpenSubKey(subkey, true))
				{
					parametersKey.SetValue(parameterName, parameterValue, RegistryValueKind.DWord);
				}
			}
		}

		void DeleteRegistryDwordParameter(string subkey, string parameterName)
		{
			using (var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
			using (var parametersKey = hklm32.OpenSubKey(subkey, true))
			{
				parametersKey.DeleteValue(parameterName, throwOnMissingValue: false);
			}
		}

		public const string TestRegistrySubKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
		public const string TestRegistryParameter = "MaxUserPortTest";
	}
}
