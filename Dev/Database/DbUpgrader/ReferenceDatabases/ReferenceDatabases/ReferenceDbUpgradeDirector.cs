using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	public class ReferenceDbUpgradeDirector
	{
		public ReferenceDbUpgradeDirector(IUpgradeContext upgradeContext, DbConnection connection, IUpgradeTaskWorkflowLogger logger)
		{
			this.upgradeContext = upgradeContext;
			this.connection = connection;
			this.logger = logger;
		}

		public void UpgradeReferenceDbs()
		{
			var usedSynonymPrefixes = new List<string>();

			foreach (var factory in ReferenceDbUpgraderFactories)
			{
				var upgrader = factory.New(upgradeContext, connection, logger);
				UpgradeSpecificReferenceDb(upgrader);
				usedSynonymPrefixes.Add(upgrader.SynonymPrefix);
			}

			ReferenceDbSynonymRecreateDirector.DropOldRefDbSynonyms(connection, usedSynonymPrefixes);

			((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
		}

		public void UpgradeSingleRefDatabase()
		{
			CreateSingleRefDatabases(RefDbTableNameResolver.SingleRefDatabaseName);
			if (bool.TryParse(Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var onDat) && onDat)
			{
#if DEBUG
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Reset {0} for DAT testing", RefDbTableNameResolver.DefaultSingleRefDbName));
				OfflineSRDbHelper.ResetSRDbAndPopulateSchema(connection, logger);
#endif
			}
#if DEBUG
			else if (NUnit.Framework.TestingState.IsRunningOnDAT)
			{
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Reset {0} for DAT use", RefDbTableNameResolver.DefaultSingleRefDbName));
				OfflineSRDbHelper.ResetSRDbAndPopulateSchema(connection, logger);
			}
			else if (connection.DatabaseExists(RefDbTableNameResolver.DefaultSingleRefDbName) && DataUtils.LoadDbExtendedProperty(connection, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, RefDbTableNameResolver.DefaultSingleRefDbName) == "Y")
			{
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Reset {0} for local development", RefDbTableNameResolver.DefaultSingleRefDbName));
				using (var resetConnection = Db.NewAdminConnection())
				{
					OfflineSRDbHelper.ResetSRDbAndPopulateSchema(resetConnection, logger);
				}
			}
#endif
			else
			{
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Upgrade {0} with live service", RefDbTableNameResolver.SingleRefDatabaseName));
				UpgradeSRDbWithLiveService(connection, RefDbTableNameResolver.SingleRefDatabaseName);
			}

			logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Begin synonym synchronization for {0} to {1}", connection.CurrentDatabase, RefDbTableNameResolver.SingleRefDatabaseName));
			var creator = new RefDatabaseSynonymRecreator(connection);
			creator.RecreateSynonym();
			logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Begin synonym synchronization for {0} to {1}", connection.CurrentDatabase, RefDbTableNameResolver.SingleRefDatabaseName));
		}

		public void UpgradeSRDbWithLiveService(DbConnection connection, string sRDbname)
		{
			try
			{
#if DEBUG
				DataUtils.DropDbExtendedProperty(connection, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, RefDbTableNameResolver.DefaultSingleRefDbName);
				logger?.ShowInfoMessage("Drop extendedproperty SingleRefDatabaseConsumeDATSnapshot");
#endif
				var upgrader = SetupUpgrader();
				using (((ICurrentDbControl)connection).UseDatabase(sRDbname))
				{
					upgrader.DoUpgrade(((IDbConnectionInternals)connection).ADOTransaction);
				}
			}
			catch (SqlException)
			{
				throw;
			}
#pragma warning disable CS0168 // Variable is declared but never used
			catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
			{
				logger?.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Warning: Cannot upgrade {0} this time, it will be retried later by RDU service task.", RefDbTableNameResolver.SingleRefDatabaseName));
#if DEBUG
				logger?.ShowTaskError("Please use Help -> Database Administration -> Reference Data option and try again");
				logger?.ShowTaskError("Exception: " + ex.ToString());
#endif
			}
		}

		public void CreateSingleRefDatabases(string databaseName)
		{
			var serverList = GetServerList();
			CreateSingleRefDatabaseOnServers(serverList, databaseName);
		}

#if DEBUG
		protected virtual
#endif
		IEnumerable<string> GetServerList()
		{
			var serverList = new HashSet<string>() { Db.ServerName };

			if (AlwaysOn.IsDbPartOfAlwaysOn(connection, Db.DatabaseName))
			{
				var availabilityGroup = AvailabilityGroupInfo.GetGroupInfo((AdminConnection)connection, Db.DatabaseName);
				var secondaryServers = availabilityGroup.GetSecondaryServers((AdminConnection)connection);
				if (secondaryServers != null)
				{
					serverList.UnionWith(secondaryServers);
				}
			}
			if (connection.ServerName != Db.ServerName && !serverList.Contains(connection.ServerName))
			{
				serverList.Add(connection.ServerName);
			}
			return serverList;
		}

		void CreateSingleRefDatabaseOnServers(IEnumerable<string> serverList, string databaseName)
		{
			foreach (var server in serverList)
			{
				try
				{
					CreateSingleRefDatabase(server, databaseName);
				}
				catch (Exception ex)
				{
					logger.ShowTaskError($"Fail to create SRDB on {server} because : {ex.Message}");
				}
			}
		}

#if DEBUG
		protected virtual
#endif
		void CreateSingleRefDatabase(string server, string databaseName)
		{
			using (var createConnection = Db.NewAdminConnection(server, Db.SqlMasterDb))
			{
				if (!createConnection.DatabaseExists(databaseName))
				{
					logger.ShowInfoMessage(string.Format("Initiating reference database on server: {0}", server));
					while (createConnection.RunLocked(string.Format(CultureInfo.InvariantCulture, "CargoWiseOne_SingleReferencDatabase_Creation"), (f) => RefDatabaseInitialiser.CreateRefDbIfNotExists(createConnection, databaseName), null, max_tries: 3, Db.SqlMasterDb) != LockedProcessResult.Completed)
					{
						Thread.Sleep(TimeSpan.FromMilliseconds(1000));
					}
				}
			}
		}

		void UpgradeSpecificReferenceDb(ReferenceDbUpgrader upgrader)
		{
			logger.ShowInfoMessage(
				string.Format("Creating/updating [{0}] reference database (Version: {1} => {2})",
				upgrader.ReferenceName, upgrader.VersionBeforeUpgrade, upgrader.LatestVersion));
			upgrader.CreateAndUpgradeIfRequired();
		}

		public IEnumerable<string> ExclusiveRefDbSuffixList => ReferenceDbUpgraderFactories.Select(f => f.New(upgradeContext, connection, logger).ExclusiveRefDbNameSuffix);

		// ******************* IMPORTANT *******************
		//
		// When removing an item from this list please ensure to remove it from the Model Update Tool's configuration to prevent errors during regen
		// See https://devops.wisetechglobal.com/wtg/InternalTools/_git/WTG.ModelUpdateTool/pullrequest/202452 as an example
		//
		// ******************* IMPORTANT *******************
		internal static IEnumerable<IRefDbUpgraderFactory> ReferenceDbUpgraderFactories
		{
			get
			{
				yield return new AU.AUCmrReferenceDbUpgrader.AUCmrRefDbUpgraderFactory();
				yield return new AU.AUTariffReferenceDbUpgrader.AUTrfRefDbUpgraderFactory();
				yield return new US.USReferenceDbUpgrader.USRefDbUpgraderFactory();
				yield return new NZ.NZTariffReferenceDbUpgrader.NZTrfRefDbUpgraderFactory();
				yield return new CA.CAReferenceDbUpgrader.CARefDbUpgraderFactory();
				yield return new CA.CATariffReferenceDbUpgrader.CATrfRefDbUpgraderFactory();
			}
		}

		#region Helper Methods

#if DEBUG
		protected virtual
#endif
		IRefDataBaseUpgrader SetupUpgrader()
		{
			return new RefDataBaseUpgrader(
				new LoggerWrapper(logger),
				new ServerProxyHelper(new LoggerWrapper(logger), new ClientConfiguration()).GetServerProxy(),
				new DBUpgradeHelper(((IDbConnectionInternals)connection).ADOConnection));
		}
		#endregion

		readonly IUpgradeContext upgradeContext;
		readonly DbConnection connection;
		readonly IUpgradeTaskWorkflowLogger logger;
	}
}
