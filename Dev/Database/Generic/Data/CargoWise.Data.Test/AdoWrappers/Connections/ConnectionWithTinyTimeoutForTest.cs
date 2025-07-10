using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data.Testing
{
	public class ConnectionWithTinyTimeoutForTest : DbConnection<RestrictedWriterLoginCredentials>
	{
		public ConnectionWithTinyTimeoutForTest(string serverName, string databaseName) : base(
			new SqlDataProviderFactory(
				ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataServiceFactory>().CreateSystemService(serverName, databaseName),
				ProtectedDataService.GlobalServiceProvider.GetRequiredService<ISqlConnectionProvider>()),
			serverName,
			databaseName)
		{
		}

		public ConnectionWithTinyTimeoutForTest(IDataProviderFactory dataProviderFactory, string serverName, string databaseName, string applicationNameSuffix = null) : base(dataProviderFactory, serverName, databaseName, applicationNameSuffix, null)
		{
		}

		protected override int ConnectionStringTimeoutValue
		{
			get { return 1; }
		}

		public void OpenConnectionCore_Exposed()
		{
			OpenConnectionCore();
		}
		public override string UserLogin => RestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);
	}

	internal class ExtraConnectionWithTinyTimeoutForTest : ExtraConnection
	{
		public ExtraConnectionWithTinyTimeoutForTest(string serverName, string databaseName, string userLogin, string userPwd)
			: base(serverName, databaseName, userLogin, userPwd)
		{
		}
		public ExtraConnectionWithTinyTimeoutForTest(IDataProviderFactory dataProviderFactory, string serverName, string databaseName, string userLogin, string userPwd)
			: base(dataProviderFactory, serverName, databaseName, userLogin, userPwd)
		{
		}

		protected override int ConnectionStringTimeoutValue
		{
			get { return 1; }
		}

		public void OpenConnectionCore_Exposed()
		{
			OpenConnectionCore();
		}
	}
}
