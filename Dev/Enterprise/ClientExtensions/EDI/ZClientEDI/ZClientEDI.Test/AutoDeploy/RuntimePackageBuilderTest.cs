using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using WTG.DevTools.Definitions;
using BuildXml = CargoWise.BuildTools.BuildXml;

namespace Enterprise.Client.EDI.AutoDeploy.Test;

sealed class RuntimePackageBuilderTest : TransactionedTestCase
{
	const string PackageFilePattern = "Package????????_??????_*_*_*_*.edp";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
	LicenceEnterprise CreateNewLicence(BusinessObjectFactory factory, Guid licencePK, string enterpriseCode)
	{
		var licence = factory.Load<LicenceEnterprise>(licencePK);
		if (licence == null)
		{
			var org = factory.NewWithValidTestData<EDIOrgHeader>();
			licence = (LicenceEnterprise)factory.New(typeof(LicenceEnterprise), licencePK);
			licence.LE_EnterpriseCode = enterpriseCode;
			licence.LE_OH = org.PK;
			factory.Save();
		}

		return licence;
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestBuildGenericPackage()
	{
		CopyDllsFromStartupPath(goodBuildLocation);

		foreach (string fileName in Directory.GetFiles(goodBuildLocation))
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}

		Assert("files found in GoodBuildLocation", Directory.GetFiles(goodBuildLocation).Length > 0);

		AssertEquals("output package count", 0, Directory.GetFiles(targetLocation, PackageFilePattern).Length);

		var builder = new RuntimePackageBuilderForTest(templateLocation, goodBuildLocation, targetLocation);
		builder.Build();

		CheckPackageName(builder.LastPackagePath);

		string[] zippedFiles = Directory.GetFiles(targetLocation, PackageFilePattern);
		AssertEquals("output Package count", 1, zippedFiles.Length);
		string targetPackage = zippedFiles[0];

		AssertEquals("Unzipping", true, ZipCompression.Unzip(targetPackage, unzipTargetDir));

		string[] unzippedDirectories = Directory.GetDirectories(unzipTargetDir);
		AssertEquals("Directories unzipped", 1, unzippedDirectories.Length);
		string[] unzippedFiles = Directory.GetFiles(unzipTargetDir);
		AssertEquals("Files Unzipped", 2, unzippedFiles.Length);

		Array.Sort(unzippedDirectories);
		Array.Sort(unzippedFiles);

		AssertEquals("UnzippedFiles 0", Path.Combine(unzipTargetDir, "ediLoad.exe"), unzippedFiles[0]);
		AssertEquals("UnzippedFiles 1", Path.Combine(unzipTargetDir, "ediUninstall.exe"), unzippedFiles[1]);

		string distributionDirectory = Path.Combine(unzipTargetDir, "Distribution");
		AssertEquals("Unzipped Directory 0", distributionDirectory, unzippedDirectories[0]);

		string[] unzippedDistributionDirectories = Directory.GetDirectories(distributionDirectory);
		Array.Sort(unzippedDistributionDirectories);
		AssertEquals("Unzipped Distribution Directories count", 2, unzippedDistributionDirectories.Length);

		string applicationDirectory = Path.Combine(distributionDirectory, "Application");

		string installDirectory = Path.Combine(distributionDirectory, "Install");
		AssertEquals("Unzipped Distribution Directory 0", applicationDirectory, unzippedDistributionDirectories[0]);

		string fontDirectory = Path.Combine(installDirectory, "Font");
		Assert(fontDirectory + " should have 3OF9.TTF", File.Exists(Path.Combine(fontDirectory, "3OF9.TTF")));

		Assert("Should not deploy RemoteDesktopServices.Client", !File.Exists(Path.Combine(applicationDirectory, "Enterprise.RemoteDesktopServices.Client.dll")));

		var filesShouldBeDeployed = new List<string>(BuildXml.Instance.GetAllAssembliesDeployedToClient(BaseSourcePath));

		var filesShouldBeDeployedForCargoWiseWeb = GetFilesThatShouldBeDeployedForCargoWiseWeb(excludePdb: true);
		var filesShouldBeDeployedForNetCore = GetFilesShouldBeDeployedForNetCore(filesShouldBeDeployed, excludePdb: true);

		filesShouldBeDeployed = filesShouldBeDeployed.Concat(filesShouldBeDeployedForCargoWiseWeb)
													 .Concat(filesShouldBeDeployedForNetCore)
													 .ToList();

		CheckPackageDifferences(filesShouldBeDeployed, applicationDirectory, removeClientDLLs: true, keepEDIClientFiles: false, isBuildingMaster: false);
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestBuildGenericPackageWithDotNetEdiLoad()
	{
		CopyDllsFromStartupPath(goodBuildLocation);

		foreach (string fileName in Directory.GetFiles(goodBuildLocation))
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}

		Assert("files found in GoodBuildLocation", Directory.GetFiles(goodBuildLocation).Length > 0);

		AssertEquals("output package count", 0, Directory.GetFiles(targetLocation, PackageFilePattern).Length);

		var builder = new RuntimePackageBuilderForTest(templateLocation, goodBuildLocation, targetLocation);
		builder.Build();

		CheckPackageName(builder.LastPackagePath);

		string[] zippedFiles = Directory.GetFiles(targetLocation, PackageFilePattern);
		AssertEquals("output Package count", 1, zippedFiles.Length);
		string targetPackage = zippedFiles[0];

		AssertEquals("Unzipping", true, ZipCompression.Unzip(targetPackage, unzipTargetDir));

		string[] unzippedDirectories = Directory.GetDirectories(unzipTargetDir);
		AssertEquals("Directories unzipped", 1, unzippedDirectories.Length);
		string[] unzippedFiles = Directory.GetFiles(unzipTargetDir);
		AssertEquals("Files Unzipped", 2, unzippedFiles.Length);

		Array.Sort(unzippedDirectories);
		Array.Sort(unzippedFiles);

		AssertEquals("UnzippedFiles 0", Path.Combine(unzipTargetDir, "ediLoad.exe"), unzippedFiles[0]);
		AssertEquals("UnzippedFiles 1", Path.Combine(unzipTargetDir, "ediUninstall.exe"), unzippedFiles[1]);

		string distributionDirectory = Path.Combine(unzipTargetDir, "Distribution");
		AssertEquals("Unzipped Directory 0", distributionDirectory, unzippedDirectories[0]);

		string[] unzippedDistributionDirectories = Directory.GetDirectories(distributionDirectory);
		Array.Sort(unzippedDistributionDirectories);
		AssertEquals("Unzipped Distribution Directories count", 2, unzippedDistributionDirectories.Length);

		string applicationDirectory = Path.Combine(distributionDirectory, "Application");
		string installDirectory = Path.Combine(distributionDirectory, "Install");
		AssertEquals("Unzipped Distribution Directory 0", applicationDirectory, unzippedDistributionDirectories[0]);
		AssertEquals("Unzipped Distribution Directory 0", installDirectory, unzippedDistributionDirectories[1]);

		string fontDirectory = Path.Combine(installDirectory, "Font");
		Assert(fontDirectory + " should have 3OF9.TTF", File.Exists(Path.Combine(fontDirectory, "3OF9.TTF")));

		var filesShouldBeDeployed = new List<string>(BuildXml.Instance.GetAllAssembliesDeployedToClient(BaseSourcePath));

		var filesShouldBeDeployedForCargoWiseWeb = GetFilesThatShouldBeDeployedForCargoWiseWeb(excludePdb: true);
		var filesShouldBeDeployedForNetCore = GetFilesShouldBeDeployedForNetCore(filesShouldBeDeployed, excludePdb: true);

		filesShouldBeDeployed = filesShouldBeDeployed.Concat(filesShouldBeDeployedForCargoWiseWeb)
													 .Concat(filesShouldBeDeployedForNetCore)
													 .ToList();

		CheckPackageDifferences(filesShouldBeDeployed, applicationDirectory, removeClientDLLs: true, keepEDIClientFiles: false, isBuildingMaster: false);
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestBuildClientSpecificPackage()
	{
		if (!Directory.Exists(clientDocXmlDir))
		{
			Directory.CreateDirectory(clientDocXmlDir);
		}
		string clientDocXmlFilePath = Path.Combine(clientDocXmlDir, "EDIDocuments.xml");
		File.CreateText(clientDocXmlFilePath).Close();

		CopyDllsFromStartupPath(goodBuildLocation);

		foreach (string fileName in Directory.GetFiles(goodBuildLocation))
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}

		Assert("files found in GoodBuildLocation", Directory.GetFiles(goodBuildLocation).Length > 0);

		AssertEquals("output package count", 0, Directory.GetFiles(targetLocation, PackageFilePattern).Length);

		var builder = new RuntimePackageBuilderForTest(templateLocation, goodBuildLocation, targetLocation);
		builder.Build("EDI", true);

		CheckPackageName(builder.LastPackagePath);

		CheckPackageForEDIClient(targetLocation, isBasedOnMasterPackage: false, expectBlazorFiles: true, expectNetCoreBinaryFiles: true);
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	[SnailTest]
	public void TestAlphaReleaseBuildWithWinzorFiles()
	{
		CopyDllsFromStartupPath(goodBuildLocation);

		var clientBuilder = new RuntimePackageBuilderForTest(goodBuildLocation, targetLocation);
		clientBuilder.Build("EDI", true);

		CheckPackageName(clientBuilder.LastPackagePath);

		using (var zip = new ZipArchive(File.OpenRead(clientBuilder.LastPackagePath), ZipArchiveMode.Read, false))
		{
			AssertGreaterThan(zip.Entries.Count(x => x.FullName.ToLowerInvariant().Contains("winzor")), 0);
			AssertGreaterThan(zip.Entries.Count(x => x.FullName.ToLowerInvariant().Contains("appserver")), 0);
			AssertGreaterThan(zip.Entries.Count(x => x.FullName.ToLowerInvariant().Contains("sessionbroker")), 0);
			foreach (var netCoreDirectory in GetNetCoreDirectories())
			{
				AssertGreaterThan(zip.Entries.Count(x => x.FullName.ToLowerInvariant().Contains(netCoreDirectory)), 0);
			}
		}
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	[SnailTest]
	public void TestReleaseBuildWithWinzorFiles()
	{
		CreateDummyFilesFromFilesOnStartupPath(goodBuildLocation);

		// Load the releaseInfo.xml file
		var xmlDoc = new XmlDocument();
		xmlDoc.Load(Path.Combine(goodBuildLocation, "ReleaseInfo.xml"));

		// Modify the XML data
		var rootNode = xmlDoc.DocumentElement;
		var node = rootNode.SelectSingleNode("/ReleaseInfo/ReleaseRing");
		node.InnerText = ReleaseRings.Codes.GPR;

		// Save the changes
		xmlDoc.Save(Path.Combine(goodBuildLocation, "ReleaseInfo.xml"));

		var clientBuilder = new RuntimePackageBuilderForTest(goodBuildLocation, targetLocation);
		clientBuilder.Build("EDI", true);

		CheckPackageName(clientBuilder.LastPackagePath);

		using (var zip = new ZipArchive(File.OpenRead(clientBuilder.LastPackagePath), ZipArchiveMode.Read, false))
		{
			AssertGreaterThan(zip.Entries.Count(x => x.FullName.ToLowerInvariant().Contains("winzor")), 0);
			AssertGreaterThan(zip.Entries.Count(x => x.FullName.ToLowerInvariant().Contains("appserver")), 0);
			AssertGreaterThan(zip.Entries.Count(x => x.FullName.ToLowerInvariant().Contains("sessionbroker")), 0);
			foreach (var netCoreDirectory in GetNetCoreDirectories())
			{
				AssertGreaterThan(zip.Entries.Count(x => x.FullName.ToLowerInvariant().Contains(netCoreDirectory)), 0);
			}
		}
	}

	[SnailTest]
	[UseSnapshotProtection(skipTransaction: true)]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestBuildMasterPackage()
	{
		new ClientDbSchemaCreationForTesting().RunClientDbCreateScripts(false);

		string clientCMLDllPath = Path.Combine(goodBuildLocation, "ZClientCML.dll");
		File.CreateText(clientCMLDllPath).Close();

		CopyDllsFromStartupPath(goodBuildLocation);

		string clientEDIDocXmlFilePath = Path.Combine(clientDocXmlDir, "EDIDocuments.xml");
		File.CreateText(clientEDIDocXmlFilePath).Close();

		string clientCMLDocXmlFilePath = Path.Combine(clientDocXmlDir, "CMLDocuments.xml");
		File.CreateText(clientCMLDocXmlFilePath).Close();

		foreach (string fileName in Directory.GetFiles(goodBuildLocation))
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}

		Assert("files found in GoodBuildLocation", Directory.GetFiles(goodBuildLocation).Length > 0);

		AssertEquals("output package count", 0, Directory.GetFiles(targetLocation, PackageFilePattern).Length);

		var builder = new RuntimePackageBuilderForTest(templateLocation, goodBuildLocation, targetLocation);
		builder.BuildMaster();

		string[] zippedFiles = Directory.GetFiles(targetLocation, PackageFilePattern);
		AssertEquals("output Package count", 1, zippedFiles.Length);
		string targetPackage = zippedFiles[0];

		CheckPackageName(builder.LastPackagePath);

		bool writable = false;
		for (int i = 0; i < 3; i++)
		{
			if (!IsDirectoryWritable(unzipTargetDir))
			{
				Thread.Sleep(100);
				DeleteAndRecreate(unzipTargetDir);
			}
			else
			{
				writable = true;
				break;
			}
		}

		AssertEquals("unzipTargetDir is not writable", true, writable);

		AssertEquals("Unzipping", true, ZipCompression.Unzip(targetPackage, unzipTargetDir));

		string[] unzippedDirectories = Directory.GetDirectories(unzipTargetDir);
		AssertEquals("Directories unzipped", 1, unzippedDirectories.Length);
		string[] unzippedFiles = Directory.GetFiles(unzipTargetDir);

		AssertCollectionContains(Path.Combine(unzipTargetDir, "CMLDocuments.xml"), unzippedFiles);
		AssertCollectionContains(Path.Combine(unzipTargetDir, "EDIDocuments.xml"), unzippedFiles);
		AssertCollectionContains(Path.Combine(unzipTargetDir, "ediLoad.exe"), unzippedFiles);
		AssertCollectionContains(Path.Combine(unzipTargetDir, "ediUninstall.exe"), unzippedFiles);
		AssertCollectionContains(Path.Combine(unzipTargetDir, "ReleaseInfo.xml"), unzippedFiles);

		Array.Sort(unzippedDirectories);

		string distributionDirectory = Path.Combine(unzipTargetDir, "Distribution");
		AssertEquals("Unzipped Directory 0", distributionDirectory, unzippedDirectories[0]);

		string[] unzippedDistributionDirectories = Directory.GetDirectories(distributionDirectory);
		Array.Sort(unzippedDistributionDirectories);
		AssertEquals("Unzipped Distribution Directories count", 2, unzippedDistributionDirectories.Length);

		string applicationDirectory = Path.Combine(distributionDirectory, "Application");
		string installDirectory = Path.Combine(distributionDirectory, "Install");
		AssertEquals("Unzipped Distribution Directory 0", applicationDirectory, unzippedDistributionDirectories[0]);
		AssertEquals("Unzipped Distribution Directory 2", installDirectory, unzippedDistributionDirectories[1]);

		var filesShouldBeDeployed = new List<string>(BuildXml.Instance.GetAllAssembliesDeployedToClient(BaseSourcePath));

		var filesShouldBeDeployedForCargoWiseWeb = GetFilesThatShouldBeDeployedForCargoWiseWeb(excludePdb: false);
		var filesShouldBeDeployedForNetCore = GetFilesShouldBeDeployedForNetCore(filesShouldBeDeployed, excludePdb: true);

		filesShouldBeDeployed = filesShouldBeDeployed.Concat(filesShouldBeDeployedForCargoWiseWeb)
													 .Concat(filesShouldBeDeployedForNetCore)
													 .ToList();

		CheckPackageDifferences(filesShouldBeDeployed, applicationDirectory, removeClientDLLs: false, keepEDIClientFiles: false, isBuildingMaster: true);

		// now let's check building client package from master package
		Directory.Delete(unzipTargetDir, true);
		Directory.CreateDirectory(unzipTargetDir);

		//TestCaseHelper.ClearTable("ReleaseBuild");
		using (var importer = new PackageImporter(new BusinessObjectFactory(), builder.LastPackagePath))
		{
			importer.Import();
		}

		var releaseBuildsInDB = new BusinessObjectFactory().Load<ReleaseBuild>(new ZQuery());
		AssertEquals("ReleaseBuildsInDB count", 1, releaseBuildsInDB.Length);

		string targetPathForEDIPackage = Path.Combine(targetLocation, "EDI");
		Directory.CreateDirectory(targetPathForEDIPackage);

		var clientBuilder = new RuntimePackageBuilderForTest(releaseBuildsInDB[0], targetPathForEDIPackage);
		clientBuilder.Build("EDI", false);

		CheckPackageName(clientBuilder.LastPackagePath);
		CheckPackageForEDIClient(targetPathForEDIPackage, isBasedOnMasterPackage: true, expectBlazorFiles: false, expectNetCoreBinaryFiles: false);
	}

	List<string> GetFilesThatShouldBeDeployedForCargoWiseWeb(bool excludePdb = false)
	{
		var goodBuildLocationWithPathSeparator = goodBuildLocation + Path.DirectorySeparatorChar.ToString();

		var webSubfolders = new List<string> { "AppServer", "SessionBroker", "winzor" };

		var filesThatShouldBeDeployed = webSubfolders
			.SelectMany(webSubfolder => Directory.GetFiles(Path.Combine(goodBuildLocation, webSubfolder), "*", SearchOption.AllDirectories)
				//convert to relative path
				.Select(fullPath => fullPath.Substring(goodBuildLocationWithPathSeparator.Length))
			)
			.Where(relativeFilePath => (!excludePdb || !relativeFilePath.EndsWith(".pdb")))
			.Where(relativeFilePath => !RuntimePackageBuilder.MatchesNetCoreExcludeFilePatterns(relativeFilePath))
			.ToList();

		return filesThatShouldBeDeployed;
	}

	List<string> GetFilesShouldBeDeployedForNetCore(List<string> filesShouldBeDeployed, bool excludePdb)
	{
		var goodBuildLocationWithPathSeparator = goodBuildLocation + Path.DirectorySeparatorChar.ToString();

		var netCoreSubfolders = GetNetCoreDirectories();

		var noDeployFiles = RuntimePackageBuilder.GetNoDeployFiles(BuildXml.Instance)
			.Where(x => netCoreSubfolders.Any(x.StartsWith)).ToHashSet(StringComparer.OrdinalIgnoreCase);

		var alreadyIncludedNetCoreDlls = filesShouldBeDeployed
			.Where(x => netCoreSubfolders.Any(x.StartsWith)).ToHashSet(StringComparer.OrdinalIgnoreCase);

		var netCoreRefsSubfolderRelativePaths = netCoreSubfolders.Select(netCoreSubfolder => Path.Combine(netCoreSubfolder, "refs")).ToList();

		var filesShouldBeDeployedForNetCore = netCoreSubfolders
			.SelectMany(netCoreSubfolder => Directory.GetFiles(Path.Combine(goodBuildLocation, netCoreSubfolder), "*", SearchOption.AllDirectories)
				//convert to relative path
				.Select(fullPath => fullPath.Substring(goodBuildLocationWithPathSeparator.Length))
			)
			.Where(relativeFilePath => (!excludePdb || !relativeFilePath.EndsWith(".pdb"))
				&& !alreadyIncludedNetCoreDlls.Contains(relativeFilePath)
				&& !noDeployFiles.Contains(relativeFilePath)
				&& !netCoreRefsSubfolderRelativePaths.Any(netCoreRefsSubfolderRelativePath => relativeFilePath.StartsWith(netCoreRefsSubfolderRelativePath, StringComparison.OrdinalIgnoreCase))
			)
			.Where(relativeFilePath => !RuntimePackageBuilder.MatchesNetCoreExcludeFilePatterns(relativeFilePath))
			.ToList();

		return filesShouldBeDeployedForNetCore;
	}

	public bool IsDirectoryWritable(string dirPath)
	{
		for (int i = 0; i < 3; i++)
		{
			try
			{
				using (FileStream fs = File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
				{
				}

				return true;
			}
			catch
			{
				Thread.Sleep(100);
			}
		}

		return false;
	}

	[SnailTest]
	[UseSnapshotProtection(skipTransaction: true)]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestBuildMasterPackageThenGeneric()
	{
		new ClientDbSchemaCreationForTesting().RunClientDbCreateScripts(false);

		string clientCMLDllPath = Path.Combine(goodBuildLocation, "ZClientCML.dll");
		File.CreateText(clientCMLDllPath).Close();

		CopyDllsFromStartupPath(goodBuildLocation);

		string clientEDIDocXmlFilePath = Path.Combine(clientDocXmlDir, "EDIDocuments.xml");
		File.CreateText(clientEDIDocXmlFilePath).Close();

		string clientCMLDocXmlFilePath = Path.Combine(clientDocXmlDir, "CMLDocuments.xml");
		File.CreateText(clientCMLDocXmlFilePath).Close();

		foreach (string fileName in Directory.GetFiles(goodBuildLocation))
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}

		Assert("files found in GoodBuildLocation", Directory.GetFiles(goodBuildLocation).Length > 0);

		AssertEquals("output package count", 0, Directory.GetFiles(targetLocation, PackageFilePattern).Length);

		var builder = new RuntimePackageBuilderForTest(templateLocation, goodBuildLocation, targetLocation);
		builder.BuildMaster();

		string[] zippedFiles = Directory.GetFiles(targetLocation, PackageFilePattern);
		AssertEquals("output Package count", 1, zippedFiles.Length);
		string targetPackage = zippedFiles[0];

		CheckPackageName(builder.LastPackagePath);

		AssertEquals("Unzipping", true, ZipCompression.Unzip(targetPackage, unzipTargetDir));

		string[] unzippedDirectories = Directory.GetDirectories(unzipTargetDir);
		AssertEquals("Directories unzipped", 1, unzippedDirectories.Length);
		string[] unzippedFiles = Directory.GetFiles(unzipTargetDir);

		AssertCollectionContains(Path.Combine(unzipTargetDir, "CMLDocuments.xml"), unzippedFiles);
		AssertCollectionContains(Path.Combine(unzipTargetDir, "EDIDocuments.xml"), unzippedFiles);
		AssertCollectionContains(Path.Combine(unzipTargetDir, "ediLoad.exe"), unzippedFiles);
		AssertCollectionContains(Path.Combine(unzipTargetDir, "ediUninstall.exe"), unzippedFiles);

		Array.Sort(unzippedDirectories);

		string distributionDirectory = Path.Combine(unzipTargetDir, "Distribution");
		AssertEquals("Unzipped Directory 0", distributionDirectory, unzippedDirectories[0]);

		string[] unzippedDistributionDirectories = Directory.GetDirectories(distributionDirectory);
		Array.Sort(unzippedDistributionDirectories);
		AssertEquals("Unzipped Distribution Directories count", 2, unzippedDistributionDirectories.Length);

		string applicationDirectory = Path.Combine(distributionDirectory, "Application");
		string installDirectory = Path.Combine(distributionDirectory, "Install");
		AssertEquals("Unzipped Distribution Directory 0", applicationDirectory, unzippedDistributionDirectories[0]);
		AssertEquals("Unzipped Distribution Directory 1", installDirectory, unzippedDistributionDirectories[1]);

		var filesShouldBeDeployed = new List<string>(BuildXml.Instance.GetAllAssembliesDeployedToClient(BaseSourcePath));

		var filesShouldBeDeployedForCargoWiseWeb = GetFilesThatShouldBeDeployedForCargoWiseWeb(excludePdb: false);
		var filesShouldBeDeployedForNetCore = GetFilesShouldBeDeployedForNetCore(filesShouldBeDeployed, excludePdb: false);

		filesShouldBeDeployed = filesShouldBeDeployed.Concat(filesShouldBeDeployedForCargoWiseWeb)
													 .Concat(filesShouldBeDeployedForNetCore)
													 .ToList();

		CheckPackageDifferences(filesShouldBeDeployed, applicationDirectory, removeClientDLLs: false, keepEDIClientFiles: false, isBuildingMaster: true);

		// now lets check building generic package from master package
		Directory.Delete(unzipTargetDir, true);
		Directory.CreateDirectory(unzipTargetDir);

		TestCaseHelper.ClearTable("ReleaseBuild");

		using (var importer = new PackageImporter(new BusinessObjectFactory(), targetPackage))
		{
			importer.Import();
		}

		var releaseBuildsInDB = new BusinessObjectFactory().Load<ReleaseBuild>(new ZQuery());
		AssertEquals("ReleaseBuildsInDB count", 1, releaseBuildsInDB.Length);

		string targetPathForGenericPackage = Path.Combine(targetLocation, "Generic");
		Directory.CreateDirectory(targetPathForGenericPackage);

		var genericBuilder = new RuntimePackageBuilderForTest(releaseBuildsInDB[0], targetPathForGenericPackage);
		genericBuilder.Build();

		zippedFiles = Directory.GetFiles(targetPathForGenericPackage, PackageFilePattern);
		AssertEquals("output Package count", 1, zippedFiles.Length);
		targetPackage = zippedFiles[0];

		CheckPackageName(genericBuilder.LastPackagePath);

		AssertEquals("Unzipping", true, ZipCompression.Unzip(targetPackage, unzipTargetDir));

		unzippedDirectories = Directory.GetDirectories(unzipTargetDir);
		AssertContainsExactElementsInAnyOrder(new[] { distributionDirectory }, unzippedDirectories);

		unzippedFiles = Directory.GetFiles(unzipTargetDir);
		AssertContainsExactElementsInAnyOrder(new[]
		{
				Path.Combine(unzipTargetDir, "ediLoad.exe"),
				Path.Combine(unzipTargetDir, "ediUninstall.exe"),
				Path.Combine(unzipTargetDir, "ReleaseInfo.xml"),
			}, unzippedFiles);

		unzippedDistributionDirectories = Directory.GetDirectories(distributionDirectory);
		Array.Sort(unzippedDistributionDirectories);
		AssertEquals("Unzipped Distribution Directories count", 2, unzippedDistributionDirectories.Length);

		installDirectory = Path.Combine(distributionDirectory, "Install");
		AssertEquals("Unzipped Distribution Directory 0", applicationDirectory, unzippedDistributionDirectories[0]);
		AssertEquals("Unzipped Distribution Directory 1", installDirectory, unzippedDistributionDirectories[1]);

		filesShouldBeDeployed = new List<string>(BuildXml.Instance.GetAllAssembliesDeployedToClient(BaseSourcePath));

		filesShouldBeDeployedForCargoWiseWeb = GetFilesThatShouldBeDeployedForCargoWiseWeb(excludePdb: true);
		filesShouldBeDeployedForNetCore = GetFilesShouldBeDeployedForNetCore(filesShouldBeDeployed, excludePdb: true);

		filesShouldBeDeployed = filesShouldBeDeployed.Concat(filesShouldBeDeployedForCargoWiseWeb)
													 .Concat(filesShouldBeDeployedForNetCore)
													 .ToList();

		CheckPackageDifferences(filesShouldBeDeployed, applicationDirectory, removeClientDLLs: true, keepEDIClientFiles: false, isBuildingMaster: false);
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	[SnailTest]
	public void TestPackageNameIsTheSameAsReleaseBuild()
	{
		CopyDllsFromStartupPath(goodBuildLocation);
		foreach (var fileName in Directory.GetFiles(goodBuildLocation))
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}

		var builder = new RuntimePackageBuilderForTest(templateLocation, goodBuildLocation, targetLocation);
		builder.BuildMaster();

		var enterpriseExeFile = Path.Combine(goodBuildLocation, ExeFileNames.CargoWiseOneExeForVersionInfo);
		var info = FileVersionInfo.GetVersionInfo(enterpriseExeFile);
		var build = new BusinessObjectFactory().New<ReleaseBuild>();
		build.VersionNumber = new VersionNumber(info.FileVersion);
		AssertEquals("PackageName should be the same as ReleaseBuild's PackageName.", build.PackageName, Path.GetFileName(builder.LastPackagePath));
	}

	internal static string GetTrueRootBinPath()
	{
		var sourceFolder = AssemblyLoader.GetBinPath();
		if (sourceFolder.EndsWith("net8.0", StringComparison.OrdinalIgnoreCase) ||
			sourceFolder.EndsWith("winzor", StringComparison.OrdinalIgnoreCase))
		{
			sourceFolder = Directory.GetParent(sourceFolder)!.FullName;
		}
		return sourceFolder;
	}

	public void TestBuildFromMasterEdiEnterprise()
	{
		var sourceFolder = GetTrueRootBinPath();
		using (var zipArchiveFile = TempFile.New())
		{
			var versionExe = Path.Combine(sourceFolder, ExeFileNames.CargoWiseOneExeForVersionInfo);
			using (var zip = new ZipArchive(File.Create(zipArchiveFile.Filename), ZipArchiveMode.Create, false))
			{
				zip.CreateEntryFromFile(versionExe, "Distribution/Application/Enterprise.exe");
			}

			var info = FileVersionInfo.GetVersionInfo(versionExe);
			var factory = new BusinessObjectFactory();
			var releaseBuild = factory.New<ReleaseBuild>();
			releaseBuild.HL_PackagePath = zipArchiveFile.Filename;
			releaseBuild.VersionNumber = new VersionNumber(info.FileVersion);

			var builder = new RuntimePackageBuilderForTest(releaseBuild, targetLocation);
			builder.Build();
			AssertEquals("PackageName should be the same as ReleaseBuild's PackageName.", releaseBuild.PackageName, Path.GetFileName(builder.LastPackagePath));
			Assert(File.Exists(builder.LastPackagePath));
		}
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	[SnailTest]
	public void TestPackageBuilderFailsDevBuild()
	{
		CreateDummyFilesFromFilesOnStartupPath(goodBuildLocation);
		foreach (string fileName in Directory.GetFiles(goodBuildLocation))
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}

		var builder = new RuntimePackageBuilder(templateLocation, goodBuildLocation, targetLocation);
		AssertExceptionThrown<InvalidOperationException>(builder.BuildMaster);
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	[SnailTest]
	public void TestPackageWithWinzorFiles()
	{
		CreateDummyFilesFromFilesOnStartupPath(goodBuildLocation);

		Directory.CreateDirectory(Path.Combine(goodBuildLocation, "AppServer"));
		File.WriteAllText(Path.Combine(goodBuildLocation, "AppServer", "CargoWise.Blazor.Common.dll"), "common");
		File.WriteAllText(Path.Combine(goodBuildLocation, "AppServer", "CargoWise.Blazor.AppServer.dll"), "app server");

		Directory.CreateDirectory(Path.Combine(goodBuildLocation, "AppServer", "wwwroot", "css"));
		File.WriteAllText(Path.Combine(goodBuildLocation, "AppServer", "wwwroot", "css", "classes.css"), "classes css");
		File.WriteAllText(Path.Combine(goodBuildLocation, "AppServer", "wwwroot", "css", "site.css"), "site css");

		Directory.CreateDirectory(Path.Combine(goodBuildLocation, "SessionBroker"));
		File.WriteAllText(Path.Combine(goodBuildLocation, "SessionBroker", "CargoWise.Blazor.Common.dll"), "common");
		File.WriteAllText(Path.Combine(goodBuildLocation, "SessionBroker", "CargoWise.Blazor.SessionBroker.dll"), "session broker");

		Directory.CreateDirectory(Path.Combine(goodBuildLocation, "winzor"));
		File.WriteAllText(Path.Combine(goodBuildLocation, "winzor", "CargoWise.Blazor.Common.dll"), "common");

		Directory.CreateDirectory(Path.Combine(goodBuildLocation, "winzor", "ClientAppInstaller"));
		File.WriteAllText(Path.Combine(goodBuildLocation, "winzor", "ClientAppInstaller", "CargoWise.msix"), "cargowise client msix installer");
		File.WriteAllText(Path.Combine(goodBuildLocation, "winzor", "ClientAppInstaller", "CargowiseAppx.appinstaller"), "cargowise client app installer");

		var builder = new RuntimePackageBuilderForTest(goodBuildLocation, targetLocation);
		builder.Build();
		AssertEquals("Unzipping", true, ZipCompression.Unzip(builder.LastPackagePath, unzipTargetDir));

		var applicationDir = Path.Combine(unzipTargetDir, "Distribution", "Application");
		AssertFileSameAsString(Path.Combine(applicationDir, "AppServer", "CargoWise.Blazor.Common.dll"), "common");
		AssertFileSameAsString(Path.Combine(applicationDir, "AppServer", "CargoWise.Blazor.AppServer.dll"), "app server");
		AssertFileSameAsString(Path.Combine(applicationDir, "AppServer", "wwwroot", "css", "classes.css"), "classes css");
		AssertFileSameAsString(Path.Combine(applicationDir, "AppServer", "wwwroot", "css", "site.css"), "site css");
		AssertFileSameAsString(Path.Combine(applicationDir, "SessionBroker", "CargoWise.Blazor.Common.dll"), "common");
		AssertFileSameAsString(Path.Combine(applicationDir, "SessionBroker", "CargoWise.Blazor.SessionBroker.dll"), "session broker");
		AssertFileSameAsString(Path.Combine(applicationDir, "winzor", "CargoWise.Blazor.Common.dll"), "common");
		AssertFileSameAsString(Path.Combine(applicationDir, "winzor", "ClientAppInstaller", "CargoWise.msix"), "cargowise client msix installer");
		AssertFileSameAsString(Path.Combine(applicationDir, "winzor", "ClientAppInstaller", "CargowiseAppx.appinstaller"), "cargowise client app installer");
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	[SnailTest]
	public void TestPackageNetCoreFiles()
	{
		CreateDummyFilesFromFilesOnStartupPath(goodBuildLocation);

		var netCoreFolder = GetNetCoreDirectories().First();
		Directory.CreateDirectory(Path.Combine(goodBuildLocation, netCoreFolder));

		File.WriteAllText(Path.Combine(goodBuildLocation, netCoreFolder, "NetCore.WebInfrastructure.dll"), "common");
		File.WriteAllText(Path.Combine(goodBuildLocation, netCoreFolder, "CargoWise.ServiceManager.Next.Launcher.dll"), "Launcher-dll");
		File.WriteAllText(Path.Combine(goodBuildLocation, netCoreFolder, "CargoWise.ServiceManager.Next.Launcher.exe"), "Launcher-exe");

		var builder = new RuntimePackageBuilderForTest(goodBuildLocation, targetLocation);
		builder.Build();
		AssertEquals("Unzipping", true, ZipCompression.Unzip(builder.LastPackagePath, unzipTargetDir));

		var applicationDir = Path.Combine(unzipTargetDir, "Distribution", "Application");
		AssertFileSameAsString(Path.Combine(applicationDir, netCoreFolder, "NetCore.WebInfrastructure.dll"), "common");
		AssertFileSameAsString(Path.Combine(applicationDir, netCoreFolder, "CargoWise.ServiceManager.Next.Launcher.dll"), "Launcher-dll");
		AssertFileSameAsString(Path.Combine(applicationDir, netCoreFolder, "CargoWise.ServiceManager.Next.Launcher.exe"), "Launcher-exe");
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestPackageWithBlazorFiles()
	{
		var releaseBuild0 = PreRunOnTestZClientEDIPackage();
		using (EDIDataRegistry.Instance.WinzorLicences.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewLicenceCollectionWithEDI()))
		{
			var enterpriseCode = "NNN";
			AssertBuildWinzorDeployment(releaseBuild0, enterpriseCode,
				isHostedOnWiseCloud: false,
				winzorUpgradeAllowed: false,
				netCoreBinaryAllowed: false);
		}
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestClientPackageWithBlazorFilesWhenWinzorUpgradeAllowedAndIsNotHostedOnWiseCloud()
	{
		var releaseBuild0 = PreRunOnTestZClientEDIPackage();
		using (EDIDataRegistry.Instance.WinzorLicences.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewLicenceCollectionWithEDI()))
		{
			var enterpriseCode = "EDI";
			AssertBuildWinzorDeployment(releaseBuild0, enterpriseCode,
				isHostedOnWiseCloud: false,
				winzorUpgradeAllowed: true,
				netCoreBinaryAllowed: false);
		}
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestClientPackageWithBlazorFilesWhenWinzorUpgradeNotAllowedAndIsHostedOnWiseCloud()
	{
		var releaseBuild0 = PreRunOnTestZClientEDIPackage();
		using (EDIDataRegistry.Instance.WinzorLicences.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewLicenceCollectionWithEDI()))
		{
			var enterpriseCode = "NNN";
			AssertBuildWinzorDeployment(releaseBuild0, enterpriseCode,
				isHostedOnWiseCloud: true,
				winzorUpgradeAllowed: false,
				netCoreBinaryAllowed: false);
		}
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestClientPackageWithBlazorFilesWhenWinzorUpgradeAllowedAndIsHostedOnWiseCloud()
	{
		var releaseBuild0 = PreRunOnTestZClientEDIPackage();
		using (EDIDataRegistry.Instance.WinzorLicences.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewLicenceCollectionWithEDI()))
		{
			var enterpriseCode = "EDI";
			AssertBuildWinzorDeployment(releaseBuild0,
				enterpriseCode,
				isHostedOnWiseCloud: true,
				winzorUpgradeAllowed: true,
				netCoreBinaryAllowed: false);
		}
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestPackageWithNoNetCoreBinary()
	{
		var releaseBuild0 = PreRunOnTestZClientEDIPackage();
		using (EDIDataRegistry.Instance.NetCoreBinaryLicences.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewLicenceCollectionWithEDI()))
		{
			var enterpriseCode = "NNN";
			AssertBuildWinzorDeployment(releaseBuild0, enterpriseCode,
				isHostedOnWiseCloud: false,
				winzorUpgradeAllowed: false,
				netCoreBinaryAllowed: false);
		}
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestPackageWithNetCoreBinary()
	{
		var releaseBuild0 = PreRunOnTestZClientEDIPackage();
		using (EDIDataRegistry.Instance.NetCoreBinaryLicences.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewLicenceCollectionWithEDI()))
		{
			var enterpriseCode = "EDI";
			AssertBuildWinzorDeployment(releaseBuild0, enterpriseCode,
				isHostedOnWiseCloud: false,
				winzorUpgradeAllowed: false,
				netCoreBinaryAllowed: true);
		}
	}

	[SnailTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestPackageWithNetCoreBinaryAndWinzor()
	{
		var releaseBuild0 = PreRunOnTestZClientEDIPackage();
		using (EDIDataRegistry.Instance.NetCoreBinaryLicences.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewLicenceCollectionWithEDI()))
		using (EDIDataRegistry.Instance.WinzorLicences.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewLicenceCollectionWithEDI()))
		{
			var enterpriseCode = "EDI";
			AssertBuildWinzorDeployment(releaseBuild0, enterpriseCode,
				isHostedOnWiseCloud: true,
				winzorUpgradeAllowed: true,
				netCoreBinaryAllowed: true);
		}
	}

	ReleaseBuild PreRunOnTestZClientEDIPackage()
	{
		new ClientDbSchemaCreationForTesting().RunClientDbCreateScripts(false);

		string clientCMLDllPath = Path.Combine(goodBuildLocation, "ZClientCML.dll");
		File.CreateText(clientCMLDllPath).Close();

		CopyDllsFromStartupPath(goodBuildLocation);

		string clientEDIDocXmlFilePath = Path.Combine(clientDocXmlDir, "EDIDocuments.xml");
		File.CreateText(clientEDIDocXmlFilePath).Close();

		string clientCMLDocXmlFilePath = Path.Combine(clientDocXmlDir, "CMLDocuments.xml");
		File.CreateText(clientCMLDocXmlFilePath).Close();

		foreach (string fileName in Directory.GetFiles(goodBuildLocation))
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}

		Assert("files found in GoodBuildLocation", Directory.GetFiles(goodBuildLocation).Length > 0);

		AssertEquals("output package count", 0, Directory.GetFiles(targetLocation, PackageFilePattern).Length);

		var builder = new RuntimePackageBuilderForTest(templateLocation, goodBuildLocation, targetLocation);
		builder.BuildMaster();

		CheckPackageName(builder.LastPackagePath);

		string[] zippedFiles = Directory.GetFiles(targetLocation, PackageFilePattern);
		AssertEquals("output Package count", 1, zippedFiles.Length);
		string targetPackage = zippedFiles[0];

		AssertEquals("Unzipping", true, ZipCompression.Unzip(targetPackage, unzipTargetDir));

		var applicationDir = Path.Combine(unzipTargetDir, "Distribution", "Application");
		AssertEquals(true, Directory.Exists(Path.Combine(applicationDir, "AppServer")));
		AssertEquals(true, Directory.Exists(Path.Combine(applicationDir, "SessionBroker")));
		AssertEquals(true, Directory.Exists(Path.Combine(applicationDir, "winzor")));
		foreach (var netCoreDirectory in GetNetCoreDirectories())
		{
			AssertEquals(true, Directory.Exists(Path.Combine(applicationDir, netCoreDirectory)));
		}

		DeleteAndRecreate(unzipTargetDir);
		DeleteAndRecreate(goodBuildLocation);

		using (var importer = new PackageImporter(new BusinessObjectFactory(), builder.LastPackagePath))
		{
			importer.Import();
		}

		var releaseBuildsInDB = new BusinessObjectFactory().Load<ReleaseBuild>(new ZQuery());
		AssertEquals("ReleaseBuildsInDB count", 1, releaseBuildsInDB.Length);
		return releaseBuildsInDB[0];
	}

	void AssertBuildWinzorDeployment(ReleaseBuild masterBuild, string enterpriseCode, bool isHostedOnWiseCloud, bool winzorUpgradeAllowed, bool netCoreBinaryAllowed)
	{
		DeleteAndRecreate(unzipTargetDir);

		var builder = masterBuild == null
			? new RuntimePackageBuilderForTest(templateLocation, goodBuildLocation, targetLocation)
			: new RuntimePackageBuilderForTest(masterBuild, targetLocation);
		builder.Build(enterpriseCode, isHostedOnWiseCloud);

		var targetPackage = Directory.GetFiles(targetLocation, PackageFilePattern).Single();
		AssertEquals("Unzip package", true, ZipCompression.Unzip(targetPackage, unzipTargetDir));

		bool expectWinzor = winzorUpgradeAllowed || isHostedOnWiseCloud;

		var applicationDir = Path.Combine(unzipTargetDir, "distribution/application");
		AssertEquals(expectWinzor, Directory.Exists(Path.Combine(applicationDir, "AppServer")));
		AssertEquals(expectWinzor, Directory.Exists(Path.Combine(applicationDir, "SessionBroker")));
		AssertEquals(expectWinzor, Directory.Exists(Path.Combine(applicationDir, "winzor")));

		bool expectNetCore = netCoreBinaryAllowed || isHostedOnWiseCloud;
		foreach (var netCoreDirectory in GetNetCoreDirectories())
		{
			AssertEquals(expectNetCore, Directory.Exists(Path.Combine(applicationDir, netCoreDirectory)));
		}
	}

	NeoUpgradeLicenceCollection CreateNewLicenceCollectionWithEDI()
	{
		var factory = new BusinessObjectFactory();
		var licence1 = CreateNewLicence(factory, new Guid("18F0B663-0F42-49B7-AC0E-DAC48079947B"), "EDI");

		var allowedLicences = new NeoUpgradeLicenceCollection();
		var allowedLicence1 = allowedLicences.AddNew();
		allowedLicence1.LicencePK = licence1.PK;

		return allowedLicences;
	}

	void CheckPackageForEDIClient(string targetLocation, bool isBasedOnMasterPackage, bool expectBlazorFiles, bool expectNetCoreBinaryFiles)
	{
		string[] zippedFiles = Directory.GetFiles(targetLocation, PackageFilePattern);
		AssertEquals("output Package count", 1, zippedFiles.Length);
		string targetPackage = zippedFiles[0];

		AssertEquals("Unzipping", true, ZipCompression.Unzip(targetPackage, unzipTargetDir));

		string[] unzippedDirectories = Directory.GetDirectories(unzipTargetDir);
		var distributionDirectory = Path.Combine(unzipTargetDir, "Distribution");
		AssertContainsExactElementsInAnyOrder(new[] { distributionDirectory }, unzippedDirectories);

		string[] unzippedFiles = Directory.GetFiles(unzipTargetDir);

		var expectedFiles = new List<string>
			{
				Path.Combine(unzipTargetDir, "ediLoad.exe"),
				Path.Combine(unzipTargetDir, "ediUninstall.exe"),
				Path.Combine(unzipTargetDir, "EDIDocuments.xml")
			};

		if (isBasedOnMasterPackage)
		{
			expectedFiles.Add(Path.Combine(unzipTargetDir, "ReleaseInfo.xml"));
		}
		AssertContainsExactElementsInAnyOrder(expectedFiles, unzippedFiles);

		string[] unzippedDistributionDirectories = Directory.GetDirectories(distributionDirectory);
		Array.Sort(unzippedDistributionDirectories);
		AssertEquals("Unzipped Distribution Directories count", 2, unzippedDistributionDirectories.Length);

		string applicationDirectory = Path.Combine(distributionDirectory, "Application");
		string installDirectory = Path.Combine(distributionDirectory, "Install");
		AssertEquals("Unzipped Distribution Directory 0", applicationDirectory, unzippedDistributionDirectories[0]);
		AssertEquals("Unzipped Distribution Directory 0", installDirectory, unzippedDistributionDirectories[1]);

		var unzippedZClientFilesHashSet = Directory.GetFiles(applicationDirectory, "*", SearchOption.AllDirectories)
			.Select(Path.GetFileName)
			.Where(fileName => fileName.StartsWith("zclient", StringComparison.InvariantCultureIgnoreCase))
			.ToHashSet(StringComparer.InvariantCultureIgnoreCase);

		Assert("zclientedi.dll is missing", unzippedZClientFilesHashSet.Contains("zclientedi.dll"));
		Assert("zclientedi.business.dll is missing", unzippedZClientFilesHashSet.Contains("zclientedi.business.dll"));
		Assert("zclientedi.gui.dll is missing", unzippedZClientFilesHashSet.Contains("zclientedi.gui.dll"));
		Assert("zclientedi.business.xmlserializers.dll is missing", unzippedZClientFilesHashSet.Contains("zclientedi.business.xmlserializers.dll"));
		Assert("zclientedi.xmlserializers.dll should not be present", !unzippedZClientFilesHashSet.Contains("zclientedi.xmlserializers.dll"));
		Assert("zclientedi.pdb should not be present", !unzippedZClientFilesHashSet.Contains("zclientedi.pdb"));

		var filesShouldBeDeployed = new List<string>(BuildXml.Instance.GetAllAssembliesDeployedToClient(BaseSourcePath));

		var filesShouldBeDeployedForNetCore = expectNetCoreBinaryFiles
			? GetFilesShouldBeDeployedForNetCore(filesShouldBeDeployed, excludePdb: true)
			: [];
		var filesShouldBeDeployedForCargoWiseWeb = expectBlazorFiles
			? GetFilesThatShouldBeDeployedForCargoWiseWeb(excludePdb: true)
			: [];

		if (!expectNetCoreBinaryFiles)
		{
			var netCoreDirectories = GetNetCoreDirectories();
			filesShouldBeDeployed.RemoveAll(file =>
				netCoreDirectories.Any(filePattern =>
					file.StartsWith(filePattern, StringComparison.OrdinalIgnoreCase)));
		}

		filesShouldBeDeployed = filesShouldBeDeployed.Concat(filesShouldBeDeployedForCargoWiseWeb)
													 .Concat(filesShouldBeDeployedForNetCore)
													 .ToList();

		CheckPackageDifferences(filesShouldBeDeployed, applicationDirectory, removeClientDLLs: true, keepEDIClientFiles: true, isBuildingMaster: false);
	}

	void CheckPackageDifferences(List<string> relativePathFilesThatShouldBeDeployed, string unzippedApplicationDirectory, bool removeClientDLLs, bool keepEDIClientFiles, bool isBuildingMaster)
	{
		if (removeClientDLLs)
		{
			for (var index = 0; index < relativePathFilesThatShouldBeDeployed.Count;)
			{
				var relativeFilePath = relativePathFilesThatShouldBeDeployed[index];

				if (Path.IsPathRooted(relativeFilePath))
				{
					throw new ArgumentException($"Should contain relative paths only. index {index} is an absolute path: {relativeFilePath}", nameof(relativePathFilesThatShouldBeDeployed));
				}

				var fileName = relativeFilePath;
				if (fileName.StartsWith("ZClient") &&
					(!keepEDIClientFiles ||
					(fileName != "ZClientEDI.dll" &&
					fileName != "ZClientEDI.XmlSerializers.dll" &&
					fileName != "ZClientEDI.Business.dll" &&
					fileName != "ZClientEDI.Business.XmlSerializers.dll" &&
					fileName != "ZClientEDI.GUI.dll" &&
					fileName != "ZClientWebEDI.dll" &&
					fileName != "ZClientWebEDI.zip" &&
					fileName != "ZClientWebCargoWiseEDI.dll" &&
					fileName != "ZClientEDI.ElasticSearchAdaptor.dll")))
				{
					relativePathFilesThatShouldBeDeployed.RemoveAt(index);
				}
				else
				{
					index++;
				}
			}
		}

		relativePathFilesThatShouldBeDeployed.Add("GlowWebDeploy.zip");

		var undeployedFileNamesMessage = new StringBuilder();

		//build a list of all unzipped files, using a relative path to the unzipped folder
		var unzippedApplicationDirectoryWithPathSeparator = unzippedApplicationDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()) ? unzippedApplicationDirectory : unzippedApplicationDirectory + Path.DirectorySeparatorChar;
		var unzippedAllFilesHashSet = Directory.GetFiles(unzippedApplicationDirectory, "*", SearchOption.AllDirectories)
										.Select(file => file.Substring(unzippedApplicationDirectoryWithPathSeparator.Length))
										.ToHashSet(StringComparer.OrdinalIgnoreCase);

		var relativePathFilesThatShouldBeDeployedHashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var relativePathFileThatShouldBeDeployed in relativePathFilesThatShouldBeDeployed)
		{
			if (relativePathFilesThatShouldBeDeployedHashSet.Contains(relativePathFileThatShouldBeDeployed))
			{
				Fail($"{relativePathFileThatShouldBeDeployed} in {nameof(relativePathFilesThatShouldBeDeployed)} list more than once");
			}
			else
			{
				relativePathFilesThatShouldBeDeployedHashSet.Add(relativePathFileThatShouldBeDeployed);
			}

			if (!unzippedAllFilesHashSet.Contains(relativePathFileThatShouldBeDeployed))
			{
				undeployedFileNamesMessage.Append(relativePathFileThatShouldBeDeployed + "  ");
			}
		}

