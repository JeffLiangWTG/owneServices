using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class TaxTable : ComplianceReportXmlBuilder
	{
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var taxIDs = additionalData.GetInvoicedTaxIDs().OrderBy(x => x.Key).Select(x => x.Value).GroupBy(x => new {
				TaxType = PortugalComplianceInfo.GetTaxType(x.TaxCode),
				TaxCountryRegion = PortugalComplianceInfo.GetTaxCountryRegion(x.TaxCode),
				TaxCode = GetTaxCode(x, PortugalComplianceInfo.GetTaxType(x.TaxCode)),
				Description = GetTaxDescription(GetTaxCode(x, PortugalComplianceInfo.GetTaxType(x.TaxCode))),
				TaxPercentage = x.TaxRate
			});

			return taxIDs.Any() ? new XStreamingElement("TaxTable", taxIDs.Select(x => BuildTaxTableEntryXml(x.First(), "TaxTableEntry", true))) : null; // Hard-coded xml node name
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		internal static XStreamingElement BuildTaxXml(TaxID taxID) => BuildTaxTableEntryXml(taxID, "Tax", false);

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		static XStreamingElement BuildTaxTableEntryXml(TaxID taxID, string rootNodeName, bool includeDescription)
		{
			var taxType = PortugalComplianceInfo.GetTaxType(taxID.TaxCode);
			var taxCode = GetTaxCode(taxID, taxType);

			return new XStreamingElement(rootNodeName,
					new XElement("TaxType", taxType),
					new XElement("TaxCountryRegion", PortugalComplianceInfo.GetTaxCountryRegion(taxID.TaxCode)),
					new XElement("TaxCode", taxCode),
					includeDescription ? new XElement("Description", GetTaxDescription(taxCode)) : null,
					new XElement("TaxPercentage", taxID.TaxRate)
				);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded value")]
		static ZString GetTaxCode(TaxID taxID, string taxType)
		{
			if (taxType == "NS")
			{
				return "NS";
			}

			switch (taxID.TaxCode)
			{
				case "EXEMPT":
				case "FREEIVA":
				case "FREEIVAREV":
				case "IVAREV":
				case "IVAREV6":
				case "IVAREV13":
					return "ISE";
				case "IVA6":
					return "RED";
				case "IVA13":
					return "INT";
				default:
					return "NOR";
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded value, Dveloper error message")]
		static ZString GetTaxDescription(ZString code)
		{
			switch (code)
			{
				case "ISE":
					return "Isenta";
				case "RED":
					return "Reduzida";
				case "INT":
					return "Intermédia";
				case "NS":
					return "Nao sujeicao";
				case "NOR":
					return "Normal";
				default:
					throw new ArgumentException("Incorrect Tax Code: " + code);
			}
		}
	}
}
