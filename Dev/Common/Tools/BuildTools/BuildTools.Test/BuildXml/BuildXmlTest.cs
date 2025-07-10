using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class BuildXmlTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFullyQualifiedFileNameMustBeLessThan260CharactersAsRestrictedByWindows()
		{
			AssertFullyQualifiedFileNameMustBeLessThan260CharactersAsRestrictedByWindows(BaseSourcePath);
		}

		public void TestFullyQualifiedFileNameExceeded260CharactersAsRestrictedByWindows()
		{
			using var embeddedResourceRetriever = new EmbeddedResourceRetriever();
			_ = embeddedResourceRetriever.SaveResourceToFile("TestBuildXmlHavingLongFileNames.xml", BuildConstants.BuildXmlFileName);
			_ = AssertExceptionThrown<NotSupportedException>(() =>
			{
				AssertFullyQualifiedFileNameMustBeLessThan260CharactersAsRestrictedByWindows(embeddedResourceRetriever.DirectoryPath);
			});
		}

		void AssertFullyQualifiedFileNameMustBeLessThan260CharactersAsRestrictedByWindows(string sourcePath)
		{
			const int MaxPathLengthRestrictedByWindows = 260;
			const int BaselineMaxAllowedPathLength = 126;
			const string TypicalWebInstallationTempDir = @"C:\ProgramData\WiseTech Global\WebInstallationTemp\0883390e-7a86-4da8-b943-b5f40e691fb3\pnvu2z2h.w3b\Distribution\Application\";

			var maxPossiblePathLength = BaselineMaxAllowedPathLength + TypicalWebInstallationTempDir.Length;
			AssertLessThan("BaselineMaxLength const value, if need to be adjusted, must be within the max allowed path length", maxPossiblePathLength, MaxPathLengthRestrictedByWindows);

			var buildXmlFile = Path.Combine(sourcePath, BuildConstants.BuildXmlFileName);
			Assert($"Build.xml file does not exist: {buildXmlFile}", File.Exists(buildXmlFile));

			var buildXml = new BuildXml(buildXmlFile);
			var deploymentFiles = buildXml.GetAllAssembliesDeployedToClient(sourcePath);
			var filesHavingTooLongPath = deploymentFiles.Where(x => x.Length >= BaselineMaxAllowedPathLength).Select(x => $"({x.Length}): {x}").ToArray();
			if (filesHavingTooLongPath.Length > 0)
			{
				throw new NotSupportedException($@"The following files would potentially have a fully qualified file name that is 260 characters or more when combined with run-time installation dir.
please shorten them.

Max allowed length of a file name combined with target sub-folder: {BaselineMaxAllowedPathLength}

{string.Join(Environment.NewLine, filesHavingTooLongPath)}
");
			}

			var longestFile = deploymentFiles.OrderByDescending(x => x.Length).First();
			Assert($"Longest file: {longestFile}, Length: {longestFile.Length}", condition: true);
		}

		public void TestContainsStrictlyVersionedFile()
		{
			var strictlyVersioned = testBuildXml.GetStrictlyVersionedOtherFiles();
			AssertEquals("Should have one strictly versioned file", 1, strictlyVersioned.Count);

			var kvp = strictlyVersioned[0];
			AssertEquals("Path\\To\\Assembly.dll", kvp.Key);
			AssertEquals(new Version("14.5.7.9"), kvp.Value);
		}

		public void TestDoesNotContainOtherFilesToSign()
		{
			string[] assemblies = testBuildXml.GetAllAssembliesDeployedToClient(tempDirectory);
			AssertNotContained(assemblies);
			assemblies = testBuildXml.GetOtherDeployedFiles(tempDirectory).ToArray();
			AssertNotContained(assemblies);
		}

		public void TestGetOtherDeployedFilesIncludesDirectoryFiles()
		{
			var files = testBuildXml.GetOtherDeployedFiles(tempDirectory).ToArray();
			AssertCollectionContains(schemaSaveResultFile, files);
		}

		public void TestGetOtherDeployedFilesFailsWhenDirectoryDoesNotExist()
		{
			string buildXmlText = retriever.GetString("TestBuild.xml", System.Text.Encoding.UTF8);
			buildXmlText = buildXmlText.Replace(@"<Directory CopyFrom=""Testing""", @"<Directory CopyFrom=""C:\This\Directory\Does\Not\Exist""");
			var tempFile = TempForTest.GetTempFileName();
			try
			{
				File.WriteAllText(tempFile, buildXmlText);
				var buildXml = new BuildXml(tempFile);
				AssertExceptionThrown(typeof(DirectoryNotFoundException), delegate
				{
					buildXml.GetOtherDeployedFiles(tempDirectory);
				});
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		void AssertNotContained(string[] fileNames)
		{
			AssertCollectionNotContains("ShouldOnlyBeInOtherFilesToSign", fileNames);
		}

		public void TestGetOtherDeployedFilesCopyFrom()
		{
			BuildXml testBuildXml = new BuildXml(testBuildXmlFile);
			Dictionary<string, string> files = testBuildXml.GetOtherDeployedFilesWithCopyFrom(tempDirectory);
			AssertCollectionNotContains("PDF32.dll", files.Keys);
			AssertCollectionContains("Enterprise.Tracking.Module.dll", files.Keys);
			AssertCollectionContains("Flexcel.dll", files.Keys);
			AssertCollectionContains("LinkedAssembly.dll", files.Keys);

			AssertEquals(Path.Combine("some path", "Enterprise.Tracking.Module.dll"), files["Enterprise.Tracking.Module.dll"]);
			AssertEquals(Path.Combine("temp\\1", "Flexcel.dll"), files["Flexcel.dll"]);
			Assert(files["DestDirectory\\SchemaSaveResult.xml"].EndsWith(schemaSaveResultFile));
			Assert(files["DestDirectory\\TestSubFolder\\TestSubFolderFile.xml"].EndsWith(testSubFolderFile));

			AssertEquals(Path.Combine(@"\\linked\network\path\Client", "LinkedAssembly.dll"), files["LinkedAssembly.dll"]);
		}

		public void TestGetAllRedirctionFiles()
		{
			var testBuildXml = new BuildXml(testBuildXmlFile);
			AssertContainsExactElementsInAnyOrder(new string[] { @"Testing\Test.link" }, testBuildXml.GetAllRedirectionFiles(tempDirectory));
		}

		public void TestCargoWiseElementsWhenNotIncludingSubmodules()
		{
			var buildXmlPath = retriever.SaveResourceToFile("TestBuildWithSubmodules.xml", "TestBuildWithSubmodules.xml", @"Testing");
			_ = retriever.SaveResourceToFile("TestFolder1Build.xml", BuildConstants.BuildXmlFileName, @"Testing\TestFolder1");
			_ = retriever.SaveResourceToFile("TestFolder2Build.xml", BuildConstants.BuildXmlFileName, @"Testing\TestFolder2");
			var buildXml = new BuildXml(buildXmlPath, flattenSubmodules: false);
			AssertSubModulesData(buildXml, "Table1", "View1", "BusinessObject1-ModMono.Architecture", @"src\Core\Architecture\ModMono.Architecture.sln");
		}

		public void TestLoadSubmodulesData()
		{
			var testBuildWithSubmodulesXml = retriever.SaveResourceToFile("TestBuildWithSubmodules.xml", "TestBuildWithSubmodules.xml", @"Testing");
			var testFolder1BuildXml = retriever.SaveResourceToFile("TestFolder1Build.xml", BuildConstants.BuildXmlFileName, @"Testing\TestFolder1");
			retriever.SaveResourceToFile("ModMono.TestSub1.link", "ModMono.TestSub1.sln", @"Testing\TestFolder1\TestSub1");
			var testSub2BuildXml = retriever.SaveResourceToFile("TestSub2Build.xml", BuildConstants.BuildXmlFileName, @"Testing\TestFolder1\TestSub2");
			retriever.SaveResourceToFile("ModMono.TestSubA.link", BuildConstants.BuildXmlFileName, @"Testing\TestFolder1\TestSub2\TestSubA");

			var testFolder2BuildXml = retriever.SaveResourceToFile("TestFolder2Build.xml", BuildConstants.BuildXmlFileName, @"Testing\TestFolder2");
			retriever.SaveResourceToFile("ModMono.TestSub3.link", "ModMono.TestSub3.sln", @"Testing\TestFolder2\TestSub3");
			var testSub4BuildXml = retriever.SaveResourceToFile("TestSub4Build.xml", BuildConstants.BuildXmlFileName, @"Testing\TestFolder2\TestSub4");
			retriever.SaveResourceToFile("ModMono.TestSubB.link", "ModMono.TestSubB.sln", @"Testing\TestFolder2\TestSub4\TestSubB");
			var testSub5BuildXml = retriever.SaveResourceToFile("TestSub5Build.xml", BuildConstants.BuildXmlFileName, @"Testing\TestFolder2\TestSub5");
			retriever.SaveResourceToFile("ModMono.TestSubC.link", "ModMono.TestSubC.sln", @"Testing\TestFolder2\TestSub5\TestSubC");
			var testSubDBuildXml = retriever.SaveResourceToFile("TestSubDBuild.xml", BuildConstants.BuildXmlFileName, @"Testing\TestFolder2\TestSub5\TestSubD");
			retriever.SaveResourceToFile("ModMono.TestSubE.link", "ModMono.TestSubE.sln", @"Testing\TestFolder2\TestSub5\TestSubD\TestSubE");

			CombineAssertions("testSub2BuildXml", () => AssertSubModulesData(testSub2BuildXml, "TableSub2", "ViewSub2", "BusinessObjectSub2-ModMono.TestSubA", @"TestSubA\ModMono.TestSubA.sln"));
			CombineAssertions("testFolder1BuildXml", () => AssertSubModulesData(testFolder1BuildXml, @"TableFolder1
TableSub2", @"ViewFolder1
ViewSub2", @"BusinessObjectFolder1-ModMono.TestSub1
BusinessObjectSub2-ModMono.TestSubA", @"TestSub1\ModMono.TestSub1.sln
TestSub2\TestSubA\ModMono.TestSubA.sln"));

			CombineAssertions("testSub4BuildXml", () => AssertSubModulesData(testSub4BuildXml, "TableSub4", "ViewSub4", "BusinessObjectSub4-ModMono.TestSubB", @"TestSubB\ModMono.TestSubB.sln"));
			CombineAssertions("testSubDBuildXml", () => AssertSubModulesData(testSubDBuildXml, "TableSubD", "ViewSubD", "BusinessObjectSubD-ModMono.TestSubE", @"TestSubE\ModMono.TestSubE.sln"));
			CombineAssertions("testSub5BuildXml", () => AssertSubModulesData(testSub5BuildXml, @"TableSub5
TableSubD", @"ViewSubF
ViewSubD", @"BusinessObjectSub5-ModMono.TestSubC
BusinessObjectSubD-ModMono.TestSubE", @"TestSubC\ModMono.TestSubC.sln
TestSubD\TestSubE\ModMono.TestSubE.sln"));
			CombineAssertions("testFolder2BuildXml", () => AssertSubModulesData(testFolder2BuildXml, @"TableFolder2
TableSub4
TableSub5
TableSubD", @"ViewSubFolder2
ViewSub4
ViewSubF
ViewSubD", @"BusinessObjectFolder2-ModMono.TestSub3
BusinessObjectSub4-ModMono.TestSubB
BusinessObjectSub5-ModMono.TestSubC
BusinessObjectSubD-ModMono.TestSubE", @"TestSub3\ModMono.TestSub3.sln
TestSub4\TestSubB\ModMono.TestSubB.sln
TestSub5\TestSubC\ModMono.TestSubC.sln
TestSub5\TestSubD\TestSubE\ModMono.TestSubE.sln"));

			CombineAssertions("testBuildWithSubmodulesXml", () => AssertSubModulesData(testBuildWithSubmodulesXml, @"Table1
TableFolder1
TableSub2
TableFolder2
TableSub4
TableSub5
TableSubD", @"View1
ViewFolder1
ViewSub2
ViewSubFolder2
ViewSub4
ViewSubF
ViewSubD", @"BusinessObject1-ModMono.Architecture
BusinessObjectFolder1-ModMono.TestSub1
BusinessObjectFolder2-ModMono.TestSub3
BusinessObjectSub2-ModMono.TestSubA
BusinessObjectSub4-ModMono.TestSubB
BusinessObjectSub5-ModMono.TestSubC
BusinessObjectSubD-ModMono.TestSubE", @"src\Core\Architecture\ModMono.Architecture.sln
TestFolder1\TestSub1\ModMono.TestSub1.sln
TestFolder1\TestSub2\TestSubA\ModMono.TestSubA.sln
TestFolder2\TestSub3\ModMono.TestSub3.sln
TestFolder2\TestSub4\TestSubB\ModMono.TestSubB.sln
TestFolder2\TestSub5\TestSubC\ModMono.TestSubC.sln
TestFolder2\TestSub5\TestSubD\TestSubE\ModMono.TestSubE.sln"));
		}

		static void AssertSubModulesData(string buildXmlPath, string expectedExcludedTables, string expectedSchemaOnlyViews, string expectedAllBusinessObjects, string expectedAllSolutionFileNames)
		{
			var buildXml = new BuildXml(buildXmlPath);
			AssertSubModulesData(buildXml, expectedExcludedTables, expectedSchemaOnlyViews, expectedAllBusinessObjects, expectedAllSolutionFileNames);
		}

		static void AssertSubModulesData(BuildXml buildXml, string expectedExcludedTables, string expectedSchemaOnlyViews, string expectedAllBusinessObjects, string expectedAllSolutionFileNames)
		{
			AssertMultilineASCIIEquals("ExcludedTables", expectedExcludedTables, string.Join("\r\n", buildXml.ExcludedTables));
			AssertMultilineASCIIEquals("SchemaOnlyViews", expectedSchemaOnlyViews, string.Join("\r\n", buildXml.SchemaOnlyViews));
			AssertMultilineASCIIEquals("AllBusinessObjects", expectedAllBusinessObjects, string.Join("\r\n", buildXml.AllBusinessObjects.Cast<BuildXmlBizOEntry>().Select(x => $"{x.TableName}-{x.SolutionName}").OrderBy(x => x)));
			AssertMultilineASCIIEquals("AllSolutionFileNames", expectedAllSolutionFileNames, string.Join("\r\n", buildXml.GetAllSolutionFileNames()));
		}

		public void TestTestInstance()
		{
			BuildXml buildXml = BuildXml.Instance;
			BuildXml testBuildXml = new BuildXml(testBuildXmlFile);

			BuildXml.SetInstanceForTesting(testBuildXml);

			Assert(buildXml != testBuildXml);
			Assert(buildXml != BuildXml.Instance);
			AssertEquals(testBuildXml, BuildXml.Instance);

			BuildXml.RemoveTestingInstance();

			Assert(buildXml != testBuildXml);
			AssertEquals(buildXml, BuildXml.Instance);
			Assert(testBuildXml != BuildXml.Instance);
		}

		[SnailTest, RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestAllBusinessObjectsExistInNominatedSolutions()
		{
			StringBuilder errors = new StringBuilder();
			foreach (BuildXmlBizOEntry bizOEntry in BuildXml.Instance.AllBusinessObjects)
			{
				string declaredTableName = bizOEntry.TableName;
				string tableName = declaredTableName.Replace("Schema.xml", "").Replace("DataSet.xsd", "");
				int lastIndex = tableName.LastIndexOf('.');
				if (lastIndex > -1)
				{
					tableName = tableName.Remove(0, lastIndex + 1);
				}
				string autoName = "Auto" + tableName;
				bool foundClass = false;
				string solutionName = bizOEntry.SolutionName + ".sln";
				List<string> assemblies = BuildXml.Instance.GetAssembliesDeployedInSolution(solutionName);
				foreach (string assembly in BuildXml.Instance.GetAssembliesTestedInSolution(solutionName))
				{
					if (!assemblies.Contains(assembly))
					{
						assemblies.Add(assembly);
					}
				}
				foreach (string assemblyName in assemblies)
				{
					Assembly assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assemblyName));
					Type[] types;
					try
					{
						types = assembly.GetTypes();
					}
					catch (ReflectionTypeLoadException ex)
					{
						types = ex.Types.Where(t => t != null).ToArray();
					}

					foreach (Type type in types)
					{
						if (type.Name == autoName)
						{
							foundClass = true;
							break;
						}
					}
					if (foundClass)
					{
						break;
					}
				}
				if (!foundClass)
				{
					errors.AppendLine(string.Format("Looked for BizO {0} in solution {1} and failed. Name declared in build.xml: {2}.", autoName, bizOEntry.SolutionName, declaredTableName));
				}
			}
			if (errors.Length > 0)
			{
				Fail("Incorrectly located classes. Remember that the filenames of XML schemas in build.xml used to defined business objects must end with 'Schema.xml' or 'DataSet.xsd', e.g. MyClassSchema.xml, not MyClass.xml.\r\n\r\n" + errors.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestGetAllTestedAssemblies()
		{
			string[] dllNames = testBuildXml.GetAllTestedAssemblies();
			AssertEquals("Count", 16, dllNames.Length);

			AssertEquals("BuildTools.dll", dllNames[0]);
			AssertEquals("Resources.dll", dllNames[1]);
			AssertEquals("Enterprise.VirtualChannels.CitrixClient.dll", dllNames[2]);
			AssertEquals("Enterprise.VirtualChannels.Client.dll", dllNames[3]);
			AssertEquals("Enterprise.VirtualChannels.Server.dll", dllNames[4]);
			AssertEquals("RemotePrinting.Engine.dll", dllNames[5]);
			AssertEquals("RemotePrinting.Types.dll", dllNames[6]);
			AssertEquals("Enterprise.DbUpgrader.Resource.dll", dllNames[7]);
			AssertEquals("Enterprise.DbUpgrader.Resource.Version.dll", dllNames[8]);
			AssertEquals("BusinessObjectGenerator.dll", dllNames[9]);
			AssertEquals("ZClientWOW.dll", dllNames[10]);
			AssertEquals("ZClientROH.dll", dllNames[11]);
			AssertEquals("ZClientWebEDI.dll", dllNames[12]);
			AssertEquals("Enterprise.Tracking.Business.dll", dllNames[13]);
			AssertEquals("Enterprise.Tracking.Web.dll", dllNames[14]);
			AssertEquals("Enterprise.Tracking.Module.dll", dllNames[15]);
			AssertCollectionNotContains("ShouldOnlyBeInOtherFilesToSign", dllNames);
		}

		public void TestUsingOtherBuildXml()
		{
			Assert("Assembies deploy to client count", BuildXml.Instance.GetAllAssembliesDeployedToClient(tempDirectory).Length > 20);

			try
			{
				var testXml = new BuildXml("BadFile");
				Fail("FileNotFoundException expected.");
			}
			catch (FileNotFoundException)
			{
			}
		}

		public void TestMasterFileReference()
		{
			foreach (BuildXmlBizOEntry entry in testBuildXml.AllBusinessObjects)
			{
				AssertEquals(entry.TableName.Equals("DummyDependentBizo"), entry.MasterFileReference);
			}
		}

		public void TestAllBusinessObjects()
		{
			BuildXmlBizOEntryCollection allBusinessObjects = testBuildXml.AllBusinessObjects;

			AssertEquals("Count", 13, allBusinessObjects.Count);
			AssertEquals("AccStatement", allBusinessObjects["AccStatement"].TableName);
			AssertEquals("Accounting", allBusinessObjects["AccStatement"].SolutionName);
			AssertEquals("AddInfoDataSet.xsd", allBusinessObjects["AddInfoDataSet.xsd"].TableName);
			AssertEquals("AUCustoms", allBusinessObjects["AddInfoDataSet.xsd"].SolutionName);
			AssertEquals("AssayCodeDataSet.xsd", allBusinessObjects["AssayCodeDataSet.xsd"].TableName);
			AssertEquals("AUCustoms", allBusinessObjects["AssayCodeDataSet.xsd"].SolutionName);
			AssertEquals("DummyBizo", allBusinessObjects["DummyBizo"].TableName);
			AssertEquals("ZArchitecture", allBusinessObjects["DummyBizo"].SolutionName);
			AssertEquals("DummyDependentBizo", allBusinessObjects["DummyDependentBizo"].TableName);
			AssertEquals("ZArchitecture", allBusinessObjects["DummyDependentBizo"].SolutionName);
			AssertEquals("AccBankAccount", allBusinessObjects["AccBankAccount"].TableName);
			AssertEquals("MasterFiles", allBusinessObjects["AccBankAccount"].SolutionName);
			AssertEquals("AccChargeCode", allBusinessObjects["AccChargeCode"].TableName);
			AssertEquals("MasterFiles", allBusinessObjects["AccChargeCode"].SolutionName);
			AssertEquals("AUCAHECC", allBusinessObjects["AUCAHECC"].TableName);
			AssertEquals("AUCustoms", allBusinessObjects["AUCAHECC"].SolutionName);
			AssertEquals("AUCChapter", allBusinessObjects["AUCChapter"].TableName);
			AssertEquals("AUCustoms", allBusinessObjects["AUCChapter"].SolutionName);
			AssertEquals(true, allBusinessObjects["AUCChapter"].ConvertZStringToWesternEuropeanCharacters);
			AssertEquals("MasterFiles", allBusinessObjects["vw_UNDGAttribute"].SolutionName);
			Assert(!allBusinessObjects["AccStatement"].PreventDelete);
			Assert(allBusinessObjects["AccBankAccount"].PreventDelete);
		}

		public void TestAllBusinessObjectsWithBusinessObjectsXmlAndExistingNodesInBuildXml_ThrowsInvalidOperationException()
		{
			retriever.SaveResourceToFile("TestBusinessObjects.xml", BuildConstants.BusinessObjectsXmlFileName);

			AssertExceptionThrown<InvalidOperationException>(() => _ = testBuildXml.AllBusinessObjects);
		}

		public void TestAllBusinessObjectsWithBusinessObjectsXml()
		{
			var testBuildXmlWithoutBusinessObjectsFile = retriever.SaveResourceToFile("TestBuildWithoutBusinessObjects.xml");
			var testBuildXmlWithoutBusinessObjects = new BuildXml(testBuildXmlWithoutBusinessObjectsFile);
			tempDirectory = Path.GetDirectoryName(testBuildXmlWithoutBusinessObjectsFile);
			retriever.SaveResourceToFile("TestBusinessObjects.xml", BuildConstants.BusinessObjectsXmlFileName);

			BuildXmlBizOEntryCollection allBusinessObjects = testBuildXmlWithoutBusinessObjects.AllBusinessObjects;

			AssertEquals("Count", 13, allBusinessObjects.Count);
			AssertEquals("AccStatement_FromBusinessObjectsXml", allBusinessObjects["AccStatement_FromBusinessObjectsXml"].TableName);
			AssertEquals("Accounting", allBusinessObjects["AccStatement_FromBusinessObjectsXml"].SolutionName);
			AssertEquals("AddInfoDataSet.xsd", allBusinessObjects["AddInfoDataSet.xsd"].TableName);
			AssertEquals("AUCustoms", allBusinessObjects["AddInfoDataSet.xsd"].SolutionName);
			AssertEquals("AssayCodeDataSet.xsd", allBusinessObjects["AssayCodeDataSet.xsd"].TableName);
			AssertEquals("AUCustoms", allBusinessObjects["AssayCodeDataSet.xsd"].SolutionName);
			AssertEquals("DummyBizo", allBusinessObjects["DummyBizo"].TableName);
			AssertEquals("ZArchitecture", allBusinessObjects["DummyBizo"].SolutionName);
			AssertEquals("DummyDependentBizo", allBusinessObjects["DummyDependentBizo"].TableName);
			AssertEquals("ZArchitecture", allBusinessObjects["DummyDependentBizo"].SolutionName);
			AssertEquals("AccBankAccount", allBusinessObjects["AccBankAccount"].TableName);
			AssertEquals("MasterFiles", allBusinessObjects["AccBankAccount"].SolutionName);
			AssertEquals("AccChargeCode", allBusinessObjects["AccChargeCode"].TableName);
			AssertEquals("MasterFiles", allBusinessObjects["AccChargeCode"].SolutionName);
			AssertEquals("AUCAHECC", allBusinessObjects["AUCAHECC"].TableName);
			AssertEquals("AUCustoms", allBusinessObjects["AUCAHECC"].SolutionName);
			AssertEquals("AUCChapter", allBusinessObjects["AUCChapter"].TableName);
			AssertEquals("AUCustoms", allBusinessObjects["AUCChapter"].SolutionName);
			AssertEquals(true, allBusinessObjects["AUCChapter"].ConvertZStringToWesternEuropeanCharacters);
			AssertEquals("MasterFiles", allBusinessObjects["vw_UNDGAttribute"].SolutionName);
			Assert(!allBusinessObjects["AccStatement_FromBusinessObjectsXml"].PreventDelete);
			Assert(allBusinessObjects["AccBankAccount"].PreventDelete);
		}

		public void TestAllBusinessObjectsWithAddInfo()
		{
			var allBusinessObjects = testBuildXml.AllBusinessObjects;

			var jobDeclarationBizOEntry = allBusinessObjects["JobDeclaration"];
			AssertNotNull(jobDeclarationBizOEntry);
			var addInfoEntries = jobDeclarationBizOEntry.AddInfoEntries;
			AssertEquals("Count", 2, addInfoEntries.Count);
			AssertEquals("View", "BRJobDeclaration", addInfoEntries.First().ViewName);
			AssertEquals("Solution", "BR", addInfoEntries.First().SolutionName);
			AssertEquals("View", "ZAJobDeclaration", addInfoEntries.Last().ViewName);
			AssertEquals("Solution", "ZA", addInfoEntries.Last().SolutionName);
		}
		public void TestAllBusinessObjectsWithAddInfoWithBusinessObjectsXml()
		{
			var testBuildXmlWithoutBusinessObjectsFile = retriever.SaveResourceToFile("TestBuildWithoutBusinessObjects.xml");
			var testBuildXmlWithoutBusinessObjects = new BuildXml(testBuildXmlWithoutBusinessObjectsFile);
			tempDirectory = Path.GetDirectoryName(testBuildXmlWithoutBusinessObjectsFile);
			retriever.SaveResourceToFile("TestBusinessObjects.xml", BuildConstants.BusinessObjectsXmlFileName);

			var allBusinessObjects = testBuildXmlWithoutBusinessObjects.AllBusinessObjects;

			var jobDeclarationBizOEntry = allBusinessObjects["JobDeclaration"];
			AssertNotNull(jobDeclarationBizOEntry);
			var addInfoEntries = jobDeclarationBizOEntry.AddInfoEntries;
			AssertEquals("Count", 2, addInfoEntries.Count);
			AssertEquals("View", "BRJobDeclaration", addInfoEntries.First().ViewName);
			AssertEquals("Solution", "BR", addInfoEntries.First().SolutionName);
			AssertEquals("View", "ZAJobDeclaration", addInfoEntries.Last().ViewName);
			AssertEquals("Solution", "ZA", addInfoEntries.Last().SolutionName);
		}

		public void TestGetAllSolutionFileNames()
		{
			string[] allSolutionFileNames = testBuildXml.GetAllSolutionFileNames();

			AssertEquals("Count", 12, allSolutionFileNames.Length);
			AssertEquals(@"NUnit\NUnit.sln", allSolutionFileNames[0]);
			AssertEquals(@"BuildTools\BuildTools.sln", allSolutionFileNames[1]);
			AssertEquals(@"Resources\Resources.sln", allSolutionFileNames[2]);
			AssertEquals(@"VirtualChannels\VirtualChannels.sln", allSolutionFileNames[3]);
			AssertEquals(@"RemotePrinting\Engine\RemotePrinting.Engine.sln", allSolutionFileNames[4]);
			AssertEquals(@"DbUpgrader\Resource\DbUpgrader.Resource.sln", allSolutionFileNames[5]);
			AssertEquals(@"BuildTools\MyCheckOuts\MyCheckOuts.sln", allSolutionFileNames[6]);
			AssertEquals(@"BusinessObjectGenerator\BusinessObjectGenerator.sln", allSolutionFileNames[7]);
			AssertEquals(@"Clients\WOW\ZClientWOW\ZClientWOW.sln", allSolutionFileNames[8]);
			AssertEquals(@"Clients\ROH\ZClientROH\ZClientROH.sln", allSolutionFileNames[9]);
			AssertEquals(@"Clients\EDI\ZClientWebEDI\ZClientWebEDI.sln", allSolutionFileNames[10]);
			AssertEquals(@"Tracking\Tracking.sln", allSolutionFileNames[11]);
		}

		public void TestGetAllSolutionFileNamesWithSolutionsXmlAndBuildXmlHasSolutions_ThrowException()
		{
			retriever.SaveResourceToFile("TestSolutions.xml", BuildConstants.SolutionXmlFileName);
			AssertExceptionThrown<InvalidOperationException>(() => _ = testBuildXml.GetAllSolutionFileNames());
		}

		public void TestGetSniffedSolutionFileNames()
		{
			string[] sniffedSolutionFileNames = testBuildXml.GetSniffedSolutionFileNames();
			AssertEquals("Count", 9, sniffedSolutionFileNames.Length);
			AssertEquals(@"BuildTools\BuildTools.sln", sniffedSolutionFileNames[0]);
			AssertEquals(@"Resources\Resources.sln", sniffedSolutionFileNames[1]);
			AssertEquals(@"VirtualChannels\VirtualChannels.sln", sniffedSolutionFileNames[2]);
			AssertEquals(@"RemotePrinting\Engine\RemotePrinting.Engine.sln", sniffedSolutionFileNames[3]);
			AssertEquals(@"DbUpgrader\Resource\DbUpgrader.Resource.sln", sniffedSolutionFileNames[4]);
			AssertEquals(@"Clients\WOW\ZClientWOW\ZClientWOW.sln", sniffedSolutionFileNames[5]);
			AssertEquals(@"Clients\ROH\ZClientROH\ZClientROH.sln", sniffedSolutionFileNames[6]);
			AssertEquals(@"Clients\EDI\ZClientWebEDI\ZClientWebEDI.sln", sniffedSolutionFileNames[7]);
			AssertEquals(@"Tracking\Tracking.sln", sniffedSolutionFileNames[8]);
		}

		public void TestGetSolutionFileNamesNotIncludingClientSpecific()
		{
			string[] nonClientSpecificSolutionFileNames = testBuildXml.GetSolutionFileNamesNotIncludingClientSpecific();
			AssertEquals("Count", 9, nonClientSpecificSolutionFileNames.Length);
			AssertEquals(@"NUnit\NUnit.sln", nonClientSpecificSolutionFileNames[0]);
			AssertEquals(@"BuildTools\BuildTools.sln", nonClientSpecificSolutionFileNames[1]);
			AssertEquals(@"Resources\Resources.sln", nonClientSpecificSolutionFileNames[2]);
			AssertEquals(@"VirtualChannels\VirtualChannels.sln", nonClientSpecificSolutionFileNames[3]);
			AssertEquals(@"RemotePrinting\Engine\RemotePrinting.Engine.sln", nonClientSpecificSolutionFileNames[4]);
			AssertEquals(@"DbUpgrader\Resource\DbUpgrader.Resource.sln", nonClientSpecificSolutionFileNames[5]);
			AssertEquals(@"BuildTools\MyCheckOuts\MyCheckOuts.sln", nonClientSpecificSolutionFileNames[6]);
			AssertEquals(@"BusinessObjectGenerator\BusinessObjectGenerator.sln", nonClientSpecificSolutionFileNames[7]);
			AssertEquals(@"Tracking\Tracking.sln", nonClientSpecificSolutionFileNames[8]);
		}

		public void TestGetFileNameOfSolution()
		{
			AssertEquals(nameOfThisSolution + ".sln", Path.GetFileName(FileNameOfThisSolution));
		}

		public void TestGetFileNameOfSolution_Null()
		{
			AssertEquals("", testBuildXml.GetFileNameOfSolution(null));
		}

		public void TestAssembliesTestedInSolution()
		{
			string[] assemblies = testBuildXml.GetAssembliesTestedInSolution(FileNameOfThisSolution);
			AssertEquals("Count", 1, assemblies.Length);
			AssertEquals("Assembly filename", nameOfThisAssembly, assemblies[0]);
			AssertCollectionNotContains("ShouldOnlyBeInOtherFilesToSign", assemblies);
		}

		public void TestAssembliesTestedInSolutionNull()
		{
			string[] assemblies = testBuildXml.GetAssembliesTestedInSolution(null);
			if (assemblies.Length != 16)
			{
				Fail("The following assemblies were returned:" + Environment.NewLine + string.Join(Environment.NewLine, assemblies));
			}

			AssertEquals("BuildTools.dll", assemblies[0]);
			AssertEquals("Resources.dll", assemblies[1]);
			AssertEquals("Enterprise.VirtualChannels.CitrixClient.dll", assemblies[2]);
			AssertEquals("Enterprise.VirtualChannels.Client.dll", assemblies[3]);
			AssertEquals("Enterprise.VirtualChannels.Server.dll", assemblies[4]);
			AssertEquals("RemotePrinting.Engine.dll", assemblies[5]);
			AssertEquals("RemotePrinting.Types.dll", assemblies[6]);
			AssertEquals("Enterprise.DbUpgrader.Resource.dll", assemblies[7]);
			AssertEquals("Enterprise.DbUpgrader.Resource.Version.dll", assemblies[8]);
			AssertEquals("BusinessObjectGenerator.dll", assemblies[9]);
			AssertEquals("ZClientWOW.dll", assemblies[10]);
			AssertEquals("ZClientROH.dll", assemblies[11]);
			AssertEquals("ZClientWebEDI.dll", assemblies[12]);
			AssertEquals("Enterprise.Tracking.Business.dll", assemblies[13]);
			AssertEquals("Enterprise.Tracking.Web.dll", assemblies[14]);
			AssertEquals("Enterprise.Tracking.Module.dll", assemblies[15]);
			AssertCollectionNotContains("ShouldOnlyBeInOtherFilesToSign", assemblies);
		}

		public void TestAssembliesTestedWithSolutionXmlWithBuildXmlHasSolutions_ThrowException()
		{
			retriever.SaveResourceToFile("TestSolutions.xml", BuildConstants.SolutionXmlFileName);

			AssertExceptionThrown<InvalidOperationException>(() =>
				_ = testBuildXml.GetAssembliesTestedInSolution(null));
		}

		public void TestGetAllAssembliesDeployedToClientIncludesDependencyFiles()
		{
			var result = testBuildXml.GetAllAssembliesDeployedToClient(tempDirectory);
			AssertCollectionContains("DependencyToDeploy1.dll", result);
			AssertCollectionContains("DependencyToDeploy2.dll", result);
			AssertCollectionContains("DependencyToDeploy3.dll", result);
			AssertCollectionNotContains("DependencyDontDeploy.dll", result);
		}

		public void TestGetDependencies()
		{
			var result = testBuildXml.GetDependencies();
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					new KeyValuePair<string, string>("http://devops/repo1", "$/Whatever/Bar"),
					new KeyValuePair<string, string>("https://devops/repo2", "$/Whatever/Foo"),
					new KeyValuePair<string, string>("http://devops/repo3", string.Empty),
				},
				result);
		}

		public void TestLoadingFromSolutionsXml()
		{
			var buildXmlContent = @"
				<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"" MSBuild=""17"">
				</Build>";

			var buildXmlPath = Path.Combine(testTempDir, BuildConstants.BuildXmlFileName);
			File.WriteAllText(buildXmlPath, buildXmlContent);

			var solutionsXmlContent = @"
				<Build>
					<Solutions>
					  <Solution Filename=""TestSolution1.sln"">
					    <Bin>TestAssembly1.dll</Bin>
					    <Bin>TestAssembly2.dll</Bin>
					  </Solution>
					  <Solution Filename=""TestSolution2.sln"">
					    <Bin>TestAssembly3.dll</Bin>
					  </Solution>
					</Solutions>
				</Build>";

			var solutionsXmlPath = Path.Combine(testTempDir, BuildConstants.SolutionXmlFileName);
			File.WriteAllText(solutionsXmlPath, solutionsXmlContent);

			var buildXml = new BuildXml(buildXmlPath);

			var solutions = buildXml.GetAllSolutionFileNames();
			AssertEquals("Should have 2 solutions", 2, solutions.Length);
			AssertCollectionContains("TestSolution1.sln", solutions);
			AssertCollectionContains("TestSolution2.sln", solutions);

			var assembliesInSolution1 = buildXml.GetAllAssembliesInSolution("TestSolution1.sln");
			AssertEquals("Solution 1 should have 2 assemblies", 2, assembliesInSolution1.Count);
			AssertCollectionContains("TestAssembly1.dll", assembliesInSolution1);
			AssertCollectionContains("TestAssembly2.dll", assembliesInSolution1);
		}

		public void TestLoadingFromBusinessObjectsXml()
		{
			var buildXmlContent = @"
	            <Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"" MSBuild=""17.0"">
	            </Build>";

			var buildXmlPath = Path.Combine(testTempDir, BuildConstants.BuildXmlFileName);
			File.WriteAllText(buildXmlPath, buildXmlContent);

			var businessObjectsXmlContent = @"
	            <Generator>
	              <BusinessObjects>
	                <BusinessObject Table=""TestTable"" Solution=""TestSolution"" PreventDelete=""false"">
	                  <AddInfo View=""TestView"" Solution=""TestViewSolution"" />
	                  <AddInfo View=""SecondView"" Solution=""SecondSolution"" ParentSchema=""ParentTest"" />
	                </BusinessObject>
	              </BusinessObjects>
	              <BackingColumns>
	                <BackingColumn Column=""TestColumn"" SourceTable=""TestSourceTable"" SourceColumn=""TestSourceColumn"" />
	                <BackingColumn Column=""AnotherColumn"" SourceTable=""AnotherSourceTable"" SourceColumn=""AnotherSourceColumn"" />
	              </BackingColumns>
	              <ExcludedTables>
	                <ExcludedTable Table=""ExcludedTable1"" />
	                <ExcludedTable Table=""ExcludedTable2"" />
	              </ExcludedTables>
	              <SchemaOnlyEntities>
	                <SchemaOnly View=""SchemaView1"" />
	                <SchemaOnly View=""SchemaView2"" />
	              </SchemaOnlyEntities>
	              <ExcludedColumns>
	                <ExcludedColumn Column=""ExcludedColumn1"" />
	                <ExcludedColumn Column=""ExcludedColumn2"" />
	              </ExcludedColumns>
	            </Generator>";

			var businessObjectsXmlPath = Path.Combine(testTempDir, BuildConstants.BusinessObjectsXmlFileName);
			File.WriteAllText(businessObjectsXmlPath, businessObjectsXmlContent);

			var buildXml = new BuildXml(buildXmlPath);

			var businessObjects = buildXml.AllBusinessObjects;
			AssertEquals("Should have 1 business object", 1, businessObjects.Count);

			var businessObj = businessObjects["TestTable"];
			AssertNotNull("Business object should exist", businessObj);
			AssertEquals("TestTable", businessObj.TableName);
			AssertEquals("TestSolution", businessObj.SolutionName);
			Assert("PreventDelete should be false", !businessObj.PreventDelete);

			var addInfos = businessObj.AddInfoEntries;
			AssertEquals("Should have 2 AddInfo entries", 2, addInfos.Count);

			var firstAddInfo = addInfos.First();
			AssertEquals("TestView", firstAddInfo.ViewName);
			AssertEquals("TestViewSolution", firstAddInfo.SolutionName);

			var secondAddInfo = addInfos.Last();
			AssertEquals("SecondView", secondAddInfo.ViewName);
			AssertEquals("SecondSolution", secondAddInfo.SolutionName);
			AssertEquals("ParentTest", secondAddInfo.ParentSchema);

			var backingColumns = buildXml.BackingColumns;
			AssertEquals("Should have 2 backing columns", 2, backingColumns.Count);

			var (sourceTable, sourceColumn) = backingColumns["TestColumn"];
			AssertEquals("TestSourceTable", sourceTable);
			AssertEquals("TestSourceColumn", sourceColumn);

			(sourceTable, sourceColumn) = backingColumns["AnotherColumn"];
			AssertEquals("AnotherSourceTable", sourceTable);
			AssertEquals("AnotherSourceColumn", sourceColumn);

			var excludedTables = buildXml.ExcludedTables.ToArray();
			AssertEquals("Should have 2 excluded tables", 2, excludedTables.Length);
			AssertCollectionContains("ExcludedTable1", excludedTables);
			AssertCollectionContains("ExcludedTable2", excludedTables);

			var schemaOnlyViews = buildXml.SchemaOnlyViews.ToArray();
			AssertEquals("Should have 2 schema only views", 2, schemaOnlyViews.Length);
			AssertCollectionContains("SchemaView1", schemaOnlyViews);
			AssertCollectionContains("SchemaView2", schemaOnlyViews);

			var excludedColumns = buildXml.ExcludedColumns.ToArray();
			AssertEquals("Should have 2 excluded columns", 2, excludedColumns.Length);
			AssertCollectionContains("ExcludedColumn1", excludedColumns);
			AssertCollectionContains("ExcludedColumn2", excludedColumns);
		}

		public void TestExistingNodesAreOverwritten()
		{
			var buildXmlContent = @"
		        <Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"" MSBuild=""17.0"">
		          <Solutions>
		            <Solution Filename=""ExistingSolution.sln"">
		              <Bin>ExistingAssembly.dll</Bin>
		            </Solution>
		          </Solutions>
		          <BusinessObjects>
		            <BusinessObject Table=""ExistingTable"" Solution=""ExistingSolution"">
		              <AddInfo View=""ExistingView"" Solution=""ExistingSolutionView"" />
		            </BusinessObject>
		          </BusinessObjects>
		          <BackingColumns>
		            <BackingColumn Column=""ExistingColumn"" SourceTable=""ExistingSourceTable"" SourceColumn=""ExistingSourceColumn"" />
		          </BackingColumns>
		          <ExcludedTables>
		            <ExcludedTable Table=""ExistingExcludedTable"" />
		          </ExcludedTables>
		          <SchemaOnlyEntities>
		            <SchemaOnly View=""ExistingSchemaView"" />
		          </SchemaOnlyEntities>
		          <ExcludedColumns>
		            <ExcludedColumn Column=""ExistingExcludedColumn"" />
		          </ExcludedColumns>
		        </Build>";

			var buildXmlPath = Path.Combine(testTempDir, BuildConstants.BuildXmlFileName);
			File.WriteAllText(buildXmlPath, buildXmlContent);

			var solutionsXmlContent = @"
		        <Build>
		          <Solutions>
		            <Solution Filename=""NewSolution1.sln"">
		              <Bin>NewAssembly1.dll</Bin>
		            </Solution>
		          </Solutions>
		        </Build>";

			var solutionsXmlPath = Path.Combine(testTempDir, BuildConstants.SolutionXmlFileName);
			File.WriteAllText(solutionsXmlPath, solutionsXmlContent);

			var businessObjectsXmlContent = @"
		        <Generator>
		          <BusinessObjects>
		            <BusinessObject Table=""NewTable"" Solution=""NewSolution"">
		              <AddInfo View=""NewView"" Solution=""NewSolution"" />
		            </BusinessObject>
		          </BusinessObjects>
		          <BackingColumns>
		            <BackingColumn Column=""NewColumn"" SourceTable=""NewSourceTable"" SourceColumn=""NewSourceColumn"" />
		          </BackingColumns>
		          <ExcludedTables>
		            <ExcludedTable Table=""NewExcludedTable"" />
		          </ExcludedTables>
		          <SchemaOnlyEntities>
		            <SchemaOnly View=""NewSchemaView"" />
		          </SchemaOnlyEntities>
		          <ExcludedColumns>
		            <ExcludedColumn Column=""NewExcludedColumn"" />
		          </ExcludedColumns>
		        </Generator>";

			var businessObjectsXmlPath = Path.Combine(testTempDir, BuildConstants.BusinessObjectsXmlFileName);
			File.WriteAllText(businessObjectsXmlPath, businessObjectsXmlContent);

			var buildXml = new BuildXml(buildXmlPath);

			AssertExceptionThrown<InvalidOperationException>(() => _ = buildXml.GetAllSolutionFileNames());
		}

		protected override void SetUp()
		{
			base.SetUp();
			retriever = new EmbeddedResourceRetriever();
			testBuildXmlFile = retriever.SaveResourceToFile("TestBuild.xml");
			testBuildXml = new BuildXml(testBuildXmlFile);
			tempDirectory = Path.GetDirectoryName(testBuildXmlFile);
			_ = SaveEmbeddedResourceToSubdirectory(Path.Combine(tempDirectory, "Testing"), "Test.link");
			schemaSaveResultFile = SaveEmbeddedResourceToSubdirectory(Path.Combine(tempDirectory, "Testing"), "SchemaSaveResult.xml");
			testSubFolderFile = SaveEmbeddedResourceToSubdirectory(Path.Combine(tempDirectory, "Testing", "TestSubFolder"), "TestSubFolderFile.xml");

			testTempDir = Path.Combine(tempDirectory, "BuildXmlTest_" + Guid.NewGuid());
			Directory.CreateDirectory(testTempDir);
		}

		protected override void TearDown()
		{
			try
			{
				if (testTempDir != null && Directory.Exists(testTempDir))
				{
					Directory.Delete(testTempDir, true);
				}
			}
			finally
			{
				base.TearDown();
				retriever.Dispose();
			}
		}

		BuildXml testBuildXml;
		EmbeddedResourceRetriever retriever;
		string tempDirectory;
		string testBuildXmlFile;
		string schemaSaveResultFile;
		string testSubFolderFile;
		const string nameOfThisSolution = "BuildTools";
		const string nameOfThisAssembly = "BuildTools.dll";
		string testTempDir;

		string FileNameOfThisSolution
		{
			get { return testBuildXml.GetFileNameOfSolution(nameOfThisSolution); }
		}

		string SaveEmbeddedResourceToSubdirectory(string subdirPath, string resourceName)
		{
			if (!Directory.Exists(subdirPath))
			{
				Directory.CreateDirectory(subdirPath);
			}
			var filePath = Path.Combine(subdirPath, resourceName);
			using (var fileStream = File.Create(filePath))
			{
				retriever.GetStream(resourceName).CopyTo(fileStream);
			}
			return filePath;
		}
	}
}