		var hasPdbFiles = false;
		var filesThatShouldNotBeDeployedMessage = new StringBuilder();

		foreach (var unzippedRelativeFilePath in unzippedAllFilesHashSet)
		{
			if (unzippedRelativeFilePath.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase))
			{
				hasPdbFiles = true;
				continue;
			}

			if (!relativePathFilesThatShouldBeDeployedHashSet.Contains(unzippedRelativeFilePath))
			{
				filesThatShouldNotBeDeployedMessage.Append(unzippedRelativeFilePath + "  ");
			}
		}

		if (undeployedFileNamesMessage.Length > 0 || filesThatShouldNotBeDeployedMessage.Length > 0)
		{
			if (undeployedFileNamesMessage.Length == 0)
			{
				undeployedFileNamesMessage.Append("<none>");
			}

			if (filesThatShouldNotBeDeployedMessage.Length == 0)
			{
				filesThatShouldNotBeDeployedMessage.Append("<none>");
			}

			var errorMessage = "The following files should be deployed, but were not: " + undeployedFileNamesMessage.ToString() + System.Environment.NewLine +
				"The following files should not be deployed: " + filesThatShouldNotBeDeployedMessage.ToString() + System.Environment.NewLine +
				"Total files that should be deployed: " + relativePathFilesThatShouldBeDeployed.Count + "  and Total files that were deployed " + unzippedAllFilesHashSet.Count;
			Fail(errorMessage);
		}
		else
		{
			if (!hasPdbFiles)
			{
				AssertContainsExactElementsInAnyOrder(relativePathFilesThatShouldBeDeployed.Select(x => x.ToUpperInvariant()), unzippedAllFilesHashSet.Select(x => x.ToUpperInvariant()));
			}
		}

		AssertEquals("Only master build should have pdb files", isBuildingMaster, hasPdbFiles);
	}

	void CheckPackageName(string resultPackageName)
	{
		var mainExeFile = Path.Combine(goodBuildLocation, ExeFileNames.CargoWiseOneExeForVersionInfo);
		Assert(ExeFileNames.CargoWiseOneExeForVersionInfo + " exists in Source path", File.Exists(mainExeFile));

		FileVersionInfo info = FileVersionInfo.GetVersionInfo(mainExeFile);
		string dateTimePart = new VersionNumber(info).GetReleaseDate().ToString("yyyyMMdd_HHmm00_");
		string packageName = "Package" + dateTimePart + info.FileVersion.Replace('.', '_') + ".edp";
		AssertEquals("PackageName", packageName, Path.GetFileName(resultPackageName));
	}

	internal void CopyDllsFromStartupPath(string targetPath)
	{
		string startupPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		CreateFolders(startupPath, targetPath);

		var filesInStartupPath = Directory.GetFiles(startupPath, "*.*", SearchOption.AllDirectories);
		Parallel.ForEach(filesInStartupPath, fileName =>
		{
			var targetFileName = fileName.Replace(startupPath, targetPath);
			if (targetFileName.EndsWith(".pdb") || targetFileName.EndsWith(".dll") || targetFileName.EndsWith(".zrs"))
			{
				File.WriteAllText(targetFileName, targetFileName);
			}
			else
			{
				File.Copy(fileName, targetFileName, true);
			}
		});

		ImportableBuild.CopyDocumentXmlToBin(targetPath);
		ImportableBuild.Prepare(BaseSourcePath, targetPath);
	}

	internal void CreateDummyFilesFromFilesOnStartupPath(string targetPath)
	{
		var startupPath = System.Windows.Forms.Application.StartupPath;
		if (startupPath.StartsWith("C:\\Program Files\\Microsoft Visual Studio")) // running in Visual Studio
		{
			startupPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		CreateFolders(startupPath, targetPath);

		var filesInStartupPath = Directory.GetFiles(startupPath, "*.*", SearchOption.AllDirectories);
		Parallel.ForEach(filesInStartupPath, filePath =>
		{
			var targetFilePath = filePath.Replace(startupPath, targetPath);
			var fileName = Path.GetFileName(filePath);
			if (0 == string.Compare(fileName, "Build.xml", StringComparison.OrdinalIgnoreCase) ||
				0 == string.Compare(fileName, ExeFileNames.CargoWiseOneExeForVersionInfo, StringComparison.OrdinalIgnoreCase) ||
				0 == string.Compare(fileName, "Enterprise.exe", StringComparison.OrdinalIgnoreCase) ||
				0 == string.Compare(fileName, "BusinessObjects.xml", StringComparison.OrdinalIgnoreCase))
			{
				File.Copy(filePath, targetFilePath, true);
			}
			else
			{
				using (File.Create(targetFilePath))
				{
				}
			}
		});

		ImportableBuild.CopyDocumentXmlToBin(targetPath);
		ImportableBuild.Prepare(BaseSourcePath, targetPath);
	}

#pragma warning disable CS0436 // Type conflicts with imported type - due to InternalsVisibleTo
	IReadOnlyList<string> GetNetCoreDirectories() => RuntimePackageBuilder.NetCoreFilePatterns;
#pragma warning restore CS0436 // Type conflicts with imported type

	void CreateFolders(string rootPath, string targetFolder)
	{
		foreach (string dirPath in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
		{
			Directory.CreateDirectory(dirPath.Replace(rootPath, targetFolder));
		}
	}

	public void TestBuildXmlWhenOtherFilesSectionHasDirectoryElement()
	{
		// Arrange
		if (!Directory.Exists(clientDocXmlDir))
		{
			Directory.CreateDirectory(clientDocXmlDir);
		}

		var exeFilePath = Path.Combine(goodBuildLocation, ExeFileNames.CargoWiseOneExeForVersionInfo);
		File.Create(exeFilePath).Close();

		var buildXml =
@"<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<OtherFiles>
		<Directory CopyFrom=""Folder1"" CopyTo=""DestFolder1"" />
		<Directory DeployToClients=""false"" CopyFrom=""Folder2"" CopyTo=""DestFolder2"" />
	</OtherFiles>
</Build>
";
		var testBuildXmlPath = Path.Combine(goodBuildLocation, "Build.xml");
		File.WriteAllText(testBuildXmlPath, buildXml);

		var folder1 = Path.Combine(goodBuildLocation, "Folder1");
		DeleteAndRecreate(folder1);
		File.WriteAllText(Path.Combine(folder1, "TestFile1.txt"), "test 1");
		File.WriteAllText(Path.Combine(folder1, "TestFile2.txt"), "test 2");

		var folder2 = Path.Combine(goodBuildLocation, "Folder2");
		DeleteAndRecreate(folder2);
		File.WriteAllText(Path.Combine(folder2, "TestFile3.txt"), "test 3");

		// Act
		new RuntimePackageBuilderForTest(goodBuildLocation, targetLocation)
			.Build();

		// Assert
		var zippedFiles = Directory.GetFiles(targetLocation, PackageFilePattern);
		AssertEquals("output Package count", 1, zippedFiles.Length);
		var targetPackage = zippedFiles[0];
		AssertEquals("Unzipping", true, ZipCompression.Unzip(targetPackage, unzipTargetDir));

		var distributionDirectory = Path.Combine(unzipTargetDir, "Distribution");
		var applicationDirectory = Path.Combine(distributionDirectory, "Application");
		var destFolder1 = Path.Combine(applicationDirectory, "DestFolder1");
		var destFolder2 = Path.Combine(applicationDirectory, "DestFolder2");
		AssertEquals("DestFolder1 exists", true, Directory.Exists(destFolder1));
		AssertEquals("DestFolder2 not exists since not deployed to clients", false, Directory.Exists(destFolder2));
		AssertEquals("TestFile1.txt exists", true, File.Exists(Path.Combine(destFolder1, "TestFile1.txt")));
		AssertEquals("TestFile2.txt exists", true, File.Exists(Path.Combine(destFolder1, "TestFile2.txt")));
	}

	#region Implementation

	string templateLocation;
	string goodBuildLocation;
	string targetLocation;
	string unzipTargetDir;
	string clientDocXmlDir;

	protected override void SetUp()
	{
		base.SetUp();

		templateLocation = Path.Combine(Env.TempPath, "BuilderTestTemplate");
		goodBuildLocation = Path.Combine(Env.TempPath, "BuilderTestSource");
		targetLocation = Path.Combine(Env.TempPath, "BuilderTestTarget");
		unzipTargetDir = Path.Combine(Env.TempPath, "BuilderUnzipTarget");
		clientDocXmlDir = Path.Combine(goodBuildLocation, "DocumentXmls");

		DeleteAndRecreate(templateLocation);
		DeleteAndRecreate(goodBuildLocation);
		DeleteAndRecreate(targetLocation);
		DeleteAndRecreate(unzipTargetDir);

		CreateReleaseBuildFiles();
		CreateTemplateFiles();
		CreateGlowFiles();
	}

	readonly string[] releaseBuildFiles = new string[] {
			"EnterpriseWebDeploy.zip",
			"ediLoad.exe",
			"ediUninstall.exe"
		};

	void CreateReleaseBuildFiles()
	{
		foreach (string file in releaseBuildFiles)
		{
			File.Create(Path.Combine(goodBuildLocation, file)).Close();
		}

		foreach (string file in BuildXml.Instance.GetWebPackageFiles())
		{
			File.Create(Path.Combine(goodBuildLocation, file)).Close();
		}
	}

	void CreateTemplateFiles()
	{
		Directory.CreateDirectory(Path.Combine(templateLocation, @"Distribution\Application"));

		foreach (string directory in BuildXml.Instance.GetRuntimePackageDistributionInstallDirectories())
		{
			var templateInstallDirectory = Path.Combine(templateLocation, @"Distribution\Install", directory);
			Directory.CreateDirectory(templateInstallDirectory);
			File.Create(Path.Combine(templateInstallDirectory, "Foo")).Close();

			if (directory == "Font")
			{
				File.Create(Path.Combine(templateInstallDirectory, "3OF9.TTF")).Close();
			}
		}
	}

	void CreateGlowFiles()
	{
		Directory.CreateDirectory(Path.Combine(goodBuildLocation, "Client"));
		File.Create(Path.Combine(goodBuildLocation, "GlowWebDeploy.zip")).Close();
	}

	void DeleteAndRecreate(string directoryName)
	{
		TempDirectory.DeleteDirectory(directoryName);
		Directory.CreateDirectory(directoryName);
	}

	protected override void TearDown()
	{
		base.TearDown();

		TempDirectory.DeleteDirectory(templateLocation);
		TempDirectory.DeleteDirectory(goodBuildLocation);
		TempDirectory.DeleteDirectory(targetLocation);
		TempDirectory.DeleteDirectory(unzipTargetDir);
	}
	#endregion

	class RuntimePackageBuilderForTest : RuntimePackageBuilder
	{
		public RuntimePackageBuilderForTest(ReleaseBuild build, string targetPath)
			: base(build, targetPath)
		{
		}

		public RuntimePackageBuilderForTest(string goodBuildPath, string targetPath)
			: base(goodBuildPath, targetPath)
		{
		}

		public RuntimePackageBuilderForTest(string templatePath, string goodBuildPath, string targetPath)
			: base(templatePath, goodBuildPath, targetPath)
		{
		}

		protected override void ValidateDeployableBuild(FileVersionInfo info)
		{
		}
	}
}
