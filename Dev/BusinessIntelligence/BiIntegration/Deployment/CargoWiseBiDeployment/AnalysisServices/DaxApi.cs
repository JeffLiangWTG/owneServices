using System;
using System.Data;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Bi.Deployment.AnalysisServices
{
	public static class DaxApi
	{
		public static DataTable GetDaxQueryResult(string ssasDatabaseName, string daxQuery)
		{
			DataTable data = new DataTable();

			VerifyReportAPIIsEnabled();
			using (Db.DisposableActionForDbConnection())
			using (var server = SsasServer.New(AnalysisServer))
			{
				data = server.GetDataTableFromQuery(ssasDatabaseName, daxQuery);
			}

			if (data.Columns.Count == 0)
			{
				throw new SsasException($"Query returned no results on \"{ssasDatabaseName}\" database. Query:\r\n{daxQuery}"); // Exception message
			}
			return data;
		}

		#region Analysis Services

		public static string AnalysisServer
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return BiServers.LoadAnalysisServerUsingCacheIfPossible(Db.Connection);
				}
			}
		}

		public static bool IsAPIEnabled
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return BiServers.LoadReportAPIUsingCacheIfPossible(Db.Connection);
				}
			}
		}

		static void VerifyReportAPIIsEnabled()
		{
			if (!IsAPIEnabled && !Globals.IsTest)
			{
				throw new Exception(Res.GetString("E04435B5-DF22-483E-9818-9BDB6F35CF27", "The Reporting API has not been enabled on this system"));
			}
		}

		#endregion
	}
}
