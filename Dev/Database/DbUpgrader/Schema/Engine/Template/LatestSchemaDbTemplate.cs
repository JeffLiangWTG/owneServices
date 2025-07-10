using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	public abstract class LatestSchemaDbTemplate : AuxiliaryDbCreator
	{
		public LatestSchemaDbTemplate(IUpgradeManager manager, string templateDbName, string serverName = null)
			: base(templateDbName, serverName)
		{
			Argument.NotNull(manager, nameof(manager));

			this.manager = manager;
		}

		protected override void SetupDatabaseAfterCreation(DbConnection conn)
		{
			conn.DefaultCommandTimeOutInSeconds = 0;

			ShowInfoMessage("Setting database delayed transaction durability");
			// this is faster than a transaction because it doesn't need rolling back at the end
			conn.ExecuteNonQuery($"ALTER DATABASE [{dbName}] SET DELAYED_DURABILITY = FORCED;");

			ShowInfoMessage("Creating all database objects");
			CreateAllDbObjects(conn);

			ShowInfoMessage("Setting upgrade manager key");
			SetManagerKey(conn);

			ShowInfoMessage("Setting database read-only");
			SetDatabaseReadOnly(conn);
		}

		void SetManagerKey(DbConnection connection)
		{
			manager.SetManagerKey(connection, dbName);
		}

		protected override void CreateDropExisting_Core(AdminConnection conn)
		{
			if (!manager.ManagerKeyExists(conn, dbName))
			{
				base.CreateDropExisting_Core(conn);
			}
		}

		protected abstract void CreateAllDbObjects(DbConnection conn);

		protected virtual void SetDatabaseReadOnly(DbConnection conn)
		{
			string sqlText = String.Format("ALTER DATABASE [{0}] SET READ_ONLY", this.dbName);
			conn.ExecuteNonQueryWithRetry(
				sqlText,
				retryingEventHandler: (sender, e) => ShowInfoMessage($"    Retrying in {e.Delay} due to LockTimeout exception"));
		}

		protected readonly IUpgradeManager manager;

		protected void ShowInfoMessage(string message)
		{
			manager.ShowInfoMessage($"\t{message}");
		}
	}
}
