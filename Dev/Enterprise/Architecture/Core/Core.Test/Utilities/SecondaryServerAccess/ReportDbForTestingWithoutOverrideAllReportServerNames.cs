using System;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ReportDbForTestingWithoutOverrideAllReportServerNames : SecondaryServerConnectionDetailsProvider
	{
		public ReportDbForTestingWithoutOverrideAllReportServerNames(string[] serverNames, string dbName, bool isPartOfAlwaysOn = true, Action<Exception, string> showException = null, Action<string> addLogs = null)
			: base(showException, addLogs)
		{
			this.serverNames = serverNames;
			this.dbName = dbName;
			this.isPartOfAlwaysOn = isPartOfAlwaysOn;
			ReportServerRespondTime = null;
			ClearCache();
		}

		protected override string DatabaseName
		{
			get { return dbName; }
		}

		readonly string dbName;

		internal protected override long SuitableDelay
		{
			get { return 1000000; }
		}

		protected override bool IsPartOfAlwaysOn
		{
			get { return isPartOfAlwaysOn; }
		}
		readonly bool isPartOfAlwaysOn;

		protected readonly string[] serverNames;

		public long? ReportServerRespondTime;

		public Exception ReportServerException { get; set; }

		public Exception ReportConnectionException { get; set; }

		public int GetDelayCallCount;

		protected override long GetDelayBetweenPrimaryAndReportDatabaseInTicks(string reportServerName)
		{
			GetDelayCallCount++;
			if (ReportServerRespondTime.HasValue)
			{
				return ReportServerRespondTime.Value;
			}
			else if (ReportServerException != null)
			{
				ShowException(ReportServerException, reportServerName);
				return long.MaxValue;
			}
			else
			{
				return base.GetDelayBetweenPrimaryAndReportDatabaseInTicks(reportServerName);
			}
		}

		protected override bool TryGetSystemIdle(string reportServerName, out int systemIdle)
		{
			systemIdle = default;
			return true;
		}

		protected override DbConnectionForReportingWrapper GetNewConnectionCore(string reportServerName, string dbUserName, string applicationNameSuffix = null)
		{
			if (ReportConnectionException != null)
			{
				throw ReportConnectionException;
			}

			DbConnectionForReportingWrapper result;
			try
			{
				result = base.GetNewConnectionCore(reportServerName, dbUserName, applicationNameSuffix);
			}
			catch (Exception ex)
			{
				ShowException(ex, reportServerName);
				throw;
			}

			CreatedConnectionSpid = result.Connection.SPID;
			return result;
		}

		protected override DbConnectionForReportingWrapper NewSecondaryConnection(string reportServerName, string dbUserName, string applicationNameSuffix = null)
		{
			if (string.IsNullOrWhiteSpace(dbUserName))
			{
				return new DbConnectionForReportingWrapper(Db.NewExtraRestrictedReaderConnection(reportServerName, DatabaseName, applicationNameSuffix));
			}
			else
			{
				return new DbConnectionForReportingWrapper(Db.NewExtraUnrestrictedWriterConnection(reportServerName, DatabaseName, applicationNameSuffix))
					.ImpersonateDbUser(dbUserName);
			}
		}

		public int? CreatedConnectionSpid { get; private set; }
	}
}
