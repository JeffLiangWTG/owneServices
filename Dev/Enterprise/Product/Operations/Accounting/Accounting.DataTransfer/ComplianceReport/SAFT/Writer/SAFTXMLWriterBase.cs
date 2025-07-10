using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	abstract class SAFTXMLWriterBase : ComplianceReportXmlWriter, ISAFTXMLWriter
	{
		public SAFTXMLWriterBase(AccComplianceReport report, ReportModeAndCreditorSelector reportModeAndCreditorSelector, Action<string, int?> updateProgressStatus)
			: base(report, ComplianceReportXmlWriter.GetReportMode(reportModeAndCreditorSelector), updateProgressStatus, reportModeAndCreditorSelector.Creditor)
		{
		}

		public SAFTXMLWriterBase(IEnumerable<AccComplianceReport> reports, ReportModeAndCreditorSelector reportModeAndCreditorSelector, Action<string, int?> updateProgressStatus)
			: base(reports, ComplianceReportXmlWriter.GetReportMode(reportModeAndCreditorSelector), updateProgressStatus, reportModeAndCreditorSelector.Creditor)
		{
		}

		public SAFTXMLWriterBase(ReportModeAndCreditorSelector reportModeAndCreditorSelector, Action<string, int?> updateProgressStatus)
			: base(ComplianceReportXmlWriter.GetReportMode(reportModeAndCreditorSelector), updateProgressStatus, reportModeAndCreditorSelector.Creditor)
		{
		}

		protected override string ReportName => Res.GetString("b36d538a-85d7-45b5-abfa-768d4198359e", "SAFT");

		protected override string MultipleReportsName => Res.GetString("6dd3a833-17b7-47f0-8597-4e650174f60b", "Annual SAFT");

		protected override string NameSpaceAttributeXsi => "http://www.w3.org/2001/XMLSchema-instance";

		protected override string TopLevelElementName => "AuditFile";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Element name")]
		protected override string TransactionElementName => "Invoice";

		protected abstract bool IsSupportSalesInvoice { get; }

		protected override IEnumerable<XStreamingElement> BuildTransactionsXml(AccComplianceReport report, ComplianceReportAdditionalDataCollector additionalData)
		{
			var saftAdditionalData = (SAFTAdditionalDataCollector)additionalData;
			return saftAdditionalData.SalesInvoices.OrderBy(x => x.Value.First().ACL_ReportSequence).Select(x => SalesInvoices.BuildInvoiceXml(report, x.Value, saftAdditionalData));
		}

		protected override (bool IsNeedCompression, int AllowedMaxSize) GetCompressionInfo()
		{
			var countryCode = Report?.Company.GC_RN_NKCountryCode ?? Reports.FirstOrDefault().Company.GC_RN_NKCountryCode;
			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode) as IInstanceProvider<IComplianceReportGUIActionProvider>)?.Get();
			return provider?.GetSAFTFileCompressionInfo() ?? base.GetCompressionInfo();
		}

		XStreamingElement ISAFTXMLWriter.BuildSingleReportXml(AccComplianceReport report, AccComplianceReportLineBase line, ComplianceReportAdditionalDataCollector additionalData)
			=> BuildXml(report, line, additionalData);

		XStreamingElement ISAFTXMLWriter.BuildAnnualReportXml(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData)
			=> BuildMultiReportsXml(reports, additionalData);

		(IEnumerable<string> FileNames, ZStringBuilder Messages, bool HasValidationError) ISAFTXMLWriter.WriteSingleReportXmlToStream(Func<Stream> openXmlFile, string unmappedFileName)
		{
			using (var tempStream = new VirtualMemoryStream())
			{
				var result = WriteSingleReportXmlToStreamCore(tempStream, openXmlFile, unmappedFileName);
				var notifications = new NotificationBuffer();
				ValidateXml(tempStream, notifications);

				return (result.FileNames, result.Messages, notifications.ContainsNotificationType(ErrorType.Error));
			}
		}

		(IEnumerable<string> FileNames, ZStringBuilder Messages, bool HasValidationError) ISAFTXMLWriter.WriteAnnualReportXmlToStream(Func<Stream> openXmlFile, string unmappedFileName)
		{
			using (var tempStream = new VirtualMemoryStream())
			{
				var result = WriteMultipleReportsXmlToStreamCore(tempStream, openXmlFile, unmappedFileName);
				var notifications = new NotificationBuffer();
				ValidateXml(tempStream, notifications);

				return (result.FileNames, result.Messages, notifications.ContainsNotificationType(ErrorType.Error));
			}
		}
	}
}
