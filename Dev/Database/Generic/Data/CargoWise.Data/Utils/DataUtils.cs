using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Utils;
using CargoWise.DataProtection;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.SqlServer.Types;
using static System.FormattableString;

namespace CargoWise.Data
{
	/// <summary>
	/// Database and Ado.Net utility methods
	/// </summary>
	public static partial class DataUtils
	{
		#region Get Primary Key Name / Value

		/// <summary>
		/// Assumes there is "1 and only 1" PK column
		/// </summary>
		public static string GetPkNameFromTable(DataTable table)
		{
			Argument.NotNull(table, nameof(table)); // Suggested By ReviewBot

			if (table.PrimaryKey.Length == 0)
			{
				return table.Columns[0].ColumnName;
			}
			else
			{
				return table.PrimaryKey[0].ColumnName;
			}
		}

		public static Guid GetPk(DataRow row)
		{
			Argument.NotNull(row, nameof(row));

			Guid result = Guid.Empty;

			object pK = GetKeyFromRow(row);

			if (pK is Guid)
			{
				result = (Guid)pK;
			}

			return result;
		}

		#endregion

		#region Should Row Be Saved

		public const string NonPersistentRowsExtendedPropertyName = "NonPersistentRowsHash";

		public static bool ShouldRowBeSaved(DataRow row)
		{
			object key = GetPk(row);
			Hashtable nonPersistentRowsHash = (Hashtable)row.Table.ExtendedProperties[NonPersistentRowsExtendedPropertyName];

			return (nonPersistentRowsHash == null || nonPersistentRowsHash[key] == null);
		}

		#endregion

		#region DoesUpdateRowHavePersistentChangesFromOriginal

		public static bool DoesUpdateRowHavePersistentChangesFromOriginal(IApplicationSchemaResolver resolver, DataRow row)
		{
			foreach (DataColumn col in row.Table.Columns)
			{
				if (resolver.SchemaColumnExists(col.ColumnName, row.Table.TableName) && IsRowValueChanged(row, col))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Is row value changed

		public static bool IsRowValueChanged(DataRow row, DataColumn column)
		{
			return !row[column].Equals(row[column, DataRowVersion.Original]);
		}

		#endregion

		#region Get TableName from DB + TableName

		public static string GetTableNameFromDbAndTableName(string fullyQualifiedTableName)
		{
			string result;
			var indexOfLastPeriod = fullyQualifiedTableName.LastIndexOf('.');

			if (indexOfLastPeriod != -1)
			{
				result = fullyQualifiedTableName.Substring(indexOfLastPeriod + 1);
			}
			else
			{
				result = fullyQualifiedTableName;
			}

			return result;
		}

		#endregion

		#region Get Approximate Row Count for Table

		public static long GetApproximateRowCountForTable(DbConnection dbConnection, ITableSchema tableSchema)
		{
			return GetApproximateRowCountForTable(dbConnection, tableSchema.TableName, tableSchema.SqlSchemaName);
		}

		public static long GetApproximateRowCountForTable(DbConnection dbConnection, string tableName, string schemaName = "dbo")
		{
			var sqlText = @"SELECT ISNULL(SUM(rows), 0)
							FROM sys.partitions
							WHERE object_id = OBJECT_ID(@schemaAndTableName) AND index_id < 2";

			using (var cmd = dbConnection.Command(sqlText))
			{
				cmd.AddParameter("@schemaAndTableName", SqlDbType.NVarChar, 4000, $"{schemaName.QuoteName()}.{tableName.QuoteName()}");
				return (long)cmd.ExecuteScalar();
			}
		}

		#endregion

		#region GetConcurrencyPolicy

		public static ConcurrencyPolicy GetConcurrencyPolicy(DataRow row, DataColumn column)
		{
			var result = GetConcurrencyPolicyFromTable(row.Table);
			if (result == null)
			{
				var concurrencyInfoDictionary = (IDictionary)column.ExtendedProperties[typeof(ConcurrencyPolicy)];

				if (concurrencyInfoDictionary != null)
				{
					result = concurrencyInfoDictionary[GetPk(row)] as ConcurrencyPolicy;
				}
			}

			return result ?? ConcurrencyPolicy.Default;
		}

		public static ConcurrencyPolicy GetConcurrencyPolicyFromTable(DataTable table)
		{
			return table.ExtendedProperties[typeof(ConcurrencyPolicy)] as ConcurrencyPolicy;
		}

		#endregion

		#region ShouldCheckConcurrency

		public static bool ShouldCheckConcurrency(this SchemaColumn schemaColumn, DataRow row, DataColumn column)
		{
			var policy = GetConcurrencyPolicy(row, column);

			if (schemaColumn is { IsLargeBinaryOrText: true } && policy == ConcurrencyPolicy.Default)
			{
				policy = ConcurrencyPolicy.Ignore;
			}

			return policy.ShouldCheck(row, column);
		}

		#endregion

		#region Get truncated basic value for database

		public static object GetTruncatedBasicValueForDatabase(object value, SchemaColumn schemaColumn, bool isTableValued = false)
		{
			if (isTableValued && value is ICollection collection)
			{
				var newValues = new List<object>(collection.Count);
				foreach (var item in collection)
				{
					newValues.Add(GetTruncatedBasicValueForDatabase(item, schemaColumn));
				}

				return newValues;
			}

			var result = GetBasicValueForDatabase(value, schemaColumn);
			return TruncateIfNecessary(result, schemaColumn);
		}

		public static object GetBasicValueForDatabase(object value, SchemaColumn schemaColumn)
		{
			object result;

			if (schemaColumn is SchemaBoolColumn && (value is string || value is char))
			{
				result = (value.ToString() == "Y");
			}
			else if (value == null)
			{
				result = DBNull.Value;
			}
			else if (
				value is bool
				|| value is ValueType
				|| value is decimal
				|| value is byte[]
				|| value is string
				|| value is DBNull
				)
			{
				result = value;
			}
			else if (value is SqlGeography sqlGeoValue)
			{
				if (sqlGeoValue.IsNull)
				{
					result = SqlGeography.STPointFromText(new SqlChars("POINT EMPTY"), 4326);
				}
				else
				{
					result = sqlGeoValue;
				}
			}
			else
			{
				throw new ArgumentException(value.GetType().FullName + " is not a valid type for using as an SQL parameter");
			}

			if (schemaColumn is SchemaBoolColumn { IsBitField: true } && result is bool boolResult)
			{
				result = boolResult ? 1 : 0;
			}

			return result;
		}

		public static object TruncateIfNecessary(object value, SchemaColumn schemaColumn)
		{
			object result = value;

			if (schemaColumn is SchemaDecimalColumn decimalColumn && value != DBNull.Value)
			{
				decimal decimalValue;

				switch (value)
				{
					case decimal d:
						decimalValue = d;
						break;

					default:
						decimalValue = decimal.Parse(value.ToString());
						break;
				}

				result = ZTypeUtils.TruncateDecimal(decimalValue, decimalColumn.Scale);
			}
			else if (value is DateTime full && schemaColumn.SqlDbType == SqlDbType.SmallDateTime)
			{
				result = new DateTime(full.Year, full.Month, full.Day, full.Hour, full.Minute, 0, 0);
			}

			return result;
		}

		#endregion

		#region IsRowAccessible

		public static bool IsDataInRowAccessible(DataRow row)
		{
			var rowState = (row != null) ? row.RowState : DataRowState.Unchanged;
			return
				row != null &&
				row.Table.Columns.Count > 0 &&
				rowState != DataRowState.Deleted &&
				rowState != DataRowState.Detached;
		}

		static bool IsProposedDataAccessible(DataRow row)
		{
			return row != null &&
				row.Table.Columns.Count > 0 &&
				row.HasVersion(DataRowVersion.Proposed);
		}

		#endregion

		#region Get Key from Row

		public static object GetKeyFromRow(DataRow row)
		{
			Argument.NotNull(row, nameof(row)); // Suggested By ReviewBot

			object key = null;

			if (IsDataInRowAccessible(row))
			{
				key = row[0];
			}
			else if (IsProposedDataAccessible(row))
			{
				key = row[0, DataRowVersion.Proposed];
			}
			else if (row.HasVersion(DataRowVersion.Original))
			{
				key = row[0, DataRowVersion.Original];
			}

			return key;
		}

		#endregion

		#region SQL Script Helper Methods

		public static string ReplaceSqlLikeWildcard(string sourceString)
		{
			Argument.NotNull(sourceString, nameof(sourceString));

			return sourceString.Replace("_", "[_]").Replace("%", "[%]");
		}

		public static string EscapeSingleQuotes(string sourceString)
		{
			Argument.NotNull(sourceString, nameof(sourceString));

			return sourceString.Replace("'", "''");
		}

		// Modified from http://drizin.io/Removing-comments-from-SQL-scripts/
		public static string StripCommentsFromSql(string sql)
		{
			var everythingExceptNewLines = new Regex("[^\r\n]");
			const string lineComments = @"--(.*?)\r?\n"; // Developer string that is used for Regex
			const string lineCommentsOnLastLine = @"--(.*?)$";
			const string literals = @"('(('')|[^'])*')";
			const string bracketedIdentifiers = @"\[((\]\])|[^\]])* \]";
			const string quotedIdentifiers = @"(\""((\""\"")|[^""])*\"")";

			const string nestedBlockComments = @"/\*
                                 (?>
                                 /\*  (?<LEVEL>)      # On opening push level
                                 |
                                 \*/ (?<-LEVEL>)     # On closing pop level
                                 |
                                 (?! /\* | \*/ ) . # Match any char unless the opening and closing strings
                                 )+                         # /* or */ in the lookahead string
                                 (?(LEVEL)(?!))             # If level exists then fail
                                 \*/"; // Developer string that is used for Regex

			var noComments = Regex.Replace(sql,
				nestedBlockComments + "|" + lineComments + "|" + lineCommentsOnLastLine + "|" + literals + "|" +
				bracketedIdentifiers + "|" + quotedIdentifiers,
				me =>
				{
					if (me.Value.StartsWith("/*", StringComparison.OrdinalIgnoreCase))
					{
						return "";
					}
					if (me.Value.StartsWith("--", StringComparison.OrdinalIgnoreCase))
					{
						return everythingExceptNewLines.Replace(me.Value, "");
					}
					return me.Value;
				},
				RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

			return string.Join("\n", noComments.Split('\n').Select(s => s.TrimEnd()));
		}

		public static string GetBetweenEndExpressionForQueryUsingLikeWithFilteredIndex(string beginExpression, SchemaStringColumn schemaColumn)
		{
			if (string.IsNullOrEmpty(beginExpression))
			{
				throw new ArgumentException(Res.GetString("1501dd0f-3246-453d-8ae4-3de31c6839fe", "The value of the column {0} can't be empty.", schemaColumn.Name));
			}
			if (schemaColumn.IsUnicode)
			{
				throw new ArgumentException(Res.GetString("3C79E5BE-94D5-41D1-8B0D-4E5409774E27", "{0} column is using UNICODE encoding. This method must be used with ASCII encoding column.", schemaColumn.Name));
			}
			if (schemaColumn.MaxLength == beginExpression.Length)
			{
				return beginExpression;
			}
			return beginExpression.Substring(0, beginExpression.Length - 1) + "þ"; // Developer string that is used SQL
		}

		public static string BytesToHexString(byte[] ba)
		{
			if (ba is null)
			{
				return null;
			}

			var hex = new StringBuilder(ba.Length * 2);
			foreach (var b in ba)
			{
				hex.AppendFormat("{0:x2}", b);
			}

			return $"0x{hex}";
		}

		#endregion

		#region GetDataTableFromQuery

		public static DataTable GetDataTableFromQuery(DbConnection conn, string sqlText, params IStructuralEquatable[] cmdParams)
		{
			Argument.NotNullOrEmpty(sqlText, nameof(sqlText));
			Argument.NotNull(conn, nameof(conn)); // Suggested By ReviewBot

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameters(cmdParams);
				return GetDataTableFromCommand(cmd);
			}
		}

		public static DataTable GetDataTableFromCommand(DbCommand cmd)
		{
			Argument.NotNull(cmd, nameof(cmd)); // Suggested By ReviewBot

			var resultTable = new DataTable();

			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(resultTable);
			}

			return resultTable;
		}

