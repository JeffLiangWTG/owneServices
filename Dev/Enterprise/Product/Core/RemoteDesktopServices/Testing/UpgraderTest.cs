using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Enterprise.RemoteDesktopServices.Server;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	sealed class UpgraderTest : TestCase
	{
		public void TestRightUpgraderExeNameIsUsedByStartupTask()
		{
			var binPath = Path.GetDirectoryName(typeof(UpgraderTest).Assembly.Location);
			AssertEquals("Citrix upgrader file exists", true, File.Exists(Path.Combine(binPath, UpgraderResources.CitrixUpgraderExeName)));
			AssertEquals("Rds upgrader file exists", true, File.Exists(Path.Combine(binPath, UpgraderResources.RdsUpgraderExeName)));
		}

		public void TestDependenciesAreMerged()
		{
			Test(UpgraderResources.CitrixUpgraderExeName, "CargoWise.CitrixServices.Upgrader.csproj");
			Test(UpgraderResources.RdsUpgraderExeName, "CargoWise.RemoteDesktopServices.Upgrader.csproj");

			void Test(string upgraderExeName, string projectName)
			{
				var references = GetReferences(upgraderExeName);
				AssertContainsExactElementsInAnyOrder(
					$@"All dependencies need to be merged into {upgraderExeName} since this single program will be transmitted to client side solely and the client side will fail to run the upgrader if one dependency is missing.

Please note that all the dependencies and children dependencies that are from external repo should be referenced by {projectName} explicitly, otherwise DAT build might run the project build and the dependency retrieval simultaneously.",
					Array.Empty<string>(),
					references.Except(NoMergeBaseline));
			}
		}

		string[] GetReferences(string assemblyRelativeFileName)
		{
			var currentAssemblyPath = typeof(UpgraderTest).Assembly.Location;
			var binPath = Path.GetDirectoryName(currentAssemblyPath);

			var resolver = new PathAssemblyResolver(new string[] { typeof(UpgraderTest).Assembly.Location, typeof(object).Assembly.Location });
			using var mlc = new MetadataLoadContext(resolver);

			var assembly = mlc.LoadFromAssemblyPath(Path.Combine(binPath, assemblyRelativeFileName));
			return assembly.GetReferencedAssemblies().Select(r => r.Name).ToArray();
		}

		// PLEASE DO NOT ADD MORE ITEMS INTO THIS LIST UNLESS YOU ARE SURE THAT THE ITEM SHOULD NOT BE MERGED INTO THE UPGRADER EXE.
		static readonly string[] NoMergeBaseline =
		[
			// .net48 runtime
			"mscorlib",
			"System",
			"System.Core",
			"System.DirectoryServices",
			"System.DirectoryServices.AccountManagement",
			"System.DirectoryServices.Protocols",
			"System.Drawing",
			"System.Net.Http",
			"System.ServiceProcess",
			"System.Text.Json",
			"System.Web",
			"System.Windows.Forms",
			"System.Xml",
			"WindowsBase",

			// They should be added into the ILMerge list in the first place. But it looks that the upgrader can succeed to run without them.
			// We should add references to them in .csproj file to make sure the project is built after the dependencies are retrieved by QGL.
			"CargoWise.ApplicationManager.Common",
			"Microsoft.Graph",
			"Microsoft.Graph.Core",
			"MimeKit",
			"Outlook",
			"WTG.Authenticode",
			"WTG.StaticAnalysis.Annotation",
		];
	}
}
