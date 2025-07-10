using CargoWise.Schema;
using Moq;

namespace Enterprise.DataPurge.Test.Utility
{
	public static class SchemaUtility
	{
		static ITableSchema MockTableSchemaByTableName(string tableName)
		{
			var tableSchemaMock = new Mock<ITableSchema>();
			tableSchemaMock.SetupGet(x => x.TableName).Returns(tableName);
			return tableSchemaMock.Object;
		}

		public static SchemaGuidColumn CreateGuidColumnForTest(string tableName, string columnName)
		{
			var tableSchema = MockTableSchemaByTableName(tableName);
			return new SchemaGuidColumn(tableSchema, columnName, 0, System.Guid.Empty, false);
		}
	}
}
