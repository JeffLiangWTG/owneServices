using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Maintenance
{
	class TranslationFileExporterWriter : IDisposable
	{
		public TranslationFileExporterWriter(string path, string guiStringContext)
		{
			this.path = path;
			this.guiStringContext = guiStringContext;
		}

		public void WriteString(ResourceStringData data)
		{
			EnsureState();
			ResourceStringXmSerializer.ToXml(data, writer);
		}

		public void StartContainer(string containerName)
		{
			nextContainers.AddLast(containerName);
		}

		public void EndContainer()
		{
			if (nextContainers.Count > 0)
			{
				nextContainers.RemoveLast();
			}
			else
			{
				writer.WriteEndElement();
			}
		}

		void EnsureState()
		{
			if (writer == null)
			{
				if (File.Exists(path))
				{
					OpenAppend();
				}
				else
				{
					Directory.CreateDirectory(Path.GetDirectoryName(path));
					writer = XmlWriter.Create(path);
					WriteHeader(guiStringContext);
				}
			}
			while (nextContainers.Count > 0)
			{
				writer.WriteStartElement(ContainerElementName);
				writer.WriteAttributeString("Name", nextContainers.First.Value);
				nextContainers.RemoveFirst();
			}
		}

		void WriteHeader(string guiStringContext)
		{
			writer.WriteStartElement(RootElementName);
			writer.WriteAttributeString("xml", "lang", null, "en-US");
			if (!string.IsNullOrEmpty(guiStringContext))
			{
				writer.WriteAttributeString(GUIStringContextAttributeName, guiStringContext);
			}
		}

		void OpenAppend()
		{
			using var tempFile = TempFile.New();
			File.Copy(path, tempFile.Filename, overwrite: true);
			writer = XmlWriter.Create(path);
			using var xmlReader = XmlReader.Create(tempFile.Filename);
			while (xmlReader.Read() && !(xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == RootElementName))
			{ }
			WriteHeader(xmlReader.GetAttribute(GUIStringContextAttributeName));
			_ = xmlReader.Read();
			while (!(xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.LocalName == RootElementName))
			{
				writer.WriteNode(xmlReader, defattr: false);
			}
		}

		public void Dispose()
		{
			if (writer != null)
			{
				writer.WriteEndElement();
				writer.Close();
			}
		}

		public string FilePath
		{
			get { return path; }
		}

		readonly string path;
		readonly string guiStringContext;

		readonly LinkedList<string> nextContainers = new();

		XmlWriter writer;

		internal const string RootElementName = "EnterpriseResources";
		const string GUIStringContextAttributeName = "GUIStringContext";
		const string ContainerElementName = "Container";
	}
}
