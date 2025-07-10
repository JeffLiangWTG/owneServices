using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.Common;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace CargoWise.BuildTools.Testing
{
	sealed class CertificationComplianceTest : TestCase
	{
		string currentDirectoryName;

		public void TestExecutablesHaveValidFileVersions()
		{
			RunTest("The following files have missing file version information. The CompanyName, ProductName and ProductVersion fields must not be empty.", delegate (StringBuilder sb)
			{
				foreach (string file in FileNameHelpers.GetFileNames(GetExecutableFileExtensions(), GetFileNamesThatCanHaveInvalidFileVersion()))
				{
					if (!file.EndsWith(".XmlSerializers.dll") &&
						!file.EndsWith(".Contracts.dll") &&
						!file.StartsWith("Mono.Cecil.", StringComparison.OrdinalIgnoreCase) &&
						!file.StartsWith("Jint") &&
						!file.StartsWith("Grpc.") &&
						!file.StartsWith("grpc_") &&
						!file.StartsWith("Microsoft.VisualStudio.") &&
						!file.Equals("System.Diagnostics.EventLog.Messages.dll", StringComparison.OrdinalIgnoreCase) &&
						!file.Equals("Antlr4.Runtime.Standard.dll", StringComparison.OrdinalIgnoreCase) &&
						!file.Equals("System.Net.Http.Formatting.dll", StringComparison.OrdinalIgnoreCase))
					{
						FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(GetBinFilePath(file));
						if (versionInfo == null ||
							IsNullOrEmptyTrimmed(versionInfo.CompanyName) ||
							IsNullOrEmptyTrimmed(versionInfo.ProductName) ||
							IsNullOrEmptyTrimmed(versionInfo.ProductVersion))
						{
							sb.AppendLine(file);
						}
					}
				}
			});
		}

		public void TestExeFilesHaveDescription()
		{
			RunTest("The following files have missing file descriptions. Please make sure that their assemblies have an AssemblyTitle attribute.", delegate (StringBuilder sb)
			{
				foreach (string file in FileNameHelpers.GetExeFileNames(GetFileNamesThatCanHaveNoFileDescription()))
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(GetBinFilePath(file));
					if ((versionInfo == null) || IsNullOrEmptyTrimmed(versionInfo.FileDescription))
					{
						sb.AppendLine(file);
					}
				}
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProjectsHaveDebugSymbolsForReleaseBuildMode()
		{
			RunTest("The following projects do not have debug symbols in release build mode.", delegate (StringBuilder sb)
			{
				var projects = new ConcurrentHashSet<(string projectFilePath, string contents, string solutionFileName)>();
				Parallel.ForEach(BuildXml.Instance.GetAllSolutionFileNames(), solutionFileName =>
				{
					var solutionFilePath = BuildConstants.GetLocalPath(solutionFileName);
					var solutionDirectoryPath = Path.GetDirectoryName(solutionFilePath);
					var solution = new SolutionFile(solutionFilePath);
					foreach (var project in solution.Projects)
					{
						var projectFilePath = Path.Combine(solutionDirectoryPath, project.ProjectPath);
						if (File.Exists(projectFilePath))
						{
							var contents = File.ReadAllText(projectFilePath);
							projects.TryAdd((projectFilePath, contents, solutionFileName));
						}
					}
				});

				foreach (var (projectFilePath, contents, solutionFileName) in projects)
				{
					var extension = Path.GetExtension(projectFilePath);
					var hasDebugSymbols = false;
					if (extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase) || extension.Equals(".vbproj", StringComparison.OrdinalIgnoreCase) || (extension.Equals(".sqlproj", StringComparison.OrdinalIgnoreCase)))
					{
						hasDebugSymbols = DoesMSBuildProjectHaveDebugSymbolsForReleaseBuildMode(contents);
					}
					else if (extension.Equals(".vcproj", StringComparison.OrdinalIgnoreCase))
					{
						hasDebugSymbols = DoesCProjectHaveDebugSymbolsForReleaseBuildMode(contents);
					}
					else if (
						extension.Equals(".rptproj", StringComparison.OrdinalIgnoreCase)
						|| extension.Equals(".smproj", StringComparison.OrdinalIgnoreCase)
						|| extension.Equals(".dtproj", StringComparison.OrdinalIgnoreCase)
						)
					{
						// These project types do not have debug symbols
						//   - rptproj (reporting services)
						//   - smproj (analysis services semantic model)
						//   - dtproj (database project)
						// => Skips to the next project
						continue;
					}
					else
					{
						Fail(projectFilePath + " has an unknown project type.");
					}

					if (!hasDebugSymbols)
					{
						sb.AppendLine(Path.Combine(Path.GetDirectoryName(solutionFileName), projectFilePath));
					}
				}
			});
		}

		[RequiresSoftware(RequiredSoftware.WinSdk)]
		public void TestUacManifests()
		{
			RunTest("The following files do not have valid manifest files. Either a manifest is not embedded or the requestedExecutionLevel element is invalid.", delegate (StringBuilder sb)
			{
				string mtPath = WTG.DevTools.Common.WindowsSdk.GetWindowsSdkToolPath("mt.exe");
				string manifestPath = TempForTest.GetTempFileName();
				try
				{
					foreach (string file in FileNameHelpers.GetExeFileNames(GetFileNamesThatCanHaveMissingUacManifests()))
					{
						File.Delete(manifestPath);

						string errorText;
						var hasManifest = ExtractManifest(mtPath, GetBinFilePath(file), manifestPath, out errorText);
						if (!hasManifest)
						{
							sb.AppendLine(string.Format("{0}: Missing manifest.", file));
							sb.AppendLine(errorText);
							continue;
						}

						XmlDocument xml = new XmlDocument();
						xml.Load(manifestPath);

						// Can't use XPath because the namespace is different for various elements between manifests.
						// i.e. some manifests use asm.v1, some use asm.v2, and some use asm.v3 for different nodes.
						XmlNode node =
							SelectChildNode(
									SelectChildNode(
											SelectChildNode(
													SelectChildNode(xml.DocumentElement, "trustInfo"), "security"), "requestedPrivileges"), "requestedExecutionLevel");

						var hasValidManifest = true;

						if (node != null)
						{
							XmlAttribute attribute = node.Attributes["level"];
							if ((attribute != null) && ((attribute.Value == "asInvoker") || (attribute.Value == "highestAvailable") || (attribute.Value == "requireAdministrator")))
							{
								attribute = node.Attributes["uiAccess"];
								hasValidManifest = (attribute != null) && (attribute.Value == "false");
							}
						}

						if (!hasValidManifest)
						{
							sb.AppendLine(string.Format("{0}: Invalid manifest.", file));
						}
					}
				}
				finally
				{
					DeleteIfExists(manifestPath);
				}
			});
		}

		//NGen should only be tested in netframework since it is not used in net8
#if NETFRAMEWORK
		public void TestDbUpgraderAssembliesAreNotNGened()
		{
			CombineAssertions(delegate
			{
				foreach (var solution in BuildXml.Instance.GetAllSolutionFileNames())
				{
					foreach (var assembly in BuildXml.Instance.GetAllAssembliesInSolution(solution))
					{
						// Although the test is only in net48, here the var assembly will include 'net8.0\' if the assemblies have net8 version in Build.xml. So skip this redundant NGen test situation for net8 assemblies. 
						if (assembly.Contains("net8.0"))
						{
							continue;
						}

						string assemblyFileName = Path.GetFileName(assembly);

						if (assemblyFileName.IndexOf("DbUpgrader", StringComparison.OrdinalIgnoreCase) > -1 && assemblyFileName.IndexOf("DbUpgrader.Resource.", StringComparison.OrdinalIgnoreCase) == -1 && assemblyFileName != "Enterprise.DbUpgrader.Shared.dll")
						{
							AssertEquals(assemblyFileName + " shoud have NGen=\"false\" in Build.xml", false, BuildXml.Instance.NGen(assemblyFileName));
						}
					}
				}
			});
		}
#endif

		public void TestGetAllAssembliesDeployedToClientReturnsEnterpriseUpgradesPreinstall40()
		{
			Assert(BuildXml.Instance.GetAllAssembliesDeployedToClient(BaseSourcePath).Contains("Enterprise.Upgrades.Preinstall4.0.exe"));
		}

		string CurrentDirectoryName
		{
			get
			{
#if NET
				return currentDirectoryName ?? (currentDirectoryName = Directory.GetParent(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)).FullName);
#else
				return currentDirectoryName ?? (currentDirectoryName = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
#endif
			}
		}

		static bool DoesCProjectHaveDebugSymbolsForReleaseBuildMode(string projectFileContents)
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(projectFileContents);
			foreach (XmlNode node in xml.DocumentElement.SelectNodes("Configurations/Configuration[starts-with(@Name,'Release|')]/Tool[@Name='VCCLCompilerTool']"))
			{
				XmlAttribute attribute = node.Attributes["DebugInformationFormat"];
				if ((attribute == null) || ((attribute.Value != "3") && (attribute.Value != "4")))
				{
					return false;
				}
			}
			return true;
		}

		static bool DoesMSBuildProjectHaveDebugSymbolsForReleaseBuildMode(string projectFileContents)
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(projectFileContents);
			XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xml.NameTable);
			namespaceManager.AddNamespace("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
			foreach (XmlNode node in xml.DocumentElement.SelectNodes("Project:PropertyGroup[contains(@Condition,'Release|')]", namespaceManager))
			{
				XmlNode debugTypeNode = node.SelectSingleNode("Project:DebugType", namespaceManager);
				if ((debugTypeNode == null) || ((debugTypeNode.InnerText != "pdbonly") && (debugTypeNode.InnerText != "full")))
				{
					return false;
				}
			}
			return true;
		}

		static bool ExtractManifest(string mtPath, string exePath, string manifestPath, out string errorText)
		{
			errorText = string.Empty;

			ProcessStartInfo info = new ProcessStartInfo(mtPath, string.Format(@"""-inputresource:{0};#1"" ""-out:{1}""", exePath, manifestPath));
			info.CreateNoWindow = true;
			info.UseShellExecute = false;
			info.RedirectStandardError = true;
			info.RedirectStandardOutput = true;
			using (Process process = Process.Start(info))
			{
				process.WaitForExit();

				if (process.ExitCode != 0)
				{
					errorText = process.StandardError.ReadToEnd() + Environment.NewLine + process.StandardOutput.ReadToEnd() + Environment.NewLine;
				}

				return (process.ExitCode == 0);
			}
		}

		string GetBinFilePath(string fileName)
		{
			var result = Directory.GetFiles(CurrentDirectoryName, fileName, SearchOption.AllDirectories).FirstOrDefault();
			if (string.IsNullOrEmpty(result))
			{
				result = Path.Combine(CurrentDirectoryName, fileName);
			}

			return result;
		}

		static string[] GetExecutableFileExtensions()
		{
			return new string[]
			{
					".exe",
					".dll",
					".ocx",
					".sys",
					".cpl",
					".drv",
					".scr"
			};
		}

		static string[] GetFileNamesThatCanHaveInvalidFileVersion()
		{
			return new string[]
			{
					// Interop files generated by AxImp.exe and TlbImp.exe.
					"AxPdfLib.dll",
					"Enterprise.Interop.AxMSTSCax.dll",
					"Enterprise.Interop.AxSHDocVw.dll",
					"Enterprise.Interop.MSTSCAx.dll",
					"Enterprise.Interop.SHDocVw.dll",
					"Excel.dll",
					"MSForms.dll",
					"Office.dll",
					"Outlook.dll",
					"PdfLib.dll",
					"VBIDE.dll",
					"Enterprise.Warehouse.RF.exe",
					"Enterprise.LocalTransport.Mobile.Client.exe",
					"LocalTransport.Mobile.Client.Load.exe",
					"Enterprise.Interop.CertEnroll.dll",
					"Enterprise.Interop.certcli.dll",
					"SpiceLogic-wtg.Interop.mshtml.dll",

					// Third-party files.
					"antlr.runtime.dll",
					"hunspell.exe",
					"PtxSdkCommon.wm.dll",
					"PtxSdkCommon.CE.dll",
					"Fluent.dll",
					"QuickGraph.dll",
					"QuickGraph.Data.dll",
					"Aga.Business.dll",
					"Aga.Controls.dll",
					"Ude.dll",
					"pdfium.dll",
					"YamlDotNet.dll",
					"Microsoft.IdentityModel.Logging.dll",
					"Microsoft.IdentityModel.Tokens.dll",
					"Microsoft.mshtml.dll",
					"System.IdentityModel.Tokens.Jwt.dll",
					"librdkafka.dll",
					"librdkafkacpp.dll",
					"libzstd.dll",
					"msvcp120.dll",
					"msvcr120.dll",
					"zlib1.dll",
					"Owin.dll",
					"Swashbuckle.Core.dll",
					"Swashbuckle.Examples.dll",
					"toxiproxy-server-windows-amd64.exe",

					// Files that are not deployed to clients.
					"Rhino.Mocks.dll",
					"Unzip.exe",
					"handle.exe",
					"wsecutil.dll",
			};
		}

		// These files are not deployed to clients.
		static string[] GetFileNamesThatCanHaveNoFileDescription()
		{
			return new string[]
			{
					"DbUsage.exe",
					"Enterprise.ServiceManager.TestServiceProvider.exe",
					"GenerateEdiDeployDb.exe",
					"MockProgram.exe",
					"MockUnsignedProgram.exe",
					"Replacer.exe",
					"Unzip.exe",
					"handle.exe",
					"WebDeployBuilder.exe",
					"Enterprise.Warehouse.RF.exe",
					"Enterprise.LocalTransport.Mobile.Client.exe",
					"LocalTransport.Mobile.Client.Load.exe",
					"hunspell.exe",
					"LegacyBpRunner.exe",
					"DBTool.exe",
					"toxiproxy-server-windows-amd64.exe",
			};
		}

		static string[] GetFileNamesThatCanHaveMissingUacManifests()
		{
			return new string[]
			{
					"Enterprise.Warehouse.RF.exe",
					"EW.RF.Load.exe",
					"EW.RF.VP.Load.exe",
					"Enterprise.LocalTransport.Mobile.Client.exe",
					"LocalTransport.Mobile.Client.Load.exe",
					"Enterprise.Main.UI.Browser.exe",
					"hunspell.exe",
					"makecert.exe",
					"CargoWiseOne.Desktop.exe",
					"toxiproxy-server-windows-amd64.exe",
			};
		}

		static bool IsNullOrEmptyTrimmed(string value)
		{
			return (value == null) || (value.Trim().Length == 0);
		}

		static void RunTest(string message, TestDelegate testDelegate)
		{
			StringBuilder sb = new StringBuilder();
			testDelegate.Invoke(sb);
			if (sb.Length > 0)
			{
				sb.Insert(0, message + Environment.NewLine + Environment.NewLine);
				Fail(sb.ToString());
			}
			else
			{
				Assert("No errors found.", true);
			}
		}

		static XmlNode SelectChildNode(XmlNode parent, string childName)
		{
			if (parent != null)
			{
				foreach (XmlNode child in parent)
				{
					if (child.Name == childName)
					{
						return child;
					}
				}
			}
			return null;
		}

		delegate void TestDelegate(StringBuilder sb);
	}
}
