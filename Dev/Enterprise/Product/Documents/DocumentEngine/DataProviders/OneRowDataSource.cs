
namespace Enterprise.DocumentEngine.DataProviders
{
	class OneRowDataSource : IDataRowSource
	{
		public const string TableIdentifier = "DummyCollection";

		public int RowCount
		{
			get { return 1; }
		}

		public IDataRowSource[] GroupBy(string[] columnNames)
		{
			return new IDataRowSource[1] { this };
		}

		public int GroupCount(string[] columnNames)
		{
			return 1;
		}

		public IDataRowSource Split(int rowsToKeep)
		{
			return null;
		}

		public IDataRowSource GetFirstNRows(int n)
		{
			return this;
		}

		public IDataRowSource Filter(string expressions)
		{
			return null;
		}

		public IDataRowSource GetRowsFromIndexes(int[] indexes)
		{
			return this;
		}
	}
}
