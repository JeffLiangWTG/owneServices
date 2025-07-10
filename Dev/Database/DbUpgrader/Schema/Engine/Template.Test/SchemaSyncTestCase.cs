using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	public abstract class SchemaSyncTestCase : TestCase
	{
		protected DbConnection TestConnection
		{
			get { return adminConnection ?? (adminConnection = Db.NewAdminConnection()); }
		}
		AdminConnection adminConnection;
	}
}
