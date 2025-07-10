using System.Data;
using System.Globalization;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbTransactionTest : TransactionedTestCase
	{
		public void TestFixReaderLogin_Reader()
		{
			var loginName = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);

			using (var adminConn = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			{
				var sqlTextDropLogin = string.Format(CultureInfo.InvariantCulture, @"
					IF  EXISTS (SELECT * FROM master.sys.server_principals WHERE name = @name)
						DROP LOGIN [{0}]", loginName);
				adminConn.ExecuteNonQuery(sqlTextDropLogin, p => p.AddParameter("@name", SqlDbType.NVarChar, 128, loginName));
			}

			AssertExceptionThrown(typeof(SqlException), () =>
			{
				using (var connection = new ObsoleteReaderConnectionForTest())
				{
					connection.EnsureIsOpen();
				}
			});

			Db.FixReaderLogin(Db.ServerName);

			AssertNoExceptionThrown(() =>
			{
				using (var connection = Db.NewExtraConnectionToMainDbWithReaderCredentials())
				{
					connection.EnsureIsOpen();
				}
			});
		}

		public void TestFixReaderLogin_RestrictedReader()
		{
			var loginName = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);

			using (var adminConn = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			{
				var sqlTextDropLogin = string.Format(CultureInfo.InvariantCulture, @"
					IF  EXISTS (SELECT * FROM master.sys.server_principals WHERE name = @name)
						DROP LOGIN [{0}]", loginName);
				adminConn.ExecuteNonQuery(sqlTextDropLogin, p => p.AddParameter("@name", SqlDbType.NVarChar, 128, loginName));
			}

			AssertExceptionThrown(typeof(SqlException), () =>
			{
				using (var connection = new RestrictedReaderConnectionWithNoRepairForTest())
				{
					connection.EnsureIsOpen();
				}
			});

			Db.FixReaderLogin(Db.ServerName);

			AssertNoExceptionThrown(() =>
			{
				using (var connection = new RestrictedReaderConnectionWithNoRepairForTest())
				{
					connection.EnsureIsOpen();
				}
			});
		}

		internal class ObsoleteReaderConnectionForTest : DbConnection<CargoWiseReaderLoginCredentials>
		{
			public override string UserLogin => CargoWiseReaderLoginCredentials.UserNameFor(fInitialDatabaseName);
		}
		class RestrictedReaderConnectionWithNoRepairForTest : DbConnection<RestrictedReaderLoginCredentials>
		{
			public override string UserLogin => RestrictedReaderLoginCredentials.UserNameFor(fInitialDatabaseName);
		}

		public void TestWindowsIntegratedSecurityLogin()
		{
			AssertNoExceptionThrown(() =>
			{
				using (var connection = Db.NewExtraConnectionIntegratedSecurityEnabled(Db.ServerName, Db.DatabaseName))
				{
					connection.EnsureIsOpen();
				}
			});
		}
	}
}
