using CargoWise.Common;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	public class RestrictedWriterConnection : DbConnection<RestrictedWriterLoginCredentials>
	{
		protected RestrictedWriterConnection(string applicationNameSuffix = null) : base(applicationNameSuffix)
		{
		}

		protected RestrictedWriterConnection(string serverName, string databaseName, string applicationNameSuffix = null)
			: this(GetDataProviderFactory(serverName, databaseName), serverName, databaseName, applicationNameSuffix)
		{ }

		static IDataProviderFactory GetDataProviderFactory(string serverName, string databaseName)
		{
			var sqlConnectionProvider = ProtectedDataService.GlobalServiceProvider.GetRequiredService<ISqlConnectionProvider>();
			var protectedDataService = ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataServiceFactory>().CreateSystemService(serverName, databaseName);
			var dataProviderFactory = new SqlDataProviderFactory(protectedDataService, sqlConnectionProvider);

			return dataProviderFactory;
		}

		protected RestrictedWriterConnection(IDataProviderFactory dataProviderFactory, string serverName, string databaseName, string applicationNameSuffix = null)
			: base(dataProviderFactory,
				serverName,
				databaseName,
				applicationNameSuffix,
				null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));
		}

		public override string UserLogin => RestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);

		internal static RestrictedWriterConnection New(string applicationNameSuffix = null)
		{
			return new RestrictedWriterConnection(applicationNameSuffix);
		}

		internal static RestrictedWriterConnection New(string serverName, string databaseName, string applicationNameSuffix = null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return new RestrictedWriterConnection(serverName, databaseName, applicationNameSuffix);
		}

		internal static RestrictedWriterConnection New(IDataProviderFactory dataProviderFactory, string serverName, string databaseName, string applicationNameSuffix = null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return new RestrictedWriterConnection(dataProviderFactory, serverName, databaseName, applicationNameSuffix);
		}
	}
}
