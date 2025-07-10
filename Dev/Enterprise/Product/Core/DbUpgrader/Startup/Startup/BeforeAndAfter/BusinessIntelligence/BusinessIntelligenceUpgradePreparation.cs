using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Startup
{
	abstract class BusinessIntelligenceUpgradePreparation
	{
		protected BusinessIntelligenceUpgradePreparation(AdminConnection mainDbConnection, AdminConnection biConnection, IVersionChangeInfo upgVersionInfo)
		{
			this.mainDbConnection = mainDbConnection;
			this.biConnection = biConnection;
			this.upgVersionInfo = upgVersionInfo;
			mainDbName = ((ICurrentDbControl)mainDbConnection).InitialDatabase;
		}

		protected AdminConnection mainDbConnection;
		protected AdminConnection biConnection;
		protected IVersionChangeInfo upgVersionInfo;
		protected readonly string mainDbName;

		protected abstract bool MustRecreateBiDatabase { get; }
		protected abstract string BiDatabaseName { get; }
		protected abstract string BiDatabaseType { get; }

		public virtual void PerformNonTransactionalSettings(IUpgradeTaskWorkflowLogger logger)
		{
			using (var createDbConnection = Db.NewAdminConnection(biConnection.ServerName, ((ICurrentDbControl)biConnection).InitialDatabase))
			{
				ApplyBiServerConfigurationSettings(logger);
				CreateBiDatabase(logger, BiDatabaseName, createDbConnection, MustRecreateBiDatabase);
				PerformNonTransactionalDatabaseSettings(logger);
				logger.ShowInfoMessage(".");
			}
		}

		#region BI Database creation

		void CreateBiDatabase(IUpgradeTaskWorkflowLogger logger, string dbName, AdminConnection adminConnection, bool mustRecreateDatabase)
		{
			if (mustRecreateDatabase)
			{
				CreateDatabaseDropExisting(logger, dbName, adminConnection);
			}
			else
			{
				CreateDatabaseIfNotExists(logger, dbName, adminConnection);
			}
		}

		void CreateDatabaseDropExisting(IUpgradeTaskWorkflowLogger logger, string biDbName, AdminConnection adminConnection)
		{
			logger.StartTask(string.Format(CultureInfo.InvariantCulture, "Creating database [{0}].", biDbName));
			IDbCreator dbCreator = new EmptyDbCreator(biDbName, dataPath: null, mapDbLogins: true);
			dbCreator.CreateDropExisting(adminConnection);
			DataUtils.AddDbExtendedProperty(adminConnection, "ShouldCreateBackup", "True", biDbName);
			SetDatabaseFileGrowth(adminConnection, biDbName);
		}

		public void CreateDatabaseIfNotExists(IUpgradeTaskWorkflowLogger logger, string biDbName, AdminConnection adminConnection)
		{
			if (!biConnection.DatabaseExists(biDbName))
			{
				logger.StartTask(string.Format(CultureInfo.InvariantCulture, "Creating database [{0}].", biDbName));
				IDbCreator dbCreator = new EmptyDbCreator(biDbName, dataPath: null, mapDbLogins: true);
				dbCreator.CreateIfNotExists(adminConnection);
				DataUtils.AddDbExtendedProperty(adminConnection, "ShouldCreateBackup", "True", biDbName);
				SetDatabaseFileGrowth(adminConnection, biDbName);
			}
			else
			{
				var userMode = GetDatabaseUserAccessMode(biDbName);
				if (string.Equals(userMode, SingleUser))
				{
					SetDatabaseIntoAccessMode(biDbName, MultiUser);
				}
			}
		}

		void SetDatabaseFileGrowth(AdminConnection adminConnection, string biDbName)
		{
#if DEBUG
			if (NUnit.Framework.TestingState.IsRunningOnDAT)
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(biDbName))
				{
					var manager = new DatabaseFileGroupCreator(adminConnection, biDbName);
					manager.ChangeFileGrowthSize("PRIMARY", 1, "MB");
				}
			}
#endif
		}

		public const string SingleUser = "SINGLE_USER";
		public const string MultiUser = "MULTI_USER";

		internal static string GetDatabaseUserAccessMode(string dbName)
		{
			using (var adminConnection = Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb))
			{
				var sqltext = "select user_access_desc from sys.databases where name = @DbName";
				var command = adminConnection.Command(sqltext);
				command.AddParameter("@DbName", SqlDbType.VarChar, dbName);

				var result = (string)command.ExecuteScalar();
				return result;
			}
		}

		internal static void SetDatabaseIntoAccessMode(string dbName, string accessMode)
		{
			using (var adminConnection = Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb))
			{
				//We need to kill all connected spids to dbName first.
				DbConnectionKiller.KillOtherConnections(adminConnection, dbName);

				var sqltext = FormattableString.Invariant($@"
SET DEADLOCK_PRIORITY HIGH
ALTER DATABASE [{dbName}] SET {accessMode} WITH NO_WAIT
ALTER DATABASE [{dbName}] SET {accessMode} WITH ROLLBACK IMMEDIATE");

				adminConnection.ExecuteNonQuery(sqltext);
			}
		}

		public virtual string AuditBackupFilePath
		{
			get
			{
				if (backupFilePath == null)
				{
					using (((ICurrentDbControl)mainDbConnection).UseDatabase(Db.AuditDatabaseName))
					{
						backupFilePath = DbRegistry.BackupFilePath?.LoadValue(mainDbConnection);
					}
				}
				return backupFilePath;
			}
		}
		string backupFilePath;

		#endregion

		#region BI Server/Database settings

		void ApplyBiServerConfigurationSettings(IUpgradeTaskWorkflowLogger logger)
		{
			DataUtils.SetServerConfigOption(biConnection, "clr enabled", "1");
		}

		protected void PerformNonTransactionalDatabaseSettings(IUpgradeTaskWorkflowLogger logger)
		{
			logger.StartTask($"Ensuring {BiDatabaseType} database settings.");

			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				EnsureDatabaseSettings(biConnection, BiDatabaseName);
			}

			logger.ShowInfoMessage(".");
		}

		void EnsureDatabaseSettings(AdminConnection connection, string dbName)
		{
			UpgradePreparation.AlterDbAuthorisation(connection, dbName);
			DataUtils.EnsureClrEnabledAndTrustworthyOn(connection, dbName);
			DataUtils.SetCompatibilityLevelBasedOnServerVersion(connection, dbName);
		}

		#endregion
	}
}
