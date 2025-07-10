using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.ZArchitecture.Core;
using Microsoft.SqlServer.Types;

namespace Enterprise.Builder.Generator
{
	public delegate void GeneratorEvent(string message);

	/// <summary>
	/// Base class for generators for typed dataset and business object classes.
	/// </summary>
	public abstract class AbstractGenerator
	{
		protected AbstractGenerator(GeneratorOutputDirectory outputDirectory)
		{
			this.outputDirectory = outputDirectory;
		}

		protected readonly GeneratorOutputDirectory outputDirectory;

		public GeneratorEvent AddReportLine;
		public GeneratorEvent AddFileGeneratedLine;
		public GeneratorEvent AddErrorLine;
		public GeneratorEvent AddSkippedFile;

		protected readonly string BuildXmlFileName = BuildXml.Instance.BuildXmlFileName;

		public abstract void Generate();

		public virtual void AddReportLineEvent(GeneratorEvent gE)
		{
			AddReportLine += gE;
		}

		public virtual void AddFileGeneratedLineEvent(GeneratorEvent gE)
		{
			AddFileGeneratedLine += gE;
		}

		public virtual void AddErrorLineEvent(GeneratorEvent gE)
		{
			AddErrorLine += gE;
		}

		public virtual void AddSkippedFileEvent(GeneratorEvent gE)
		{
			AddSkippedFile += gE;
		}

		public abstract void GenerateSpecificFile(string fileName);
		public abstract void GenerateAllFilesForSolution(string solutionName);

		#region Implementation

		protected void OnAddReportLine(string text)
		{
			if (AddReportLine != null)
			{
				text = text.Replace(System.Environment.NewLine, System.Environment.NewLine + "\t");
				AddReportLine(text);
			}
		}

		protected void OnAddFileGeneratedLine(string text)
		{
			if (AddFileGeneratedLine != null)
			{
				text = text.Replace(System.Environment.NewLine, System.Environment.NewLine + "\t");
				AddFileGeneratedLine(text);
			}
		}

		protected void OnAddErrorLine(string text)
		{
			if (AddErrorLine != null)
			{
				text = System.Environment.NewLine + "Error: " + text.Replace(System.Environment.NewLine, System.Environment.NewLine + "\t");
				AddErrorLine(text);
			}
		}

		protected void OnAddSkippedFile(string text)
		{
			if (AddSkippedFile != null)
			{
				AddSkippedFile(text);
			}
		}

		protected XmlNodeList FilesInProject(string projectFileName)
		{
			XmlDocument doc = new XmlDocument();

			try
			{
				doc.Load(projectFileName);
			}
			catch (Exception e)
			{
				throw new ApplicationException("Cannot load project file \"" + projectFileName + "\"", e);
			}

			XmlNamespaceManager buildNamespaceManager = new XmlNamespaceManager(doc.NameTable);
			buildNamespaceManager.AddNamespace("csproj", "http://schemas.microsoft.com/developer/msbuild/2003");
			return doc.SelectNodes("//csproj:Compile|//csproj:Content|//csproj:None", buildNamespaceManager);
		}

		protected bool NodeIsXsdFile(XmlNode node)
		{
			bool result = false;
			XmlAttribute relPathAttribute = node.Attributes["Include"];

			if (relPathAttribute != null && IsXsdFile(relPathAttribute.Value))
			{
				XmlNode generatorNode = node["Generator"];
				result = (generatorNode != null) && generatorNode.InnerText == "MSDataSetGenerator";
			}

			return result;
		}

		protected bool NodeIsCSharpFile(XmlNode node)
		{
			bool result = false;
			XmlAttribute attribute = node.Attributes["Include"];

			if (attribute != null && IsCSharpFile(attribute.Value))
			{
				XmlNode subTypeNode = node["SubType"];
				result = subTypeNode == null || (subTypeNode != null && subTypeNode.InnerText == "Code");
			}

			return result;
		}

		protected bool NodeIsXmlFile(XmlNode node)
		{
			XmlAttribute attribute = node.Attributes["Include"];
			return attribute != null && IsXmlFile(attribute.Value);
		}

		protected bool IsProjectFile(string fileName)
		{
			return Path.GetExtension(fileName).ToLower() == ".csproj";
		}

		protected bool IsXsdFile(string fileName)
		{
			return Path.GetExtension(fileName).ToLower() == ".xsd";
		}

		protected bool IsXmlFile(string potentialXmlFileName)
		{
			return Path.GetExtension(potentialXmlFileName).ToLower() == ".xml";
		}

