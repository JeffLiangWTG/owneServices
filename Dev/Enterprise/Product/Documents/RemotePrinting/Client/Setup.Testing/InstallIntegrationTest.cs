using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
#if NETFRAMEWORK
using Enterprise.RemotePrinting.Client.RemotePrintServer;
#endif
using Microsoft.Win32;
using NUnit.Framework;
using WTG.TestHelpers;
using SnailTestAttribute = NUnit.Framework.SnailTestAttribute;
using TestCase = NUnit.Framework.TestCase;
using TestingState = NUnit.Framework.TestingState;

namespace Enterprise.RemotePrinting.Client.Setup.Testing
{
	[TestRequiresAdministrativePrivileges("Requires admin rights to run MSI installation package, install a Windows Service, change Windows Registry")]
	public class InstallIntegrationTest : TestCase
	{
		[SnailTest]
		[DeveloperOnlyTest]
		public void TestInstall()
		{
			if (!CanRunTest())
			{
				return;
			}

			var msiFileName = GetMsiFileName();
			Install(msiFileName);
			try
			{
				AssertInstalled();
			}
			finally
			{
				Uninstall(msiFileName, ProductId);
				AssertNotInstalled();
			}
		}

		[SnailTest]
		[DeveloperOnlyTest]
		public void TestUpgradeInstallExecuteSequence()
		{
			var msiFileName = GetMsiFileName();
			var expectedExecuteSequence = new string[]
			{
				"INSTALL",
				"FindRelatedProducts",
				"AppSearch",
				"LaunchConditions",
				"ValidateProductID",
				"CostInitialize",
				"FileCost",
				"CostFinalize",
				"InstallValidate",
				"RemoveExistingProducts",
				"InstallInitialize",
				"ProcessComponents",
				"UnpublishFeatures",
				"UnregisterFonts",
				"RemoveRegistryValues",
				"RemoveShortcuts",
				"CA_SAVESTATE",
				"RemoveFiles",
				"InstallFiles",
				"CreateShortcuts",
				"WriteRegistryValues",
				"RegisterFonts",
				"RegisterUser",
				"RegisterProduct",
				"PublishFeatures",
				"PublishProduct",
				"InstallFinalize",
				"CA_RESTORESTATE"
			};

			if (!ClientIsInstalled() && !ServiceIsInstalled())
			{
				Install(msiFileName);
			}

			try
			{
				AssertInstalled();
				Install(msiFileName, expectedExecuteSequence: expectedExecuteSequence);
				AssertInstalled();
			}
			finally
			{
				Uninstall(msiFileName, ProductId);
				AssertNotInstalled();
			}
		}

		[SnailTest]
		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutomaticUpgrade()
		{
			if (!CanRunTest())
			{
				return;
			}

			var newMsiFileName = GetMsiFileName();
			var newVersionFileName = Path.ChangeExtension(newMsiFileName, "version");
			var newVersion = File.ReadAllText(newVersionFileName);

			const string ServerUrl = "http://localhost:54443/";

			using (var dummyPrintServer = new DummyPrintServer(ServerUrl, newMsiFileName, newVersion))
			{
				dummyPrintServer.Start();

				PrepareTestClientConfig(ServerUrl);

				var oldMsiFileName = AssetsHelper.FetchTestAsset("Architecture/content/RemotePrinting/TestFiles/CargoWiseOneWebPrintClientSetup.msi");
				Assert("Should have test msi file with old version of Client: " + oldMsiFileName, File.Exists(oldMsiFileName));
				Install(oldMsiFileName);

				try
				{
					AssertInstalled();

					var controller = new ServiceController(ServiceName);
					AssertNotNull("Cannot get controller for print client service", controller);
					if (controller.Status == ServiceControllerStatus.Stopped)
					{
						controller.Start();
					}

					var sw = new Stopwatch();
					sw.Start();
					string[] requests;
					do
					{
						Thread.Sleep(100);
						requests = dummyPrintServer.Requests;
					} while (requests.Length < 3 && sw.Elapsed.TotalSeconds < 600);
					sw.Stop();

					controller.Refresh();
					if (controller.Status == ServiceControllerStatus.Running)
					{
						controller.Stop();
						controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
					}

					AssertGreaterThanOrEqualTo(requests.Length, 3);

#if NETFRAMEWORK
					const string CheckUpdateRequestPrefix = nameof(RemotePrintingService.CheckClientUpdate) + "|";
#else
					const string CheckUpdateRequestPrefix = "CheckClientUpdate|";
#endif

					AssertEquals("1st request should be update check from old version", CheckUpdateRequestPrefix + "-", requests[0]);
					AssertEquals("2nd request should be update check from new version", CheckUpdateRequestPrefix + newVersion, requests[1]);

					Assert("3rd request should be not be update check", !requests[2].StartsWith(CheckUpdateRequestPrefix));
					Assert("3rd request should be for new version", requests[2].EndsWith("|" + newVersion));
				}
				finally
				{
					DeleteTestClientConfig();

					Uninstall(newMsiFileName, ProductId);
					AssertNotInstalled();
				}
			}
		}

