using System;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class UtcDateTimeCacheForLongLatencyTesting : UtcDateTimeCache
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		readonly int oneWayTripInMilliseconds = 4 * 1000 / 2;

		protected override DateTime GetUtcNowFromDatabaseServer(DbConnection connection)
		{
			System.Threading.Thread.Sleep(oneWayTripInMilliseconds);
			DateTime result = base.GetUtcNowFromDatabaseServer(connection);
			System.Threading.Thread.Sleep(oneWayTripInMilliseconds);
			return result;
		}
	}
}
