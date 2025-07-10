using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	abstract public class DatabaseLoginTestCase : TestCase
	{
		protected abstract DatabaseLogin DatabaseLogin { get; }

		public virtual void TestIsImpersonateEnterpriseDbUser()
		{
			Assert(!DatabaseLogin.IsImpersonateEnterpriseDbUser);
		}

		protected AdminConnection TestAdminConnection => TestConnection as AdminConnection;

		protected DbConnection TestConnection
		{
			get
			{
				if (adminConnection == null)
				{
					adminConnection = Db.NewAdminConnection();
				}
				return adminConnection;
			}
		}
		AdminConnection adminConnection;
	}
}