		public static IEnumerable<string> GetListOfValuesFromQuery(DbConnection conn, string query)
		{
			Argument.NotNull(conn, nameof(conn)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(query, nameof(query));

			using (var cmd = conn.Command(query))
			{
				return GetListOfValuesFromCommand(cmd);
			}
		}

		public static IEnumerable<string> GetListOfValuesFromQuery(DbConnection connection, string sqlText, Action<DbCommand> parameters)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(sqlText, nameof(sqlText));
			Argument.NotNull(parameters, nameof(parameters));

			var result = new List<string>();
			connection.ExecuteReader(sqlText, parameters, reader => result.Add(reader[0].ToString()));

			return result;
		}

		public static IEnumerable<string> GetListOfValuesFromCommand(DbCommand cmd)
		{
			Argument.NotNull(cmd, nameof(cmd));

			var result = new List<string>();

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(reader[0].ToString());
				}
			}

			return result;
		}

		public static DataSet GetDataSetFromQuery(string commandText)
		{
			if (Db.Connection.IsInTransaction)
			{
				using (var adminConn = Db.NewAdminConnection())
				{
					return GetDataSetFromQuery(adminConn, commandText);
				}
			}

			return GetDataSetFromQuery(Db.Connection, commandText);
		}

		public static DataSet GetDataSetFromQuery(DbConnection connection, string commandText)
		{
			var sqlConnection = ((IDbConnectionInternals)connection).InternalDbConnection;

			using (var adapter = connection.DataProviderFactory.NewDataAdapter(commandText, sqlConnection))
			{
				return GetDataSetFromQuery(adapter);
			}
		}

		public static DataSet GetDataSetFromQuery(DbCommand command)
		{
			using (var adapter = command.DbConnection.DataProviderFactory.NewDataAdapter(command.InternalCommand))
			{
				return GetDataSetFromQuery(adapter);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed to determin SqlClient types")]
		public static DataSet GetDataSetFromQuery(System.Data.Common.DbCommand command)
		{
			System.Data.Common.DbDataAdapter adapter = null;
			if (command is System.Data.SqlClient.SqlCommand sqlCommand)
			{
				adapter = new System.Data.SqlClient.SqlDataAdapter(sqlCommand);
			}
#if NET
			else if (command is Microsoft.Data.SqlClient.SqlCommand sqlCommandMS)
			{
				adapter = new Microsoft.Data.SqlClient.SqlDataAdapter(sqlCommandMS);
			}
#endif
			else
			{
				throw new NotSupportedException($"Unsupported DbCommand type: {command.GetType()}");
			}

			try
			{
				return GetDataSetFromQuery(adapter);
			}
			finally
			{
				adapter?.Dispose();
			}
		}

		static DataSet GetDataSetFromQuery(IDbDataAdapter adapter)
		{
			var dataSet = new DataSet { Locale = CultureInfo.InvariantCulture };
			try
			{
				adapter.FillSchema(dataSet, SchemaType.Source);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				dataSet.Dispose();
				throw;
			}

			return dataSet;
		}

#endregion

		#region Check Ping

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Fale alarm, running ping not opening a file or url")]
		internal static bool CheckPing(string machine)
		{
			Argument.NotNull(machine, nameof(machine));

			bool result = false;

			try
			{
				using (var ping = new Process()) // Fale alarm, running ping not opening a file or url
				{
					var pingStart = new ProcessStartInfo();
					pingStart.CreateNoWindow = true;
					pingStart.FileName = "ping.exe";
					pingStart.Arguments = "-n 1 -w 2000 " + machine; // string for developers only
					pingStart.RedirectStandardOutput = true;
					pingStart.UseShellExecute = false;

					ping.StartInfo = pingStart;
					ping.Start();

					string pingOutput = ping.StandardOutput.ReadToEnd();

					ping.WaitForExit();

					Match packetsReceivedText = Regex.Match(pingOutput, @"Received\s*=\s*(\d+)");
					int packetsReceivedCount = int.Parse(packetsReceivedText.Groups[1].ToString());

					result = (packetsReceivedCount > 0);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = false;
			}

			return result;
		}

		#endregion

		#region Extended Properties

		public static string LoadDbExtendedPropertyOnMainDb(DbConnection connection, string propertyName, bool withNoLock = false)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			var sqlText = $"SELECT value FROM {Db.DatabaseName.QuoteName()}.sys.extended_properties "
				+ (withNoLock ? "WITH (NOLOCK) " : string.Empty)
				+ "WHERE class = 0 AND name = @propertyName;"; // sql statement for developers only
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@propertyName", SqlDbType.NVarChar, 128, propertyName);
				var objectValue = cmd.ExecuteScalar();
				var result = (objectValue == null || objectValue == DBNull.Value) ? null : objectValue.ToString();
				return result;
			}
		}

		public static string LoadDbExtendedProperty(DbConnection connection, string propertyName)
			=> LoadDbExtendedProperty(connection, propertyName, dbName: null, withNoLock: false);

		public static string LoadDbExtendedProperty(DbConnection connection, string propertyName, string dbName)
			=> LoadDbExtendedProperty(connection, propertyName, dbName, withNoLock: false);

		public static string LoadDbExtendedPropertyWithNoLock(DbConnection connection, string propertyName)
			=> LoadDbExtendedProperty(connection, propertyName, dbName: null, withNoLock: true);

		public static string LoadDbExtendedProperty(DbConnection connection, string propertyName, string dbName, bool withNoLock)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			var sqlText = $"SELECT value FROM sys.extended_properties "
				+ (withNoLock ? "WITH (NOLOCK) " : string.Empty)
				+ "WHERE class = 0 AND name = @propertyName;"; // sql statement for developers only
			using (dbName != null ? ((ICurrentDbControl)connection).UseDatabase(dbName) : null)
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@propertyName", SqlDbType.NVarChar, 128, propertyName);
				var objectValue = cmd.ExecuteScalar();
				var result = (objectValue == null || objectValue == DBNull.Value) ? null : objectValue.ToString();
				return result;
			}
		}

		public static void AddDbExtendedProperty(DbConnection connection, string propertyName, string value, string dbName = null)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			using (dbName != null ? ((ICurrentDbControl)connection).UseDatabase(dbName) : null)
			{
				var sql = $@"
if (NOT EXISTS(
		SELECT NULL FROM sys.extended_properties
		WHERE 1=1
			AND class = 0
			AND name = @propertyName
   ))
begin
	EXEC sys.sp_addextendedproperty
		@name = @propertyName, @value = @propertyValue
end
"; // sql statement for developers only

				using (var cmd = connection.Command(sql))
				{
					cmd.AddParameter("@propertyName", SqlDbType.NVarChar, 128, propertyName);
					cmd.AddParameter("@propertyValue", SqlDbType.VarChar, 7000, (value) ?? (object)DBNull.Value);

					cmd.ExecuteNonQuery();
				}
			}
		}

		public static void SaveDbExtendedProperty(DbConnection connection, string propertyName, string value, string dbName = null)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			var sqlText = $@"
