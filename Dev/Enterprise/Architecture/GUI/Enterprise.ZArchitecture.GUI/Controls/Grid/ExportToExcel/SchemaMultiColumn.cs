using System.Data;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Excel
{
	public class SchemaMultiColumn : SchemaStringColumn
	{
		public SchemaMultiColumn(string columnName, ITableSchema tableSchema)
			: base(tableSchema, columnName, 0, SqlDbType.VarChar, "", false, 255)
		{
		}
	}
}