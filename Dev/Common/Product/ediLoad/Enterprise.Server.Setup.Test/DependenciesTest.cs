using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	public class DependenciesTest : TestCase
	{
		public void TestDependenciesAreMerged()
		{
			// Act
			var references = GetReferences(MergedExeName);

			// Assert
			AssertContainsExactElementsInAnyOrder($"Dependencies should be merged into {MergedExeName}", Array.Empty<string>(), references.Except(AllowedNotMergedDependencies));
		}

		IEnumerable<string> GetReferences(string assemblyRelativeFileName)
		{
#if NETCOREAPP
			var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyRelativeFileName);

			Assembly asm;
			var resolver = new PathAssemblyResolver([assemblyPath]);
			using (var mlc = new MetadataLoadContext(resolver))
			{
				asm = mlc.LoadFromAssemblyPath(assemblyPath);
			}
#else
			var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyRelativeFileName);
			var asm = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
#endif

			return asm.GetReferencedAssemblies().Select(r => r.Name);
		}

		const string MergedExeName = "CargoWiseServerSetup.exe";

		static readonly string[] AllowedNotMergedDependencies =
		{
			"CargoWise.ApplicationManager.Common",
			"CargoWise.Async",
			"CargoWise.Data.HttpClient",
			"CargoWise.Data.SqlProxy.Interface",
			"CargoWise.Data.Providers.Common",
			"CargoWise.Data.Shared",
			"CargoWise.Database.Abstractions",
			"CargoWise.Database.Shared",
			"CargoWise.DataLink",
			"CargoWise.DataProtection",
			"CargoWise.DataProtection.SqlExtensions",
			"CargoWise.Definitions",
			"CargoWise.Schema",
			"CargoWise.ResourceStrings",
			"Castle.Core",
			"Enterprise.ZArchitecture.Core",
			"Microsoft.Extensions.DependencyInjection",
			"Microsoft.Extensions.DependencyInjection.Abstractions",
			"Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling",
			"Microsoft.SqlServer.Types",
			"Mono.Cecil",
			"Moq",
			"mscorlib",
			"System",
			"System.Buffers",
			"System.Collections.Immutable",
			"System.Core",
			"System.Data",
			"System.Data.DataSetExtensions",
			"System.DirectoryServices",
			"System.DirectoryServices.AccountManagement",
			"System.DirectoryServices.Protocols",
			"System.Drawing",
			"System.Management",
			"System.Memory",
			"System.Numerics",
			"System.Numerics.Vectors",
			"System.Reflection.Metadata",
			"System.Runtime.Caching",
			"System.Runtime.CompilerServices.Unsafe",
			"System.Runtime.Serialization",
			"System.Runtime.Serialization.Formatters.Soap",
			"System.ServiceModel",
			"System.ServiceModel.Web",
			"System.ServiceProcess",
			"System.Text.Encodings.Web",
			"System.Text.Json",
			"System.Threading.Tasks.Extensions",
			"System.Web",
			"System.Web.Services",
			"System.Windows.Forms",
			"System.Xml",
			"System.Xml.Linq",
			"VsnetUrl",
			"WTG.StaticAnalysis.Annotation",
			"Newtonsoft.Json",
			"WTG.Authenticode",
			"WTG.DevTools.Definitions", // Transitive dependency of NUnitCore.dll - not needed in production
			"WTG.DevTools.ServiceClient.Submissions", // Transitive dependency of NUnitCore.dll - not needed in production
		};
	}
}
