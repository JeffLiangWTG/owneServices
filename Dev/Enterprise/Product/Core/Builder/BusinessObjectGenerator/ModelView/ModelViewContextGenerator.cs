using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Enterprise.BusinessObjectGenerator.ModelView
{
	public class ModelViewContextGenerator
	{
		public ModelViewContextGenerator(string cwSharedPath)
		{
			projectRootPath = Path.Combine(cwSharedPath, ModelViewConstants.RelativeProjectRootPath);
		}

		public virtual ModelViewContext GetCodeGeneratorContext(string inputFilePath)
		{
			var parentDirectory = Path.GetDirectoryName(inputFilePath);
			var modelName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(inputFilePath));
			if (string.IsNullOrWhiteSpace(parentDirectory) || string.IsNullOrWhiteSpace(modelName))
			{
				return null;
			}

			var view = GetView(inputFilePath);
			var columns = GetColumns(view.Table);

			if (!string.IsNullOrEmpty(view.ParentView) && AllModelFiles.TryGetValue($"{view.ParentView}{ModelViewConstants.ModelFileSuffix}", out var parentFilePath))
			{
				view.ParentContext = GetCodeGeneratorContext(parentFilePath);
			}

			var context = new ModelViewContext
			{
				ModelView = view,
				ModelName = modelName,
				ParentDirectory = parentDirectory,
				Columns = columns,
				AddInfoColumnName = columns.FirstOrDefault(IsAddInfo) ?? view.AddInfoColumn,
				NAddInfoColumnName = columns.FirstOrDefault(IsNAddInfo) ?? view.NAddInfoColumn,
				ClusterKeyColumnName = columns.FirstOrDefault(IsClusterKey),
				PKColumnName = columns.FirstOrDefault(IsPrimaryKey),
				DefinitionNamespace = GetNamespace(parentDirectory),
			};

			return context;
		}

		public List<ModelViewContext> GetAllCodeGeneratorContexts(string rootDirectory)
		{
			var allContexts = new List<ModelViewContext>();

			var allModelFiles = GetModelFiles(rootDirectory);
			foreach (var modelFile in allModelFiles)
			{
				allContexts.Add(GetCodeGeneratorContext(modelFile));
			}

			return allContexts;
		}

		public string[] GetModelFiles(string rootDirectory)
		{
			return Directory.GetFiles(rootDirectory, $"*{ModelViewConstants.ModelFileSuffix}", SearchOption.AllDirectories);
		}

		public Dictionary<string, string> AllModelFiles
		{
			get
			{
				if (allModelFiles != null)
				{
					return allModelFiles;
				}

				allModelFiles = new Dictionary<string, string>();
				var allFiles = GetModelFiles(projectRootPath);
				foreach (var modelFile in allFiles)
				{
					allModelFiles.Add(Path.GetFileName(modelFile), modelFile);
				}

				return allModelFiles;
			}
		}
		Dictionary<string, string> allModelFiles;

		public List<string> GetColumns(string tableName)
		{
			if (tableName.Equals("DummyBizo", StringComparison.Ordinal))
			{
				return new List<string> { "Z0_PK" };
			}

			var regex = new Regex(@$"^CREATE TABLE {tableName} \(\r?$.*?^\);\r?$", RegexOptions.Singleline | RegexOptions.Multiline);
			var match = regex.Match(SchemaMainContent);

			if (!match.Success)
			{
				return [];
			}

			var parser = new TSql160Parser(true);
			var reader = new StringReader(match.Value);

			var statementList = parser.ParseStatementList(reader, out var errors);
			var statements = statementList.Statements;

			if (errors.Count == 0 && statements.Count != 0 && statements[0] is CreateTableStatement createTableStatement)
			{
				var definition = createTableStatement.Definition;
				return definition.ColumnDefinitions.Select(def => def.ColumnIdentifier.Value).ToList();
			}

			return [];
		}

		public bool IsAddInfo(string columnName)
		{
			return columnName.EndsWith("_AddInfo");
		}

		public bool IsNAddInfo(string columnName)
		{
			return columnName.EndsWith("_NAddInfo");
		}

		public bool IsClusterKey(string columnName)
		{
			return columnName.EndsWith("_ClusterKey");
		}

		public bool IsPrimaryKey(string columnName)
		{
			return columnName.EndsWith("_PK");
		}

		View GetView(string filePath)
		{
			using var fileStream = new FileStream(filePath, FileMode.Open);
			using var reader = XmlReader.Create(fileStream);

			var serializer = new XmlSerializer(typeof(View));
			var view = (View)serializer.Deserialize(reader);

			return view;
		}

		string SchemaMainContent
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(schemaMainContent))
				{
					return schemaMainContent;
				}

				var schemaMainBuilder = new StringBuilder();

				using (var reader = new StreamReader(Assembly.Load(ModelViewConstants.DbSchemaScriptAssembly).GetManifestResourceStream(ModelViewConstants.DbSchemaScriptResourcePath)))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						schemaMainBuilder.AppendLine(line);
					}
				}

				schemaMainContent = schemaMainBuilder.ToString();

				return schemaMainContent;
			}
		}
		string schemaMainContent;

		string GetNamespace(string directoryPath)
		{
			var defLocation = directoryPath.IndexOf(ModelViewConstants.ScriptsDefinitionsNamespace, StringComparison.Ordinal);

			if (defLocation == -1)
			{
				return default;
			}

			var ns = directoryPath.Substring(defLocation + ModelViewConstants.ScriptsDefinitionsNamespace.Length).Trim('\\').Replace("\\", ".");

			return string.Join(ModelViewConstants.NamespaceSeparator, ModelViewConstants.NamespacePrefix, ns);
		}

		readonly string projectRootPath;
	}
}
