using System;
using System.IO;
using CargoWise.Common;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class NGenTest : TestCase
	{
		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestMSBuildVersionNotEmpty()
		{
			AssertNotNullOrEmpty(BuildXml.Instance.GetMSBuildVersion());
		}

		public void TestNGenCompliesWithDeployToClientsSolutionBinFiles()
		{
			var buildXmlContent = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<Solutions>
		<Solution Filename=""Database\Generic\Data\CargoWise.Data.sln"">
			<Bin>CargoWise.Data.dll</Bin>
			<Bin DeployToClients=""false"">CargoWise.Data.Test.dll</Bin>
		</Solution>
	</Solutions>
</Build>
";
			using (DisposablTestBuildXml(buildXmlContent, out var buildXmlPath))
			{
				var buildXml = new BuildXml(buildXmlPath);

				AssertEquals(true, buildXml.NGen("CargoWise.Data.dll"));
				AssertEquals(false, buildXml.NGen("CargoWise.Data.Test.dll"));
			}
		}

		public void TestNGenOverridesDeployToClientsSolutionBinFiles()
		{
			var buildXmlContent = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<Solutions>
		<Solution Filename=""Database\Generic\Data\CargoWise.Data.sln"">
			<Bin DeployToClients=""true"" NGen=""false"">Enterprise.Build.Database.Script.dll</Bin>
			<Bin DeployToClients=""false"" NGen=""true"">Enterprise.Build.Database.Script.Test.dll</Bin>
		</Solution>
	</Solutions>
</Build>
";
			using (DisposablTestBuildXml(buildXmlContent, out var buildXmlPath))
			{
				var buildXml = new BuildXml(buildXmlPath);

				AssertEquals(false, buildXml.NGen("Enterprise.Build.Database.Script.dll"));
				AssertEquals(true, buildXml.NGen("Enterprise.Build.Database.Script.Test.dll"));
			}
		}

		public void TestNGenCompliesWithDeployToClientsCopyDependencies()
		{
			var buildXmlContent = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<Dependencies>
		<Dependency Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared"" Path=""/ActiveDirectory"">
			<Copy Source=""net46\CargoWise.ActiveDirectory.dll""/>
			<Copy DeployToClients=""false"" Source=""net46\CargoWise.ActiveDirectory.TestFramework.dll""/>
			<Copy Source=""net472\CargoWise.Database.TestFramework.dll"" DeployToClients=""false""/>
			<Copy Source=""netstandard2.0\CargoWise.Glow.Model.CW.Validator.dll"" DeployToClients=""false""/>
			<Copy Source=""netstandard2.0\CargoWise.Glow.Model.Validator.dll"" DeployToClients=""false""/>
		</Dependency>
		<Dependency Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared"" Path=""/PAVE.Common"">
			<Copy DeployToClients=""false"" Source=""CargoWise.PAVE.Common.TestUtils.dll""/>
		</Dependency>
		<Dependency Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared"">
			<Copy DeployToClients=""false"" Source=""Common\CargoWise.RefDbRepo.Client.Common.TestHelper.dll""/>
		</Dependency>
	</Dependencies>
</Build>
";
			using (DisposablTestBuildXml(buildXmlContent, out var buildXmlPath))
			{
				var buildXml = new BuildXml(buildXmlPath);

				CombineAssertions(() =>
				{
					foreach (var fileName in new[]
					{
							"CargoWise.ActiveDirectory.dll",
					})
					{
						AssertEquals(fileName, true, buildXml.NGen(fileName));
					}

					foreach (var fileName in new[]
					{
							"CargoWise.ActiveDirectory.TestFramework.dll",
							"CargoWise.Database.TestFramework.dll",
							"CargoWise.Glow.Model.CW.Validator.dll",
							"CargoWise.Glow.Model.Validator.dll",
							"CargoWise.PAVE.Common.TestUtils.dll",
							"CargoWise.RefDbRepo.Client.Common.TestHelper.dll",
					})
					{
						AssertEquals(false, buildXml.NGen(fileName));
					}
				});
			}
		}

		public void TestDeployToClientsCopyDependenciesIgnoresTargetSubfolder()
		{
			var buildXmlContent = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<Solution Filename=""Enterprise\Architecture\GUI\Enterprise.ZArchitecture.GUI.sln"">
		<Bin>Enterprise.ZArchitecture.GUI.dll</Bin>
	</Solution>
	<Dependencies>
		<Dependency Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/RemoteDesktopServices"">
			<Copy DeployToClients=""false"" Source=""Enterprise.ZArchitecture.GUI.dll"" Target=""ZArchitectureGUIStub"" />
		</Dependency>
	</Dependencies>
</Build>
";
			using (DisposablTestBuildXml(buildXmlContent, out var buildXmlPath))
			{
				var buildXml = new BuildXml(buildXmlPath);

				CombineAssertions(() =>
				{
					foreach (var fileName in new[]
					{
							"Enterprise.ZArchitecture.GUI.dll",
					})
					{
						AssertEquals(fileName, true, buildXml.NGen(fileName));
					}
				});
			}
		}

		public void TestNGenCompliesWithDeployToClientsOtherFiles()
		{
			var buildXmlContent = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<OtherFiles>
		<Filename DeployToClients=""true"" SignWithAuthenticode=""true"">CargoWise.Start.exe</Filename>
		<Filename DeployToClients=""false"" SignWithAuthenticode=""true"">CargoWiseServerSetup.exe</Filename>
		<Filename DeployToClients=""false"" CopyFrom=""packages\build\CargoWise.Analyzers\analyzers\dotnet\cs"">CargoWise.Analyzers.dll</Filename>
	</OtherFiles>
</Build>
";
			using (DisposablTestBuildXml(buildXmlContent, out var buildXmlPath))
			{
				var buildXml = new BuildXml(buildXmlPath);
				CombineAssertions(() =>
				{
					foreach (var fileName in new[]
					{
							"CargoWise.Start.exe",
					})
					{
						AssertEquals(fileName, true, buildXml.NGen(fileName));
					}

					foreach (var fileName in new[]
					{
							"CargoWiseServerSetup.exe",
							"CargoWise.Analyzers.dll",
					})
					{
						AssertEquals(fileName, false, buildXml.NGen(fileName));
					}
				});
			}
		}

		public void TestNGenExcludeOtherFilesDirectoryCopyNotDeployToClients()
		{
			var buildXmlContent = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<OtherFiles>
		<Directory DeployToClients=""false"" CopyFrom=""NotDeployToClients"">.</Directory>
		<Directory DeployToClients=""true"" CopyFrom=""DeployToClients"">.</Directory>
		<Directory CopyFrom=""NotDeployToClientsFromSubfolder"">Subfolder</Directory>
	</OtherFiles>
</Build>
";
			using (var temp = new TempDirectory())
			{
				var buildXmlPath = Path.Combine(temp, "test.xml");
				File.WriteAllText(buildXmlPath, buildXmlContent);

				foreach (var copyFrom in new[] { "NotDeployToClients", "DeployToClients", "NotDeployToClientsFromSubfolder" })
				{
					var packageDir = Path.Combine(temp, copyFrom);
					var sourceFilePath = Path.Combine(packageDir, $"{copyFrom}.dll");
					Directory.CreateDirectory(packageDir);
					File.WriteAllText(sourceFilePath, copyFrom);
				}

				var buildXml = new BuildXml(buildXmlPath);
				CombineAssertions(() =>
				{
					foreach (var fileName in new[]
					{
							"DeployToClients.dll",
					})
					{
						AssertEquals(fileName, true, buildXml.NGen(fileName));
					}

					foreach (var fileName in new[]
					{
							"NotDeployToClients.dll",
							"NotDeployToClientsFromSubfolder.dll",
					})
					{
						AssertEquals(fileName, false, buildXml.NGen(fileName));
					}
				});
			}
		}

		IDisposable DisposablTestBuildXml(string buildXmlContent, out string buildXmlFilePath)
		{
			var temp = new TempDirectory();
			buildXmlFilePath = Path.Combine(temp, "Build.xml");
			File.WriteAllText(buildXmlFilePath, buildXmlContent);

			return new DisposableAction(() =>
			{
				temp.Dispose();
			});
		}
	}
}
