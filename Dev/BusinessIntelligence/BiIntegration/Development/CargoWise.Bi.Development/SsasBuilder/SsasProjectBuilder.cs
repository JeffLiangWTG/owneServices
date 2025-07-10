using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Bi.BusinessIntelligence.Testing;
using CargoWise.Bi.Common;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.Bi.Developement.SchemaSync;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync;
using CargoWise.BuildTools;
using CargoWise.Common;
using Microsoft.AnalysisServices;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(BiDatabase))]

namespace CargoWise.Bi.Development.SsasBuilder
{
	public static class SsasProjectBuilder
	{
		#region SuppressResourceStringsCheckRegion

		[ThreadSafe]
		static string analysisServerName;
		public static string AnalysisServerName
		{
			get
			{
				if (string.IsNullOrWhiteSpace(analysisServerName))
				{
					var analysisServer = BiFileLogger.Instance.LoadAnalysisServerInstanceFile();
					analysisServerName = string.IsNullOrEmpty(analysisServer) ? Environment.MachineName : analysisServer;
				}
				return analysisServerName;
			}
			set
			{
				analysisServerName = value;
			}
		}

		#region File Paths

		[ThreadSafe]
		static string ssasCubeSourcePath;
		public static string SsasCubeSourcePath
		{
			get
			{
				if (ssasCubeSourcePath == null)
				{
					ssasCubeSourcePath = Path.Combine(BuildConstants.LocalEnterprisePath, @"BusinessIntelligence\CargoWiseBi\CargoWiseBi.Models");
				}
				return ssasCubeSourcePath;
			}
			set
			{
				ssasCubeSourcePath = value;
			}
		}

		[ThreadSafe]
		static string edwViewsDestinationPath;
		public static string EdwViewsDestinationPath
		{
			get
			{
				if (edwViewsDestinationPath == null)
				{
					edwViewsDestinationPath = Path.Combine(BiFiles.CWSharedPath, @"CargoWise.DbUpgrader\src\Scripts\Scripts.Definitions\BusinessIntelligence\EDW\Model");
				}
				return edwViewsDestinationPath;
			}
		}

		[ThreadSafe]
		static string reportUnitTestsFilePath;
		public static string ReportUnitTestsFilePath
		{
			get
			{
				if (reportUnitTestsFilePath == null)
				{
					reportUnitTestsFilePath = Path.Combine(BuildConstants.LocalEnterprisePath, @"BusinessIntelligence\BiIntegration\Deployment\CargoWiseBiDeployment\AnalysisServices.Testing\Reports\Tests");
				}
				return reportUnitTestsFilePath;
			}

			set
			{
				reportUnitTestsFilePath = value;
			}
		}

		[ThreadSafe]
		static string modelTestFilePath;
		public static string ModelTestFilePath
		{
			get
			{
				if (modelTestFilePath == null)
				{
					modelTestFilePath = Path.Combine(BuildConstants.LocalEnterprisePath, @"Database\DbUpgrader\Scripts\Script.Test\Public\BusinessIntelligence\EDW\Model");
				}
				return modelTestFilePath;
			}

			set
			{
				modelTestFilePath = value;
			}
		}

		#endregion

		#region Implementation

		public static void BuildSsasProject()
		{
			BiLogger.StartTask("Building SSAS Project");

			SortCubes();
			SyncModelViews();
			SyncSsasCubes();

			BiLogger.Complete("Building SSAS Project Completed");
		}

		#region Model Views

		static void SyncModelViews()
		{
			RemoveUnusedEdwProgrammability();
			CreateEdwEmbeddedResourcesAndClasses();
		}

		static void RemoveUnusedEdwProgrammability()
		{
			var modelViews = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig;
			var denormTableViews = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig;

			DeleteUnusedDirectories(modelViews, denormTableViews);
			DeleteUnusedModelViews(modelViews);
			DeleteUnusedDenormalizedViews(denormTableViews);
			DeleteUnusedInitialLoadProcedureForDenormalizedTables(denormTableViews);
			DeleteUnusedIncrementalLoadProcedureForDenormalizedTables(denormTableViews);
		}

