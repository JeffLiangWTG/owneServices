using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	[CodeAlive("Could be used later when certification becomes Accounting+Billing")]
	internal class GeneralLedgerEntries : ComplianceReportXmlBuilder
	{
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var journals = GetJournals(report).ToArray();
			var decimals = report.Company.LocalCurrency.Decimals;

			return new XStreamingElement("GeneralLedgerEntries",   // Hard-coded xml node name
				new XElement("NumberOfEntries", journals.Length),   // Hard-coded xml node name
					new XElement("TotalDebit", new ZDecimal(journals.Select(x => x.Sum(y => y.GeneralLedgerAmountDR)).Sum(x => x)).ToString(decimals)),   // Hard-coded xml node name
					new XElement("TotalCredit", new ZDecimal(journals.Select(x => x.Sum(y => y.GeneralLedgerAmountCR)).Sum(x => x)).ToString(decimals)),   // Hard-coded xml node name
					journals.Select(x => BuildJournalXml(report, x, (SAFTAdditionalDataCollector)additionalData))
				);
		}

		IEnumerable<IEnumerable<AccComplianceReportLine>> GetJournals(AccComplianceReport report)
		{
			return report.ReportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_Ledger == LedgerTypes.JobCosting || x.AH_Ledger == LedgerTypes.AccountsReceivable || x.AH_Ledger == LedgerTypes.AccountsPayable)
				.GroupBy(x => new { x.PostDate, x.AH_PK, x.AH_TransactionType, x.OH_Code })
				.OrderBy(x => x.First().ACL_ReportSequence);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildJournalXml(AccComplianceReport report, IEnumerable<AccComplianceReportLine> lines, SAFTAdditionalDataCollector additionalData)
		{
			var firstLine = lines.First();
			return new XStreamingElement("Journal",
					new XElement("JournalID", GetJournalID(firstLine)),
					new XElement("Description", GetDescription(firstLine, additionalData)),
					BuildTransactionXml(report, lines, additionalData)
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded representation of date in JournalID")]
		const string JournalIDDateFormat = "ddMMyyyy";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded space in the JournalID ")]
		ZString GetJournalID(AccComplianceReportLine reportLine)
		{
			ZString result = ZString.Empty;
			if (isPseudoTransaction(reportLine))
			{
				var numberFromDate = reportLine.PostDate.ToString(JournalIDDateFormat, CultureInfo.InvariantCulture);
				result = (reportLine.AH_TransactionType + " " + numberFromDate + " " + reportLine.OH_Code).TrimEnd(' ');
			}
			else if (isRealTransaction(reportLine))
			{
				result = reportLine.AH_Ledger + " " + reportLine.AH_TransactionType + " " + reportLine.AH_TransactionNum;
			}
			return result;
		}

		bool isPseudoTransaction(AccComplianceReportLine reportLine) => reportLine.AH_Ledger == LedgerTypes.JobCosting
				&& (reportLine.AH_TransactionType == TransactionLineTypes.WIP || reportLine.AH_TransactionType == TransactionLineTypes.Accrual);

		bool isRealTransaction(AccComplianceReportLine reportLine) => reportLine.AH_Ledger == LedgerTypes.AccountsReceivable || reportLine.AH_Ledger == LedgerTypes.AccountsPayable;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node value")]
		ZString GetDescription(AccComplianceReportLine reportLine, SAFTAdditionalDataCollector additionalData)
		{
			ZString result = ZString.Empty;
			if (reportLine.AH_Ledger == LedgerTypes.JobCosting)
			{
				if (reportLine.AH_TransactionType == TransactionLineTypes.WIP)
				{
					result = "WIPS JOURNAL";
				}
				if (reportLine.AH_TransactionType == TransactionLineTypes.Accrual)
				{
					result = "ACCRUALS JOURNAL";
				}
			}
			else if (reportLine.AH_Ledger == LedgerTypes.AccountsReceivable || reportLine.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				additionalData.HeaderData.TryGetValue(reportLine.AH_PK, out var header);
				result = header?.Description ?? ZString.Empty;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildTransactionXml(AccComplianceReport report, IEnumerable<AccComplianceReportLine> reportLines, SAFTAdditionalDataCollector additionalData)
		{
			var lineSequence = 1;
			var firstLine = reportLines.First();
			var orderedReportLines = reportLines.OrderByDescending(x => x.IsDebitLine).ThenBy(y => y.AG_AccountNum);
			var lines = orderedReportLines.GroupBy(x => new { IsDebit = x.IsDebitLine, AccountID = x.AG_AccountNum });
			var transactionDate = firstLine.PostDate.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture);

			var transactionID = ZString.Empty;
			var docArchivalNumber = ZString.Empty;
			if (isPseudoTransaction(firstLine))
			{
				var numberFromDate = firstLine.PostDate.ToString(JournalIDDateFormat, CultureInfo.InvariantCulture);
				transactionID = transactionDate + " " + GetJournalID(firstLine) + " " + numberFromDate;
				docArchivalNumber = numberFromDate;
			}
			if (isRealTransaction(firstLine))
			{
				transactionID = transactionDate + " " + GetJournalID(firstLine) + " " + firstLine.AH_TransactionNum;
				docArchivalNumber = firstLine.AH_TransactionNum;
			}

			return new XStreamingElement("Transaction",
					new XElement("TransactionID", transactionID),
					new XElement("Period", firstLine.PostDate.Month),
					new XElement("TransactionDate", transactionDate),
					new XElement("SourceID", GetSourceID(firstLine, additionalData)),
					new XElement("Description", GetTransactionDescription(firstLine, additionalData)),
					new XElement("DocArchivalNumber", docArchivalNumber),
					new XElement("TransactionType", "N"),
					new XElement("GLPostingDate", firstLine.PostDate.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture)),
					GetOrgCode(firstLine),
					new XStreamingElement("Lines", lines.Select(x => BuildTransactionLineXml(report, x, ref lineSequence)))
				);
		}

		XElement GetOrgCode(AccComplianceReportLine reportLine)
		{
			XElement result = null;
			if (reportLine.AH_Ledger == LedgerTypes.AccountsReceivable || (reportLine.AH_Ledger == LedgerTypes.JobCosting && reportLine.AH_TransactionType == TransactionLineTypes.WIP))
			{
				result = new XElement("CustomerID", reportLine.OH_Code);
			}
			else if (reportLine.AH_Ledger == LedgerTypes.AccountsPayable || (reportLine.AH_Ledger == LedgerTypes.JobCosting && reportLine.AH_TransactionType == TransactionLineTypes.Accrual))
			{
				result = new XElement("SupplierID", reportLine.OH_Code);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node value")]
		ZString GetSourceID(AccComplianceReportLine reportLine, SAFTAdditionalDataCollector additionalData)
		{
			ZString result = "System";
			if (reportLine.AH_Ledger != LedgerTypes.JobCosting) // we use "System" as WIP/ACR's created by user
			{
				var header = additionalData.GetTransactionHeader(reportLine.AH_PK) as TransactionHeaderDetails;
				if (header != null)
				{
					result = header.CreateUserCode;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node value")]
		ZString GetTransactionDescription(AccComplianceReportLine reportLine, SAFTAdditionalDataCollector additionalData)
		{
			ZString result = "";
			if (reportLine.AH_Ledger == LedgerTypes.JobCosting
				&& (reportLine.AH_TransactionType == TransactionLineTypes.WIP || reportLine.AH_TransactionType == TransactionLineTypes.Accrual))
			{
				result = reportLine.AH_TransactionType + " Transaction";
			}
			else
			{
				var header = additionalData.GetTransactionHeader(reportLine.AH_PK);
				if (header != null)
				{
					result = header.Description;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildTransactionLineXml(AccComplianceReport report, IEnumerable<AccComplianceReportLine> reportLines, ref int lineSequence)
		{
			var firstLine = reportLines.First();
			var decimals = report.Company.LocalCurrency.Decimals;

			return new XStreamingElement(firstLine.IsDebitLine ? "DebitLine" : "CreditLine",
					new XElement("RecordID", lineSequence++.ToString(CultureInfo.InvariantCulture).PadLeft(10, '0')),
					new XElement("AccountID", firstLine.AG_AccountNum),
					new XElement("SystemEntryDate", firstLine.PostDate.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture)),
					new XElement("Description", firstLine.AG_Description),
					firstLine.IsDebitLine ? new XElement("DebitAmount", ((ZDecimal)reportLines.Sum(x => x.GeneralLedgerAmountDR)).ToString(decimals)) :
											new XElement("CreditAmount", ((ZDecimal)reportLines.Sum(x => x.GeneralLedgerAmountCR)).ToString(decimals))
				);
		}
	}
}
