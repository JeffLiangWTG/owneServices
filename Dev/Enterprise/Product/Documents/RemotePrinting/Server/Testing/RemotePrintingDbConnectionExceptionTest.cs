using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class RemotePrintingDbConnectionExceptionTest : TestCaseWithFactory
	{
		public void TestNewWithDatabaseUpgradedException()
		{
			var exception = RemotePrintingDbConnectionException.New(new DatabaseUpgradedException());
			AssertEquals(535, exception.ErrorCode);

			exception = RemotePrintingDbConnectionException.New(new DatabaseUpgradedException(false));
			AssertEquals(536, exception.ErrorCode);

			exception = RemotePrintingDbConnectionException.New(new DatabaseUpgradeInProgressException());
			AssertEquals(537, exception.ErrorCode);
		}
	}
}
