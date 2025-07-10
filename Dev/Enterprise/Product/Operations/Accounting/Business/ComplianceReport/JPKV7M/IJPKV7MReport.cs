using Enterprise.Integration;

namespace Enterprise.Accounting.Business.ComplianceReport.JPKV7M
{
	public interface IJPKV7MReport
	{
		string ExportXmlToEDocs(AccComplianceReport complianceReport, ILogger logger);
	}
}