IF EXISTS(SELECT null FROM sys.extended_properties WHERE class = 0 AND name = @propertyName)
  EXEC sys.sp_updateextendedproperty @name = @propertyName, @value = @propertyValue;
ELSE
EXEC sys.sp_addextendedproperty @name = @propertyName, @value = @propertyValue;";

			using (dbName != null ? ((ICurrentDbControl)connection).UseDatabase(dbName) : null)
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@propertyName", SqlDbType.NVarChar, 128, propertyName);
				cmd.AddParameter("@propertyValue", SqlDbType.Variant, 8016, (value) ?? (object)DBNull.Value);

				cmd.ExecuteNonQuery();
			}
		}

		public static void DropDbExtendedProperty(DbConnection connection, string propertyName, string dbName = null)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));
			using (dbName != null ? ((ICurrentDbControl)connection).UseDatabase(dbName) : null)
			{
				var sqlText = $@"
IF EXISTS(SELECT null FROM sys.extended_properties WHERE class = 0 AND name = @propertyName)
EXEC sys.sp_dropextendedproperty @name = @propertyName;"; // sql statement for developers only

				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddParameter("@propertyName", SqlDbType.NVarChar, 128, propertyName);
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static string LoadTableExtendedProperty(DbConnection connection, DbSchemaTable table, string propertyName)
		{
			Argument.NotNull(table, nameof(table)); // Suggested By ReviewBot
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			var sql = $@"
SELECT value
FROM
	sys.extended_properties
WHERE 1=1
	AND class = 1
	AND major_id = OBJECT_ID(@fullName, N'U')
	AND minor_id = 0
	AND name = @propertyName
"; // sql statement for developers only

			using (((ICurrentDbControl)connection).UseDatabase(table.DatabaseName))
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@fullName", SqlDbType.NVarChar, 128, table.ToString());
				cmd.AddParameter("@propertyName", SqlDbType.NVarChar, 128, propertyName);

				var value = cmd.ExecuteScalar();

				return (value == null || value == DBNull.Value) ? null : value.ToString();
			}
		}

		public static void AddTableExtendedProperty(DbConnection connection, DbSchemaTable table, string propertyName, string value)
		{
			Argument.NotNull(table, nameof(table)); // Suggested By ReviewBot
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			var sql = $@"
if (NOT EXISTS(
		SELECT NULL FROM sys.extended_properties
		WHERE 1=1
			AND class = 1
			AND major_id = OBJECT_ID(@fullName, N'U')
			AND minor_id = 0
			AND name = @propertyName
	))
begin
	EXEC sys.sp_addextendedproperty
		@name        = @propertyName, @value      = @propertyValue
		,@level0type = N'SCHEMA'    , @level0name = @schemaName
		,@level1type = N'TABLE'     , @level1name = @tableName
end
"; // sql statement for developers only

			using (((ICurrentDbControl)connection).UseDatabase(table.DatabaseName))
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@fullName", SqlDbType.NVarChar, 128, table.ToString());
				cmd.AddParameter("@propertyName", SqlDbType.NVarChar, 128, propertyName);
				cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, table.SchemaName);
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, table.TableName);
				cmd.AddParameter("@propertyValue", SqlDbType.VarChar, 7000, (value) ?? (object)DBNull.Value);

				cmd.ExecuteNonQuery();
			}
		}

		public static void UpdateTableExtendedProperty(DbConnection connection, DbSchemaTable table, string propertyName, string value)
		{
			Argument.NotNull(table, nameof(table)); // Suggested By ReviewBot
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			var sql = $@"
if (EXISTS(
		SELECT NULL FROM sys.extended_properties
		WHERE 1=1
			AND class = 1
			AND major_id = OBJECT_ID(@fullName, N'U')
			AND minor_id = 0
			AND name = @propertyName
	))
begin
	EXEC sys.sp_updateextendedproperty
		@name        = @propertyName, @value      = @propertyValue
		,@level0type = N'SCHEMA'    , @level0name = @schemaName
		,@level1type = N'TABLE'     , @level1name = @tableName
end else
begin
	EXEC sys.sp_addextendedproperty
		@name        = @propertyName, @value      = @propertyValue
		,@level0type = N'SCHEMA'    , @level0name = @schemaName
		,@level1type = N'TABLE'     , @level1name = @tableName
end
";

			using (((ICurrentDbControl)connection).UseDatabase(table.DatabaseName))
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@fullName", SqlDbType.NVarChar, 128, table.ToString());
				cmd.AddParameter("@propertyName", SqlDbType.NVarChar, 128, propertyName);
				cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, table.SchemaName);
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, table.TableName);
				cmd.AddParameter("@propertyValue", SqlDbType.VarChar, 7000, (value) ?? (object)DBNull.Value);

				cmd.ExecuteNonQuery();
			}
		}

		public static void DropTableExtendedProperty(DbConnection connection, DbSchemaTable table, string propertyName)
		{
			Argument.NotNull(table, nameof(table)); // Suggested By ReviewBot
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			var sql = $@"
if (EXISTS(
		SELECT NULL FROM sys.extended_properties
		WHERE 1=1
			AND class = 1
			AND major_id = OBJECT_ID(@fullName, N'U')
			AND minor_id = 0
			AND name = @propertyName
	))
begin
	EXEC sys.sp_dropextendedproperty
		@name        = @propertyName
		,@level0type = N'SCHEMA'    , @level0name = @schemaName
		,@level1type = N'TABLE'     , @level1name = @tableName
end
"; // sql statement for developers only

			using (((ICurrentDbControl)connection).UseDatabase(table.DatabaseName))
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@fullName", SqlDbType.NVarChar, 128, table.ToString());
				cmd.AddParameter("@propertyName", SqlDbType.NVarChar, 128, propertyName);
				cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, table.SchemaName);
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, table.TableName);

				cmd.ExecuteNonQuery();
			}
		}

		public const string DateTimeForthcomingUpgradeExtPty = "CW1.DbUpg.DateTimeForthcomingUpgrade";

		#endregion

		#region Server Configuration Options

		public static object GetServerConfigOption(AdminConnection connection, string name)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(name, nameof(name));

			using (var cmd = connection.Command("SELECT value FROM sys.configurations WHERE name = @name")) // sql statement for developers only
			{
				cmd.AddParameter("@name", SqlDbType.NVarChar, 35, name); // sql command parameter name

				return cmd.ExecuteScalar();
			}
		}

		public static void SetServerConfigOption(DbConnection adminCnx, string name, string valueQuotedIfApplicable, bool isAdvancedOption = false)
		{
			Argument.NotNull(adminCnx, nameof(adminCnx)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(name, nameof(name));
			Argument.NotNull(valueQuotedIfApplicable, nameof(valueQuotedIfApplicable));

			var addParams = new Action<DbCommand>(command =>
			{
				command.AddParameter("@configName", SqlDbType.NVarChar, 35, name);
				command.AddParameter("@valueQuotedIfApplicable", SqlDbType.Int, valueQuotedIfApplicable);
			});
			if (!adminCnx.Exists("FROM sys.configurations WHERE name = (@configName) AND value = @valueQuotedIfApplicable", addParams)) // sql statement for developers only
			{
				if (isAdvancedOption)
				{
					var sqlText = $@"
					DECLARE @advancedOptionsSetValue bit, @advancedOptionsRunningValue bit;
					SELECT @advancedOptionsSetValue = CONVERT(bit, value), @advancedOptionsRunningValue = CONVERT(bit, value_in_use)
						FROM sys.configurations
						WHERE name = 'show advanced options';
					IF (@advancedOptionsSetValue = 0) EXEC sp_configure 'show advanced options', 1;
					IF (@advancedOptionsRunningValue = 0) RECONFIGURE;"; // sql statement for developers only
					using (var command = adminCnx.Command(sqlText))
					{
						addParams(command);
						command.ExecuteNonQuery();
					}
				}

				adminCnx.ExecuteNonQuery("EXEC sp_configure @configName, @valueQuotedIfApplicable;", addParams); // sql statement for developers only

				if (!adminCnx.Exists("FROM sys.configurations WHERE name = (@configName) AND value_in_use = @valueQuotedIfApplicable", addParams)) // sql statement for developers only
				{
					adminCnx.ExecuteNonQuery("RECONFIGURE");
				}
			}
		}

		#endregion

		#region Database Checks

		public static bool ObjectExists(DbConnection connection, string objectName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(objectName, nameof(objectName));

			var sql = "SELECT CONVERT(bit, CASE WHEN OBJECT_ID(@full_obj_name) is NULL THEN 0 ELSE 1 END);"; // sql statement for developers only

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@full_obj_name", SqlDbType.NVarChar, 400, objectName);

				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		public static bool ObjectExistsNolock(DbConnection connection, string schemaName, string objectName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(schemaName, nameof(schemaName));
			Argument.NotNullOrEmpty(objectName, nameof(objectName));

			var sql = @"
SELECT CONVERT(bit, CASE WHEN EXISTS(
	SELECT
		NULL
	FROM
		sys.schemas      AS sch WITH (NOLOCK)
		JOIN sys.objects AS obj WITH (NOLOCK) ON obj.schema_id = sch.schema_id
	WHERE
		sch.name = @sch_name
		AND obj.name = @obj_name
) THEN 1 ELSE 0 END)
"; // sql statement for developers only

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@sch_name", SqlDbType.NVarChar, 128, schemaName);
				cmd.AddParameter("@obj_name", SqlDbType.NVarChar, 128, objectName);

				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		public static bool IsDbNameAlphaNumeric(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			bool result = Regex.IsMatch(dbName, @"^[A-Z][A-Z0-9]*$", RegexOptions.IgnoreCase);
			return result;
		}

		public static void ValidateMainDatabaseName(string dbName)
		{
			const int maxLength = 35;

			if (String.IsNullOrWhiteSpace(dbName))
			{
				string message = "Main database name must not be empty."; // Does not apply to systems which are already up and running.
				throw new OdysseyDataException(message);
			}
			else if (dbName.Trim().Length > maxLength)
			{
				string message = String.Format(CultureInfo.InvariantCulture, "Main database must not be longer than {0} characters.\r\n\r\nDatabase: {1}", maxLength, dbName); // Does not apply to systems which are already up and running.
				throw new OdysseyDataException(message);
			}
			else if (!DataUtils.IsDbNameAlphaNumeric(dbName))
			{
				string message = String.Format(CultureInfo.InvariantCulture, "Main database name must only contain alphanumeric characters, starting with a letter.\r\n\r\nDatabase: {0}", dbName); // Does not apply to systems which are already up and running.
				throw new OdysseyDataException(message);
			}
		}

		public static bool IsWiseTechGlobalDatabaseServer(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));
#if DEBUG
			return IsWiseTechGlobalDatabaseServerForTest;
#else
			return IsWiseTechGlobalInternalComputer(connection);
#endif
		}

		internal static bool IsWiseTechGlobalInternalComputer(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			bool result = false;

			if (new CertificateAuthenticator().Verify())
			{
				result = true;
			}
			else
			{
				var domain = connection.ServerDomain;

				if (domain != null)
				{
					result = WtgDomainRegex.IsMatch(domain);
				}
			}

			return result;
		}

		public static bool IsCmrMsgTestServer(string serverName)
		{
			return cmrMsgTestServerRegex.IsMatch(serverName);
		}

		internal static readonly Regex WtgDomainRegex = new Regex(@"(((wg|corporate)\.(cargowise|wisetechglobal)\.com)|((wisecloud|wtg)\.zone))(?=\s*$)", RegexOptions.IgnoreCase);

		static readonly Regex cmrMsgTestServerRegex = new Regex(@"^sydcmr[1-9]?\.db\.corporate\.cargowise\.com$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		#endregion //Database Checks

		#region GetDbSeverFullDomainNameIncludingSqlPort

		public static string GetDbSeverFullDomainNameIncludingSqlPort(string server)
		{
			if (!string.IsNullOrEmpty(server))
			{
				string port, instance;
				(server, port) = SplitDbServerNameElement(server, ',', Db.SqlServerPort);
				(server, instance) = SplitDbServerNameElement(server, '\\', "");

				var domain = MainDbServerDomain;
				if (!server.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
				{
					server = server + "." + domain;
				}
				return $"{server}{instance}{port}";
			}

			return server;
		}

		static (string server, string element) SplitDbServerNameElement(string server, char splitChar, string valueIfEmptyElement)
		{
			var strs = server.Split(splitChar);
			var serverPart = strs[0];
			var elementPart = "";
			if (strs.Length > 2)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, @"The server name {0} is not correct.", server));
			}
			else if (strs.Length == 2 && strs[1].Length > 0)
			{
				elementPart = splitChar + strs[1];
			}

			if (string.IsNullOrEmpty(elementPart) && !string.IsNullOrEmpty(valueIfEmptyElement))
			{
				elementPart = splitChar + valueIfEmptyElement;
			}
			return (serverPart, elementPart);
		}

		#endregion

		static string MainDbServerDomain
		{
			get
			{
				if (mainDbServerDomain == null)
				{
					using (var conn = Db.NewAdminConnection())
					{
						mainDbServerDomain = conn.ServerDomain ?? "";
					}
				}

				return mainDbServerDomain;
			}
		}

		static string mainDbServerDomain;

		#region MISC

		public static string GetTheMostPopularValueForTheColumn(DbConnection connection, string dbName, string schemaName, string tableName, string columnName)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNullOrEmpty(schemaName, nameof(schemaName));
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(columnName, nameof(columnName));

			var statsName = string.Format("_stats_{0}", columnName); // string for developers only
			var qualifiedName = $"{schemaName.QuoteName()}.{tableName.QuoteName()}";// string for developers only

			var sql = $@"
