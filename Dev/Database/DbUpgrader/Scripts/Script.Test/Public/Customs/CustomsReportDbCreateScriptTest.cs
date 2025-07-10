using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	abstract class CustomsReportDbCreateScriptTest : DbCreateScriptTest
	{
		protected object GetValueOrDBNull(object value) => value ?? DBNull.Value;
		IEnumerable<(SqlDbType ParameterType, string ParameterName)> SqlParameters => sqlParameters ?? (sqlParameters = GetSqlParameters());
		IEnumerable<(SqlDbType ParameterType, string ParameterName)> sqlParameters;

		protected abstract IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters();

		protected virtual object[] GetFilteredRows(string selectedColumn, params (string ParamName, object ParamValue)[] filters)
		{
			var resultList = new List<object>();
			var stringQueryBuilder = new StringBuilder();

			using (var command = Db.Connection.Command(string.Empty))
			{
				stringQueryBuilder.Append($"SELECT {selectedColumn} FROM {ScriptToTest.Name}(");

				foreach (var sqlParameter in SqlParameters)
				{
					AddParameter(filters, command, sqlParameter);
					stringQueryBuilder.Append($" {sqlParameter.ParameterName},");
				}

				command.CommandText = $"{stringQueryBuilder.ToString().TrimEnd(',')})";

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						resultList.Add(reader[0]);
					}
				}
			}

			return resultList.ToArray();
		}

		protected virtual void AddParameter((string ParamName, object ParamValue)[] filters, DbCommand command, (SqlDbType ParameterType, string ParameterName) sqlParameter)
		{
			command.AddParameter(sqlParameter.ParameterName, sqlParameter.ParameterType, GetValueOrDBNull(sqlParameter.ParameterName, filters));
		}

		object GetValueOrDBNull(string paramName, (string ParamName, object ParamValue)[] filters) => filters.SingleOrDefault(x => x.ParamName == paramName).ParamValue ?? DBNull.Value;

		protected void UpdateDeclarationColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("JobDeclaration", columnName, columnValue, columnType, "JE_PK", primaryKeyValue);
		}

		protected void UpdateEntryHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusEntryHeader", columnName, columnValue, columnType, "CH_PK", primaryKeyValue);
		}

		protected void UpdateEntryLineColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusEntryLine", columnName, columnValue, columnType, "CL_PK", primaryKeyValue);
		}

		protected void UpdateEntryLineFee(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusEntryLineFee", columnName, columnValue, columnType, "CF_PK", primaryKeyValue);
		}

		protected void UpdateEntryNumberColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusEntryNum", columnName, columnValue, columnType, "CE_PK", primaryKeyValue);
		}

		protected void UpdateInvoiceColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("JobComInvoiceHeader", columnName, columnValue, columnType, "JZ_PK", primaryKeyValue);
		}

		protected void UpdateInvoiceLineColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("JobComInvoiceLine", columnName, columnValue, columnType, "JI_PK", primaryKeyValue);
		}

		protected void UpdateJobHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("JobHeader", columnName, columnValue, columnType, "JH_PK", primaryKeyValue);
		}

		protected void UpdateTableColumn(string tableName, string columnName, object columnValue, SqlDbType columnType, string primaryKeyColumnName, object primaryKeyValue)
		{
			var tablePrefix = primaryKeyColumnName.Split(new char[] { '_' })[0];
			var updateSetColumns = $"{columnName} = @ColumnValue, {tablePrefix}_SystemLastEditTimeUtc = GetUtcDate(), {tablePrefix}_SystemLastEditUser = '~SF'";

			using (var command = Db.Connection.Command($"UPDATE {tableName} SET {updateSetColumns} WHERE {primaryKeyColumnName} = @PrimaryKeyValue"))
			{
				command.AddParameter("@ColumnValue", columnType, columnValue);
				command.AddParameter("@PrimaryKeyValue", SqlDbType.UniqueIdentifier, primaryKeyValue);
				command.ExecuteNonQuery();
			}
		}

		protected void AssertFunctionReturnExpectedValue(string columnName, object expectedValue)
		{
			var rows = GetFilteredRows(columnName);
			AssertEquals($"Expect 1 row", 1, rows.Length);
			var row = rows.Single();
			AssertEquals(columnName, expectedValue, row);
		}
	}
}
