namespace Enterprise.DbUpgrader.Shared
{
	using System;
	using System.Globalization;
	using CargoWise.Data;

	public class DbRemover
	{
		public DbRemover(string dbName)
		{
			this.dbName = dbName;
		}

		public void Drop(AdminConnection conn)
		{
			DropCore(conn);
		}

		void DropCore(AdminConnection conn)
		{
			DbConnectionKiller.KillOtherConnections(conn, dbName);

			string sqlText = String.Format(CultureInfo.InvariantCulture, DropDbScript, dbName);

			try
			{
				conn.ExecuteNonQuery(sqlText);
			}
			catch (SqlException e)
			{
				if (e.ErrorCode == 3701) // Cannot drop the database because it does not exist or you do not have permission.
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						adminConnection.ExecuteNonQuery(sqlText);
					}
				}
				else
				{
					throw;
				}
			}
		}

		#region Database Scripts

		const string DropDbScript = @"
			IF EXISTS(SELECT name FROM sys.databases WHERE name = '{0}')
			BEGIN
				DROP DATABASE [{0}]
			END";

		#endregion

		protected readonly string dbName;
	}
}
