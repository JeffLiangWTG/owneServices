using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.Database.ExtendedProperties
{
	public static class ExtProperty
	{
		public enum Class
		{
			Database,
			Table,
			Column,
			Index,
		}

		public static void UpgradeDatabase(DbConnection connection)
		{
			if (!DataUtils.ObjectExists(connection, TableName))
			{
				CreateTable(connection);
			}
			else
			{
				UpgradeTableIfRequired(connection);
			}
		}

		static void CreateTable(DbConnection connection)
		{
			var batch = TableDefinition.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			RunCommands(connection, batch);
		}

		static void UpgradeTableIfRequired(DbConnection connection)
		{
			if (UpgradeRequired(connection))
			{
				var batch = GetUpgradeCommands().ToArray();
				RunCommands(connection, batch);
			}
		}

		static void RunCommands(DbConnection connection, string[] commands)
		{
			foreach (var statement in commands)
			{
				using (var cmd = connection.Command(statement))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		#region Table Definition

		public static string TableName
		{
			get { return StmExtendedPropertySchema.Constants.TableName; }
		}

		public static bool IsTableDefinitionMatched(DbConnection connection, string targetDbName)
		{
			var actualTableDefinition = GetTableDefinition(connection, targetDbName);
			var expected = ExtProperty.TableDefinition;
			return expected.Equals(actualTableDefinition, StringComparison.OrdinalIgnoreCase);
		}

		public static string TableDefinition
		{
			get { return tableDefinition ?? (tableDefinition = GetTableDefinition(Db.Connection, Db.DatabaseName)); }
		}

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static string tableDefinition;

		static string GetTableDefinition(DbConnection connection, string targetDbName)
		{
			if (!DataUtils.ObjectExists(connection, ExtProperty.TableName))
			{
				return ConstTableDefinition;
			}

			var builder = new StringBuilder();

			ColumnsWithDefaults(builder, connection, targetDbName);
			Indexes(builder, connection, targetDbName);
			LockEscalation(builder, connection, targetDbName);
			return builder
				.ToString();
		}

#if DEBUG
		internal
#endif
		static string ConstTableDefinition
		{
			get
			{
				return @"CREATE TABLE STMEXTENDEDPROPERTY (
  SEP_Class VARCHAR(60) DEFAULT '' NOT NULL,
  SEP_DatabaseNameSuffix VARCHAR(128) DEFAULT '' NOT NULL,
  SEP_SchemaName NVARCHAR(128) DEFAULT '' NOT NULL,
  SEP_MajorObjectName NVARCHAR(128) DEFAULT '' NOT NULL,
  SEP_MinorObjectName NVARCHAR(128) DEFAULT '' NOT NULL,
  SEP_Name VARCHAR(200) DEFAULT '' NOT NULL,
  SEP_Value VARCHAR(200) DEFAULT '' NOT NULL,
  SEP_SystemCreateTimeUtc SMALLDATETIME NULL,
  SEP_SystemCreateUser VARCHAR(3) DEFAULT '' NOT NULL,
  SEP_SystemLastEditTimeUtc SMALLDATETIME NULL,
  SEP_SystemLastEditUser VARCHAR(3) DEFAULT '' NOT NULL,
  INDEX NR_RX__SEP_SystemCreateTimeUtc (SEP_SystemCreateTimeUtc)
    INCLUDE (SEP_SystemCreateUser)
    WHERE SEP_SystemCreateTimeUtc IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF),
  INDEX NR_RX__SEP_SystemLastEditTimeUtc (SEP_SystemLastEditTimeUtc)
    INCLUDE (SEP_SystemLastEditUser)
    WHERE SEP_SystemLastEditTimeUtc IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF),
  INDEX NR_UC__SEP_Class_SEP_DatabaseNameSuffix_SEP_SchemaName_SEP_MajorObjectName_SEP_MinorObjectName_SEP_Name
    UNIQUE CLUSTERED (SEP_Class, SEP_DatabaseNameSuffix, SEP_SchemaName, SEP_MajorObjectName, SEP_MinorObjectName, SEP_Name) WITH (ALLOW_PAGE_LOCKS = OFF)
);
ALTER TABLE STMEXTENDEDPROPERTY SET (LOCK_ESCALATION = DISABLE);"; // Direct SQL statememnt.
			}
		}

		static void ColumnsWithDefaults(StringBuilder builder, DbConnection connection, string targetDbName)
		{
			var statement = "";
			var sql = string.Format(
				CultureInfo.InvariantCulture,
				@"
DECLARE
	@stmt nvarchar(max);

SELECT TOP 11
	@stmt = ISNULL(@stmt + @delimiter, '')
		+ '  ' + col.name + ' ' + TypeDefinition.value + DefaultConstraint.value + Nullable.value + ','
FROM
	[{0}].sys.schemas                       AS sch
	JOIN [{0}].sys.tables                   AS tab ON tab.schema_id = sch.schema_id
	JOIN [{0}].sys.columns                  AS col ON col.object_id = tab.object_id
	JOIN [{0}].sys.types                    AS typ ON typ.user_type_id = col.user_type_id
	LEFT JOIN [{0}].sys.default_constraints AS def ON def.object_id = col.default_object_id
	CROSS APPLY
	(
		SELECT value =
			CASE
				WHEN typ.name in (N'varchar') THEN CONCAT(typ.name, '(', col.max_length, ')')
				WHEN typ.name in (N'nvarchar') THEN CONCAT(typ.name, '(', col.max_length / 2, ')')
				WHEN typ.name in (N'smalldatetime') THEN typ.name
				ELSE '<Not supported>'
			END
	) AS TypeDefinition
	CROSS APPLY
	(
		SELECT value =
			CASE
				WHEN col.is_nullable = 1 THEN ' NULL'
				ELSE ' NOT NULL'
			END
	) AS Nullable
	CROSS APPLY
	(
		SELECT value =
			CASE
				WHEN def.definition is NULL THEN ''
				WHEN LEFT(def.definition, 1) = '(' AND RIGHT(def.definition, 1) = ')' THEN ' DEFAULT ' + REPLACE(REPLACE(def.definition, '(', ''), ')', '')
				ELSE ' DEFAULT ' + def.definition
			END
	) AS DefaultConstraint
WHERE 1=1
	AND sch.name = @table_schema
	AND tab.name = @table_name
ORDER BY
	CASE col.name
		WHEN N'SEP_Class'                 THEN 1
		WHEN N'SEP_DatabaseNameSuffix'    THEN 2
		WHEN N'SEP_SchemaName'            THEN 3
		WHEN N'SEP_MajorObjectName'       THEN 4
		WHEN N'SEP_MinorObjectName'       THEN 5
		WHEN N'SEP_Name'                  THEN 6
		WHEN N'SEP_Value'                 THEN 7
		WHEN N'SEP_SystemCreateTimeUtc'   THEN 8
		WHEN N'SEP_SystemCreateUser'      THEN 9
		WHEN N'SEP_SystemLastEditTimeUtc' THEN 10
		WHEN N'SEP_SystemLastEditUser'    THEN 11
	END

SELECT @stmt;
"
				, targetDbName
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@table_schema", SqlDbType.NVarChar, 128, Db.SqlDbOwnerSchema);
				cmd.AddParameter("@table_name", SqlDbType.NVarChar, 128, ExtProperty.TableName);
				cmd.AddParameter("@delimiter", SqlDbType.NVarChar, 10, System.Environment.NewLine); // SQL parameter name

				var value = cmd.ExecuteScalar();
				statement = (value == null || value == DBNull.Value) ? string.Empty : (string)value;
			}

			builder
				.AppendLine(string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0} (", ExtProperty.TableName)) // SQL braced table name
				.AppendLine(statement);
		}

		static void Indexes(StringBuilder builder, DbConnection connection, string targetDbName)
		{
			var statement = "";
			var mainDbName = ((ICurrentDbControl)connection).InitialDatabase;
			var sql = string.Format(
				CultureInfo.InvariantCulture,
				@"
DECLARE
	@stmt nvarchar(max);

SELECT TOP(10)
	@stmt = ISNULL(@stmt + @delimiter, '') + '  INDEX ' + idx.name + IndexType.value + ' (' + KeyColumns.value + ')' + IncludeClause.value + WithClause.value + ','
FROM

	[{0}].sys.schemas      AS sch
	JOIN [{0}].sys.tables  AS tab ON tab.schema_id = sch.schema_id
	JOIN [{0}].sys.indexes AS idx ON idx.object_id = tab.object_id
	CROSS APPLY
	(
		SELECT value =
			CASE
				WHEN idx.is_primary_key = 0 AND idx.type = 2 THEN 
				CASE
					WHEN idx.is_unique = 1 THEN CHAR(10) + CHAR(13) + '    UNIQUE '
					ELSE ''
				END
				ELSE
				CASE
					WHEN idx.is_unique = 1 THEN + CHAR(10) + CHAR(13) + '    UNIQUE ' + idx.type_desc COLLATE database_default
					ELSE ' ' + idx.type_desc
				END
			END
	) AS IndexType
	CROSS APPLY
	(
		SELECT
			value = [{1}].dbo.CLRConcatenateAgg(Data.value, ', ', 0)
		FROM
			(
				SELECT TOP(50)
					value = c.name + Ordering.value
				FROM
					[{0}].sys.index_columns AS ic
					JOIN [{0}].sys.columns  AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
					CROSS APPLY
					(
						SELECT value =
							CASE
								WHEN ic.is_descending_key = 1 THEN ' DESC'
								ELSE ''
							END
					) AS Ordering
				WHERE 1=1
					AND ic.object_id = idx.object_id
					AND ic.index_id = idx.index_id
					AND ic.key_ordinal > 0
				ORDER BY
					ic.key_ordinal
			) AS Data
	) AS KeyColumns
	CROSS APPLY
	(
		SELECT value = 
			CASE
				WHEN idx.allow_page_locks = 0 THEN ' WITH (ALLOW_PAGE_LOCKS = OFF)'
				ELSE ''
			END
	) AS WithClause
	CROSS APPLY
	(
		SELECT value = 
			CASE
				WHEN idx.has_filter = 1 THEN
				(
					SELECT CONCAT(CHAR(10) + CHAR(13) + '    INCLUDE (', c.name, ')') + CHAR(10) + CHAR(13) + '    WHERE ' + REPLACE(REPLACE(SUBSTRING(idx.filter_definition, 2, LEN(idx.filter_definition) - 2), '[', ''), ']', '')
					FROM [{0}].sys.index_columns AS ic
					JOIN [{0}].sys.columns  AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
					WHERE 1=1
						AND ic.object_id = idx.object_id
						AND ic.index_id = idx.index_id
						AND ic.is_included_column = 1
				)
				ELSE ''
			END
	) AS IncludeClause
WHERE 1=1
	AND sch.name = @table_schema
	AND tab.name = @table_name
	AND idx.index_id > 0
ORDER BY idx.name

SELECT @stmt;
"
				, targetDbName
				, mainDbName
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@table_schema", SqlDbType.NVarChar, 128, Db.SqlDbOwnerSchema);
				cmd.AddParameter("@table_name", SqlDbType.NVarChar, 128, ExtProperty.TableName);
				cmd.AddParameter("@delimiter", SqlDbType.NVarChar, 10, string.Format(CultureInfo.InvariantCulture, "{0}", Environment.NewLine)); // SQL parameter name

				var value = cmd.ExecuteScalar();
				statement = (value == null || value == DBNull.Value) ? string.Empty : (string)value;
			}

			builder.AppendLine(statement.TrimEnd(','))
				   .AppendLine(");");
		}

		#endregion // Table Definition

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		static void LockEscalation(StringBuilder builder, DbConnection connection, string targetDbName)
		{
			if (connection.ExecuteScalar<string>($"SELECT COALESCE(MAX(LOCK_ESCALATION_DESC), '') from [{targetDbName}].sys.tables t join [{targetDbName}].sys.schemas s on s.schema_id = t.Schema_ID where t.name = 'STMEXTENDEDPROPERTY' and s.Name = 'DBO'") == "DISABLE") // Direct SQL statements
			{
				builder
					.AppendLine("ALTER TABLE STMEXTENDEDPROPERTY SET (LOCK_ESCALATION = DISABLE);"); // Direct SQL statements
			}
		}

		#region Database

		public static class Database
		{
			public static string Select(DbConnection connection, string propertyName)
			{
				return ExtProperty.Select(connection, ExtProperty.Class.Database, connection.CurrentDatabase, null, null, null, propertyName);
			}

			public static string Select(DbConnection connection, string dbName, string propertyName)
			{
				return ExtProperty.Select(connection, ExtProperty.Class.Database, dbName, null, null, null, propertyName);
			}

			public static void Update(DbConnection connection, string propertyName, string value)
			{
				ExtProperty.Update(connection, ExtProperty.Class.Database, connection.CurrentDatabase, null, null, null, propertyName, value);
			}

			public static void Update(DbConnection connection, string dbName, string propertyName, string value)
			{
				ExtProperty.Update(connection, ExtProperty.Class.Database, dbName, null, null, null, propertyName, value);
			}

			public static void Delete(DbConnection connection, string propertyName)
			{
				ExtProperty.Delete(connection, ExtProperty.Class.Database, connection.CurrentDatabase, null, null, null, propertyName);
			}

			public static void Delete(DbConnection connection, string dbName, string propertyName)
			{
				ExtProperty.Delete(connection, ExtProperty.Class.Database, dbName, null, null, null, propertyName);
			}
		}

		#endregion // Database

		#region Table

		public static class Table
		{
			public static string Select(DbConnection connection, string schemaName, string tableName, string propertyName)
			{
				return ExtProperty.Select(connection, ExtProperty.Class.Table, connection.CurrentDatabase, schemaName, tableName, null, propertyName);
			}

			public static void Update(DbConnection connection, string schemaName, string tableName, string propertyName, string value)
			{
				ExtProperty.Update(connection, ExtProperty.Class.Table, connection.CurrentDatabase, schemaName, tableName, null, propertyName, value);
			}

			public static void Update(DbConnection connection, string dbName, string schemaName, string tableName, string propertyName, string value)
			{
				ExtProperty.Update(connection, ExtProperty.Class.Table, dbName, schemaName, tableName, null, propertyName, value);
			}

			public static void Delete(DbConnection connection, string schemaName, string tableName, string propertyName)
			{
				ExtProperty.Delete(connection, ExtProperty.Class.Table, connection.CurrentDatabase, schemaName, tableName, null, propertyName);
			}

			public static void Delete(DbConnection connection, string dbName, string schemaName, string tableName, string propertyName)
			{
				ExtProperty.Delete(connection, ExtProperty.Class.Table, dbName, schemaName, tableName, null, propertyName);
			}
		}

		#endregion // Table

		#region Column

		public static class Column
		{
			public static string Select(DbConnection connection, string schemaName, string tableName, string columnName, string propertyName)
			{
				return ExtProperty.Select(connection, ExtProperty.Class.Column, connection.CurrentDatabase, schemaName, tableName, columnName, propertyName);
			}

			public static void Update(DbConnection connection, string schemaName, string tableName, string columnName, string propertyName, string value)
			{
				ExtProperty.Update(connection, ExtProperty.Class.Column, connection.CurrentDatabase, schemaName, tableName, columnName, propertyName, value);
			}

			public static void Delete(DbConnection connection, string schemaName, string tableName, string columnName, string propertyName)
			{
				ExtProperty.Delete(connection, ExtProperty.Class.Column, connection.CurrentDatabase, schemaName, tableName, columnName, propertyName);
			}
		}

		#endregion // Column

		#region Index

		public static class Index
		{
			public static string Select(DbConnection connection, string schemaName, string tableName, string indexName, string propertyName)
			{
				return ExtProperty.Select(connection, ExtProperty.Class.Index, connection.CurrentDatabase, schemaName, tableName, indexName, propertyName);
			}

			public static string Select(DbConnection connection, string dbName, string schemaName, string tableName, string indexName, string propertyName)
			{
				return ExtProperty.Select(connection, ExtProperty.Class.Index, dbName, schemaName, tableName, indexName, propertyName);
			}

			public static void Update(DbConnection connection, string schemaName, string tableName, string indexName, string propertyName, string value)
			{
				ExtProperty.Update(connection, ExtProperty.Class.Index, connection.CurrentDatabase, schemaName, tableName, indexName, propertyName, value);
			}

			public static void Update(DbConnection connection, string dbName, string schemaName, string tableName, string indexName, string propertyName, string value)
			{
				ExtProperty.Update(connection, ExtProperty.Class.Index, dbName, schemaName, tableName, indexName, propertyName, value);
			}

			public static void Delete(DbConnection connection, string schemaName, string tableName, string indexName, string propertyName)
			{
				ExtProperty.Delete(connection, ExtProperty.Class.Index, connection.CurrentDatabase, schemaName, tableName, indexName, propertyName);
			}

			public static void Delete(DbConnection connection, string dbName, string schemaName, string tableName, string indexName, string propertyName)
			{
				ExtProperty.Delete(connection, ExtProperty.Class.Index, dbName, schemaName, tableName, indexName, propertyName);
			}
		}

		#endregion // Index

		#region Implementation

		static void CheckParameters(Class classDescription, string schemaName, string majorObjName, string minorObjName, string propertyName)
		{
			switch (classDescription)
			{
				case Class.Database:
					CheckNonBlank(nameof(propertyName), propertyName);
					break;
				case Class.Table:
					CheckNonBlank(nameof(schemaName), schemaName);
					CheckNonBlank(nameof(majorObjName), majorObjName);
					CheckNonBlank(nameof(propertyName), propertyName);
					break;
				case Class.Column:
					CheckNonBlank(nameof(schemaName), schemaName);
					CheckNonBlank(nameof(majorObjName), majorObjName);
					CheckNonBlank(nameof(minorObjName), minorObjName);
					CheckNonBlank(nameof(propertyName), propertyName);
					break;
				case Class.Index:
					CheckNonBlank(nameof(schemaName), schemaName);
					CheckNonBlank(nameof(majorObjName), majorObjName);
					CheckNonBlank(nameof(minorObjName), minorObjName);
					CheckNonBlank(nameof(propertyName), propertyName);
					break;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "ExtendedProperty.Class = [{0}] is not supported.", classDescription.ToString()));
			}
		}

		static void CheckNonBlank(string parameterName, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Parameter '{0}' must have a non-blank value.", parameterName));
			}
		}

		static bool UseDefaults(Class classDescription, string schemaName, string majorObjName, string minorObjName, string propertyName)
		{
			switch (classDescription)
			{
				case Class.Database:
					return 1 == 2
						|| string.IsNullOrWhiteSpace(propertyName)
						;
				case Class.Table:
					return 1 == 2
						|| string.IsNullOrWhiteSpace(schemaName)
						|| string.IsNullOrWhiteSpace(majorObjName)
						|| string.IsNullOrWhiteSpace(propertyName)
						;
				case Class.Column:
					return 1 == 2
						|| string.IsNullOrWhiteSpace(schemaName)
						|| string.IsNullOrWhiteSpace(majorObjName)
						|| string.IsNullOrWhiteSpace(minorObjName)
						|| string.IsNullOrWhiteSpace(propertyName)
						;
				case Class.Index:
					return 1 == 2
						|| string.IsNullOrWhiteSpace(schemaName)
						|| string.IsNullOrWhiteSpace(majorObjName)
						|| string.IsNullOrWhiteSpace(minorObjName)
						|| string.IsNullOrWhiteSpace(propertyName)
						;
				default:
					return false;
			}
		}

		static string Select(DbConnection connection, Class classDescription, string dbName, string schemaName, string majorObjName, string minorObjName, string propertyName)
		{
			if (UseDefaults(classDescription, schemaName, majorObjName, minorObjName, propertyName))
			{
				return null;
			}

			var sep_Class = classDescription.ToString();
			var sep_DatabaseNameSuffix = connection.DbSuffixByName(dbName);
			var sep_SchemaName = (string.IsNullOrWhiteSpace(schemaName)) ? string.Empty : schemaName;
			var sep_MajorObjectName = (string.IsNullOrWhiteSpace(majorObjName)) ? string.Empty : majorObjName;
			var sep_MinorObjectName = (string.IsNullOrWhiteSpace(minorObjName)) ? string.Empty : minorObjName;
			var sep_Name = propertyName;

			var sql = @"-- Select Extended Property
SELECT
	SEP_Value
FROM
	dbo.StmExtendedProperty
WHERE 1=1
	AND SEP_Class              = @SEP_Class
	AND SEP_DatabaseNameSuffix = @SEP_DatabaseNameSuffix
	AND SEP_SchemaName         = @SEP_SchemaName
	AND SEP_MajorObjectName    = @SEP_MajorObjectName
	AND SEP_MinorObjectName    = @SEP_MinorObjectName
	AND SEP_Name               = @SEP_Name
";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@SEP_Class", SqlDbType.VarChar, 60, sep_Class);
				cmd.AddParameterBasedOnDbColumn("@SEP_DatabaseNameSuffix", sep_DatabaseNameSuffix, StmExtendedPropertySchema.SEP_DatabaseNameSuffix);
				cmd.AddParameterBasedOnDbColumn("@SEP_SchemaName", sep_SchemaName, StmExtendedPropertySchema.SEP_SchemaName);
				cmd.AddParameterBasedOnDbColumn("@SEP_MajorObjectName", sep_MajorObjectName, StmExtendedPropertySchema.SEP_MajorObjectName);
				cmd.AddParameterBasedOnDbColumn("@SEP_MinorObjectName", sep_MinorObjectName, StmExtendedPropertySchema.SEP_MinorObjectName);
				cmd.AddParameterBasedOnDbColumn("@SEP_Name", sep_Name, StmExtendedPropertySchema.SEP_Name);

				var result = cmd.ExecuteScalar();

				return (result == DBNull.Value || result == null)
					? null
					: result.ToString();
			}
		}

		static void Update(DbConnection connection, Class classDescription, string dbName, string schemaName, string majorObjName, string minorObjName, string propertyName, string value)
		{
			var dbNameSuffix = connection.DbSuffixByName(dbName);
			CheckParameters(classDescription, schemaName, majorObjName, minorObjName, propertyName);

			var sep_Class = classDescription.ToString();
			var sep_DatabaseNameSuffix = dbNameSuffix;
			var sep_SchemaName = (string.IsNullOrWhiteSpace(schemaName)) ? string.Empty : schemaName;
			var sep_MajorObjectName = (string.IsNullOrWhiteSpace(majorObjName)) ? string.Empty : majorObjName;
			var sep_MinorObjectName = (string.IsNullOrWhiteSpace(minorObjName)) ? string.Empty : minorObjName;
			var sep_Name = propertyName;
			var sep_Value = (string.IsNullOrWhiteSpace(value)) ? string.Empty : value;

			var sql = @"-- Update Extended Property
WITH
	source AS
		(
			SELECT
				SEP_Class              = @SEP_Class              ,
				SEP_DatabaseNameSuffix = @SEP_DatabaseNameSuffix ,
				SEP_SchemaName         = @SEP_SchemaName         ,
				SEP_MajorObjectName    = @SEP_MajorObjectName    ,
				SEP_MinorObjectName    = @SEP_MinorObjectName    ,
				SEP_Name               = @SEP_Name               ,
				SEP_Value              = @SEP_Value
		)
MERGE
	dbo.StmExtendedProperty AS target
USING source ON 1=1
	AND source.SEP_Class              = target.SEP_Class              
	AND source.SEP_DatabaseNameSuffix = target.SEP_DatabaseNameSuffix 
	AND source.SEP_SchemaName         = target.SEP_SchemaName         
	AND source.SEP_MajorObjectName    = target.SEP_MajorObjectName    
	AND source.SEP_MinorObjectName    = target.SEP_MinorObjectName    
	AND source.SEP_Name               = target.SEP_Name               
WHEN MATCHED THEN
	UPDATE SET
		SEP_Value                     = source.SEP_Value,
		SEP_SystemLastEditTimeUtc     = GetUtcDate(),
		SEP_SystemLastEditUser        = @CurrentUser
WHEN NOT MATCHED BY TARGET THEN
	INSERT (SEP_Class, SEP_DatabaseNameSuffix, SEP_SchemaName, SEP_MajorObjectName, SEP_MinorObjectName, SEP_Name, SEP_Value, SEP_SystemCreateTimeUtc, SEP_SystemCreateUser) VALUES
		(source.SEP_Class, source.SEP_DatabaseNameSuffix, source.SEP_SchemaName, source.SEP_MajorObjectName, source.SEP_MinorObjectName, source.SEP_Name, source.SEP_Value, GetUtcDate(), @CurrentUser)
;
";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@SEP_Class", SqlDbType.VarChar, 60, sep_Class);
				cmd.AddParameterBasedOnDbColumn("@SEP_DatabaseNameSuffix", sep_DatabaseNameSuffix, StmExtendedPropertySchema.SEP_DatabaseNameSuffix);
				cmd.AddParameterBasedOnDbColumn("@SEP_SchemaName", sep_SchemaName, StmExtendedPropertySchema.SEP_SchemaName);
				cmd.AddParameterBasedOnDbColumn("@SEP_MajorObjectName", sep_MajorObjectName, StmExtendedPropertySchema.SEP_MajorObjectName);
				cmd.AddParameterBasedOnDbColumn("@SEP_MinorObjectName", sep_MinorObjectName, StmExtendedPropertySchema.SEP_MinorObjectName);
				cmd.AddParameterBasedOnDbColumn("@SEP_Name", sep_Name, StmExtendedPropertySchema.SEP_Name);
				cmd.AddParameterBasedOnDbColumn("@SEP_Value", sep_Value, StmExtendedPropertySchema.SEP_Value);
				cmd.AddParameterBasedOnDbColumn("@CurrentUser", Db.GetCurrentUserOrDefault(), StmExtendedPropertySchema.SEP_SystemLastEditUser);

				cmd.ExecuteNonQuery();
			}
		}

		static void Delete(DbConnection connection, Class classDescription, string dbName, string schemaName, string majorObjName, string minorObjName, string propertyName)
		{
			var dbNameSuffix = connection.DbSuffixByName(dbName);
			CheckParameters(classDescription, schemaName, majorObjName, minorObjName, propertyName);

			var sep_Class = classDescription.ToString();
			var sep_DatabaseNameSuffix = dbNameSuffix;
			var sep_SchemaName = (string.IsNullOrWhiteSpace(schemaName)) ? string.Empty : schemaName;
			var sep_MajorObjectName = (string.IsNullOrWhiteSpace(majorObjName)) ? string.Empty : majorObjName;
			var sep_MinorObjectName = (string.IsNullOrWhiteSpace(minorObjName)) ? string.Empty : minorObjName;
			var sep_Name = propertyName;

			var sql = @"-- Delete Extended Property
DELETE
	dbo.StmExtendedProperty
WHERE 1=1
	AND SEP_Class              = @SEP_Class              
	AND SEP_DatabaseNameSuffix = @SEP_DatabaseNameSuffix 
	AND SEP_SchemaName         = @SEP_SchemaName         
	AND SEP_MajorObjectName    = @SEP_MajorObjectName    
	AND SEP_MinorObjectName    = @SEP_MinorObjectName    
	AND SEP_Name               = @SEP_Name               
";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@SEP_Class", SqlDbType.VarChar, 60, sep_Class);
				cmd.AddParameterBasedOnDbColumn("@SEP_DatabaseNameSuffix", sep_DatabaseNameSuffix, StmExtendedPropertySchema.SEP_DatabaseNameSuffix);
				cmd.AddParameterBasedOnDbColumn("@SEP_SchemaName", sep_SchemaName, StmExtendedPropertySchema.SEP_SchemaName);
				cmd.AddParameterBasedOnDbColumn("@SEP_MajorObjectName", sep_MajorObjectName, StmExtendedPropertySchema.SEP_MajorObjectName);
				cmd.AddParameterBasedOnDbColumn("@SEP_MinorObjectName", sep_MinorObjectName, StmExtendedPropertySchema.SEP_MinorObjectName);
				cmd.AddParameterBasedOnDbColumn("@SEP_Name", sep_Name, StmExtendedPropertySchema.SEP_Name);

				cmd.ExecuteNonQuery();
			}
		}

		#endregion // Implementation

		#region Upgrade Commands

		internal static bool UpgradeRequired(DbConnection connection)
		{
			var sql = @"SELECT CONVERT(BIT, CASE WHEN EXISTS(SELECT TOP 1 NULL FROM sys.objects so JOIN sys.columns sc ON so.[object_id] = sc.[object_id] WHERE so.[type] = 'U' AND so.[name] = 'StmExtendedProperty' AND sc.[name] = 'SEP_SYSTEMCREATETIMEUTC') THEN 0 ELSE 1 END)";
			return connection.ExecuteScalar<bool>(sql);
		}

		internal static IEnumerable<string> GetUpgradeCommands()
		{
			yield return AddAuditColumns();
			yield return AddAuditIndexes();
		}

		internal static string AddAuditColumns()
		{
			var sql = @"-- Add audit fields to [StmExtendedProperty]
IF NOT EXISTS(SELECT TOP 1 NULL FROM sys.objects so JOIN sys.columns sc ON so.[object_id] = sc.[object_id] WHERE so.[type] = 'U' AND so.[name] = 'StmExtendedProperty' AND sc.[name] = 'SEP_SYSTEMCREATETIMEUTC')
BEGIN
	ALTER TABLE [STMEXTENDEDPROPERTY] ADD [SEP_SYSTEMCREATETIMEUTC] SMALLDATETIME NULL;
	ALTER TABLE [STMEXTENDEDPROPERTY] ADD [SEP_SYSTEMCREATEUSER] VARCHAR(3) NOT NULL DEFAULT '';
	ALTER TABLE [STMEXTENDEDPROPERTY] ADD [SEP_SYSTEMLASTEDITTIMEUTC] SMALLDATETIME NULL;
	ALTER TABLE [STMEXTENDEDPROPERTY] ADD [SEP_SYSTEMLASTEDITUSER] VARCHAR(3) NOT NULL DEFAULT '';
END";

			return sql;
		}

		internal static string AddAuditIndexes()
		{
			var sql = @"-- Add audit indexes to [StmExtendedProperty]
IF EXISTS(SELECT TOP 1 NULL FROM sys.objects so JOIN sys.columns sc ON so.[object_id] = sc.[object_id] WHERE so.[type] = 'U' AND so.[name] = 'StmExtendedProperty' AND sc.[name] = 'SEP_SYSTEMCREATETIMEUTC')
AND NOT EXISTS(SELECT TOP 1 NULL FROM sys.indexes where [name] = 'NR_RX__SEP_SYSTEMCREATETIMEUTC')
BEGIN
	CREATE NONCLUSTERED INDEX [NR_RX__SEP_SYSTEMCREATETIMEUTC] ON [STMEXTENDEDPROPERTY] ([SEP_SYSTEMCREATETIMEUTC] ASC)
	INCLUDE ( [SEP_SYSTEMCREATEUSER] )
	WHERE SEP_SYSTEMCREATETIMEUTC IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF);

	CREATE NONCLUSTERED INDEX [NR_RX__SEP_SYSTEMLASTEDITTIMEUTC] ON [STMEXTENDEDPROPERTY] ([SEP_SYSTEMLASTEDITTIMEUTC] ASC)
	INCLUDE ( [SEP_SYSTEMLASTEDITUSER] )
	WHERE SEP_SYSTEMLASTEDITTIMEUTC IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF);
END";

			return sql;
		}
		#endregion
	}
}
