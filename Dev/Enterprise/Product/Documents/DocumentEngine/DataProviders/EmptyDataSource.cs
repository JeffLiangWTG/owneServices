namespace Enterprise.DocumentEngine.DataProviders
{
	class EmptyDataSource : IDataRowSource
	{
		public int RowCount
		{
			get { return 0; }
		}

		public IDataRowSource[] GroupBy(string[] groupByColumnName)
		{
			return null;
		}

		public IDataRowSource Split(int rowsToKeep)
		{
			return null;
		}

		public IDataRowSource GetFirstNRows(int n)
		{
			return null;
		}

		public IDataRowSource Filter(string expressions)
		{
			return null;
		}

		public int GroupCount(string[] columnNames)
		{
			return 0;
		}

		public IDataRowSource GetRowsFromIndexes(int[] indexes)
		{
			return null;
		}
	}
}
