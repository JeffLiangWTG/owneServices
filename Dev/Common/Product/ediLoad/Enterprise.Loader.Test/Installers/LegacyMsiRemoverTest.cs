using System.ComponentModel;
using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	class LegacyMsiRemoverTest : TestCase
	{
		public void TestErrorCodesToIgnore()
		{
			AsserErrorCodesToIgnore(new Win32Exception(1605).Message, false);
			AsserErrorCodesToIgnore(new Win32Exception(1619).Message, false);
			AsserErrorCodesToIgnore(new Win32Exception(1620).Message, true);
			AsserErrorCodesToIgnore("", false);
		}

		void AsserErrorCodesToIgnore(string errorString, bool error)
		{
			var configuration = new EnterpriseConfigurationForTesting();
			var appManager = new AppManagerForTesting();
			configuration.SetAppManagerClient(appManager);
			var installation = new ClientInstallation(configuration);
			var msiRemover = new LegacyMsiRemoverForTesting(installation);
			appManager.AppManagerInvocable = msiRemover;

			var errorMessage = string.IsNullOrEmpty(errorString) ? "" : "Some Error doing operation: " + errorString + ".";
			msiRemover.InstallResultErrorMessage = errorMessage;

			var results = new InstallationResultCollection();
			msiRemover.Install(results);

			AssertEquals(1, results.Count);
			var result = results[0];

			var expectedMessage = error ? errorMessage : "";
			var expectedRemoveRegistry = !expectedMessage.Equals(errorMessage);

			if (error)
			{
				Assert(result.IsError);
				AssertEquals(1, results.ErrorCount);
				AssertEquals(0, results.WarningCount);
				AssertEquals(0, results.OKCount);
			}
			else
			{
				Assert(result.IsOK);
				AssertEquals(0, results.ErrorCount);
				AssertEquals(0, results.WarningCount);
				AssertEquals(1, results.OKCount);
			}

			AssertEquals(expectedRemoveRegistry, msiRemover.IsRemoveRegistryInstanceCalled);
			AssertEquals(expectedMessage, result.Message);
		}
	}

	class LegacyMsiRemoverForTesting : LegacyMsiRemover
	{
		public LegacyMsiRemoverForTesting(ClientInstallation installation)
			: base(installation)
		{
			IsRemoveRegistryInstanceCalled = false;
		}

		public InstallationResultCollection InvokeInstallerExposed()
		{
			return InvokeInstaller(InstallerAbsolutePath);
		}

		protected override bool NeedsToInstallCore()
		{
			productCode = "f31967cc-f0b9-4ac1-a3f5-915fed72b055";
			instanceName = "Instance.123";
			return true;
		}

		public string InstallResultErrorMessage { get; set; }
		protected override WindowsInstallerProgram CreateWindowsInstallerProgram(InstallationItem parentInstallationItem, string nameOfComponent)
		{
			return new WindowsInstallerProgramForTesting(parentInstallationItem, nameOfComponent, InstallResultErrorMessage);
		}

		protected override void RemoveRegistryInstance()
		{
			IsRemoveRegistryInstanceCalled = true;
		}

		public bool IsRemoveRegistryInstanceCalled { get; private set; }
	}

	class WindowsInstallerProgramForTesting : WindowsInstallerProgram
	{
		public WindowsInstallerProgramForTesting(InstallationItem parentInstallationItem, string nameOfComponent, string resultErrorMessage)
			: base(parentInstallationItem, nameOfComponent)
		{
			this.resultErrorMessage = resultErrorMessage;
		}

		readonly string resultErrorMessage;

		override public void Install(InstallationResultCollection results)
		{
			if (string.IsNullOrEmpty(resultErrorMessage))
			{
				results.Add(InstallationResult.OK());
			}
			else
			{
				results.Add(InstallationResult.Error(resultErrorMessage));
			}
		}
	}

	class AppManagerForTesting : MockAppManager
	{
		public IAppManagerInvocable AppManagerInvocable { get; set; }

		public override AppManagerResult Invoke(string assemblyPath, string typeName, object state, MutexRequest request)
		{
			return AppManagerInvocable.Invoke(true, state);
		}
	}

	class EnterpriseConfigurationForTesting : EnterpriseConfiguration
	{
		public override IAppManager GetNewAppManagerClient()
		{
			return appManagerClient ?? new MockAppManager();
		}

		public void SetAppManagerClient(IAppManager value)
		{
			appManagerClient = value;
		}

		IAppManager appManagerClient;
	}
}
