using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro
{
	internal class CedentePrestatoreDTR : ComplianceReportXmlBuilder
	{
		internal CedentePrestatoreDTR(List<AccComplianceReportLine> lines)
		{
			Argument.NotNull(lines, nameof(lines));
			if (!lines.Any())
			{
				throw new ArgumentException(nameof(lines) + " must contain elements");
			}

			var transactionPK = lines.First().AH_PK;
			if (!lines.All(x => x.AH_PK == transactionPK))
			{
				throw new ArgumentException(nameof(lines) + " must contain report lines for the same transaction header");
			}

			reportLines = lines;
		}

		readonly List<AccComplianceReportLine> reportLines;

		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			Argument.NotNull(additionalData, nameof(additionalData));

			return new XStreamingElement("CedentePrestatoreDTR",   // Hard-coded xml node name
					BuildIdentificativiFiscaliXml(reportLines.First(), additionalData),
					BuildAltriDatiIdentificativiXml(reportLines.First(), additionalData),
					BuildDatiFatturaBodyDTRXml(additionalData)
				);
		}

		XStreamingElement BuildIdentificativiFiscaliXml(AccComplianceReportLine lineData, ComplianceReportAdditionalDataCollector additionalData)
		{
			additionalData.OrgTaxRegistrationNumberDetails.TryGetValue(lineData.OH_Code, out (ZString, ZString) countryCodeAndtaxRegNumber);
			var taxRegNumber = countryCodeAndtaxRegNumber.Item2;
			var countryCode = countryCodeAndtaxRegNumber.Item1;
			if (taxRegNumber.IsEmpty)
			{
				if (!lineData.OH_FullName.IsEmpty && lineData.OrgCountryCode != "IT")
				{
					countryCode = lineData.OrgCountryCode;
					taxRegNumber = lineData.OH_FullName.Substring(0, 28);
				}
				else
				{
					countryCode = ZString.Empty;
				}
			}
			return new XStreamingElement("IdentificativiFiscali", FatturaElettronicaXmlElementCreator.CreateIdFiscaleType(countryCode, taxRegNumber));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildAltriDatiIdentificativiXml(AccComplianceReportLine lineData, ComplianceReportAdditionalDataCollector additionalData)
		{
			var orgFullName = lineData.OH_FullName;
			var denominazione = !orgFullName.IsEmpty && orgFullName.Length > 80 ? orgFullName.Substring(0, 80) : orgFullName;
			ZString address1, address2, combinedAddress, city, state, postCode, country;

			if (additionalData.OrgAddresses.TryGetValue(lineData.OH_Code, out OrganizationAddress orgAddress))
			{
				address1 = orgAddress.Address1 ?? ZString.Empty;
				address2 = orgAddress.Address2 ?? ZString.Empty;
				combinedAddress = address1 + " " + address2;
				city = orgAddress.City ?? ZString.Empty;
				state = ((ZString?)orgAddress.State).GetValueOrDefault();
				postCode = orgAddress.Postcode.HasValue ? FatturaElettronicaXmlValueFormatter.GetItalyPostCode(orgAddress.Postcode.Value) : string.Empty;
				country = orgAddress.Country.Code ?? ZString.Empty;
			}

			return new XStreamingElement("AltriDatiIdentificativi",
							new XElement("Denominazione", denominazione.EnsureComplianceWithBasicLatinAndLatin1Supplement()),
							FatturaElettronicaXmlElementCreator.CreateIndirizzoType(combinedAddress.Trim(), null, postCode, city, state, country));
		}

		XStreamingElement BuildDatiFatturaBodyDTRXml(ComplianceReportAdditionalDataCollector additionalData)
		{
			var tipoDocumento = GetTipoDocumentoForTransaction(reportLines.First());
			var result = new XStreamingElement("DatiFatturaBodyDTR", BuildDatiGeneraliXml(reportLines.First(), tipoDocumento)); // real work is going to be done in the next WI,
			foreach (var lineData in reportLines)
			{
				result.Add(BuildDatiRiepilogoXml(lineData, additionalData, tipoDocumento));
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildDatiGeneraliXml(AccComplianceReportLine lineData, ZString tipoDocumento)
		{
			return new XStreamingElement("DatiGenerali",
				new XElement("TipoDocumento", tipoDocumento),
				new XElement("Data", lineData.InvoiceDate.ToDateType()),
				new XElement("Numero", lineData.AH_TransactionNum.GetFormattedTransactionNumber()),
				new XElement("DataRegistrazione", lineData.PostDate.ToDateType()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildDatiRiepilogoXml(AccComplianceReportLine lineData, ComplianceReportAdditionalDataCollector additionalData, ZString tipoDocumento)
		{
			ZDecimal lineTotal = lineData.TotalExTaxAmount;
			ZDecimal taxAmount = lineData.AT_Type == AccTaxRate.Types.ReverseRated ? lineData.TaxReverseChargeAmount
							   : lineData.AT_Type.IsEmpty ? (ZDecimal)0.00m
							   : lineData.TotalTaxAmount;
			switch (tipoDocumento)
			{
				case "TD01":
				case "TD04":
					lineTotal = Math.Abs(lineTotal);
					taxAmount = Math.Abs(taxAmount);
					break;
				case "TD10":
				case "TD11":
					lineTotal = -lineTotal;
					taxAmount = -taxAmount;
					break;
			}
			var natura = lineData.AT_Type.IsEmpty ? (ZString)"N2" : additionalData.GetTaxMsgGroupCode(lineData);

			return new XStreamingElement("DatiRiepilogo",
			new XElement("ImponibileImporto", lineTotal.ToAmountDecimalType()),
				new XStreamingElement("DatiIVA",
					new XElement("Imposta", taxAmount.ToAmountDecimalType()),
					new XElement("Aliquota", lineData.AL_TaxRate.ToRateDecimalType())),
				natura.IsEmpty ? null : new XElement("Natura", natura));
		}

		ZString GetTipoDocumentoForTransaction(AccComplianceReportLine lineData)
		{
			var tipoDocumento = ZString.Empty;
			var complianceSubType = lineData.AH_ComplianceSubType;
			var transactionType = lineData.AH_TransactionType;
			var isPositiveAdjNote = transactionType == TransactionTypes.AdjustmentNote && lineData.AH_InvoiceAmount <= 0; //Positive AP Adjustment Notes are stored as negative values in DB, and vice versa for negative AP Adjustment notes
			var isNegativeAdjNote = transactionType == TransactionTypes.AdjustmentNote && lineData.AH_InvoiceAmount > 0; //Negative AP Adjustment Notes are stored as positive values in DB, and vice versa for negative AP Adjustment notes

			if ((transactionType == TransactionTypes.Invoice || isPositiveAdjNote)
				&& (complianceSubType == ItalyComplianceInfo.ComplianceSubTypeCodes.APS))
			{
				tipoDocumento = "TD01";
			}
			else if (transactionType == TransactionTypes.CreditNote || isNegativeAdjNote)
			{
				tipoDocumento = "TD04";
			}
			else if ((transactionType == TransactionTypes.Invoice || isPositiveAdjNote)
				&& complianceSubType == ItalyComplianceInfo.ComplianceSubTypeCodes.INT)
			{
				tipoDocumento = GetTipoDocumentBasedOnGoodsAndServiceAmounts();
			}
			return tipoDocumento;
		}

		ZString GetTipoDocumentBasedOnGoodsAndServiceAmounts()
		{
			var tipoDocumento = ZString.Empty;
			var totalServiceAmount = Math.Abs(reportLines.Sum(x => x.ServiceExTaxAmount));
			var totalGoodsAmount = Math.Abs(reportLines.Sum(x => x.GoodsExTaxAmount));

			if (totalGoodsAmount >= totalServiceAmount)
			{
				tipoDocumento = "TD10";
			}
			else
			{
				tipoDocumento = "TD11";
			}
			return tipoDocumento;
		}
	}
}
