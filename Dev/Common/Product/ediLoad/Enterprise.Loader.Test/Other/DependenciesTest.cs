using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	public class DependenciesTest : TestCase
	{
		public void TestCW1DependenciesAreMerged()
		{
			// Act
			var references = GetReferences(CW1MergedExeName);

			// Assert
			AssertContainsExactElementsInAnyOrder($"Dependencies should be IL merged into {CW1MergedExeName}, unless it is a system assembly in which case it can be added to {nameof(AllowedReferences)} array", Array.Empty<string>(), references.Except(AllowedReferences));
		}

		public void TestCWDependenciesAreMerged()
		{
			// Act
			var references = GetReferences(CWMergedExeName);

			// Assert
			AssertContainsExactElementsInAnyOrder($"Dependencies should be IL merged into {CWMergedExeName}, unless it is a system assembly in which case it can be added to {nameof(AllowedReferences)} array", Array.Empty<string>(), references.Except(AllowedReferences));
		}

		IEnumerable<string> GetReferences(string assemblyRelativeFileName)
		{
			var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyRelativeFileName);
			var asm = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
			return asm.GetReferencedAssemblies().Select(r => r.Name);
		}

		const string CW1MergedExeName = "CargoWiseOne.Start.exe";

		const string CWMergedExeName = "CargoWise.Start.exe";

		static readonly string[] AllowedReferences =
		{
			"CargoWise.ApplicationManager.Common",
			"CargoWise.Database.Abstractions",
			"CargoWise.DataLink",
			"CargoWise.Interop",
			"CargoWise.ResourceStrings",
			"Enterprise.RemoteDesktopServices.Client",
			"Enterprise.RemoteDesktopServices.Shared",
			"Enterprise.URLHandler.Integration",
			"Microsoft.Extensions.Configuration",
			"Microsoft.Extensions.Configuration.Abstractions",
			"Microsoft.Extensions.Configuration.Binder",
			"Microsoft.Extensions.Configuration.FileExtensions",
			"Microsoft.Extensions.Configuration.Json",
			"Microsoft.Extensions.DependencyInjection.Abstractions",
			"Microsoft.Extensions.FileProviders.Abstractions",
			"Microsoft.Extensions.FileProviders.Physical",
			"Microsoft.Extensions.FileSystemGlobbing",
			"Microsoft.Extensions.Logging",
			"Microsoft.Extensions.Logging.Abstractions",
			"Microsoft.Extensions.Logging.Configuration",
			"Microsoft.Extensions.Options",
			"Microsoft.Extensions.Options.ConfigurationExtensions",
			"Microsoft.Extensions.Primitives",
			"mscorlib",
			"NLog",
			"NLog.Extensions.Logging",
			"OpenTelemetry",
			"OpenTelemetry.Api",
			"OpenTelemetry.Api.ProviderBuilderExtensions",
			"Outlook",
			"System",
			"System.Buffers",
			"System.ComponentModel.DataAnnotations",
			"System.Configuration",
			"System.Core",
			"System.Data",
			"System.Diagnostics.DiagnosticSource",
			"System.DirectoryServices",
			"System.DirectoryServices.AccountManagement",
			"System.DirectoryServices.Protocols",
			"System.Drawing",
			"System.IO.Compression",
			"System.Management",
			"System.Memory",
			"System.Net.Http",
			"System.Numerics",
			"System.Reflection.Metadata",
			"System.Runtime.Caching",
			"System.Runtime.Serialization",
			"System.Security",
			"System.ServiceModel",
			"System.ServiceProcess",
			"System.Threading.Tasks.Extensions",
			"System.Web",
			"System.Windows.Forms",
			"System.Xml",
			"System.Xml.Linq",
			"WindowsBase",
			"WTG.ApplicationLogging",
			"WTG.ApplicationLogging.Abstractions",
			"WTG.StaticAnalysis.Annotation",
		};
	}
}
