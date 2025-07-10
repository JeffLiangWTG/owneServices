using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Enterprise.ZArchitecture.Web.Shared.Test
{
	public static class ProjectFileParser
	{
		public static IEnumerable<string> GetDeployableFiles(string projectFileName)
		{
			if (File.Exists(projectFileName))
			{
				using (var xmlTextReader = new XmlTextReader(projectFileName))
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(xmlTextReader);
					XmlElement root = doc.DocumentElement;
					return GetFilesFromRootForWhidbey(root);
				}
			}
			else
			{
				return Enumerable.Empty<string>();
			}
		}

		static IEnumerable<string> GetFilesFromRootForWhidbey(XmlElement root)
		{
			var result = new List<string>();
			AddIncludeFilesFromNodeList(result, root.GetElementsByTagName("Content"));	// This is an element
			AddIncludeFilesFromNodeList(result, root.GetElementsByTagName("None"));	// This is an element
			AddIncludeSpecificFilesFromNodeList(
				result,
				root.GetElementsByTagName("EmbeddedResource"),
				new[] { "Web.Config.Instructions" });	// This is an element
			return result;
		}

		static void AddIncludeSpecificFilesFromNodeList(List<string> files, IEnumerable nodes, string[] fileNames)
		{
			files.AddRange(
				from XmlNode node in nodes
				select node.Attributes?.GetNamedItem("Include").Value
				into specificFile
				where !string.IsNullOrEmpty(specificFile) && fileNames.Contains(specificFile)
				select specificFile);
		}

		static void AddIncludeFilesFromNodeList(ICollection<string> files, XmlNodeList nodes)
		{
			foreach (XmlNode node in nodes)
			{
				var fileName = node.Attributes?.GetNamedItem("Include").Value;	// This is an element
				if (!string.IsNullOrEmpty(fileName)
					&& !fileName.EndsWith(".snk", StringComparison.OrdinalIgnoreCase))
				{
					files.Add(fileName);
				}
			}
		}
	}
}
