using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.TestFramework
{
	/// <summary>
	/// Class for inserting data into DB for tests. Specific areas e.g. accounting should derive from this class.
	/// </summary>
	/// <remarks>
	/// It is usually preferable to test scripts from the Accounting solution \Enterprise\Product\Operations\Accounting\Business\ScriptTests so
	/// that you can use the BusinessObjectFactory to more easily create the data. If only I had known...
	/// </remarks>
	public class TestDbHelperBase
	{
		protected Guid lastPK;
		protected Guid FreshPK() { return lastPK = Guid.NewGuid(); }

		protected DbConnection DbConnection => dbConnection;
		readonly DbConnection dbConnection;

		public TestDbHelperBase(DbConnection dbConnection)
		{
			this.dbConnection = dbConnection;
		}

		const int DefaultStringColumnLength = 3;

		public void Insert(string tableName, object valuesObject)
		{
			var schema = EnterpriseSchema.GetTableSchema(tableName);
			var properties = valuesObject.GetType().GetProperties();
			var firstPropertyName = properties.First().Name;
			var prefix = schema?.PK.ColumnPrefix ?? firstPropertyName.Substring(0, firstPropertyName.IndexOf('_'));
			var columnExists = schema != null
				? (Func<string, bool>)(name => schema.GetSchemaColumn(name) != null)
				: name => DbConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM sys.columns col JOIN sys.tables tab ON col.object_id = tab.object_id WHERE col.name = '{name}' AND tab.name = '{tableName}'") > 0;

			var auditColumns = new[]
			{
				new { Name = $"{prefix}_SystemCreateTimeUtc", Value = $"'{ZDateTime.UtcNow.SqlFormat}'" },
				new { Name = $"{prefix}_SystemCreateUser", Value = "'A'" },
				new { Name = $"{prefix}_SystemLastEditTimeUtc", Value = $"'{ZDateTime.UtcNow.SqlFormat}'" },
				new { Name = $"{prefix}_SystemLastEditUser", Value = "'A'" }
			}.Where(c => columnExists(c.Name) && !properties.Any(p => p.Name == c.Name));

			string sql =
				string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})",
				tableName,
				string.Join(",", properties.Select(p => string.Format("[{0}]", p.Name)).Concat(auditColumns.Select(c => c.Name))),
				string.Join(",", properties.Select(p => "@" + p.Name).Concat(auditColumns.Select(c => c.Value))));

			RunSQL(valuesObject, sql);
		}

		public IDataReader RunSP(string spName, object valuesObject)
		{
			return (IDataReader)RunSQL(valuesObject, spName, CommandType.StoredProcedure, SQLExecutionTypes.ExecuteReader);
		}

		public IDataReader RunTableValuedFunction(string functionName, object valuesObject)
		{
			string sql =
				string.Format("SELECT * FROM {0}({1})",
				functionName,
				string.Join(",", valuesObject.GetType().GetProperties().Select(p => "@" + p.Name)));

			return (IDataReader)RunSQL(valuesObject, sql, CommandType.Text, SQLExecutionTypes.ExecuteReader);
		}

		public object RunSQL(object valuesObject, string sql, CommandType commandType = CommandType.Text, SQLExecutionTypes executionType = SQLExecutionTypes.Non)
		{
			using (var command = dbConnection.Command(sql))
			{
				command.CommandType = commandType;

				if (valuesObject != null)
				{
					foreach (var property in valuesObject.GetType().GetProperties())
					{
						object val = property.GetValue(valuesObject, null);
						if (property.PropertyType == typeof(TVPParamInfo))
						{
							var tvpVal = val as TVPParamInfo;
							if (tvpVal != null)
							{
								tvpVal.AddTVPParameters(command);
							}
						}
						else
						{
							val = val ?? DBNull.Value;
							var parameter = new SqlParameter(property.Name, val);
							if (parameter.DbType == DbType.String && parameter.Size == 0)
							{
								parameter.Size = DefaultStringColumnLength;
							}
							command.AddParameter(parameter, parameter.Precision, parameter.Scale, val);
						}
					}
				}

				object result = null;
				switch (executionType)
				{
					case SQLExecutionTypes.Non:
						command.ExecuteNonQuery();
						break;
					case SQLExecutionTypes.ExecuteReader:
						result = command.ExecuteReader();
						break;
					case SQLExecutionTypes.ExecuteScalar:
						result = command.ExecuteScalar();
						break;
				}

				return result;
			}
		}

		public DateTime ToDate(string dateString)
		{
			if (dateString.Length == 10)
			{
				return DateTime.ParseExact(dateString, "yyyy-MM-dd", null);
			}
			else if (dateString.Length == 19)
			{
				return DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", null);
			}
			throw new ArgumentException("dateString length");
		}

		public enum SQLExecutionTypes
		{
			Non,
			ExecuteReader,
			ExecuteScalar
		}

		public void AssertTableAsTextFromSQLServerManagenentStudio(string message, DataTable resultTable, string expectedResult, IEnumerable<string> columnsWithoutRounding)
		{
			var expectedResultByLines = expectedResult.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			var columnWidthTemplate = expectedResultByLines[1];
			var columnWidths = columnWidthTemplate.Split(' ');
			Assertion.AssertEquals("Number of columns in expected and actual results is not equal", columnWidths.Length, resultTable.Columns.Count);

			var maxColumnWidth = new Dictionary<string, int>();
			int i = 0;
			foreach (DataColumn column in resultTable.Columns)
			{
				maxColumnWidth.Add(column.ColumnName, columnWidths[i++].Length);
			}

			var tableAsText = new List<List<string>>();
			foreach (DataRow row in resultTable.Rows)
			{
				var rowLine = new List<string>();
				foreach (DataColumn column in resultTable.Columns)
				{
					var rowColumn = row[column];
					var columnName = column.ColumnName;
					var value = DBValueToString(rowColumn, false, columnsWithoutRounding.Contains(columnName));
					rowLine.Add(value.PadRight(maxColumnWidth[columnName]));
				}
				tableAsText.Add(rowLine);
			}

			var result = new StringBuilder();
			result.AppendLine();
			result.AppendLine(string.Join(" ", maxColumnWidth.Select(item => item.Key.PadRight(item.Value))).Trim());
			result.AppendLine(columnWidthTemplate);
			foreach (var row in tableAsText)
			{
				result.AppendLine(string.Join(" ", row).Trim());
			}

			Assertion.AssertMultilineASCIIEquals(message, expectedResult, result.ToString());
		}

		string DBValueToString(object rowColumn, bool isSmallDateTime, bool doNotRound)
		{
			string value = "";
			if (rowColumn is DBNull)
			{
				value = "NULL";
			}
			else if (rowColumn is DateTime)
			{
				var smallDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
				var dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
				value = ((DateTime)rowColumn).ToString(isSmallDateTime ? smallDateTimeFormat : dateTimeFormat, CultureInfo.InvariantCulture);
			}
			else if (rowColumn is DateTimeOffset)
			{
				var dateTimeFormat = "yyyy-MM-dd HH:mm:ss zzz";
				value = ((DateTimeOffset)rowColumn).ToString(dateTimeFormat, CultureInfo.InvariantCulture);
			}
			else if (rowColumn is decimal)
			{
				value = ((decimal)rowColumn).ToString(doNotRound ? "0.000000" : "0.00", CultureInfo.InvariantCulture);
			}
			else if (rowColumn is bool)
			{
				value = (bool)rowColumn ? "1" : "0";
			}
			else
			{
				value = rowColumn.ToString();
			}

			return value;
		}
	}

	public class TVPParamInfo
	{
		public TVPParamInfo(string parameterName, string parameterTypeName, Type valueType, IEnumerable<object> values)
		{
			if (values == null)
			{
				throw new ArgumentException("TVP: Provided Table Value Parameters cannot be null");
			}
			if (string.IsNullOrEmpty(parameterName))
			{
				throw new ArgumentException("TVP: Provided ParameterName cannot be null or empty");
			}
			if (string.IsNullOrEmpty(parameterTypeName))
			{
				throw new ArgumentException("TVP: Provided Parameter Type Name cannot be null or empty");
			}
			if (valueType == null)
			{
				throw new ArgumentException("TVP: Provided Value Type cannot be null");
			}

			this.parameterName = parameterName;
			this.parameterTypeName = parameterTypeName;
			this.valueType = valueType;
			this.values = values;
		}

		readonly string parameterName;
		readonly string parameterTypeName;
		readonly IEnumerable<object> values;
		readonly Type valueType;

		public string ParameterName
		{
			get { return parameterName; }
		}

		public string ParameterTypeName
		{
			get { return parameterTypeName; }
		}

		public IEnumerable<object> Values
		{
			get { return values; }
		}

		public void AddTVPParameters(DbCommand command)
		{
			using (var table = new DataTable())
			{
				table.Locale = CultureInfo.InvariantCulture;
				table.Columns.Add("Value", valueType); // Part of SQL code

				if (values != null)
				{
					foreach (object value in Values)
					{
						table.Rows.Add(value);
					}
				}
				command.AddTableValuedParameter(ParameterName, ParameterTypeName, table);
			}
		}
	}
}