		static void DeleteUnusedDirectories(Configuration.DataSets.BiAutomationConfigDataSet.EdwModelViewTableConfigDataTable modelViews, Configuration.DataSets.BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable denormTableViews)
		{
			var directoryInfo = new DirectoryInfo(EdwViewsDestinationPath);
			if (directoryInfo.Exists)
			{
				foreach (var folderPath in Directory.GetDirectories(EdwViewsDestinationPath))
				{
					var folderName = new DirectoryInfo(folderPath).Name;
					if (!modelViews.Select(t => t.Schema).Contains(folderName) &&
							!denormTableViews.Select(t => t.Schema).Contains(folderName))
					{
						Directory.Delete(folderPath, true);
					}
				}
			}
			else
			{
				directoryInfo.Create();
			}
		}

		static void DeleteUnusedModelViews(Configuration.DataSets.BiAutomationConfigDataSet.EdwModelViewTableConfigDataTable modelViews)
		{
			foreach (var filePath in Directory.GetFiles(EdwViewsDestinationPath, string.Format(CultureInfo.InvariantCulture, "{0}*", BiConstants.EdwModelViewPrefix), SearchOption.AllDirectories))
			{
				if (!modelViews.Any(t =>
					new Regex(string.Format(CultureInfo.InvariantCulture, "{0}.(cs|sql)", t.Name.Replace(" ", "_")), RegexOptions.IgnoreCase).Match(filePath).Success &&
					t.Schema == Directory.GetParent(filePath).Name))
				{
					File.Delete(filePath);
				}
			}
		}

		static void DeleteUnusedDenormalizedViews(Configuration.DataSets.BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable denormTableViews)
		{
			foreach (var filePath in Directory.GetFiles(EdwViewsDestinationPath, string.Format(CultureInfo.InvariantCulture, "{0}{1}*", BiConstants.EdwAggregateViewPrefix, BiConstants.EdwAggregateTablePrefix), SearchOption.AllDirectories))
			{
				if (!denormTableViews.Any(t =>
					new Regex(string.Format(CultureInfo.InvariantCulture, "{0}.(cs|sql)", t.Name.Replace(" ", "_")), RegexOptions.IgnoreCase).Match(filePath).Success &&
					t.Schema == Directory.GetParent(filePath).Name))
				{
					File.Delete(filePath);
				}
			}
		}

		static void DeleteUnusedInitialLoadProcedureForDenormalizedTables(Configuration.DataSets.BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable denormTableViews)
		{
			foreach (var filePath in Directory.GetFiles(EdwViewsDestinationPath, string.Format(CultureInfo.InvariantCulture, "{0}{1}*", BiConstants.EdwAggregateInitialLoadProcPrefix, BiConstants.EdwAggregateTablePrefix), SearchOption.AllDirectories))
			{
				if (!denormTableViews.Any(t =>
					new Regex(string.Format(CultureInfo.InvariantCulture, "{0}.(cs|sql)", t.Name.Replace(" ", "_")), RegexOptions.IgnoreCase).Match(filePath).Success &&
					t.Schema == Directory.GetParent(filePath).Name))
				{
					File.Delete(filePath);
				}
			}
		}

		static void DeleteUnusedIncrementalLoadProcedureForDenormalizedTables(Configuration.DataSets.BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable denormTableViews)
		{
			foreach (var filePath in Directory.GetFiles(EdwViewsDestinationPath, string.Format(CultureInfo.InvariantCulture, "{0}{1}*", BiConstants.EdwAggregateIncrementalLoadProcPrefix, BiConstants.EdwAggregateTablePrefix), SearchOption.AllDirectories))
			{
				if (!denormTableViews.Any(t =>
					new Regex(string.Format(CultureInfo.InvariantCulture, "{0}.(cs|sql)", t.Name.Replace(" ", "_")), RegexOptions.IgnoreCase).Match(filePath).Success &&
					t.Schema == Directory.GetParent(filePath).Name))
				{
					File.Delete(filePath);
				}
			}
		}

