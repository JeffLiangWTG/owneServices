using System;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	class AlertIncident
	{
		public Guid SourceRuleId { get; set; }
		public Guid SourceAlertId { get; set; }
		public Guid AlertRuleId { get; set; }
		public string TargetKey { get; set; }
	}
}
