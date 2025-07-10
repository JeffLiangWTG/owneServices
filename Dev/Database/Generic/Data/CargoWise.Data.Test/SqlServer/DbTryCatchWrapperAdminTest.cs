using System;
using System.Data;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbTryCatchWrapperAdminTest : TestCase
	{
		public void TestCannotEnableLoginWithEmptyPasswordErrorNumber()
		{
			using (AdminConnection connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();

				try
				{
					connection.ExecuteNonQuery("CREATE LOGIN F54DF11FCB02411785D6058251E6C7DE WITH PASSWORD = '', CHECK_POLICY = OFF");
					DbTryCatchWrapperTest.AssertErrorNumber(
						connection,
						"ALTER LOGIN F54DF11FCB02411785D6058251E6C7DE ENABLE",
						DbTryCatchWrapper.CannotEnableLoginWithEmptyPasswordErrorNumber);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestGetEnableDbLoginCommandSafeExpectsBracketEscapedLogin()
		{
			string loginName = "LoginWith]Bracket";

			using (AdminConnection connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();

				try
				{
					CreateAndAssertTestLogin(connection, loginName);

					var tryCatchWrapper = new DbTryCatchWrapper();
					connection.ExecuteNonQuery(tryCatchWrapper.GetEnableDbLoginCommandSafe(loginName));
					AssertLoginEnable(connection, loginName, true);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestGetEnableDbLoginCommandSafeExpectsSingleQuotationEscapedLogin()
		{
			string loginName = "LoginWith'Bracket";

			using (AdminConnection connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();

				try
				{
					CreateAndAssertTestLogin(connection, loginName);

					var tryCatchWrapper = new DbTryCatchWrapper();
					connection.ExecuteNonQuery(tryCatchWrapper.GetEnableDbLoginCommandSafe(loginName));
					AssertLoginEnable(connection, loginName, true);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		void CreateAndAssertTestLogin(AdminConnection connection, string loginName)
		{
			string sqlText = string.Format("CREATE LOGIN [{0}] WITH PASSWORD = '', CHECK_POLICY = OFF;", loginName.Replace("]", "]]"));
			connection.ExecuteNonQuery(sqlText);
			AssertLoginEnable(connection, loginName, true);
			sqlText = string.Format("ALTER LOGIN [{0}] DISABLE;", loginName.Replace("]", "]]"));
			connection.ExecuteNonQuery(sqlText);
			AssertLoginEnable(connection, loginName, false);
		}

		void AssertLoginEnable(DbConnection connection, string loginName, bool expected)
		{
			using (var cmd = connection.Command("SELECT is_disabled FROM sys.server_principals WHERE name = @userLogin"))
			{
				cmd.AddParameter("@userLogin", SqlDbType.VarChar, 128, loginName);
				object objResult = cmd.ExecuteScalar();
				bool isEnabled = (objResult != null && !Convert.ToBoolean(objResult));
				AssertEquals(loginName + " is enabled?", expected, isEnabled);
			}
		}
	}
}
