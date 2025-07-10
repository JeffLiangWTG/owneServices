using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Common;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CargoWise.Bi.Common.BiConstants))]

namespace CargoWise.Bi.Configuration
{
	public static class BiConfiguration
	{
		#region SuppressResourceStringsCheckRegion

		#region File Operations

		public static void LoadConfigurationXml<T>(T configuration, IEnumerable<string> filePaths, bool clearRows = true) where T : DataSet // Internal tool only
		{
			try
			{
				if (clearRows)
				{
					configuration.Clear();
				}
				configuration.EnforceConstraints = false;

				foreach (var filePath in filePaths)
				{
					LoadConfigurationXmlCore(configuration, filePath);
				}

				configuration.EnforceConstraints = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new BiConfigurationException(ex.Message);
			}
		}

		public static void LoadConfigurationXml<T>(T configuration, string filePath, bool clearRows = true) where T : DataSet // Internal tool only
		{
			try
			{
				if (clearRows)
				{
					configuration.Clear();
				}
				configuration.EnforceConstraints = false;

				LoadConfigurationXmlCore(configuration, filePath);

				configuration.EnforceConstraints = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new BiConfigurationException(ex.Message);
			}
		}

		public static void LoadConfigurationXml<T>(T configuration, byte[] byteArray, bool clearRows = true) where T : DataSet // Internal tool only
		{
			try
			{
				if (clearRows)
				{
					configuration.Clear();
				}
				configuration.EnforceConstraints = false;

				LoadConfigurationXmlCore(configuration, byteArray);

				configuration.EnforceConstraints = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new BiConfigurationException(ex.Message);
			}
		}

		static void LoadConfigurationXmlCore<T>(T configuration, byte[] byteArray) where T : DataSet
		{
			using (Stream scriptContentsStream = new MemoryStream(byteArray))
			{
				using (var tempDataSet = new DataSet()) // Internal tool only
				{
					tempDataSet.EnforceConstraints = false;
					tempDataSet.Locale = CultureInfo.InvariantCulture;
					tempDataSet.ReadXml(scriptContentsStream, XmlReadMode.ReadSchema);
					tempDataSet.AcceptChanges();
					configuration.Merge(tempDataSet, false);
				}
			}
		}

		static void LoadConfigurationXmlCore<T>(T configuration, string filePath) where T : DataSet // Internal tool only
		{
			if (!File.Exists(filePath))
			{
				throw new BiConfigurationException(Path.GetFileName(filePath) + " file does not exist.");
			}

			using (var reader = XmlReader.Create(filePath, ReaderSettings))
			{
				using (var tempDataSet = new DataSet()) // Internal tool only
				{
					tempDataSet.EnforceConstraints = false;
					tempDataSet.Locale = CultureInfo.InvariantCulture;
					tempDataSet.ReadXml(reader, XmlReadMode.ReadSchema);
					tempDataSet.AcceptChanges();
					configuration.Merge(tempDataSet, false);
				}
			}
		}

		static XmlReaderSettings ReaderSettings
		{
			get
			{
				var readerSettings = new XmlReaderSettings()
				{
					ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema
				};
				return readerSettings;
			}
		}

		public static List<String> LoadTestDataContextsXmlListLocal(String path) // Internal tool only
		{
			try
			{
				List<String> testDataContextFiles = new List<String>();
				DirectoryInfo d = new DirectoryInfo(path);
				FileInfo[] files = d.GetFiles("*.xml"); // Internal tool only
				foreach (FileInfo file in files)
				{
					testDataContextFiles.Add(file.Name.Substring(0, file.Name.Length - 4));
				}
				return testDataContextFiles;
			}
			catch (Exception ex)
			{
				throw new BiConfigurationException("Failed to load file list from local folder", ex);
			}
		}

		public static List<String> LoadResultSetXmlListLocal(String path) // Internal tool only
		{
			try
			{
				List<String> resultSetFiles = new List<String>();
				DirectoryInfo d = new DirectoryInfo(path);
				FileInfo[] files = d.GetFiles("*.xml"); // Internal tool only
				foreach (FileInfo file in files)
				{
					resultSetFiles.Add(file.Name.Substring(0, file.Name.Length - 4));
				}
				return resultSetFiles;
			}
			catch (Exception ex)
			{
				throw new BiConfigurationException("Failed to load file list from local folder", ex);
			}
		}

		#endregion

		#endregion
	}
}
