using System;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	[CodeAlive("Could be used later when certification becomes Accounting+Billing")]
	internal class Payments : ComplianceReportXmlBuilder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var totals = ((SAFTAdditionalDataCollector)additionalData).PaymentsTotals;
			var decimals = report.Company.LocalCurrency.Decimals;

			return new XStreamingElement("Payments",
					new XElement("NumberOfEntries", totals.NumberOfEntries),
					new XElement("TotalDebit", totals.TotalDebit.ToString(decimals)),
					new XElement("TotalCredit", totals.TotalCredit.ToString(decimals)),
					new XElement("Payment")
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		internal static XStreamingElement BuildPaymentXml(AccComplianceReport report, AccComplianceReportLine line, SAFTAdditionalDataCollector additionalData)
		{
			var paymentData = new { lineData = line, extraData = additionalData.HeaderData[line.AH_PK] };

			var config = AccountingConfigurationRegistry.Instance.TaxRecognitionDefaultingRules.GetValueWithoutFallback(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty);

			return new XStreamingElement("Payment",
					new XStreamingElement("PaymentRefNo", "PAY 001/" + paymentData.lineData.AH_TransactionNum),
					new XStreamingElement("ATCUD", 0),
					new XStreamingElement("TransactionID", paymentData.lineData.AH_TransactionNum),
					new XStreamingElement("TransactionDate", paymentData.lineData.PostDate.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant)),
					new XStreamingElement("PaymentType", config.AROutputServices == "CHS" ? "RC" : "RG"),
					BuildDocumentStatusXml(paymentData.extraData),
					new XStreamingElement("SourceID", paymentData.extraData.CreateUserName),
					new XStreamingElement("SystemEntryDate", paymentData.extraData.CreateTime.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant)),
					new XStreamingElement("CustomerID", paymentData.lineData.OH_Code),
					BuildLineXml(report, paymentData.lineData, additionalData),
					BuildDocumentTotalsXml(report, paymentData.lineData, paymentData.extraData)
				);
		}

		static XStreamingElement BuildDocumentStatusXml(TransactionHeaderDetails headerDetails)
		{
			return new XStreamingElement("DocumentStatus",   // Hard-coded xml node name
					new XElement("PaymentStatus", headerDetails.IsCancelled ? "A" : "N"),   // Hard-coded xml node name
					new XElement("PaymentStatusDate", headerDetails.LastEditTime.ToString(SAFTXmlBuilder.DateTimeFormat, Culture.Invariant)),   // Hard-coded xml node name
					new XElement("SourceID", headerDetails.LastEditUserName),   // Hard-coded xml node name
					new XElement("SourcePayment", "P")   // Hard-coded xml node name
				);
		}

		static XStreamingElement BuildDocumentTotalsXml(AccComplianceReport report, AccComplianceReportLine lineData, TransactionHeaderDetails additionalHeaderData)
		{
			var localCurrency = report.Company.GC_RX_NKLocalCurrency;
			var decimals = report.Company.LocalCurrency.Decimals;
			var taxAmount = new ZDecimal(Math.Abs(lineData.TotalTaxAmount));
			var exTaxAmount = new ZDecimal(Math.Abs(lineData.TotalExTaxAmount));

			return new XStreamingElement("DocumentTotals",   // Hard-coded xml node name
					new XElement("TaxPayable", taxAmount.ToString(decimals)),   // Hard-coded xml node name
					new XElement("NetTotal", exTaxAmount.ToString(decimals)),   // Hard-coded xml node name
					new XElement("GrossTotal", new ZDecimal(taxAmount + exTaxAmount).ToString(decimals)),   // Hard-coded xml node name
					additionalHeaderData.TransactionCurrency != localCurrency ? SalesInvoices.BuildCurrencyXml(report, additionalHeaderData) : null,   // Hard-coded xml node name
					BuildPaymentXml(report, lineData)
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		static XStreamingElement BuildPaymentXml(AccComplianceReport report, AccComplianceReportLine lineData)
		{
			var decimals = report.Company.LocalCurrency.Decimals;

			return new XStreamingElement("Payment",
					new XElement("PaymentAmount", lineData.TotalExTaxAmount.ToString(decimals)),
					new XElement("PaymentDate", lineData.PostDate.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant))
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		static XStreamingElement BuildLineXml(AccComplianceReport report, AccComplianceReportLine lineData, ComplianceReportAdditionalDataCollector additionalData)
		{
			var decimals = report.Company.LocalCurrency.Decimals;
			var exTaxAmountFormatted = new ZDecimal(Math.Abs(lineData.TotalExTaxAmount)).ToString(decimals);
			var additionalLineData = additionalData.GetTransactionLine(lineData.ACL_ReportSequence);

			return new XStreamingElement("Line",
					new XStreamingElement("LineNumber", 1),
					new XStreamingElement("SourceDocumentID",
						new XStreamingElement("OriginatingON", "Omisso"),
						new XStreamingElement("InvoiceDate", lineData.PostDate.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant))),
					Math.Sign(lineData.TotalExTaxAmount) == -1 ? new XStreamingElement("DebitAmount", exTaxAmountFormatted) : new XStreamingElement("CreditAmount", exTaxAmountFormatted),
					additionalData.TaxIDs.TryGetValue(lineData.AT_Code, out var tax) ? TaxTable.BuildTaxXml(tax) : null,   // Tax
					!lineData.TaxMessage.IsEmpty ? new XStreamingElement("TaxExemptionReason", lineData.TaxMessage) : null,
					additionalLineData?.TaxMsgTaxGroupCode.IsEmpty ?? false ? new XStreamingElement("TaxExemptionCode", additionalLineData.TaxMsgTaxGroupCode) : null
				);
		}
	}
}
