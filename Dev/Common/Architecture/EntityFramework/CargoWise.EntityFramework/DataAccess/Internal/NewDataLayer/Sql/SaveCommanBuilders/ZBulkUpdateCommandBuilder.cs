using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	internal class ZBulkUpdateCommandBuilder : ZBulkUpdateCommandBuilderBase
	{
		public ZBulkUpdateCommandBuilder(IEnumerable<DataRow> rowsToUpdate, string tableName, bool compress, ITableSchema schema)
			: base(rowsToUpdate, tableName, compress, schema)
		{ }

		public override object GetTruncatedBasicValueForDatabase(object value, SchemaColumn schemaColumn)
		{
			return ZSqlParameter.GetTruncatedBasicValueForDatabase(value, schemaColumn);
		}

		public override ZUpdateCommandBuilderBase GetZUpdateCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema, bool isBulk, StringBuilder commandText, List<ILargeColumnSaver> largeColumnSavers)
		{
			return new ZUpdateCommandBuilder(row, tableName, compress, statementIndex, schema, isBulk, commandText, largeColumnSavers);
		}

		protected override object GetSourceValueIfSet(DataRow row, object value, SchemaColumn column)
		{
			return ZDataUtils.GetSourceValueIfSet(row, value, column);
		}
	}
}
