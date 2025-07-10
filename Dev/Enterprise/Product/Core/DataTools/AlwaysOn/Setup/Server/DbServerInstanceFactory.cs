

namespace Enterprise.AlwaysOn.Setup
{
	public static class DbServerInstanceFactory
	{
		public static ISecondaryServerInstance ConnectAndValidateSecondaryServer(SqlServerInfo serverInfo, IPrimaryServerInstance primaryServer)
		{
			var result = new SecondaryServerInstance(serverInfo, primaryServer);
			result.Load();
			return result;
		}

		public static IPrimaryServerInstance ConnectAndValidatePrimaryServer(SqlServerInfo serverInfo)
		{
			var result = new PrimaryServerInstance(serverInfo);
			result.Load();
			return result;
		}
	}
}
