using System;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	public class AlertRule
	{
		public Guid PK { get; set; }
		public string AlertName { get; set; }
		public string Product { get; set; }
		public string Module { get; set; }
		public string ProgramArea { get; set; }
		public int Type { get; set; }
		public string Path { get; set; }
		public string Owner { get; set; }
		public string Capability { get; set; }
		public bool IsDefault { get; set; }
		public string Criticality { get; set; }
		public string Priority { get; set; }
		public string ChangeType { get; set; }
		public int MaxTargets { get; set; }
	}
}
