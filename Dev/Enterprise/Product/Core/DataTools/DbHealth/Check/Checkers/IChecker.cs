using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	public interface IChecker
	{
		void Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger);
		string Description { get; }
	}
}
