using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public sealed class TableColumn
	{
		/// <summary>
		/// Pass non null tablename and columnname to the constructor
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="columnName"></param>
		public TableColumn(string tableName, string columnName)
		{
			Argument.NotNull(tableName, "tableName");
			Argument.NotNull(columnName, "columnName");
			TableName = tableName;
			ColumnName = columnName;
		}

		public readonly string TableName;
		public readonly string ColumnName;
	}
}
