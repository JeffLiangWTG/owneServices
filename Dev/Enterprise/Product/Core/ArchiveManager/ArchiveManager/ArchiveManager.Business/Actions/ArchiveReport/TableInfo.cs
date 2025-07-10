namespace Enterprise.ArchiveManager.Business.Actions.ArchiveReport
{
	public readonly struct TableInfo
	{
		public TableInfo(long rowCount, long dataKB, long indexSizeKB)
		{
			RowCount = rowCount;
			DataKB = dataKB;
			IndexSizeKB = indexSizeKB;
		}

		public long RowCount { get; }

		public long DataKB { get; }

		public long IndexSizeKB { get; }
	}
}
