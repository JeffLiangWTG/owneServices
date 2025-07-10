using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ZSqlSaverWithRetry : ZSqlSaverWithRetryBase
	{
		public ZSqlSaverWithRetry(DataSet data, DbConnection connection, IApplicationSchemaResolver schemaResolver)
			: base(data, connection, schemaResolver) { }

		public ZSqlSaverWithRetry(DataTable dataTable, DbConnection connection, IApplicationSchemaResolver schemaResolver)
			: base(dataTable, connection, schemaResolver) { }

		public ZSqlSaverWithRetry(DataSet data, ZSqlConnectionInfo connectionInfo, IApplicationSchemaResolver schemaResolver)
			: base(data, connectionInfo, schemaResolver) { }

		#region command builder getter

		public override ZSqlCommandBuilder GetZBulkUpdateCommandBuilder(IEnumerable<DataRow> rowsToUpdate, string tableName, bool compress, ITableSchema schema)
		{
			return new ZBulkUpdateCommandBuilder(rowsToUpdate, tableName, compress, schema);
		}

		public override ZSqlCommandBuilder GetZUpdateCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema, bool isBulk = false, StringBuilder commandText = null, List<ILargeColumnSaver> largeColumnSavers = null)
		{
			return new ZUpdateCommandBuilder(row, tableName, compress, statementIndex, schema, isBulk, commandText, largeColumnSavers);
		}

		public override ZSqlCommandBuilder GetZDeleteCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema)
		{
			return new ZDeleteCommandBuilder(row, tableName, compress, statementIndex, schema);
		}

		public override ZSqlCommandBuilder GetZInsertCommandBuilder(string tableName, bool compress, int statementIndex, ITableSchema schema, int expectedNumberOfRows)
		{
			return new ZInsertCommandBuilder(tableName, compress, statementIndex, schema, expectedNumberOfRows);
		}

		#endregion

		#region DataRow

		protected override bool HasDataSourceColumn(DataRow row)
		{
			if (row is ZDataRow zDataRow && row.Table != null)
			{
				var columns = row.Table.Columns?.Cast<DataColumn>();
				return columns.Any(col => zDataRow.HasSource(col.ColumnName));
			}
			return false;
		}

		protected override bool IsRowValueChanged(DataRow row, DataColumn column)
		{
			return ZDataUtils.IsRowValueChanged(row, column);
		}

		protected override bool DoesUpdateRowHavePersistentChangesFromOriginal(IApplicationSchemaResolver resolver, DataRow row)
		{
			return ZDataUtils.DoesUpdateRowHavePersistentChangesFromOriginal(resolver, row);
		}

		#endregion
	}
}