		static void CreateEdwEmbeddedResourcesAndClasses()
		{
			BiLogger.StartSubtask("Creating EDW Views");
			foreach (var schema in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig.Select(t => t.Schema).Distinct())
			{
				var directoryInfo = new DirectoryInfo(Path.Combine(EdwViewsDestinationPath, schema));
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
			}

			foreach (var schema in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig.Select(t => t.Schema).Distinct())
			{
				var directoryInfo = new DirectoryInfo(Path.Combine(EdwViewsDestinationPath, schema));
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
			}

			var edwFileList = new List<string>();

			foreach (var modelView in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig)
			{
				var filePath = Path.Combine(EdwViewsDestinationPath, modelView.Schema, string.Format(CultureInfo.InvariantCulture, "{0}.sql", modelView.Name.Replace(" ", "_")));
				var fileContent = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetCreateModelViewQuery(modelView);
				BiFiles.SaveFile(filePath, fileContent);
				CreateViewClassFile(modelView.Schema, modelView.Name, 1);

				edwFileList.Add($"BusinessIntelligence.EDW.Model.{modelView.Schema}.{modelView.Name}");
			}

			foreach (var denormTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig)
			{
				var filePath = Path.Combine(EdwViewsDestinationPath, denormTable.Schema, string.Format(CultureInfo.InvariantCulture, "{0}{1}.sql", BiConstants.EdwAggregateViewPrefix, denormTable.Name.Replace(" ", "_")));
				var fileContent = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetCreateDenormalizedTableViewQuery(denormTable);
				BiFiles.SaveFile(filePath, fileContent);
				CreateViewClassFile(denormTable.Schema, string.Format(CultureInfo.InvariantCulture, "{0}{1}", BiConstants.EdwAggregateViewPrefix, denormTable.Name), 2);
				edwFileList.Add($"BusinessIntelligence.EDW.Model.{denormTable.Schema}.{BiConstants.EdwAggregateViewPrefix}{denormTable.Name}");

				filePath = Path.Combine(EdwViewsDestinationPath, denormTable.Schema, string.Format(CultureInfo.InvariantCulture, "{0}{1}.sql", BiConstants.EdwAggregateInitialLoadProcPrefix, denormTable.Name.Replace(" ", "_")));
				fileContent = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetInitialLoadQueryForDenormalizedTable(denormTable);
				BiFiles.SaveFile(filePath, fileContent);
				CreateProcedureClassFile(denormTable.Schema, string.Format(CultureInfo.InvariantCulture, "{0}{1}", BiConstants.EdwAggregateInitialLoadProcPrefix, denormTable.Name));
				edwFileList.Add($"BusinessIntelligence.EDW.Model.{denormTable.Schema}.{BiConstants.EdwAggregateInitialLoadProcPrefix}{denormTable.Name}");

				filePath = Path.Combine(EdwViewsDestinationPath, denormTable.Schema, string.Format(CultureInfo.InvariantCulture, "{0}{1}.sql", BiConstants.EdwAggregateIncrementalLoadProcPrefix, denormTable.Name.Replace(" ", "_")));
				fileContent = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetIncrementalLoadQueryForDenormalizedTable(denormTable);
				BiFiles.SaveFile(filePath, fileContent);
				CreateProcedureClassFile(denormTable.Schema, string.Format(CultureInfo.InvariantCulture, "{0}{1}", BiConstants.EdwAggregateIncrementalLoadProcPrefix, denormTable.Name));
				edwFileList.Add($"BusinessIntelligence.EDW.Model.{denormTable.Schema}.{BiConstants.EdwAggregateIncrementalLoadProcPrefix}{denormTable.Name}");
			}

			foreach (var customTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig.Where(t => !string.IsNullOrWhiteSpace(t.CreateViewQuery)))
			{
				var filePath = Path.Combine(EdwViewsDestinationPath, customTable.Schema, string.Format(CultureInfo.InvariantCulture, "{0}.sql", customTable.ViewName.Replace(" ", "_")));
				var fileContent = customTable.CreateViewQuery;
				BiFiles.SaveFile(filePath, fileContent);
				CreateViewClassFile(customTable.Schema, customTable.ViewName, 2);
				edwFileList.Add($"BusinessIntelligence.EDW.Model.{customTable.Schema}.{customTable.ViewName}");
			}

			AddEdwFilesToScriptIndex(edwFileList);
		}

