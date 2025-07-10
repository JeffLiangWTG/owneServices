using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class GeneralLedgerEntries1_30 : ComplianceReportXmlBuilder
	{
		internal override XStreamingElement BuildAnnualXml(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData)
		{
			Argument.NotNull(additionalData, nameof(additionalData));

			var lines = new List<AccComplianceReportLine>();
			reports.ForEach(x => lines.AddRange(x.ReportLinesExport.Cast<AccComplianceReportLine>()));

			return BuildAnnualXmlCore(lines, (SAFTAdditionalDataCollector)additionalData, reports.Last());
		}

		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			Argument.NotNull(additionalData, nameof(additionalData));

			var lines = report.ReportLinesExport.Cast<AccComplianceReportLine>();
			return BuildXmlCore(lines, (SAFTAdditionalDataCollector)additionalData, report);
		}

		XStreamingElement BuildXmlCore(IEnumerable<AccComplianceReportLine> lines, SAFTAdditionalDataCollector additionalData, AccComplianceReport report)
		{
			var validLines = lines.Where(x => x.AH_TransactionType != TransactionTypes.GLNoteJournal);
			var transactionGroups = GetTransactionGroups(validLines).ToArray();
			var decimals = report.Company.LocalCurrency.Decimals;

			return new XStreamingElement("GeneralLedgerEntries",
					new XElement("NumberOfEntries", transactionGroups.Length),
					new XElement("TotalDebit", new ZDecimal(validLines.Sum(x => x.GeneralLedgerAmountDR)).ToString(decimals)),
					new XElement("TotalCredit", new ZDecimal(validLines.Sum(x => x.GeneralLedgerAmountCR)).ToString(decimals)),
					BuildJournalXml(transactionGroups, additionalData, report)
					);
		}

		XStreamingElement BuildAnnualXmlCore(IEnumerable<AccComplianceReportLine> lines, SAFTAdditionalDataCollector additionalData, AccComplianceReport report)
		{
			var validAnnualLines = GetValidAnnualLine(report);
			var annualTransactionGroups = GetTransactionGroups(validAnnualLines).ToArray();

			var validLines = lines.Where(x => x.AH_TransactionType != TransactionTypes.GLNoteJournal);
			var transactionGroups = GetTransactionGroups(validLines).ToArray();
			var decimals = report.Company.LocalCurrency.Decimals;

			return new XStreamingElement("GeneralLedgerEntries",
					new XElement("NumberOfEntries", annualTransactionGroups.Length),
					new XElement("TotalDebit", new ZDecimal(validAnnualLines.Sum(x => x.GeneralLedgerAmountDR)).ToString(decimals)),
					new XElement("TotalCredit", new ZDecimal(validAnnualLines.Sum(x => x.GeneralLedgerAmountCR)).ToString(decimals)),
					BuildJournalXml(transactionGroups, additionalData, report)
					);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildJournalXml(IEnumerable<AccComplianceReportLine>[] transactionGroups, SAFTAdditionalDataCollector additionalData, AccComplianceReport report)
		{
			return new XStreamingElement("Journal",
						new XElement("JournalID", report.Company.GC_Code),
						new XElement("Description", report.Company.GC_Name),
						new XElement("Type", additionalData.JournalType),
						transactionGroups.Select(x => BuildTransactionXml(x, additionalData, report.Company.LocalCurrency.Decimals)));
		}

		IEnumerable<AccComplianceReportLine> GetValidAnnualLine(AccComplianceReport report)
		{
			var periodCalculator = new AccountingPeriodCalculator(report.Factory);
			var financialYear = new ZInt(periodCalculator.GetPeriodFromDate(report.ACR_DateFrom) / 100);
			var startDate = periodCalculator.GetFirstDayForPeriod(periodCalculator.GetFirstPeriodForYear(financialYear));
			var endDate = periodCalculator.GetLastDayForPeriod(periodCalculator.GetLastPeriodForYear(financialYear));

			var query = new ZQuery(AccComplianceReportSchema.ACR_ReportType, SQLComparisonOperator.Equal, report.ACR_ReportType);
			query.AddToFilter(AccComplianceReportSchema.ACR_GC_Company, SQLComparisonOperator.Equal, report.ACR_GC_Company);
			query.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
			query.AddToFilter(AccComplianceReportSchema.ACR_DateTo, SQLComparisonOperator.LessThanOrEqualTo, endDate);

			var reports = report.Factory.Load<AccComplianceReport>(query);
			var annualLines = new List<AccComplianceReportLine>();
			reports.ForEach(x => annualLines.AddRange(x.ReportLinesExport.Cast<AccComplianceReportLine>()));
			return annualLines.Where(x => x.AH_TransactionType != TransactionTypes.GLNoteJournal);
		}

		IEnumerable<IEnumerable<AccComplianceReportLine>> GetTransactionGroups(IEnumerable<AccComplianceReportLine> lines)
		{
			var contraLines = lines.Where(x => x.AH_TransactionType == TransactionTypes.Contra)
				.GroupBy(y => new { y.AH_TransactionType, y.AH_TransactionNum });
			var transferLines = lines.Where(x => x.AH_TransactionType == TransactionTypes.Transfer)
				.GroupBy(y => new { y.AH_Ledger, y.AH_TransactionType, y.AH_TransactionNum });
			var wipAccrualLines = lines.Where(x => x.AH_TransactionType == TransactionLineTypes.WIP || x.AH_TransactionType == TransactionLineTypes.Accrual)
				.GroupBy(y => new { y.AH_TransactionType, y.PostDate });
			var journalLines = lines.Where(x => x.AH_TransactionType == TransactionTypes.GLReversingJournal || x.AH_TransactionType == TransactionTypes.GLAutoJournal)
				.GroupBy(y => new { y.AH_Ledger, y.AH_TransactionType, y.PostDate });
			var toExcludedLineTypes = new ZString[] { TransactionTypes.Contra, TransactionTypes.Transfer, TransactionLineTypes.WIP, TransactionLineTypes.Accrual, TransactionTypes.GLReversingJournal, TransactionTypes.GLAutoJournal };
			var otherLines = lines.Where(x => !toExcludedLineTypes.Contains(x.AH_TransactionType)).GroupBy(x => new { x.AH_PK, x.PostDate });

			var result = new List<IEnumerable<AccComplianceReportLine>>();
			result.AddRange(contraLines);
			result.AddRange(transferLines);
			result.AddRange(wipAccrualLines);
			result.AddRange(journalLines);
			result.AddRange(otherLines);

			return result.OrderBy(x => x.First().ACL_ReportSequence);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildTransactionXml(IEnumerable<AccComplianceReportLine> reportLines, SAFTAdditionalDataCollector additionalData, int localCurrencyDecimal)
		{
			var firstLine = reportLines.First();
			var postDate = firstLine.PostDate.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture);
			var period = AccountingPeriodCalculator.GetPeriodManagementFromDate(firstLine.PostDate);

			var transactionNum = firstLine.AH_TransactionNum;
			if (firstLine.AH_TransactionType == TransactionLineTypes.WIP || firstLine.AH_TransactionType == TransactionLineTypes.Accrual)
			{
				transactionNum = firstLine.PostDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			}

			var transactionID = firstLine.AH_TransactionType + transactionNum;
			var transactionIDWithoutLedgerList = new List<string>
			{
				TransactionLineTypes.WIP,
				TransactionLineTypes.Accrual,
				TransactionTypes.Contra,
				TransactionTypes.GLStandardJournal,
				TransactionTypes.GLNoteJournal
			};
			var transactionIDWithPostingDateList = new List<string>
			{
				TransactionTypes.GLReversingJournal,
				TransactionTypes.GLAutoJournal
			};
			var transactionIDWithLedgerAndPostingDateList = new List<string>
			{
				TransactionTypes.Invoice,
				TransactionTypes.CreditNote,
				TransactionTypes.AdjustmentNote
			};

			if (transactionIDWithPostingDateList.Contains(firstLine.AH_TransactionType))
			{
				transactionID = transactionID + "-" + firstLine.PostDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			}
			else if (!transactionIDWithoutLedgerList.Contains(firstLine.AH_TransactionType))
			{
				transactionID = firstLine.AH_Ledger + transactionID;

				if (transactionIDWithLedgerAndPostingDateList.Contains(firstLine.AH_TransactionType))
				{
					transactionID = transactionID + "-" + firstLine.PostDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				}
			}

			return new XStreamingElement("Transaction",
					new XElement("TransactionID", transactionID),
					new XElement("Period", int.Parse(period.AM_Period.ToString().Substring(4, 2))),
					new XElement("PeriodYear", period.AM_Year),
					new XElement("TransactionDate", postDate),
					new XElement("Description", GetTransactionDescription(firstLine, additionalData)),
					new XElement("SystemEntryDate", postDate),
					new XElement("GLPostingDate", postDate),
					BuildTransactionLinesXml(reportLines, additionalData, transactionNum, localCurrencyDecimal)
				);
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
				result = additionalData.HeaderDescriptions.GetValueSafe(reportLine.AH_PK);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		ArrayList BuildTransactionLinesXml(IEnumerable<AccComplianceReportLine> reportLines, SAFTAdditionalDataCollector additionalData, ZString transactionID, int localCurrencyDecimal)
		{
			var lineSequence = 1;
			var result = new ArrayList();

			foreach (var line in reportLines)
			{
				var taxGroupCode = ZString.Empty;
				additionalData.TaxGroupCodes.TryGetValue(line.AL_A9_VATClass, out taxGroupCode);
				var glAccountType = ZString.Empty;
				additionalData.GLAccountTypes.TryGetValue(line.GLAccountPK, out glAccountType);
				var amount = line.IsDebitLine ? line.GeneralLedgerAmountDR.ToString(localCurrencyDecimal) : line.GeneralLedgerAmountCR.ToString(localCurrencyDecimal);

				var lineXml = new XStreamingElement("Line",
						new XElement("RecordID", line.AH_Ledger + line.AH_TransactionType + transactionID + "-" + lineSequence++.ToString(CultureInfo.InvariantCulture)),
						new XElement("AccountID", line.AG_AccountNum),
						line.AH_Ledger == LedgerTypes.AccountsReceivable && line.GLAccountPK == AccountingConfigurationRegistry.Instance.ARControlAccount.Value
						? new XElement("CustomerID", line.OH_Code)
						: null,
						line.AH_Ledger == LedgerTypes.AccountsPayable && line.GLAccountPK == AccountingConfigurationRegistry.Instance.APControlAccount.Value
						? new XElement("SupplierID", line.OH_Code)
						: null,
						new XElement("Description", line.AG_Description),
						line.IsDebitLine ? new XStreamingElement("DebitAmount",
													new XElement("Amount", amount)) :
												new XStreamingElement("CreditAmount",
													new XElement("Amount", amount)),

						!line.AT_Code.IsEmpty && glAccountType == Core.Constants.AccountType.ProfitAndLossAccount ?
						new XStreamingElement("TaxInformation",
							new XElement("TaxType", additionalData.TaxRegistrationCode),
							new XElement("TaxCode", taxGroupCode),
							new XElement("TaxPercentage", line.AL_TaxRate.Round(8)),
							new XElement("Country", line.GC_RN_NKCountryCode),
							new XElement("TaxBase", amount),
							new XStreamingElement(line.IsDebitLine ? "DebitTaxAmount" : "CreditTaxAmount",
								new XElement("Amount", ((ZDecimal)Math.Abs(line.TotalTaxAmount)).ToString(localCurrencyDecimal)))
							) : null
						);

				result.Add(lineXml);
			}

			return result;
		}

		AccountingPeriodCalculator AccountingPeriodCalculator => accountingPeriodCalculator ?? (accountingPeriodCalculator = new AccountingPeriodCalculator(new BusinessObjectFactory()));
		AccountingPeriodCalculator accountingPeriodCalculator;
	}
}