DECLARE @t TABLE
(
	RANGE_HI_KEY        varchar(max),
	RANGE_ROWS          bigint,
	EQ_ROWS             bigint,
	DISTINCT_RANGE_ROWS bigint,
	AVG_RANGE_ROWS      bigint
);

INSERT @t(RANGE_HI_KEY, RANGE_ROWS, EQ_ROWS, DISTINCT_RANGE_ROWS, AVG_RANGE_ROWS)
	EXEC sp_executesql N'DBCC SHOW_STATISTICS(@qualifiedName, @stats) WITH HISTOGRAM;', N'@qualifiedName nvarchar(256), @stats nvarchar(128)', @qualifiedName = @qualifiedName, @stats = @stats ;

SELECT TOP(1)
	RANGE_HI_KEY
FROM
	@t
ORDER BY
	EQ_ROWS DESC;
"; // sql statement for developers only

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				if (!connection.Exists("FROM sys.stats WHERE object_id = OBJECT_ID(@qualifiedName, 'U') AND name = @stats", (cmd) =>  // Sql command
				{
					cmd.AddParameter("@qualifiedName", SqlDbType.NVarChar, 256, qualifiedName);
					cmd.AddParameter("@stats", SqlDbType.NVarChar, 128, statsName); // sql statement for developers only
				}))
				{
					connection.ExecuteNonQuery($"CREATE STATISTICS {statsName} ON {qualifiedName}({columnName.QuoteName()})");  // Sql command
				}

				using (var cmd = connection.Command(sql))
				{
					cmd.AddParameter("@stats", SqlDbType.NVarChar, 128, statsName); // sql statement for developers only
					cmd.AddParameter("@qualifiedName", SqlDbType.NVarChar, 256, qualifiedName); // sql statement for developers only
					var value = cmd.ExecuteScalar();
					return (value == null || value == DBNull.Value) ? null : (string)value;
				}
			}
		}

		public static string SQL_InitialTablesForEmptyDatabase()
		{
			return @"-- SQL_InitialTablesForEmptyDatabase
-- StmData
CREATE TABLE dbo.StmData
(
	SD_PK                       uniqueidentifier NOT NULL DEFAULT NEWID(),
	SD_Name                     varchar(300)         NULL,
	SD_Owner                    uniqueidentifier     NULL,
	SD_DepartmentGuid           uniqueidentifier     NULL,
	SD_Type                     char(3)              NULL,
	SD_BinaryValue              varbinary(max)       NULL,
	SD_GuidValue                uniqueidentifier     NULL,
	SD_PreserveTestValue        bit                  NULL,
	SD_IsLogged                 bit                  NULL,
	SD_IsCancelled              bit              NOT NULL DEFAULT 0,
	SD_SystemCreateTimeUtc      smalldatetime        NULL,
	SD_SystemCreateUser         varchar(3)       NOT NULL default '',
	SD_SystemLastEditTimeUtc    smalldatetime        NULL,
	SD_SystemLastEditUser       varchar(3)       NOT NULL default '',
)

-- GlbStaff - 1 Valid Login
CREATE TABLE dbo.GlbStaff
(
	GS_PK                        uniqueidentifier NULL,
	GS_Code                      char(3)          NULL,
	GS_IsActive                  char(1)          NULL,
	GS_LoginName                 varchar(35)      NULL,
	GS_IsController              char(1)          NULL,
	GS_IsSystemAccount           char(1)          NULL,
	GS_ChangePasswordAtNextLogin char(1)          NULL,
	GS_IsOperational             bit              NULL,
	GS_PasswordHash              varbinary(20)    NULL,
	GS_PasswordHashIterations    int              NULL,
	GS_PasswordSalt              varbinary(16)    NULL,
)

INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_IsActive, GS_IsController, GS_IsSystemAccount, GS_ChangePasswordAtNextLogin, GS_IsOperational, GS_PasswordHash, GS_PasswordHashIterations, GS_PasswordSalt) VALUES
	('4D001790-4A73-43FB-9393-3786FB8BCF84', 'X', 'sysadmin', 'N', 'N', 'Y', 'Y', 0, 0x4F8A9F7D3CD07874F1E4F993AECDAE7A3AC112FF, 200000, 0x2F6D29A6FB2AE97644CC78252C41F079)

