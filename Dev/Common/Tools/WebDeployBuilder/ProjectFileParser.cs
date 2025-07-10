using System.Collections.Immutable;
using System.Xml;

namespace WebDeployBuilder
{
	/// <summary>
	/// Class to parse project files and select all relevant files for the Web deployment
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html element")]
	public class ProjectFileParser
	{
		public ProjectFileParser(string projectFileName)
		{
			this.projectFileName = projectFileName;
		}

		readonly string projectFileName;

		/// <summary>
		/// gets list of all the files that need to be deployed, i.e. those with BuildAction attribute = Content or None
		/// </summary>
		/// <returns>Collection of all file names</returns>
		public IEnumerable<string> GetDeployableFiles()
		{
			if (File.Exists(projectFileName) && Path.GetDirectoryName(projectFileName) != null)
			{
				string projectFileFolder = Path.GetDirectoryName(projectFileName)!;
				using (var txtReader = new XmlTextReader(projectFileName))
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(txtReader);
					XmlElement? root = doc.DocumentElement;
					if (root == null)
					{
						return [];
					}
					return GetFilesFromRootForWhidbey(root, projectFileFolder);
				}
			}
			else
			{
				return Enumerable.Empty<string>();
			}
		}

		public string GetProjectDLL()
		{
			var result = "";

			if (File.Exists(projectFileName))
			{
				using (var textReader = new XmlTextReader(projectFileName))
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(textReader);
					XmlElement root = doc.DocumentElement!;
					XmlNodeList nodes = root.GetElementsByTagName("AssemblyName");
					if (nodes.Count == 1)
					{
						result = nodes.Item(0)?.InnerText;
					}
				}
			}
			return result ?? string.Empty;
		}

		protected IEnumerable<string> GetFilesFromRootForWhidbey(XmlElement root, string projectFileFolder)
		{
			var result = new List<string>();
			AddIncludeFilesFromNodeList(result, root.GetElementsByTagName("Content"), projectFileFolder);
			AddIncludeFilesFromNodeList(result, root.GetElementsByTagName("None"), projectFileFolder);
			AddIncludeSpecificFilesFromNodeList(result, root.GetElementsByTagName("EmbeddedResource"), new string[] { "Web.Config.Instructions" }, projectFileFolder);
			return result;
		}

		protected void AddIncludeSpecificFilesFromNodeList(List<string> files, XmlNodeList nodes, string[] fileNames, string projectFileFolder)
		{
			foreach (XmlNode node in nodes)
			{
				var str = node.Attributes!.GetNamedItem("Include")?.Value ?? node.Attributes.GetNamedItem("Update")?.Value;

				if (fileNames.Contains(str))
				{
					files.Add(Path.Combine(projectFileFolder, str!));
				}
			}
		}

		protected void AddIncludeFilesFromNodeList(List<string> files, XmlNodeList nodes, string projectFileFolder)
		{
			foreach (XmlNode node in nodes)
			{
				var str = node.Attributes?.GetNamedItem("Include")?.Value;

				str ??= node.Attributes?.GetNamedItem("Update")?.Value;

				if
				(
					!string.IsNullOrWhiteSpace(str) &&
					!str.EndsWith(".snk", StringComparison.OrdinalIgnoreCase) &&
					!IsExcluded(str)
				)
				{
					files.Add(Path.Combine(projectFileFolder, str));
				}
			}
		}

		bool IsExcluded(string fileName) => fileNamesToExcludeCaseInsensitive.Contains(fileName);

		readonly ImmutableHashSet<string> fileNamesToExcludeCaseInsensitive = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"web.base.config",
			"web.debug.config",
			"web.release.config",
			"web.base.tt",
			"web.debug.tt",
			"web.release.tt",
			".gitignore",
		}.ToImmutableHashSet();
	}
}
