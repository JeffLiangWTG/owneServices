using System;
using System.Data;

namespace CargoWise.Data.Testing
{
	sealed class DbLoginForTest : IDisposable
	{
		public DbLoginForTest(string userLoginName, string userPassword, bool checkPolicy = false)
		{
			LoginName = userLoginName;
			Password = userPassword;
			CheckPolicy = checkPolicy;

			CreateDbLogin();
		}

		public void CreateDbLogin()
		{
			DropDbLogin();

			using (DbConnection conn = Db.NewAdminConnection())
			{
				string sqlText = string.Format(
					"CREATE LOGIN [{0}] WITH PASSWORD = '{1}', DEFAULT_DATABASE = [{2}], CHECK_POLICY = {3}",
					LoginName, Password, Db.DatabaseName, CheckPolicy ? "ON" : "OFF");

				using (DbCommand cmd = conn.Command(sqlText))
				{
					cmd.ExecuteNonQuery();

					cmd.CommandText = string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", LoginName);
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void DropDbLogin()
		{
			using (DbConnection conn = Db.NewAdminConnection())
			{
				using (DbCommand cmd = conn.Command("IF EXISTS (SELECT null FROM sys.database_principals WHERE name = @userLogin) EXEC sp_dropuser @name_in_db = @userLogin"))
				{
					cmd.AddParameter("@userLogin", SqlDbType.VarChar, 128, LoginName);
					cmd.ExecuteNonQuery();

					cmd.CommandText = "IF EXISTS (SELECT null FROM sys.server_principals WHERE name = @userLogin) EXEC master..sp_droplogin @loginame = @userLogin";
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void CreateSysAdminLogin()
		{
			using var adminConnection = Db.NewAdminConnection();
			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{LoginName}] WITH PASSWORD = '{Password}', CHECK_POLICY = {(CheckPolicy ? "ON" : "OFF")};
EXEC master..sp_addsrvrolemember @loginame = N'{LoginName}', @rolename = N'sysadmin';
");
		}

		public string LoginName { get; private set; }
		public string Password { get; private set; }
		public bool CheckPolicy { get; private set; }

		#region IDisposable Members

		public void Dispose()
		{
			DropDbLogin();
		}

		#endregion
	}
}
