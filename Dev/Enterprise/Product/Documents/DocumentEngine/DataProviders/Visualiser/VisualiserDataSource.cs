using System.Collections.Generic;
using System.Data;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine.DataProviders
{
	sealed class VisualiserDataSource : ADODataSource
	{
		public VisualiserDataSource(VisualiserDataSet workingDataSet, string tableName)
			: this(tableName, workingDataSet.Tables[tableName])
		{
		}

		VisualiserDataSource(string tableName, DataTable table)
			: base(table)
		{
			VisualiserDataSetTableName = tableName;
		}

		VisualiserDataSource(string tableName, ADODataSource wrappedSource, int[] wrappedRowsIndexes)
			: base(wrappedSource, wrappedRowsIndexes)
		{
			VisualiserDataSetTableName = tableName;
		}

		readonly string VisualiserDataSetTableName;

		protected override ADODataSource GetNewDataSource(DataTable table)
		{
			return new VisualiserDataSource(VisualiserDataSetTableName, table);
		}

		protected override ADODataSource GetNewDataSource(ADODataSource wrappedSource, int[] wrappedRowsIndexes)
		{
			return new VisualiserDataSource(VisualiserDataSetTableName, wrappedSource, wrappedRowsIndexes);
		}

		protected override IDataRowSource FilterInternal(string expressions)
		{
			return this;
		}

		protected override IEnumerable<GroupByIndexes> GroupByCore(string[] columnNames)
		{
			string[] formattedColumnNames = new string[columnNames.Length];
			for (int index = 0; index < columnNames.Length; index++)
			{
				formattedColumnNames[index] = VisualiserDataSet.GetColumnName(columnNames[index], VisualiserDataSet.GetCollectionName(TableName));
			}

			return base.GroupByCore(formattedColumnNames);
		}

		protected override string MissingColumnErrorMessage
		{
			get
			{
				return Res.GetString("7267fdc8-898b-41c8-a73c-0569d4eca74a", "The template may have been updated since this document was last modified. Please, either click on Modify and 'Reset and Close' or 'Save and Close' after you have reviewed current modification.");
			}
		}
	}
}
