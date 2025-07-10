using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public class StartupCheckClientDll : AbstractApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("8D1DBACA-BDAC-44ca-8437-29F962FF942B", "Checking client DLL");

		public override int FailureExitCode => ExitCodes.StartupCheckClientDllError;

		protected override bool GetShouldExecute(CommandLineArguments arguments) => true;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
#if DEBUG
			string clientDLLToLoad = arguments[ApplicationArguments.OptionClient] as string;
			if (!String.IsNullOrWhiteSpace(clientDLLToLoad))
			{
				IDisposable overridenClientAssembly = null;

				var success = false;
				try
				{
					var clientAssembly = ClientHookLoader.Instance.GetAssemblyFromFileName(Path.Combine(AssemblyLoader.GetBinPath(), "ZClient" + clientDLLToLoad.ToUpper() + ".dll"));
					var productRegistration = ObjectFactory.Get<IProductRegistration>();

					if (productRegistration.LocalVerify() == ProductRegistrationVerifyResult.OK && !string.Equals(productRegistration.Key.EnterpriseCode, clientDLLToLoad, StringComparison.OrdinalIgnoreCase))
					{
						Globals.Message.Show(
							$@"***DEBUG MODE ONLY***
Failed to load ZClient{clientDLLToLoad}.dll
The system has a valid registration, but the enterprise code does not match the dll.",
							"Client Specific DLL Load Failure",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
						return false;
					}

					overridenClientAssembly = ClientHookLoader.Instance.OverrideClientAssemblyForTest(clientAssembly);
					Env.Registry.ExpectedClientDLL = "ZClient" + clientDLLToLoad.ToUpper();
					success = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (Globals.Message.Show(
							"***DEBUG MODE ONLY***\r\nFailed to load ZClient" + clientDLLToLoad +
							".dll\r\nMessage: " + ex.Message + "\r\n\r\nDo you want to continue?\t\t\t\t\t\t",
							"Client Specific DLL Load Failure",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question) != DialogResult.Yes)
					{
						return false;
					}
				}
				finally
				{
					if (!success)
					{
						if (overridenClientAssembly != null)
						{
							overridenClientAssembly.Dispose();
						}
					}
				}

				UpdateClientDocuments(arguments);
			}
			else
#endif
			{
				ClientDllChecker.Result clientDllCheckResult = ClientDllChecker.CheckRegistry();
				if (!clientDllCheckResult.IsOKToRun)
				{
					var caption = Res.GetString("f21498f8-2c3c-4e45-b3f1-eacdb04deeb4", "{0} Cannot Start", BrandingFactory.Instance.ProductName);
					var message = String.Format("{0}{2}{2}{1}",
						clientDllCheckResult.ErrorMessage,
						Res.GetString("8d6b8ed2-7295-4604-83f7-0dc9f418b31c", "Would you like to attempt to repair the {0} installation?", BrandingFactory.Instance.ProductName),
						System.Environment.NewLine);
					if (StartupNotification.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
					{
						TopLevelExceptionHandler.RepairCurrentInstallation();
					}
#if DEBUG
					if (
						Globals.Message.Show(
							"***DEBUG MODE ONLY***\r\nDo you want to reset your database to a non-client-specific database?",
							"Reset Client Specific DLL",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question) == DialogResult.Yes)
					{
						Env.Registry.ExpectedClientDLL = null;
						Globals.Message.Show(
							"Database reset to non-client-specific mode. You must restart " + BrandingFactory.Instance.ProductName + " for the change to take effect.");
					}
#endif
					return false;
				}
			}
			return true;
		}

#if DEBUG
		void UpdateClientDocuments(CommandLineArguments arguments)
		{
			string serverDirectory = arguments[ApplicationArguments.OptionServerDirectoryPath] as string;
			if (!string.IsNullOrEmpty(serverDirectory))
			{
				string[] documentsXmls = Directory.GetFiles(serverDirectory, "*documents.xml");
				if (documentsXmls.Length > 0)
				{
					string clientXmlFilePath = Path.Combine(serverDirectory, documentsXmls[0]);
					DbUpgrader.Data.ClientDocumentsUpgradeTask task = new DbUpgrader.Data.ClientDocumentsUpgradeTask(clientXmlFilePath);
					task.Run();
					File.SetAttributes(clientXmlFilePath, FileAttributes.Normal);
					File.Delete(clientXmlFilePath);
					Globals.Message.ShowInformation(
						string.Format("***DEBUG MODE ONLY***\r\nDocument and Report data has been upgraded successfully using client specific data file located on: {0}", clientXmlFilePath),
						"Client Documents and Reports Upgrade");
				}
			}
		}
#endif
	}
}
