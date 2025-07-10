using System.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	internal class ZInsertCommandBuilder : ZInsertCommandBuilderBase
	{
		public ZInsertCommandBuilder(string tableName, bool compress, int statementIndex, ITableSchema schema, int expectedNumberOfRows)
			: base(tableName, compress, statementIndex, schema, expectedNumberOfRows) { }

		public override object GetTruncatedBasicValueForDatabase(object value, SchemaColumn schemaColumn)
		{
			return ZSqlParameter.GetTruncatedBasicValueForDatabase(value, schemaColumn);
		}

		protected override object GetSourceValueIfSet(DataRow row, object value, SchemaColumn column)
		{
			return ZDataUtils.GetSourceValueIfSet(row, value, column);
		}
	}
}
