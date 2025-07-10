using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	class SAFTXMLWriter1_04 : SAFTXMLWriterBase
	{
		public SAFTXMLWriter1_04(AccComplianceReport report, ReportModeAndCreditorSelector reportModeAndCreditorSelector, Action<string, int?> updateProgressStatus) : base(report, reportModeAndCreditorSelector, updateProgressStatus) { }

		public SAFTXMLWriter1_04(IEnumerable<AccComplianceReport> reports, ReportModeAndCreditorSelector reportModeAndCreditorSelector, Action<string, int?> updateProgressStatus) : base(reports, reportModeAndCreditorSelector, updateProgressStatus) { }

		protected override IEnumerable<ComplianceReportXmlBuilder> MainBodyElements => elements ?? (elements = new ComplianceReportXmlBuilder[] { new Header(), new MasterFiles(), new SourceDocuments() });
		ComplianceReportXmlBuilder[] elements;

		protected override string XSDName => "Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT.SAFTPT1.04_01.xsd";

		protected override string NameSpace => "urn:OECD:StandardAuditFile-Tax:PT_1.04_01";

		protected override Encoding FileEncoding => Encoding.GetEncoding("Windows-1252");

		protected override bool IsSupportSalesInvoice => true;

		protected override IEnumerable<XStreamingElement> BuildMultiReportsXmlCore(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData)
		{
			return MainBodyElements.Select(x => x.BuildXml(reports.Last(), null, additionalData));
		}
	}
}
