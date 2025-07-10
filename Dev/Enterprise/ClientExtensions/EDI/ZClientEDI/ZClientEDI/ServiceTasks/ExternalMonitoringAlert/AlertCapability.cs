using System;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	public class AlertCapability
	{
		public Guid PK { get; set; }
		public string Capability { get; set; }
		public string Product { get; set; }
		public string Module { get; set; }
		public Guid CapabilityId { get; set; }
		public string ProgramArea { get; set; }
		public string Priority { get; set; }
		public string ChangeType { get; set; }
		public int MaxTargets { get; set; }
	}
}
