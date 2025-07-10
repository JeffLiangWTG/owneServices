using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.IO;
using Enterprise.Upgrades;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace CargoWiseOne.WebInfrastructure.Integration.Test.Helpers
{
	class DebugBuildPackageMaker : IDisposable
	{
		public DebugBuildPackageMaker()
		{
			tempDirectory = new();
			localFiles = new();

			applicationDir = Path.Combine(tempDirectory.DirectoryName, "Distribution", "Application");
			Directory.CreateDirectory(applicationDir);
		}

		public UpgradeInfoExtended CreateAndUploadDebugBuildPackage(string serverName, System.Data.Common.DbConnection sqlConnection, Version version, string status = "CUR")
		{
			var edpFilePath = CreateEdpPackageFile();

			try
			{
				var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));
				return upgradeManager.UploadUpgradePackage(edpFilePath, version, DateTime.UtcNow, status, string.Empty, null);
			}
			finally
			{
				File.Delete(edpFilePath);
			}

			string CreateEdpPackageFile()
			{
				CreateTestWebDeployZip();
				EnumerateLocalFiles();
				CopyLocalFiles();

				var edpFile = TempForTest.GetTempFileName();
				new FastZip().CreateZip(edpFile, tempDirectory.DirectoryName, recurse: true, string.Empty);

				return edpFile;
			}

			void CopyLocalFiles()
			{
				var sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				foreach (var fileName in localFiles)
				{
					File.Copy(Path.Combine(sourceDir, fileName), Path.Combine(applicationDir, fileName));
				}

				File.Copy(Path.Combine(sourceDir, TestWebDeployZip), Path.Combine(applicationDir, EnterpriseWebDeployZip), overwrite: true);
			}

			void EnumerateLocalFiles()
			{
				var sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				localFiles.UnionWith(new HashSet<string>(Directory.GetFiles(sourceDir, "*.dll", SearchOption.TopDirectoryOnly)
					.Select(Path.GetFileName)
					.Where(fileName =>
						MustIncludeEndsWithNames.Any(fileName.EndsWith)
						|| !ExcludedStartsWithNames.Any(fileName.StartsWith)
						&& !ExcludedEndsWithNames.Any(fileName.EndsWith))
				));

				localFiles.UnionWith(new HashSet<string>(Directory.GetFiles(sourceDir, "Enterprise.ResourceStrings*.zrs", SearchOption.TopDirectoryOnly)
					.Select(Path.GetFileName)));

				localFiles.UnionWith(new[] { "CargoWiseOne.WebInfrastructure.Updater.exe", "CurrentVersionWriter.exe", });
			}

			void CreateTestWebDeployZip()
			{
				var assemblyFilePath = Assembly.GetExecutingAssembly().Location;
				var binPath = Path.GetDirectoryName(assemblyFilePath);
				var appConfig = ConfigurationManager.OpenExeConfiguration(assemblyFilePath);
				var webDeployXmlFileName =
					appConfig.AppSettings.Settings[WebDeployXmlFile]?.Value
					?? throw new NotSupportedException("You need to specify the test web deploy xml file path in the app.Config file.");

				var webDeployXmlFilePath = Path.Combine(binPath, webDeployXmlFileName);
				if (!File.Exists(webDeployXmlFilePath))
				{
					throw new NotSupportedException($"TestWebDeploy xml file: {webDeployXmlFilePath} not found.");
				}

				string exePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "net8.0", "WebDeployBuilder.exe");

				string arguments = $"{webDeployXmlFilePath} {binPath}";

				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = exePath,
					Arguments = arguments,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				};
				Process process = Process.Start(startInfo);

				string error = process.StandardError.ReadToEnd();

				if (!string.IsNullOrEmpty(error))
				{
					throw new Exception($"WebDeployBuilder failed: {error}");
				}

				process.WaitForExit();
			}
		}

		public void Dispose()
		{
			tempDirectory.Dispose();
		}

		static List<string> ExcludedStartsWithNames => new()
		{
			"CargoWise.Customs",
			"CargoWise.RefDataRepo",
			"WTG.ProductionRules",
			"Enterprise.Warehouse",
			"Enterprise.Transport",
			"Enterprise.Workflow",
			"Enterprise.ServiceManager",
			"Enterprise.Telematics",
			"Enterprise.Universal",
			"Enterprise.Workflow",
			"Enterprise.WebCFS",
			"CargoWise.Glow",
			"CargoWise.Bi",
			"CargoWise.eHub",
			"CargoWise.BarcodeReader",
			"betterlistviewexpress",
			"CargoWise.eServices",
			"CargoWise.PAVE",
			"CargoWise.RefDbRepo",
			"CargoWise.ServiceManager",
			"CargoWise.Tools",
			"Enterprise.Accounting",
			"Enterprise.ArchiveManager",
			"Enterprise.AuditDataServices",
			"Enterprise.Barcode",
			"Enterprise.Billing",
			"Enterprise.BlazorWinFormsInterop.",
			"Enterprise.BufferManagement",
			"Enterprise.CodeAnalysis.",
			"Enterprise.Customs",
			"Enterprise.CommissionManagement.",
			"Enterprise.DataTransfer",
			"Enterprise.DocumentEngine",
			"Enterprise.DocumentScanning",
			"Enterprise.DocumentVisualizer",
			"Enterprise.DeniedPartyScreening",
			"Enterprise.EConversation",
			"Enterprise.Edifact",
			"Enterprise.eHubMessaging",
			"Enterprise.eManifest",
			"Enterprise.eTail",
			"Enterprise.Freight",
			"Enterprise.Faxing",
			"Enterprise.FaxRouter",
			"Enterprise.Interop",
			"Enterprise.LocalTransport",
			"Enterprise.MarketingManager",
			"Enterprise.Messaging",
			"Enterprise.Mobile",
			"Enterprise.PAVE",
			"Enterprise.Packing",
			"Enterprise.ProcessManagement",
			"Enterprise.Rating",
			"Enterprise.RemoteDesktopServices",
			"Enterprise.Recruitment",
			"Enterprise.Recruiter",
			"Enterprise.Services",
			"Enterprise.Tracking",
			"Enterprise.TimeEngineScheduler",
			"Enterprise.TFSCheckInPolicy",
			"Enterprise.TrustedMessaging",
			"Enterprise.StabilityChecker",
			"WTG.ROPE",
			"CargoWise.Services",
			"Enterprise.DbBackup",
			"lib",
			"MailManager",
			"Enterprise.Dat.",
			"BouncyCastle.Crypto",
			"Accord",
			"Xware",
			"Warehouse.RF",
			"MathNet.Numerics",
			"WTG.SpellCheck",
			"Microsoft.ReportViewer",
			"OxyPlot",
			"QuickGraph",
			"Rnwood.",
			"SpiceLogic-wtg.",
			"Swashbuckle.Core",
			"WTG.TrustedMessaging",
			"WTG.MachineLearning",
			"Enterprise.xTMessaging",
			"Enterprise.VisualBoards",
			"GlowIndexQueryService",
			"HHP.DataCollection",
			"grpc_csharp_ext",
			"Grpc.",
			"HandHeldProducts",
			"CWNUnit.",
			"CargoWise.Bi.",
			"CargoWise.VisualStudioCustomTools.",
			"CargoWise.WPF",
			"AWSSDK",
			"BuildTools",
			"CargoWise.NetworkVisualisation",
			"Microsoft.TeamFoundation.",
			"Microsoft.VisualStudio",
			"nunit",
			"Office.dll",
			"Outlook.dll",
			"Pinyin4net.dll",
			"RemotePrinting.",
			"WTG.Telematics.",
			"ZClient",
			"BorderWise",
			"BaseCodeGeneratorWithSite",
			"WiseRates.",
			"XmlDiffPatch.",
			"ClearImageNet.",
			"DocumentScanning.",
			"DocumentWrappers.",
			"ExcelTemplates.",
			"FirebirdSql.",
			"FlexCel.",
			"IronPython.",
			"LibPhoneNumber.",
			"Microsoft.CodeAnalysis.",
			"Microsoft.SqlServer.Management.SqlParser.",
			"Microsoft.Toolkit.Forms.UI.Controls.WebView.",
			"PdfToTextNet.",
			"WinFormHtmlEditor.",
		};

		static List<string> ExcludedEndsWithNames => new()
		{
			"Test.dll",
			"Tests.dll",
			"Testing.dll",
			"TestFramework.dll",
			"TestHelper.dll",
		};

		static List<string> MustIncludeEndsWithNames => new()
		{
			"CargoWise.Common.dll",
			"CargoWise.EntityFramework",
			"CargoWiseOne.WebInfrastructure.dll",
			"Enterprise.Environment.dll",
			"Enterprise.MasterFiles.Business.dll",
			"Enterprise.Semaphores.Common.dll",
			"System.Net.Http.Formatting.dll",
			"System.Web.Http.WebHost.dll",
			"System.Web.Http.dll",
			"Enterprise.ZArchitecture.Core.dll",
			"Enterprise.ZArchitecture.Web.Business",
			"Enterprise.ZArchitecture.Web.Shared.dll",
			"Enterprise.ZArchitecture.Web.Utilities.dll",
			"Mail.dll",
			"Enterprise.DocumentEngineCore.dll",
			"Enterprise.BufferManagement.Business.dll",
			"Enterprise.UniversalCopy.Business.dll",
			"Enterprise.DocumentEngine.dll",
			"Enterprise.Workflow.Business.dll",
			"Enterprise.Workflow.Integration.dll",
			"CargoWise.Tools.DuplicateDetector.dll",
			"Enterprise.UniversalDataBuss.Integration.dll",
			"Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll",
		};

		readonly HashSet<string> localFiles;
		readonly string applicationDir;
		readonly TempDirectory tempDirectory;

		const string WebDeployXmlFile = "webDeployXmlFile";
		public const string TestWebDeployZip = "ZClientWebTestWebDeploy.zip";
		public const string EnterpriseWebDeployZip = "EnterpriseWebDeploy.zip";
	}
}
