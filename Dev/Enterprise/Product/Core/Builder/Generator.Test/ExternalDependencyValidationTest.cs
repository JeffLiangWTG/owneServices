 using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Test
{
	class ExternalDependencyValidationTest : TestCase
	{
		// WARNING WARNING WARNING
		//From now on code in Builder must not reference any Code in other areas of DEV.
		//References to the whitelisted dependencies are being removed. Do NOT add more references!
		//For more information contact Olivia Stewart or Brett Shearer.
		//WARNING WARNING WARNING
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDependenciesAreValid()
		{
			var assemblies = GetProjectsFromSolution("a");
			var rawDependencies = GetSolutionCombinedDependencies(assemblies);
			var culledAssemblies = GetDisallowedAssemblies(rawDependencies, assemblies);

			Assert(FailedDependencies(culledAssemblies), culledAssemblies.Count == 0);
		}

		static string FailedDependencies(List<string> failedDependencies)
		{
			var sb = new StringBuilder();
			failedDependencies.ForEach(x => sb.AppendLine(x));
			return $"There should not be dependencies on: \n{sb}";
		}

		static List<string> GetDisallowedAssemblies(List<string> rawDependencies, List<string> rawAssemblies)
		{
			var disallowed = new List<string>();
			var whitelistedDependencies = WhitelistedDependencies;
			foreach (var dependency in rawDependencies)
			{
				var header = dependency.Split('.')[0];
				var allowed = rawAssemblies.Contains(dependency)
					|| systemAssemblyNamePrefixes.Contains(header)
					|| whitelistedDependencies.Contains(dependency);
				if (!allowed)
				{
					disallowed.Add(dependency);
				}
			}
			return disallowed;
		}

		static List<string> GetSolutionCombinedDependencies(List<string> projectPaths)
		{
			var dependencies = new HashSet<string>();
			foreach (var projectPath in projectPaths)
			{
				var combinedDependencies = GetIndividualProjectDependencies(projectPath);
				combinedDependencies.ForEach(x => dependencies.Add(x));
			}
			return dependencies.ToList();
		}

		static List<string> GetIndividualProjectDependencies(string projectPath)
		{
			var projectXml = new XmlDocument();
			projectXml.Load(Path.Combine(BaseSourcePath, LocalSolutionPath, projectPath));
			var projectElement = projectXml.DocumentElement;
			var namespaceManager = new XmlNamespaceManager(projectXml.NameTable);
			namespaceManager.AddNamespace("p", projectElement.NamespaceURI);
			return projectElement
				.SelectNodes("//p:Reference", namespaceManager)
				.Cast<XmlElement>()
				.Select(x => x.SelectSingleNode("p:HintPath", namespaceManager)?.InnerText)
				.Where(x => x != null)
				.Select(x => Path.GetFileName(x).ToUpperInvariant())
				.ToList();
		}

		static List<string> GetProjectsFromSolution(string assemblyPath)
		{
			using (var reader = File.OpenText(Path.Combine(BaseSourcePath, LocalSolutionPath, "Builder.sln")))
			{
				return new Regex(@"\,\s\""(?<main>(.*?))\\(?<secondary>(.*?)\.csproj)").Matches(reader.ReadToEnd()).OfType<Match>()
					.Select(x => $"{x.Groups["main"].Value}/{x.Groups["secondary"].Value}").ToList();
			}
		}

		const string LocalSolutionPath = "Enterprise\\Product\\Core\\Builder";
		static readonly HashSet<string> systemAssemblyNamePrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "YAMLDOTNET", "NUNITCORE", "System", "Microsoft", "Presentation", "Windows", "mscorlib", "EnvDTE", "EnvDTE80", "UIAutomationProvider", "Accessibility", "CustomMarshalers", "Azure" };
		static readonly HashSet<string> WhitelistedDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
"BUILDTOOLS.DLL",
"CARGOWISE.APPLICATIONCONTEXT.DLL",
"CARGOWISE.COMMON.DLL",
"CARGOWISE.CUSTOMSADDINFOSCHEMA.DLL",
"CARGOWISE.DATAPROTECTION.DLL",
"CARGOWISE.DATAPROTECTION.ADMINISTRATION.DLL",
"CARGOWISE.DATAPROTECTION.ADMINISTRATION.SQLSERVER.DLL",
"CARGOWISE.DATAPROTECTION.SQLEXTENSIONS.DLL",
"CARGOWISE.DATA.PROVIDERS.COMMON.DLL",
"CARGOWISE.DATA.SHARED.DLL",
"CARGOWISE.DATABASE.ABSTRACTIONS.DLL",
"CARGOWISE.DATABASE.SHARED.DLL",
"CARGOWISE.DBUPGRADER.FOUNDATION.DLL",
"CARGOWISE.DBUPGRADER.SCHEMA.SYNCHRONISERS.DLL",
"CARGOWISE.DBUPGRADER.SCRIPTS.ABSTRACTIONS.DLL",
"CARGOWISE.DBUPGRADER.SCRIPTS.DEFINITIONS.DLL",
"CARGOWISE.SCHEMA.DLL",
"CARGOWISE.SHARED.40.DLL",
"CARGOWISE.WINDOWS.UI.DLL",
"ENTERPRISE.BUILD.DATABASE.SCRIPT.DLL",
"ENTERPRISE.DBUPGRADER.ASSEMBLIES.DLL",
"ENTERPRISE.DBUPGRADER.REFERENCEDATABASES.DLL",
"ENTERPRISE.DBUPGRADER.RESOURCE.VERSION.DLL",
"ENTERPRISE.DBUPGRADER.SCHEMA.TEMPLATE.DLL",
"ENTERPRISE.DBUPGRADER.SCRIPT.DLL",
"ENTERPRISE.INTEGRATION.DLL",
"ENTERPRISE.ZARCHITECTURE.CORE.DLL",
"CARGOWISE.DATA.DLL",
"CARGOWISE.DATA.TEST.DLL",
"ENTERPRISE.DBUPGRADER.DATA.DOCUMENTS.DLL",
"ENTERPRISE.DBUPGRADER.DATA.SHARED.DLL",
"ENTERPRISE.DBUPGRADER.DATA.STARTUP.DLL",
"ENTERPRISE.DBUPGRADER.DATA.STARTUP.TEST.DLL",
"ENTERPRISE.DBUPGRADER.DATA.STMEVENT.DLL",
"ENTERPRISE.DBUPGRADER.SHARED.DLL",
"ENTERPRISE.ZARCHITECTURE.GUI.DLL",
"ENTERPRISE.ZARCHITECTURE.MODULES.DLL",
"NUNITCORE.DLL",
"RESOURCES.DLL",
"WTG.DEVTOOLS.DEFINITIONS.DLL",
"WTG.STATICANALYSIS.ANNOTATION.DLL",
"BUILDTOOLS.TEST.DLL",
"CARGOWISE.BI.DEVELOPMENT.AUTOMATION.DLL",
"CARGOWISE.BI.DEVELOPMENT.COMMON.DLL",
"ENTERPRISE.DBUPGRADER.DATA.SHARED.TEST.DLL",
"CARGOWISE.ENTITYFRAMEWORK.DLL",
"CARGOWISE.IO.DLL",
"ENTERPRISE.DATATRANSFER.NATIVE.DB.DLL",
"CARGOWISE.GLOW.MODEL.VALIDATOR.DLL",
"CARGOWISE.GLOW.MODEL.CW.VALIDATOR.DLL",
"CARGOWISE.STATICANALYSIS.DLL",
"CARGOWISE.ODYSSEY.SCHEMA.GENERATEDRESOURCES.DLL"
		};
	}
}
