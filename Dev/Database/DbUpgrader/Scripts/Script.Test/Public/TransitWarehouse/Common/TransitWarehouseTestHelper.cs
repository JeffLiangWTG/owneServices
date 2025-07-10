using CargoWise.Schema;

public static class TransitWarehouseTestHelper
{
	public static string TableDescriptor(ITableSchema schema) => $"{schema.SqlSchemaName}.{schema.TableName}";
}
