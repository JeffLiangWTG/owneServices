using System.Xml.Linq;
using Enterprise.Accounting.Business.ComplianceReport;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class SourceDocuments : ComplianceReportXmlBuilder
	{
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			return new XStreamingElement("SourceDocuments",   // Hard-coded xml node name
					new SalesInvoices().BuildXml(report, null, additionalData)
				);
		}
	}
}
