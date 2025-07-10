using System;
using CargoWise.Data;
using CargoWise.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	static class ClusterKeyWorkerPopulateStrategyTestHelper
	{
		public static void AssertChildClusterKey(DbConnection connection, SchemaGuidColumn childFkColumn, (Guid Pk, int Ck) parent, int populated, int nonPopulated)
		{
			Assertion.CombineAssertions(() =>
			{
				AssertChildClusterKeyValueCount(connection, childFkColumn, parent.Pk, parent.Ck, populated);
				AssertChildClusterKeyValueCount(connection, childFkColumn, parent.Pk, 0, nonPopulated);
			});
		}

		public static void AssertChildClusterKeyValueCount(DbConnection connection, SchemaGuidColumn childFkColumn, Guid parentFk, int ckValue, int expectedCount)
		{
			var sql = $@"
				SELECT count(*)
				FROM {childFkColumn.TableName}
				WHERE {childFkColumn.Name} = '{parentFk}'
				AND {childFkColumn.ColumnPrefix + CargoWise.Schema.Schema.ClusterKeyColumnSuffix} = {ckValue}";
			var actualCount = connection.ExecuteScalar<int>(sql);
			var assertMsg = $"Parent FK: {parentFk}, ClusterKey [{ckValue}] count.";
			Assertion.AssertEquals(assertMsg, expectedCount, actualCount);
		}
	}
}
