using CargoWise.Common;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	public class UnrestrictedWriterConnection : DbConnection<UnrestrictedWriterLoginCredentials>
	{
		protected UnrestrictedWriterConnection(string applicationNameSuffix = null) : base(applicationNameSuffix)
		{
		}

		protected UnrestrictedWriterConnection(string serverName, string databaseName, string applicationNameSuffix = null)
			: base(new SqlDataProviderFactory(
				ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataServiceFactory>().CreateSystemService(serverName, databaseName),
				ProtectedDataService.GlobalServiceProvider.GetRequiredService<ISqlConnectionProvider>()),
				serverName,
				databaseName,
				applicationNameSuffix,
				null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));
		}

		public override string UserLogin => UnrestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);

		internal static UnrestrictedWriterConnection New(string applicationNameSuffix = null)
		{
			return new UnrestrictedWriterConnection(applicationNameSuffix);
		}

		internal static UnrestrictedWriterConnection New(string serverName, string databaseName, string applicationNameSuffix = null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return new UnrestrictedWriterConnection(serverName, databaseName, applicationNameSuffix);
		}
	}
}
