using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.AlwaysOn.Setup;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Testing
{
	class DbManagerTest : AlwaysOnTestFixture
	{
		public void TestCreateDatabaseLoginEscapesSingleQuote()
		{
			using (var connection = Db.NewAdminConnection())
			{
				const string loginName = "Erm'al";
				const string passwordHash = "0x0200017422689ED954059C41A7DB8AA3E84F6C73B01100D82C32EF42644908917D3D963F17CAF656490CD02B1F8FCEACFAE325F9089B29B541E695EF104D9015A038EF36ADA7";

				var dropScript = Invariant($"IF EXISTS (SELECT null FROM sys.server_principals WHERE [name] = '{loginName.QuoteEscapedName('\'')}') DROP LOGIN [{loginName}]");
				var existScript = Invariant($"SELECT name FROM sys.server_principals WHERE [name] = '{loginName.QuoteEscapedName('\'')}'");

				try
				{
					AssertNoExceptionThrown(() =>
					{
						connection.ExecuteNonQuery(dropScript);
						AssertNull(connection.ExecuteScalar(existScript));

						var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);
						DbManager.CreateDatabaseLogin(sqlContext, loginName, DbLoginInfo.LoginType.SQL, connection.CurrentDatabase, null, passwordHash);

						AssertEquals(loginName, (string)connection.ExecuteScalar(existScript));
					});
				}
				finally
				{
					connection.ExecuteNonQuery(dropScript);
				}
			}
		}

		/// <summary>
		/// Ensures error message is current (SQL Server)
		/// </summary>
		public void TestLoginFailedForUserError()
		{
			var nonExistentLogin = "~this@must#be$an%invalid^login!";

			using (var connection = Db.NewExtraConnection(Db.ServerName, Db.SqlMasterDb, nonExistentLogin, ""))
			{
				AssertSqlExceptionThrown("Invalid Login", DbManager.LoginFailedForUserErrorNumber, () => connection.EnsureIsOpen());
			}

			using (var connection = Db.NewExtraConnection(Db.ServerName, Db.SqlMasterDb, "UserName", "~some@crappy#pwd!"))
			{
				AssertSqlExceptionThrown("Invalid password", DbManager.LoginFailedForUserErrorNumber, () => connection.EnsureIsOpen());
			}
		}

		/// <summary>
		/// Ensure error number constants are valid
		/// </summary>
		public void TestSqlServerErrorNumbersMatch()
		{
			using (var connection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.SqlMasterDb))
			{
				DbManagerTest.AssertErrorMessageMatches(connection, DbManager.LoginFailedForUserErrorNumber, "Login failed for user '%.*ls'.%.*ls%.*ls");
				DbManagerTest.AssertErrorMessageMatches(connection, DbManager.LoginAlreadyExistsErrorNumber, "The server principal '%s' already exists.");
			}
		}

		void AssertSqlExceptionThrown(string assertMessage, int errorNumber, Action codeToRun)
		{
			try
			{
				codeToRun();
			}
			catch (Exception ex)
			{
				AssertEquals(assertMessage, typeof(SqlException).FullName, ex.GetType().FullName);
				AssertEquals(assertMessage, errorNumber, (ex as SqlException).Number);
			}
		}

		public static void AssertErrorMessageMatches(DbConnection connection, int errorNumber, string expectedMessage)
		{
			var sqlText = "SELECT text FROM sys.messages WHERE language_id = 1033 AND message_id = " + errorNumber;
			var actualMessage = connection.ExecuteScalar(sqlText).ToString();
			AssertEquals(Invariant($"Error #{errorNumber} message"), expectedMessage, actualMessage);
		}
	}
}
