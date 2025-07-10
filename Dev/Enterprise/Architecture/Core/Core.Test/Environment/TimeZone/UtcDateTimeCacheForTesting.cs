using System;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class UtcDateTimeCacheForTesting : UtcDateTimeCache
	{
		protected override DateTime GetUtcNowFromClientPc()
		{
			if (UtcNow_Override == DateTime.MinValue)
			{
				return base.GetUtcNowFromClientPc();
			}
			else
			{
				return UtcNow_Override;
			}
		}

		protected override DateTime GetUtcNowFromDatabaseServer(DbConnection connection)
		{
			if (UtcNow_Override == DateTime.MinValue)
			{
				return base.GetUtcNowFromDatabaseServer(connection);
			}
			else
			{
				return UtcNow_Override;
			}
		}

		public DateTime UtcNow_Override = DateTime.MinValue;
	}
}
