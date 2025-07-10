using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.Upgrades.Postinstall.Testing
{
	[TestRequiresAdministrativePrivileges("Changes registry setting")]
	class EdiUrlRegistrationTest
	{
		[Test]
		public void TestInstallExcludingDependencies()
		{
			var win8OriginalValue = GetValueSet(@"Software\Classes\EdiEnterprise.edient");
			var win7OriginalValue = GetValueSet(@"Software\Classes\edient");

			try
			{
				Registry.LocalMachine.DeleteSubKeyTree(@"Software\Classes\EdiEnterprise.edient", false);
				Registry.LocalMachine.DeleteSubKeyTree(@"Software\Classes\edient", false);

				var item = new EdiUrlRegistration(Installation);
				Assert.That(item.NeedsToInstall(), Is.EqualTo(true));

				item.Install(InstallationResults);
				if (InstallationResults.ErrorCount > 0)
				{
					throw new Exception(InstallationResults.GetErrorMessages());
				}

				var cwstartPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WiseTech Global", "CargoWise", "CargoWise.Start.exe");

				var value = GetValueSet(@"Software\Classes\EdiEnterprise.edient");
				Assert.That("\"" + cwstartPath + "\" \"%1\"", Is.EqualTo(value.CommandValue));
				Assert.That("\"" + cwstartPath + "\",0", Is.EqualTo(value.IconValue));

				using (RegistryKey edientKey = Registry.LocalMachine.OpenSubKey(@"Software\Classes\EdiEnterprise.edient"))
				{
					Assert.That(edientKey.GetValue(null), Is.EqualTo("URL:edient Protocol"), "edient default value");
					Assert.That(((IList<string>)edientKey.GetValueNames()).Contains("URL Protocol"), Is.EqualTo(true), "edient 'URL Protocol' value");
				}

				value = GetValueSet(@"Software\Classes\edient");
				Assert.That(value.CommandValue, Is.EqualTo("\"" + cwstartPath + "\" \"%1\""));
				Assert.That(value.IconValue, Is.EqualTo("\"" + cwstartPath + "\",0"));

				using (RegistryKey edientKey = Registry.LocalMachine.OpenSubKey(@"Software\Classes\edient"))
				{
					Assert.That(edientKey.GetValue(null), Is.EqualTo("URL:edient Protocol"), "edient default value");
					Assert.That(((IList<string>)edientKey.GetValueNames()).Contains("URL Protocol"), Is.EqualTo(true), "edient 'URL Protocol' value");
				}
			}
			finally
			{
				RestoreValueSet(win8OriginalValue);
				RestoreValueSet(win7OriginalValue);
			}
		}

		[Test]
		public void TestInstallExcludingDependencies_ErrorCondition()
		{
			configuration = new MockConfiguration(string.Empty, "");
			installation = new Installation(configuration);

			new EdiUrlRegistration(Installation).HasShownRegistryKeyException = false;
			var win8OriginalValue = GetValueSet(@"Software\Classes\EdiEnterprise.edient");
			var win7OriginalValue = GetValueSet(@"Software\Classes\edient");

			try
			{
				Registry.LocalMachine.DeleteSubKeyTree(@"Software\Classes\EdiEnterprise.edient", false);
				Registry.LocalMachine.DeleteSubKeyTree(@"Software\Classes\edient", false);

				new TestFailEdiUrlRegistration(Installation).Install(InstallationResults);

				Assert.That(InstallationResults[0].IsWarning, Is.True, "IsWarning");
				Assert.That(
					InstallationResults[0].Message,
					Is.EqualTo(@"Could not install the 'edient' url protocol. Hyperlinks and shortcuts will not work correctly. (Could not load file or assembly 'CargoWise.Loader.Common.Test, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350' or one of its dependencies. The system cannot find the file specified.).
This is an informational message only. If you would like hyperlinks and shortcuts to work correctly,
ask your system administrator to provide write access to the HKEY_CURRENT_USER registry hive on this computer."),
					"Message");

				new TestFailEdiUrlRegistration(Installation).Install(InstallationResults);
				Assert.That(InstallationResults[1].IsOK, Is.True, "The error will not be shown a second time");
			}
			finally
			{
				new EdiUrlRegistration(Installation).HasShownRegistryKeyException = false;
				RestoreValueSet(win8OriginalValue);
				RestoreValueSet(win7OriginalValue);
			}
		}

		[Test]
		public void TestDoNotOverrideCargoWiseRDPLoad()
		{
			var win8OriginalValue = GetValueSet(@"Software\Classes\EdiEnterprise.edient");
			var win7OriginalValue = GetValueSet(@"Software\Classes\edient");

			try
			{
				var rdpLoadCommand = @"""C:\Program Files\WiseTech Global\CargoWise One Remote Desktop Services\CargoWiseRDPLoad.exe"" ""%1""";
				var rdpLoadIcon = @"""C:\Program Files\WiseTech Global\CargoWise One Remote Desktop Services\CargoWiseRDPLoad.exe"",0";

				RestoreValueSet(new RegistryValueSet() { BasePath = @"Software\Classes\EdiEnterprise.edient", CommandValue = rdpLoadCommand, IconValue = rdpLoadIcon });
				RestoreValueSet(new RegistryValueSet() { BasePath = @"Software\Classes\edient", CommandValue = rdpLoadCommand, IconValue = rdpLoadIcon });

				Assert.That(new EdiUrlRegistration(Installation).NeedsToInstall(), Is.False);
			}
			finally
			{
				RestoreValueSet(win8OriginalValue);
				RestoreValueSet(win7OriginalValue);
			}
		}

		[Test]
		public void TestDoNotRunWhenCargoWiseOneStartIsAlreadySet()
		{
			var win8OriginalValue = GetValueSet(@"Software\Classes\EdiEnterprise.edient");
			var win7OriginalValue = GetValueSet(@"Software\Classes\edient");

			try
			{
				var cwLoadCommand = @"""C:\Program Files\WiseTech Global\CargoWise\CargoWise.Start.exe"" ""%1""";
				var cwLoadIcon = @"""C:\Program Files\WiseTech Global\CargoWise\CargoWise.Start.exe"",0";

				RestoreValueSet(new RegistryValueSet() { BasePath = @"Software\Classes\EdiEnterprise.edient", CommandValue = cwLoadCommand, IconValue = cwLoadCommand });
				RestoreValueSet(new RegistryValueSet() { BasePath = @"Software\Classes\edient", CommandValue = cwLoadIcon, IconValue = cwLoadIcon });

				Assert.That(new EdiUrlRegistration(Installation).NeedsToInstall(), Is.False);
			}
			finally
			{
				RestoreValueSet(win8OriginalValue);
				RestoreValueSet(win7OriginalValue);
			}
		}

		#region Test Classes

		class TestFailEdiUrlRegistration : EdiUrlRegistration
		{
			TestFailEdiUrlRegistration()
				: this(null)
			{ }

			public TestFailEdiUrlRegistration(Installation installation)
				: base(installation)
			{
			}

			protected override RegistryKey CreateSubKey(RegistryKey parentKey, string subKeyName)
			{
				throw new UnauthorizedAccessException();
			}
		}

		#endregion

		#region Implementation

		[SetUp]
		public void Setup()
		{
			configuration = new Configuration();
			installation = new Installation(configuration);
			installationResults = null;
		}

		Installation Installation
		{
			get
			{
				return installation;
			}
		}
		Installation installation;

		Configuration configuration;

		InstallationResultCollection InstallationResults
		{
			get
			{
				if (installationResults == null)
				{
					installationResults = new InstallationResultCollection();
				}
				return installationResults;
			}
		}
		InstallationResultCollection installationResults;

		RegistryValueSet GetValueSet(string registryPath)
		{
			var valueSet = new RegistryValueSet();
			valueSet.BasePath = registryPath;
			using (var iconKey = Registry.LocalMachine.OpenSubKey(registryPath + @"\DefaultIcon"))
			using (var commandKey = Registry.LocalMachine.OpenSubKey(registryPath + @"\shell\open\command"))
			{
				if (iconKey != null)
				{
					valueSet.IconValue = iconKey.GetValue(null) as string;
				}

				if (commandKey != null)
				{
					valueSet.CommandValue = commandKey.GetValue(null) as string;
				}
			}
			return valueSet;
		}

		void RestoreValueSet(RegistryValueSet valueSet)
		{
			using (var iconKey = Registry.LocalMachine.CreateSubKey(valueSet.BasePath + @"\DefaultIcon"))
			using (var commandKey = Registry.LocalMachine.CreateSubKey(valueSet.BasePath + @"\shell\open\command"))
			{
				if (valueSet.IconValue == null)
				{
					iconKey.DeleteValue(null, false);
				}
				else
				{
					iconKey.SetValue(null, valueSet.IconValue);
				}

				if (valueSet.CommandValue == null)
				{
					commandKey.DeleteValue(null, false);
				}
				else
				{
					commandKey.SetValue(null, valueSet.CommandValue);
				}
			}
		}

		class RegistryValueSet
		{
			public string BasePath { get; set; }
			public string IconValue { get; set; }
			public string CommandValue { get; set; }
		}
		#endregion
	}
}
