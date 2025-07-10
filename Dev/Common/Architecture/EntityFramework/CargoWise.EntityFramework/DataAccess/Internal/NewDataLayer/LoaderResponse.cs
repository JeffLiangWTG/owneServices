using System;

namespace CargoWise.EntityFramework
{
	public class LoaderResponse
	{
		public LoaderResponse(int databaseRoundTrips, DataRowLoadResponse[] dataRowLoadResponses)
		{
			if (databaseRoundTrips < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(databaseRoundTrips));
			}
			if (dataRowLoadResponses == null)
			{
				throw new ArgumentNullException(nameof(dataRowLoadResponses));
			}

			this.databaseRoundTrips = databaseRoundTrips;
			this.dataRowLoadResponses = dataRowLoadResponses;
		}

		public int DatabaseRoundTrips
		{
			get { return databaseRoundTrips; }
		}

		public DataRowLoadResponse[] DataRowLoadResponses
		{
			get { return dataRowLoadResponses; }
		}

		readonly int databaseRoundTrips;
		readonly DataRowLoadResponse[] dataRowLoadResponses;
	}
}
