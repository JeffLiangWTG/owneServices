using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using Microsoft.Extensions.FileSystemGlobbing;
using NUnit.Framework;
using BuildXml = CargoWise.BuildTools.BuildXml;

namespace Enterprise.ReflectionTest
{
	class ProjectSettingsTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCodeAnalysisIsNotConfigured()
		{
			CombineAssertions(() =>
				{
					foreach (var solution in BuildXml.CreateFromSourceCodeDirectory().GetAllSolutionFileNames())
					{
						TestCodeAnalysisIsNotConfiguredInSolution(solution);
					}
				});
		}

		void TestCodeAnalysisIsNotConfiguredInSolution(string solution)
		{
			string solutionPath = Path.Combine(BaseSourcePath, solution);
			using (var reader = File.OpenText(solutionPath))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("Project"))
					{
						string projectPath = line.Split(',')[1].Trim(' ', '\t', '"');
						TestCodeAnalysisIsNotConfiguredInProject(Path.Combine(Path.GetDirectoryName(solutionPath), projectPath));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllProjectsSdkFormat()
		{
			var exclusionFile = Path.Combine(BaseSourcePath, @"ProjectSafeguards\NonSdkProjectsBaseline.txt");
			var exclusionsFromFile = File.ReadAllLines(exclusionFile)
				.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("--"))
				.ToHashSet();

			var nonSdkProjects = new ConcurrentHashSet<string>();

			var matcher = new Matcher();
			matcher.AddInclude(@"**\*.csproj");
			matcher.AddExclude(@"**\.git\**\*");
			foreach (var exclusion in exclusionsFromFile)
			{
				matcher.AddExclude(exclusion);
			}

			var allProjects = matcher.GetResultsInFullPath(BaseSourcePath);

			Parallel.ForEach(allProjects, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (projectFile) =>
			{
				var xDocument = XDocument.Load(projectFile);
				var relativeProjectPath = GetRelativePath(BaseSourcePath, projectFile);
				var isSdkProject = xDocument.Root.Attribute("Sdk") != null;
				if (!isSdkProject)
				{
					nonSdkProjects.TryAdd(relativeProjectPath);
				}
			});

			var errorMessage = nonSdkProjects.Any()
				? $"Convert the following projects to SDK-style or, if conversion is not possible, add exclusion to '{GetRelativePath(BaseSourcePath, exclusionFile)}'"
				: null;
			CombineAssertions(errorMessage, () =>
			{
				foreach (var nonSdkProject in nonSdkProjects)
				{
					Assert($"'{nonSdkProject}' is not SDK-style project.", false);
				}

				Assert(true);
			});
		}

		static void TestCodeAnalysisIsNotConfiguredInProject(string projectPath)
		{
			if (projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
			{
				var projectXml = new XmlDocument();
				projectXml.Load(projectPath);
				var projectElement = projectXml.DocumentElement;
				var namespaceManager = new XmlNamespaceManager(projectXml.NameTable);
				namespaceManager.AddNamespace("p", projectElement.NamespaceURI);

				var propertyGroups = projectXml.SelectNodes("//p:PropertyGroup", namespaceManager).Cast<XmlElement>();
				Assert(projectPath + " has RunCodeAnalysis=true for one or more configurations. Turn off code analysis in the csproj file, analysis rules are run as unit tests.", !propertyGroups.Any(group => ChildText(group, "RunCodeAnalysis").Equals("true", StringComparison.OrdinalIgnoreCase)));

				var releaseProperties = propertyGroups.Where(group => group.GetAttribute("Condition").IndexOf("release|", StringComparison.OrdinalIgnoreCase) > 0);
				foreach (var properties in releaseProperties)
				{
					Assert(projectPath + " defines the CODE_ANALYSIS symbol on the RELEASE build. Remove the symbol to avoid [SuppressMessage] attributes from being compiled into release builds.", !properties.SelectNodes("p:DefineConstants", namespaceManager).Cast<XmlElement>().Any(o => o.InnerText != null && o.InnerText.IndexOf("CODE_ANALYSIS", StringComparison.OrdinalIgnoreCase) > -1));
				}
			}
		}

		static string GetRelativePath(string relativeTo, string path)
		{
			var basePathUri = new Uri(relativeTo);
			var pathUri = new Uri(path);
			var relativePath = Uri.UnescapeDataString(basePathUri.MakeRelativeUri(pathUri).ToString())
				.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			return relativePath;
		}

		static string ChildText(XmlElement element, string name)
		{
			var child = element[name];
			return child == null ? string.Empty : child.InnerText;
		}
	}
}
