namespace Enterprise.Accounting.Business.ComplianceReport
{
	public interface IAccComplianceReportUsageCollectorFactory
	{
		IAccComplianceReportUsageCollector GetAccComplianceReportUsageCollector(AccComplianceReport accComplianceReport);
}
}
