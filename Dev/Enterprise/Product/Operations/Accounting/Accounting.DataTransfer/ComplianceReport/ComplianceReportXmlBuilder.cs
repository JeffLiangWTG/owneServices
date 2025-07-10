using System.Collections.Generic;
using System.Xml.Linq;
using Enterprise.Accounting.Business.ComplianceReport;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	public abstract class ComplianceReportXmlBuilder
	{
		internal abstract XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null);

		internal virtual XStreamingElement BuildAnnualXml(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			return null;
		}
	}
}
