using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using CargoWise.Schema;

namespace Enterprise.DbUpgrader.Transformations
{
	static class FkToNkTestDataHelper
	{
		internal static void AddTableInsert(IDictionary<ITableSchema, IList<KeyValuePair<string, string>>> tableInserts, ITableSchema table, string columnName, string value)
		{
			IList<KeyValuePair<string, string>> columnConstraints;
			if (tableInserts.ContainsKey(table))
			{
				columnConstraints = tableInserts[table];
			}
			else
			{
				columnConstraints = new List<KeyValuePair<string, string>>();
				tableInserts.Add(table, columnConstraints);
			}
			columnConstraints.Add(new KeyValuePair<string, string>(columnName, value));
		}

		internal static Guid BuildInsertsSqlAndExecute(DbConnection connection, ITableSchema table, IList<KeyValuePair<string, string>> columnInserts)
		{
			StringBuilder insertColumnsText = new StringBuilder();
			StringBuilder insertValuesText = new StringBuilder();

			Guid insertPk = Guid.NewGuid();
			int insertIndex = 2;

			for (int i = columnInserts.Count - 1; i >= 0; i--)
			{
				KeyValuePair<string, string> columnConstraint = columnInserts[i];
				if (table.PK.Name.Equals(columnConstraint.Key))
				{
					insertPk = new Guid(columnConstraint.Value);
					columnInserts.RemoveAt(i);
				}
				else
				{
					insertColumnsText.Append(", {" + ++insertIndex + "}");
					insertValuesText.Append(", '{" + ++insertIndex + "}'");
				}
			}

			StringBuilder sqlText = new StringBuilder("declare @id uniqueidentifier SET @id = '{2}' ");
			sqlText.Append("INSERT INTO {0} ({1}");
			sqlText.Append(insertColumnsText);
			sqlText.Append(") VALUES (@id");
			sqlText.Append(insertValuesText);
			sqlText.Append(") select @id");

			List<object> stringFormatObjects = new List<object>();
			stringFormatObjects.Add(table.TableName);
			stringFormatObjects.Add(table.PK.Name);
			stringFormatObjects.Add(insertPk);

			foreach (KeyValuePair<string, string> columnConstraint in columnInserts)
			{
				stringFormatObjects.Add(columnConstraint.Key);
				stringFormatObjects.Add(columnConstraint.Value);
			}

			return (Guid)connection.ExecuteScalar(String.Format(sqlText.ToString(), stringFormatObjects.ToArray()));
		}
	}
}
