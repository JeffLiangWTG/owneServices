using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M
{
	internal static class JPKWriterHelper
	{
		#region Formatting methods

		internal static string BRAKifEmpty(ZString value) => value.Trim().IsEmpty ? "BRAK" : value.ToString();

		internal static string FormatDate(ZDate date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

		internal static string FormatAmount(decimal amount, string ledger = LedgerTypes.AccountsReceivable)
			=> (ledger == LedgerTypes.AccountsPayable ? -amount : amount).ToString("F2", CultureInfo.InvariantCulture);

		internal static string FormatAmountAsInteger(decimal amount)
			=> decimal.Round(amount).ToString("0", CultureInfo.InvariantCulture);

		#endregion

		#region Service and Goods Ex Tax and Tax Sums for a single transaction lines. No sign reverse for AP

		internal static decimal GetServiceExTaxAmount(IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var noTaxFilter = !taxCodes.Any();

			return lines.Where(x => !x.ServiceExTaxAmount.IsEmpty && (noTaxFilter || taxCodes.Contains(x.AT_Code.ToString())))
				.Sum(x => x.ServiceExTaxAmount);
		}

		internal static decimal GetServiceTaxAmount(IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var noTaxFilter = !taxCodes.Any();
			return lines.Where(x => !x.ServiceExTaxAmount.IsEmpty && (noTaxFilter || taxCodes.Contains(x.AT_Code.ToString())))
				.Sum(x => x.ServiceTaxAmount + x.TaxReverseChargeAmount);
		}

		internal static decimal GetGoodsExTaxAmount(IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var noTaxFilter = !taxCodes.Any();
			return lines.Where(x => !x.GoodsExTaxAmount.IsEmpty && (noTaxFilter || taxCodes.Contains(x.AT_Code.ToString())))
				.Sum(x => x.GoodsExTaxAmount);
		}

		internal static decimal GetGoodsTaxAmount(IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var noTaxFilter = !taxCodes.Any();
			return lines.Where(x => !x.GoodsExTaxAmount.IsEmpty && (noTaxFilter || taxCodes.Contains(x.AT_Code.ToString())))
				.Sum(x => x.GoodsTaxAmount + x.TaxReverseChargeAmount);
		}

		internal static decimal GetTotalExTaxAmount(IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var noTaxFilter = !taxCodes.Any();
			return lines.Where(x => noTaxFilter || taxCodes.Contains(x.AT_Code.ToString())).Sum(x => x.TotalExTaxAmount);
		}

		internal static decimal GetTotalTaxAmount(IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var noTaxFilter = !taxCodes.Any();
			return lines.Where(x => noTaxFilter || taxCodes.Contains(x.AT_Code.ToString())).Sum(x => (x.TotalTaxAmount + x.TaxReverseChargeAmount));
		}

		#endregion

		#region Service and Goods Ex Tax and Tax Total Element getters

		internal static XElement GetServiceExTaxAmountElement(string nodeName, IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var linesWithTaxAndService = lines.Where(x => !x.ServiceExTaxAmount.IsEmpty && taxCodes.Contains(x.AT_Code.ToString()));

			return linesWithTaxAndService.Any()
				? new XElement(nodeName, FormatAmount(linesWithTaxAndService.Sum(x => x.ServiceExTaxAmount), linesWithTaxAndService.First().AH_Ledger))
				: null;
		}

		internal static XElement GetServiceTaxAmountElement(string nodeName, IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var linesWithTaxAndService = lines.Where(x => !x.ServiceExTaxAmount.IsEmpty && taxCodes.Contains(x.AT_Code.ToString()));

			return linesWithTaxAndService.Any()
				? new XElement(nodeName, FormatAmount(linesWithTaxAndService.Sum(x => x.ServiceTaxAmount + x.TaxReverseChargeAmount), linesWithTaxAndService.First().AH_Ledger))
				: null;
		}

		internal static XElement GetGoodsExTaxAmountElement(string nodeName, IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var linesWithTaxAndGoods = lines.Where(x => !x.GoodsExTaxAmount.IsEmpty && taxCodes.Contains(x.AT_Code.ToString()));

			return linesWithTaxAndGoods.Any()
				? new XElement(nodeName, FormatAmount(linesWithTaxAndGoods.Sum(x => x.GoodsExTaxAmount), linesWithTaxAndGoods.First().AH_Ledger))
				: null;
		}

		internal static XElement GetGoodsTaxAmountElement(string nodeName, IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var linesWithTaxAndGoods = lines.Where(x => !x.GoodsExTaxAmount.IsEmpty && taxCodes.Contains(x.AT_Code.ToString()));

			return linesWithTaxAndGoods.Any()
				? new XElement(nodeName, FormatAmount(linesWithTaxAndGoods.Sum(x => x.GoodsTaxAmount + x.TaxReverseChargeAmount), linesWithTaxAndGoods.First().AH_Ledger))
				: null;
		}

		internal static XElement GetTotalExTaxAmountElement(string nodeName, IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var noTaxFilter = !taxCodes.Any();
			var linesWithExTaxAmount = lines.Where(x => !x.TotalExTaxAmount.IsEmpty
				&& (noTaxFilter || taxCodes.Contains(x.AT_Code.ToString())));
			var ledger = linesWithExTaxAmount.FirstOrDefault()?.AH_Ledger;

			return linesWithExTaxAmount.Any()
				? new XElement(nodeName, FormatAmount(linesWithExTaxAmount.Sum(x => x.TotalExTaxAmount), ledger))
				: null;
		}

		internal static XElement GetTotalTaxAmountElement(string nodeName, IEnumerable<AccComplianceReportLine> lines, params string[] taxCodes)
		{
			var noTaxFilter = !taxCodes.Any();
			var linesWithExTaxAmount = lines.Where(x => !(x.TotalTaxAmount.IsEmpty && x.TaxReverseChargeAmount.IsEmpty)
				&& (noTaxFilter || taxCodes.Contains(x.AT_Code.ToString())));
			var ledger = lines.FirstOrDefault()?.AH_Ledger;

			return new XElement(nodeName, FormatAmount(lines.Sum(x => x.TotalTaxAmount + x.TaxReverseChargeAmount), ledger));
		}

		#endregion

		#region ReportLines Getter

		internal static IEnumerable<AccComplianceReportLine> GetTransactionLines(this AccComplianceReport report, TransactionHeaderDetailsJPK transaction)
		{
			var lines = report.ReportLines.Where(x => x.ACL_ReportSequence >= transaction.HeaderSequence && x.ACL_ReportSequence <= transaction.LastSequence);
			if (!lines.Any(x => x.ACL_ReportSequence == transaction.HeaderSequence) || !lines.Any(x => x.ACL_ReportSequence == transaction.LastSequence))
			{
				throw new NotSupportedException($@"Unable to generate {report.ACR_ReportType} output file.
Please, make sure the report's queue and line records were generated with valid configuration for {report.ACR_ReportType} report.");
			}
			return lines.ToArray();
		}

		#endregion

		#region Org Key Data Getter EU

		internal enum CountryCategory
		{
			OutsideEU,
			IsEU = 1,
			IsPoland = 3,
		}

		internal static CountryCategory GetOrgCountryCategory(JPKAdditionalDataCollector additionalData, AccComplianceReportLine line)
		{
			additionalData.OrgAddresses.TryGetValue(line?.OH_Code ?? ZString.Empty, out var orgDetails);
			var orgCountry = orgDetails?.Country.Code;

			return orgCountry.HasValue && orgCountry.Value == CountryCodes.Poland
				? CountryCategory.IsPoland
				: orgCountry.HasValue && additionalData.IsEUCountryCode(orgCountry)
				? CountryCategory.IsEU
				: CountryCategory.OutsideEU;
		}

		internal static (CountryCategory Category, string Country, string RegistrationNumber) GetOrgKeyData(JPKAdditionalDataCollector additionalData, AccComplianceReportLine line)
		{
			var category = GetOrgCountryCategory(additionalData, line);

			(var regCountry, var orgRegistrationNumber) = additionalData.OrgTaxRegistrationNumberDetails.TryGetValue(line?.OH_Code ?? ZString.Empty, out var regDetails)
				? (regDetails.Item1, new ZString(Regex.Replace(regDetails.Item2, @"[^0-9a-zA-Z]", string.Empty)))
				: (ZString.Empty, ZString.Empty);

			return (category, regCountry, orgRegistrationNumber);
		}

		#endregion
	}
}
