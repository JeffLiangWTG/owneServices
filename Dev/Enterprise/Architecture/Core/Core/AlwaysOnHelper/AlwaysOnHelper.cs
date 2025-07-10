using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.AlwaysOnHelper
{
	public static class AlwaysOnHelper
	{
		public static readonly Overridable<Func<DbConnection, string>> OverridablePrimaryReplicaFullName
			= new Overridable<Func<DbConnection, string>>(connection => DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(connection.ServerNameReportedByDatabase));

		#region ReplicaNames
		public static LockedProcessResult RunActionWithAlwaysOnLock(AdminConnection connection, Action<bool> callBackAction)
		{
			const string eDocExclusiveKeyForAON = "AlwaysOnExclusiveLockKey";
			return connection.RunLocked(eDocExclusiveKeyForAON, callBackAction, max_tries: 1, dbName: Db.DatabaseName);
		}

		public static string[] GetAlwaysOnSecondaryReplicaNames(string dbName, bool useCache)
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				return GetAlwaysOnSecondaryReplicaNames(connection, dbName, useCache);
			}
		}

		public static string[] GetAlwaysOnSecondaryReplicaNames(AdminConnection connection, string dbName, bool useCache)
		{
			var registry = EnvProxy.Instance.Registry;
			var allReplicasInfos = new List<AlwaysOnReplicaInfo>();

			var primaryReplicaFullName = OverridablePrimaryReplicaFullName.Value(connection);

			if (registry.UseAlwaysOnReplicaCache)
			{
				var cachedAlwaysOnReplicaCachedInfos = registry.AlwaysOnReplicaCachedInfos;
				if (cachedAlwaysOnReplicaCachedInfos.Length == 0
					|| !useCache
					|| !cachedAlwaysOnReplicaCachedInfos.Any(info => info.ReplicaServerName.Equals(primaryReplicaFullName, StringComparison.OrdinalIgnoreCase)))
				{
					allReplicasInfos = AlwaysOn.GetAlwaysOnReplicaInfos(connection, dbName);
					UpdateAlwaysOnCache(allReplicasInfos);
				}
				else
				{
					allReplicasInfos.AddRange(cachedAlwaysOnReplicaCachedInfos);
				}
			}
			else
			{
				allReplicasInfos = AlwaysOn.GetAlwaysOnReplicaInfos(connection, dbName);
			}

			return allReplicasInfos
				.Where(r => string.Compare(r.ReplicaServerName, primaryReplicaFullName, StringComparison.OrdinalIgnoreCase) != 0)
				.Select(r => r.ReplicaServerName).ToArray();
		}

		static void UpdateAlwaysOnCache(List<AlwaysOnReplicaInfo> allReplicasInfos)
		{
			if (!IsArrayEqualsByElements(EnvProxy.Instance.Registry.AlwaysOnReplicaCachedInfos, allReplicasInfos))
			{
				EnvProxy.Instance.Registry.AlwaysOnReplicaCachedInfos = allReplicasInfos.ToArray();
			}

			bool IsArrayEqualsByElements(AlwaysOnReplicaInfo[] arrayA, List<AlwaysOnReplicaInfo> arrayB)
			{
				return arrayA.Length == arrayB.Count && arrayA.All(objA => arrayB.Any(objA.Equals));
			}
		}
		#endregion ReplicaNames

		#region GroupInfo

		public static AvailabilityGroupInfo GetAvailabilityGroupInfo(AdminConnection connection, string dbName)
		{
			var registry = EnvProxy.Instance.Registry;
			if (registry.UseAlwaysOnReplicaCache)
			{
				var groupInfo = GetGroupInfoFromCache(connection, dbName, registry.AvailabilityGroupInfo);
				if (groupInfo == null)
				{
					groupInfo = AvailabilityGroupInfo.GetGroupInfo(connection, dbName);
					registry.AvailabilityGroupInfo = groupInfo.ConvertToArray();
				}

				return groupInfo;
			}

			return AvailabilityGroupInfo.GetGroupInfo(connection, dbName);
		}

		static AvailabilityGroupInfo GetGroupInfoFromCache(AdminConnection connection, string dbName, string[] groupInfos)
		{
			if (groupInfos.Length == 0)
			{
				return null;
			}
			else
			{
				var groupInfo = new AvailabilityGroupInfo(groupInfos);
				var currentReplicaId = GetCurrentReplicaId(connection, dbName);
				return groupInfo.PrimaryReplicaId == currentReplicaId ? groupInfo : null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static Guid GetCurrentReplicaId(AdminConnection connection, string dbName)
		{
			using (connection.UseMasterDb())
			using (var cmd = connection.Command($"SELECT ISNULL(replica_id, 0x0) FROM sys.databases WHERE name = @dbName"))
			{
				cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
				return (Guid)cmd.ExecuteScalar();
			}
		}

		#endregion GroupInfo
	}
}
