using CargoWise.DataProtection;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class ExecuteAsReaderTest : TestCase
	{
		public void TestExecuteAsReaderIfReaderAccountIsMissing()
		{
			using (var connection = Db.NewAdminConnection())
			{
				Db.FixReaderLogin(Db.Connection.ServerName);
				var dbLogin = new RestrictedReaderDatabaseLogin(connection);
				AdoTestUtils.DropDbLoginIfExists(connection, dbLogin.LoginName);

				AssertNoExceptionThrown(() => new ExecuteAsReader().Execute(Db.Connection, () => Db.Connection.Command("Select GetDate()")));

				new DbSecurityNonTransactionedTest().AssertDatabasePrincipal(connection, Db.DatabaseName, dbLogin.LoginName, true);
			}
		}

		[UseSnapshotProtection]
		public void TestExecuteAsReaderCannotAccessHrm()
		{
			AssertExceptionThrown("The ExecuteAsReader should not have access to sensitive data", typeof(SqlException), "The SELECT permission was denied on the object 'GlbStaffRemuneration'", SelectSensitiveInfo, assertStartsWith: true);

			void SelectSensitiveInfo()
				=> new ExecuteAsReader().Execute(Db.Connection, () => Db.Connection.ExecuteNonQuery("SELECT COUNT(*) FROM hrm.GlbStaffRemuneration"));
		}

		public void TestExecuteAsReaderImpersonatedLogin()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var restrictedReaderLoginName = RestrictedReaderLoginCredentials.UserNameFor(connection.CurrentDatabase);

				new ExecuteAsReader().Execute(Db.Connection,
					() =>
					{
						AssertEquals(restrictedReaderLoginName, Db.Connection.ImpersonatedLogin);
						return Db.Connection.Command("Select GetDate()");
					});

				AssertNull(Db.Connection.ImpersonatedLogin);
			}
		}

		[UseSnapshotProtection]
		public void TestExecuteAsReaderImpersonatedLogin_WithException()
		{
			var restrictedReaderLoginName = RestrictedReaderLoginCredentials.UserNameFor(Db.Connection.CurrentDatabase);

			AssertExceptionThrown("The ExecuteAsReader should not have access to sensitive data", typeof(SqlException), "The SELECT permission was denied on the object 'GlbStaffRemuneration'", SelectSensitiveInfo, assertStartsWith: true);

			void SelectSensitiveInfo()
				=> new ExecuteAsReader().Execute(Db.Connection,
				() =>
				{
					AssertEquals(restrictedReaderLoginName, Db.Connection.ImpersonatedLogin);
					return Db.Connection.ExecuteNonQuery("SELECT COUNT(*) FROM hrm.GlbStaffRemuneration");
				});

			AssertNull(Db.Connection.ImpersonatedLogin);
		}
	}
}
