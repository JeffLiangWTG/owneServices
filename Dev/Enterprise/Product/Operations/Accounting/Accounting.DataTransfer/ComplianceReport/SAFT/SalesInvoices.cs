using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class SalesInvoices : ComplianceReportXmlBuilder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var totals = ((SAFTAdditionalDataCollector)additionalData).SalesInvoicesTotals;
			var decimals = report.Company.LocalCurrency.Decimals;

			return new XStreamingElement("SalesInvoices",
					new XElement("NumberOfEntries", totals.NumberOfEntries),
					new XElement("TotalDebit", totals.TotalDebit.ToString(decimals)),
					new XElement("TotalCredit", totals.TotalCredit.ToString(decimals)),
					new XElement("Invoice")
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		internal static XStreamingElement BuildInvoiceXml(AccComplianceReport report, IEnumerable<AccComplianceReportLine> lines, SAFTAdditionalDataCollector additionalData)
		{
			var invoiceExtraData = additionalData.HeaderData[lines.First().AH_PK];
			var invoiceData = new { lineData = lines.First(x => x.ACL_ReportSequence == invoiceExtraData.HeaderSequence), extraData = invoiceExtraData };
			var invoiceType = PortugalComplianceInfo.GetInvoiceType(invoiceData.lineData.AH_ComplianceSubType);

			var transactionAuthorizationNumber = ObjectFactory.Get<ICountryComplianceFactory>()?.GetITransactionAuthorizationNumber(report.Company.GC_RN_NKCountryCode);
			var isTransactionAuthorizationNumberEnabled = transactionAuthorizationNumber?.IsTransactionAuthorizationNumberEnabled(report.ACR_GC_Company, invoiceData.lineData.InvoiceDate) ?? false;
			var transactionAuthorizationNumberLabel = transactionAuthorizationNumber?.GetTransactionAuthorizationNumberLabel() ?? "TransactionAuthorizationNumber";

			return new XStreamingElement("Invoice",
					new XStreamingElement("InvoiceNo", invoiceData.lineData.AH_TransactionReference),
					new XStreamingElement(transactionAuthorizationNumberLabel, isTransactionAuthorizationNumberEnabled && additionalData.HeaderAuthorizationNumberReferences.ContainsKey(invoiceData.lineData.AH_PK)
						? additionalData.HeaderAuthorizationNumberReferences[invoiceData.lineData.AH_PK]
						: new ZString("0")),
					BuildDocumentStatusXml(invoiceData.lineData, invoiceData.extraData),
					new XStreamingElement("Hash", invoiceData.extraData.DigitalSignature),
					new XStreamingElement("HashControl", GetHashControl(invoiceData.lineData.AH_ComplianceSubType, invoiceData.extraData.SourceReference)),
					new XStreamingElement("Period", invoiceData.lineData.InvoiceDate.Month),
					new XStreamingElement("InvoiceDate", invoiceData.lineData.InvoiceDate.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant)),
					new XStreamingElement("InvoiceType", invoiceType),
					BuildSpecialRegimesXml(report, invoiceData.lineData, invoiceData.extraData),
					new XStreamingElement("SourceID", GetSourceID(invoiceData.extraData)),
					new XStreamingElement("SystemEntryDate", GetSystemEntryDate(invoiceData.extraData)),
					new XStreamingElement("CustomerID", additionalData.PostedWithoutIVASalesInvoicePKs.Contains(invoiceData.lineData.AH_PK)
						? new ZString(PortugalComplianceInfo.PortugalAccountCodeForMissingRegistrationNumber)
						: invoiceData.lineData.OH_Code),
					BuildAllLinesXml(),
					BuildDocumentTotalsXml(report, lines, invoiceData.lineData, invoiceData.extraData)
				);

			IEnumerable<XStreamingElement> BuildAllLinesXml()
			{
				if (report.IsTransactionLinesBased)
				{
					return lines.OrderBy(x => x.ACL_ReportSequence).Select((y, index) => BuildLineXml(report, y, invoiceData.extraData, additionalData, index + 1));
				}
				else
				{
					var linesWithoutInvoiceHeader = lines.Where(x => x.ACL_ReportSequence > invoiceData.lineData.ACL_ReportSequence);
					var reportSubCode = linesWithoutInvoiceHeader.FirstOrDefault()?.ReportSubCode ?? string.Empty;
					if (!reportSubCode.IsEmpty)
					{
						return linesWithoutInvoiceHeader.Where(x => x.ReportSubCode == reportSubCode).OrderBy(x => x.ACL_ReportSequence)
							.Select((line, index) => BuildLineXml(report, line, invoiceData.extraData, additionalData, index + 1));
					}
					else
					{
						return linesWithoutInvoiceHeader.GroupBy(x => x.ACL_ReportSequence, (sequence, lineGroup) => new { ReportLine = lineGroup.First() })
							.Select((x, index) => BuildLineXml(report, x.ReportLine, invoiceData.extraData, additionalData, index + 1));
					}
				}
			}
		}

		internal static string GetHashControl(string complianceSubType, string complianceNumber) =>
			PortugalComplianceInfo.IsComplianceSubTypeCompatibleWithSourceReference(complianceSubType)
			? "1-" + complianceNumber.Trim() : "1";

		static string GetSystemEntryDate(TransactionHeaderDetails headerData)
		{
			return headerData.CreateTime.ToString(SAFTXmlBuilder.DateTimeFormat, Culture.Invariant);
		}

		static XStreamingElement BuildDocumentStatusXml(AccComplianceReportLine lineData, TransactionHeaderDetails headerDetails)
		{
			return BuildDocumentStatusXml(headerDetails, GetSourceBilling(lineData));
		}

		internal static XStreamingElement BuildDocumentStatusXml(TransactionHeaderDetails headerDetails, string sourceBilling)
		{
			return new XStreamingElement("DocumentStatus",   // Hard-coded xml node name
					new XElement("InvoiceStatus", GetInvoiceStatusCode(GetInvoiceStatus(headerDetails))),   // Hard-coded xml node name
					new XElement("InvoiceStatusDate", GetInvoiceStatusDate(headerDetails)),   // Hard-coded xml node name
					new XElement("SourceID", GetSourceID(headerDetails)),   // Hard-coded xml node name
					new XElement("SourceBilling", sourceBilling)   // Hard-coded xml node name
				);
		}

		static XStreamingElement BuildSpecialRegimesXml(AccComplianceReport report, AccComplianceReportLine lineData, TransactionHeaderDetails additionalHeaderData)
		{
			var thirdPartiesBillingIndicator = lineData.AH_ComplianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.XCL
				|| lineData.AH_ComplianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.XCR
				? 1 : 0;

			return new XStreamingElement("SpecialRegimes",   // Hard-coded xml node name
					!additionalHeaderData.IsSelfBilling ? new XElement("SelfBillingIndicator", 0) : null,   // Hard-coded xml node name
					new XElement("CashVATSchemeIndicator", report.Company.GC_IsGSTCashBasis ? 1 : 0),   // Hard-coded xml node name
					new XElement("ThirdPartiesBillingIndicator", thirdPartiesBillingIndicator)   // Hard-coded xml node name
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name, Hard-coded xml node name and node value, Dev Error")]
		static XStreamingElement BuildLineXml(AccComplianceReport report, AccComplianceReportLine lineData, TransactionHeaderDetails additionalHeaderData, SAFTAdditionalDataCollector additionalData, int lineNumber)
		{
			XStreamingElement result = null;
			var additionalLineData = additionalData.GetTransactionLine(lineData.ACL_ReportSequence);

			if (additionalLineData != null)
			{
				var decimals = report.Company.LocalCurrency.Decimals;
				var exTaxAmountFormatted = new ZDecimal(Math.Abs(lineData.TotalExTaxAmount)).ToString(decimals);
				var productCode = !additionalLineData.ChargeCode.IsEmpty ? additionalLineData.ChargeCode : additionalLineData.GLAccountNum;
				var productDecription = additionalData.ChargeCodes.TryGetValue(productCode, out var chargeCodeDetails) ? chargeCodeDetails.ChargeCode.Description
					: additionalData.GLAccounts.TryGetValue(productCode, out var glAccountsDetails) ? glAccountsDetails.Description
					: report.GLMovementDetails.Cast<GeneralLedgerBalanceLine>().FirstOrDefault(x => x.AG_AccountNum == productCode)?.AG_Description;

				result = new XStreamingElement("Line",
						new XStreamingElement("LineNumber", lineNumber),
						new XStreamingElement("ProductCode", productCode),
						new XStreamingElement("ProductDescription", productDecription),
						new XStreamingElement("Quantity", 1),
						new XStreamingElement("UnitOfMeasure", "Unidade"),
						new XStreamingElement("UnitPrice", exTaxAmountFormatted),
						new XStreamingElement("TaxPointDate", additionalLineData.TaxDate.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant)),
						BuildReferencesXml(additionalHeaderData),
						new XStreamingElement("Description", additionalLineData.Description.SubstringSafe(0, 200)),
						BuildDebitCreditAmountsXml(lineData, exTaxAmountFormatted),
						additionalData.TaxIDs.TryGetValue(lineData.AT_Code, out var tax) ? TaxTable.BuildTaxXml(tax) : null,   // Tax
						lineData.AT_Code.IsEmpty
							? new XStreamingElement("TaxExemptionReason", "Não Sujeito; não tributado / Not Subject to VAT")
							: lineData.TotalTaxAmount.IsEmpty && !lineData.TaxMessage.IsEmpty ? new XStreamingElement("TaxExemptionReason", lineData.TaxGroupDescription) : null,
						lineData.AT_Code.IsEmpty
							? new XStreamingElement("TaxExemptionCode", "M99")
							: lineData.TotalTaxAmount.IsEmpty && !additionalLineData.TaxMsgTaxGroupCode.IsEmpty ? new XStreamingElement("TaxExemptionCode", additionalLineData.TaxMsgTaxGroupCode) : null
					);
			}
			else
			{
				var errorMessage = string.Format(CultureInfo.InvariantCulture, @"Missing invoice line in the report:
Report Details: Company Code = {0}, Type = {1}, Period = {2}, Date From = {3}, Date To = {4}, Status = {5}, Last Edit User = {6}, Last Edit Time UTC = {7}
Missing Line Details: Line # = {8}, Ledger = {9}, Type = {10}, Invoice # = {11}, Transaction Date = {12}, Post Date = {13}.",
					report.Company.GC_Code, report.ACR_ReportType, report.ACR_Periodicity, report.ACR_DateFrom, report.ACR_DateTo, report.ACR_Status, report.ACR_SystemLastEditUser, report.ACR_SystemLastEditTimeUtc,
					lineData.ACL_ReportSequence, lineData.AH_Ledger, lineData.AH_TransactionType, lineData.AH_TransactionNum, lineData.InvoiceDate, lineData.PostDate);

				ErrorReporter.ReportOnce("Missing invoice line in the report", errorMessage);

				var userMessage = Res.GetString("8f036cf5-e37c-4d3f-9111-2cd017a7619d", "There are critical problems with this report. These problems could be fixed by re-queuing/generating this report.");
				throw new NotSupportedException(userMessage);
			}

			return result;
		}

		static XStreamingElement BuildDebitCreditAmountsXml(AccComplianceReportLine lineData, string exTaxAmountFormatted)
		{
			XStreamingElement result = null;

			if (lineData.AH_TransactionType == TransactionTypes.CreditNote)
			{
				result = new XStreamingElement("DebitAmount", exTaxAmountFormatted);   // Hard-coded xml node name
			}
			else if (lineData.AH_TransactionType == TransactionTypes.Invoice)
			{
				result = new XStreamingElement("CreditAmount", exTaxAmountFormatted);   // Hard-coded xml node name
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		internal static XStreamingElement BuildReferencesXml(TransactionHeaderDetails additionalHeaderData)
		{
			var references = new List<XElement>();
			var reasonText = string.Empty;
			var reference = additionalHeaderData.OriginalTransactionReference;

			if (!reference.IsEmpty)
			{
				if (!additionalHeaderData.OriginalReferenceSourceReference.IsEmpty)
				{
					var sourceReferenceOptionalPrefix = PortugalComplianceInfo.GetSourceReferencePrefix(additionalHeaderData.OriginalReferenceComplianceSubType) + " ";

					//If the Source Reference is compatible, we want to remove the prefix and whitespace from it
					reference = PortugalComplianceInfo.IsComplianceSubTypeCompatibleWithSourceReference(additionalHeaderData.OriginalReferenceComplianceSubType)
						&& additionalHeaderData.OriginalReferenceSourceReference.StartsWith(sourceReferenceOptionalPrefix)
						? additionalHeaderData.OriginalReferenceSourceReference.SubstringSafe(sourceReferenceOptionalPrefix.Length)
						: additionalHeaderData.OriginalReferenceSourceReference;
				}

				references.Add(new XElement("Reference", reference));

				if (!additionalHeaderData.OriginalReferenceReversalReason.IsEmpty)
				{
					reasonText = additionalHeaderData.OriginalReferenceReversalReason.Split("|").Last();
				}
				else
				{
					var isCancelled = additionalHeaderData.IsCancelled;
					var isAmendment = additionalHeaderData.IsReversedAmendment || !isCancelled;
					var isReversal = !isAmendment && isCancelled;

					reasonText = additionalHeaderData.ReceiptType.IsEmpty
						? null
						: isAmendment
						? AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value.GetDescriptionFromCode(additionalHeaderData.ReceiptType)
						: isReversal
						? AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.Value.GetDescriptionFromCode(additionalHeaderData.ReceiptType)
						: null;
				}
			}
			else if (additionalHeaderData.OriginalReferenceStartDate.IsValid
				&& additionalHeaderData.OriginalReferenceEndDate.IsValid)
			{
				reasonText = string.Format(CultureInfo.InvariantCulture, "{0} - {1}",
					additionalHeaderData.OriginalReferenceStartDate.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant),
					additionalHeaderData.OriginalReferenceEndDate.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant));
			}

			if (!string.IsNullOrEmpty(reasonText))
			{
				references.Add(new XElement("Reason", reasonText.Substring(0, Math.Min(reasonText.Length, 50))));
			}

			return references.Count > 0 ? new XStreamingElement("References", references) : null;
		}

		static XStreamingElement BuildDocumentTotalsXml(AccComplianceReport report, IEnumerable<AccComplianceReportLine> lines, AccComplianceReportLine lineData, TransactionHeaderDetails additionalHeaderData)
		{
			var localCurrency = report.Company.GC_RX_NKLocalCurrency;
			var decimals = report.Company.LocalCurrency.Decimals;
			var isReportWithTransactionLineOnly = report.IsTransactionLinesBased;
			var taxAmount = new ZDecimal(Math.Abs(isReportWithTransactionLineOnly ? (ZDecimal)lines.Sum(x => x.TotalTaxAmount) : lineData.TotalTaxAmount));
			var exTaxAmount = new ZDecimal(Math.Abs(isReportWithTransactionLineOnly ? (ZDecimal)lines.Sum(x => x.TotalExTaxAmount) : lineData.TotalExTaxAmount));

			return new XStreamingElement("DocumentTotals",   // Hard-coded xml node name
					new XElement("TaxPayable", taxAmount.ToString(decimals)),   // Hard-coded xml node name
					new XElement("NetTotal", exTaxAmount.ToString(decimals)),   // Hard-coded xml node name
					new XElement("GrossTotal", new ZDecimal(taxAmount + exTaxAmount).ToString(decimals)),   // Hard-coded xml node name
					additionalHeaderData.TransactionCurrency != localCurrency ? BuildCurrencyXml(report, additionalHeaderData) : null,   // Hard-coded xml node name
					BuildSettlementXml(lineData, additionalHeaderData)   // Hard-coded xml node name
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		static XStreamingElement BuildSettlementXml(AccComplianceReportLine lineData, TransactionHeaderDetails additionalHeaderData)
		{
			return lineData.AH_TransactionType == TransactionTypes.Invoice
				? new XStreamingElement("Settlement",
					new XElement("PaymentTerms", GetPaymentTerms(lineData, additionalHeaderData)))
				: null;
		}

		static string GetPaymentTerms(AccComplianceReportLine lineData, TransactionHeaderDetails additionalHeaderData)
		{
			var creditTerms = TransactionHeaderHelper.GetCreditTerms(lineData.AH_TransactionType, Core.Constants.CountryCodes.Portugal,
				additionalHeaderData.InvoiceTerm, additionalHeaderData.InvoiceTermDays.ToString(), additionalHeaderData.InvoiceDate, additionalHeaderData.DueDate);

			var agreedPaymentMethodOverride = additionalHeaderData.AgreedPaymentMethodOverride.IsEmpty
				? string.Empty
				: OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList().GetDescriptionFromCode(additionalHeaderData.AgreedPaymentMethodOverride);

			var paymentTermsValues = new string[] { creditTerms, additionalHeaderData.DueDate.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant), agreedPaymentMethodOverride };
			var result = string.Join(" - ", paymentTermsValues.Where(x => !string.IsNullOrEmpty(x)));

			if (result.Length > 100)
			{
				paymentTermsValues = new string[] { Truncate(creditTerms.ToString(), 42), additionalHeaderData.DueDate.ToString(SAFTXmlBuilder.DateFormat, Culture.Invariant), Truncate(agreedPaymentMethodOverride, 42) };
				result = string.Join(" - ", paymentTermsValues.Where(x => !string.IsNullOrEmpty(x)));
			}

			return result;

			string Truncate(string value, int maxLength) => (value.Length > maxLength) ? value.Substring(0, maxLength - 2) + ".." : value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		internal static XStreamingElement BuildCurrencyXml(AccComplianceReport report, TransactionHeaderDetails additionalHeaderData)
		{
			var exRateDecimals = report.Company.ExchangeRateDecimalPlaces;

			return new XStreamingElement("Currency",
					new XElement("CurrencyCode", additionalHeaderData.TransactionCurrency),
					new XElement("CurrencyAmount", new ZDecimal(Math.Abs(additionalHeaderData.OSTotal)).ToString(additionalHeaderData.TransactionCurrencyDecimals)),
					new XElement("ExchangeRate", additionalHeaderData.ExchangeRate.ToString(exRateDecimals))
				);
		}

		enum InvoiceStatus
		{
			Normal,
			SelfBilling,
		}

		static string GetSourceBilling(AccComplianceReportLine lineData) =>
			PortugalComplianceInfo.IsComplianceSubTypeCompatibleWithSourceReference(lineData.AH_ComplianceSubType)
			? "M" : "P";

		static InvoiceStatus GetInvoiceStatus(TransactionHeaderDetails headerDetails) => headerDetails.IsSelfBilling ? InvoiceStatus.SelfBilling : InvoiceStatus.Normal;

		static string GetInvoiceStatusCode(InvoiceStatus invoiceStatus)
		{
			switch (invoiceStatus)
			{
				case InvoiceStatus.Normal:
					return "N";
				case InvoiceStatus.SelfBilling:
					return "S";
				default:
					throw new ArgumentOutOfRangeException(nameof(invoiceStatus));
			}
		}

		static string GetInvoiceStatusDate(TransactionHeaderDetails headerDetails) => GetSystemEntryDate(headerDetails);

		static ZString GetSourceID(TransactionHeaderDetails headerDetails) => headerDetails.CreateUserCode;
	}
}
