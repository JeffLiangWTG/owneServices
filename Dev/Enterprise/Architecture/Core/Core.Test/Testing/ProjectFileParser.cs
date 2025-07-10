using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ProjectFileParser
	{
		public KeyValuePair<string, string> GetProjectAndAssemblyName(string fileName)
		{
			var projectAndAssemblyName = new KeyValuePair<string, string>();
			var projectFileContents = GetProjectFileContentsAndName(fileName);
			if (projectFileContents.Value != null)
			{
				var assemblyName = GetAssemblyName(projectFileContents.Key, projectFileContents.Value);
				if (assemblyName != null)
				{
					var projectName = projectFileContents.Key;
					projectAndAssemblyName = new KeyValuePair<string, string>(projectName, assemblyName);
				}
			}
			return projectAndAssemblyName;
		}

		static string GetAssemblyName(string projectFileName, XDocument projectXml)
		{
			var element = projectXml.Descendants().FirstOrDefault(e => e.Name.LocalName == "AssemblyName");
			if (element != null)
			{
				return element.Value;
			}
			else
			{
				return Path.GetFileNameWithoutExtension(projectFileName);
			}
		}

		KeyValuePair<string, XDocument> GetProjectFileContentsAndName(string fileName)
		{
			var projectFileContents = GetProjectFileContentsAndNameFromCache(fileName);

			if (projectFileContents.Key == default)
			{
				return LoadPossibleProjectDataAndGetProjectFileContentsAndName(fileName);
			}
			return projectFileContents;
		}

		KeyValuePair<string, XDocument> LoadPossibleProjectDataAndGetProjectFileContentsAndName(string fileName)
		{
			fileName = fileName.Replace(".csproj", "");
			var directory = new DirectoryInfo(Path.GetDirectoryName(fileName));
			while (directory != null)
			{
				var projectFileDirectoryPath = directory.FullName;
				var allProjectsInDirectory = directory.GetFiles("*.csproj");

				if (allProjectsInDirectory.Length > 0)
				{
					foreach (FileInfo fileInfo in allProjectsInDirectory)
					{
						var projectFileName = fileInfo.Name;
						try
						{
							var projectXml = XDocument.Load(fileInfo.FullName);
							if (!ProjectFiles.ContainsKey(fileInfo.FullName))
							{
								ProjectFiles.Add(fileInfo.FullName, projectXml);
							}

							if (DoesProjectContainFile(projectFileDirectoryPath, projectXml, fileName))
							{
								return new KeyValuePair<string, XDocument>(projectFileName, projectXml);
							}
						}
						catch (XmlException)
						{
							// Don't crash the app if someone is editing csproj files poorly.
						}
					}
				}
				directory = directory.Parent;
			}
			return default;
		}

		KeyValuePair<string, XDocument> GetProjectFileContentsAndNameFromCache(string fileName)
		{
			foreach (var pair in ProjectFiles)
			{
				if (DoesProjectContainFile(Path.GetDirectoryName(pair.Key), pair.Value, fileName))
				{
					return new KeyValuePair<string, XDocument>(Path.GetFileName(pair.Key), pair.Value);
				}
			}
			return default;
		}

		bool DoesProjectContainFile(string projectFileDirectoryPath, XDocument projectXml, string fileName)
		{
			if (FileResidesWithinProjectDirectory(projectFileDirectoryPath, fileName))
			{
				var relativePath = fileName.Substring(projectFileDirectoryPath.Length).TrimStart('\\');
				if (!string.IsNullOrEmpty(relativePath))
				{
					if (IsNewSdkProject(projectXml))
					{
						var isExcluded = projectXml.Descendants().Any(e => (e.Name.LocalName == "None" || e.Name.LocalName == "Content") && e.Attributes().Any(a => a.Name.LocalName == "Exclude" && string.Equals(a.Value, relativePath, StringComparison.OrdinalIgnoreCase)));
						return !isExcluded; // For new SDK projects, unless the file is explicitly excluded, a file belongs to the project if it is located at the same level or below in the directory hierarchy.
					}
					else
					{
						return projectXml.Descendants().Any(e => e.Name.LocalName == "Compile" && e.Attributes().Any(a => a.Name.LocalName == "Include" && string.Equals(a.Value, relativePath, StringComparison.OrdinalIgnoreCase)));
					}
				}
			}
			return false;
		}

		static bool FileResidesWithinProjectDirectory(string projectFileDirectoryPath, string fileName)
		{
			for (var directory = new DirectoryInfo(fileName); directory != null; directory = directory.Parent)
			{
				if (directory.FullName == projectFileDirectoryPath)
				{
					return true;
				}
			}
			return false;
		}

		static bool IsNewSdkProject(XDocument document)
		{
			return document.Root.Name.LocalName == "Project" && document.Root.Attributes().Any(a => a.Name.LocalName == "Sdk");
		}

		Dictionary<string, XDocument> ProjectFiles
		{
			get
			{
				return _projectFiles ?? (_projectFiles = new Dictionary<string, XDocument>());
			}
		}

		Dictionary<string, XDocument> _projectFiles;
	}
}
