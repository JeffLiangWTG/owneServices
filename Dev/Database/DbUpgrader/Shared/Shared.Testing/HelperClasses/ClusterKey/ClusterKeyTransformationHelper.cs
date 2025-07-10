using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public static class ClusterKeyTransformationHelper
	{
		public static void DeleteClusterKeyNumberFountain(DbConnection connection, string fountainName)
		{
			var sql = $"DELETE dbo.StmNums WHERE SN_Name = '{fountainName}'";
			connection.ExecuteNonQuery(sql);
		}

		public static void DropClusterKeyIndexesAndConstraints(DbConnection connection, string[] participatingClusterKeys)
		{
			var cmdRunner = new BatchRunner();

			var dropIndexWhereClause = string.Join(" OR ", participatingClusterKeys.Select(kcol => $"i.name like '%[_][RU]C[_][_]{kcol}'"));

			var dropIndexSql = $@"
				SELECT 'DROP INDEX [' + i.name +  '] ON [' + t.name + '];'
				FROM sys.tables t
				INNER JOIN sys.indexes i ON i.object_id = t.object_id
				WHERE ({dropIndexWhereClause})";

			cmdRunner.RunCommandsGeneratedByQuery(connection, dropIndexSql);

			var dropConstraintInClause = string.Join(",", participatingClusterKeys.Select(kcol => $"'Constraint_{kcol}'"));

			var dropConstrintSql = $@"
				SELECT 'ALTER TABLE [' + t.name + '] DROP CONSTRAINT [' + c.name +  '];'
				FROM sys.tables t
				INNER JOIN sys.check_constraints c ON c.parent_object_id = t.object_id
				WHERE c.name in ({dropConstraintInClause})";

			cmdRunner.RunCommandsGeneratedByQuery(connection, dropConstrintSql);
		}

		public static void AssertClusterKey(DbConnection connection, SchemaIntColumn keyColumn, Guid pk, int expectedValue, string assertMsgSuffix = null)
		{
			var sql = $"SELECT {keyColumn.Name} FROM {keyColumn.TableName} WHERE {keyColumn.TableSchema.PK.Name} = '{pk}'";
			var actualValue = connection.ExecuteScalar<int>(sql);
			var assertMsg = keyColumn.Name + (string.IsNullOrWhiteSpace(assertMsgSuffix) ? "" : $" ({assertMsgSuffix})");
			Assertion.AssertEquals(assertMsg, expectedValue, actualValue);
		}

		public static void AssertNumberFountain(DbConnection connection, string fountainName, long expectedValue)
		{
			string sql = $"DECLARE @result BIGINT = (SELECT SN_Value FROM dbo.StmNums WHERE SN_name = '{fountainName}'); SELECT ISNULL(@result, 0);";
			var actualValue = connection.ExecuteScalar<long>(sql);
			Assertion.AssertEquals($"Number Fountain [{fountainName}]", expectedValue, actualValue);
		}
	}
}
