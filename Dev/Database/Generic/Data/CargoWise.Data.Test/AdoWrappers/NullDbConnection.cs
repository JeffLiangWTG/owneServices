using System.Data;

namespace CargoWise.Data.Testing
{
	public class NullDbConnection : DbConnection
	{
		public NullDbConnection()
			: base(Db.DefaultDataProviderFactory, "serverName", "databaseName")
		{
		}

		public override string UserLogin => throw new System.NotImplementedException();

		protected override IDbConnection OpenNewDbConnection()
		{
			throw new System.NotImplementedException();
		}
	}
}
