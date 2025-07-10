namespace Enterprise.Accounting.Business.ComplianceReport
{
	internal class AccComplianceReportUsageCollectorFactory : IAccComplianceReportUsageCollectorFactory
	{
		IAccComplianceReportUsageCollector IAccComplianceReportUsageCollectorFactory.GetAccComplianceReportUsageCollector(AccComplianceReport accComplianceReport)
		{
			return new AccComplianceReportUsageCollector(accComplianceReport);
		}
	}
}
