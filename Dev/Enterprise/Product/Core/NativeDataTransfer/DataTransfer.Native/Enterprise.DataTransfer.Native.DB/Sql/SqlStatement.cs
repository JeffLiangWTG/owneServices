using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public abstract class SqlStatement
	{
		protected SqlStatement(string tableName, ColumnDef[] columnsDef, Dictionary<string, SqlParameter> keyToParameters)
		{
			this.keyToParameters = keyToParameters;
			this.tableName = tableName;
			this.columnsDef = columnsDef;
		}
		protected readonly string tableName;
		protected readonly ColumnDef[] columnsDef;
		protected readonly Dictionary<string, SqlParameter> keyToParameters;

		public string Generate()
		{
			var sql = string.Empty;
			sql += CreateSelectClause();
			sql += CreateFromClause();
			sql += CreateWhereClause();

			return sql;
		}

		public static void AddParameters(DbCommand command, Dictionary<string, SqlParameter> keyToParameters)
		{
			foreach (var keyToParameter in keyToParameters)
			{
				var parameter = keyToParameter.Value;
				command.AddParameter(parameter.Name, parameter.Type, parameter.Value);
			}
		}

		protected abstract string CreateFromClause();
		protected abstract string CreateWhereClause();

		protected string CreateSelectClause()
		{
			var selectClause = "SELECT";
			if (columnsDef == null || columnsDef.Length <= 0)
			{
				selectClause += " " + tableName + ".*";
			}
			else
			{
				selectClause += " DISTINCT ";
				selectClause += string.Join(", ", columnsDef.Select(item => item.Name).ToArray());
			}

			return selectClause;
		}

		protected SqlParameter AddSqlParameter(string value, IColumnDef columnDef)
		{
			var sqlParameter = NewSqlParameter(value, columnDef);
			var key = sqlParameter.keyForDuplicateCheck;
			if (keyToParameters.ContainsKey(key))
			{
				return keyToParameters[key];
			}

			keyToParameters.Add(key, sqlParameter);
			return sqlParameter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string for sql")]
		SqlParameter NewSqlParameter(string value, IColumnDef columnDef)
		{
			var parameterName = string.Format(CultureInfo.InvariantCulture, "@param{0}", keyToParameters.Count + 1);
			var parameterValue = value;
			var parameterType = (columnDef.DataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase)) ? SqlDbType.NVarChar : SqlDbType.VarChar;
			return new SqlParameter(parameterName, parameterValue, parameterType);
		}
	}
}
