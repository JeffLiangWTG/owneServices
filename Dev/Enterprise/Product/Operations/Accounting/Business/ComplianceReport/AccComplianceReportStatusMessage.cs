using System;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	[Flags]
	public enum AccComplianceReportCategory
	{
		Undefined = 0,
		Standard = 1,
		QueuedByServiceTask = 2,
		OutputGeneratedByServiceTask = 4
	}

	class AccComplianceReportStatusMessage
	{
		public string ReportStatus { get; }
		public string StatusMessage { get; }
		public AccComplianceReportCategory Category {  get; }

		public AccComplianceReportStatusMessage(string reportStatus, AccComplianceReportCategory category, string statusMessage)
		{
			ReportStatus = reportStatus ?? string.Empty;
			Category = category;
			StatusMessage = statusMessage ?? string.Empty;
		}
	}
}