-- StmServiceHost

CREATE TABLE [dbo].[StmServiceHost]
(
	[SH_PK]                 [uniqueidentifier] NOT NULL,
	[SH_IsActive]           [bit] NOT NULL,
	[SH_Status] AS CASE
WHEN
	(SH_DeleteTimeStampUtc IS NULL)
	OR
	(DATEDIFF(MINUTE, SH_DeleteTimeStampUtc, GETUTCDATE()) < 0)
THEN 'INS'
WHEN
	DATEDIFF(MINUTE, SH_DeleteTimeStampUtc, GETUTCDATE()) < 60
THEN 'DEL'
ELSE 'OBS'
END
,
	[SH_HostName]           [nvarchar](255) NOT NULL,
	[SH_DeleteTimeStampUtc] [smalldatetime] NULL,
	[SH_ProxyHost]          [nvarchar](255) NOT NULL,
	[SH_ProxyPort]          [int] NOT NULL,
	[SH_ProxyUserName]      [nvarchar](256) NOT NULL,
	[SH_ProxyPassword]      [nvarchar](127) NOT NULL,
	[SH_ProxyAutoDetect]    [bit] NOT NULL,
)

-- StmUpgrade
CREATE TABLE dbo.StmUpgrade
(
	SZ_PK                     uniqueidentifier NOT NULL,
	SZ_Type                   varchar(3)       NOT NULL,
	SZ_ExeVersionDate         smalldatetime        NULL,
	SZ_MajorVersion           int              NOT NULL,
	SZ_MinorVersion           int              NOT NULL,
	SZ_Release                int              NOT NULL,
	SZ_Patch                  int              NOT NULL,
	SZ_MajorDataVersion       int              NOT NULL,
	SZ_MinorDataVersion       int              NOT NULL,
	SZ_WorkingVersion         char(1)          NOT NULL,
	SZ_UpgradeNotes           varchar(max)     NOT NULL,
	SZ_UpgradeData_Compressed varbinary(max)       NULL,
	SZ_SplitCount             smallint         NOT NULL,
	SZ_SplitLastItem          char(1)          NOT NULL,
	SZ_Reference              varchar(128)     NOT NULL,
	SZ_Cancelled              char(1)          NOT NULL,
	SZ_CancelledReason        varchar(128)     NOT NULL,
	SZ_Status                 varchar(3)       NOT NULL,
	SZ_StatusTime             smalldatetime        NULL,
	SZ_StatusComment          varchar(128)     NOT NULL,
	SZ_SystemCreateTimeUtc    smalldatetime        NULL,
    SZ_SystemCreateUser		  varchar(3)	   NOT NULL,
    SZ_SystemLastEditTimeUtc  smalldatetime		   NULL,
    SZ_SystemLastEditUser	  varchar(3)	   NOT NULL,
)

