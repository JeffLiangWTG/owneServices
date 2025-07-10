using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M.JPKAdditionalDataCollector;
using static Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M.JPKWriterHelper;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M
{
	internal class Deklaracja : ComplianceReportXmlBuilder
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

			#region SuppressResourceStringsCheckRegion

			return new XStreamingElement("Deklaracja",
					getNaglowek(),
					GetPozycjeSzczegolowe(report, jpkAdditionalData),
					new XElement("Pouczenia", "1")
				);

			XElement getNaglowek()
			{
				var formCodeElement = new XElement("KodFormularzaDekl", "VAT-7");
				formCodeElement.SetAttributeValue("kodSystemowy", "VAT-7 (22)");
				formCodeElement.SetAttributeValue("kodPodatku", "VAT");
				formCodeElement.SetAttributeValue("rodzajZobowiazania", "Z");
				formCodeElement.SetAttributeValue("wersjaSchemy", "1-0E");

				return new XElement("Naglowek",
						formCodeElement,
						new XElement("WariantFormularzaDekl", "22")
					);
			}

			#endregion
		}

		XElement GetPozycjeSzczegolowe(AccComplianceReport report, JPKAdditionalDataCollector additionalData)
		{
			#region SuppressResourceStringsCheckRegion

			var totals = new Dictionary<string, decimal>
			{
				{ "P_10", decimal.Zero },
				{ "P_11", decimal.Zero },
				{ "P_12", decimal.Zero },
				{ "P_13", decimal.Zero },
				{ "P_15", decimal.Zero },
				{ "P_16", decimal.Zero },
				{ "P_17", decimal.Zero },
				{ "P_18", decimal.Zero },
				{ "P_19", decimal.Zero },
				{ "P_20", decimal.Zero },
				{ "P_23", decimal.Zero },
				{ "P_24", decimal.Zero },
				{ "P_27", decimal.Zero },
				{ "P_28", decimal.Zero },
				{ "P_29", decimal.Zero },
				{ "P_30", decimal.Zero },
				{ "P_37", decimal.Zero },
				{ "P_38", decimal.Zero },
				{ "P_39", decimal.Zero },
				{ "P_42", decimal.Zero },
				{ "P_43", decimal.Zero },
				{ "P_48", decimal.Zero },
				{ "P_51", decimal.Zero },
				{ "P_53", decimal.Zero },
				{ "P_54", decimal.Zero },
			};

			var requiredElements = new string[] { "P_38", "P_39", "P_51" };

			foreach (var transaction in additionalData.HeaderData.Values)
			{
				var lines = report.ReportLines.Where(x => x.ACL_ReportSequence >= transaction.HeaderSequence && x.ACL_ReportSequence <= transaction.LastSequence).ToArray();

				if (lines.Any())
				{
					var firstLine = lines.FirstOrDefault(x => x.ACL_ReportSequence == transaction.HeaderSequence);
					var category = GetOrgCountryCategory(additionalData, firstLine);
					var isPL = category == CountryCategory.IsPoland;
					var isEUexPL = category == CountryCategory.IsEU;
					var outsideEU = category == CountryCategory.OutsideEU;

					switch (transaction.Ledger)
					{
						case LedgerTypes.AccountsReceivable:
							var totalExTaxEXCLUDE = GetTotalExTaxAmount(lines, TaxCodes.EXCLUDE);
							var serviceExTaxEXCLUDE = GetServiceExTaxAmount(lines, TaxCodes.EXCLUDE);

							totals["P_10"] += GetServiceExTaxAmount(lines, TaxCodes.EXEMPT);
							totals["P_11"] += !isPL ? totalExTaxEXCLUDE : decimal.Zero;
							totals["P_12"] += isEUexPL ? serviceExTaxEXCLUDE : decimal.Zero;

							totals["P_13"] += GetServiceExTaxAmount(lines, additionalData.FreePtuTaxCodes);
							totals["P_15"] += GetServiceExTaxAmount(lines, TaxCodes.LOWPTU);
							totals["P_16"] += GetServiceTaxAmount(lines, TaxCodes.LOWPTU);
							totals["P_17"] += GetServiceExTaxAmount(lines, TaxCodes.MIDPTU);
							totals["P_18"] += GetServiceTaxAmount(lines, TaxCodes.MIDPTU);
							totals["P_19"] += GetServiceExTaxAmount(lines, additionalData.PtuCapPtuCodes);
							totals["P_20"] += GetServiceTaxAmount(lines, additionalData.PtuCapPtuCodes);

							break;
						case LedgerTypes.AccountsPayable:
							var serviceExTaxREV = GetServiceExTaxAmount(lines, additionalData.RevTaxCodes);
							var serviceTaxREV = GetServiceTaxAmount(lines, additionalData.RevTaxCodes);
							var goodsTaxREV = GetGoodsTaxAmount(lines, additionalData.RevTaxCodes);
							var goodsExTaxREV = GetGoodsExTaxAmount(lines, additionalData.RevTaxCodes);

							totals["P_23"] -= isEUexPL ? goodsExTaxREV : decimal.Zero;
							totals["P_24"] -= isEUexPL ? goodsTaxREV : decimal.Zero;
							totals["P_27"] -= outsideEU ? serviceExTaxREV : decimal.Zero;
							totals["P_28"] -= outsideEU ? serviceTaxREV : decimal.Zero;
							totals["P_29"] -= isEUexPL ? serviceExTaxREV : decimal.Zero;
							totals["P_30"] -= isEUexPL ? serviceTaxREV : decimal.Zero;

							totals["P_42"] -= GetTotalExTaxAmount(lines);
							totals["P_43"] -= GetTotalTaxAmount(lines);

							break;
					}
				}
			}

			foreach (var key in totals.Keys.ToArray())
			{
				totals[key] = decimal.Round(totals[key], System.MidpointRounding.AwayFromZero);
			}

			totals["P_37"] = totals["P_10"] + totals["P_11"] + totals["P_13"] + totals["P_15"] + totals["P_17"] + totals["P_19"] + totals["P_23"] + totals["P_27"] + totals["P_29"];
			totals["P_38"] = totals["P_16"] + totals["P_18"] + totals["P_20"] + totals["P_24"] + totals["P_28"] + totals["P_30"];
			totals["P_48"] = totals["P_43"];
			totals["P_51"] = totals["P_38"] > totals["P_48"] ? totals["P_38"] - totals["P_48"] : decimal.Zero;
			totals["P_53"] = totals["P_48"] > totals["P_38"] ? totals["P_48"] - totals["P_38"] : decimal.Zero;
			totals["P_54"] = totals["P_48"] > totals["P_38"] ? totals["P_48"] - totals["P_38"] : decimal.Zero;

			return new XElement("PozycjeSzczegolowe",
				totals.Where(x => x.Value != decimal.Zero || requiredElements.Contains(x.Key)).OrderBy(x => x.Key).Select(x => new XElement(x.Key, FormatAmountAsInteger(x.Value))),
				totals["P_53"] > 0 && totals["P_54"] > 0 ? new XElement("P_57", "1") : null,
				new XElement("P_68", "0"),
				new XElement("P_69", "0"));

			#endregion
		}
	}
}
