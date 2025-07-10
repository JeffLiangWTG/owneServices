using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.BuildTools;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Bi.Development.Common
{
	public static class BiConfigurationFileHandler
	{
		#region SuppressResourceStringsCheckRegion

		#region Combine CDC config file

		public static IDisposable CombineConfigFiles(BiConfigFileType fileType, out string fileContents)
		{
			var biConfigFile = GetBiConfigFile(fileType);
			fileContents = File.Exists(biConfigFile) ? File.ReadAllText(biConfigFile) : CreateConfigFile(fileType);

			return new DisposableAction(() =>
			{
				TryDeleteBiConfigFileIfNeeded(biConfigFile);
			});
		}

		static string GetBiConfigFile(BiConfigFileType fileType)
		{
			var filePath = BiConfigFileDirectory;
			if (Globals.IsTest)
			{
				filePath = Path.Combine(BuildConstants.LocalEnterprisePath, "BusinessIntelligence", "BiIntegration", "Development", "CargoWise.Bi.Development", "SchemaSync.Testing", "ConfigXmlForTest"); // filesystem path segments
			}

			return Path.Combine(filePath, $"{fileType}.xml");
		}

		static string CreateConfigFile(BiConfigFileType fileType)
		{
			var biConfigFileDirectory = Path.Combine(BiConfigFileDirectory, fileType.ToString());
			var schemaXmlFile = Path.Combine(biConfigFileDirectory, "__Schema.xml");
			string schemaXmlFileContent;
			try
			{
				schemaXmlFileContent = File.ReadAllText(schemaXmlFile);
			}
			catch (Exception e) when (e is DirectoryNotFoundException || e is FileNotFoundException)
			{
				CreateDirectoryIfNotExists(biConfigFileDirectory);
				CreateSchemaFile(biConfigFileDirectory, string.Empty);
				schemaXmlFileContent = File.ReadAllText(schemaXmlFile);
			}
			var biConfigFilesContent = new StringBuilder();

			foreach (var biConfigFiles in Directory.EnumerateFiles(biConfigFileDirectory, "*.xml", SearchOption.AllDirectories).Except(new[] { schemaXmlFile }).OrderBy(f => f))
			{
				var biConfigFileContent = File.ReadAllText(biConfigFiles);
				biConfigFilesContent.AppendLine(biConfigFileContent);
			}

			var fileContents = string.Format(CultureInfo.InvariantCulture, BiConfigTags, fileType.ToString(), schemaXmlFileContent, biConfigFilesContent.ToString());
			return fileContents;
		}

		static void TryDeleteBiConfigFileIfNeeded(string biConfigFile)
		{
			if (Globals.IsTest)
			{
				TryDelete(biConfigFile, 0);
			}
		}

		const string BiConfigTags = @"<?xml version=""1.0"" encoding=""utf-8""?>
<{0}>
{1}
{2}
</{0}>";

		#endregion

		#region Split BI config file

		public static void SplitBiConfigFile(BiConfigFileType fileType, string biConfigFileContent)
		{
			var configFileDirectory = Path.Combine(BiConfigFileDirectory, fileType.ToString());

			CreateDirectoryIfNotExists(configFileDirectory);
			DeleteConfigFiles(configFileDirectory);
			CreateSchemaFile(configFileDirectory, biConfigFileContent);
			CreateTableConfigFiles(fileType, configFileDirectory, biConfigFileContent);
		}

		static void CreateDirectoryIfNotExists(string configFileDirectory)
		{
			if (!Directory.Exists(configFileDirectory))
			{
				Directory.CreateDirectory(configFileDirectory);
			}
		}

		static void DeleteConfigFiles(string configFileDirectory)
		{
			var fileList = Directory.EnumerateFiles(configFileDirectory, "*.xml", SearchOption.AllDirectories);
			foreach (var file in fileList)
			{
				var retryCount = 0;
				TryDelete(file, retryCount);
			}
		}

		static void TryDelete(string file, int retryCount)
		{
			try
			{
				File.Delete(file);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (retryCount < 3)
				{
					TryDelete(file, retryCount + 1);
				}
				else
				{
					throw;
				}
			}
		}

		static void CreateSchemaFile(string configFileDirectory, string biConfigFileContent)
		{
			var schemaRegex = new Regex("\\s<xs:schema(.|\\s)+?<\\/xs:schema>", RegexOptions.IgnoreCase);
			var schemaXmlFileContent = schemaRegex.Match(biConfigFileContent).Value;
			var schemaXmlFilePath = Path.Combine(configFileDirectory, "__Schema.xml");

			File.WriteAllText(schemaXmlFilePath, schemaXmlFileContent);
		}

		static void CreateTableConfigFiles(BiConfigFileType fileType, string configFileDirectory, string biConfigFileContent)
		{
			string schemaColumn, tableColumn;
			switch (fileType)
			{
				case BiConfigFileType.CdcTableConfig:
					schemaColumn = "SourceSchema";
					tableColumn = "SourceTable";
					break;
				case BiConfigFileType.SsasCubes:
					schemaColumn = string.Empty;
					tableColumn = "SsasModelLogicalName";
					break;
				case BiConfigFileType.TabularModel:
					schemaColumn = string.Empty;
					tableColumn = "TabularModelLogicName";
					break;
				case BiConfigFileType.ReportMappingConfig:
					schemaColumn = string.Empty;
					tableColumn = "ReportName";
					break;
				default:
					schemaColumn = "Schema";
					tableColumn = "Name";
					break;
			}

			var tableContentRegex = new Regex($"\\s<{fileType}\\s((.+?\\/>)|((.|\\s)+?<\\/{fileType}>))", RegexOptions.IgnoreCase);
			var schemaRegex = new Regex($"{schemaColumn}\\s*=\\s*\"(?<Schema>.+?)\\\"", RegexOptions.IgnoreCase);
			var tableRegex = new Regex($"{tableColumn}\\s*=\\s*\"(?<Table>.+?)\\\"", RegexOptions.IgnoreCase);
			var transformIdRegex = new Regex("TransformId\\s*=\\s*\"(?<TransformId>.+?)\\\"", RegexOptions.IgnoreCase);

			foreach (Match match in tableContentRegex.Matches(biConfigFileContent))
			{
				var fileContent = match.Value;
				string filePath;
				var table = tableRegex.Match(fileContent).Groups["Table"].Value;
				if (fileType == BiConfigFileType.SsasCubes
					|| fileType == BiConfigFileType.TabularModel
					|| fileType == BiConfigFileType.ReportMappingConfig)
				{
					filePath = Path.Combine(configFileDirectory, $"{table}.xml");
				}
				else if (fileType == BiConfigFileType.EdwTableConfig)
				{
					var schema = schemaRegex.Match(fileContent).Groups["Schema"].Value;
					var transformId = transformIdRegex.Match(fileContent).Groups["TransformId"].Value;
					if (transformId == "1")
					{
						filePath = Path.Combine(configFileDirectory, $"{schema}_{table}.xml");
					}
					else
					{
						filePath = Path.Combine(configFileDirectory, $"{schema}_{table}_{transformId}.xml");
					}
				}
				else
				{
					var schema = schemaRegex.Match(fileContent).Groups["Schema"].Value;
					filePath = Path.Combine(configFileDirectory, $"{schema}_{table}.xml");
				}
				File.WriteAllText(filePath, fileContent);
			}
		}

		public static void AddConfigFilesToProject()
		{
			if (!Globals.IsTest)
			{
				var fileList = Directory.EnumerateFiles(BiConfigFileDirectory, "*.xml", SearchOption.AllDirectories);
				var configFilesRegex = new Regex("<!--BI config files-->(.|\\s)*?<!--BI config files-->", RegexOptions.IgnoreCase);
				var projectFileContent = File.ReadAllText(ConfigLoaderProjectFilePath);

				var configFilesEmbeddedResources =
					string.Format(CultureInfo.InvariantCulture,
	@"<!--BI config files-->
    {0}
  <!--BI config files-->",
						string.Join("\r\n    ", fileList.Where(f => new DirectoryInfo(f).Parent.Name != "Config").Select(f =>
							string.Format(CultureInfo.InvariantCulture, @"<EmbeddedResource Include=""Config\{0}\{1}"" />",
							new DirectoryInfo(f).Parent,
							new FileInfo(f).Name))));

				var newProjectFileContent = configFilesRegex.Replace(projectFileContent, configFilesEmbeddedResources);
				File.WriteAllText(ConfigLoaderProjectFilePath, newProjectFileContent);
			}
		}

		public static string ConfigLoaderProjectFilePath => Path.Combine(BuildConstants.LocalEnterprisePath, "Database", "BusinessIntelligence", "ConfigLoader", "ConfigLoader", "ConfigLoader.csproj");

		#endregion

		static string BiConfigFileDirectory => BiAutomationConfigLoaderForDevelopment.Instance.BiConfigDirectory;

		#endregion
	}
}