ALTER TABLE dbo.StmUpgrade ADD
	CONSTRAINT [DF_SZ_Type]             DEFAULT 'EDP' FOR SZ_Type,
	CONSTRAINT [DF_SZ_MajorVersion]     DEFAULT 0     FOR SZ_MajorVersion,
	CONSTRAINT [DF_SZ_MinorVersion]     DEFAULT 0     FOR SZ_MinorVersion,
	CONSTRAINT [DF_SZ_Release]          DEFAULT 0     FOR SZ_Release,
	CONSTRAINT [DF_SZ_Patch]            DEFAULT 0     FOR SZ_Patch,
	CONSTRAINT [DF_SZ_MajorDataVersion] DEFAULT 0     FOR SZ_MajorDataVersion,
	CONSTRAINT [DF_SZ_MinorDataVersion] DEFAULT 0     FOR SZ_MinorDataVersion,
	CONSTRAINT [DF_SZ_WorkingVersion]   DEFAULT 'N'   FOR SZ_WorkingVersion,
	CONSTRAINT [DF_SZ_UpgradeNotes]     DEFAULT ''    FOR SZ_UpgradeNotes,
	CONSTRAINT [DF_SZ_SplitCount]       DEFAULT 0     FOR SZ_SplitCount,
	CONSTRAINT [DF_SZ_SplitLastItem]    DEFAULT 'N'   FOR SZ_SplitLastItem,
	CONSTRAINT [DF_SZ_Reference]        DEFAULT ''    FOR SZ_Reference,
	CONSTRAINT [DF_SZ_Cancelled]        DEFAULT 'N'   FOR SZ_Cancelled,
	CONSTRAINT [DF_SZ_CancelledReason]  DEFAULT ''    FOR SZ_CancelledReason,
	CONSTRAINT [DF_SZ_Status]           DEFAULT ''    FOR SZ_Status,
	CONSTRAINT [DF_SZ_StatusComment]    DEFAULT ''    FOR SZ_StatusComment

if (EXISTS(SELECT NULL FROM sys.tables WHERE name = 'GlbGroupLink'))
begin
	if (NOT EXISTS(
			SELECT NULL
			FROM dbo.GlbGroupLink
			WHERE
				GK_GG = (SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = 'ALL')
				AND GK_GS = (SELECT GS_PK FROM dbo.GlbStaff WHERE GS_LoginName = 'sysadmin')
		))
	begin
		INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS, GK_MembershipType, GK_SkillLevel, GK_IsValid, GK_CapacityLimitPercent) VALUES
			(NEWID(), (SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = 'ALL'), (SELECT GS_PK FROM dbo.GlbStaff WHERE GS_LoginName = 'sysadmin'), 'UDF', 0, 1, 100)
	end
end
";
		}

		public static bool ShouldDisableAutoStatisticsDuringUpgrade(DbConnection connection, string mainDbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(mainDbName))
			{
				return connection.ExecuteScalar<bool>($@"--{nameof(ShouldDisableAutoStatisticsDuringUpgrade)}
						SELECT
							BitValue = CONVERT(bit, ISNULL(MAX(
								IIF(CONVERT(nvarchar(max), SD_BinaryValue) = N'False', 0, 1)
								), 1))
						FROM
							dbo.StmData
						WHERE 1=1
							AND SD_Name = 'ISU_DisableAutoStatisticsDuringUpgrade'
							AND SD_Owner is NULL
							AND SD_DepartmentGuid is NULL
					");
			}
		}

		#endregion // MISC

		#region SetDbProperty

		public static void SetDbPropertyImmediately(AdminConnection connection, string dbName, string checkPredicate, string setCommand)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNullOrEmpty(checkPredicate, nameof(checkPredicate));
			Argument.NotNullOrEmpty(setCommand, nameof(setCommand));

