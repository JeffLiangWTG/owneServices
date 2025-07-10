using System.Linq;
using System.Xml.Linq;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M.JPKAdditionalDataCollector;
using static Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M.JPKWriterHelper;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M
{
	internal class Ewidencja : ComplianceReportXmlBuilder
	{
		/// <summary>
		/// Builds XStreamingElement for Naglowek node
		/// </summary>
		/// <param name="report">AccComplianceReport</param>
		/// <param name="line">AccComplianceReportLineBase</param>
		/// <param name="additionalData">ComplianceReportAdditionalDataCollector</param>
		/// <returns></returns>
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var jpkAdditionalData = ((JPKAdditionalDataCollector)additionalData);

			int arTransactionCount = 0;
			int apTransactionCount = 0;

			var reportLines = report.ReportLines.OfType<AccComplianceReportLine>();

			bool isEligibleForSprzedazWiersz(TransactionHeaderDetailsJPK transaction)
				=> transaction.Ledger == LedgerTypes.AccountsReceivable
					|| (transaction.Ledger == LedgerTypes.AccountsPayable
						&& report.GetTransactionLines(transaction).Any(x => jpkAdditionalData.RevTaxCodes.Contains(x.AT_Code.ToString())
							&& (GetOrgCountryCategory(jpkAdditionalData, x) == CountryCategory.IsEU
								|| (!x.ServiceExTaxAmount.IsEmpty && GetOrgCountryCategory(jpkAdditionalData, x) == CountryCategory.OutsideEU))));

			var eligibleHeaders = jpkAdditionalData.HeaderData.Where(x => isEligibleForSprzedazWiersz(x.Value)).ToArray();
			var apHeaders = jpkAdditionalData.HeaderData.Where(x => x.Value.Ledger == LedgerTypes.AccountsPayable).ToArray();

			#region SuppressResourceStringsCheckRegion

			return new XStreamingElement("Ewidencja",
					eligibleHeaders.OrderBy(x => x.Value.HeaderSequence)
						.Select(x => BuildSprzedazWiersz(report, jpkAdditionalData, x.Value, ref arTransactionCount)),
					new XElement("SprzedazCtrl", 
						new XElement("LiczbaWierszySprzedazy", eligibleHeaders.Length),
						GetPodatekNalezny(report, jpkAdditionalData)),
					apHeaders.OrderBy(x => x.Value.HeaderSequence)
						.Select(x => BuildZakupWiersz(report, jpkAdditionalData, x.Value, ref apTransactionCount)),
					new XElement("ZakupCtrl",
						new XElement("LiczbaWierszyZakupow", apHeaders.Length),
						GetTotalTaxAmountElement("PodatekNaliczony", reportLines.Where(x => x.AH_Ledger == LedgerTypes.AccountsPayable)))
				);

			#endregion
		}

		internal XElement BuildSprzedazWiersz(AccComplianceReport report, JPKAdditionalDataCollector additionalData, TransactionHeaderDetailsJPK transaction, ref int transactionCount)
		{
			var lines = report.GetTransactionLines(transaction);
			var firstLine = lines.FirstOrDefault(x => x.ACL_ReportSequence == transaction.HeaderSequence);

			var isReceivable = transaction.Ledger == LedgerTypes.AccountsReceivable;
			var isPayable = transaction.Ledger == LedgerTypes.AccountsPayable;

			(var category, var orgCountry, var orgRegistrationNumber) = GetOrgKeyData(additionalData, firstLine);
			var isPL = category == CountryCategory.IsPoland;
			var isEUexPL = category == CountryCategory.IsEU;
			var outsideEU = category == CountryCategory.OutsideEU;

			#region SuppressResourceStringsCheckRegion

			return new XElement("SprzedazWiersz",
					new XElement("LpSprzedazy", ++transactionCount),
					!string.IsNullOrEmpty(orgRegistrationNumber) ? new XElement("KodKrajuNadaniaTIN", orgCountry) : null,
					new XElement("NrKontrahenta", BRAKifEmpty(orgRegistrationNumber ?? string.Empty)),
					new XElement("NazwaKontrahenta", BRAKifEmpty(firstLine.OH_FullName)),
					new XElement("DowodSprzedazy", BRAKifEmpty(firstLine.AH_TransactionNum)),
					new XElement("DataWystawienia", FormatDate(transaction.InvoiceDate)),
					transaction.EarliestTaxDate != transaction.InvoiceDate ? new XElement("DataSprzedazy", FormatDate(transaction.EarliestTaxDate)) : null,
					transaction.IsGTU_13 ? new XElement("GTU_13", "1") : null,
					GetTP(additionalData, transaction, firstLine),
					isReceivable ? GetServiceExTaxAmountElement("K_10", lines, TaxCodes.EXEMPT) : null,
					!isPL && isReceivable ? GetTotalExTaxAmountElement("K_11", lines, TaxCodes.EXCLUDE) : null,
					isEUexPL && isReceivable ? GetServiceExTaxAmountElement("K_12", lines, TaxCodes.EXCLUDE) : null,
					isReceivable ? GetServiceExTaxAmountElement("K_13", lines, additionalData.FreePtuTaxCodes) : null,
					isReceivable ? GetServiceExTaxAmountElement("K_15", lines, TaxCodes.LOWPTU) : null,
					isReceivable ? GetServiceTaxAmountElement("K_16", lines, TaxCodes.LOWPTU) : null,
					isReceivable ? GetServiceExTaxAmountElement("K_17", lines, TaxCodes.MIDPTU) : null,
					isReceivable ? GetServiceTaxAmountElement("K_18", lines, TaxCodes.MIDPTU) : null,
					isReceivable ? GetServiceExTaxAmountElement("K_19", lines, additionalData.PtuCapPtuCodes) : null,
					isReceivable ? GetServiceTaxAmountElement("K_20", lines, additionalData.PtuCapPtuCodes) : null,
					isEUexPL && isPayable ? GetGoodsExTaxAmountElement("K_23", lines, additionalData.RevTaxCodes) : null,
					isEUexPL && isPayable ? GetGoodsTaxAmountElement("K_24", lines, additionalData.RevTaxCodes) : null,
					outsideEU && isPayable ? GetServiceExTaxAmountElement("K_27", lines, additionalData.RevTaxCodes) : null,
					outsideEU && isPayable ? GetServiceTaxAmountElement("K_28", lines, additionalData.RevTaxCodes) : null,
					isEUexPL && isPayable ? GetServiceExTaxAmountElement("K_29", lines, additionalData.RevTaxCodes) : null,
					isEUexPL && isPayable ? GetServiceTaxAmountElement("K_30", lines, additionalData.RevTaxCodes) : null
				);

			#endregion
		}

		XElement BuildZakupWiersz(AccComplianceReport report, JPKAdditionalDataCollector additionalData, TransactionHeaderDetailsJPK transaction, ref int transactionCount)
		{
			var lines = report.GetTransactionLines(transaction);
			var firstLine = lines.FirstOrDefault(x => x.ACL_ReportSequence == transaction.HeaderSequence);

			(var category, var orgCountry, var orgRegistrationNumber) = GetOrgKeyData(additionalData, firstLine);
			var outsideEU = category == CountryCategory.OutsideEU;
			var hasGoods = lines.Any(x => !x.GoodsExTaxAmount.IsEmpty);

			#region SuppressResourceStringsCheckRegion

			return new XElement("ZakupWiersz",
					new XElement("LpZakupu", ++transactionCount),
					!string.IsNullOrEmpty(orgRegistrationNumber) ? new XElement("KodKrajuNadaniaTIN", orgCountry) : null,
					new XElement("NrDostawcy", BRAKifEmpty(orgRegistrationNumber ?? string.Empty)),
					new XElement("NazwaDostawcy", BRAKifEmpty(firstLine.OH_FullName)),
					new XElement("DowodZakupu", BRAKifEmpty(firstLine.AH_TransactionNum)),
					new XElement("DataZakupu", FormatDate(transaction.InvoiceDate)),
					!transaction.DocumentReceivedDate.IsEmpty && transaction.DocumentReceivedDate != transaction.InvoiceDate ? new XElement("DataWplywu", FormatDate(transaction.DocumentReceivedDate)) : null,
					GetDokumentZakupu(additionalData, firstLine),
					outsideEU && hasGoods ? new XElement("IMP", "1") : null,
					GetTotalExTaxAmountElement("K_42", lines),
					GetTotalTaxAmountElement("K_43", lines)
				);

			#endregion
		}

		#region Element Getters

		XElement GetTP(JPKAdditionalDataCollector additionalData, TransactionHeaderDetailsJPK transaction, AccComplianceReportLine firstLine)
			=> additionalData.OrgProxies.Contains(transaction.OrgPK)
				|| (additionalData.OrgHeaderDetails.TryGetValue(firstLine.OH_Code, out var orgDetails)
					&& !orgDetails.ARConsolidatedAccountingCategory.IsEmpty && orgDetails.ARConsolidatedAccountingCategory != Constants.AccountsCategory.Unrelated)
			? new XElement("TP", "1")
			: null;

		XElement GetDokumentZakupu(JPKAdditionalDataCollector additionalData, AccComplianceReportLine firstLine)
		{
			if (additionalData.OrgHeaderDetails.TryGetValue(firstLine.OH_Code, out var orgDetails)
				&& orgDetails.APVATConfig == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code)
			{
				return new XElement("DokumentZakupu", "MK");
			}

			return null;
		}

		XElement GetPodatekNalezny(AccComplianceReport report, JPKAdditionalDataCollector additionalData)
		{
			// This methods repeats the calculation logic of the SprzedazWiersz to sum up Amounts we get for the following nodes:
			// K_16 + K_18 + K_20 + K_24 + K_28 + K_30
			var totalTax = decimal.Zero;

			foreach (var transaction in additionalData.HeaderData.Values)
			{
				var lines = report.GetTransactionLines(transaction);
				var firstLine = lines.FirstOrDefault(x => x.ACL_ReportSequence == transaction.HeaderSequence);

				if (transaction.Ledger == LedgerTypes.AccountsReceivable)
				{
					// Sum up Service Tax Amount for PTU range taxes = K_16 + K_18 + K_20
					totalTax += lines.Where(x => additionalData.PtuRangeTaxCodes.Contains(x.AT_Code.ToString())).Sum(x => x.ServiceTaxAmount);
				}
				else if (transaction.Ledger == LedgerTypes.AccountsPayable)
				{
					var category = GetOrgCountryCategory(additionalData, firstLine);
					var isEUexPL = category == CountryCategory.IsEU;
					var outsideEU = category == CountryCategory.OutsideEU;

					if (isEUexPL)
					{
						// Sum up Reverse Charge Tax Amount for both Goods and Service = K_24 + K_30
						totalTax -= lines.Where(x => additionalData.RevTaxCodes.Contains(x.AT_Code.ToString()))
							.Sum(x => x.TaxReverseChargeAmount);
					}
					else if (outsideEU)
					{
						// Sum up Reverse Charge Tax Amount for Service only = K_28
						totalTax -= lines.Where(x => !x.ServiceExTaxAmount.IsEmpty && additionalData.RevTaxCodes.Contains(x.AT_Code.ToString()))
							.Sum(x => x.TaxReverseChargeAmount);
					}
				}
			}

			return new XElement("PodatekNalezny", FormatAmount(totalTax));
		}

		#endregion
	}
}
