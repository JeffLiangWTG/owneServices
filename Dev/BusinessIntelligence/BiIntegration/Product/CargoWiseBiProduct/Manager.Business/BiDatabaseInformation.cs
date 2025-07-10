namespace CargoWise.Bi.Product.Manager.Business
{
	public abstract class BiDatabaseInformation
	{
		protected BiDatabaseInformation(string serverName, string dbName)
		{
			ServerName = serverName;
			DatabaseName = dbName;
		}

		public readonly string ServerName;
		public readonly string DatabaseName;
	}
}