using CargoWise.Data;
using Enterprise.AlwaysOn.Setup;

namespace Enterprise.AlwaysOn.Testing
{
	class DbServerInstanceFactoryTest : AlwaysOnTestFixture
	{
		public void TestConnectAndValidateServer()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName);
			AssertNoExceptionThrown(() => DbServerInstanceFactory.ConnectAndValidatePrimaryServer(serverInfo));
		}
	}
}
