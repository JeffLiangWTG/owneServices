namespace Enterprise.AlwaysOn.Setup
{
	public class DbFileAndTransactionLogInfo
	{
		public DbFileAndTransactionLogInfo(string dbName, decimal logSequenceNumber, DatabaseFileCollection dbFiles)
		{
			DbName = dbName;
			LogSequenceNumber = logSequenceNumber;
			Files = dbFiles;
		}

		public string DbName { get; }
		public decimal LogSequenceNumber { get; }
		public DatabaseFileCollection Files { get; }
	}
}
