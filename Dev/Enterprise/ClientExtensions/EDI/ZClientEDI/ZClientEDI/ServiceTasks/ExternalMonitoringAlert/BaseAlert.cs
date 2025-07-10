using System;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	public class BaseAlert
	{
		public string AlertName { get; set; }
		public Guid RuleId { get; set; }
		public string Path { get; set; }
		public Guid Id { get; set; }
		public DateTime? TimeAdded { get; set; }
		public int Priority { get; set; }
		public int Severity { get; set; }
		public long Quantity { get; set; }
		public string EntityName { get; set; }
	}
}
