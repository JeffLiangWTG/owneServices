using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;

namespace Enterprise.Builder.Generator.Cache
{
	public class ProjectFilesCache : GenericConcurrentCache<Collection<XmlNode>>
	{
		protected ProjectFilesCache() { }

		public static ProjectFilesCache Instance => overridableInstance.Value ?? (overridableInstance.Value = new ProjectFilesCache());

		static readonly Overridable<ProjectFilesCache> overridableInstance = new Overridable<ProjectFilesCache>();

		public Collection<XmlNode> GetFilesInProject(string projectPath)
		{
			return Get(projectPath);
		}

		protected override Collection<XmlNode> GeneratorFunc(string projectFileName)
		{
			var doc = GetXmlDocument(projectFileName);
			XmlNamespaceManager buildNamespaceManager = new XmlNamespaceManager(doc.NameTable);
			buildNamespaceManager.AddNamespace("csproj", "http://schemas.microsoft.com/developer/msbuild/2003");

			var documentNodes = new Collection<XmlNode>(doc.SelectNodes("//csproj:Compile|//csproj:Content|//csproj:None", buildNamespaceManager).Cast<XmlNode>().ToList());

			if (documentNodes.Count > 0)
			{
				return documentNodes;
			}

			//WI00471754 - The code below is a short-term solution for modern SDK project files
			//the medium to long term plan is to replace this with source generators
			string rootPath = Path.GetDirectoryName(projectFileName);
			var files = Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
							.Where(f => !f.Contains("\\obj\\") && (f.EndsWith(".cs") || f.EndsWith(".xml") || f.EndsWith(".xsd")));

			var fileXml = new XmlDocument();
			var rootNode = fileXml.CreateElement("FileGroup");
			fileXml.AppendChild(rootNode);

			foreach (var file in files)
			{
				XmlElement fileNode = null;

				if (file.EndsWith(".cs"))
				{
					fileNode = fileXml.CreateElement("Compile");
				}
				else
				{
					fileNode = fileXml.CreateElement("Content");
				}

				var fileAttribute = fileXml.CreateAttribute("Include");

				fileAttribute.Value = file.Substring(rootPath.Length + 1);
				fileNode.Attributes.Append(fileAttribute);
				rootNode.AppendChild(fileNode);
			}

			var fileNodes = new Collection<XmlNode>(fileXml.SelectNodes("//FileGroup/Compile|//FileGroup/Content", buildNamespaceManager).Cast<XmlNode>().ToList());

			return fileNodes;
		}

		[SuppressMessage("Microsoft.Design", "CA1059")]
		virtual protected XmlDocument GetXmlDocument(string projectFileName)
		{
			XmlDocument doc = new XmlDocument();

			try
			{
				doc.Load(projectFileName);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Cannot load project file \"" + projectFileName + "\"", e);
			}
			return doc;
		}
	}
}
