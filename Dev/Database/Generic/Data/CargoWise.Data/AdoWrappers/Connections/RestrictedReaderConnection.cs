using CargoWise.Common;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	public class RestrictedReaderConnection : DbConnection<RestrictedReaderLoginCredentials>
	{
		protected RestrictedReaderConnection(string applicationNameSuffix = null) : base(applicationNameSuffix)
		{
		}

		protected RestrictedReaderConnection(string serverName, string databaseName, string applicationNameSuffix = null)
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

		public override string UserLogin => RestrictedReaderLoginCredentials.UserNameFor(fInitialDatabaseName);

		public static RestrictedReaderConnection New(string applicationNameSuffix = null)
		{
			return new RestrictedReaderConnection(applicationNameSuffix);
		}

		public static RestrictedReaderConnection New(string serverName, string databaseName, string applicationNameSuffix = null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return new RestrictedReaderConnection(serverName, databaseName, applicationNameSuffix);
		}

		// Only when OpenConnectionIfClosed fails due to all kinds of open connection SqlExceptions,
		// IOpenConnectionErrorHandler.HandleError is used to handle errors if useErrorHandler is specified to true.
		public override bool HandleError(System.Data.Common.DbException sqlException)
		{
			var lastResult = base.HandleError(sqlException);
			if (lastResult)
			{
				return true;
			}

			var exceptionType = new DbErrorMatch(sqlException).ExceptionType;
			if (exceptionType == DbErrorType.LoginFailedForUser)
			{
				// One case for this fix is:
				// reader login doesn't exist on secondary server, once failover, secondary becomes primary,
				// login exception will occur when generating report
				Db.FixReaderLogin(ServerName);

				// Retry but without error handler to make sure connection is open for later use and avoid recursive call as well.
				// Exception will still be thrown if fix above doesn't work
				OpenConnectionIfClosed(useErrorHandler: false);
				return true;
			}

			return false;
		}
	}
}