		#region Implementation

		public const string ProductId = "{D8BCA8FF-5AB8-481B-9AFF-2FBD05446A4B}";
		const string ServiceName = "ewpcsrv";
		const string InstallerFileName = "CargoWiseOneWebPrintClientSetup.msi";
		const string ConfigName = "DEFAULT";

		void AssertInstalled()
		{
			Assert("Remote Printing Client should be installed", ClientIsInstalled());
			Assert("Remote Printing Service should not be installed automatically", !ServiceIsInstalled());
		}

		public void AssertNotInstalled()
		{
			Assert("Remote Printing Client should not be installed", !ClientIsInstalled());
			Assert("Remote Printing Service should not be installed", !ServiceIsInstalled());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		bool CanRunTest()
		{
			using (var identity = WindowsIdentity.GetCurrent())
			{
				var principal = new WindowsPrincipal(identity);
				if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
				{
					Fail("Test should be run in elevated mode");
					return false;
				}
			}

			if (!TestingState.IsRunningOnDAT || Assembly.GetEntryAssembly().Location.ToLower().Contains("visual studio"))
			{
				AssertNotInstalled();
			}
			else if (ClientIsInstalled() || ServiceIsInstalled())
			{
				Assert("There is already Remote Print Client installed - skip running this test", true); // Assert(true) so test is not marked as empty
				return false;
			}

			return true;
		}

		static bool ClientIsInstalled()
		{
			if (Guid.TryParse(ProductId, out var productGuid))
			{
				var bytes = productGuid.ToByteArray();
				var sb = new StringBuilder(32);
				foreach (var b in bytes)
				{
					var bCode = b.ToString("x2");
					sb.Append(bCode[1]).Append(bCode[0]); // Add byte's hexadecimal digits in reverse order
				}
				var code = sb.ToString();

				var installedProductKey = Registry.ClassesRoot.OpenSubKey("Installer\\Products\\" + code, RegistryRights.ReadKey);

				return installedProductKey != null;
			}
			return false;
		}

		static bool ServiceIsInstalled()
		{
			var filter = $"SELECT StartMode, State FROM Win32_Service WHERE Name = '{ServiceName}'";
			using (var svc = new ManagementObjectSearcher(filter))
			{
				var service = svc.Get().Cast<ManagementObject>().FirstOrDefault();
				return service != null;
			}
		}

		public string GetMsiFileName()
		{
			var msiFileName = Path.Combine(ExecutableDirectory, InstallerFileName);
			if (!File.Exists(msiFileName))
			{
				msiFileName = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), InstallerFileName);
			}

			Assert("Should have Client setup msi file in binary folder: " + msiFileName, File.Exists(msiFileName));

			return msiFileName;
		}

		void Install(string msiFileName, int expectedResult = 0, string[] expectedExecuteSequence = null)
		{
			var logFileName = TempForTest.GetTempFileName();
			var result = ExecuteProcess("msiexec.exe", $@"/i ""{msiFileName}"" MSIRESTARTMANAGERCONTROL=Disable /quiet /norestart /l*v ""{logFileName}""");

			if (expectedExecuteSequence != null)
			{
				AssertInstallExecuteSequence(logFileName, expectedExecuteSequence);
			}

			AssertInstallResult(expectedResult, result, logFileName, "Install");
		}