#if DEBUG
			if (setCommand.StartsWith("RECOVERY ", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Cannot set Database Recovery Model after creation.");
			}
#endif
			if (ShouldSetDbPropertyImmediately(connection, dbName, checkPredicate))
			{
				connection.ExecuteNonQuery(FormattableString.Invariant($@"
					SET DEADLOCK_PRIORITY HIGH;
					ALTER DATABASE {dbName.QuoteName()} SET {setCommand} WITH ROLLBACK IMMEDIATE;
				")); // sql statement for developers only
			}
		}

		static bool ShouldSetDbPropertyImmediately(AdminConnection connection, string dbName, string checkPredicate)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNullOrEmpty(checkPredicate, nameof(checkPredicate));
			return connection.Exists($"FROM sys.databases WHERE name = @dbName AND is_read_only = 0 AND {checkPredicate}", cmd => cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName)); // sql statement for developers only
		}

		public static void SetCompatibilityLevelBasedOnServerVersion(AdminConnection connection, string dbName)
		{
			var compatibilityLevel = connection.ServerVersionNumber.CompatibilityLevel;
			SetDbPropertyImmediately(connection, dbName,
				"compatibility_level < " + compatibilityLevel, // sql server property
				"COMPATIBILITY_LEVEL = " + compatibilityLevel // sql server property
			);
		}

		#endregion

		#region Set TRUSTWORTHY ON

		public static void SetTrustworthyOn(AdminConnection connection, string dbName)
		{
			connection.ExecuteNonQuery(Invariant($@"
SET DEADLOCK_PRIORITY HIGH;
ALTER DATABASE {dbName.QuoteName()} SET TRUSTWORTHY OFF; 
ALTER DATABASE {dbName.QuoteName()} SET TRUSTWORTHY ON WITH ROLLBACK IMMEDIATE;
")); // There is a situation for TRUSTWORTHY where if is_trustworthy_on is 1 but the actual state is OFF will lead to fail to set it to ON; instead, it needs to be turned OFF first.
		}

		public static void EnsureClrEnabledAndTrustworthyOn(AdminConnection adminConnection, string dbName)
		{
			try
			{
				TryExecuteClrOnTargetDatabase();
			}
			catch (SqlException e)
			{
				var exceptionType = new DbErrorMatch(e).ExceptionType;
				if (exceptionType == DbErrorType.ClrDbOptionDisabled)
				{
					SetServerConfigOption(adminConnection, "clr enabled", "1");

					TryThenSetTrustworthyOn();
				}
				else if (exceptionType == DbErrorType.ErrorLoadingUntrustedAssembly)
				{
					SetTrustworthyOn(adminConnection, dbName);
				}
				else if (exceptionType == DbErrorType.CouldNotFindStoredProcedure)
				{
					SetServerConfigOption(adminConnection, "clr enabled", "1");

					SetTrustworthyOn(adminConnection, dbName);
				}
				else
				{
					throw;
				}
			}

			void TryThenSetTrustworthyOn()
			{
				try
				{
					TryExecuteClrOnTargetDatabase();
				}
				catch (SqlException e) when (DbErrorType.ErrorLoadingUntrustedAssembly == new DbErrorMatch(e).ExceptionType)
				{
					SetTrustworthyOn(adminConnection, dbName);
				}
			}

			void TryExecuteClrOnTargetDatabase()
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					adminConnection.ExecuteNonQuery(@"--Just want to check whether trustWorthy is really on
EXEC [dbo].[CLRDeleteFile] 'NUL', 0");
				}
			}
		}

		#endregion

		#region AlterDbAuthorisation

		public static void AlterDbAuthorisation(AdminConnection connection, string dbName, System.Data.Common.DbException ex = null)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (connection.TryGetLock("AlterDbAuthorisation_App_Lock", TimeSpan.FromSeconds(1), out SqlApplicationLock applock, dbName))
			{
				using (applock)
				{
					var isSetTosa = false;
					if (ex != null)
					{
						if (connection.Exists(@"FROM sys.databases WHERE name = @dbName AND SUSER_SNAME(owner_sid) = @adminLogin", cmd =>  // Sql command
						{
							cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
							cmd.AddParameter("@adminLogin", SqlDbType.NVarChar, 128, OdysseyAdminCredentials.AdminUserName);
						}))
						{
							connection.ExecuteNonQuery($"ALTER AUTHORIZATION ON DATABASE::{dbName.QuoteName()} TO [sa]"); // Sql command
							isSetTosa = true;
						}

						ErrorReporter.ReportOnce("DataUtils.AlterDbAuthorisation", Invariant($"AlterDbAuthorisation on db: '{dbName}' failed with error: {ex.Message}.{(isSetTosa ? "(switched to 'sa')." : "")}"));
					}

					AlterDbAuthorisation(connection, dbName, OdysseyAdminCredentials.AdminUserName);
				}
			}
			else
			{
				throw new OperationCanceledException(FormattableString.Invariant($"Get AlterDbAuthorisation_App_Lock for database {dbName} time out. Please fix database ownership manually. Exception Message: {ex?.Message ?? "(null)"}"), ex);
			}
		}

#if DEBUG
		public
