using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing;

sealed class ZJoinQueryTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentException>("When TableName is null", () => new ZJoinQuery(null, selectList: new[] { "A" }));
		AssertExceptionThrown<ArgumentException>("When TableName is empty", () => new ZJoinQuery("", selectList: new[] { "A" }));
		AssertExceptionThrown<ArgumentNullException>("When Select List is null", () => new ZJoinQuery("ABC", selectList: null));
		AssertExceptionThrown<ArgumentException>("When Select List is empty", () => new ZJoinQuery("ABC", selectList: Array.Empty<string>()));
	}

	public void TestAddJoinPart()
	{
		var sqlQuery = new ZJoinQuery("ABC", new[] { "A" });
		sqlQuery.AddJoinPart("INNER JOIN Q on A_PK = Q_A");
		var sqlText = sqlQuery.JoinPartsSqlText;
		AssertContains("Inner Join is present", "INNER JOIN Q on A_PK = Q_A", sqlText, ignoreCase: true);
	}

	public void TestGetSql()
	{
		var sqlQuery = new ZJoinQuery("ABC", new[] { "A", "B" });
		sqlQuery.AddJoinPart("INNER JOIN Q on A_PK = Q_A");
		sqlQuery.AddFilterAndZSQLParameterCollection("A_PK = 1", null);
		var (sqlText, _) = sqlQuery.GetSql();
		var expectedSql = @"SELECT A, B
FROM ABC
INNER JOIN Q on A_PK = Q_A
	WHERE A_PK = 1";
		AssertEquals("SQL", expectedSql, sqlText);
	}
}
