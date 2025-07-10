using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	[CodeAlive("Could be used later when certification becomes Accounting+Billing")]
	internal class GeneralLedgerAccounts : ComplianceReportXmlBuilder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			Argument.NotNull(additionalData, nameof(additionalData));

			var decimals = report.Company.LocalCurrency.Decimals;

			return new XStreamingElement("GeneralLedgerAccounts",
					new XElement("TaxonomyReference", "S"),
					((SAFTAdditionalDataCollector)additionalData).ActiveGLAccounts.Values.OrderBy(x => x.AccountNum).Select(x => new XStreamingElement("Account",
						new XElement("AccountID", x.AccountNum),
						new XElement("AccountDescription", x.Description),
						new XElement("OpeningDebitBalance", x.OpeningDebitBalance.ToString(decimals)),
						new XElement("OpeningCreditBalance", x.OpeningCreditBalance.ToString(decimals)),
						new XElement("ClosingDebitBalance", x.ClosingDebitBalance.ToString(decimals)),
						new XElement("ClosingCreditBalance", x.ClosingCreditBalance.ToString(decimals)),
						new XElement("GroupingCategory", GetGroupingCategory(x.AccountType)),
						!x.ConsolidationAccountNum.IsEmpty ? new XElement("GroupingCode", x.ConsolidationAccountNum) : null,
						new XElement("TaxonomyCode", "S")
					))
				);
		}

		ZString GetGroupingCategory(ZString accountType)
		{
			switch (accountType)
			{
				case Core.Constants.AccountType.BalanceSheetAccount:
				case Core.Constants.AccountType.ProfitAndLossAccount:
					return "GM";   // Hard-coded xml node value
				case Core.Constants.AccountType.Consolidation:
				case Core.Constants.AccountType.Header:
				case Core.Constants.AccountType.Alternate:
				case Core.Constants.AccountType.Total:
					return "GA";   // Hard-coded xml node value
				case Core.Constants.AccountType.Note:	//Note Journal should not be included in SAFT
				default:
					throw new ArgumentException("Invalid GL Account type: " + accountType);   // Developer error text
			}
		}
	}
}
