#region SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.DataProtection.Administration.SqlServer;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	public static class ListenerFactory
	{
		public static IListener GetForGroup(IDbServerInstance server, IAvailabilityGroup availabilityGroup)
		{
			IListener newListener = null;
			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(server.ServerInfo);

			var getListenersQuery = Invariant($@"
SELECT listeners.dns_name, listeners_net_info.ip_address, listeners_net_info.ip_subnet_mask, listeners.port, listeners.listener_id
FROM sys.availability_group_listeners as listeners JOIN 
	 sys.availability_group_listener_ip_addresses as listeners_net_info
ON listeners.listener_id = listeners_net_info.listener_id
WHERE listeners.group_id = '{availabilityGroup.GroupId}'
;");

			using (var reader = sqlContext.ExecuteReader(getListenersQuery))
			{
				while (reader.Read())
				{
					var dnsName = reader[0];
					var ipAddress = reader[1];
					var subnetMask = reader[2];
					var listenerPort = reader[3];
					var listenerId = reader[4];

					if (dnsName == null || ipAddress == null || subnetMask == null || listenerPort == null || listenerId == null)
					{
						throw new InvalidOperationException("dns_name, ip_address, ip_subnet_mask, port, listener_id. One of those columns returned a null value");
					}
					var newListenerIp = new ListenerIp((string)ipAddress, (string)subnetMask);

					if (newListener == null)
					{
						newListener = new Listener((string)dnsName, (int)listenerPort, server, availabilityGroup, newListenerIp, (string)listenerId);
					}
					else
					{
						newListener.IpAddresses.Add(newListenerIp);
					}
				}
			}
			return newListener;
		}

		//public static IListener New(string name, string ipAddress, string subnetMask, int port, IDbServerInstance server, IAvailabilityGroup availabilityGroup)
		public static IListener New(string name, int port, IDbServerInstance server, IAvailabilityGroup availabilityGroup, List<ListenerIp> listenerIps, string id = null)
		{
			var newListener = new Listener(name, port, server, availabilityGroup, null, id);
			foreach (var ip in listenerIps)
			{
				newListener.IpAddresses.Add(new ListenerIp(ip.IpAddress, ip.SubnetMask));
			}

			return newListener;
		}

		public static void Save(IListener listener, ISqlExecutionContext sqlContext)
		{
			if (listener.Id != null)
			{
				var modifyListenerStatements = GetModifyListenerSqlStatements(listener, sqlContext);
				foreach (var modifySql in modifyListenerStatements)
				{
					sqlContext.ExecuteNonQuery(modifySql);
				}
			}
			else
			{
				var saveListenerSql = GetSaveListenerSqlStatement(listener);
				sqlContext.ExecuteNonQuery(saveListenerSql);
			}
		}

		public static List<string> GetModifyListenerSqlStatements(IListener listener, ISqlExecutionContext sqlContext)
		{
			var unsavedListeners = new List<string>();
			var savedIpList = new List<ListenerIp>();
			var id = listener.Id;
			var ipQuery = $"SELECT ip_address, ip_subnet_mask FROM sys.availability_group_listener_ip_addresses WHERE listener_id = '{id}';";

			using (var reader = sqlContext.ExecuteReader(ipQuery))
			{
				while (reader.Read())
				{
					var readIpAddress = new ListenerIp(reader.GetString(0), reader.GetString(1));
					savedIpList.Add(readIpAddress);
				}
				reader.Close();
			}

			if (savedIpList.IsNullOrEmpty())
			{
				unsavedListeners.Add(GetSaveListenerSqlStatement(listener));
				return unsavedListeners;
			}

			var unsavedIpList = listener.IpAddresses.Where(listenerIp => !savedIpList.Any(queryIp => queryIp.IpAddress == listenerIp.IpAddress && queryIp.SubnetMask == listenerIp.SubnetMask)).ToList();

			foreach (var ip in unsavedIpList)
			{
				var formattedIp = Invariant($"(N'{ip.IpAddress}', N'{ip.SubnetMask}')");
				var formattedQuery = Invariant($"ALTER AVAILABILITY GROUP [{listener.AvailabilityGroup.GroupName}] MODIFY LISTENER N'{listener.DnsName}' (ADD IP {formattedIp});");
				unsavedListeners.Add(formattedQuery);
			}

			return unsavedListeners;
		}

		public static string GetSaveListenerSqlStatement(IListener listener)
		{
			var ips = new List<string>();
			foreach (var ip in listener.IpAddresses)
			{
				ips.Add(Invariant($"(N'{ip.IpAddress}', N'{ip.SubnetMask}')"));
			}

			return Invariant($@"ALTER AVAILABILITY GROUP [{listener.AvailabilityGroup.GroupName}] ADD LISTENER N'{listener.DnsName}' (WITH IP ({string.Join(",", ips)}), PORT={listener.Port});"); // This is a SQL Script
		}

		public static void Remove(IListener listener)
		{
			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(listener.Server.ServerInfo);
			var originalNameSql = $"SELECT dns_name FROM sys.availability_group_listeners WHERE listener_id = '{listener.Id}'"; // This is a SQL Script

			var originalName = (string)sqlContext.ExecuteScalar(originalNameSql);

			var removeSql = Invariant($"ALTER AVAILABILITY GROUP [{listener.AvailabilityGroup.GroupName}] REMOVE LISTENER N'{originalName}'"); // This is a SQL Script
			sqlContext.ExecuteNonQuery(removeSql);
		}
	}
}
#endregion
