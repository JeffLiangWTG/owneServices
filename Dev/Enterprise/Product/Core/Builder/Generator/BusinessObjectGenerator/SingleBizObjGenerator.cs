using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions;
using Enterprise.Build.Database.Script;
using Enterprise.BusinessObjectGenerator;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Builder.Generator
{
	public partial class SingleBizObjGenerator : AbstractGenerator
	{
		public SingleBizObjGenerator(GeneratorOutputDirectory outputDirectory)
			: base(outputDirectory)
		{
		}

		public SingleBizObjGenerator(string fileNameOfBusinessObject, GeneratorOutputDirectory outputDirectory)
			: this(outputDirectory)
		{
			this.fFileNameOfBusinessObject = fileNameOfBusinessObject;
			this.MasterFileReference = false;
		}

		public SingleBizObjGenerator(string fileNameOfBusinessObject, GeneratorOutputDirectory outputDirectory, bool masterFileReference)
			: this(fileNameOfBusinessObject, outputDirectory)
		{
			this.MasterFileReference = masterFileReference;
		}

		public virtual string[] ListOfFilesToBeGenerated
		{
			get
			{
				return new string[]
				{
						FileNameOfBusinessObjectSchema,
						FileNameOfBusinessObject,
						FileNameOfBusinessObjectValidation,
						FileNameOfBusinessObjectValidationConcreteClass,
						FileNameOfBusinessObjectLookups,
						FileNameOfBusinessObjectLookupsConcreteClass
				};
			}
		}

		#region Generating

		public override void GenerateSpecificFile(string fileName)
		{
			throw new NotSupportedException("SingleBizObjGenerator does not support GenerateSpecificFile(), please use GenerateAll() instead.");
		}

		public override void GenerateAllFilesForSolution(string solutionName)
		{
			throw new NotSupportedException("SingleBizObjGenerator does not support GenerateAllFilesForSolution(), please use GenerateAll() instead.");
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public override void Generate()
		{
			using (Db.DisableSchemaVersionCheck())
			{
				if (ClientSpecificScripts != null)
				{
					Db.Connection.EnsureIsOpen();
					Db.Connection.BeginTransaction();
					isGeneratingWithClientSpecificScripts = true;
				}

				try
				{
					RunClientSpecificTableDropScripts(ClientSpecificScripts, false);
					RunClientSpecificTableCreateScripts(ClientSpecificScripts);
					GenerateAll(outputDirectory);
				}
				finally
				{
					if (ClientSpecificScripts != null)
					{
						Db.Connection.RollbackTransaction();
					}
				}
			}
		}

		bool isGeneratingWithClientSpecificScripts;

#if DEBUG
		internal virtual
#endif
		void GenerateAll(GeneratorOutputDirectory outputDirectory)
		{
			var stopwatch = Stopwatch.StartNew();

			GenerateBizObjSchema(outputDirectory);

			GenerateBizObj(outputDirectory);
			OnAddReportLine("[" + FileNameOfBusinessObject + "] (" + stopwatch.Elapsed + ")");
		}

		void GenerateBizObjSchema(GeneratorOutputDirectory outputDirectory)
		{
			bool regenBizObjSchema = IsRegenRequiredForBizObjSchema();
			if (regenBizObjSchema)
			{
				WriteBusinessObjectSchema(outputDirectory);
			}
		}

		void GenerateBizObj(GeneratorOutputDirectory outputDirectory)
		{
			bool regenBizObj = IsRegenRequiredForBizObj();
			bool regenBizObjValidation = IsRegenRequiredForBizObjValidation();
			bool regenBizObjLookups = IsRegenRequiredForLookups();
			bool regenBizObjLookupsConcreteClass = IsRegenRequiredForLookupsConcreteClass();
			bool regenBizObjValidationConcreteClass = IsRegenRequiredForValidationConcreteClass();

			if (regenBizObj)
			{
				GenerateBusinessObject(outputDirectory);
			}

			if (regenBizObjValidation)
			{
				WriteBusinessObjectValidation(outputDirectory);
			}

			if (regenBizObjValidationConcreteClass)
			{
				WriteBusinessObjectValidationConcreteClass(outputDirectory);
			}

			if (regenBizObjLookups)
			{
				GenerateBusinessObjectLookups(outputDirectory);
			}

			if (regenBizObjLookupsConcreteClass)
			{
				GenerateBusinessObjectLookupsConcreteClass(outputDirectory);
			}

			if (regenBizObj || regenBizObjLookups || regenBizObjLookupsConcreteClass)
			{
				OnAddFileGeneratedLine("Generated any changed files for business object " + FileNameOfBusinessObject);
			}
		}

		#region Is Generation required?

		protected virtual bool IsRegenRequiredForBizObj()
		{
			return FileIsNew(FileNameOfBusinessObject, CodeCollection.AutoBusinessObject);
		}

		protected virtual bool IsRegenRequiredForBizObjSchema()
		{
			return FileIsNew(FileNameOfBusinessObjectSchema, CodeCollection.AutoBusinessObjectSchema);
		}

		protected virtual bool IsRegenRequiredForBizObjValidation()
		{
			return FileIsNew(FileNameOfBusinessObjectValidation, CodeCollection.AutoBusinessObjectValidation);
		}

		protected virtual bool IsRegenRequiredForLookups()
		{
			return FileIsNew(FileNameOfBusinessObjectLookups, CodeCollection.AutoBusinessObjectLookups);
		}

		protected virtual bool IsRegenRequiredForLookupsConcreteClass()
		{
			return FileIsNew(FileNameOfBusinessObjectLookupsConcreteClass);
		}

		protected virtual bool IsRegenRequiredForValidationConcreteClass()
		{
			return FileIsNew(FileNameOfBusinessObjectValidationConcreteClass);
		}

#if DEBUG
		internal
#endif
		bool FileIsNew(string fileName, AutoSourceFile sourceFile = null)
		{
			return !File.Exists(fileName)
				|| !FileIsReadOnly(fileName) && !SourceControl.EnterpriseDatabase.IsFileInSourceControl(fileName) && !SourceControl.EnterpriseDatabase.GetFilesWithPendingChanges().Contains(fileName)
				|| sourceFile != null && sourceFile.IsDifferentToFile(fileName);
		}

		#endregion

		#region Generate Business Object

		void GenerateBusinessObject(GeneratorOutputDirectory outputDirectory)
		{
			try
			{
				outputDirectory.WriteToFile(FileNameOfBusinessObject, CodeCollection.AutoBusinessObject.SourceCode);
				OnAddReportLine("   Generated " + FileNameOfBusinessObject);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string errorMessage = "   CANNOT GENERATE BUSINESS OBJECT: " + ex + System.Environment.NewLine + "File: " + FileNameOfBusinessObject;
				//OnAddErrorLine(errorMessage);
				//throw new GenerateException(errorMessage, ex);
			}
		}

		#endregion

		#region Generate Schema

		void WriteBusinessObjectSchema(GeneratorOutputDirectory outputDirectory)
		{
			try
			{
				outputDirectory.WriteToFile(FileNameOfBusinessObjectSchema, CodeCollection.AutoBusinessObjectSchema.SourceCode);
				OnAddReportLine("   Generated " + FileNameOfBusinessObjectSchema);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string errorMessage = "   CANNOT GENERATE BUSINESS OBJECT SCHEMA: " + ex + System.Environment.NewLine + "File: " + FileNameOfBusinessObjectSchema;
				//OnAddErrorLine(errorMessage);
				//throw new GenerateException(errorMessage, ex);
			}
		}

		#endregion

		#region Generate Validation

		void WriteBusinessObjectValidation(GeneratorOutputDirectory outputDirectory)
		{
			try
			{
				outputDirectory.WriteToFile(FileNameOfBusinessObjectValidation, CodeCollection.AutoBusinessObjectValidation.SourceCode);
				OnAddReportLine("   Generated " + FileNameOfBusinessObjectValidation);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string errorMessage = "   CANNOT GENERATE BUSINESS OBJECT VALIDATION: " + ex + System.Environment.NewLine + "File: " + FileNameOfBusinessObjectValidation;
				//OnAddErrorLine(errorMessage);
				//throw new GenerateException(errorMessage, ex);
			}
		}

		void WriteBusinessObjectValidationConcreteClass(GeneratorOutputDirectory outputDirectory)
		{
			try
			{
				outputDirectory.WriteToFile(FileNameOfBusinessObjectValidationConcreteClass, CodeCollection.AutoBusinessObjectValidation_FirstConcreteClass.SourceCode);
				OnAddReportLine("   Generated " + FileNameOfBusinessObjectValidationConcreteClass);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string errorMessage = "   CANNOT GENERATE BUSINESS OBJECT VALIDATION CONCRETE: " + ex + System.Environment.NewLine + "File: " + FileNameOfBusinessObjectValidationConcreteClass;
				//OnAddErrorLine(errorMessage);
				//throw new GenerateException(errorMessage, ex);
			}
		}

		#endregion

		#region Generate Lookups

		void GenerateBusinessObjectLookups(GeneratorOutputDirectory outputDirectory)
		{
			try
			{
				outputDirectory.WriteToFile(FileNameOfBusinessObjectLookups, CodeCollection.AutoBusinessObjectLookups.SourceCode);
				OnAddReportLine("   Generated " + FileNameOfBusinessObjectLookups);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string errorMessage = "   CANNOT GENERATE BUSINESS OBJECT LOOKUPS: " + ex + System.Environment.NewLine + "File: " + FileNameOfBusinessObjectLookups;
				//OnAddErrorLine(errorMessage);
				//throw new GenerateException(errorMessage, ex);
			}
		}

		void GenerateBusinessObjectLookupsConcreteClass(GeneratorOutputDirectory outputDirectory)
		{
			try
			{
				outputDirectory.WriteToFile(FileNameOfBusinessObjectLookupsConcreteClass, CodeCollection.AutoBusinessObjectLookups_FirstConcreteClass.SourceCode);
				OnAddReportLine("   Generated " + FileNameOfBusinessObjectLookupsConcreteClass);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string errorMessage = "   CANNOT GENERATE BUSINESS OBJECT LOOKUPS CONCRETE: " + ex + System.Environment.NewLine + "File: " + FileNameOfBusinessObjectLookupsConcreteClass;
				//OnAddErrorLine(errorMessage);
				//throw new GenerateException(errorMessage, ex);
			}
		}

		#endregion

		#endregion

		#region Running Client-Specific Db Scripts

		void RunClientSpecificTableCreateScripts(IExtensionObjects clientScripts)
		{
			if (clientScripts != null)
			{
				foreach (var script in clientScripts.GetAllScripts())
				{
					Db.Connection.ExecuteNonQuery(script.CreateScript);
				}

				CreateDependentBaseScripts();
			}
		}

		void RunClientSpecificTableDropScripts(IExtensionObjects clientScripts, bool throwOnError)
		{
			if (clientScripts != null)
			{
				DropDependentBaseScripts(clientScripts.ViewAndRoutineCreationScripts);

				var allScripts = clientScripts.GetAllScripts();
				for (int i = allScripts.Length - 1; i >= 0; i--)
				{
					try
					{
						Db.Connection.ExecuteNonQuery(allScripts[i].DropScript);
					}
					catch (SqlException)
					{
						if (throwOnError)
						{
							throw;
						}
					}
				}
			}
		}

		void CreateDependentBaseScripts()
		{
			if (droppedDependentBaseScriptsInDropOrder != null)
			{
				for (int i = droppedDependentBaseScriptsInDropOrder.Count - 1; i >= 0; i--)
				{
					Db.Connection.ExecuteNonQuery(droppedDependentBaseScriptsInDropOrder[i].Text);
				}

				droppedDependentBaseScriptsInDropOrder = null;
			}
		}

		void DropDependentBaseScripts(ImmutableArray<DatabaseViewAndRoutineCreateScript> clientScripts)
		{
			if (clientScripts != null && clientScripts.Length > 0)
			{
				var candidatesToDrop = GetDependentScriptsToDrop(clientScripts);
				var baseScripts = GetBaseScripts();
				var scriptsToDrop = candidatesToDrop
					.Join(baseScripts, (outer) => outer, (inner) => inner, (outer, inner) => new { outer, inner }, DbScriptComparer.SchemaNameType)
					.ToList()
					;

				new BatchRunner().RunCollectionOfSqlCommands(Db.Connection, scriptsToDrop.Select(s => new KeyValuePair<string, string>(null, s.outer.Text)));
				droppedDependentBaseScriptsInDropOrder = (scriptsToDrop != null && scriptsToDrop.Count > 0) ? scriptsToDrop.Select(s => s.inner).ToList() : null;
			}
			else
			{
				droppedDependentBaseScriptsInDropOrder = null;
			}
		}

		List<IDbScript> GetBaseScripts()
		{
#if DEBUG
			if (NUnit.Framework.TestingState.IsRunningTests && BaseScripts_ForTest != null)
			{
				return BaseScripts_ForTest;
			}
#endif

			return CoreScriptIndex.GetScripts().ToList();
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]

		List<IDbScript> GetDependentScriptsToDrop(ImmutableArray<DatabaseViewAndRoutineCreateScript> clientScripts)
		{
			var dependentScripts = new List<IDbScript>();

			var sql = string.Format(Culture.Invariant, @"
WITH
	cte AS (
			SELECT
				obj_level = 1,
				obj_id    = d.referencing_id,
				obj_type  = o.type
			FROM
				sys.sql_expression_dependencies AS d
				JOIN sys.objects                AS o ON o.object_id = d.referencing_id
			WHERE 1=1
				AND d.is_schema_bound_reference = 1
				AND o.type in ('P', 'V', 'FN', 'IF', 'TF')
				AND d.referenced_id in ({0})
				AND d.referenced_minor_id = 0

			UNION ALL

			SELECT
				obj_level = cte.obj_level + 1,
				obj_id    = d.referencing_id,
				obj_type  = o.type
			FROM
				cte
				JOIN sys.sql_expression_dependencies AS d ON d.referenced_id = cte.obj_id
				JOIN sys.objects                     AS o ON o.object_id = d.referencing_id
			WHERE 1=1
				AND d.is_schema_bound_reference = 1
				AND d.referenced_minor_id = 0
				AND o.type in ('P', 'V', 'FN', 'IF', 'TF')
		)
	, cte_distinct AS (
			SELECT
				obj_id, obj_type
				, obj_level = MAX(obj_level)
			FROM
				cte
			GROUP BY
				obj_id, obj_type
		)
SELECT
	obj_schema = OBJECT_SCHEMA_NAME(o.object_id),
	obj_name   = OBJECT_NAME(o.object_id),
	obj_type   = o.type_desc,
	obj_text   = 
		CASE obj_type
			WHEN 'P'  THEN 'DROP PROCEDURE' -- SQL Stored Procedure
			WHEN 'V'  THEN 'DROP VIEW'      -- View
			WHEN 'FN' THEN 'DROP FUNCTION'  -- SQL scalar function
			WHEN 'IF' THEN 'DROP FUNCTION'  -- SQL inline table-valued function
			WHEN 'TF' THEN 'DROP FUNCTION'  -- SQL table-valued-function
		END + ' [' + OBJECT_SCHEMA_NAME(o.object_id) + '].[' + OBJECT_NAME(o.object_id) + '];'
FROM
	cte_distinct     AS c
	JOIN sys.objects AS o ON o.object_id = c.obj_id
ORDER BY
	obj_level DESC;
"
				, string.Join(", ", clientScripts.Select(s => "OBJECT_ID(N'" + s.ObjectName + "')"))
				);

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					dependentScripts.Add(new DbScript((string)reader["obj_schema"], (string)reader["obj_name"], (string)reader["obj_text"], (string)reader["obj_type"]));
				}
			}

			return dependentScripts;
		}

		IExtensionObjects ClientSpecificScripts
		{
			get
			{
				if (fClientSpecificScripts == null)
				{
					if (IsClientSpecific)
					{
						Assembly clientAssembly = Assembly.Load(ClientSpecificAssemblyName);
						fClientSpecificScripts = ClientHookLoader.Instance.GetDbSchemaExtensionObjects(clientAssembly);
					}
				}
				return fClientSpecificScripts;
			}
		}
		IExtensionObjects fClientSpecificScripts;
		List<IDbScript> droppedDependentBaseScriptsInDropOrder;

		#region Helper classes

		internal class DbScript : IDbScript
		{
			public DbScript(string schemaName, string scriptName, string scriptText, string scriptObjectType)
			{
				this.SchemaName = schemaName;
				this.Name = scriptName;
				this.Text = scriptText;
				this.ObjectType = scriptObjectType;
			}

			public string SchemaName { get; private set; }
			public string Name { get; private set; }
			public string Text { get; private set; }
			public string ObjectType { get; private set; }
		}

		#endregion // Helper classes

		#endregion

		#region CodeCollection

		protected internal virtual AutoBusinessObjectCodeCollection CodeCollection
		{
			get
			{
				if (fCodeCollection == null)
				{
					fCodeCollection = new AutoBusinessObjectCodeCollection(Info);
				}
				return fCodeCollection;
			}
		}

		AutoBusinessObjectCodeCollection fCodeCollection;

		#endregion

		#region BusinessObject Info

		protected string BaseClassFromBusinessObjectCSharpFile(string fileName, string defaultClassName = "EnterpriseBusinessObject")
		{
			const string BaseClassPattern = @"(?<=class \S+\s*:\s*)[\w\.]+";

			string result = null;
			if (File.Exists(fileName))
			{
				result = Regex.Match(File.ReadAllText(fileName), BaseClassPattern).Value;
			}

			return (result != null && result.Trim().Length != 0) ? result : defaultClassName;
		}

		protected virtual BusinessObjectInfo Info
		{
			get
			{
				if (fInfo == null)
				{
					EnsureDirectoryExists(FileNameOfBusinessObject);

					fInfo = new BusinessObjectInfo(
						fileName: FileNameOfBusinessObject
						, isInZArchitectureSolution: IsInZSolution
						, isPersistent: IsPersistent
						, @namespace: NamespaceFromCSharpFile(FileNameOfBusinessObject)
						, namespaceOfSchema: NamespaceFromCSharpFile(FileNameOfBusinessObjectSchema, "Enterprise.ZArchitecture.Schema")
						, baseClassName: BaseClassFromBusinessObjectCSharpFile(FileNameOfBusinessObject)
						, sqlSchemaName: SqlSchemaName
						, table: Table
						, decimalScaleTable: DecimalScaleFromDatabaseSchema
						, foreignKeysTable: ForeignKeysFromDatabaseSchema
						, dateTimeOffsetTable: DateTimeOffsetScaleFromDatabaseSchema
						, refDbCountry: BizObjXmlEntry.RefDbCountry
						, refDbType: BizObjXmlEntry.ReferenceDbType
						, dbTypes: DbTypesFromDatabaseSchema
						, uniqueKeys: UniqueKeyList
						, canForceUpdateNaturalKeyCacheColumns: CanForceUpdateNaturalKeyCacheColumns
						, masterFiles: MasterFilesBusinessObjects
						, pkIndex: GetPkIndexForTable(TableName)
						, indexes: GetUniqueIndexesForTable(TableName)
						, masterFileReference: MasterFileReference
						, preventDelete: BizObjXmlEntry.PreventDelete
						, literalOnlyColumns: GetLiteralOnlyColumns(TableName)
						, nonBlankFilteredIndexColumns: GetNonBlankFilteredIndexColumns(TableName)
						, tables: tables
						, convertZStringToWesternEuropeanCharacters: ConvertZStringToWesternEuropeanCharacters
						);
				}

				return fInfo;
			}
		}

		public virtual DataTable Table
		{
			get
			{
				if (fTable == null)
				{
					fTable = TableFromDatabaseSchema();
					fTable.TableName = TableName;
				}
				return fTable;
			}
		}

		protected virtual bool IsPersistent
		{
			get { return true; }
		}

		protected bool IsInZSolution
		{
			get
			{
				BuildXmlBizOEntry entry = MasterFilesBusinessObjects[TableName];
				return entry?.LivesInZArchitecture ?? false;
			}
		}

		#region TableName / Full TableName, DatabaseName

		public string SqlSchemaName
		{
			get { return sqlSchemaName ?? (sqlSchemaName = GetSqlSchemaName()); }
		}

		string sqlSchemaName;

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		string GetSqlSchemaName()
		{
			string result = null;

			var sql = string.Format(@"
SELECT
	schema_name = s.name
FROM
	{0}.sys.tables       AS t
	JOIN {0}.sys.schemas AS s ON s.schema_id = t.schema_id
WHERE
	t.is_ms_shipped = 0
	AND t.name = @table_name
"
				, (ActualDatabaseNameDuringRegen.StartsWith("[")) ? ActualDatabaseNameDuringRegen : "[" + ActualDatabaseNameDuringRegen + "]"
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@table_name", SqlDbType.NVarChar, 128, TableName);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						result = (string)reader["schema_name"];
					}
				}
			}

			return result ?? Db.SqlDbOwnerSchema;
		}

		public virtual string TableName
		{
			get
			{
				if (tableName == null)
				{
					tableName = TableNameForCSharpFile(FileNameOfBusinessObject);
				}

				return tableName;
			}
		}

		string tableName;

		protected BuildXmlBizOEntryWrapper BizObjXmlEntry
		{
			get
			{
				if (bizObjXmlEntry == null)
				{
					bizObjXmlEntry = new BuildXmlBizOEntryWrapper(BuildXml.Instance.AllBusinessObjects[TableName]);
				}

				return bizObjXmlEntry;
			}
		}

		BuildXmlBizOEntryWrapper bizObjXmlEntry;

		protected override string SubFolderNameForSchemaClasses
		{
			get { return BizObjXmlEntry.SchemaClassFolder; }
		}

		protected override string ActualDatabaseNameDuringRegen
		{
			get
			{
#if DEBUG
				if (NUnit.Framework.TestingState.IsRunningTests && BizObjXmlEntry.ReferenceDbType == RefDbTypeEnum.Single)
				{
					return "[" + RefDbTableNameResolver.DefaultSingleRefDbName + "]";
				}
#endif
				return actualDatabaseNameDuringRegen ?? (actualDatabaseNameDuringRegen = BizObjXmlEntry.GetRegenTimeDatabaseName(base.ActualDatabaseNameDuringRegen));
			}
		}
		string actualDatabaseNameDuringRegen;

		protected virtual string FullTableName
		{
			get
			{
				return BizObjXmlEntry.ReferenceDbType == RefDbTypeEnum.Single ?
					ActualDatabaseNameDuringRegen + "." + SqlSchemaName + "." + RefDatabaseVersionMapHelper.GetActualViewNameFromCombinedViewMappings(TableName) :
					ActualDatabaseNameDuringRegen + "." + SqlSchemaName + "." + TableName;
			}
		}

		protected string FullTableNameOnly
		{
			get
			{
				var fullTableNames = FullTableName.Split('.');
				return fullTableNames[2];
			}
		}

		protected string FullTableDatabaseName
		{
			get
			{
				var fullTableNames = FullTableName.Split('.');
				return fullTableNames[0];
			}
		}

		string TableNameForCSharpFile(string fileName)
		{
			string result = Path.GetFileNameWithoutExtension(fileName);

			if (!BusinessObjectFileNameRegex.IsMatch(result))
			{
				throw new ArgumentException("File names for Business Objects must start with \"Auto\"", fileName);
			}

			return result.Remove(0, 4);
		}

		#endregion

		#region Master Files Business Objects

		protected BuildXmlBizOEntryCollection MasterFilesBusinessObjects
		{
			get
			{
				if (fMasterFilesBusinessObjects == null)
				{
					fMasterFilesBusinessObjects = new BuildXmlBizOEntryCollection();
					foreach (BuildXmlBizOEntry entry in BuildXml.Instance.AllBusinessObjects)
					{
						if (IsMasterFile(entry))
						{
							fMasterFilesBusinessObjects.Add(entry);
						}
					}
				}

				return fMasterFilesBusinessObjects;
			}
		}

		bool IsMasterFile(BuildXmlBizOEntry entry)
		{
			return (entry.LivesInZArchitecture || entry.LivesInMasterFiles || entry.LivesInHRMFiles)
				&& !IsXsdFile(entry.TableName)
				&& !IsDummyBizO(entry.TableName);
		}

		bool IsDummyBizO(string bizObjName)
		{
			return bizObjName.ToLower().IndexOf("dummy") != -1;
		}

		BuildXmlBizOEntryCollection fMasterFilesBusinessObjects;

		#endregion

		#region TableInfo

		public class TableInfo : ITableInfo
		{
			public TableInfo(string tableName)
			{
				Name = tableName;
			}

			public void AddPkIndex(string indexName)
			{
				PkIndex = indexName;
			}

			public void AddIndex(string indexName)
			{
				indexes.Add(indexName);
			}

			public void AddLiteralOnlyColumn(string columnName)
			{
				literalOnlyColumns.Add(columnName);
			}

			public void AddNonBlankFilteredIndexColumn(string columnName)
			{
				nonBlankFilteredIndexColumns.Add(columnName);
			}

			public string PkIndex { get; private set; }

			public string[] Indexes
			{
				get
				{
					var result = indexes.ToArray();
					Array.Sort(result);
					return result;
				}
			}

			public string[] LiteralOnlyColumns
			{
				get { return literalOnlyColumns.ToArray(); }
			}

			public IEnumerable<string> NonBlankFilteredIndexColumns => nonBlankFilteredIndexColumns;

			public readonly string Name;
			readonly HashSet<string> indexes = new HashSet<string>();
			readonly HashSet<string> literalOnlyColumns = new HashSet<string>();
			readonly HashSet<string> nonBlankFilteredIndexColumns = new HashSet<string>();
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void InitTableInfoIfNeeded()
		{
			var sqlTemplate = @"
-- Get unique index names
SELECT
	TableName = tab.name,
	ObjName   = ind.name,
	ObjType   = CASE WHEN ind.is_primary_key = 1 THEN 'PkIndex' ELSE 'Index' END,
	IndexName = null
FROM
	sys.objects       AS tab
	JOIN sys.indexes AS ind ON ind.object_id = tab.object_id
WHERE 1=1
	AND tab.is_ms_shipped = 0
	AND tab.type IN ('U', 'V')
	AND ind.index_id > 0
	AND ind.is_unique != 0
	AND ind.is_unique_constraint = 0

UNION ALL

-- Get column names included into filter part of any statistics/indexes
SELECT DISTINCT
	TableName = tab.name,
	ObjName   = col.name,
	ObjType   = case when s.filter_definition LIKE '%[[]' + col.name + ']<>''''%' then 'NonBlankColumn' else 'LiteralColumn' end,
	IndexName = s.name
FROM
	sys.tables       AS tab
	JOIN sys.columns AS col ON col.object_id = tab.object_id
	JOIN
	(
		SELECT
			s.object_id,
			s.filter_definition,
			s.name
		FROM
			sys.stats AS s
		WHERE
			s.has_filter = 1
			AND s.name NOT LIKE '{0}%'
	) AS s ON s.object_id = col.object_id
		AND s.filter_definition     LIKE '%[[]' + col.name + ']%'
		-- exclude is [NOT] NULL
		AND s.filter_definition NOT LIKE '%[[]' + col.name + '] is NULL%'
		AND s.filter_definition NOT LIKE '%[[]' + col.name + '] is NOT NULL%'
WHERE
	tab.is_ms_shipped = 0

UNION ALL

-- Get column names that must be literalized anyway
SELECT DISTINCT
	TableName = tab.name,
	ObjName   = col.name,
	ObjType   = 'LiteralColumn',
	IndexName = null
FROM
	sys.tables       AS tab
	JOIN sys.columns AS col ON col.object_id = tab.object_id
WHERE
	tab.is_ms_shipped = 0
	AND col.name in ('{1}')

ORDER BY
	TableName, ObjType, ObjName, IndexName
;";

			if (tables == null || clientAssemblyNameUsedWhileRetrievingIndexes != ClientSpecificAssemblyName)
			{
				tables = new Dictionary<string, ITableInfo>();
				TableInfo lastTable = null;

				var sql = string.Format(CultureInfo.InvariantCulture, sqlTemplate, IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX.Replace("_", "[_]"), string.Join("', '", this.fieldsToLiteralize));
				using (var cmd = Db.Connection.Command(sql))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var tableName = reader.GetString(0);
						var objName = reader.GetString(1);
						var objType = reader.GetString(2);
						var indexName = reader.GetValue(3);

						if (lastTable == null || lastTable.Name != tableName)
						{
							lastTable = new TableInfo(tableName);
							tables.Add(tableName.ToUpperInvariant(), lastTable);
						}

						if (objType.Equals("Index", StringComparison.OrdinalIgnoreCase)) // unique index
						{
							lastTable.AddIndex(objName);
						}
						else if (objType.Equals("LiteralColumn", StringComparison.OrdinalIgnoreCase)) // literal only column
						{
							lastTable.AddLiteralOnlyColumn(objName);
							if (indexName is string str && !string.IsNullOrEmpty(str))
							{
								lastTable.AddIndex(str);
							}
						}
						else if (objType.Equals("NonBlankColumn", StringComparison.OrdinalIgnoreCase)) // NonBlank filter column
						{
							lastTable.AddNonBlankFilteredIndexColumn(objName);
							if (indexName is string str && !string.IsNullOrEmpty(str))
							{
								lastTable.AddIndex(str);
							}
						}
						else if (objType.Equals("PkIndex", StringComparison.OrdinalIgnoreCase)) // PK index
						{
							lastTable.AddPkIndex(objName);
						}
					}

					clientAssemblyNameUsedWhileRetrievingIndexes = ClientSpecificAssemblyName;
				}
			}
		}

		IEnumerable<string> fieldsToLiteralize = new string[] { "EI_Status" };

		[ThreadStatic]
		internal static Dictionary<string, ITableInfo> tables;

		[ThreadStatic]
		internal static string clientAssemblyNameUsedWhileRetrievingIndexes;

		protected string GetPkIndexForTable(string targetTableName)
		{
			InitTableInfoIfNeeded();

			tables.TryGetValue(targetTableName.ToUpperInvariant(), out var tableInfo);

			return tableInfo?.PkIndex;
		}

		protected string[] GetUniqueIndexesForTable(string targetTableName)
		{
			InitTableInfoIfNeeded();

			tables.TryGetValue(targetTableName.ToUpperInvariant(), out var tableInfo);
			if (tableInfo == null)
			{
				return Array.Empty<string>();
			}
			else
			{
				return tableInfo.Indexes;
			}
		}

		protected IEnumerable<string> GetNonBlankFilteredIndexColumns(string tableToFind)
		{
			InitTableInfoIfNeeded();
			tables.TryGetValue(tableToFind.ToUpperInvariant(), out var tableInfo);
			if (tableInfo == null)
			{
				return Array.Empty<string>();
			}
			else
			{
				return tableInfo.NonBlankFilteredIndexColumns;
			}
		}

		protected string[] GetLiteralOnlyColumns(string targetTableName)
		{
			InitTableInfoIfNeeded();

			tables.TryGetValue(targetTableName.ToUpperInvariant(), out var tableInfo);
			if (tableInfo == null)
			{
				return Array.Empty<string>();
			}
			else
			{
				return tableInfo.LiteralOnlyColumns;
			}
		}

		#endregion // TableInfo

		#region Getting Table from DB / XSD

		protected DataTable TableFromDatabaseSchema(bool forTest = false)
		{
			var result = new DataTable();

			if (FullTableName != null)
			{
				var retries = 3;
				var sqlText = "SELECT " + string.Join(",", DbTypesFromDatabaseSchema.Keys.ToArray()) + " FROM " + FullTableName + " WHERE 1 = 0";
				while (retries-- > 0)
				{
					try
					{
						if (isGeneratingWithClientSpecificScripts || forTest)
						{
							FillSchema(sqlText, Db.Connection, result);
						}
						else
						{
							using (var adminConnection = Db.NewAdminConnection())
							{
								FillSchema(sqlText, adminConnection, result);
							}
						}
						break;
					}
					catch (SqlException e)
					{
						if (retries > 0 && e.Message.Contains("timeout"))
						{
							continue;
						}
						throw new IOException("Error processing " + FileNameOfBusinessObject + System.Environment.NewLine + "SQL: " + sqlText, e);
					}
				}
			}

			return result;
		}

		void FillSchema(string sqlText, DbConnection connection, DataTable result)
		{
			using (var adapter = connection.DataProviderFactory.NewDataAdapter(sqlText, ((IDbConnectionInternals)connection).InternalDbConnection))
			{
				adapter.SelectCommand.Transaction = ((IDbConnectionInternals)connection).ADOTransaction;
				adapter.FillSchema(result, SchemaType.Source);
			}

			OverrideTableNameIfNeeded(result);

			SetColumnExtendedProperties(result.Columns, connection);
			foreach (DataColumn column in result.Columns)
			{
				SetColumnDefaultValue(SqlSchemaName, column, connection);
				SetColumnIsSensitive(column);
			}
			if (IsBizoFromAView)
			{
				SetColumnIsNullableForViews(result.Columns, connection);
			}
		}

		protected virtual void OverrideTableNameIfNeeded(DataTable result)
		{
		}

		[SuppressMessage("CargoWiseOne", "CW1107:DoNotUseDbConnectionMethodsAnalyzer", Justification = "Baseline")]
		void SetColumnExtendedProperties(DataColumnCollection columns, DbConnection connection)
		{
			var sqlText = string.Format(@"
			SELECT
				col.name,
				col.is_sparse,
				col.is_computed
			FROM
				{0}.sys.columns col
				INNER JOIN {0}.sys.objects o
					ON o.object_id = col.object_id
			WHERE
				o.name = '{1}'",
				ActualDatabaseNameDuringRegen, TableName);

			using var command = connection.Command(sqlText);
			using var reader = command.ExecuteReader();

			while (reader.Read())
			{
				var column = columns[reader["name"].ToString()];

				if (column != null)
				{
					//missing column is STL_PK from RefStlScript (and probably the rest of the table too).
					//It's in (name of Odyssey Dat DB).dbo.RefStlScript_ForSRDbOffline but it's not in [Single Reference Database].sys.columns.
					//I don't know how to reproduce it locally or how I'd fix it on DAT.
					//(Single Reference Database is made and updated when you do a database upgrade and the schema version changes, I think?
					//So maybe the DBs used for DAT are subtly wrong right now?)
					//For now, there are no SPARSE columns in ZZScriptManager.cs, we can assume false.
					//If someone wants to add them in the future, they can run into this problem and fix it (assuming it's still broken).
					// Moved during refactoring. -M12 2023-04-23
					if (reader["is_sparse"] is not DBNull && (bool)reader["is_sparse"])
					{
						column.ExtendedProperties.Add("IsSparse", "Y");
					}

					if (reader["is_computed"] is not DBNull && (bool)reader["is_computed"])
					{
						column.ExtendedProperties.Add(nameof(AutoProperty.IsComputed), "Y");
					}
				}
			}
			reader.Close();
		}

		void SetColumnIsSensitive(DataColumn column)
		{
			var isSensitive = CargoWise.Data.SensitiveColumnsCache.Instance.SensitiveColumns.Contains(column.ColumnName);
			if (isSensitive)
			{
				column.ExtendedProperties.Add("IsSensitive", "Y");
			}
		}

		/// <summary>
		/// Sets AllowDBNull based on the is_nullable value from database metadata.
		/// </summary>
		/// <param name="Column"></param>
		void SetColumnIsNullableForViews(DataColumnCollection columns, DbConnection connection)
		{
			var sqlText = string.Format(@"
				SELECT
					col.name,
					col.is_nullable
				FROM
					{0}.sys.columns col
					INNER JOIN {0}.sys.views vw
						ON vw.object_id = col.object_id
				WHERE
					vw.name = '{1}'",
				FullTableDatabaseName, FullTableNameOnly);

			using var command = connection.Command(sqlText);
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				var column = columns[reader["name"].ToString()];
				if (column != null)
				{
					var isNullable = reader["is_nullable"];
					column.AllowDBNull = isNullable != null && isNullable != DBNull.Value && Convert.ToInt32(isNullable) == 1;
				}
			}
		}

		bool IsBizoFromAView
		{
			get
			{
				if (fIsBizoFromAView == null)
				{
					var sqlText = string.Format(
						"IF exists (SELECT [name] FROM {0}.sys.views WHERE name = '{1}') SELECT 1 ELSE SELECT 0;",
						FullTableDatabaseName, FullTableNameOnly);
					int viewExists = (int)Db.Connection.ExecuteScalar(sqlText);
					fIsBizoFromAView = (viewExists == 1);
				}
				return fIsBizoFromAView.Value;
			}
		}
		bool? fIsBizoFromAView;

		#endregion

		#region Getting Decimal Scale / Foreign Keys / DbTypes

		DataTable dateTimeOffsetScaleFromDatabaseSchema;
		[SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual DataTable DateTimeOffsetScaleFromDatabaseSchema
		{
			get
			{
				if (dateTimeOffsetScaleFromDatabaseSchema == null)
				{
					dateTimeOffsetScaleFromDatabaseSchema = new DataTable();

					if (FullTableName != null)
					{
						string sqlText =
							"SELECT	column_name, datetime_precision " +
							"FROM	" + ActualDatabaseNameDuringRegen + ".information_schema.columns " +
							"WHERE table_name = '" + TableName + "' " +
							"AND data_type in ('datetimeoffset')";

						using (var adapter = Db.Connection.Command(sqlText).NewDataAdapter())
						{
							adapter.Fill(dateTimeOffsetScaleFromDatabaseSchema);
						}
					}
				}

				return dateTimeOffsetScaleFromDatabaseSchema;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual DataTable DecimalScaleFromDatabaseSchema
		{
			get
			{
				if (fDecimalScaleFromDatabaseSchema == null)
				{
					fDecimalScaleFromDatabaseSchema = new DataTable();

					if (FullTableName != null)
					{
						string sqlText =
							"SELECT	column_name, numeric_precision, numeric_scale " +
							"FROM	" + ActualDatabaseNameDuringRegen + ".information_schema.columns " +
							"WHERE table_name = '" + TableName + "' " +
							"AND data_type in ('decimal', 'money', 'bigint')";

						using (var adapter = Db.Connection.Command(sqlText).NewDataAdapter())
						{
							adapter.Fill(fDecimalScaleFromDatabaseSchema);
						}
					}
				}

				return fDecimalScaleFromDatabaseSchema;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected DataTable ForeignKeysFromDatabaseSchema
		{
			get
			{
				if (fForeignKeysFromDatabaseSchema == null)
				{
					fForeignKeysFromDatabaseSchema = new DataTable();

					if (ActualDatabaseNameDuringRegen == Db.DatabaseName && FullTableName != null)
					{
						var sqlText =
							$@"SELECT
									COL_NAME(FKC.Parent_Object_Id, Parent_Column_Id), OBJECT_NAME(FKC.Referenced_Object_Id)
								FROM SYS.FOREIGN_KEYS FK
								JOIN SYS.FOREIGN_KEY_COLUMNS FKC ON FK.Object_Id = FKC.Constraint_Object_Id
								WHERE FK.Parent_Object_Id = OBJECT_ID('{TableName}') AND
								FK.NAME IN
								(
										SELECT FK.NAME
										FROM SYS.FOREIGN_KEYS AS FK
										INNER JOIN SYS.FOREIGN_KEY_COLUMNS AS FKC ON FK.object_id = FKC.Constraint_Object_Id
										WHERE FK.Parent_Object_Id = OBJECT_ID('{TableName}')
										GROUP BY FK.NAME
										HAVING COUNT(FK.NAME) = 1
								)";
						using (var adapter = Db.Connection.Command(sqlText).NewDataAdapter())
						{
							adapter.Fill(fForeignKeysFromDatabaseSchema);
						}
					}
				}

				return fForeignKeysFromDatabaseSchema;
			}
		}

		protected static HashSet<string> UniqueKeyList
		{
			get { return uniqueKeyList ?? (uniqueKeyList = DataUtils.BuildUniqueSingleKeyList()); }
		}

		protected static HashSet<string> CanForceUpdateNaturalKeyCacheColumns
		{
			get { return canForceUpdateNaturalKeyCacheColumns ?? (canForceUpdateNaturalKeyCacheColumns = GetCanForceUpdateNaturalKeyCacheColumns()); }
		}

		static HashSet<string> GetCanForceUpdateNaturalKeyCacheColumns()
		{
			canForceUpdateNaturalKeyCacheColumns = new HashSet<string>();

			var query = $@"
				SELECT
					TableName = tab.name
					, ColumnName = col.name
					, HasFilter = has_filter
					, CONVERT(
						BIT
						, CASE WHEN filter_definition = CONCAT(N'(', QUOTENAME(col.name), N'<>'''')') THEN 1 ELSE 0 END
					) AS OnlyFiltersOutBlanks
				FROM
					sys.tables tab
					INNER JOIN sys.columns col ON col.object_id = tab.object_id
					INNER JOIN sys.index_columns ikey ON ikey.object_id = col.object_id AND ikey.column_id = col.column_id
					INNER JOIN sys.indexes ind ON ind.object_id = tab.object_id AND ind.index_id = ikey.index_id
					INNER JOIN (
						SELECT ic.object_id, index_id
						FROM sys.index_columns ic
						WHERE ic.is_included_column = 0
						GROUP BY ic.object_id, index_id
						HAVING count(*) = 1
					) ski ON ski.object_id = tab.object_id AND ski.index_id = ind.index_id
				WHERE
					tab.is_ms_shipped = 0
					AND ind.is_unique = 1
					AND ikey.is_included_column = 0
					AND (col.name not like '__[_]PK' AND col.name not like '___[_]PK')";

			Db.Connection.ExecuteReader(
				query
				, (reader) =>
				{
					var tableNameColumnName = ((string)reader["TableName"] + "." + (string)reader["ColumnName"]).ToUpperInvariant();
					var hasFilter = (bool)reader["HasFilter"];
					var onlyFiltersOutBlanks = (bool)reader["OnlyFiltersOutBlanks"];

					if (hasFilter && !onlyFiltersOutBlanks)
					{
						canForceUpdateNaturalKeyCacheColumns.Add(tableNameColumnName);
					}
				});

			return canForceUpdateNaturalKeyCacheColumns;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual Dictionary<string, string> DbTypesFromDatabaseSchema
		{
			get
			{
				if (fDbTypesFromDatabaseSchema == null)
				{
					fDbTypesFromDatabaseSchema = new Dictionary<string, string>();

					if (FullTableName != null)
					{
						var sqlText =
							string.Format(
							@"SELECT	c.name ColumnName,
										t.name DbType
							FROM		{0}.sys.types t
							JOIN		{0}.sys.columns c on c.user_type_id = t.user_type_id
							JOIN		{0}.sys.objects o on o.object_id = c.object_id
							where		o.name = '" + FullTableNameOnly + "'", FullTableDatabaseName);

						var schemaTable = new DataTable();

						using (var adapter = Db.Connection.Command(sqlText).NewDataAdapter())
						{
							adapter.Fill(schemaTable);
						}

						foreach (DataRow row in schemaTable.Rows)
						{
							var c = row["ColumnName"].ToString();
							var t = row["DbType"].ToString();
							if (!fDbTypesFromDatabaseSchema.ContainsKey(c))
							{
								fDbTypesFromDatabaseSchema.Add(c, t);
							}
							else
							{
								throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Key already exists in dictionary. Table={0}; Column={1}; Type={2}; SQL=\r\n {3}\r\n", FullTableName, c, t, sqlText));
							}
						}
					}
				}

				return fDbTypesFromDatabaseSchema;
			}
		}

		DataTable fDecimalScaleFromDatabaseSchema;
		DataTable fForeignKeysFromDatabaseSchema;
		Dictionary<string, string> fDbTypesFromDatabaseSchema;

		[ThreadStatic]
		static HashSet<string> uniqueKeyList;

		[ThreadStatic]
		static HashSet<string> canForceUpdateNaturalKeyCacheColumns;

		#endregion

		readonly Regex BusinessObjectFileNameRegex = new Regex(@"^Auto\w+$", RegexOptions.IgnoreCase);
		BusinessObjectInfo fInfo;
		DataTable fTable;
		protected bool MasterFileReference;
		protected bool ConvertZStringToWesternEuropeanCharacters;

		#endregion

		#region BusinessFilesInfo

		public BusinessObjectFileNamesInfo BusinessObjectFilesInfo
		{
			private get { return businessObjectFilesInfo; }
			set { businessObjectFilesInfo = value; }
		}

		BusinessObjectFileNamesInfo businessObjectFilesInfo;

		#endregion

		#region File Names		

		// Although it is *nicer* to pull the filenames from the Auto object, this was done here to improve
		// performance when using -BizObj or -BizObjForSolution. Otherwise it would take ages to construct
		// each Auto object just to get the FileNames - and the user might want to cancel anyway		

		public string FileNameOfBusinessObject
		{
			get { return fFileNameOfBusinessObject; }
		}

		public virtual string FileNameOfBusinessObjectSchema
		{
			get
			{
				string result;

				if (IsClientSpecific)
				{
					string directoryName = Path.GetDirectoryName(FileNameOfBusinessObject);
					result = Path.Combine(directoryName, NonAutoFileNameWithSuffix("Schema"));
				}
				else
				{
					string schemasPath = Path.Combine(
						outputDirectory.CWSharedSourceDirectory,
						IsCommonBizo
							? @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\DummySchema"
							: @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\Schemas");
					string fileName = Path.GetFileName(NonAutoFileNameWithSuffix("Schema"));

					if (string.IsNullOrEmpty(SubFolderNameForSchemaClasses))
					{
						result = Path.Combine(schemasPath, fileName);
					}
					else
					{
						result = Path.Combine(Path.Combine(schemasPath, SubFolderNameForSchemaClasses), fileName);
					}
				}
				return result;
			}
		}

		public bool IsCommonBizo
		{
			get
			{
				string fileName = Path.GetFileName(NonAutoFileNameWithSuffix(""));
				fileName = fileName.Remove(fileName.LastIndexOf(".cs"));
				return (new List<string> { "DummyBizo", "DummyPivot", "DummyLogged", "DummyDependentBizo" }).Contains(fileName);
			}
		}

		public string FileNameOfBusinessObjectValidation
		{
			get { return FileNameOfBusinessObject.Insert(FileNameOfBusinessObject.LastIndexOf(".cs"), "Validation"); }
		}

		public string FileNameOfBusinessObjectLookups
		{
			get { return FileNameOfBusinessObject.Insert(FileNameOfBusinessObject.LastIndexOf(".cs"), "Lookups"); }
		}

		public string FileNameOfBusinessObjectLookupsConcreteClass
		{
			get
			{
				return (BusinessObjectFilesInfo != null && !string.IsNullOrEmpty(BusinessObjectFilesInfo.BusinessObjectLookupsConcreteClass))
					? BusinessObjectFilesInfo.BusinessObjectLookupsConcreteClass
					: NonAutoFileNameWithSuffix("Lookups");
			}
		}

		public string FileNameOfBusinessObjectValidationConcreteClass
		{
			get
			{
				return (BusinessObjectFilesInfo != null && !string.IsNullOrEmpty(BusinessObjectFilesInfo.BusinessObjectValidationConcreteClass))
					? BusinessObjectFilesInfo.BusinessObjectValidationConcreteClass
					: NonAutoFileNameWithSuffix("Validation");
			}
		}

		public bool IsClientSpecific
		{
			get { return ClientSpecificAssemblyName != null; }
		}

		string ClientSpecificAssemblyName
		{
			get
			{
				string result = null;
				if (FileNameOfBusinessObject.IndexOf(@"\ClientExtensions\", StringComparison.InvariantCultureIgnoreCase) > 0)
				{
					int index = FileNameOfBusinessObject.IndexOf("ZClient", StringComparison.InvariantCultureIgnoreCase);
					if (index != -1)
					{
						result = FileNameOfBusinessObject.Substring(index, "ZClient".Length + 3);
					}
				}
				return result;
			}
		}

		protected string NonAutoFileNameWithSuffix(string suffix)
		{
			string fileNameWithoutAuto = FileNameOfBusinessObject.Remove(FileNameOfBusinessObject.LastIndexOf("Auto"), 4);
			return fileNameWithoutAuto.Insert(fileNameWithoutAuto.LastIndexOf(".cs"), suffix);
		}

		readonly string fFileNameOfBusinessObject;

		#endregion
	}

	#region FileNameInfo

	public class BusinessObjectFileNamesInfo
	{
		public string BusinessObjectValidationConcreteClass { get; set; }
		public string BusinessObjectLookupsConcreteClass { get; set; }
	}

	#endregion
}

#region Test
#if DEBUG

namespace Enterprise.Builder.Generator
{
	public partial class SingleBizObjGenerator
	{
		public List<IDbScript> BaseScripts_ForTest { get; set; }

		public string[] GetUniqueIndexesForTable_ForTest(string tableName)
		{
			return GetUniqueIndexesForTable(tableName);
		}

		public string[] GetLiteralOnlyColumns_ForTest(string tableName)
		{
			return GetLiteralOnlyColumns(tableName);
		}

		public IEnumerable<string> GetNonBlankFilteredIndexColumns_ForTest(string tableNameForTest)
		{
			return GetNonBlankFilteredIndexColumns(tableNameForTest);
		}

		public IEnumerable<string> FieldsToLiteralize_ForTest
		{
			get { return this.fieldsToLiteralize; }
			set { this.fieldsToLiteralize = value; }
		}

		public void RunClientSpecificTableCreateScripts_ForTest(IExtensionObjects scripts)
		{
			RunClientSpecificTableCreateScripts(scripts);
		}

		public void RunClientSpecificTableDropScripts_ForTest(IExtensionObjects scripts, bool throwOnError)
		{
			RunClientSpecificTableDropScripts(scripts, throwOnError);
		}

		public IEnumerable<KeyValuePair<string, string>> GetLiteralOnlyColumns_ForTest()
		{
			var result = new List<KeyValuePair<string, string>>(tables.Values.Count);
			foreach (var table in tables.Values.Cast<TableInfo>())
			{
				foreach (var column in table.LiteralOnlyColumns)
				{
					result.Add(new KeyValuePair<string, string>(table.Name, column));
				}
			}

			return result;
		}

		public HashSet<string> CanForceUpdateNaturalKeyCacheColumns_ForTest =>
			CanForceUpdateNaturalKeyCacheColumns;

		public void ClearCanForceUpdateNaturalKeyCacheColumns_ForTest() => canForceUpdateNaturalKeyCacheColumns = null;
	}
}
#endif
#endregion