		public void Uninstall(string msiFileName, string productId, int expectedResult = 0)
		{
			var logFileName = TempForTest.GetTempFileName();
			var retries = 2;
			int result;
			do
			{
				// Uninstall using productId instead of msi file, otherwise service task is not removed for some reason
				result = ExecuteProcess("msiexec.exe", $@"/x ""{productId}"" MSIRESTARTMANAGERCONTROL=Disable /quiet /norestart /l ""{logFileName}""");
				retries--;
				if (result != expectedResult && retries > 0)
				{
					Thread.Sleep(100);
				}
			} while (result != expectedResult && retries > 0);
			AssertInstallResult(expectedResult, result, logFileName, "Uninstall");
		}

		static int ExecuteProcess(string processName, string arguments)
		{
			var processStartInfo = new ProcessStartInfo(processName, arguments)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
			};

			var process = Process.Start(processStartInfo);
			if (process == null)
			{
				return -1000;
			}
			process.WaitForExit();

			return process.ExitCode;
		}

		void AssertInstallExecuteSequence(string logFileName, string[] expectedExecuteSequence)
		{
			var realExecuteSequence = new List<string>();
			var logs = File.ReadAllLines(logFileName);
			foreach (var line in logs)
			{
				if (line.StartsWith("Action start", StringComparison.OrdinalIgnoreCase))
				{
					realExecuteSequence.Add(line.Substring(23, line.Length - 24));
				}
			}

			AssertContainsExactElementsInExactOrder(expectedExecuteSequence, realExecuteSequence.ToArray());
		}

		void AssertInstallResult(int expectedResult, int result, string logFileName, string operationName)
		{
			try
			{
				var log = File.Exists(logFileName) ? Environment.NewLine + File.ReadAllText(logFileName) : string.Empty;

				if (expectedResult != result)
				{
					AssertEquals(operationName + " failed" + log, expectedResult, result);
				}

				if (!string.IsNullOrEmpty(log) && operationName == "Uninstall")
				{
					AssertContains("Should call CA_DELETE during uninstall", "CA_DELETE", log);
					AssertNotContains("Should not call CA_SAVESTATE during uninstall", "CA_SAVESTATE", log);
					AssertNotContains("Should not call CA_RESTORESTATE during uninstall", "CA_RESTORESTATE", log);
				}
			}
			finally
			{
				DeleteIfExists(logFileName);
			}
		}

		public void PrepareTestClientConfig(string serverUrl)
		{
			var config = new WebClientConfiguration
			{
				WebServiceUrl = serverUrl,
				WebServiceUser = "user1",
				WebServicePwd = "pwd",
				RequestPauseInSeconds = 30,
				LocalMachineName = Environment.MachineName,
				ProxyEnabled = false,
				ProxyAddress = "",
				ProxyPort = 0,
				ProxyUser = "",
				ProxyPwd = "",
				ProxyUseDefaultSystemSettings = false,
				WindowsServiceConfigurationSelected = true,
				SecondsBetweenScanForNewPrinters = 600,
			};

			var registryManager = new ConnectionRegistryManagerForIntegrationTest();

			// Save both to 64- and 32-bit branches:
			// old client saves config in 64-bit registry,
			// new client saves config in 32-bit registry (WOW6432Node)

			registryManager.RegistryViewMode = RegistryView.Registry64;
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(config);

			registryManager.RegistryViewMode = RegistryView.Registry32;
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(config);
		}

		public void DeleteTestClientConfig()
		{
			var registryManager = new ConnectionRegistryManagerForIntegrationTest();

			registryManager.RegistryViewMode = RegistryView.Registry64;
			registryManager.DeleteFromRegistry(ConfigName);

			registryManager.RegistryViewMode = RegistryView.Registry32;
			registryManager.DeleteFromRegistry(ConfigName);
		}

		class ConnectionRegistryManagerForIntegrationTest : ConnectionRegistryManager
		{
			public RegistryView RegistryViewMode { get; set; } = RegistryView.Default;

			protected override RegistryKey RegistryRoot => RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryViewMode);
		}

		#endregion
	}
}
