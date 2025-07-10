using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.Integration;
using Enterprise.ZArchitecture.AlwaysOnHelper;

namespace Enterprise.DbHealth.Check
{
	using static FormattableString;

	class AlwaysOnGroupChecker : IChecker
	{
		#region IChecker Memebers

		void IChecker.Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			if (connection.ServerEdition == DbConnection.SqlServerEdition.EnterpriseDeveloper)
			{
				string mainDbName = ((ICurrentDbControl)connection).InitialDatabase;
				CheckDatabasesIncludedInTheAlwaysOnGroup(connection.ServerName, mainDbName, warningList, logger);
			}
		}

		string IChecker.Description
		{
			get { return "Check all operational databases included in the SQL AlwaysOn Availability Group"; }
		}

		#endregion

		const string WarningAction = "Ensure all databases are added to the Availability Group";
		const string WarningPrefix = "Some databases are not part of the Availability Group:\r\n\r\n";

		void CheckDatabasesIncludedInTheAlwaysOnGroup(string serverName, string mainDbName, DbHealthWarningList warningList, ILogger logger)
		{
			using (var adminConnection = Db.NewAdminConnection(serverName, mainDbName))
			{
				var availabilityGroup = AlwaysOnHelper.GetAvailabilityGroupInfo(adminConnection, mainDbName);

				if (availabilityGroup.GroupId != Guid.Empty)
				{
					var operationalDbs = GetDatabases(adminConnection);
					CheckPrimaryServer(adminConnection, warningList, availabilityGroup, operationalDbs);
					CheckSecondaryServers(warningList, availabilityGroup, availabilityGroup.GetSecondaryServers(adminConnection), operationalDbs, logger);
				}
			}
		}

