using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	interface IDbOptionsChecker
	{
		void CheckDelayedDurabilityOff(DbConnection connection, ILogger logger);
	}
}