#endif
		static void AlterDbAuthorisation(AdminConnection connection, string dbName, string loginName)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNullOrEmpty(loginName, nameof(loginName));

			if (connection.IsDbWriteable(dbName))
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					if (connection.Exists(@"
							FROM
								sys.server_principals AS AdminLogin
								LEFT JOIN (SELECT [sid] FROM sys.database_principals WHERE name = 'dbo') AS DboUser ON DboUser.sid = AdminLogin.sid
								LEFT JOIN (SELECT owner_sid FROM sys.databases WHERE name = @dbName) AS UserDatabase ON UserDatabase.owner_sid = AdminLogin.sid
							WHERE
								(DboUser.sid is NULL OR UserDatabase.owner_sid is NULL)
								AND AdminLogin.name = @loginName
							",
							cmd =>
							{
								cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);
								cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
							})) // sql statement for developers only
					{
						if (connection.Exists("FROM sys.database_principals WHERE name = @loginName", cmd => cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName))) // sql statement for developers only
						{
							connection.ExecuteNonQuery($"DROP USER {loginName.QuoteName()}"); // sql statement for developers only
						}

						connection.ExecuteNonQuery($@"ALTER AUTHORIZATION ON DATABASE::{dbName.QuoteName()} TO {loginName.QuoteName()}"); // sql statement for developers only
					}
				}
			}
		}

		#endregion

		#region RetryPolicy

		public static RetryPolicy LockTimeoutRetryPolicy
		{
			get
			{
#if DEBUG
				if (LockTimeoutRetryPolicyForTest != null)
				{
					return LockTimeoutRetryPolicyForTest;
				}
#endif
				// The algorithm of ExponentialBackoff is like delay = minBackoff + (2^(n-1) - 1) * deltaBackoff,
				// and no more than maxBackoff.
				//
				// There are 8 retries with wait delays like 1, 2, 4, 8, 16, 32, 64 and 120 seconds with 20% jitter,
				// then the total wait delay range is from 3.2 minutes to 4.8 minutes.
				//
				// Most of the lock timeouts observed are a little over 15 seconds, which is more than 2 minutes with all 8 retries.
				//
				// So the maximum total elapsed time calling ExecuteNoQueryWithRetry() will be around 7 to 8 minutes
				// with all 8 retries most of the time.
				return new RetryPolicy(
					new LockTimeoutDetectionStrategy(),
					new ExponentialBackoff(
						retryCount: 8,
						minBackoff: TimeSpan.FromSeconds(1),
						maxBackoff: TimeSpan.FromSeconds(120),
						deltaBackoff: TimeSpan.FromSeconds(1)));
			}
		}

		public class LockTimeoutDetectionStrategy : ITransientErrorDetectionStrategy
		{
			public bool IsTransient(Exception ex)
			{
				return ex is SqlException sqlException
					&& new DbErrorMatch(sqlException).ExceptionType == DbErrorType.LockTimeoutExpired;
			}
		}

		#endregion // RetryPolicy

		#region BulkRegistryRetrieval

		public static Dictionary<string, int> ReadBulkRegistry(DbConnection connection, params string[] names)
		{
			var sqlQuery = @"
	SELECT SD_Name, SD_BinaryValue
	FROM dbo.StmData
	WHERE SD_Name in (" + string.Join(", ", names.Select((_, i) => $"@CW{i}")) + @")
		AND SD_Owner is null
		AND SD_DepartmentGuid is null
		AND SD_BinaryValue is not null;";

			var dictionary = new Dictionary<string, int>();
			var command = connection.Command(sqlQuery);

			for (var i = 0; i < names.Length; i++)
			{
				command.AddParameter($"@CW{i}", SqlDbType.VarChar, names[i]);
			}

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var key = reader.GetString(0);
					var value = Encoding.Unicode.GetString(reader.ReadAllBytes(1));
					if (key == DbRegistry.SuspendAuditTriggersName)
					{
						dictionary[key] = string.CompareOrdinal(bool.TrueString, value) == 0 ? 1 : 0;
					}
					else
					{
						dictionary[key] = Convert.ToInt32(value);
					}
				}
			}
			return dictionary;
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace CargoWise.Data
{
	public static partial class DataUtils
	{
		public static bool IsWiseTechGlobalDatabaseServerForTest { get; set; }

		#region Build List of Unique Keys

		public static HashSet<string> BuildUniqueSingleKeyList()
		{
			var result = new HashSet<string>();

			var mainDbQuery = $@"
				SELECT TableName, ColumnName
				FROM ({string.Format(CultureInfo.InvariantCulture, NaturalKeyQuery, Db.DatabaseName)}) AS NaturalKey
				GROUP BY TableName, ColumnName
				ORDER BY TableName, ColumnName";

			AddUniqueKeysFromQuery(mainDbQuery, result);

			var refDbs = Db.Connection.GetDatabases(DatabaseType.ExclusiveRefOrSharedRef);

			foreach (string refDbName in refDbs)
			{
				var refDbQuery = $@"
					SELECT TableName = sn.name, ColumnName
					FROM
						({string.Format(CultureInfo.InvariantCulture, NaturalKeyQuery, refDbName)}) AS NaturalKey
						INNER JOIN [{Db.DatabaseName}].sys.synonyms sn ON sn.base_object_name like '[[]{refDbName}].%.[[]' + NaturalKey.TableName + ']'
					GROUP BY sn.name, ColumnName
					ORDER BY sn.name, ColumnName";

				AddUniqueKeysFromQuery(refDbQuery, result);
			}

			if (!Db.Connection.DatabaseExists(RefDbTableNameResolver.SingleRefDatabaseName))
			{
				throw new InvalidOperationException($"The database [{RefDbTableNameResolver.SingleRefDatabaseName}] does not exist.");
			}

			var sRDbQuery = $@"
					SELECT TableName = sn.name, ColumnName
					FROM
						({string.Format(CultureInfo.InvariantCulture, NaturalKeyQuery, RefDbTableNameResolver.SingleRefDatabaseName)}) AS NaturalKey
						INNER JOIN [{Db.DatabaseName}].sys.synonyms sn ON sn.base_object_name like '[[]{RefDbTableNameResolver.SingleRefDatabaseName}].%.[[]' + NaturalKey.TableName + 'TableView_%]'
					GROUP BY sn.name, ColumnName
					ORDER BY sn.name, ColumnName";

			AddUniqueKeysFromQuery(sRDbQuery, result);

			var sameDbQuery = $@"
					SELECT TableName = sn.name, ColumnName
					FROM
						({string.Format(CultureInfo.InvariantCulture, NaturalKeyQuery, Db.DatabaseName)}) AS NaturalKey
						INNER JOIN [{Db.DatabaseName}].sys.synonyms sn ON sn.base_object_name like '[[]%].[[]' + NaturalKey.TableName + ']'
					GROUP BY sn.name, ColumnName
					ORDER BY sn.name, ColumnName";

			AddUniqueKeysFromQuery(sameDbQuery, result);

			// these columns are often used as unique - however there are blanks in the table so cannot add unique index
			result.Add("REFDBTRFAU_AUCAHECC.UA_AHECC");
			result.Add("REFDBTRFAU_AUCCLASS.UJ_CODE");
			result.Add("REFDBENTAU_AUCCPQUESTION.UQ_CPDECNUMBER");
			result.Add("REFDBENTCA_CACEXPORTTARIFF.CE_CODE");

			result.Add("REFCOUNTRYSTATES.RW_CODE");
			result.Add("ACCCHARGECODE.AC_CODE");
			result.Add("JOBCARTAGE.JJ_CONSIGNMENTID");
			result.Add("REFCONTAINER.RC_CODE");

			// unit testing only
			result.Add("DUMMYBIZO.Z0_CODE");
			result.Add("DUMMYBIZO.Z0_DESCRIPTION");
			result.Add("DUMMYBIZO.Z0_FK_CODE");

			return result;
		}

		static void AddUniqueKeysFromQuery(string selectUniqueKeySql, HashSet<string> result)
		{
			using (var cmd = Db.Connection.Command(selectUniqueKeySql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var keyValue = (string)reader[0] + "." + (string)reader[1];

					try
					{
						result.Add(keyValue.ToUpperInvariant());
					}
					catch (ArgumentException ex)
					{
						var insertSameKeyToDicDebugInfo = string.Format(CultureInfo.InvariantCulture, @"An item with the same key '{0}' has already been added.", keyValue.ToUpperInvariant());
						throw new ArgumentException(insertSameKeyToDicDebugInfo, ex);
					}
				}
			}
		}

		static string NaturalKeyQuery
		{
			get
			{
				return @"
					SELECT TableName = tab.name, ColumnName = col.name
					FROM
						[{0}].sys.tables tab
						INNER JOIN [{0}].sys.columns col ON col.object_id = tab.object_id
						INNER JOIN [{0}].sys.index_columns ikey ON ikey.object_id = col.object_id AND ikey.column_id = col.column_id
						INNER JOIN [{0}].sys.indexes ind ON ind.object_id = tab.object_id AND ind.index_id = ikey.index_id
						INNER JOIN (
							SELECT ic.object_id, index_id
							FROM [{0}].sys.index_columns ic
							WHERE ic.is_included_column = 0
							GROUP BY ic.object_id, index_id
							HAVING count(*) = 1
						) ski ON ski.object_id = tab.object_id and ski.index_id = ind.index_id
					WHERE
						tab.is_ms_shipped = 0
						AND ind.is_unique = 1
						AND ikey.is_included_column = 0
						AND (col.name not like '__[_]PK' AND col.name not like '___[_]PK')";
			}
		}

		#endregion

		#region RetryPolicy

		static RetryPolicy LockTimeoutRetryPolicyForTest { get; set; }

		public static IDisposable SetTemporaryLockTimeoutRetryPolicyForTest()
		{
			LockTimeoutRetryPolicyForTest = new RetryPolicy(
				new LockTimeoutDetectionStrategy(),
				new ExponentialBackoff(
					retryCount: 2,
					minBackoff: TimeSpan.FromMilliseconds(0),
					maxBackoff: TimeSpan.FromSeconds(0),
					deltaBackoff: TimeSpan.FromSeconds(0)));

			return new DisposableAction(() => LockTimeoutRetryPolicyForTest = null);
		}

		#endregion // RetryPolicy
	}
}

#endif
#endregion