		static void AddEdwFilesToScriptIndex(List<string> edwFileList)
		{
			var edwModelFileListRegex = new Regex(@"\t*//EDW Model Scripts(.|\s)*?//EDW Model Scripts", RegexOptions.IgnoreCase);

			var edwScriptIndexFilePath = Path.Combine(BiFiles.CWSharedPath, "CargoWise.DbUpgrader", "src", "Scripts", "Scripts.Definitions", "CoreEdwScriptIndex.cs");

			if (File.Exists(edwScriptIndexFilePath))
			{
				var fileContent = File.ReadAllText(edwScriptIndexFilePath);

				var embeddedFileList = new StringBuilder();
				embeddedFileList.AppendLine("\t\t\t//EDW Model Scripts");
				foreach (var edwFile in edwFileList.OrderBy(f => f))
				{
					embeddedFileList.AppendLine($"\t\t\tyield return new {edwFile}();");
				}
				embeddedFileList.Append("\t\t\t//EDW Model Scripts");

				fileContent = edwModelFileListRegex.Replace(fileContent, embeddedFileList.ToString());
				File.WriteAllText(edwScriptIndexFilePath, fileContent);
			}
			else
			{
				throw new FileNotFoundException($"Could not find file: {edwScriptIndexFilePath}");
			}
		}

		static void CreateViewClassFile(string schema, string viewName, int testableObjectType)
		{
			string resourceName = "CargoWise.Bi.Development.SsasBuilder.Templates.ViewClassTemplate.txt";
			string viewClassTemplate = LoadTemplateResource(resourceName);

			Directory.CreateDirectory(Path.Combine(EdwViewsDestinationPath, schema));

			var filePath = Path.Combine(EdwViewsDestinationPath, schema, string.Format(CultureInfo.InvariantCulture, "{0}.cs", viewName.Replace(" ", "_")));
			var fileContent = viewClassTemplate.Replace("@@SCHEMA", schema).Replace("@@VIEWNAME", viewName.Replace(" ", "_")).Replace("@@TESTABLEOBJECTTYPE", testableObjectType.ToString(CultureInfo.InvariantCulture));
			if (!File.Exists(filePath))
			{
				BiFiles.SaveFile(filePath, fileContent);
			}

			CreateModelTestClassFile(schema, viewName);
		}

		static void CreateModelTestClassFile(string schema, string procedureName)
		{
			var resourceName = "CargoWise.Bi.Development.SsasBuilder.Templates.ModelTestClassTemplate.txt";
			var procedureClassTemplate = LoadTemplateResource(resourceName);

			Directory.CreateDirectory(Path.Combine(ModelTestFilePath, schema));

			var filePath = Path.Combine(ModelTestFilePath, schema, string.Format(CultureInfo.InvariantCulture, "{0}Test.cs", procedureName.Replace(" ", "_")));
			var fileContent = procedureClassTemplate.Replace("@@SCHEMA", schema).Replace("@@OBJNAME", procedureName.Replace(" ", "_"));
			if (!File.Exists(filePath))
			{
				BiFiles.SaveFile(filePath, fileContent);
			}
		}

		static void CreateProcedureClassFile(string schema, string procedureName)
		{
			var resourceName = "CargoWise.Bi.Development.SsasBuilder.Templates.ProcedureClassTemplate.txt";
			var procedureClassTemplate = LoadTemplateResource(resourceName);

			Directory.CreateDirectory(Path.Combine(EdwViewsDestinationPath, schema));

			var filePath = Path.Combine(EdwViewsDestinationPath, schema, string.Format(CultureInfo.InvariantCulture, "{0}.cs", procedureName.Replace(" ", "_")));
			var fileContent = procedureClassTemplate.Replace("@@SCHEMA", schema).Replace("@@PROCNAME", procedureName.Replace(" ", "_"));
			if (!File.Exists(filePath))
			{
				BiFiles.SaveFile(filePath, fileContent);
			}

			CreateProcedureTestClassFile(schema, procedureName);
		}

		static void CreateProcedureTestClassFile(string schema, string procedureName)
		{
			var resourceName = "CargoWise.Bi.Development.SsasBuilder.Templates.ProcedureTestClassTemplate.txt";
			var procedureClassTemplate = LoadTemplateResource(resourceName);

			Directory.CreateDirectory(Path.Combine(ModelTestFilePath, schema));

			var filePath = Path.Combine(ModelTestFilePath, schema, string.Format(CultureInfo.InvariantCulture, "{0}Test.cs", procedureName.Replace(" ", "_")));
			var fileContent = procedureClassTemplate.Replace("@@SCHEMA", schema).Replace("@@PROCNAME", procedureName.Replace(" ", "_"));
			if (!File.Exists(filePath))
			{
				BiFiles.SaveFile(filePath, fileContent);
			}
		}

		static string LoadTemplateResource(string resourceName)
		{
			var thisType = typeof(SsasProjectBuilder);
			var resourceStream = thisType.Assembly.GetManifestResourceStream(resourceName)
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Null resource stream returned from resource name: {0}", resourceName));

			using (var reader = new StreamReader(resourceStream))
			{
				return reader.ReadToEnd();
			}
		}

