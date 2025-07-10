using System.IO.Compression;
using System.Reflection;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XsdGeneration;
using Enterprise.ZArchitecture.Core;

namespace XMLSchemaExtractor
{
	public class XMLExtractor
	{
		readonly static string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
		readonly static string EmbedResourceFolder = "GeneratedSchemas";

		public void GenerateUniversalXmlSchemas()
		{
			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
			var directory = binPath;
			try
			{
				if (directory != null)
				{
					var xsdGenerator = new UniversalXsdGenerator(CurrentSchema.Namespace, CurrentSchema.Version);
					var universalFilesWritten = 0;
					var universalDataObjectsAssembly = typeof(Event).Assembly;
					using (var tempDir = new TempDirectory())
					{
						foreach (var type in universalDataObjectsAssembly.GetExportedTypes())
						{
							if (typeof(TopLevelDataObject).IsAssignableFrom(type) && type != typeof(TopLevelDataObject))
							{
								var schemaInfo = type.GetCustomAttribute<XsdSchemaAttribute>();
								var fileName = schemaInfo.SchemaName;
								var universalXsd = xsdGenerator.GetXsdOutput(type);
								UniversalXsdGenerator.SaveXsdFile(universalXsd, Path.Combine(tempDir, fileName));
								universalFilesWritten++;
							}
						}

						var commonSchemaXsd = xsdGenerator.GetXsdOutput(typeof(Event).Assembly);
						UniversalXsdGenerator.SaveXsdFile(commonSchemaXsd, Path.Combine(tempDir, UniversalXmlInfo.CommonSchemaName));
						ExportUniversalInterchangeSchema(tempDir);
						ExportUniversalResponseSchema(tempDir);
						universalFilesWritten += 3;

						var targetZipFileName = Path.Combine(directory, $@"{UniversalXmlInfo.ZipFileName}.zip");

						if (File.Exists(Path.Combine(binPath, targetZipFileName)))
						{
							File.Delete(targetZipFileName);
						}
						ZipFile.CreateFromDirectory(tempDir, targetZipFileName);
					}
				}
			}
			catch (XsdCreationException ex)
			{
				Console.Error.Write(ex.ToString());
				Environment.Exit(-1);
			}
		}

		void ExportUniversalInterchangeSchema(string directory)
		{
			var fullNameAndPath = directory + "\\UniversalInterchange.xsd";
			var schemaAssembly = Assembly.Load("Enterprise.Messaging.Module");
			using (var schemaStream = schemaAssembly.GetManifestResourceStream("Enterprise.Messaging.Module.Schemas.UniversalInterchange.xsd"))
			{
				UniversalXsdGenerator.SaveXsdFile(schemaStream.WriteToString(), fullNameAndPath);
			}
		}

		void ExportUniversalResponseSchema(string directory)
		{
			var fullNameAndPath = directory + "\\UniversalResponse.xsd";
			var schemaAssembly = Assembly.Load("Enterprise.Messaging.Module");
			using (var schemaStream = schemaAssembly.GetManifestResourceStream("Enterprise.Messaging.Module.Schemas.UniversalResponse.xsd"))
			{
				UniversalXsdGenerator.SaveXsdFile(schemaStream.WriteToString(), fullNameAndPath);
			}
		}

		public void GenerateNativeXmlSchemas()
		{
			var directory = binPath;
			try
			{
				if (directory != null)
				{
					var universalDataObjectsAssembly = typeof(HeaderData).Assembly;
					using (var tempDir = new TempDirectory())
					{
						var assembly = typeof(HeaderData).Assembly;
						var resourceNames = assembly.GetManifestResourceNames();

						var filteredResources = resourceNames
							.Where(name => name.Contains($".{EmbedResourceFolder}."))
							.ToList();

						foreach (var resourceName in filteredResources)
						{
							var prefix = $"{assembly.GetName().Name}.{EmbedResourceFolder}.";
							var fileName = resourceName.StartsWith(prefix)
								? resourceName.Substring(prefix.Length)
								: resourceName;
							var tempFilePath = Path.Combine(tempDir, fileName);
							using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
							{
								if (resourceStream != null)
								{
									using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
									{
										resourceStream.CopyTo(fileStream);
									}
								}
								else
								{
									throw new InvalidOperationException(
										$"Unable to find or load embedded resource: {resourceName}");
								}
							}
						}
#pragma warning disable CW1161 // Res.GetString Analyzer
						var targetZipFileName = Path.Combine(directory, $@"{NativeXmlInfo.ZipFileName}.zip");
#pragma warning restore CW1161 // Res.GetString Analyzer
						ZipCompression.Zip(tempDir, targetZipFileName);
					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.Write(ex.ToString());
				Environment.Exit(-1);
			}
		}
		//eAdaptorRegistry.Instance.UseDate2012_11NamespaceAndFormat
		public static IUniversalXmlSchema CurrentSchema
		{
			get
			{
				var current = UniversalXmlSchema.Version_2011_11;
				return current;
			}
		}
	}
}
