using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	sealed class DummyChecker : IChecker
	{
		public void Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			checkCount++;
		}
		public string Description
		{
			get { return "Test checker"; }
		}
		public int CheckCount
		{
			get { return checkCount; }
		}
		int checkCount;
	}
}