		protected bool IsCSharpFile(string fileName)
		{
			return Path.GetExtension(fileName).ToLower() == ".cs";
		}

		protected string CSharpFileName(string fileName)
		{
			return Path.ChangeExtension(fileName, ".cs");
		}

		protected string CSharpDesignerFileName(string fileName)
		{
			return Path.ChangeExtension(fileName, ".Designer.cs");
		}

		protected void EnsureDirectoryExists(string fileName)
		{
			string directoryName = Path.GetDirectoryName(fileName);
			if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
		}

		protected bool FileIsReadOnly(string fileName)
		{
			return File.Exists(fileName) && (File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
		}

		protected string NamespaceFromCSharpFile(string fileName)
		{
			return NamespaceFromCSharpFile(fileName, "Enterprise. <<<--- INSERT NAMESPACE HERE!");
		}

		protected string NamespaceFromCSharpFile(string fileName, string defaultNamespace)
		{
			const string NamespacePattern = "[A-Z][A-Za-z0-9_.]+[A-Za-z0-9_]";
			const string Pattern = @"\snamespace\s+(" + NamespacePattern + ")";

			string result = null;

			if (File.Exists(fileName))
			{
				result = InnerMatch(File.ReadAllText(fileName), Pattern);
			}

			return result ?? defaultNamespace;
		}

		#region Column Defaults

		/// <summary>
		/// Note: The code "Value = ColumnDefaultValue(Column)" is repeated throughout the method
		/// to avoid querying the DB for TYPES not in the if-list for performance sake.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		protected void SetColumnDefaultValue(string schemaName, DataColumn column, DbConnection connection)
		{
			var dataType = column.DataType;
			var value = ColumnDefaultValue(schemaName, column, connection);

			try
			{
				if (dataType == typeof(string))
				{
					if (value != null)
					{
						column.DefaultValue = StrippedEnclosedMatch(value, "'", "'");
					}
				}
				else if (dataType == typeof(decimal))
				{
					if (value != null)
					{
						column.DefaultValue = decimal.Parse(value);
					}
				}
				else if (dataType == typeof(double))
				{
					if (value != null)
					{
						column.DefaultValue = double.Parse(value);
					}
				}
				else if (dataType == typeof(float))
				{
					if (value != null)
					{
						column.DefaultValue = float.Parse(value);
					}
				}
				else if (dataType == typeof(int))
				{
					if (value != null)
					{
						column.DefaultValue = int.Parse(value);
					}
				}
				else if (dataType == typeof(long))
				{
					if (value != null)
					{
						if (value.EndsWith(".", StringComparison.OrdinalIgnoreCase))
						{
							value = value.Substring(0, value.Length - 1);
						}
						column.DefaultValue = long.Parse(value);
					}
				}
				else if (dataType == typeof(short))
				{
					if (value != null)
					{
						column.DefaultValue = short.Parse(value);
					}
				}
				else if (dataType == typeof(byte))
				{
					if (value != null)
					{
						column.DefaultValue = byte.Parse(value);
					}
				}
				else if (dataType == typeof(byte[]))
				{
					if (value == "''" || value == "0x")
					{
						column.DefaultValue = Array.Empty<byte>();
					}
					else if (value != null)
					{
						throw new ApplicationException("Unsupported default value for data type " + column.DataType);
					}
				}
				else if (dataType == typeof(bool))
				{
					if (value != null)
					{
						column.DefaultValue = value == "1";
					}
				}
				else if (dataType == typeof(SqlGeography))
				{
					if (value != null)
					{
						var regex = new Regex(@"CONVERT *\( *\[GEOGRAPHY\] *, *' *(((POINT)|(POLYGON)) +((?:EMPTY)|(?(3)\(\d+(?:\.\d+)? +\d+(?:\.\d+)?\)|(?(4)\( *\( *(\d+(?:\.\d+)?) +(\d+(?:\.\d+)?)(?: *, *\d+(?:\.\d+)? +\d+(?:\.\d+)?){2,}, *\6 +\7 *\) *\))))) *' *\)");
						var match = regex.Match(value.ToUpper(System.Globalization.CultureInfo.InvariantCulture));
						if (match.Success)
						{
							var type = match.Groups[2].Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
							var points = match.Groups[1].Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

							switch (type)
							{
								case "POINT":
									column.DefaultValue = SqlGeography.STPointFromText(new SqlChars(new SqlString(points)), 4326);
									break;
								case "POLYGON":
									column.DefaultValue = SqlGeography.STPolyFromText(new SqlChars(new SqlString(points)), 4326);
									break;
								case "MULTIPOLYGON":
									column.DefaultValue = SqlGeography.STMPolyFromText(new SqlChars(new SqlString(points)), 4326);
									break;
									//we will have more types supported later but not now.
							}
						}
					}
				}
				else if (dataType != typeof(Guid) && dataType != typeof(DateTime) && dataType != typeof(TimeSpan) && dataType != typeof(DateTimeOffset))
				{
					throw new ApplicationException("Unsupported data type " + dataType);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string message =
					"Setting Column Default." + System.Environment.NewLine +
					"Table: " + column.Table.TableName +
					", Column: " + column.ColumnName +
					", Type: " + dataType;

				if (value != null)
				{
					message += ", Proposed Default: (" + value + ")";
				}

				OnAddErrorLine(message);
				throw;
			}
		}

		string ColumnDefaultValue(string schemaName, DataColumn column, DbConnection connection)
		{
			var result = ColumnDefaultValueFromDb(ActualDatabaseNameDuringRegen, schemaName, column.ColumnName, connection, column.Table.TableName);

			result = StrippedEnclosedMatch(result, @"\(", @"\)");
			string sql2005Result = StrippedEnclosedMatch(result, @"\(", @"\)");

			if (sql2005Result != null)
			{
				result = sql2005Result;
			}

			return result;
		}

		protected virtual string SubFolderNameForSchemaClasses
		{
			get { return ""; }
		}

		protected virtual string ActualDatabaseNameDuringRegen
		{
			get { return Db.DatabaseName; }
		}

		string InformationSchemaColumnsReplacement(string dbName)
		{
			return string.Format(Culture.Invariant, @"
{0}.sys.columns c
join {0}.sys.objects o on o.object_id = c.object_id
join {0}.sys.schemas s on o.schema_id = s.schema_id
left join {0}.sys.objects def on c.default_object_id = def.object_id
left join {0}.sys.default_constraints dc on dc.object_id = c.default_object_id", dbName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected string ColumnDefaultValueFromDb(string dbName, string schemaName, string columnName, DbConnection connection, string tableName)
		{
			if (!columnDefaultValuesFromDb.TryGetValue((dbName, schemaName, columnName, tableName), out var value)
				&& !columnDefaultValuesFromDb.TryGetValue((dbName, schemaName, columnName, string.Empty), out value))
			{
				GetColumnDefaultValuesFromDb(dbName, schemaName, columnName, connection, tableName);
				if (!columnDefaultValuesFromDb.TryGetValue((dbName, schemaName, columnName, tableName), out value))
				{
					GetColumnDefaultValuesFromDb(dbName, schemaName, columnName, connection, string.Empty);
					columnDefaultValuesFromDb.TryGetValue((dbName, schemaName, columnName, string.Empty), out value);
				}
			}

			return value;

			void GetColumnDefaultValuesFromDb(string dbName, string schemaName, string columnName, DbConnection connection, string tableName = "")
			{
				var sqlText = "select c.name, dc.definition from " + InformationSchemaColumnsReplacement(dbName) + " where s.name = '" + schemaName + "'";

				if (!string.IsNullOrEmpty(tableName))
				{
					sqlText += $" and o.Type = 'U' and o.name = '{tableName}'";
				}
				else
				{
					sqlText += $"and o.Type IN ('U', 'V') and c.name like @ColumnPrefix + '[_]%' and o.name not like '%{DBHelper.SRDbOfflineTableSuffix}' order by o.type desc, len(o.name)";
				}

				using var cmd = connection.Command(sqlText);
				cmd.AddParameter("ColumnPrefix", SqlDbType.NVarChar, 128, columnName.Split('_')[0]);
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var name = reader.GetString(0);
					var definition = reader.IsDBNull(1) ? "" : reader.GetString(1);
					columnDefaultValuesFromDb[(dbName, schemaName, name, tableName)] = definition;
				}
			}
		}

		readonly Dictionary<(string dbName, string schemaName, string columnName, string tableName), string> columnDefaultValuesFromDb = new ();

		#endregion

		#region Regular Expressions

		protected static string StrippedEnclosedMatch(string value, string left, string right)
		{
			string pattern = "^" + left + "(.*)" + right + "$";
			return InnerMatch(value, pattern);
		}

		protected static string InnerMatch(string value, string pattern)
		{
			string result = null;

			if (value != null)
			{
				Match matchResult = Regex.Match(value, pattern);
				if (matchResult.Success)
				{
					result = matchResult.Groups[1].Value;
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
