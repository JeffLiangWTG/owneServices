using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	public interface ISAFTXMLWriter
	{
		(IEnumerable<string> FileNames, ZStringBuilder Messages, bool HasValidationError) WriteSingleReportXmlToStream(Func<Stream> openXmlFile, string unmappedFileName);

		(IEnumerable<string> FileNames, ZStringBuilder Messages, bool HasValidationError) WriteAnnualReportXmlToStream(Func<Stream> openXmlFile, string unmappedFileName);

		XStreamingElement BuildSingleReportXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null);

		XStreamingElement BuildAnnualReportXml(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData = null);
	}

	class SAFTXMLWriterHelper
	{
		public static ISAFTXMLWriter GetSAFTXMLWriter(SAFTVersion version, AccComplianceReport report, ReportModeAndCreditorSelector reportModeAndCreditorSelector, Action<string, int?> updateProgressStatus)
		{
			switch (version)
			{
				case SAFTVersion.SAFT1_04:
					return new SAFTXMLWriter1_04(report, reportModeAndCreditorSelector, updateProgressStatus);
				case SAFTVersion.SAFT1_10:
					return new SAFTXMLWriter1_10(report, reportModeAndCreditorSelector, updateProgressStatus);
				case SAFTVersion.SAFT1_30:
					return new SAFTXMLWriter1_30(report, reportModeAndCreditorSelector, updateProgressStatus);
				default:
					throw new ArgumentException("not supported");
			}
		}

		public static ISAFTXMLWriter GetSAFTXMLWriter(SAFTVersion version, AccComplianceReport[] reports, ReportModeAndCreditorSelector reportModeAndCreditorSelector, Action<string, int?> updateProgressStatus)
		{
			switch (version)
			{
				case SAFTVersion.SAFT1_04:
					return new SAFTXMLWriter1_04(reports, reportModeAndCreditorSelector, updateProgressStatus);
				case SAFTVersion.SAFT1_10:
					return new SAFTXMLWriter1_10(reports, reportModeAndCreditorSelector, updateProgressStatus);
				case SAFTVersion.SAFT1_30:
					return new SAFTXMLWriter1_30(reports, reportModeAndCreditorSelector, updateProgressStatus);
				default:
					throw new ArgumentException("not supported");
			}
		}
	}
}
