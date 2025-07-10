namespace Enterprise.DocumentEngine.DataProviders
{
	interface IDataRowSourceXXXX : IDataRowSource
	{
	}

	public interface IDataRowSource
	{
		int RowCount
		{
			get;
		}

		/// <param name="indexes">Should be 1 based.</param>
		IDataRowSource GetRowsFromIndexes(int[] indexes);
		IDataRowSource[] GroupBy(string[] columnNames);
		IDataRowSource Split(int rowsToKeep);
		IDataRowSource GetFirstNRows(int n);
		IDataRowSource Filter(string expressions);
		int GroupCount(string[] columnNames);
	}
}
