using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	internal class ZUpdateCommandBuilder : ZUpdateCommandBuilderBase
	{
		public ZUpdateCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema, bool isBulk = false, StringBuilder commandText = null, List<ILargeColumnSaver> largeColumnSavers = null)
			: base(row, tableName, compress, statementIndex, schema, isBulk, commandText, largeColumnSavers)
		{ }

		public override object GetTruncatedBasicValueForDatabase(object value, SchemaColumn schemaColumn)
		{
			return ZSqlParameter.GetTruncatedBasicValueForDatabase(value, schemaColumn);
		}

		protected override object GetSourceValueIfSet(DataRow row, object value, SchemaColumn column)
		{
			return ZDataUtils.GetSourceValueIfSet(row, value, column);
		}

		protected override bool IsRowValueChanged(DataRow row, DataColumn column)
		{
			return ZDataUtils.IsRowValueChanged(row, column);
		}
	}
}
