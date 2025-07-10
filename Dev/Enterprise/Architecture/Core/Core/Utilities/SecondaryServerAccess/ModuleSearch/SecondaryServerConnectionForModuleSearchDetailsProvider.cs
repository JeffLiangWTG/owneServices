using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public static class SecondaryServerConnectionForModuleSearchDetailsProvider
	{
		public static bool IsSecondaryDbEnabled
		{
			get { return ObjectFactory.Get<ISystemDataRegistry>().EnableModuleQueryFromSecondaryDbReplica && (hasSecondaryDbServerNamesInRegistry || IsPartOfAlwaysOn) && AllSecondaryServerNames.Count > 0; }
		}

		[ThreadStatic]
		static Dictionary<string, DbConnection> secondaryServerDbConnectionsMap;

		public static void AddDbServer(string serverName, DbConnection connection)
		{
			if (secondaryServerDbConnectionsMap == null)
			{
				secondaryServerDbConnectionsMap = new Dictionary<string, DbConnection>();
			}

			if (!secondaryServerDbConnectionsMap.ContainsKey(serverName))
			{
				secondaryServerDbConnectionsMap.Add(serverName, connection);
			}
		}

		public static DbConnection GetDbServer(string serverName)
		{
			if (secondaryServerDbConnectionsMap == null || serverName.IsNullOrEmpty())
			{
				return null;
			}

			secondaryServerDbConnectionsMap.TryGetValue(serverName, out var connection);
			return connection;
		}

		public static void RemoveDbServer(string serverName)
		{
			secondaryServerDbConnectionsMap?.Remove(serverName);
			allSecondaryServerNames?.Remove(serverName);
		}

		static bool hasSecondaryDbServerNamesInRegistry => secondaryDBServerNamesInRegistry?.Length > 0;

		static string[] secondaryDBServerNamesInRegistry => ObjectFactory.Get<ISystemDataRegistry>().ModuleQueryDbServerNames;

		static bool IsPartOfAlwaysOn
		{
			get
			{
				if (!isPartOfAlwaysOn.HasValue)
				{
					isPartOfAlwaysOn = AlwaysOn.IsDbPartOfAlwaysOn(Db.Connection, Db.DatabaseName);
				}
				return isPartOfAlwaysOn.Value;
			}
		}

		[ThreadStatic]
		static bool? isPartOfAlwaysOn;

		static List<string> AllSecondaryServerNames
		{
			get
			{
				if (allSecondaryServerNames == null)
				{
					if (hasSecondaryDbServerNamesInRegistry)
					{
						allSecondaryServerNames = secondaryDBServerNamesInRegistry.ToList();
					}
					else
					{
						allSecondaryServerNames = AlwaysOnHelper.AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(Db.DatabaseName, useCache: false).ToList() ?? new List<string>();
					}
				}

				return allSecondaryServerNames;
			}
		}

		[ThreadStatic]
		static List<string> allSecondaryServerNames;

		static Random random { get; } = new Random();

		public static string CurrentSecondaryServerName => AllSecondaryServerNames.Count > 0 ? AllSecondaryServerNames[random.Next(AllSecondaryServerNames.Count)] : string.Empty;

#if DEBUG

		public static void ClearCache()
		{
			secondaryServerDbConnectionsMap = null;
			isPartOfAlwaysOn = null;
			allSecondaryServerNames = null;
		}

#endif
	}
}
