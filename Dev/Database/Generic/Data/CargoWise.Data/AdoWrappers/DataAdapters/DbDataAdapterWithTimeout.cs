using System.Data;
using System.Data.Common;

namespace CargoWise.Data
{
	internal class DbDataAdapterWithTimeout : DbDataAdapter
	{
		public DbDataAdapterWithTimeout(System.Data.Common.DbCommand command, int timeoutSeconds)
		{
			SelectCommand = command;
			this.timeoutSeconds = timeoutSeconds;
		}

		readonly int timeoutSeconds;

		protected override int Fill(DataTable[] dataTables, IDataReader dataReader, int startRecord, int maxRecords)
		{
			var dataReaderWrapper = new DataReaderTimeoutWrapper(dataReader, timeoutSeconds);
			return base.Fill(dataTables, dataReaderWrapper, startRecord, maxRecords);
		}

		protected override int Fill(DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords)
		{
			var dataReaderWrapper = new DataReaderTimeoutWrapper(dataReader, timeoutSeconds);
			return base.Fill(dataSet, srcTable, dataReaderWrapper, startRecord, maxRecords);
		}
	}
}