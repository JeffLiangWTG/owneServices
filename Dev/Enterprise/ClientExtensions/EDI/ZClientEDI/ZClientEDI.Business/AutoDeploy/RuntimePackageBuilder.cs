using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.IO;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using ICSharpCode.SharpZipLib.Zip;
using BuildXml = CargoWise.BuildTools.BuildXml;
using FileIO = CargoWise.Shared.FileIO;

namespace Enterprise.Client.EDI.AutoDeploy
{
	public interface IPackageBuilder
	{
		void BuildMaster();
		string LastPackagePath { get; }
	}

	/// <summary>
	/// Builds Deployment packages.
	/// </summary>

	public partial class RuntimePackageBuilder : IPackageBuilder
	{
		static string DefaultTemplatePath => Path.Combine(@"\\datfiles.wtg.zone\DAT\", "ServerInstallCD", "Template");

		static string[] WinzorFilePatterns => new[]
		{
			"winzor", "appserver", "sessionbroker"
		};

		public RuntimePackageBuilder(ReleaseBuild build, string targetPath)
			: this(string.Empty, targetPath)
		{
			masterBuild = build;
		}

		public RuntimePackageBuilder(string goodBuildPath, string targetPath)
			: this(DefaultTemplatePath, goodBuildPath, targetPath)
		{
			this.goodBuildPath = goodBuildPath;
			this.targetPath = targetPath;
		}

		public RuntimePackageBuilder(string templatePath, string goodBuildPath, string targetPath)
		{
			this.templatePath = templatePath;
			this.goodBuildPath = goodBuildPath;
			this.targetPath = targetPath;
		}

		/// <summary>
		/// Builds a master package that contains all client specific files
		/// </summary>
		public void BuildMaster()
		{
			BuildCore(string.Empty, true);
		}

		/// <summary>
		/// Builds a generic package that does NOT contain any client specific files
		/// </summary>
		public void Build()
		{
			Build(string.Empty, true);
		}

		/// <summary>
		/// Builds a client specific package
		/// </summary>
		/// <param name="enterpriseCode">EnterpriseCode that this package will be built for, ClientDocument.xml and dll will be included based on this Code.</param>
		public void Build(string enterpriseCode, bool isHostedOnWiseCloud)
		{
			if (masterBuild == null)
			{
				BuildCore(enterpriseCode, false);
			}
			else
			{
				BuildFromMasterPackageBlob(enterpriseCode, isHostedOnWiseCloud);
			}
		}

		protected virtual void BuildFromMasterPackageBlob(string enterpriseCode, bool isHostedOnWiseCloud)
		{
			using (var masterFile = TempFile.New())
			{
				File.Copy(masterBuild.HL_PackagePath, masterFile.Filename, true);
				using (var zip = new ZipFileCore(masterFile.Filename))
				{
					BuildFromMaster(enterpriseCode, zip, isHostedOnWiseCloud);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		void BuildFromMaster(string enterpriseCode, ZipFileCore zip, bool isHostedOnWiseCloud)
		{
			zip.BeginUpdate();

			var entCodeLower = enterpriseCode.ToLower(CultureInfo.InvariantCulture);
			var clientSpecific = enterpriseCode != null && enterpriseCode.Length > 0;
			var applicationFolder = "distribution/application";
			var zClientFilePattern = $"{applicationFolder}/zclient";
			var clientDllStartWith = zClientFilePattern + entCodeLower;
			var clientWebDllOrZipStartWith = zClientFilePattern + "web" + entCodeLower;
			var zClientDocumentXMLFilePattern = "documents.xml";
			var zClientDocumentXMLFileName = entCodeLower + zClientDocumentXMLFilePattern;
			var neoFilePattern = $"{applicationFolder}/neo";
			var neoUpgradeAllowed = IsNeoUpgradeAllowed(enterpriseCode);

			// It's possible there could be a mix of WiseTech-hosted and self-hosted systems in the upgrade
			// for a single client. WiseTech-hosted upgrade packages must include Winzor and DotNetCore binaries, even
			// if the customer is not licensed for Winzor or DotNetCore, because another tenant in WiseCloud sharing the same version
			// may need Winzor or DotNetCore.
			var winzorUpgradeAllowed = isHostedOnWiseCloud || IsWinzorUpgradeAllowed(enterpriseCode);
			var isNetCoreBinaryAllowed = isHostedOnWiseCloud || IsNetCoreBinaryUpgradeAllowed(enterpriseCode);

			var entriesToDelete = new List<ZipEntry>();

			foreach (ZipEntry entry in zip)
			{
				//Patterns of files to delete from the master zip package:
				//		*.pdb
				// If not client specific:
				//		ZClient*
				//		*documents.xml
				// If client specific then as above except keep:
				//		ZClient{EntCode}*
				//		ZClientWeb{EntCode}*
				//		{EntCode}documents.xml
				var entryFileName = entry.Name.ToLower(CultureInfo.InvariantCulture);

				if (entryFileName.EndsWith(".pdb", StringComparison.Ordinal))
				{
					entriesToDelete.Add(entry);
				}
				else if (entryFileName.StartsWith(zClientFilePattern, StringComparison.Ordinal))
				{
					if (!clientSpecific)
					{
						entriesToDelete.Add(entry);
					}
					else if (!entryFileName.StartsWith(clientDllStartWith, StringComparison.Ordinal)
						&& !entryFileName.StartsWith(clientWebDllOrZipStartWith, StringComparison.Ordinal))
					{
						entriesToDelete.Add(entry);
					}
				}
				else if (!entryFileName.Contains(@"/")
					&& entryFileName.EndsWith(zClientDocumentXMLFilePattern, StringComparison.Ordinal)
					&& entryFileName != zClientDocumentXMLFileName)
				{
					entriesToDelete.Add(entry);
				}
				else if (!neoUpgradeAllowed && entryFileName.StartsWith(neoFilePattern, StringComparison.Ordinal))
				{
					entriesToDelete.Add(entry);
				}
				else if (WinzorFilePatterns.Any(filePattern => entryFileName.StartsWith($"{applicationFolder}/{filePattern}", StringComparison.Ordinal))
					&& !winzorUpgradeAllowed)
				{
					entriesToDelete.Add(entry);
				}
				else if (NetCoreFilePatterns.Any(filePattern => entryFileName.StartsWith($"{applicationFolder}/{filePattern}", StringComparison.Ordinal))
					&& !isNetCoreBinaryAllowed)
				{
					entriesToDelete.Add(entry);
				}
			}

			zip.RemoveEntries(entriesToDelete.ToArray());

			using (var tempExtractDir = new TempDirectory())
			{
				if (zip.HasEntry("Distribution/Application/" + ExeFileNames.CargoWiseOneExeForVersionInfo))
				{
					zip.ExtractEntry("Distribution/Application/" + ExeFileNames.CargoWiseOneExeForVersionInfo, tempExtractDir);
				}
				else
				{
					zip.ExtractEntry("Distribution/Application/Enterprise.exe", tempExtractDir);
				}

				lastPackagePath = GetPackageName(tempExtractDir);
			}

			zip.CommitUpdate();
			string directory = Path.GetDirectoryName(lastPackagePath);
			Directory.CreateDirectory(directory);

			zip.SaveToFile(lastPackagePath);
		}

		bool IsNeoUpgradeAllowed(string enterpriseCode) => IsLicenseAllowed(enterpriseCode, EDIDataRegistry.Instance.NeoUpgradeLicences.Value);
		bool IsWinzorUpgradeAllowed(string enterpriseCode) => IsLicenseAllowed(enterpriseCode, EDIDataRegistry.Instance.WinzorLicences.Value);
		bool IsNetCoreBinaryUpgradeAllowed(string enterpriseCode) => IsLicenseAllowed(enterpriseCode, EDIDataRegistry.Instance.NetCoreBinaryLicences.Value);

		bool IsLicenseAllowed(string enterpriseCode, NeoUpgradeLicenceCollection licencesValue)
		{
			if (!string.IsNullOrEmpty(enterpriseCode))
			{
				return licencesValue?
					.Cast<NeoUpgradeLicence>()
					.Any(l => enterpriseCode.Equals(l.EnterpriseCode, StringComparison.OrdinalIgnoreCase)) ?? false;
			}
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void BuildCore(string enterpriseCode, bool isBuildingMaster)
		{
			lastPackagePath = "";

			if (!File.Exists(BuildXmlPath))
			{
				throw new ArgumentException(BuildXmlPath + " not found.");
			}

			using (var tempBuildDir = new TempDirectory())
			{
				var distributionDir = Path.Combine(tempBuildDir, "Distribution");
				Directory.CreateDirectory(distributionDir);

				var distributionApplicationDir = Path.Combine(distributionDir, "Application");
				Directory.CreateDirectory(distributionApplicationDir);

				var distributionInstallDir = Path.Combine(distributionDir, "Install");
				Directory.CreateDirectory(distributionInstallDir);

				var dotNetEdiLoadExePath = Path.Combine(goodBuildPath, "ediLoad.exe");
				if (File.Exists(dotNetEdiLoadExePath))
				{
					File.Copy(dotNetEdiLoadExePath, Path.Combine(tempBuildDir, "ediLoad.exe"));
				}

				var ediUninstallExePath = Path.Combine(goodBuildPath, "ediUninstall.exe");
				if (File.Exists(ediUninstallExePath))
				{
					File.Copy(ediUninstallExePath, Path.Combine(tempBuildDir, "ediUninstall.exe"));
				}

				// ediLoad flavors only built in release mode, execluded from build.xml
				var ediClientUninstallPath = Path.Combine(goodBuildPath, "ediEnterpriseClientUninstall.exe");
				if (File.Exists(ediClientUninstallPath))
				{
					File.Copy(ediClientUninstallPath, Path.Combine(distributionApplicationDir, "ediEnterpriseClientUninstall.exe"));
				}

				var glowWebDeployPath = Path.Combine(goodBuildPath, "GlowWebDeploy.zip");
				if (File.Exists(glowWebDeployPath))
				{
					File.Copy(glowWebDeployPath, Path.Combine(distributionApplicationDir, "GlowWebDeploy.zip"));
				}

				var weChatDeployPath = Path.Combine(goodBuildPath, "WeChatDeploy.zip");
				if (File.Exists(weChatDeployPath))
				{
					File.Copy(weChatDeployPath, Path.Combine(distributionApplicationDir, "WeChatDeploy.zip"));
				}

				var buildXml = new BuildXml(BuildXmlPath);

				CopyInstallComponents(distributionInstallDir, buildXml);

				var filesToPackageUp = buildXml.GetAllAssembliesDeployedToClient(goodBuildPath);
				var rootedCopyFroms = buildXml.GetOtherDeployedFilesWithCopyFrom(goodBuildPath).Where(x => Path.IsPathRooted(x.Value));

				var distinctTargetPaths = rootedCopyFroms.Select(x => Path.GetDirectoryName(x.Key)).Distinct().Where(d => !string.IsNullOrEmpty(d));
				foreach (var target in distinctTargetPaths)
				{
					var fullPath = Path.Combine(distributionApplicationDir, target);
					if (!Directory.Exists(fullPath))
					{
						Directory.CreateDirectory(fullPath);
					}
				}

				var clientSpecific = enterpriseCode != null && enterpriseCode.Length > 0;

				var clientNameRegex = @"^ZClient\w*?" + enterpriseCode;
				var clientWebNameRegex = @"^ZClientWeb\w*?" + enterpriseCode;

				Parallel.ForEach(filesToPackageUp, assemblyName =>
				{
					if (isBuildingMaster || (!assemblyName.StartsWith("ZClient", StringComparison.OrdinalIgnoreCase)
						|| (clientSpecific && (Regex.IsMatch(assemblyName, clientNameRegex, RegexOptions.IgnoreCase)
						|| Regex.IsMatch(assemblyName, clientWebNameRegex, RegexOptions.IgnoreCase)))))
					{
						var sourceFilePath = Path.IsPathRooted(assemblyName) ? assemblyName : Path.Combine(goodBuildPath, assemblyName);
						if (File.Exists(sourceFilePath))
						{
							var targetDir = distributionApplicationDir;
							var rootedCopyFromItem = rootedCopyFroms.Where(x => x.Value == sourceFilePath);
							if (rootedCopyFromItem.Any())
							{
								var targetRelativePath = rootedCopyFromItem.First().Key;
								var targetRelativePathDirectory = Path.GetDirectoryName(targetRelativePath);
								targetDir = Path.Combine(distributionApplicationDir, targetRelativePathDirectory);
							}
							else if (!string.IsNullOrEmpty(Path.GetDirectoryName(assemblyName)))
							{
								targetDir = Path.Combine(targetDir, Path.GetDirectoryName(assemblyName));
								Directory.CreateDirectory(targetDir);
							}

							var targetFilePath = Path.Combine(targetDir, Path.GetFileName(assemblyName));
							File.Copy(sourceFilePath, targetFilePath, true);

							if (isBuildingMaster && (sourceFilePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || sourceFilePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)))
							{
								string pdbSourceFilePath = sourceFilePath.Substring(0, sourceFilePath.Length - 4) + ".pdb";

								if (File.Exists(pdbSourceFilePath))
								{
									string pdbTargetFilePath = targetFilePath.Substring(0, targetFilePath.Length - 4) + ".pdb";
									File.Copy(pdbSourceFilePath, pdbTargetFilePath, true);
								}
							}
						}
					}
				});

				var documentXmlsPath = Path.Combine(goodBuildPath, "DocumentXmls");

				foreach (var clientDocXml in Directory.GetFiles(documentXmlsPath, "*Documents.xml"))
				{
					var xmlFileName = Path.GetFileName(clientDocXml);
					if (isBuildingMaster || (clientSpecific && xmlFileName.StartsWith(enterpriseCode, StringComparison.OrdinalIgnoreCase)))
					{
						var targetFilePath = Path.Combine(tempBuildDir, xmlFileName);
						File.Copy(clientDocXml, targetFilePath, true);
						File.SetAttributes(targetFilePath, FileAttributes.Normal);
					}
				}

				var noDeployFiles = GetNoDeployFiles(buildXml);

				var winzorAndNetCoreFilePatterns = WinzorFilePatterns.Concat(NetCoreFilePatterns).ToArray();
				Parallel.ForEach(winzorAndNetCoreFilePatterns, item =>
				{
					if (Directory.Exists(Path.Combine(goodBuildPath, item)))
					{
						var targetDirectory = Path.Combine(distributionApplicationDir, item);
						var targetDirectoryInfo = new DirectoryInfo(targetDirectory);
						FileIO.CopyDirectory(Path.Combine(goodBuildPath, item), targetDirectory);

						if (!isBuildingMaster)
						{
							RemoveMatchFiles(targetDirectoryInfo,
								fileInfo => fileInfo.Extension.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase));
						}

						if (NetCoreFilePatterns.Contains(item))
						{
							RecursivelyRemoveNoDeployFiles(targetDirectoryInfo, item, noDeployFiles);
						}

						RemoveMatchFiles(targetDirectoryInfo,
							fileInfo => MatchesNetCoreExcludeFilePatterns(fileInfo.FullName));
					}
					else if (File.Exists(Path.Combine(goodBuildPath, item)))
					{
						File.Copy(Path.Combine(goodBuildPath, item),
									Path.Combine(distributionApplicationDir, item), true);
					}
				});

				if (isBuildingMaster)
				{
					File.Copy(Path.Combine(goodBuildPath, ReleaseInfo.XmlFileName), Path.Combine(tempBuildDir, ReleaseInfo.XmlFileName));
				}

				lastPackagePath = GetPackageName(goodBuildPath);

				if (!ZipCompression.Zip(tempBuildDir, lastPackagePath))
				{
					throw new InvalidOperationException(ZipCompression.LastError);
				}

				void RemoveMatchFiles(DirectoryInfo directoryInfo, Func<FileInfo, bool> matchPredicate)
				{
					foreach (var file in directoryInfo.GetFiles())
					{
						if (matchPredicate(file))
						{
							file.Delete();
						}
					}

					foreach (var subfolder in directoryInfo.GetDirectories())
					{
						RemoveMatchFiles(subfolder, matchPredicate);
					}
				}

				void RecursivelyRemoveNoDeployFiles(DirectoryInfo directoryInfo, string folderName, HashSet<string> noDeployList)
				{
					foreach (FileInfo file in directoryInfo.GetFiles())
					{
						if (noDeployList.Contains(Path.Combine(folderName, file.Name)))
						{
							file.Delete();
						}
					}

					foreach (DirectoryInfo subfolder in directoryInfo.GetDirectories())
					{
						if (subfolder.Name.Equals("refs", StringComparison.OrdinalIgnoreCase)) // for local tests
						{
							subfolder.Delete(true);
						}
						else
						{
							RecursivelyRemoveNoDeployFiles(subfolder, Path.Combine(folderName, subfolder.Name), noDeployFiles);
						}
					}
				}
			}
		}

		public string LastPackagePath
		{
			get { return lastPackagePath; }
		}

		#region Implementation

		readonly string templatePath;
		readonly string goodBuildPath;
		readonly string targetPath;
		string lastPackagePath = "";
		readonly ReleaseBuild masterBuild;

		void CopyInstallComponents(string distributionInstallDir, BuildXml buildXml)
		{
			var templateInstallPath = Path.Combine(templatePath, @"Distribution\Install");

			if (buildXml.HasRuntimePackageDefinition())
			{
				foreach (var dir in buildXml.GetRuntimePackageDistributionInstallDirectories())
				{
					CopyInstallComponent(templateInstallPath, distributionInstallDir, dir);
				}
			}
			else
			{
				// older verions were hardcoded here
				CopyInstallComponent(templateInstallPath, distributionInstallDir, "BlackIceFax");
				CopyInstallComponent(templateInstallPath, distributionInstallDir, "Font");
			}
		}

		void CopyInstallComponent(string sourceMainDir, string targetMainDir, string subDirToCopy)
		{
			var sourceFolder = Path.Combine(sourceMainDir, subDirToCopy);
			var targetFolder = Path.Combine(targetMainDir, subDirToCopy);

			CopyFolder(sourceFolder, targetFolder);
		}

		void CopyFolder(string sourceFolder, string targetFolder)
		{
			if (Directory.Exists(sourceFolder))
			{
				if (!Directory.Exists(targetFolder))
				{
					Directory.CreateDirectory(targetFolder);
				}

				foreach (var subSourceFolder in Directory.GetDirectories(sourceFolder))
				{
					var subSourceFolderName = Path.GetFileName(subSourceFolder);
					var subTargetFolder = Path.Combine(targetFolder, subSourceFolderName);
					CopyFolder(subSourceFolder, subTargetFolder);
				}

				var sourceFiles = Directory.GetFiles(sourceFolder);
				foreach (var sourceFile in sourceFiles)
				{
					var fileName = Path.GetFileName(sourceFile);
					var targetFile = Path.Combine(targetFolder, fileName);
					File.Copy(sourceFile, targetFile, true);
				}
			}
			else
			{
				throw new ArgumentException(sourceFolder + " does not exist");
			}
		}

		string BuildXmlPath
		{
			get { return Path.Combine(goodBuildPath, "Build.xml"); }
		}

		string GetPackageName(string buildPath)
		{
			var versionFile = Path.Combine(buildPath, ExeFileNames.CargoWiseOneExeForVersionInfo);
			if (!File.Exists(versionFile))
			{
				versionFile = Path.Combine(buildPath, "Enterprise.exe");
			}
			var info = FileVersionInfo.GetVersionInfo(versionFile);
			ValidateDeployableBuild(info);
			var packageName = ReleaseBuild.GetPackageName(new VersionNumber(info));
			return Path.Combine(targetPath, packageName);
		}

		protected virtual void ValidateDeployableBuild(FileVersionInfo info)
		{
			if (info.FileDescription.Contains("placeholder", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Deployment packages must be made from a CW1Deployment build only, attempting to create a package from a Dev build will result in version collisions");
			}
		}

		#endregion
	}
}
