using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class DatiBeniServizi
	{
		public XStreamingElement BuildXML(TransactionInfo transaction)
		{
			BuildXmlForDettaglioLineeAndDatiRiepilogo(transaction, out IEnumerable<XStreamingElement> dettaglioLineeList, out IEnumerable<XStreamingElement> datiRiepilogoList);
			return new XStreamingElement("DatiBeniServizi", dettaglioLineeList, datiRiepilogoList);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No localization required., it is the xml node name, fixed value, it is the xml node name and doc. number, it is the xml node name and doc. date")]
		void BuildXmlForDettaglioLineeAndDatiRiepilogo(TransactionInfo transaction, out IEnumerable<XStreamingElement> dettaglioLineeList, out IEnumerable<XStreamingElement> datiRiepilogoList)
		{
			dettaglioLineeList = new List<XStreamingElement>();
			var summaryMap = new Dictionary<Tuple<string, string>, TaxSummary>();
			var linesEligibleToBeSend = transaction?.PostingJournalCollection?.Where(l => l.GLAccount != null) ?? new List<PostingJournal>().AsEnumerable();
			FatturaElettronicaDataHelper.RemoveZeroSequenceIfAny(linesEligibleToBeSend);

			foreach (var line in linesEligibleToBeSend)
			{
				var sequence = line.Sequence;

				CreateChargeCode(line, out var chargeCode);
				CreateJobNumber(line, transaction, out var jobNumber);

				ZDecimal localAmount = line.LocalAmount.Value * GetMultiplier(transaction);
				var taxRate = line.VATTaxID?.TaxRate.ToRateDecimalType();
				var taxGroupCode = GetTaxGroupCode(line);
				var taxMessage = taxGroupCode == null ? null : line.TaxMessageID?.EnglishTaxMessage;

				var key = new Tuple<string, string>(line.VATTaxID?.TaxCode ?? null, line.TaxMessageID?.TaxMessageCode ?? null);
				if (!summaryMap.ContainsKey(key))
				{
					var newSummary = new TaxSummary();
					newSummary.TaxRate = line.VATTaxID?.TaxRate;
					newSummary.ExtraRateTypeSymble = CreateExtraTaxType(line);
					newSummary.TaxGroupCode = taxGroupCode;
					newSummary.TaxMessage = taxMessage;
					newSummary.LocalAmountSum = 0m;
					newSummary.LocalGSTVATAmountSum = 0m;

					summaryMap.Add(key, newSummary);
				}

				var summary = summaryMap[key];
				summary.LocalAmountSum = summary.LocalAmountSum.Value + (line.LocalAmount ?? ZDecimal.Zero);
				summary.LocalGSTVATAmountSum = summary.LocalGSTVATAmountSum.Value + GetAmountForImposta(transaction, line);

				var dettaglioLinee = new XStreamingElement("DettaglioLinee");
				dettaglioLinee.Add(new XElement("NumeroLinea", sequence),
									new XElement("CodiceArticolo",
												 new XElement("CodiceTipo", new ZString(chargeCode).EnsureComplianceWithBasicLatin()),
												 new XElement("CodiceValore", jobNumber?.Left(35).EnsureComplianceWithBasicLatin())),
									new XElement("Descrizione", line.Description.EnsureComplianceWithBasicLatinAndLatin1Supplement()),
									new XElement("PrezzoUnitario", localAmount.ToAmountDecimalType(transaction)),
									new XElement("PrezzoTotale", localAmount.ToAmountDecimalType(transaction)),
									new XElement("AliquotaIVA", taxRate));
				if (taxGroupCode != null)
				{
					if (taxGroupCode != ItalyComplianceInfo.TaxMessageGroupCodes.N0)
					{
						dettaglioLinee.Add(new XElement("Natura", taxGroupCode));
						dettaglioLinee.Add(new XElement("RiferimentoAmministrazione", taxMessage?.Left(20).EnsureComplianceWithBasicLatin()));
					}

					if (FatturaElettronicaDataHelper.IsReceivable(transaction) && taxGroupCode == ItalyComplianceInfo.TaxMessageGroupGovtCodes.N35)
					{
						var exporterExemptionDocuments = FatturaElettronicaDataHelper.ExtractExporterExemptionDocumentNumberAndDate(transaction);
						foreach ((var exDocText, var exDocDate) in exporterExemptionDocuments)
						{
							var altriDatiGestionaliElement = new XElement("AltriDatiGestionali");
							altriDatiGestionaliElement.Add(new XElement("TipoDato", "INTENTO"),
								new XElement("RiferimentoTesto", exDocText.EnsureComplianceWithBasicLatin()),
								!exDocDate.IsEmpty ? new XElement("RiferimentoData", exDocDate.ToDateType()) : null);

							dettaglioLinee.Add(altriDatiGestionaliElement);
						}
					}
				}

				((IList<XStreamingElement>)dettaglioLineeList).Add(dettaglioLinee);
			}

			datiRiepilogoList = CreateDatiRiepilogoList(summaryMap, transaction);
		}

		class TaxSummary
		{
			public ZDecimal? TaxRate;
			public ZDecimal? LocalAmountSum;
			public ZDecimal? LocalGSTVATAmountSum;
			public string ExtraRateTypeSymble;
			public string TaxMessage;
			public string TaxGroupCode;
		}

		string GetTaxGroupCode(PostingJournal line)
		{
			string ret = line.TaxMessageID?.TaxGroupCode?.Code;
			if (ret != null)
			{
				if (FatturaElettronicaDataHelper.ShouldUseNewSchema)
				{
					ret = line.TaxMessageID?.TaxGroupCode?.GovernmentCode;
				}
				else if (ret.Length > 2)
				{
					ret = ret.Substring(0, 2);
				}
			}
			return ret;
		}

		string CreateExtraTaxType(PostingJournal line)
		{
			var extraTaxTypeCode = line.VATTaxID?.ExtraTaxType?.Code;
			return (extraTaxTypeCode.HasValue &&
				extraTaxTypeCode.Value == AccTaxRate.ExtraTypes.VATRemittedByCustomer) ?
				"S" : "I";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No localization required.")]
		IEnumerable<XStreamingElement> CreateDatiRiepilogoList(Dictionary<Tuple<string, string>, TaxSummary> summaryMap, TransactionInfo transaction)
		{
			var result = new List<XStreamingElement>();
			var decimals = GlbCompany.CurrentCompany.LocalCurrency.Decimals;

			foreach (var summary in summaryMap.Values)
			{
				XElement naturaElement = null;
				XElement riferimentoNormativoElement = null;

				if (summary.TaxGroupCode != null)
				{
					if (summary.TaxGroupCode != ItalyComplianceInfo.TaxMessageGroupCodes.N0)
					{
						naturaElement = new XElement("Natura", summary.TaxGroupCode);
						riferimentoNormativoElement = new XElement("RiferimentoNormativo", new ZString(summary.TaxMessage).Left(100).EnsureComplianceWithBasicLatinAndLatin1Supplement());
					}
				}

				int multiplier = GetMultiplier(transaction);
				ZDecimal imponibileImporto = summary.LocalAmountSum.Value * multiplier;
				ZDecimal imposta = summary.LocalGSTVATAmountSum.HasValue
					? (ZDecimal)Utilities.Round(summary.LocalGSTVATAmountSum.Value * multiplier, decimals)
					: ZDecimal.Zero;

				var datiRiepilogo = new XStreamingElement("DatiRiepilogo",
										new XElement("AliquotaIVA", summary.TaxRate.ToRateDecimalType()),
										naturaElement,
										new XElement("ImponibileImporto", imponibileImporto.ToAmountDecimalType(transaction)),
										new XElement("Imposta", imposta.ToAmountDecimalType(transaction)),
										new XElement("EsigibilitaIVA", summary.ExtraRateTypeSymble),
										riferimentoNormativoElement);

				result.Add(datiRiepilogo);
			}

			return result;
		}

		ZDecimal? GetAmountForImposta(TransactionInfo transaction, PostingJournal line)
		{
			ZDecimal? value = ZDecimal.Zero;
			if (FatturaElettronicaDataHelper.IsPayable(transaction) && line.VATTaxID?.TaxType?.Code.ToString() == AccTaxRate.Types.ReverseRated)
			{
				value = line.LocalAmount.Value * (line.VATTaxID.TaxRate / 100);
			}
			else
			{
				value = (line.VATTaxID?.ExtraTaxType?.Code?.ToString() == AccTaxRate.ExtraTypes.VATRemittedByCustomer) ?
						  (line.LocalExtraVATAmount.HasValue ?
							  new ZDecimal?(-line.LocalExtraVATAmount.Value) :
							  null) :
						  line.LocalGSTVATAmount;
			}
			return value;
		}

		void CreateChargeCode(PostingJournal line, out UniversalDataBuss.DataObjects.Core.ZCodeMappedZString? chargeCode)
		{
			chargeCode = line.ChargeCode?.Code;
			if (!chargeCode.HasValue || chargeCode.Value == ZString.Empty)
			{
				chargeCode = line.GLAccount?.AccountCode;
			}
		}

		void CreateJobNumber(PostingJournal line, TransactionInfo transaction, out ZString? jobNumber)
		{
			jobNumber = line.Job?.Key;
			if (!jobNumber.HasValue || jobNumber.Value == ZString.Empty)
			{
				jobNumber = transaction.Number;
			}
		}
		int GetMultiplier(TransactionInfo transaction) => FatturaElettronicaDataHelper.IsPayableInvOrPositiveAdj(transaction) ? -1 : 1;
	}
}
