using System.Data;
using CargoWise.Common;
using CargoWise.Data.Providers.Common;

namespace CargoWise.Data
{
	class ExtraConnection : DbConnection
	{
		protected readonly string fUserLogin;
		protected readonly string fUserPassword;

		public ExtraConnection(string serverName, string databaseName, string userLogin, string userPassword, string applicationNameSuffix = null) : this(new SqlDataProviderFactory(null, null), serverName, databaseName, userLogin, userPassword, applicationNameSuffix)
		{
		}

		public ExtraConnection(IDataProviderFactory dataProviderFactory, string serverName, string databaseName, string userLogin, string userPassword, string applicationNameSuffix = null) : base(dataProviderFactory, serverName, databaseName, applicationNameSuffix)
		{
			Argument.NotNull(dataProviderFactory, nameof(dataProviderFactory));
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			fUserLogin = userLogin;
			fUserPassword = userPassword;
		}

		protected override IDbConnection OpenNewDbConnection()
		{
			var connectionPooling = ConnectionPoolingValue;
			return DataProviderFactory.OpenNewDbConnection(
						fServerName,
						fInitialDatabaseName,
						fUserLogin,
						fUserPassword,
						SuffixedApplicationName,
						ConnectionStringTimeoutValue,
						connectionPooling.IsPooling,
						connectionPooling.LoadBalanceTimeout,
						connectionPooling.MaxPoolSize,
						connectionPooling.MinPoolSize);
		}

		public override string UserLogin
		{
			get { return fUserLogin; }
		}

		internal string UserPassword
		{
			get { return fUserPassword; }
		}
	}
}
