using System.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	internal class ZDeleteCommandBuilder : ZDeleteCommandBuilderBase
	{
		public ZDeleteCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema)
			: base(row, tableName, compress, statementIndex, schema) { }

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
