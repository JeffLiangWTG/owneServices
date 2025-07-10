using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using CargoWise.Common;

#if DEBUG
[assembly: InternalsVisibleTo("CargoWise.Data.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
namespace CargoWise.Data.SqlServer
{
	public partial class AvailabilityGroupInfo
	{
		public static AvailabilityGroupInfo GetGroupInfo(AdminConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(dbName, nameof(dbName));

			return GetGroupInfoCore(string.Format(CultureInfo.InvariantCulture, getGroupInfoByPrimaryDbNameSqlScript, dbName), connection);
		}

		public static AvailabilityGroupInfo GetFirstGroupInfoOnServer(AdminConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			return GetGroupInfoCore(getGroupInfoSqlScript, connection);
		}

		static AvailabilityGroupInfo GetGroupInfoCore(string sqlScript, AdminConnection connection)
		{
			if (OveridableGroupInfo.Value != null)
			{
				return OveridableGroupInfo.Value;
			}

			AvailabilityGroupInfo result = null;
			using (var reader = connection.Command(sqlScript).ExecuteReader())
			{
				result = GetGroupInfoFromDataReader(reader);
			}

			return result ?? new AvailabilityGroupInfo(null, Guid.Empty, Guid.Empty, null);
		}

		internal static AvailabilityGroupInfo GetGroupInfoFromDataReader(IDataReader reader)
		{
			Argument.NotNull(reader, nameof(reader));
			AvailabilityGroupInfo agInfo = null;
			while (reader.Read())
			{
				if (agInfo == null)
				{
					agInfo = new AvailabilityGroupInfo(reader.GetString(0), reader.GetGuid(1), reader.GetGuid(2), new List<string>() { reader.GetString(3) });
				}
				else
				{
					agInfo.listenerIPs.Add(reader.GetString(3));
				}
			}
			return agInfo;
		}

		public AvailabilityGroupInfo()
		{
			this.listenerIPs = new List<string>();
		}

		public AvailabilityGroupInfo(string groupName, Guid groupId, Guid primaryReplicaId, List<string> listenerIP)
		{
			this.groupName = groupName;
			this.groupId = groupId;
			this.primaryReplicaId = primaryReplicaId;
			this.listenerIPs = listenerIP;
		}

		public string GroupName
		{
			get { return groupName; }
		}
		readonly string groupName;

		public Guid GroupId
		{
			get { return groupId; }
		}
		readonly Guid groupId;

		public Guid PrimaryReplicaId
		{
			get { return primaryReplicaId; }
		}
		readonly Guid primaryReplicaId;

		public List<string> ListenerIPs
		{
			get { return listenerIPs; }
		}
		readonly List<string> listenerIPs;

		const string commonGroupInfoSqlScript = @"
SELECT
	ag.name AS GroupName,
	ag.group_id AS GroupId,
	priReplica.replica_id,
	ISNULL(ipAdr.ip_address, '') ip_address
FROM
	sys.databases priDb
	INNER JOIN sys.dm_hadr_database_replica_states priReplica ON priReplica.replica_id = priDb.replica_id AND priDb.group_database_id = priReplica.group_database_id
	INNER JOIN sys.availability_groups ag ON priReplica.group_id = ag.group_id
	LEFT OUTER JOIN sys.availability_group_listeners lis ON ag.group_id = lis.group_id
	LEFT OUTER JOIN sys.availability_group_listener_ip_addresses ipAdr ON lis.listener_id = ipAdr.listener_id
";

		const string getGroupInfoByPrimaryDbNameSqlScript = commonGroupInfoSqlScript + "WHERE priDb.name = '{0}'";
		const string getGroupInfoSqlScript = @"SELECT TOP(1) * FROM (" + commonGroupInfoSqlScript + ") AS subquery";

#if DEBUG
		virtual
#endif
		public IEnumerable<string> GetSecondaryServers(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));
			var result = new List<string>();

			string sqlText = @"
					SELECT secReplica.replica_server_name
					FROM
						sys.availability_replicas secReplica
						INNER JOIN sys.dm_hadr_availability_replica_states secReplicaState
							ON secReplicaState.replica_id = secReplica.replica_id
					WHERE
						secReplica.group_id = @groupId
						AND secReplicaState.is_local = 0
						AND secReplicaState.role = 2";

			var command = connection.Command(sqlText);
			command.AddParameter("@groupId", SqlDbType.UniqueIdentifier, groupId);

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(reader[0].ToString()));
				}
			}

			return result;
		}

		public AvailabilityGroupInfo(string[] groupInfo)
		{
			Argument.NotNull(groupInfo, nameof(groupInfo));
			if (groupInfo.Length < 4)
			{
				groupName = null;
				groupId = Guid.Empty;
				primaryReplicaId = Guid.Empty;
				listenerIPs = null;
			}
			else
			{
				groupName = groupInfo[0];
				if (!Guid.TryParse(groupInfo[1], out groupId))
				{
					throw new ArgumentException("groupId is not a valid GUID", "groupInfo[1]");
				}

				if (!Guid.TryParse(groupInfo[2], out primaryReplicaId))
				{
					throw new ArgumentException("primaryReplicaId is not a valid GUID", "groupInfo[2]");
				}

				var ips = groupInfo[3]
					?? throw new ArgumentException("listenerIPs should not be null", "groupInfo[3]");
				listenerIPs = new List<string>(ips.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
			}
		}

		public string[] ConvertToArray()
		{
			if (!string.IsNullOrEmpty(GroupName)
				&& GroupId != Guid.Empty
				&& PrimaryReplicaId != Guid.Empty
				&& ListenerIPs != null)
			{
				return new string[]
				{
					GroupName,
					GroupId.ToString(),
					PrimaryReplicaId.ToString(),
					string.Join(",", ListenerIPs)
				};
			}
			return Array.Empty<string>();
		}

		static readonly Overridable<AvailabilityGroupInfo> OveridableGroupInfo = new Overridable<AvailabilityGroupInfo>(null);
	}
}

#region Test
#if DEBUG

namespace CargoWise.Data.SqlServer
{
	public partial class AvailabilityGroupInfo
	{
		public static IDisposable SetupGroupInfoForTest(AvailabilityGroupInfo mockInfo)
		{
			var originalInfo = OveridableGroupInfo.Value;
			OveridableGroupInfo.Value = mockInfo;

			return new DisposableAction(() => OveridableGroupInfo.Value = originalInfo);
		}
	}
}
#endif
#endregion
