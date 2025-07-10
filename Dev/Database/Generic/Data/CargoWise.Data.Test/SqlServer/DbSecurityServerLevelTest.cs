using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbSecurityServerLevelTest : TransactionedTestCase
	{
		#region EFFECTIVE DB SECURITY MODE

		public void TestIsDatabaseSecurityOpen()
		{
			var testSecurity = new DbSecurityWithFakeSaPasswordForTesting();
			AssertEquals("IsDatabaseSecurityOpen?", true, testSecurity.IsDatabaseSecurityOpen());
		}

		class DbSecurityWithFakeSaPasswordForTesting : DbSecurity
		{
			protected override string GetSaPwd()
			{
				return "~SOME_INVALID_SA_PASSWORD~" + Guid.NewGuid().ToString();
			}
		}

		#endregion

		#region Implementation

		protected override DbConnection TestConnection
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

		#endregion
	}
}
