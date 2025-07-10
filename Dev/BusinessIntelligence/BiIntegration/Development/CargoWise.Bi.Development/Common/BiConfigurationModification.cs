using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Bi.Configuration;

namespace CargoWise.Bi.Development.Common
{
	public static class BiConfigurationModification
	{
		public static void SaveConfigurationXml<T>(T configuration, string filePath) where T : DataSet // Internal tool only
		{
			byte[] fileContent;
			using (var ms = new MemoryStream())
			{
				using (XmlWriter writer = XmlWriter.Create(ms, WriterSettings))
				{
					configuration.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
					configuration.WriteXml(writer, XmlWriteMode.WriteSchema);
				}
				fileContent = ms.ToArray();
			}

			File.WriteAllBytes(filePath, fileContent);
		}

		public static string GetConfigurationFileContent<T>(T configuration) where T : DataSet
		{
			byte[] fileContent;
			using (var ms = new MemoryStream())
			{
				using (XmlWriter writer = XmlWriter.Create(ms, WriterSettings))
				{
					configuration.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
					configuration.WriteXml(writer, XmlWriteMode.WriteSchema);
				}
				fileContent = ms.ToArray();
			}
			return Encoding.Default.GetString(fileContent);
		}

		public static void DeleteConfigurationXml(string file, string filePath)
		{
			try
			{
				var directory = new DirectoryInfo(filePath);
				var files = directory.GetFiles("*.xml");
				foreach (var localfile in files)
				{
					if (Path.Combine(filePath, localfile.Name).Equals(file))
					{
						File.Delete(file);
					}
				}
			}
			catch (Exception ex)
			{
				throw new BiConfigurationException("Failed to save configuration XML", ex);
			}
		}

		static XmlWriterSettings WriterSettings
		{
			get
			{
				var writerSettings = new XmlWriterSettings()
				{
					Encoding = Encoding.UTF8,
					NewLineChars = "\r\n",
					Indent = true,
					IndentChars = "\t",
					ConformanceLevel = ConformanceLevel.Document,
					OmitXmlDeclaration = false,
					NewLineOnAttributes = false,
					NamespaceHandling = NamespaceHandling.OmitDuplicates,
					WriteEndDocumentOnClose = true,
					CloseOutput = true
				};

				return writerSettings;
			}
		}
	}
}