		static IEnumerable<string> GetDatabases(AdminConnection connection)
		{
			return (RefDbTableNameResolver.ShouldUseSharedAvailabilityGroupDatabases(connection))
				? connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef)
				: connection.GetDatabases(DatabaseType.Operational | DatabaseType.UserRepository | DatabaseType.Audit);
		}

		#region Primary Server

		void CheckPrimaryServer(DbConnection connection, DbHealthWarningList warningList, AvailabilityGroupInfo availabilityGroup, IEnumerable<string> operationalDbs)
		{
			var primaryServerMissingDbs = GetDatabasesNotIncludedInPrimaryReplica(connection, availabilityGroup.PrimaryReplicaId, operationalDbs);

			if (primaryServerMissingDbs.Count > 0)
			{
				string source = Invariant($"Group [{availabilityGroup.GroupName}] - Primary Replica [{connection.ServerNameReportedByDatabase}]");
				string description = WarningPrefix + string.Join("\r\n", primaryServerMissingDbs);
				warningList.Add(new DatabaseWarning(source, DatabaseWarning.AlwaysOnWarning, description, WarningAction));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		List<string> GetDatabasesNotIncludedInPrimaryReplica(DbConnection connection, Guid primaryReplicaId, IEnumerable<string> operationalDbs)
		{
			var result = new List<string>();
			string dbCsvList = string.Join(",", operationalDbs.Select(db => Invariant($"'{db}'")));

			string sqlText = Invariant($@"
				SELECT name FROM sys.databases
				WHERE name in ({dbCsvList})
				AND (replica_id is null OR replica_id != '{primaryReplicaId}')");

			using (var reader = connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(reader[0].ToString());
				}
			}

			return result;
		}

		#endregion

		#region Secondary Servers

		void CheckSecondaryServers(DbHealthWarningList warningList, AvailabilityGroupInfo availabilityGroup, IEnumerable<string> secondaryServers, IEnumerable<string> operationalDbs, ILogger logger)
		{
			if (secondaryServers.Any())
			{
				CheckDatabasesNotIncludedInSecondaryReplicas(availabilityGroup, secondaryServers, operationalDbs, warningList, logger);
			}
		}

		void CheckDatabasesNotIncludedInSecondaryReplicas(AvailabilityGroupInfo availabilityGroup, IEnumerable<string> secondaryServers, IEnumerable<string> operationalDbs, DbHealthWarningList warningList, ILogger logger)
		{
			var secServerChecker = new AlwaysOnSecondaryServerChecker(availabilityGroup.GroupId, operationalDbs);

			Parallel.ForEach(secondaryServers, secServerChecker.CheckForDatabasesNotIncludedInSecondaryReplica);

			string secServerWarningLog = string.Join("\r\n", secServerChecker.SecondaryServerExceptions
				.Select(kvp => string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", kvp.Key, kvp.Value.Message)));

			if (!string.IsNullOrWhiteSpace(secServerWarningLog))
			{
				logger.Log(LogType.Warning, "Unable to check AlwaysOn from the following servers:\r\n" + secServerWarningLog);
			}

			foreach (var secServerMissingDbList in secServerChecker.SecondaryServerMissingDatabaseLists)
			{
				string source = string.Format(CultureInfo.InvariantCulture, "Group [{0}] - Secondary Replica [{1}]", availabilityGroup.GroupName, secServerMissingDbList.Key);
				string description = WarningPrefix + string.Join("\r\n", secServerMissingDbList.Value);
				warningList.Add(new DatabaseWarning(source, DatabaseWarning.AlwaysOnWarning, description, WarningAction));
			}
		}

		internal class AlwaysOnSecondaryServerChecker
		{
			public AlwaysOnSecondaryServerChecker(Guid availabilityGroupId, IEnumerable<string> operationalDbs)
			{
				string dbValueTable = string.Join(",", operationalDbs.Select(db => Invariant($"('{db}')")));
				this.availabilityGroupId = availabilityGroupId;
				this.getSecondaryServerMissingDatabasesSql =
					string.Format(CultureInfo.InvariantCulture, getSecondaryServerMissingDatabasesSqlRaw, dbValueTable);
			}

			#region Multithreaded

			[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			public void CheckForDatabasesNotIncludedInSecondaryReplica(string secondaryServer)
			{
				try
				{
					var missingDbList = new List<string>();

					using (var secondaryServerConnection = Db.NewAdminConnection(secondaryServer, Db.SqlMasterDb))
					{
						var command = secondaryServerConnection.Command(getSecondaryServerMissingDatabasesSql);
						command.AddParameter("@groupId", SqlDbType.UniqueIdentifier, availabilityGroupId);
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								missingDbList.Add(reader[0].ToString());
							}
						}
					}

					if (missingDbList.Count > 0)
					{
						secServerMissingDbLists[secondaryServer] = missingDbList;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					secServerExceptions[secondaryServer] = ex;
				}
			}

			#endregion

			public IEnumerable<KeyValuePair<string, IEnumerable<string>>> SecondaryServerMissingDatabaseLists
			{
				get
				{
					foreach (var secondaryServer in secServerMissingDbLists)
					{
						yield return secondaryServer;
					}
				}
			}

			public IEnumerable<KeyValuePair<string, Exception>> SecondaryServerExceptions
			{
				get
				{
					foreach (var secondaryServer in secServerExceptions)
					{
						yield return secondaryServer;
					}
				}
			}

			readonly ConcurrentDictionary<string, IEnumerable<string>> secServerMissingDbLists = new ConcurrentDictionary<string, IEnumerable<string>>();
			readonly ConcurrentDictionary<string, Exception> secServerExceptions = new ConcurrentDictionary<string, Exception>();
			readonly string getSecondaryServerMissingDatabasesSql;
			readonly Guid availabilityGroupId;

			const string getSecondaryServerMissingDatabasesSqlRaw = @"
				SELECT
					priDb.name
				FROM
					(VALUES {0}) AS priDb (name)
					LEFT JOIN
					(
						sys.databases secDb
						INNER JOIN sys.dm_hadr_availability_replica_states secReplica
							ON secReplica.replica_id = secDb.replica_id
							AND secReplica.group_id = @groupId
							AND secReplica.is_local = 1 AND secReplica.role = 2
					) ON secDb.name = priDb.name
				WHERE
					secDb.name is null";
		}

		#endregion
	}
}
