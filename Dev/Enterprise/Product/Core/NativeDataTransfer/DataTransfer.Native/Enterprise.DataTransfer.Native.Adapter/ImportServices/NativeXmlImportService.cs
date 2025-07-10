using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.DataTransfer.Common.GUI.Import;
using Enterprise.DataTransfer.Native.Business;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.Native.Adapter.ImportServices
{
	public class NativeXmlImportService : IImportService
	{
		#region Import Service Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public NativeXmlImportService()
		{
			definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			VersionHeader = string.Format(@"<!-- CW1 Version : {0} Release : {1}-->", new EnterpriseInformationRetriever().VersionNumber, new EnterpriseInformationRetriever().Release);
		}

		public void Import()
		{
			using (var form = new DataImportForm<XElement>())
			{
				var session = new AncillaryImportServices(form.Logger);
				form.StreamProvider = new FileStreamProvider();
				form.ImportService = new ImportHandler(session);
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		readonly IDefinitionFinder definitionFinder;

		public bool CanBeImported(string tableName)
			=> !NativeXMLSupportValidator.IsTableDeprecated(tableName) && definitionFinder.HasDefinitionWithTopTableName(tableName);

		string DialogCaption
		{
			get { return Res.GetString("1184cc88-fdb9-41f5-92fc-c290ee512ae6", "Generate Native XML Schemas"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "path string is safe")]
		public void GenerateAndSaveXSD(string tableName)
		{
			var directory = GetDirectory();

			try
			{
				if (directory != null)
				{
					using (var tempDir = new TempDirectory())
					{
						if (tableName == null)
						{
							var entitySetCount = GenerateAndSaveAllXSDs(tempDir);
							WriteUniversalCommonSchema(tempDir);

							var targetZipFileName = Path.Combine(directory, $@"{NativeXmlInfo.ZipFileName}.zip");
							if (RemoteZipCompression.Zip(tempDir, targetZipFileName))
							{
								Globals.Message.ShowInformation(Res.GetString("1ffbe5c6-09a0-4d74-9b21-e8f0019fbb52", "{0} Individual Schema files plus the Common Schema file and Reference Data Schema file exported to [{1}].", entitySetCount.ToString(), directory), DialogCaption);
							}
							else
							{
								Globals.Message.ShowError(Res.GetString("6F5F796D-7C6B-404A-9A68-12BF36252E23", "Failed to create zip file \"[{0}]\\Native XML.zip\".", directory));
							}
						}

						else
						{
							var entitySet = definitionFinder.FindByTopTableName(tableName);
							var fileName = Path.Combine(tempDir, entitySet.GetXSDFileName());

							SaveXsdFile(GenerateXSD(entitySet), fileName);
							WriteUniversalCommonSchema(tempDir);
							WriteNativeRootSchema(tempDir);
							WriteNativeRequestSchema(tempDir);

							var targetZipFileName = Path.Combine(directory, $@"Native{entitySet.Name}.zip");
							if (RemoteZipCompression.Zip(tempDir, targetZipFileName))
							{
								Globals.Message.ShowInformation(Res.GetString("52a46e6c-e5b9-435c-ae5a-19c3d1dee2fe", "{0} plus the Common Schema file and Native Schema files exported to [{1}].", entitySet.GetXSDFileName(), directory), DialogCaption);
							}
							else
							{
								Globals.Message.ShowError(Res.GetString("6F5F796D-7C6B-404A-9A68-12BF36252E23", "Failed to create zip file \"[{0}]\\Native XML.zip\".", directory));
							}
						}
					}
				}
			}
			catch (XsdCreationException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (ApplicationException exception)
			{
				if (!string.IsNullOrEmpty(tableName))
				{
					throw new ApplicationException("Error Generating XSD for table: [" + tableName + "]", exception);
				}
				else
				{
					throw;
				}
			}
		}

		public int GenerateAndSaveAllXSDs(string directoryName)
		{
			var globalDefinitions = GlobalDefinition.Instance;
			var definitions = globalDefinitions.TableMapping;
			var entitySetCount = 0;

			foreach (var definition in definitions)
			{
				var entitySet = GetEntitySetDefinition(definition.Value);
				if (!entitySet.IsLegacyOperationalNativeDataSetReplacedByUniversal())
				{
					string fileName = Path.Combine(directoryName, entitySet.GetXSDFileName());
					SaveXsdFile(GenerateXSD(entitySet), fileName);
					entitySetCount++;
				}
			}
			WriteNativeRootSchema(directoryName);
			WriteNativeRequestSchema(directoryName);

			return entitySetCount;
		}

		void WriteUniversalCommonSchema(string directoryName)
		{
			var generator = ObjectFactory.Get<IUniversalXsdGenerator>();
			string generatedXsd = generator.GetCommonSchemaXsdOutput();
			SaveXsdFile(generatedXsd, Path.Combine(directoryName, UniversalXmlInfo.CommonSchemaName));
		}

		void WriteNativeRootSchema(string directoryName)
		{
			var xsdGenerator = new NativeXsdGenerator();
			string nativeRootXsd = xsdGenerator.GenerateNativeRootSchema();
			var fullNameAndPath = Path.Combine(directoryName, "Native.xsd");
			SaveXsdFile(nativeRootXsd, fullNameAndPath);
		}

		void WriteNativeRequestSchema(string directoryName)
		{
			var xsdGenerator = new NativeXsdGenerator();
			string nativeRootXsd = xsdGenerator.GenerateNativeRequestSchema();
			var fullNameAndPath = Path.Combine(directoryName, "NativeRequest.xsd");
			SaveXsdFile(nativeRootXsd, fullNameAndPath);
		}

#if DEBUG
		public void WriteUniversalCommonSchemaForTesting(string directoryName)
		{
			WriteUniversalCommonSchema(directoryName);
		}
#endif

		#endregion

		#region Get Directory

#if DEBUG
		protected virtual
#endif
 string GetDirectory()
		{
			using (var folderBrowserDialog = new ZFolderBrowserDialog())
			{
				var result = ZArchitecture.GUI.ZFormModaliser.ShowCommonDialogWithoutDispose(folderBrowserDialog);
				return result == DialogResult.OK ? folderBrowserDialog.UnmappedSelectedPath : null;
			}
		}

		#endregion
		public readonly string VersionHeader;

		string GenerateXSD(EntitySetDefinition entitySet)
		{
			var xsdGenerator = new NativeXsdGenerator();
			return xsdGenerator.Generate(entitySet).ToString();
		}

		EntitySetDefinition GetEntitySetDefinition(string entitySetName)
		{
			var definitionData = new DefinitionAssemblyLoader().Load(entitySetName);
			return new EntitySetDefinitionBuilder(definitionData).GetEntitySetDefinition();
		}

		void SaveXsdFile(string content, string fileNameWithPath)
		{
			if (string.IsNullOrEmpty(content))
			{
				ErrorReporter.ReportOnce("XSD file should not be empty", $"XSD file {Path.GetFileName(fileNameWithPath)} should not be empty");
				throw new XsdCreationException("XSD file should not be empty");
			}

			content = string.Join("\r\n", VersionHeader, content);

			try
			{
				var doc = new XmlDocument();
				doc.LoadXml(content);
				using (var streamWriter = new StreamWriter(File.Open(fileNameWithPath, FileMode.Create, FileAccess.ReadWrite)))
				{
					using (var xmlWriter = NativeXmlWriter.Create(streamWriter))
					{
						doc.Save(xmlWriter);
					}
				}
			}
			catch (ArgumentException exception)
			{
				throw new XsdCreationException(string.Format("Cannot create [{0}] - {1}", fileNameWithPath, exception.Message));
			}
			catch (IOException ioException)
			{
				throw new XsdCreationException(string.Format("Cannot create [{0}] - {1}", fileNameWithPath, ioException.Message));
			}
			catch (UnauthorizedAccessException)
			{
				throw new XsdCreationException("Cannot write the file to disk. Please check with your system administrator.");
			}
		}
	}
}