		#endregion

		#region SSAS Cubes

		static void SyncSsasCubes()
		{
			var bimFileList = Directory.EnumerateFiles(SsasCubeSourcePath, "*.bim", SearchOption.AllDirectories);
			AddToConfiguration(bimFileList);
			SyncCubeTables();
			SyncTabularModelLinkedTables();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:Avoid excessive complexity")]
		public static void SortCubes()
		{
			foreach (var file in Directory.EnumerateFiles(SsasCubeSourcePath, "*.bim", SearchOption.AllDirectories))
			{
				var modelJsonStr = File.ReadAllText(file);

				var deserializedtabularModel = JsonConvert.DeserializeObject<TabularModel>(modelJsonStr, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

				var sortedTabularModel = new TabularModel
				{
					Name = deserializedtabularModel.Name,
					CompatibilityLevel = deserializedtabularModel.CompatibilityLevel,
					Id = deserializedtabularModel.Id,
					Model = new Model
					{
						Culture = deserializedtabularModel.Model.Culture,
						DataSourcesList = deserializedtabularModel.Model.DataSourcesList,
						TablesList = deserializedtabularModel.Model.TablesList.
						Select(t => new Table {
							Name = t.Name,
							IsHidden = t.IsHidden,
							DataCategory = t.DataCategory,
							ColumnsList = t.ColumnsList.OrderBy(c => c.Name).ToList(),
							PartitionsList = t.PartitionsList != null ? t.PartitionsList.OrderBy(p => p.Name).ToList() : t.PartitionsList,
							HierarchiesList = t.HierarchiesList != null ? t.HierarchiesList.OrderBy(h => h.Name).ToList() : t.HierarchiesList,
							MeasuresList = t.MeasuresList != null ? t.MeasuresList.OrderBy(m => m.Name).ToList() : t.MeasuresList,
							AnnotationsList = t.AnnotationsList != null ? t.AnnotationsList.OrderBy(a => a.Name).ToList() : t.AnnotationsList
						})
						.OrderBy(t => t.Name).ToList(),
						RelationshipsList = deserializedtabularModel.Model.RelationshipsList?.OrderBy(r => r.Name)?.ToList()
					}
				};

				var sortedFile = JsonConvert.SerializeObject(sortedTabularModel, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
				BiFiles.SaveFile(file, sortedFile.Replace("SemanticModel", Path.GetFileNameWithoutExtension(file).Replace(" ", string.Empty)));
			}
			BiLogger.Complete("Tabular model files are successfully sorted.");
		}

		static void AddToConfiguration(IEnumerable<string> bimFileList)
		{
			var ssasCubes = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasCubes.ToList();

			foreach (var cube in ssasCubes)
			{
				if (!bimFileList.Select(f => Path.GetFileNameWithoutExtension(f)).Contains(cube.SsasModelFileName))
				{
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasCubes.RemoveSsasCubesRow(cube);
				}
			}

			foreach (var cubePath in bimFileList)
			{
				if (!ssasCubes.Select(c => c.SsasModelFileName).Contains(Path.GetFileNameWithoutExtension(cubePath)))
				{
					var databaseName = GetDatabaseNameFromBimFile(cubePath);
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasCubes.AddSsasCubesRow(Path.GetFileNameWithoutExtension(cubePath), databaseName);
				}
			}
		}

		#region Cube Tables

		static void SyncCubeTables()
		{
			using (var sourceControl = SourceControlFactory.Instance.GetSourceControl())
			{
				var bimFiles = sourceControl.GetFilesWithPendingChanges(includeDeletedFiles: false).Where(f => f.EndsWith(".bim", StringComparison.OrdinalIgnoreCase));
				if (bimFiles.Any())
				{
					BiLogger.StartTask("Synchronising SSAS Table Partition queries");
					using (var server = GetNewSsasServerConnection())
					{
						if (server != null)
						{
							var cubeList = new List<string>();
							try
							{
								foreach (var bimFile in bimFiles)
								{
									var dbName = CreateSsasCube(server, bimFile);
									cubeList.Add(dbName);

									var ssasModelFileName = Path.GetFileName(bimFile).Replace(".bim", "");
									AddSsasTablesToConfiguration(server, dbName, ssasModelFileName);
								}
							}
							finally
							{
								DropSsasDatabase(server, cubeList);
							}
						}
					}
				}
			}
		}

		static void SyncTabularModelLinkedTables()
		{
			new ColumnMapper().MapTabularModelsToCdcTables();
		}

		static Server GetNewSsasServerConnection()
		{
			var server = new Server();
			if (!string.IsNullOrEmpty(AnalysisServerName))
			{
				server.Connect(string.Format(CultureInfo.InvariantCulture, "Data Source={0};Application Name=BI Automation Configuration SSAS", AnalysisServerName)); // SQL Analysis Server connection string
				if ((!IsSql2016(server) &&
					!IsSql2019(server)) ||
					server.ServerMode != ServerMode.Tabular)
				{
					throw new SsasSyncException(string.Format(CultureInfo.InvariantCulture, "'{0}' needs to be a 2016 or 2019 edition tabular server. Select a different server.", AnalysisServerName));
				}
			}
			else
			{
					throw new SsasSyncException(string.Format(CultureInfo.InvariantCulture, "Failed to synchronise SSAS cubes.Analysis Server is empty. Create [{0}] and specify analysis server.", BiFileLogger.Instance.AnalysisServerInstanceFileName));
			}
			return server;
		}

		static bool IsSql2016(Server server)
		{
			return server.Version.StartsWith("13.", StringComparison.OrdinalIgnoreCase);
		}

		static bool IsSql2019(Server server)
		{
			return server.Version.StartsWith("15.", StringComparison.OrdinalIgnoreCase);
		}

		#region SSAS Deployment

		static string CreateSsasCube(Server server, string bimFile)
		{
			var databaseName = GetDatabaseNameFromBimFile(bimFile);
			var dbToDeploy = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", BiDatabase.TemplateDatabaseName, databaseName);
			var executeCommand = PrepareDeploymentCommand(bimFile, dbToDeploy);

			DropSsasDatabase(server, new List<string> { dbToDeploy });
			ExecuteBatchCommand(server, executeCommand);

			return dbToDeploy;
		}

		static void DropSsasDatabase(Server server, List<string> cubeList)
		{
			server.Refresh();
			foreach (var cube in cubeList)
			{
				var ssasDb = server.Databases.FindByName(cube);
				if (ssasDb != null)
				{
					ssasDb.Drop();
				}
			}
		}

		static string GetDatabaseNameFromBimFile(string filePath)
		{
			var bimFileContent = File.ReadAllText(filePath);
			Regex databaseNameRegex = new Regex(@"""name""\s*:\s*""(?<databaseName>.+)""", RegexOptions.IgnoreCase);
			Match match = databaseNameRegex.Match(bimFileContent);
			string databaseName = "";
			if (match.Success)
			{
				var matchGroup = match.Groups["databaseName"];
				if (matchGroup != null)
				{
					databaseName = match.Groups["databaseName"].Value;
				}
				else
				{
					throw new SsasSyncException(string.Format(CultureInfo.InvariantCulture, "Cannot find database name in script ({0}).", filePath));
				}
			}
			return databaseName;
		}

		static string PrepareDeploymentCommand(string bimFile, string databaseName)
		{
			var bimFileContent = File.ReadAllText(bimFile);
			ReplaceDatabaseFields(ref bimFileContent, databaseName);

			return string.Format(CultureInfo.InvariantCulture, @"
{{
	""createOrReplace"": {{
		""object"": {{
				""database"": ""{0}""
		}},
		""database"": {1}
	}}
}}", // Create command for JSON scripting TMSL
			databaseName,
			bimFileContent);
		}

		static void ReplaceDatabaseFields(ref string bimFileContent, string databaseName)
		{
			Regex databaseNameFieldRegex = new Regex(@"(?<databaseNameField>""name""\s*:\s*"".+"")", RegexOptions.IgnoreCase);
			Match match = databaseNameFieldRegex.Match(bimFileContent);
			if (match.Success)
			{
				var matchGroup = match.Groups["databaseNameField"];
				if (matchGroup != null)
				{
					bimFileContent = bimFileContent.Replace(match.Groups["databaseNameField"].Value, string.Format(CultureInfo.InvariantCulture, @"""name"": ""{0}""", databaseName));
				}
				else
				{
					throw new SsasSyncException("Cannot find database name in script.");
				}
			}

			Regex databaseIdFieldRegex = new Regex(@"(?<databaseIdField>""id""\s*:\s*"".+"")", RegexOptions.IgnoreCase);
			match = databaseIdFieldRegex.Match(bimFileContent);
			if (match.Success)
			{
				var matchGroup = match.Groups["databaseIdField"];
				if (matchGroup != null)
				{
					bimFileContent = bimFileContent.Replace(match.Groups["databaseIdField"].Value, string.Format(CultureInfo.InvariantCulture, @"""id"": ""{0}""", databaseName));
				}
				else
				{
					throw new SsasSyncException("Cannot find database id in script.");
				}
			}
		}

		static void ExecuteBatchCommand(Server server, string executeCommand)
		{
			if (!string.IsNullOrEmpty(executeCommand))
			{
				var executeResults = server.Execute(executeCommand);
				if (executeResults.Count > 0)
				{
					var resultsList = new StringBuilder();
					foreach (XmlaResult result in executeResults)
					{
						foreach (XmlaMessage message in result.Messages)
						{
							if (!string.IsNullOrEmpty(message.Description))
							{
								resultsList.AppendLine(message.Description);
							}
						}
					}

					if (resultsList.Length > 0)
					{
						throw new SsasSyncException(string.Format(CultureInfo.InvariantCulture, "Execution of BIM query failed.\r\n{0}", resultsList.ToString()));
					}
				}
			}
		}

		#endregion

		static void AddSsasTablesToConfiguration(Server server, string dbName, string ssasModelFileName)
		{
			server.Refresh();
			var ssasDb = server.Databases.FindByName(dbName);
			var ssasCubeRow = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasCubes.Where(c => c.SsasModelFileName == ssasModelFileName).FirstOrDefault();
			if (ssasDb != null && ssasCubeRow != null)
			{
				foreach (var ssasTable in ssasDb.Model.Tables)
				{
					string query = null;
					bool isCalculated = false;
					if (ssasTable.Partitions.Any())
					{
						switch (ssasTable.Partitions[0].SourceType)
						{
							case Microsoft.AnalysisServices.Tabular.PartitionSourceType.Query:
								query = ((Microsoft.AnalysisServices.Tabular.QueryPartitionSource)(ssasTable.Partitions[0]).Source).Query;
								break;
							case Microsoft.AnalysisServices.Tabular.PartitionSourceType.Calculated:
								query = ((Microsoft.AnalysisServices.Tabular.CalculatedPartitionSource)(ssasTable.Partitions[0]).Source).Expression;
								isCalculated = true;
								break;
							case Microsoft.AnalysisServices.Tabular.PartitionSourceType.None:
								break;
							default:
								break;
						}
					}

					var ssasTableRow = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasTables.Where(t => t.TableName == ssasTable.Name && t.SsasModelLogicalName == ssasCubeRow.SsasModelLogicalName).FirstOrDefault();
					if (ssasTableRow != null)
					{
						ssasTableRow.Query = query;
						ssasTableRow.IsCalculated = isCalculated;
					}
					else
					{
						BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasTables.AddSsasTablesRow(ssasCubeRow, ssasTable.Name, query, "", "", isCalculated);
					}
				}

				foreach (var ssasTable in ssasCubeRow.GetSsasTablesRows())
				{
					if (!ssasDb.Model.Tables.Any(t => t.Name == ssasTable.TableName))
					{
						BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasTables.RemoveSsasTablesRow(ssasTable);
					}
				}
			}
		}

		#endregion

		#region Loading models to XML

		public static List<string> LoadDatabaseList(string analysisServer)
		{
			var result = new List<string>();
			try
			{
				using (var server = SsasServer.New(analysisServer))
				{
					result = server.GetSsasDatabaseList();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ }
			return result;
		}

		public static List<string> LoadTableList(string analysisServer, string databaseName)
		{
			var result = new List<string>();
			try
			{
				using (var server = SsasServer.New(analysisServer))
				{
					result = server.GetSsasTableList(databaseName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ }
			return result;
		}

		public static DataTable LoadDataTable(string analysisServer, string databaseName, string tableName)
		{
			var dataTable = SsasHelper.ExecuteDaxQuery(analysisServer, databaseName, $"EVALUATE('{tableName}')");
			dataTable.TableName = tableName;
			return dataTable;
		}

		#endregion

		#endregion

		#endregion

		#endregion
	}
}
