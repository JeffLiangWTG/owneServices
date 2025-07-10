namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	using System;
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Shared;

	class StandardRefDbPreparationStrategy : IRefDbPreparationStrategy
	{
		public StandardRefDbPreparationStrategy(string mainDbName, RefDbTypeEnum refDbType, string refDbCountry, DbConnection upgradeConnection)
		{
			this.mainDbName = mainDbName;
			this.refDbType = refDbType;
			this.refDbCountry = refDbCountry;
			this.mainUpgConnection = upgradeConnection;
		}

		protected readonly string mainDbName;
		protected readonly RefDbTypeEnum refDbType;
		protected readonly string refDbCountry;
		protected readonly DbConnection mainUpgConnection;

		protected Action<DbConnection> performUpgradeTasks;

		#region IRefDbPreparationStrategy Members

		public string RefDbName
		{
			get
			{
				return refDbName ?? (refDbName = GetLatestVersionDatabaseName());
			}
		}
		string refDbName;

		int IRefDbPreparationStrategy.GetVersionFromDatabase()
		{
			return GetVersionFromDatabaseSafe();
		}

		void IRefDbPreparationStrategy.PrepareAndUpgradeDatabase(Action<DbConnection> performUpgradeTasksCallback)
		{
			InitialiseStrategy(performUpgradeTasksCallback);
			CreateDatabaseIfNotExists();
			DoPrepareAndUpgradeDatabase();
		}

		#endregion

		protected virtual string GetLatestVersionDatabaseName()
		{
			return CalculatedPrivateRefDbName;
		}

		protected string CalculatedPrivateRefDbName => RefDbTableNameResolver.GetExclusiveRefDbName(mainDbName, refDbType, refDbCountry);

		protected virtual int GetVersionFromDatabaseSafe()
		{
			return GetVersionFromExtendedPropertyWithFallbackToVersionTable(RefDbName);
		}

		protected int GetVersionFromExtendedPropertyWithFallbackToVersionTable(string dbName)
		{
			int versionOnDb = 0;

			if (DbObjectCreator.DatabaseExists(mainUpgConnection, dbName))
			{
				string extendedPtyValue = DataUtils.LoadDbExtendedProperty(mainUpgConnection, ReferenceDbUpgrader.VersionPropertyName, dbName);

				versionOnDb = (extendedPtyValue == null)
					? CopyVersionValueFromTableToExtendedProperty(dbName)
					: Convert.ToInt32(extendedPtyValue);
			}

			return versionOnDb;
		}

		int CopyVersionValueFromTableToExtendedProperty(string dbName)
		{
			string sqlText = String.Format(
				"IF EXISTS(SELECT null FROM [{0}].sys.tables WHERE name = '{1}') SELECT SV_Version FROM [{0}]..[{1}];",
				dbName, ReferenceDbUpgrader.VersionPropertyName);

			object queryResult = mainUpgConnection.ExecuteScalar(sqlText);
			int result = (queryResult == null || queryResult == DBNull.Value) ? 0 : Convert.ToInt32(queryResult);

			DataUtils.SaveDbExtendedProperty(mainUpgConnection, ReferenceDbUpgrader.VersionPropertyName, result.ToString(), dbName);

			return result;
		}

		void InitialiseStrategy(Action<DbConnection> performUpgradeTasksCallback)
		{
			performUpgradeTasks = performUpgradeTasksCallback;
		}

		protected virtual void CreateDatabaseIfNotExists()
		{
			if (!mainUpgConnection.DatabaseExists(RefDbName))
			{
				using (var adminConnection = Db.NewAdminConnection(mainUpgConnection.ServerName, Db.SqlMasterDb))
				{
					DoCreateDatabase(adminConnection);
				}
			}
		}

		protected void DoCreateDatabase(AdminConnection adminConnection)
		{
			GetPhysicalDatabaseSettings(out var dataPath, out var logPath, out var dataInitialSizeMb, out var logInitialSizeMb, out var dataGrowthMb, out var logGrowthMb);

			adminConnection.DefaultCommandTimeOutInSeconds = DbCommand.Timeout.Infinite;
			adminConnection.CreateDatabase(RefDbName, dataPath, logPath, dataInitialSizeMb, logInitialSizeMb, dataGrowthMb, logGrowthMb);
		}

		public const int RefDbLogFileIntialSizeMb = 100;
		public const int RefDbLogFileGrowthMb = 100;

		protected virtual void GetPhysicalDatabaseSettings(out string dataPath, out string logPath, out int? dataSizeMb, out int? logSizeMb, out int? dataGrowthMb, out int? logGrowthMb)
		{
			dataPath = logPath = null;
			dataSizeMb = dataGrowthMb = null;

			logSizeMb = RefDbLogFileIntialSizeMb;
			logGrowthMb = RefDbLogFileGrowthMb;
		}

		protected virtual void DoPrepareAndUpgradeDatabase()
		{
			using (((ICurrentDbControl)mainUpgConnection).UseDatabase(RefDbName))
			{
				DropLegacyVersionTableIfExists(mainUpgConnection);
				performUpgradeTasks(mainUpgConnection);
			}
		}

		protected void DropLegacyVersionTableIfExists(DbConnection connection)
		{
			string sqlText = String.Format(
				"IF EXISTS(SELECT null FROM sys.tables WHERE name = '{0}') DROP TABLE [{0}];",
				ReferenceDbUpgrader.VersionPropertyName);
			connection.ExecuteNonQuery(sqlText);
		}
	}
}
