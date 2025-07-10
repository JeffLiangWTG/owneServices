using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.BusinessObjectGenerator;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Builder.Generator
{
	public class AutoSchemaGenerator : BizObjGenerator
	{
		public AutoSchemaGenerator(GeneratorOutputDirectory outputDirectory)
			: base(outputDirectory)
		{
		}

		#region Generating

		public override void Generate()
		{
			OnAddReportLine("[" + FileName + "]");

			if (IsRegenRequired())
			{
				try
				{
					outputDirectory.WriteToFile(FileName, AutoSchema.SourceCode);
					OnAddReportLine("   Generated " + FileName);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					string errorMessage = "   CANNOT GENERATE AUTO SCHEMA: " + ex + System.Environment.NewLine + "File: " + FileName;
					OnAddErrorLine(errorMessage);
					throw new GenerateException(errorMessage, ex);
				}
			}
			else
			{
				OnAddReportLine("   C# Auto Schema file has no changes. It doesn't need generation.");
			}
		}

		public override void GenerateSpecificFile(string fileName)
		{
			throw new NotSupportedException("AutoSchemaGenerator does not support GenerateSpecificFile(), please use GenerateAll() instead.");
		}

		public override void GenerateAllFilesForSolution(string solutionName)
		{
			throw new NotSupportedException("AutoSchemaGenerator does not support GenerateAllFilesForSolution(), please use GenerateAll() instead.");
		}

		protected bool IsRegenRequired()
		{
			return AutoSchema.IsDifferentToFile(FileName);
		}

		#region Generate Schema for NonBusinessObject Tables

		public void GenerateSchemaForNonBusinessObjectTables()
		{
			var nonBusinessObjectTablesSqlTemplate = $@"
select
	name
from
	{{0}}.sys.tables t
where
	{PersistantTableCondition}
";

			foreach (string subFolder in DatabaseCommaDelimitedBusinessObjectTableTextList.Keys)
			{
				if (subFolder != RefDbTableNameResolver.SingleRefDatabaseNameSynonymPrefix)
				{
					var dbInfo = DatabaseCommaDelimitedBusinessObjectTableTextList[subFolder];
					var dbName = dbInfo.GetRegenTimeDatabaseName(Db.DatabaseName);
					var nonBusinessObjectTablesSqlText = string.Format(CultureInfo.InvariantCulture, nonBusinessObjectTablesSqlTemplate, dbName, dbInfo.CsvBizObjTableList);

					var nonBusinessObjectTables = ZArchitecture.Core.Utilities.GetDataTableFromQuery(nonBusinessObjectTablesSqlText);

					foreach (DataRow row in nonBusinessObjectTables.Rows)
					{
						var tableName = (string)row[0];

						if (TablesExistInBothOdysseyAndSRDb.Any(x => x == tableName))
						{
							continue;
						}

						if (String.IsNullOrEmpty(subFolder) || ShouldGenerateSchemaForNonMainDbTable(tableName))
						{
							GenerateSingleNonBizObjTableSchema(dbName, subFolder, tableName);
						}
					}
				}
			}
		}

		bool ShouldGenerateSchemaForNonMainDbTable(string tableName)
		{
			return
			(
				String.Compare(tableName, "StmData", StringComparison.OrdinalIgnoreCase) != 0
				&& String.Compare(tableName, "RefDbVersion", StringComparison.OrdinalIgnoreCase) != 0
				&& String.Compare(tableName, "SchemaVersion", StringComparison.OrdinalIgnoreCase) != 0
			);
		}

		readonly string[] TablesExistInBothOdysseyAndSRDb =
		{
			"RefUNLOCOUtcOffset"
		};

		/// <summary>
		/// Made virtual for testing purposes.
		/// </summary>
		protected virtual void GenerateSingleNonBizObjTableSchema(string dbName, string subFolder, string tableName)
		{
			SingleNonBizObjTableSchemaGenerator nonBizObjSchemaGenerator = new SingleNonBizObjTableSchemaGenerator(dbName, subFolder, tableName, outputDirectory);
			nonBizObjSchemaGenerator.Generate();
		}

		#endregion

		#endregion

		#region Auto Schema

		protected AutoSchema AutoSchema
		{
			get
			{
				if (fAutoSchema == null)
				{
					fAutoSchema = new AutoSchema(PersistantTablesColumns, NonPersistantTablesColumns);
				}
				return fAutoSchema;
			}
		}

		protected AutoSchema fAutoSchema;

		#endregion

		#region Non-Persistant (XSD) Tables' Columns

		protected DataTable[] NonPersistantTablesColumns
		{
			get
			{
				if (fNonPersistantTablesColumns == null)
				{
					ArrayList tables = new ArrayList();

					foreach (BuildXmlBizOEntry entry in BuildXml.Instance.AllBusinessObjects)
					{
						string bizObjName = entry.TableName;

						if (IsXsdFile(bizObjName) || IsXmlFile(bizObjName))
						{
							foreach (SingleBizObjGeneratorFromTable xsdGenerator in CreateSingleBizObjXsdGenerators(entry))
							{
								DataTable columnNamesTable = new DataTable(xsdGenerator.Table.TableName);
								columnNamesTable.Columns.Add("Name");

								foreach (DataColumn column in xsdGenerator.Table.Columns)
								{
									columnNamesTable.Rows.Add(new object[] { column.ColumnName });
								}

								tables.Add(columnNamesTable);
							}
						}
					}

					fNonPersistantTablesColumns = (DataTable[])tables.ToArray(typeof(DataTable));
				}

				return fNonPersistantTablesColumns;
			}
		}

		protected DataTable[] fNonPersistantTablesColumns;

		#endregion

		#region Persistant (DB) Tables' Columns

		protected DataTable[] PersistantTablesColumns
		{
			get
			{
				if (fPersistantTablesColumns == null)
				{
					DataTable allResults = ZArchitecture.Core.Utilities.GetDataTableFromQuery(SqlTextForPersistantTableColumns);
					ArrayList tables = SplitTablesAndColumnsDataTableIntoATableBasedArray(allResults);
					fPersistantTablesColumns = (DataTable[])tables.ToArray(typeof(DataTable));
				}

				return fPersistantTablesColumns;
			}
		}

		DataTable[] fPersistantTablesColumns;

		protected string SqlTextForPersistantTableColumns
		{
			get
			{
				if (fSqlTextForPersistantTableColumns == null)
				{
					var query = new StringBuilder();
					const string unionClause = " union ";

					// 1st select = Business Object Tables and Views, 
					// 2nd select = Non Business Object Tables
					// {0} = Each Database Name
					// {1} = List of BusinessObject Tables on each Database
					// {2} = List of Tables which already have a Schema class in the solution
					string sqlTemplate = $@"
select
	o.name as TableName,
	c.name as ColumnName,
	c.column_id as ColumnId
from
	[{{0}}].sys.objects o
	join [{{0}}].sys.columns c on c.object_id = o.object_id
where
	o.type in ('u', 'v')
	and SCHEMA_NAME(schema_id) in ({ValidSchemas})
	and o.name in ({{1}})
	and o.name not in ({ClientOnlyTables})

union

select
	t.name as TableName,
	c.name as ColumnName,
	c.column_id as ColumnId
from
	[{{0}}].sys.tables t
	join [{{0}}].sys.columns c on c.object_id = t.object_id
where
	{PersistantTableCondition}
";

					foreach (var subFolder in DatabaseCommaDelimitedBusinessObjectTableTextList.Keys)
					{
						if (query.Length > 0)
						{
							query.Append(unionClause);
						}

						var dbInfo = DatabaseCommaDelimitedBusinessObjectTableTextList[subFolder];
						var dbName = dbInfo.GetRegenTimeDatabaseName(Db.DatabaseName, false);
						var allSchemas = dbInfo.CsvBizObjTableList + "," + SchemaOnlyViewsCondition;
						if (dbName == RefDbTableNameResolver.SingleRefDatabaseName)
						{
							// select only Business Object Views
							// use bizobjname as the table name
							query.Append(RefDbTableNameResolver.GetSelectBizoViewNameQueryFromDbWithVersionedViewNameList(dbName, allSchemas));
						}
						else
						{
							query.Append(string.Format(sqlTemplate, dbName, allSchemas));
						}
					}

					// Add ORDER BY clause
					query.Append(" order by TableName, ColumnId");

					fSqlTextForPersistantTableColumns = query.ToString();
				}

				return fSqlTextForPersistantTableColumns;
			}
		}

		string fSqlTextForPersistantTableColumns;

		#region DatabaseCommaDelimitedBusinessObjectTableTextList

		protected Dictionary<string, DatabaseBizObjectTableCollection> DatabaseCommaDelimitedBusinessObjectTableTextList
		{
			get
			{
				if (fDatabaseCommaDelimitedBusinessObjectTableTextList == null)
				{
					Dictionary<string, DatabaseBizObjectTableCollection> tempDictionary = new Dictionary<string, DatabaseBizObjectTableCollection>();

					foreach (BuildXmlBizOEntry entry in BuildXml.Instance.AllBusinessObjects)
					{
						BuildXmlBizOEntryWrapper bizObjEntry = new BuildXmlBizOEntryWrapper(entry);
						string subFolderName = bizObjEntry.SchemaClassFolder;

						string bizObjName = entry.TableName;

						if (!IsXsdFile(bizObjName) && !IsXmlFile(bizObjName))
						{
							DatabaseBizObjectTableCollection databaseInfo;

							if (bizObjEntry.ReferenceDbType == RefDbTypeEnum.Single)
							{
								bizObjName = RefDatabaseVersionMapHelper.GetActualViewNameFromCombinedViewMappings(bizObjName);
							}
							if (!tempDictionary.TryGetValue(subFolderName, out databaseInfo))
							{
								databaseInfo = new DatabaseBizObjectTableCollection(bizObjEntry);
								tempDictionary.Add(subFolderName, databaseInfo);
							}

							databaseInfo.AppendBizObjTable(bizObjName);

							foreach (var addInfoEntry in entry.AddInfoEntries)
							{
								databaseInfo.AppendBizObjTable(addInfoEntry.ViewName);
							}
						}
					}

					fDatabaseCommaDelimitedBusinessObjectTableTextList = tempDictionary;
				}

				return fDatabaseCommaDelimitedBusinessObjectTableTextList;
			}
		}

		Dictionary<string, DatabaseBizObjectTableCollection> fDatabaseCommaDelimitedBusinessObjectTableTextList;

		#endregion

		protected ArrayList SplitTablesAndColumnsDataTableIntoATableBasedArray(DataTable tablesAndColumns)
		{
			ArrayList tables = new ArrayList();
			DataTable currentTable = null;

			foreach (DataRow row in tablesAndColumns.Rows)
			{
				string tableName = (string)row[0];
				string columnName = (string)row[1];

				if (currentTable == null || tableName != currentTable.TableName)
				{
					currentTable = new DataTable(tableName);
					currentTable.Columns.Add("ColumnName");
					tables.Add(currentTable);
				}

				currentTable.Rows.Add(new object[] { columnName });
			}

			return tables;
		}

		#endregion

		#region Helper

		string PersistantTableCondition => $@"
	t.is_ms_shipped = 0
	and SCHEMA_NAME(schema_id) in ({ValidSchemas})
	and t.name not in ({ExcludedTablesCondition})
	and t.name not in ({ClientOnlyTables})
	and t.name not like 'Client%'
	and t.name not like 'RptDt%'
	and t.name not in ({{1}})
";

		string ClientOnlyTables
		{
			get
			{
				if (clientOnlyTables == null)
				{
					var tables = BuildXml.Instance.AllBusinessObjects
							.Cast<BuildXmlBizOEntry>()
							.Where(entry => entry.SolutionName.Equals("ZClientEDI", StringComparison.InvariantCultureIgnoreCase))
							.Select(entry => $"'{entry.TableName}'");

					clientOnlyTables = tables.Any() ? string.Join(", ", tables) : "''";
				}

				return clientOnlyTables;
			}
		}
		string clientOnlyTables;

		string ValidSchemas => string.Join(", ", Db.CW1AdditionalSchemas.Select(x => $"'{x}'").Union(new[] { "'dbo'" }));

		string ExcludedTablesCondition
		{
			get
			{
				if (excludedTablesCondition == null)
				{
					var excludedTables = BuildXml.Instance.ExcludedTables
							.Select(x => $"'{x}'");

					excludedTablesCondition = excludedTables.Any() ? string.Join(", ", excludedTables) : "''";
				}

				return excludedTablesCondition;
			}
		}
		string excludedTablesCondition;

		string SchemaOnlyViewsCondition
		{
			get
			{
				if (schemaOnlyViewsCondition == null)
				{
					var schemaOnlyViews = BuildXml.Instance.SchemaOnlyViews
							.Select(x => $"'{x}'");

					schemaOnlyViewsCondition = schemaOnlyViews.Any() ? string.Join(", ", schemaOnlyViews) : "''";
				}

				return schemaOnlyViewsCondition;
			}
		}
		string schemaOnlyViewsCondition;

		#endregion

		public string FileName
		{
			get
			{
				return Path.Combine(outputDirectory.CWSharedSourceDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\AutoEnterpriseSchema.cs");
			}
		}

#if DEBUG
		[TypeFactoryAnnotationMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method called via Reflection from Enterprise.ReflectionTestDeadCodeTest.TestNoDeadCode()")]
		static IEnumerable<string> TypeFactoryAnnotation()
		{
			using (Db.DisposableActionForDbConnection())
			using (var reader = Db.Connection.Command("select name from sys.tables").ExecuteReader())
			{
				while (reader.Read())
				{
					yield return "Enterprise.ZArchitecture.Schema." + (string)reader["name"] + "Schema,Enterprise.ZArchitecture.Schema";
				}
			}
		}
#endif
	}
}
