using System;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.AlwaysOnHelper;

namespace Enterprise.DbHealth.Check
{
	class AvailabilityGroupListenerChecker : IChecker
	{
		#region IChecker Memebers

		void IChecker.Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			if (AlwaysOn.IsDbPartOfAlwaysOn(connection, Db.DatabaseName))
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					CheckConnectionIsUsingTheAvailabilityGroupListener(adminConnection, warningList);
				}
			}
		}

		string IChecker.Description
		{
			get { return "Check database connections are using the AlwaysOn Listener"; }
		}

		#endregion

		static string WarningSource => FormattableString.Invariant($"{Constants.ProductName} availability group checker");
		const string WarningMessage_NoListenerConnection = "This application is not using the availability group listener to connect to the database.";
		const string WarningAction_NoListenerConnection = "Ensure this application is using the availability group listener to connect to the database.";
		static string WarningMessage_NoListenerConfigured => FormattableString.Invariant($"{Constants.ProductName} databases are joined to an availability group but there is no listener configured for this group.");
		const string WarningAction_NoListenerConfigured = "Configure a listener for the availability group and ensure the application is using the availability group listener to connect to the database.";

		void CheckConnectionIsUsingTheAvailabilityGroupListener(AdminConnection connection, DbHealthWarningList warningList)
		{
			var ag = AlwaysOnHelper.GetAvailabilityGroupInfo(connection, Db.DatabaseName);
			var connectionIP = GetConnectionIP(connection);

			CheckAgAndConnectionIP(ag, connectionIP, warningList);
		}

		internal void CheckAgAndConnectionIP(AvailabilityGroupInfo ag, string connectionIP, DbHealthWarningList warningList)
		{
			if (ag.ListenerIPs == null)
			{
				warningList.Add(new DatabaseWarning(WarningSource, DatabaseWarning.AlwaysOnWarning, WarningMessage_NoListenerConfigured, WarningAction_NoListenerConfigured));
			}
			else if (!ag.ListenerIPs.Exists(x => String.Compare(x, connectionIP, StringComparison.OrdinalIgnoreCase) == 0))
			{
				warningList.Add(new DatabaseWarning(WarningSource, DatabaseWarning.AlwaysOnWarning, WarningMessage_NoListenerConnection, WarningAction_NoListenerConnection));
			}
		}

		public string GetConnectionIP(DbConnection connection)
		{
			var sqlScript = "Select ISNULL(MIN(local_net_address), '') From sys.dm_exec_connections Where session_id = @@SPID";
			return connection.ExecuteScalar(sqlScript).ToString();
		}
	}
}
