#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.BuildTools;
using Enterprise.ZArchitecture.Core.Test;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	public abstract class BaseWebConfigTopLevelOnlyAssembliesTest : BaseWebConfigTest
	{
		public void TestCompilationHasRemoveAllAssembliesNode()
		{
			var config = new XmlDocument();
			config.Load(DebugFilePath);
			var assembliesSection = config.DocumentElement.SelectSingleNode(DebugCompilationAssembliesXPath);

			string removeAllAssembliesNode = null;

			if (assembliesSection != null)
			{
				removeAllAssembliesNode = assembliesSection
					.ChildNodes
					.OfType<XmlElement>()
					.Where(x => x.Name == "remove")
					.Select(x => x.GetAttribute("assembly"))
					.SingleOrDefault();
			}

			AssertEquals($"The compilation section should have <remove assembly='*' /> in {DebugFilePath}", "*", removeAllAssembliesNode);
		}

		public void TestCompilationAddedAssembliesDeployedToClients()
		{
			var config = new XmlDocument();
			config.Load(DebugFilePath);

			HashSet<string> addedAssemblies;

			var assembliesSection = config.DocumentElement.SelectSingleNode(DebugCompilationAssembliesXPath);

			if (assembliesSection == null)
			{
				addedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				addedAssemblies = assembliesSection
					.ChildNodes
					.OfType<XmlElement>()
					.Where(x => x.Name == "add")
					.Select(x => x.GetAttribute("assembly"))
					.Where(x => !x.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
					.ToHashSet(StringComparer.OrdinalIgnoreCase);
			}

			var deployedAssemblies = AssemblyHelper.GetDistinctAssemblyNames(BuildXml.Instance.GetAllAssembliesDeployedToClient(BaseSourcePath));
			var notDeployedAssemblies = addedAssemblies.Where(a => !deployedAssemblies.Contains(a));

			AssertSequencesEqual($"The following compilation assemblies in {DebugFilePath} should be deployed to clients:", Enumerable.Empty<string>(), notDeployedAssemblies);
		}

		public void TestExpectedCompilationAddedAssemblies()
		{
			var config = new XmlDocument();
			config.Load(DebugFilePath);

			HashSet<string> addedAssemblies;

			var assembliesSection = config.DocumentElement.SelectSingleNode(DebugCompilationAssembliesXPath);

			if (assembliesSection == null)
			{
				addedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				addedAssemblies = assembliesSection
					.ChildNodes
					.OfType<XmlElement>()
					.Where(x => x.Name == "add")
					.Select(x => x.GetAttribute("assembly"))
					.ToHashSet(StringComparer.OrdinalIgnoreCase);
			}

			var expectedAddedAssemblies = GetExpectedAddedAssembliesForCompilation();
			var missingAssemblies = expectedAddedAssemblies.Where(e => !addedAssemblies.Contains(e));

			AssertNotEquals($"Provide the list of expected compilation assemblies for {DebugFilePath}", 0, expectedAddedAssemblies.Count());
			AssertSequencesEqual($"The following compilation assemblies are missing from {DebugFilePath}", Enumerable.Empty<string>(), missingAssemblies);
		}

		protected abstract IEnumerable<string> GetExpectedAddedAssembliesForCompilation();
	}

	public abstract class BaseWebConfigRemovedAssembliesTest : BaseWebConfigTest
	{
		public void TestDebugRemoveAssembliesNotDeployed()
		{
			var config = new XmlDocument();
			config.Load(DebugFilePath);
			var assembliesSection = config.DocumentElement.SelectSingleNode(DebugCompilationAssembliesXPath);
			var removedAssemblies = assembliesSection.ChildNodes.OfType<XmlElement>().Where(x => x.Name == "remove").Select(x => x.GetAttribute("assembly"));

			var deployedAssemblies = AssemblyHelper.GetDistinctAssemblyNames(BuildXml.Instance.GetAllAssembliesDeployedToClient(BaseSourcePath));
			var notDeployedAssemblies = AssemblyHelper.GetDistinctAssemblyNames(BuildXml.Instance.GetNotDeployToClientsFiles());

			var missingRemoveElements = new List<string>();
			foreach (var assembly in notDeployedAssemblies.Where(x => !deployedAssemblies.Contains(x)))
			{
				if (!removedAssemblies.Any(x => x == assembly))
				{
					missingRemoveElements.Add($"<remove assembly=\"{assembly}\"/>");
				}
			}

			var incorrectRemoves = new List<string>();
			foreach (var assembly in deployedAssemblies)
			{
				if (removedAssemblies.Any(x => x == assembly))
				{
					incorrectRemoves.Add(assembly);
				}
			}

			CombineAssertions(() =>
			{
				AssertEquals($"The following elements should be added to {DebugFilePath}\r\n" + string.Join("\r\n", missingRemoveElements), 0, missingRemoveElements.Count);
				AssertEquals($"The following assemblies should not have remove elements in {DebugFilePath}\r\n" + string.Join("\r\n", incorrectRemoves), 0, incorrectRemoves.Count);
			});
		}
	}

	public abstract class BaseWebConfigTest : ConfigFileBindingRedirectTest
	{
		public void TestHttpRuntime_UpToDateOrOmitted()
		{
			var exisitingAssemblyBindings = new HashSet<string>();

			var config = new XmlDocument();
			config.Load(DebugFilePath);
			var httpRuntime = config.SelectSingleNode("//httpRuntime");

			var targetFrameworkAttribute = httpRuntime?.Attributes["targetFramework"];
			if (targetFrameworkAttribute != null)
			{
				AssertEquals("targetFramework attribute of httpRuntime element should be omitted or up to date.", "4.8", targetFrameworkAttribute.Value);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestTargetFrameworksMatch()
		{
			var config = new XmlDocument();
			config.Load(DebugFilePath);

			var httpRuntime = config.SelectSingleNode("//httpRuntime");
			var compilation = config.SelectSingleNode("//compilation");

			var httpRuntimeTargetFrameworkAttribute = httpRuntime?.Attributes["targetFramework"];
			var compilationTargetFrameworkAttribute = compilation?.Attributes["targetFramework"];
			if (httpRuntimeTargetFrameworkAttribute != null && compilationTargetFrameworkAttribute != null)
			{
				AssertEquals("targetFramework attributes of httpRuntime and compilation elements should be omitted or match.", compilationTargetFrameworkAttribute.Value, httpRuntimeTargetFrameworkAttribute.Value);
			}
			else
			{
				Assert(true);
			}
		}

		protected abstract string DebugCompilationAssembliesXPath { get; }
	}
}
#endif
