using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class XmlMistakesTest : TestCase
	{
		public void TestBusinessObjectNamesAreValid()
		{
			const string pattern = @"^\w+(|\.xsd|(\.\w+)*)$";
			Regex validNamesRegex = new Regex(pattern, RegexOptions.IgnoreCase);

			foreach (BuildXmlBizOEntry entry in testBuildXml.AllBusinessObjects)
			{
				AssertXml(entry.TableName, validNamesRegex.IsMatch(entry.TableName));
			}
		}

		public void TestBusinessObjectsHaveOnlyOneValidValue()
		{
			foreach (BuildXmlBizOEntry entry in testBuildXml.AllBusinessObjects)
			{
				AssertXml(entry.TableName + "=\"" + entry.SolutionName + "\"", Regex.IsMatch(entry.SolutionName, @"^[a-z0-9_.]+$", RegexOptions.IgnoreCase));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSolutionFilesForBusinessObjectExist()
		{
			foreach (BuildXmlBizOEntry entry in testBuildXml.AllBusinessObjects)
			{
				string solutionFileName = Path.Combine(BaseSourcePath, testBuildXml.GetFileNameOfSolution(entry.SolutionName));
				AssertFileExistsInBaseSourcePath(entry.TableName + "=\"" + entry.SolutionName + "\"", solutionFileName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSolutionFilesExist()
		{
			foreach (string solutionFileName in testBuildXml.GetAllSolutionFileNames())
			{
				AssertFileExistsInBaseSourcePath(solutionFileName, Path.Combine(BaseSourcePath, solutionFileName));
			}
		}

		public void TestAssemblyFilesExist()
		{
			foreach (string solutionFileName in testBuildXml.GetSolutionFileNamesNotIncludingClientSpecific())
			{
				foreach (string assemblyFileName in testBuildXml.GetAssembliesTestedInSolution(solutionFileName))
				{
					AssertFileExists(assemblyFileName + " in " + solutionFileName, Path.Combine(Path.GetDirectoryName(testBuildXml.BuildXmlFileName)!, assemblyFileName));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestXmlSerializerAssembliesPresentInBuildXml()
		{
			var assembliesDeployedToClients = testBuildXml.GetAllAssemblies(true, BaseSourcePath);
			foreach (string assembly in assembliesDeployedToClients)
			{
				var assemblyFileName = Path.Combine(Path.GetDirectoryName(testBuildXml.BuildXmlFileName)!, assembly);
				if (File.Exists(assemblyFileName))
				{
					var pattern = Path.GetFileNameWithoutExtension(assemblyFileName) + ".*.XmlSerializers.dll";
					var serializers = Directory.GetFiles(Path.GetDirectoryName(testBuildXml.BuildXmlFileName)!, pattern, SearchOption.TopDirectoryOnly);
					foreach (var serializer in serializers)
					{
						var serializerName = Path.GetFileName(serializer);
						string[] suppressions = { "Enterprise.Registry.Business.Test.XmlSerializers.dll" };
						if ((!assembliesDeployedToClients.Contains(serializerName)) && (!suppressions.Contains(serializerName)))
						{
							Fail("Assembly " + serializerName + " must be specified in build.xml.");
						}
					}
				}
			}
			Assert(true);
		}

		public void TestExesDeployedToClientsHaveConfigFiles()
		{
			//Arrange
			var exeExceptionList = new[]
			{
					"AppDomainWrappers.Net48.exe",
					"AppDomainWrappers.Net8.exe",
					"CargoWise.Setup.exe",
					"CargoWiseOne.exe",
					"CargoWiseOneAnyCpu.exe",
					"CargoWise.ServiceManager.Next.Launcher.exe",
					"CargoWise.ServiceManager.Next.Runner.exe",
					"CargoWiseOne.WebInfrastructure.Updater.exe",
					"CargoWise.CitrixServices.Upgrader.exe",
					"CargoWise.RemoteDesktopServices.Upgrader.exe",
					"CargoWiseOneWebServerSetup.exe",
					"CopyDocumentXmls.exe",
					"ediLogShippingSetup.exe",
					"Enterprise.exe",
					"Enterprise.LocalTransport.Mobile.Client.exe",
					"Enterprise.MasterFiles.Services.KeysGenerator.exe",
					"Enterprise.Upgrades.Postinstall4.0.exe",
					"Enterprise.Upgrades.Preinstall4.0.exe",
					"Enterprise.Warehouse.RF.exe",
					"EW.RF.Load.exe",
					"handle.exe",
					"LocalTransport.Mobile.Client.Load.exe",
					"CargoWise.Data.SqlProxy.Server.exe",
				};

			var configExceptionList = new[]
			{
					"Enterprise.DbBackupAndRestore.log4net.config",
				};

			var notDeployToClientsFiles = BuildXml.Instance.GetNotDeployToClientsFiles().ToArray();

			var exes = FileNameHelpers
				.GetExeFileNames(notDeployToClientsFiles)
				.Except(exeExceptionList, StringComparer.OrdinalIgnoreCase);

			var configs = FileNameHelpers
				.GetFileNames(new string[] { ".config" }, notDeployToClientsFiles)
				.Except(configExceptionList, StringComparer.OrdinalIgnoreCase)
				.Select(Path.GetFileNameWithoutExtension);

			// Assert
			AssertContainsExactElementsInAnyOrder(StringComparer.OrdinalIgnoreCase, exes, configs);
		}

		readonly BuildXml testBuildXml = new BuildXml();

		void AssertXml(string context, bool condition)
		{
			Assert("There seems to be a mistake in the Build.xml file with " + context, condition);
		}

		void AssertFileExists(string message, string fileName)
		{
			Assert(message + " [" + fileName + "]", File.Exists(fileName));
		}

		void AssertFileExistsInBaseSourcePath(string message, string relativeFilePath)
		{
			AssertFileExists(message, Path.Combine(BaseSourcePath, relativeFilePath));
		}
	}
}
