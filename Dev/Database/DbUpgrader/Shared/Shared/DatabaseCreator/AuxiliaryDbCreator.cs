using CargoWise.Data;

namespace Enterprise.DbUpgrader.Shared
{
	public interface IAuxiliaryDbCreator
	{
		void CreateDropExisting();
		void Drop();
	}

	public abstract class AuxiliaryDbCreator : IAuxiliaryDbCreator
	{
		protected AuxiliaryDbCreator(string templateDbName, string serverName = null)
		{
			this.dbName = templateDbName;
			this.serverName = string.IsNullOrEmpty(serverName) ? Db.ServerName : serverName;
		}

		void IAuxiliaryDbCreator.CreateDropExisting()
		{
			using (var conn = Db.NewAdminConnection(serverName, BaseDbName))
			{
				conn.DefaultCommandTimeOutInSeconds = Db.Connection.DefaultCommandTimeOutInSeconds;

				CreateDropExisting_Core(conn);
			}
		}

		protected virtual void CreateDropExisting_Core(AdminConnection conn)
		{
			DbCreator.CreateDropExisting(conn);

			using (((ICurrentDbControl)conn).UseDatabase(dbName))
			{
				SetupDatabaseAfterCreation(conn);
			}
		}

		void IAuxiliaryDbCreator.Drop()
		{
			using (var conn = Db.NewAdminConnection(serverName, Db.SqlMasterDb))
			{
				conn.DefaultCommandTimeOutInSeconds = Db.Connection.DefaultCommandTimeOutInSeconds;
				DbCreator.Drop(conn);
			}
		}

		IDbCreator DbCreator
		{
			get
			{
				if (dbCreator_UsePtyInstead == null)
				{
					dbCreator_UsePtyInstead = new EmptyDbCreator(dbName, (UseTempPathForDbFiles ? CargoWise.IO.Temp.TempPath : null));
				}

				return dbCreator_UsePtyInstead;
			}
		}
		IDbCreator dbCreator_UsePtyInstead;

#if DEBUG
		protected virtual
#endif
		bool UseTempPathForDbFiles
		{
			get { return false; }
		}

#if DEBUG
		protected virtual
#endif
		string BaseDbName
		{
			get { return Db.SqlMasterDb; }
		}

		protected abstract void SetupDatabaseAfterCreation(DbConnection conn);
		protected readonly string dbName;
		protected readonly string serverName;
	}
}
