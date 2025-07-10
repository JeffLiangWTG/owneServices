using System.Data;
using System.Linq;

namespace Enterprise.DocumentEngine.DataProviders
{
	sealed class ReportDataSource : ADODataSource, IDataRowSourceXXXX, IDataSourceTable
	{
		public ReportDataSource(DataTable table)
			: base(table)
		{
		}

		public ReportDataSource(DataTable wrappedSource, int[] wrappedRowsIndexes)
			: base(wrappedSource, wrappedRowsIndexes)
		{
		}

		ReportDataSource(ADODataSource wrappedSource, int[] wrappedRowsIndexes)
			: base(wrappedSource, wrappedRowsIndexes)
		{
		}

		protected override ADODataSource GetNewDataSource(DataTable table)
		{
			return new ReportDataSource(table);
		}

		protected override ADODataSource GetNewDataSource(ADODataSource wrappedSource, int[] wrappedRowsIndexes)
		{
			return new ReportDataSource(wrappedSource, wrappedRowsIndexes);
		}

		protected override IDataRowSource FilterInternal(string expressions)
		{
			return GetNewDataSource(this, Enumerable.Range(0, RowCount).Where(index => new ReportFilterDataSource(RowByIndex(index), expressions).Evaluate()).ToArray());
		}

		#region ITableInternal Members

		DataTable IDataSourceTable.Table
		{
			get { return Table; }
		}

		#endregion
	}
}
