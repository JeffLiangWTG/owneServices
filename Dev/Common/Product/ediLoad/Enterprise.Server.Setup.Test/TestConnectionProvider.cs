namespace Enterprise.Server.Setup.Testing
{
	public static class TestConnectionProvider
	{
		public static SqlConnection OpenNewConnection(string databaseName)
		{
			var builder = new SqlConnectionStringBuilder()
			{
				DataSource = ".",
				InitialCatalog = databaseName,
				TrustServerCertificate = true,
				IntegratedSecurity = true,
				Encrypt = false,
			};

#pragma warning disable CW1116 // The existing CargoWise.Data.Db.Connection should be used rather than creating a new one - think twice if you really need a new connection.
			var connection = new SqlConnection(builder.ConnectionString);
#pragma warning restore CW1116
			connection.Open();
			return connection;
		}
	}
}
