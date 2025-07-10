using CargoWise.Common;
using CargoWise.Data.Providers.Common;

namespace CargoWise.Data
{
	//
	// This is part of the old Db class, responsible for spawning new "ExtraConnection" instances,
	// generally using the singleton database credentials.
	//
	// In the future, this should be abstracted away and/or deleted to provide a much more rigorous
	// public API for managing new database connections.
	//
	// Static dependencies from Db:
	// - ServerName
	// - DatabaseName
	// - MainDbRestrictedWriterLoginName
	// - MainDbRestrictedWriterLoginPassword
	//

	public partial class Db
	{
		/// <summary>
		/// Creates a new connection to a given server/database
		/// </summary>
		public static DbConnection NewExtraConnection(string serverName, string databaseName, string userLogin, string userPassword, string applicationNameSuffix = null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));
			Argument.NotNullOrEmpty(userLogin, nameof(userLogin));
			Argument.NotNull(userPassword, nameof(userPassword));

			return new ExtraConnection(serverName, databaseName, userLogin, userPassword, applicationNameSuffix);
		}

		public static DbConnection NewExtraConnectionIntegratedSecurityEnabled(string serverName, string databaseName)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return new ExtraConnection(new IntegratedSecuritySqlDataProviderFactory(), serverName, databaseName, null, null);
		}

		public static DbConnection NewExtraConnectionToMainDb()
		{
			return NewExtraRestrictedWriterConnection(ServerName, DatabaseName);
		}

		public static DbConnection NewExtraConnectionToMainDb(string applicationNameSuffix)
		{
			return NewExtraRestrictedWriterConnection(ServerName, DatabaseName, applicationNameSuffix);
		}

		public static DbConnection NewExtraConnectionWithMainDbCredentials(string serverName, string databaseName, string applicationNameSuffix = null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return RestrictedWriterConnection.New(Db.DefaultDataProviderFactory, serverName, databaseName, applicationNameSuffix);
		}

		public static DbConnection NewExtraConnectionToMainDbWithReaderCredentials(string applicationNameSuffix = null)
		{
			return RestrictedReaderConnection.New(applicationNameSuffix);
		}

		public static DbConnection NewExtraConnectionWithTargetDatabaseSpecificCredentials(string serverName, string databaseName)
		{
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));
			Argument.NotNullOrEmpty(serverName, nameof(serverName));

			return NewExtraRestrictedWriterConnection(serverName, databaseName);
		}

		public static DbConnection NewExtraUnrestrictedWriterConnection(string serverName, string databaseName, string applicationNameSuffix = null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return UnrestrictedWriterConnection.New(serverName, databaseName, applicationNameSuffix);
		}

		public static DbConnection NewExtraRestrictedReaderConnection(string serverName, string databaseName, string applicationNameSuffix = null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return RestrictedReaderConnection.New(serverName, databaseName, applicationNameSuffix);
		}

		public static DbConnection NewExtraRestrictedWriterConnection(string serverName, string databaseName, string applicationNameSuffix = null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return RestrictedWriterConnection.New(serverName, databaseName, applicationNameSuffix);
		}
	}
}
