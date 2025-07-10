using System;
using CargoWise.Common;
using CargoWise.Data;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.Common
{
	public static class BiServers
	{
		#region AuditAPI Cache

		public static bool LoadAuditAPIUsingCacheIfPossible(DbConnection connection)
		{
			if (auditAPICache == null)
			{
				lock (DbRegistry.BiAuditAPI)
				{
					if (auditAPICache == null)
					{
						auditAPICache = DbRegistry.BiAuditAPI.LoadValue(connection);
					}
				}
			}

			return auditAPICache.Value;
		}

		[ThreadSafe]
		static bool? auditAPICache;

		#endregion

		#region ReportAPI Cache

		public static bool LoadReportAPIUsingCacheIfPossible(DbConnection connection)
		{
			if (reportAPICache == null)
			{
				lock (DbRegistry.BiReportAPI)
				{
					if (reportAPICache == null)
					{
						reportAPICache = DbRegistry.BiReportAPI.LoadValue(connection);
					}
				}
			}

			return reportAPICache.Value;
		}

		[ThreadSafe]
		static bool? reportAPICache;

		#endregion

		#region AuditServer Cache

#if DEBUG
		public static IDisposable TemporarilySetAuditServerToNull()
		{
			isAuditServerNullForTest = true;

			return new DisposableAction(new Action(() =>
			{
				isAuditServerNullForTest = false;
			}));
		}

		[ThreadSafe]
		static bool isAuditServerNullForTest;
#endif

		public static string LoadAuditServerUsingCacheIfPossible(DbConnection connection)
		{
#if DEBUG
			if (isAuditServerNullForTest)
			{
				return null;
			}
#endif

			if (!isAuditServerLoaded)
			{
				lock (DbRegistry.BiAuditServer)
				{
					if (auditServerNameCache == null)
					{
						auditServerNameCache = DbRegistry.BiAuditServer.LoadValue(connection);
#if DEBUG
						if (auditServerNameCache == null)
						{
							auditServerNameCache = connection.ServerName;
						}
#endif
					}
				}
				isAuditServerLoaded = true;
			}

			return auditServerNameCache;
		}

		internal static string LoadAuditServerWithoutCache(DbConnection connection)
		{
#if DEBUG
			if (isAuditServerNullForTest)
			{
				return null;
			}
#endif
			var serverName = DbRegistry.BiAuditServer.LoadValue(Db.Connection);
#if DEBUG
			serverName ??= connection.ServerName;
#endif
			return serverName;
		}

		public static void SaveAuditServer(DbConnection connection, string auditServer)
		{
			lock (DbRegistry.BiAuditServer)
			{
				DbRegistry.BiAuditServer.SaveValue(auditServer, connection);
				isAuditServerLoaded = false;
				auditServerNameCache = null;
			}
		}

		[ThreadSafe]
		static bool isAuditServerLoaded = false;

		[ThreadSafe]
		static string auditServerNameCache;

		#endregion

		#region DataWarehouseServer Cache

#if DEBUG
		public static IDisposable TemporarilySetDataWarehouseServerToNull()
		{
			isDataWarehouseServerNullForTest = true;

			return new DisposableAction(new Action(() =>
			{
				isDataWarehouseServerNullForTest = false;
			}));
		}

		[ThreadSafe]
		static bool isDataWarehouseServerNullForTest = false;
#endif

		public static string LoadDataWarehouseServerUsingCacheIfPossible(DbConnection connection)
		{
#if DEBUG
			if (isDataWarehouseServerNullForTest)
			{
				return null;
			}
#endif

			if (!isDwServerLoaded)
			{
				lock (DbRegistry.BiDataWarehouseServer)
				{
					if (dwServerNameCache == null)
					{
						dwServerNameCache = DbRegistry.BiDataWarehouseServer.LoadValue(connection);
#if DEBUG
						if (dwServerNameCache == null)
						{
							dwServerNameCache = connection.ServerName;
						}
#endif
					}
				}
				isDwServerLoaded = true;
			}

			return dwServerNameCache;
		}

		internal static string LoadDataWarehouseServerWithoutCache(DbConnection connection)
		{
#if DEBUG
			if (isDataWarehouseServerNullForTest)
			{
				return null;
			}
#endif
			var serverName = DbRegistry.BiDataWarehouseServer.LoadValue(Db.Connection);
#if DEBUG
			serverName ??= connection.ServerName;
#endif
			return serverName;
		}

		public static void SaveEdwServer(DbConnection connection, string dwServer)
		{
			lock (DbRegistry.BiDataWarehouseServer)
			{
				DbRegistry.BiDataWarehouseServer.SaveValue(dwServer, connection);
				isDwServerLoaded = false;
				dwServerNameCache = null;
			}
		}

		[ThreadSafe]
		static bool isDwServerLoaded = false;

		[ThreadSafe]
		static string dwServerNameCache;

		#endregion

		#region AnalysisServer Cache

		public static string LoadAnalysisServerUsingCacheIfPossible(DbConnection connection)
		{
			if (analysisServerNameCache == null)
			{
				lock (DbRegistry.BiAnalysisServer)
				{
					if (analysisServerNameCache == null)
					{
						analysisServerNameCache = DbRegistry.BiAnalysisServer.LoadValue(connection);

#if DEBUG
						if (string.IsNullOrWhiteSpace(analysisServerNameCache))
						{
							analysisServerNameCache = connection.ServerName;
						}
#endif
					}
				}
			}

			return analysisServerNameCache;
		}
		[ThreadSafe]
		static string analysisServerNameCache;

		#endregion

		public static void ResetBiServers(DbConnection connection)
		{
			connection.ExecuteNonQuery("DELETE FROM dbo.StmData WHERE SD_Name IN ('BiAuditServer', 'BiDataWarehouseServer', 'BiAnalysisServer', 'BiSsrsWebServiceUrl', 'BiPowerBiWebPortalUrl', 'BiAuditAPI', 'BiResetChangeDataCapture')");
			ClearBiServersCache();
		}

		public static void ClearBiServersCache()
		{
			isAuditServerLoaded = false;
			auditServerNameCache = null;
			isDwServerLoaded = false;
			dwServerNameCache = null;
			analysisServerNameCache = null;
			auditAPICache = null;
			reportAPICache = null;
		}
	}
}
