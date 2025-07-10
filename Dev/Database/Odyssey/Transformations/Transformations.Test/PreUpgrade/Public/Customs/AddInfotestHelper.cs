using System;
using CargoWise.Data;
using static NUnit.Framework.Assertion;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Public.Customs.Testing
{
	internal class AddInfoTestHelper
	{
		internal AddInfoTestHelper(DbConnection testConnection, string tableName, string tablePrefix)
		{
			this.testConnection = testConnection;
			this.tableName = tableName;
			this.tablePrefix = tablePrefix;
		}
		readonly DbConnection testConnection;
		readonly string tableName;
		readonly string tablePrefix;

		internal void AssertFieldValue(string message, Guid pk, string fieldName, string expectedValue)
		{
			var sql = $@"
SELECT a.Value
  FROM {tableName}
CROSS APPLY csfn_GetAddInfoValueFromCodeInline({tablePrefix}_AddInfo, '{fieldName}') a
 WHERE {tablePrefix}_PK = '{pk}'";

			using (var reader = testConnection.Command(sql).ExecuteReader())
			{
				reader.Read();
				var actualValue = reader.IsDBNull(0) ? null : reader.GetString(0);
				AssertEquals($"{tableName}.{tablePrefix}.{fieldName} {message}", expectedValue, actualValue);
			}
		}

		internal void AssertTableEmpty(string tableName)
		{
			AssertEquals($"{tableName} rowcount", 0, testConnection.ExecuteScalar($"SELECT COUNT(*) FROM {tableName}"));
		}
	}
}
