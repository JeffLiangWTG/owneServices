using System;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ReportDbForTesting : ReportDbForTestingWithoutOverrideAllReportServerNames
	{
		public ReportDbForTesting(string serverName, string dbName, bool isPartOfAlwaysOn = true, Action<Exception, string> showException = null, Action<string> addLogs = null)
			: this(string.IsNullOrEmpty(serverName) ? Array.Empty<string>() : new[] { serverName }, dbName, isPartOfAlwaysOn, showException, addLogs)
		{
		}

		public ReportDbForTesting(string[] serverNames, string dbName, bool isPartOfAlwaysOn = true, Action<Exception, string> showException = null, Action<string> addLogs = null)
			: base(serverNames, dbName, isPartOfAlwaysOn, showException, addLogs)
		{
		}

		protected override string[] GetAllReportServerNames()
		{
			return serverNames;
		}
	}
}
