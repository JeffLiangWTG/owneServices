using System;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	class SqlCpuUsageAlert : BaseAlert
	{
		public long Reads { get; set; }
		public long Writes { get; set; }
		public long CpuMilliSeconds { get; set; }
		public string Owner { get; set; }
		public DateTime? ExeDate { get; set; }
		public string CurrentVersion { get; set; }
		public string AffectedClient { get; set; }
		public string ServerInstanceName { get; set; }
		public string QueryHash { get; set; }
	}
}
