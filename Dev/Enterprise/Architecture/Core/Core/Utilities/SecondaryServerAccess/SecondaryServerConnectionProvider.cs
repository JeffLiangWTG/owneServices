using System;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Core
{
	class SecondaryServerConnectionProvider : ISecondaryServerConnectionProvider
	{
		internal SecondaryServerConnectionProvider(Action<Exception, string> showException = null, Action<string> addLogs = null)
		{
			detailsProvider = new SecondaryServerConnectionDetailsProvider(showException, addLogs);
			this.addLogs = addLogs;
		}

		internal SecondaryServerConnectionProvider(SecondaryServerConnectionDetailsProvider detailsProvider)
		{
			this.detailsProvider = detailsProvider;
		}

		readonly SecondaryServerConnectionDetailsProvider detailsProvider;
		readonly Action<string> addLogs;

		void AddLogs(string message) => addLogs?.Invoke(message);

		public SecondaryServerConnectionDetailsProvider SecondaryServerConnectionDetails
		{
			get { return detailsProvider; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Just for DB Server Info logs")]
		public IDbConnectionForReportingWrapper GetNewConnectionWrapper(string dbUserName = null, string applicationNameSuffix = null)
		{
			IDbConnectionForReportingWrapper connection;
			var isReportingDbEnabled = SecondaryServerConnectionDetails.IsReportingDbEnabled;
			if (!isReportingDbEnabled || NeedUsePrimaryServer)
			{
				if (!isReportingDbEnabled)
				{
					AddLogs("The reporting database servers are not set up in the registry setting, report will be run on primary server.");
				}

				connection = GetNewPrimaryServerConnection(dbUserName, applicationNameSuffix);
			}
			else
			{
				connection = GetNewSecondaryServerConnection(dbUserName, applicationNameSuffix);
			}

			return connection;
		}

		public bool IsReportingDbEnabled
		{
			get { return SecondaryServerConnectionDetails.IsReportingDbEnabled; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Just for DB Server Info logs")]
		public bool NeedUsePrimaryServer
		{
			get
			{
				var serverDelay = SecondaryServerConnectionDetails.GetAnyUpToDateReportServer();
				var result = !(serverDelay.Delay <= SecondaryServerConnectionDetails.SuitableDelay);

				if (result && serverDelay.ServerName != Db.ServerName)
				{
					AddLogs("The reporting database server data has exceeded the old data threshold, report will be run on primary server.");
				}

				return result;
			}
		}

		IDbConnectionForReportingWrapper GetNewPrimaryServerConnection(string dbUserName, string applicationNameSuffix)
			=> new PrimaryServerConnectionProvider().GetNewConnectionWrapper(dbUserName, applicationNameSuffix);

		IDbConnectionForReportingWrapper GetNewSecondaryServerConnection(string dbUserName, string applicationNameSuffix)
		{
			return SecondaryServerConnectionDetails.GetNewConnectionCore(dbUserName, applicationNameSuffix);
		}
	}
}
