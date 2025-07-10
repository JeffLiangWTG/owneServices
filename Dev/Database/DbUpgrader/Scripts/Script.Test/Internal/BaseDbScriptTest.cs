using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using Enterprise.Build.Database.Script.Testing.Internal;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Build.Database.Script
{
	abstract class BaseDbScriptTest : TransactionedTestCase
	{
		#region Base Tests (no subclass test implementation required)

		public void TestAllFunctionsShouldNotUseImplicitConversions()
		{
			var implicitConvertionQuerys = new List<string>();
			var scriptExecutionText = GetScriptExecutionTextForTesting();
			if (!ImplicitConversionTestWhiteList.excludedScriptsFromImplicitConversionTest.Contains($"{ScriptToTest.SchemaName}.{ScriptToTest.Name}") && !string.IsNullOrEmpty(scriptExecutionText))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					@"{0};
			SELECT SUBSTRING(stx.[text],(eqs.statement_start_offset / 2) + 1,
			(CASE WHEN eqs.statement_end_offset =-1 
			THEN DATALENGTH(stx.text) 
			ELSE eqs.statement_end_offset 
			END - eqs.statement_start_offset
			 )/ 2 + 1) AS QueryText,
			pl.query_plan AS QueryPlan
			FROM sys.dm_exec_query_stats AS eqs
			     CROSS APPLY sys.dm_exec_text_query_plan(eqs.plan_handle, 
			     eqs.statement_start_offset, 
			     eqs.statement_end_offset) AS pl
			     CROSS APPLY sys.dm_exec_sql_text(eqs.sql_handle) AS stx
			WHERE pl.dbid=DB_ID()
			AND stx.text LIKE '%'+@ScriptExecutionText+'%'
			AND pl.query_plan NOT LIKE '%OBJECT_SCHEMA_NAME (stx.objectid, stx.dbid) %'
			AND pl.query_plan LIKE '%CONVERT_IMPLICIT%'",
					scriptExecutionText);

				try
				{
					using (var cmd = Db.NewAdminConnection().Command(sqlText))
					{
						cmd.AddParameter("ScriptExecutionText", SqlDbType.VarChar, scriptExecutionText);
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.NextResult())
							{
								while (reader.Read())
								{
									var queryText = reader["QueryText"] as string;
									var queryPlan = reader["QueryPlan"] as string;
									Assert($"Implicit Conversions detected. \r\n QueryText:{queryText}\r\n QueryPlan:{queryPlan}", false);
								}
							}
						}
					}
				}
				catch(Exception ex)
				{
					var message = $"Exception was thrown during executing '{scriptExecutionText}',\r\nPlease override method GetScriptExecutionTextForTesting to get a correct scriptExcecutionText manually.\r\nException Message: {ex.Message} \r\n";
					Assert(message, false);
				}
			}
			Assert(true);
		}

		protected virtual string GetScriptExecutionTextForTesting()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				@"with defaultValues as
			(
						  select 'bit' TypeName, '1' DefaultValue
				union all select 'bigint' TypeName, '1' DefaultValue
				union all select 'binary' TypeName, '1' DefaultValue
				union all select 'char' TypeName, '''C''' DefaultValue
				union all select 'date' TypeName, '''1900-01-01''' DefaultValue
				union all select 'datetime' TypeName, '''1900-01-01''' DefaultValue
				union all select 'datetimeoffset' TypeName, '''1900-01-01''' DefaultValue
				union all select 'decimal' TypeName, '0.1' DefaultValue
				union all select 'float' TypeName, '0.1' DefaultValue
				union all select 'int' TypeName, '1' DefaultValue
				union all select 'money' TypeName, '0.1' DefaultValue
				union all select 'nchar' TypeName, 'N''C''' DefaultValue
				union all select 'nvarchar' TypeName, 'N''V''' DefaultValue
				union all select 'numeric' TypeName, '1' DefaultValue
				union all select 'smallint' TypeName, '1' DefaultValue
				union all select 'smalldatetime' TypeName, '''1900-01-01''' DefaultValue
				union all select 'uniqueidentifier' TypeName, '0x' DefaultValue
				union all select 'varbinary' TypeName, '0x' DefaultValue
				union all select 'varchar' TypeName, '''V''' DefaultValue
				union all select 'tinyint' TypeName, '1' DefaultValue
				union all select 'xml' TypeName, '''''' DefaultValue
			)

			select 'select top 1 * from ' + s.name + '.' + o.name + '(' +
			(select coalesce(string_agg(defaultValues.DefaultValue, ', ') within group (order by parameters.parameter_id), '') from sys.parameters 
			join sys.types on types.user_type_id = parameters.user_type_id
			left join defaultValues on TypeName = types.name
			where parameters.object_id = o.object_id)
			+ ')' from sys.objects o
			join sys.schemas s on s.schema_id = o.schema_id
			where s.name not in ('cdc', 'sys')
			and type = 'IF'
			and o.object_id not in (
				select object_id from sys.parameters p
				join sys.types t on t.user_type_id = p.user_type_id and t.name like 'TVP%'
			)
			and s.name = '{0}'
			and o.name = '{1}';",
				ScriptToTest.SchemaName,
				ScriptToTest.Name);

			return TestConnection.ExecuteScalar(sqlText) as string;
		}

		public void TestNoFullyQualifiedTableNames()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DECLARE @ReferencedDatabases varchar(max);

				SELECT
					@ReferencedDatabases =
						ISNULL(@ReferencedDatabases + char(13) + char(10), '') +
						sed.referenced_database_name + '..' + sed.referenced_entity_name
				FROM
					[{0}].sys.sql_expression_dependencies sed
					INNER JOIN [{0}].sys.objects obj ON obj.object_id = sed.referencing_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = obj.schema_id
				WHERE
					sch.name = '{1}'
					AND obj.name = '{2}'
					AND sed.referenced_database_name is not null
					AND sed.referenced_database_name not in ('master', 'msdb')
					AND sed.is_ambiguous = 0;

				SELECT ISNULL(@ReferencedDatabases, '');",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name);

			string referencedDatabases = TestConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("The following objects are referenced with hard-coded database names:", "", referencedDatabases);
		}

		public void TestNoMissingSchemaNamesInStoredObjects()
		{
			//Arrange
			const string sqlText = @"
SELECT 
	OBJECT_NAME(sed.referencing_id) AS ObjectName,
	o.type_desc AS TypeDescription,
	STRING_AGG(sed.referenced_entity_name, ', ') AS EntityNames,
	COUNT(*) AS Qty
FROM 
	sys.sql_expression_dependencies sed
JOIN 
	sys.objects o ON o.object_id = sed.referencing_id
WHERE 
	sed.referenced_schema_name IS NULL 
	AND sed.referenced_entity_name NOT IN ('inserted', 'deleted')
	AND NOT EXISTS (
		SELECT Null 
		FROM tempdb.sys.objects t 
		WHERE t.object_id = sed.referenced_id 
		AND (t.name LIKE '#%' OR t.name LIKE '##%')
	)
	AND OBJECT_NAME(sed.referencing_id) = @scriptToTestNameParameter
GROUP BY 
	OBJECT_NAME(sed.referencing_id), 
	o.type_desc
HAVING 
	STRING_AGG(OBJECT_NAME(sed.referenced_id), ', ') IS NOT NULL
ORDER BY 
	type_desc, 
	ObjectName;";

			var sb = new StringBuilder();

			//Act
			using (var command = TestConnection.Command(sqlText))
			{
				command.AddParameter("@scriptToTestNameParameter", System.Data.SqlDbType.VarChar, ScriptToTest.Name);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var objectName = reader["ObjectName"].ToString();
						var typeDescription = reader["TypeDescription"].ToString();
						var entityNames = reader["EntityNames"].ToString();
						var qty = Convert.ToInt32(reader["Qty"]);

						var result = $"Object Name: {objectName}, Type: {typeDescription}, Entity Names: {entityNames}, Quantity: {qty}";
						sb.AppendLine(result);
					}
				}
			}

			//Assert
			AssertEquals($"Assertion failed: Found stored objects with missing schema names: {sb.ToString()}", sb.Length, 0);
		}

		public virtual void TestScriptIsTheSameAsInTheDatabase()
		{
			string scriptFromDb = GetScriptFromDb();
			string scriptFromCode = ScriptToTest.Text;

			if (IsScriptTheSameAsInTheDatabase(scriptFromDb, scriptFromCode))
			{
				Assert(true);
			}
			else
			{
				string datMessage =
					"The script in the database is not in sync with the source script class.\r\n" +
					"DbUpgrader Script version must be bumped in order to synchronise scripts.";

				string developerMessage =
					"*** When this test fails in a developer PC it doesn't mean it will fail on DAT ***\r\n" +
					"This is just to notify the script embedded in the application doesn't match the version in the database.\r\n\r\n" +
					"To fix it locally, please alter/create the script in your database\r\n" +
					"or select 'Run DB Script Upgrade' on the ediEnterprise Testing menu to synchronise all scripts.";

				string message = string.Format(CultureInfo.InvariantCulture, "\r\n{0}\r\n\r\n", (TestingState.IsRunningOnDAT) ? datMessage : developerMessage);

				AssertXMLEquals(message, scriptFromCode, scriptFromDb);
			}
		}

		public void TestFunctionsCalledByFunctionsAreInlineable()
		{
			var sqlText = @"with ToOptimise as
				(
					select * from sys.objects o where name = @ScriptToTestName
					and type_desc in ('SQL_TABLE_VALUED_FUNCTION', 'SQL_SCALAR_FUNCTION')
					and exists (select null from sys.parameters where parameters.object_id = o.object_id)
					and (select is_inlineable from sys.sql_modules where sql_modules.object_id = o.object_id) = 0
				),
				Calleree as
				(
					select
						object_name(sql_expression_dependencies.referencing_id) Caller,
						object_name(ToOptimise.object_id) Callee from sys.sql_expression_dependencies
						join ToOptimise on ToOptimise.object_id = referenced_id
				)

				select min(Caller), max(Caller), count(*)
				from Calleree
				group by Callee
				order by count(*) desc, Callee";
			(string minCaller, string maxCaller, int calls)? badScript = null;
			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@ScriptToTestName", SqlDbType.NVarChar, 128, ScriptToTest.Name);
				using var reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					badScript = (reader.GetString(0), reader.GetString(1), reader.GetInt32(2));
				}
			}

			var baselineContainsScript = BaseDbScriptTestBaselines.TestFunctionsCalledByFunctionsAreInlineable.Contains(ScriptToTest.Name);

			if (badScript.HasValue)
			{
				var (minCaller, maxCaller, calls) = badScript.Value;
				Assert($"Functions called by other functions with parameters should be inlineable. {ScriptToTest.Name} has {calls} calls from {minCaller} to {maxCaller}", baselineContainsScript);
			}
			else
			{
				Assert($"{ScriptToTest.Name} is fixed and can be removed from BaseDbScriptTestBaselines.TestFunctionsCalledByFunctionsAreInlineable", !baselineContainsScript);
			}
		}

		protected bool IsScriptTheSameAsInTheDatabase(string scriptFromDb, string scriptFromCode)
		{
			string scriptFromDbWithNoSpaces = GetScriptWithUpperCaseCreateKeywordAndNoWhiteSpaces(scriptFromDb);
			string scriptFromCodeWithNoSpaces = GetScriptWithUpperCaseCreateKeywordAndNoWhiteSpaces(scriptFromCode);

			return string.CompareOrdinal(scriptFromCodeWithNoSpaces, scriptFromDbWithNoSpaces) == 0;
		}

		public void TestSchemabindingOption()
		{
			if (!RequiresSchemaBinding)
			{
				Assert(true);
				return;
			}

			switch (ScriptToTest.ObjectType)
			{
				case DbRoutineType.SqlViewTypeDesc:
				case DbRoutineType.SqlFunctionScalarTypeDesc:
				case DbRoutineType.SqlFunctionInlineTypeDesc:
				case DbRoutineType.SqlFunctionTableTypeDesc:
					if (IsSchemaBound())
					{
						Assert(true);
						return;
					}
					AssertIsValidIncompleteSchemaBinding();
					break;
				default:
					Assert(true);
					break;
			}
		}

		bool IsSchemaBound()
		{
			string sqlText = FormattableString.Invariant(
				$@"SELECT is_schema_bound
				   FROM {ScriptDbName}.sys.sql_modules
				   WHERE object_id = OBJECT_ID('{ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}')");
			return Db.Connection.ExecuteScalar<bool>(sqlText);
		}

		protected virtual bool RequiresSchemaBinding => true;

		public void TestObjectWithExplicitTransactionProperlyManagesIt()
		{
			if (beginTranRegex.IsMatch(ScriptToTest.Text))
			{
				var thisType = this.GetType();
				var transactionManagementTestMethodInfo = thisType.GetMethod("TestExplicitTransactionProperlyManaged", Type.EmptyTypes);
				AssertNotNull("TestExplicitTransactionProperlyManaged", transactionManagementTestMethodInfo);
				AssertEquals("TestMethod.IsPublic", true, transactionManagementTestMethodInfo.IsPublic);
				AssertEquals("TestMethod.IsStatic", false, transactionManagementTestMethodInfo.IsStatic);
				AssertEquals("TestMethod.ReturnType", typeof(void), transactionManagementTestMethodInfo.ReturnType);
			}
			else
			{
				AssertEquals("Object has COMMIT statement", false, commitTranRegex.IsMatch(ScriptToTest.Text));
			}
		}

		public void TestTempTableElementsAreInlined()
		{
			var tester = new TempTableInlineTest();
			var sqlText = ScriptToTest.Text;
			var failuresColumn = tester.TestNoAlterTempTableColumns(sqlText);
			var failuresIndex = tester.TestTempTableIndexesAreInline(sqlText);
			var failuresConstraints = tester.TestTempTableConstraintsAreInline(sqlText);
			var testPassed = failuresColumn.Count == 0 && failuresIndex.Count == 0 && failuresConstraints.Count == 0;
			var baselineContainsScript = BaseDbScriptTestBaselines.TestTempTableElementsAreInlined.Contains(TestedTypeHelper.GetTestedType(GetType()));
			if (baselineContainsScript)
			{
				Assert($"{ScriptToTest.Name} is fixed and can be removed from BaseDbScriptTestBaselines.TestTempTableElementsAreInlined", !testPassed);
			}
			else
			{
				Assert($"Temp table columns are not defined inline: {string.Join(", ", failuresColumn)}", failuresColumn.Count == 0);
				Assert($"Temp table indexes are not defined inline: {string.Join(", ", failuresIndex)}", failuresIndex.Count == 0);
				Assert($"Temp table constraints are not defined inline: {string.Join(", ", failuresConstraints)}", failuresConstraints.Count == 0);
			}
		}

		static readonly Regex beginTranRegex = new Regex(@"\bBEGIN\s+TRAN(SACTION)?\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex commitTranRegex = new Regex(@"\bCOMMIT\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		string GetScriptWithUpperCaseCreateKeywordAndNoWhiteSpaces(string script)
		{
			var result = Regex.Replace(script, @"(?<=(^|\n)\s*)\bCREATE\b", "CREATE", RegexOptions.IgnoreCase);
			result = Regex.Replace(result, @"\s+", "");

			return result;
		}

		protected string GetScriptFromDb(DbConnection dbConnection = null)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"IF object_id('[{0}].[{1}].[{2}]') is not null EXEC [{0}].sys.sp_helptext '[{1}].[{2}]'",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name);
			StringBuilder dbScriptBuilder = new StringBuilder();

			using (DbCommand cmd = dbConnection == null ? Db.Connection.Command(sqlText) : dbConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					dbScriptBuilder.Append(reader.GetString(0));
				}
			}

			return dbScriptBuilder.ToString();
		}

		void AssertIsValidIncompleteSchemaBinding()
		{
			HashSet<string> allReferencedEntityNameSet = new HashSet<string>();
			HashSet<string> newReferencedEntityNameSet = new HashSet<string>() { ScriptToTest.Name };
			// 1. find all referenced functions or views
			while (newReferencedEntityNameSet.Count > 0)
			{
				HashSet<string> newCopySet = new HashSet<string>();
				newCopySet.UnionWith(newReferencedEntityNameSet);
				foreach (var newReferencedEntityName in newCopySet)
				{
					HashSet<string> referencedEntityNameSet = GetReferenceDetailOfFunctionOrView(newReferencedEntityName);

					newReferencedEntityNameSet.UnionWith(referencedEntityNameSet.Except(allReferencedEntityNameSet));
					allReferencedEntityNameSet.UnionWith(newReferencedEntityNameSet);
					newReferencedEntityNameSet.Remove(newReferencedEntityName);
				}
			}

			// 2. 2788 / 2720 / partial 4513 is valid. The others is invalid.
			HashSet<string> allErrorCodeSet = new HashSet<string>();
			string errorMessage = "";
			string partialValidNoSchemaBindingErrorMessage = "";
			foreach (var referencedEntityName in allReferencedEntityNameSet)
			{
				var (errorCode, incompleteSchemaBindingDetail) = GetIncompleteSchemaBindingDetail(referencedEntityName);

				if (!string.IsNullOrEmpty(errorCode))
				{
					allErrorCodeSet.Add(errorCode);
					if (PartialValidNoSchemaBindingErrorCodeList().Contains(errorCode))
					{
						partialValidNoSchemaBindingErrorMessage += $" {incompleteSchemaBindingDetail} \n";
						continue;
					}

					if (!ValidNoSchemaBindingErrorCodeList().Contains(errorCode))
					{
						errorMessage += $" {incompleteSchemaBindingDetail} \n";
					}
				}
			}

			if (allErrorCodeSet.IsNullOrEmpty())
			{
				Assert(true);
				return;
			}

			allErrorCodeSet.ExceptWith(PartialValidNoSchemaBindingErrorCodeList());
			// all errorCodes are: PartialValid
			if (allErrorCodeSet.IsNullOrEmpty())
			{
				Assert(partialValidNoSchemaBindingErrorMessage, false);
				return;
			}

			// some errorCodes are not: Valid and PartialValid
			allErrorCodeSet.ExceptWith(ValidNoSchemaBindingErrorCodeList());
			if (!allErrorCodeSet.IsNullOrEmpty())
			{
				Assert(errorMessage, false);
				return;
			}

			Assert(errorMessage, string.IsNullOrEmpty(errorMessage));
		}

		protected (string, string) GetIncompleteSchemaBindingDetail(string objectName)
		{
			var errorCode = "";
			var incompleteSchemaBindingDetail = "";
			var noSchemaBindingObjectName = "";
			var noSchemaBindingObjectAlterDef = "";

			string sqlText = string.Format(@"
select modules.object_id objectId, o.name, modules.definition, replace(replace(modules.definition, 'CREATE FUNCTION', 'ALTER FUNCTION'), 'RETURNS TABLE', 'RETURNS TABLE WITH SCHEMABINDING') alterDef
from {0}.sys.sql_modules modules
join {0}.sys.objects o on modules.object_id = o.object_id
    join {0}.sys.schemas s on s.schema_id = o.schema_id and s.name = 'dbo'
where o.type_desc in ('SQL_SCALAR_FUNCTION', 'SQL_INLINE_TABLE_VALUED_FUNCTION', 'SQL_TABLE_VALUED_FUNCTION')
  and modules.is_schema_bound = 0
  and o.name = '{1}'
  and modules.definition like '%RETURNS TABLE%'
  and modules.definition not like '%CDC%'

union

select modules.object_id objectId, o.name, modules.definition, replace(modules.definition, 'CREATE VIEW {1}','ALTER VIEW {1} WITH SCHEMABINDING') alterDef
from {0}.sys.sql_modules modules
join {0}.sys.objects o on modules.object_id = o.object_id
    join {0}.sys.schemas s on s.schema_id = o.schema_id and s.name = 'dbo'
where o.type_desc in ('VIEW')
  and modules.is_schema_bound = 0
  and o.name = '{1}'
  and modules.definition like '%CREATE VIEW%'
  and modules.definition not like '%CDC%'", ScriptDbName, objectName);

			Db.Connection.ExecuteReader(sqlText, reader =>
			{
				noSchemaBindingObjectName = reader["name"].ToString();
				noSchemaBindingObjectAlterDef = reader["alterDef"].ToString();
			});

			if (!string.IsNullOrEmpty(noSchemaBindingObjectAlterDef))
			{
				var blocker = Db.NewExtraConnectionToMainDb();
				try
				{
					blocker.BeginTransaction();
					blocker.ExecuteNonQuery(noSchemaBindingObjectAlterDef);
					string scriptFromDb = GetScriptFromDb(blocker);
					string scriptFromCode = ScriptToTest.Text;
					blocker.RollbackTransaction();

					if (IsScriptTheSameAsInTheDatabase(scriptFromDb, scriptFromCode))
					{
						Assert(true);
					}
					else
					{
						string message = "\r\nThe script in the database is not schema-binding.\r\n" +
										 "The script must be altered to be schema-binding.\r\n";

						AssertXMLEquals(message, scriptFromCode, scriptFromDb);
					}
				}
				catch (SqlException e)
				{
					errorCode = e.Number.ToString();
					incompleteSchemaBindingDetail = $"Error Message: {errorCode}, Error Object: {noSchemaBindingObjectName}.";
				}
				finally
				{
					blocker.Dispose();
				}
			}

			return (errorCode, incompleteSchemaBindingDetail);
		}

		protected HashSet<string> GetReferenceDetailOfFunctionOrView(string objectName)
		{
			HashSet<string> referencedEntityNameSet = new HashSet<string>();

			string sqlText = string.Format(@"
select distinct d.referenced_id, d.referenced_entity_name, is_schema_bound_reference
from {0}.sys.sql_expression_dependencies d
join {0}.sys.objects oi on d.referencing_id = oi.object_id and d.referenced_schema_name = 'dbo'
join {0}.sys.objects oo on d.referenced_id = oo.object_id and d.referenced_schema_name = 'dbo'
where oo.type_desc in ('SQL_SCALAR_FUNCTION', 'SQL_INLINE_TABLE_VALUED_FUNCTION', 'SQL_TABLE_VALUED_FUNCTION', 'VIEW')
and oi.name = '{1}'", ScriptDbName, objectName);

			Db.Connection.ExecuteReader(sqlText, reader =>
			{
				referencedEntityNameSet.Add(reader["referenced_entity_name"].ToString());
			});
			return referencedEntityNameSet;
		}

		List<string> ValidNoSchemaBindingErrorCodeList() => new List<string> { "2011", "2720", "2788" };

		List<string> PartialValidNoSchemaBindingErrorCodeList() => new List<string> { "4513" };

		#endregion

		#region Assertion Methods

		public static void AssertSameSchema(DataTable expectedSchema, DataTable actualSchema)
			=> AssertSameSchema("Expected identical column order, column names and column types.", expectedSchema, actualSchema);

		public static void AssertSameSchema(string message, DataTable expectedSchema, DataTable actualSchema)
		{
			var expectedColumns = expectedSchema.Columns.Cast<DataColumn>().Select(x => new { x.ColumnName, x.DataType, x.AllowDBNull });
			var actualColumns = actualSchema.Columns.Cast<DataColumn>().Select(x => new { x.ColumnName, x.DataType, x.AllowDBNull });

			AssertContainsExactElementsInExactOrder(message, expectedColumns, actualColumns);
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (Db.Connection.CurrentDatabase != Db.DatabaseName)
			{
				Db.Connection.Command("USE " + Db.DatabaseName + ";").ExecuteNonQuery();
			}
		}

		protected abstract string ScriptDbName { get; }

		protected BaseDbScript ScriptToTest
		{
			get { return scriptToTest ?? (scriptToTest = (BaseDbScript)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()))); }
		}

		BaseDbScript scriptToTest;

		protected string GetHashString(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
			{
				sb.Append(b.ToString("X2"));
			}

			return sb.ToString();
		}

		byte[] GetHash(string inputString)
		{
			using HashAlgorithm algorithm = SHA256.Create();
			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		#endregion
	}
}

