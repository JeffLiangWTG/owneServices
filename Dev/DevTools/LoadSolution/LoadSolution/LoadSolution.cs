using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LoadSolution
{
	public class LoadSolution
	{
		const string BUILDXMLNAME = "Build.xml";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "folder name, will never be translated")]
		const string BINFOLDER = "bin";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "will never be translated")]
		const string DEFAULTASSEMBLYEXTENSION = "dll";
		const string GITFILEORDIRECTORY = ".git";
		const string CSPROJEXTENSION = ".csproj";
		string rootFolder;

		string methodSignature;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "logging output, will never be translated, keyword")]
		public int Run(string filename, int lineNumber)
		{
			var directoryOfFile = Path.GetDirectoryName(filename);
			rootFolder = LocateRootFolderOfBuildXML(directoryOfFile);

			// extract dll name from Metadata cs if we don't have real file name
			var dllFullName = new Lazy<string>(() => ExtractDLLNameFromMetadataCS(filename, lineNumber));
			if (rootFolder == null)
			{
				if (string.IsNullOrEmpty(dllFullName.Value))
				{
					Console.WriteLine("could not find DLL name in generated file, give up.");
					return -1;
				}

				directoryOfFile = Path.GetDirectoryName(dllFullName.Value);
				rootFolder = LocateRootFolderOfBuildXML(directoryOfFile);

				if (rootFolder == null)
				{
					Console.WriteLine($"{BUILDXMLNAME} not found in {directoryOfFile} and upwards");
					return -1;
				}
			}

			if (filename.StartsWith(rootFolder, StringComparison.OrdinalIgnoreCase))
			{
				// real file, not generated from Metadata
				Console.WriteLine($"no DLL metadata, using source file {filename}");
				StartVS(filename, lineNumber);
			}
			else
			{
				var dllNameWithExtension = Path.GetFileName(dllFullName.Value);

				// load build.xml and find the project producing assembly
				var csProjFolder = LocateSolutionForDLLInBuildXML(dllNameWithExtension);
				if (string.IsNullOrEmpty(csProjFolder))
				{
					Console.WriteLine($"could not find solution for dll {dllNameWithExtension} in Build.XML, give up.");
					return -1;
				}

				// extract .csproj from solution
				var sourceFileName = Path.GetFileName(filename);
				var csFileToStart = Directory.GetFiles(csProjFolder, sourceFileName, SearchOption.AllDirectories).FirstOrDefault() ?? LocateFileFromClassName(csProjFolder, "class " + Path.GetFileNameWithoutExtension(sourceFileName));
				if (!string.IsNullOrEmpty(csFileToStart))
				{
					var lineInSource = FindSignatureInSource(csFileToStart, lineNumber);
					StartVS(csFileToStart, lineInSource);
				}
			}

			return 0;
		}

		string LocateFileFromClassName(string csProjFolder, string classSignature)
		{
			var result = string.Empty;
			var allFiles = Directory.GetFiles(csProjFolder, "*.cs", SearchOption.AllDirectories);
			foreach (var csfile in allFiles)
			{
				var contents = File.ReadAllLines(csfile);
				string located = contents.FirstOrDefault(l => l.Contains(classSignature));
				if (located != null)
				{
					result = csfile;
					break;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "only used on developer workstations")]
		void StartVS(string csFileToStart, int lineNumber)
		{
			Console.WriteLine($"Starting VS for file {csFileToStart}, line {lineNumber}");
			Process.Start($"vsnet:\"{csFileToStart}#{lineNumber}\"");
		}

		string LocateSolutionForDLLInBuildXML(string dllName)
		{
			var buildXMLFullName = Path.Combine(rootFolder, BUILDXMLNAME);
			var result = FindSolutionFromBuildXML(buildXMLFullName, dllName);

			if (string.IsNullOrEmpty(result))
			{
				// not found - iterate over all build.xml in git-repo
				var gitRoot = LocateRootFolderOfRepository(rootFolder);
				var buildXMLsInRepo = Directory.EnumerateFiles(gitRoot, BUILDXMLNAME, SearchOption.AllDirectories);
				foreach (var buildXMLToTry in buildXMLsInRepo)
				{
					if (!string.Equals(buildXMLFullName, buildXMLToTry, StringComparison.OrdinalIgnoreCase) && buildXMLToTry.IndexOf(BINFOLDER, StringComparison.OrdinalIgnoreCase) == -1)
					{
						result = FindSolutionFromBuildXML(buildXMLToTry, dllName);
						if (!string.IsNullOrEmpty(result))
						{
							break;
						}
					}
				}
			}

			if (string.IsNullOrEmpty(result))
			{
				var dependency = FindDependencyFromBuildXML(buildXMLFullName, dllName);
				if (dependency != null)
				{
					Console.WriteLine($"Dependency: {dependency}, try to open in browser");
#pragma warning disable CW1078 // Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.
					Process.Start(dependency);
#pragma warning restore CW1078 // Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML element names, XML attribute, XML element name")]
		string FindSolutionFromBuildXML(string buildXMLFullName, string dllName)
		{
			var result = string.Empty;
			var buildXMLFolder = Path.GetDirectoryName(buildXMLFullName);
			var buildXML = XDocument.Load(buildXMLFullName);
			var buildXMLNamespace = buildXML.Root.Name.Namespace;
			var binNode = buildXML.Root.Element(buildXMLNamespace + "Solutions")?.Descendants(buildXMLNamespace + "Bin")?.FirstOrDefault(n => n.Value.Equals(dllName, StringComparison.OrdinalIgnoreCase));
			if (binNode != null)
			{
				var xsolutionFullName = Path.Combine(buildXMLFolder, binNode.Parent.Attribute("Filename").Value);
				result = GetFolderContainingProjectFromSolution(xsolutionFullName);
			}
			else
			{
				// bin not found, check all solution
				var solutions = buildXML.Root.Element(buildXMLNamespace + "Solutions")?.Elements(buildXMLNamespace + "Solution").Select(n => n.Attribute("Filename").Value);
				if (solutions != null)
				{
					foreach (var solutionName in solutions)
					{
						var xsolutionFullName = Path.Combine(buildXMLFolder, solutionName);
						result = GetFolderContainingProjectFromSolution(xsolutionFullName);
						if (!string.IsNullOrEmpty(result))
						{
							break;
						}
					}
				}
			}
			return result;

			string GetFolderContainingProjectFromSolution(string solution)
			{
				var csProjFolder = string.Empty;
				var solutionFileName = Path.Combine(buildXMLFolder, solution);
				var projectsInSolution = File.ReadAllLines(solutionFileName).Where(l => l.StartsWith("Project")).Select(l => l.Split(',')[1].Replace("\"", "").Trim()).Where(s => s.Contains(CSPROJEXTENSION));

				foreach (var projectFileName in projectsInSolution)
				{
					// peek into project file for AssemblyName
					var csProjFullname = Path.Combine(Path.GetDirectoryName(solutionFileName), projectFileName);
					var assemblyProducedByProject = ExtractAssemblyNameProducedByProject(csProjFullname);
					if (dllName.Equals(assemblyProducedByProject, StringComparison.OrdinalIgnoreCase))
					{
						csProjFolder = Path.GetDirectoryName(csProjFullname);
						break;
					}
				}

				return csProjFolder;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML element names")]
		string FindDependencyFromBuildXML(string buildXMLFullName, string dllName)
		{
			var result = string.Empty;
			var buildXMLFolder = Path.GetDirectoryName(buildXMLFullName);
			var buildXML = XDocument.Load(buildXMLFullName);
			var buildXMLNamespace = buildXML.Root.Name.Namespace;

			var dependency = buildXML.Root.Element(buildXMLNamespace + "Dependencies")?.Elements(buildXMLNamespace + "Dependency")?.Elements(buildXMLNamespace + "Copy")
				.Select(n => new
				{
					source = n.Attribute("Source").Value,
					repository = n.Parent.Attribute("Repository")?.Value,
					path = n.Parent.Attribute("Path")?.Value,
				})
				.FirstOrDefault(n => n.source.EndsWith(dllName, StringComparison.OrdinalIgnoreCase));

			if (dependency != null)
			{
				// for now, just report where the dependency is. If we change to packages, the process will change anyway
				result = $"{dependency.repository}?path={dependency.path}";
			}

			return result;
		}

		int FindSignatureInSource(string sourceFileName, int lineNumber)
		{
			var result = lineNumber;
			var source = File.ReadAllLines(sourceFileName);
			var sourceLength = source.Length;
			if (lineNumber > sourceLength || string.IsNullOrEmpty(methodSignature))
			{
				result = 1;
			}
			else
			{
				for (var i = 0; i < sourceLength; i++)
				{
					if (source[i].Contains(methodSignature))
					{
						result = i + 1;
						break;
					}
				}
			}

			return result;
		}

		string ExtractAssemblyNameProducedByProject(string csProjFullname)
		{
			var projectXML = XDocument.Load(csProjFullname);
			var nsProj = projectXML.Root.Name.Namespace;
			var assemblyName = projectXML.Descendants(nsProj + "AssemblyName").FirstOrDefault()?.Value ?? Path.GetFileNameWithoutExtension(csProjFullname);
			var assemblyExtension = projectXML.Descendants(nsProj + "OutputType").FirstOrDefault()?.Value ?? DEFAULTASSEMBLYEXTENSION;
			return assemblyName + "." + assemblyExtension;
		}

		string ExtractDLLNameFromMetadataCS(string filename, int lineNumber)
		{
			var sourceLines = File.ReadAllLines(filename);
			var firstCommentLine = sourceLines.FirstOrDefault(l => l.StartsWith("//"));
			var dllName = firstCommentLine?.Substring(3) ?? string.Empty;
			if (!string.IsNullOrEmpty(dllName))
			{
				ExtractMethodSignature(sourceLines, lineNumber);
			}
			return dllName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "keywords")]
		void ExtractMethodSignature(string[] sourceLines, int lineNumber)
		{
			var sourceLinesLength = sourceLines.Length;
			if (lineNumber <= sourceLinesLength)
			{
				var sourceToLocate = sourceLines[lineNumber - 1];
				// cut of property signature at the end, we won't find it in the original source
				var posCurly = sourceToLocate.IndexOf('{');
				if (posCurly != -1)
				{
					sourceToLocate = sourceToLocate.Substring(0, posCurly);
				}
				// cut of inherited classes, interfaces as they might be formatted differently in source
				var posColon = sourceToLocate.IndexOf(':');
				if (posColon != -1 && (sourceToLocate.Contains(" class ") || sourceToLocate.Contains(" interface ")))
				{
					sourceToLocate = sourceToLocate.Substring(0, posColon);
				}
				methodSignature = sourceToLocate.Trim('\t', ' ', ';');
			}
		}

		string LocateRootFolderOfBuildXML(string directoryOfFile)
		{
			var result = directoryOfFile;
			while (result != null && (new DirectoryInfo(result).Name.Equals(BINFOLDER, StringComparison.OrdinalIgnoreCase) || !File.Exists(Path.Combine(result, BUILDXMLNAME))))
			{
				result = Path.GetDirectoryName(result);
			}
			return result;
		}

		string LocateRootFolderOfRepository(string directoryOfFile)
		{
			var result = directoryOfFile;
			while (result != null && !File.Exists(Path.Combine(result, GITFILEORDIRECTORY)) && !Directory.Exists(Path.Combine(result, GITFILEORDIRECTORY)))
			{
				result = Path.GetDirectoryName(result);
			}
			return result;
		}
	}
}
