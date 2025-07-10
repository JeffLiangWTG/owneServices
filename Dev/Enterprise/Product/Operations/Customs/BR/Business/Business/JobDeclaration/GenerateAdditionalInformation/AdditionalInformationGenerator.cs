using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business
{
	public class AdditionalInformationGenerator
	{
		public AdditionalInformationGenerator(JobDeclaration jobDeclaration)
		{
			Declaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
			localCurrency = Declaration.LocalCurrency;
		}

		public readonly JobDeclaration Declaration;
		readonly RefCurrency localCurrency;

		public void GenerateAdditionalInformation()
		{
			Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(f => f.AdditionalInformation = (f.CEI_AdditionalInformationOption.IsEmpty ? ZString.Empty : CreateAdditionalInformation(f)));
		}

		CodeDescriptionPairList TaxRegimeList => Declaration.Factory.GetCachedValue<TaxRegimeList>();

		CodeDescriptionPairList IPITaxRegimeList => Declaration.Factory.GetCachedValue<IPITaxRegimeList>();

		CodeDescriptionPairList ICMSTaxRegimeList => Declaration.Factory.GetCachedValue<ICMSTaxRegimeList>();

		string CreateAdditionalInformation(CusEntryInstruction entryInstruction)
		{
			var additionalInfoText = new ZStringBuilder();
			additionalInfoText.AppendLine($"Processo: {Declaration.JE_DeclarationReference}");
			additionalInfoText.AppendLine($"Importador: {Declaration.Importer?.OH_FullName}");
			var numberType = (Declaration.Importer?.PrimaryRegistrationNumber?.NumberType ?? ZString.Empty).Replace("CJN", "CNPJ");
			additionalInfoText.AppendLine($"{numberType}: {Declaration.Importer?.PrimaryRegistrationNumber?.Number}");
			additionalInfoText.AppendLine();
			additionalInfoText.AppendLine($"Master: {Declaration.JE_MasterBill}");
			additionalInfoText.AppendLine($"House: {Declaration.JE_HouseBill}");
			additionalInfoText.AppendLine($"Conhecimento Eletrônico: {Declaration.JE_UCR}");
			if (Declaration.IsTransportByWater)
			{
				additionalInfoText.AppendLine($"Embarcação: {Declaration.JE_VesselName}");
				additionalInfoText.AppendLine($"Bandeira: {Declaration.Vessel?.CountryOfReg?.RN_DescMultilingual.ToString(Core.Constants.Languages.PortugueseBrazil) ?? ZString.Empty}");
			}
			else if (Declaration.IsAir)
			{
				additionalInfoText.AppendLine($"Número do Vôo: {Declaration.JE_VoyageFlightNo}");
			}
			else if (Declaration.IsRoad)
			{
				additionalInfoText.AppendLine($"Registro: {Declaration.JE_VoyageFlightNo}");
			}
			additionalInfoText.AppendLine($"Folio: {Declaration.JE_Folio}");
			additionalInfoText.AppendLine($"Número do Manifesto: {Declaration.JE_CargoArrivalDocumentNumber}");
			additionalInfoText.AppendLine($"Porto de Origem: {Declaration.PortOfLoading?.Description}");
			additionalInfoText.AppendLine($"Previsão de Saída: {Declaration.JE_DateAtOrigin.ToString(Constants.DataFormat)}");
			additionalInfoText.AppendLine($"Porto de Destino: {Declaration.FinalDestination?.Description}");
			additionalInfoText.AppendLine($"Previsão de Chegada: {Declaration.JE_DateAtFinalDestination.ToString(Constants.DataFormat)}");
			additionalInfoText.AppendLine();

			var packages = Declaration.Packages.Cast<BasePackage>();
			if (packages.Any())
			{
				packages.ForEach(package =>	additionalInfoText.AppendLine($"Volume: {package.CW_PackQty}x {package.PackTypeList.GetDescriptionFromCode(package.CW_PackType)}"));
				additionalInfoText.AppendLine();
			}
			
			additionalInfoText.AppendLine($"Local de Desembaraço");
			additionalInfoText.AppendLine($"URF: {Declaration.JE_CustomsOffice} - {Declaration.Lookups.CustomsOfficeList.GetDescriptionFromCode(Declaration.JE_CustomsOffice)}");
			additionalInfoText.AppendLine($"Recinto Aduaneiro: {Declaration.JE_LocationOfGoods} - {Declaration.Lookups.CustomsEnclosureList.GetDescriptionFromCode(Declaration.JE_LocationOfGoods)}");
			additionalInfoText.AppendLine();
			if (!Declaration.JE_SubLocationOfGoods.IsEmpty)
			{
				additionalInfoText.AppendLine($"Setor: {Declaration.JE_SubLocationOfGoods} - {Declaration.Lookups.SubLocationOfGoodsList.GetDescriptionFromCode(Declaration.JE_SubLocationOfGoods)}");
				additionalInfoText.AppendLine($"Área: {Declaration.WarehouseAreasConcatenated}");
				additionalInfoText.AppendLine();
			}
			additionalInfoText.AppendLine($"Local de Entrada");
			additionalInfoText.AppendLine($"URF: {Declaration.EntranceOfficeCode} - {Declaration.Lookups.CustomsOfficeList.GetDescriptionFromCode(Declaration.EntranceOfficeCode)}");
			additionalInfoText.AppendLine();

			var invoiceLines = entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>();

			additionalInfoText.AppendLine($"Total do FOB: {invoiceLines.Sum(s => s.JI_Calc_FOB_InLocalCurrency)} {localCurrency?.Code}");
			additionalInfoText.AppendLine($"Total do Frete (Collect): {invoiceLines.GetTotalChargesAmountOnInvoiceLines(c => c.J7_ChargeType == ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, localCurrency)} {localCurrency?.Code}");
			additionalInfoText.AppendLine($"Total do Frete (Prepaid): {invoiceLines.GetTotalChargesAmountOnInvoiceLines(c => c.J7_ChargeType == ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, localCurrency)} {localCurrency?.Code}");
			additionalInfoText.AppendLine($"Seguro: {invoiceLines.GetTotalChargesAmountOnInvoiceLines(c => c.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasInsurance, localCurrency)} {localCurrency?.Code}");

			var mergedLines = entryInstruction.EntryHeader?.MergedLines.Cast<CusEntryLine>();
			if (mergedLines != null)
			{
				additionalInfoText.AppendLine();
				additionalInfoText.AppendLine($"Total do II: {mergedLines.Sum(s => s.Fees.GetAmount(ChargeTypesList.Codes.DTY))} {localCurrency?.Code}");
				additionalInfoText.AppendLine($"Total do IPI: {mergedLines.Sum(s => s.Fees.GetAmount(RateTypes.IPI))} {localCurrency?.Code}");
				additionalInfoText.AppendLine($"Total do PIS: {mergedLines.Sum(s => s.Fees.GetAmount(RateTypes.PIS))} {localCurrency?.Code}");
				additionalInfoText.AppendLine($"Total do COFINS: {mergedLines.Sum(s => s.Fees.GetAmount(RateTypes.Cofins))} {localCurrency?.Code}");
				additionalInfoText.AppendLine($"Total do ICMS: {mergedLines.Sum(s => s.Fees.GetAmount(RateTypes.ICMS))} {localCurrency?.Code}");
				additionalInfoText.AppendLine($"Total do Antidumping: {mergedLines.Sum(s => s.Fees.GetAmount(RateTypes.Antidumping))} {localCurrency?.Code}");
				if (Declaration.IsImportSiscomex)
				{
					mergedLines.ForEach(entryLine =>
					{
						var randomLine = entryLine.RandomLine;

						additionalInfoText.AppendLine();
						additionalInfoText.AppendLine($"Adição {entryLine.CL_LineNumber} NCM: {randomLine.JI_Tariff}");
						if (!entryLine.ImportLicenseNumber.IsEmpty)
						{
							additionalInfoText.AppendLine($"Número do Licenciamento: {entryLine.ImportLicenseNumber}");
						}
						additionalInfoText.AppendLine($"Valor FOB: {entryLine.FOB.Amount.Round(2)} {entryLine.FOB?.Currency?.Code}");
						additionalInfoText.AppendLine($"Valor Aduaneiro: {entryLine.CL_CustomsValue.Round(2)} {localCurrency?.Code}");
						additionalInfoText.AppendLine();
						AddAdditionalInformationForAdditionalTariffs(additionalInfoText, randomLine.AdditionalTariffs);
						AddAdditionalInformationForFee(additionalInfoText, entryLine, ChargeTypesList.Codes.DTY, TaxRegimeList.GetDescriptionFromCode(randomLine.DutyTaxRegime));
						AddAdditionalInformationForFee(additionalInfoText, entryLine, RateTypes.IPI, IPITaxRegimeList.GetDescriptionFromCode(randomLine.IPITaxRegime));
						AddAdditionalInformationForFee(additionalInfoText, entryLine, RateTypes.PIS, TaxRegimeList.GetDescriptionFromCode(randomLine.PisCofinsTaxRegime));
						AddAdditionalInformationForFee(additionalInfoText, entryLine, RateTypes.Cofins, TaxRegimeList.GetDescriptionFromCode(randomLine.PisCofinsTaxRegime));
						AddAdditionalInformationForFee(additionalInfoText, entryLine, RateTypes.Antidumping, randomLine.AntidumpingLegalActType);
						AddAdditionalInformationForFee(additionalInfoText, entryLine, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee);
						AddAdditionalInformationForFee(additionalInfoText, entryLine, Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode);
						AddAdditionalInformationForFee(additionalInfoText, entryLine, Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode);
						AddAdditionalInformationForFee(additionalInfoText, entryLine, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax);
						AddAdditionalInformationForFee(additionalInfoText, entryLine, RateTypes.ICMS, ICMSTaxRegimeList.GetDescriptionFromCode(randomLine.ICMSTaxRegime));
					});
				}
			}

			return additionalInfoText.ToString().TrimEnd('\r', '\n');
		}

		void AddAdditionalInformationForAdditionalTariffs(ZStringBuilder additionalInfoText, AdditionalTariffCollection exTariffCollection)
		{
			if (exTariffCollection.Any())
			{
				exTariffCollection.Cast<AdditionalTariff>().ForEach(exTariff =>
				{
					var isTariffAgreement = exTariff.LegalActSubject == AdditionalTaxTypeList.Codes.TariffAgreement;
					additionalInfoText.Append(GetAdditionalTaxTypeLabel(exTariff.LegalActSubject, exTariff.LegalActSubjectDescription, exTariff.TariffTypeDescription));
					if (isTariffAgreement)
					{
						var agrrementCode = exTariff.TariffAgreementCode?.GetAttribute(Constants.RefCusCodeList.Attributes.AgreementCodeInImportEntry) ?? ZString.Empty;
						if (!agrrementCode.IsEmpty)
						{
							additionalInfoText.Append($" - {agrrementCode}");
						}
					}
					else
					{
						additionalInfoText.Append($" - Ex {exTariff.ExNumber}");
					}

					if (!exTariff.LegalActType.IsEmpty && !exTariff.LegalActIssuingBody.IsEmpty && !exTariff.LegalActNumber.IsEmpty && !exTariff.LegalActYear.IsEmpty)
					{
						additionalInfoText.Append($" - Ato Legal: {exTariff.LegalActType} - {exTariff.LegalActIssuingBody} - {exTariff.LegalActNumber} - {exTariff.LegalActYear}");
					}
					additionalInfoText.AppendLine(isTariffAgreement ? $" - Ex {exTariff.ExNumber}" : "");
				});
				additionalInfoText.AppendLine();
			}
		}

		void AddAdditionalInformationForFee(ZStringBuilder additionalInfoText, Customs.Business.CusEntryLine entryLine, ZString feeType, string taxRegime = null)
		{
			var fee = entryLine.Fees.GetElementWithThisCode(feeType);
			if (fee != null)
			{
				if (feeType == Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee)
				{
					additionalInfoText.AppendLine($"Taxa de Utilização do Siscomex: {fee.CF_ChargeAmount.Round(2)} {localCurrency?.Code}");
				}
				else if (feeType == Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax)
				{
					additionalInfoText.AppendLine($"AFRMM: {fee.CF_ChargeAmount.Round(2)} {localCurrency?.Code}");
				}
				else if (feeType == Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode || feeType == Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode)
				{
					additionalInfoText.AppendLine($"Valor da Multa do Licenciamento: {fee.CF_ChargeAmount.Round(2)} {localCurrency?.Code}");
				}
				else if (feeType == RateTypes.ICMS)
				{
					var invoiceLine = entryLine.RandomLine as JobComInvoiceLine;

					additionalInfoText.Append(GetFeeTypeLabel(feeType));
					additionalInfoText.Append($" Regime de Tributação: {taxRegime}");
					additionalInfoText.Append($" Base de Cálculo: {fee.CF_BaseValue.Round(2)} {localCurrency?.Code}");
					additionalInfoText.Append($" Alíquota: {fee.CF_Rate.Round(2)}");
					if (!invoiceLine.JI_ICMSBaseValueReductionPercentage.IsEmpty)
					{
						additionalInfoText.Append($" Redução da Base de Cálculo: {invoiceLine.JI_ICMSBaseValueReductionPercentage.Round(5)}");
					}
					if (!invoiceLine.JI_ICMSTotalAmountReductionPercentage.IsEmpty)
					{
						additionalInfoText.Append($" Redução do Valor Total: {invoiceLine.JI_ICMSTotalAmountReductionPercentage.Round(2)}");
					}
					ZString icmsLegaBase = invoiceLine.Lookups.ICMSLegalBaseList.GetDescriptionFromCode(invoiceLine.ICMSLegalBase);
					if (!icmsLegaBase.IsEmpty)
					{
						additionalInfoText.Append($" Base Legal: {icmsLegaBase}");
					}
					additionalInfoText.AppendLine($" Valor Total: {fee.CF_ChargeAmount.Round(2)} {localCurrency?.Code}");
				}
				else
				{
					additionalInfoText.Append(GetFeeTypeLabel(feeType));
					if (feeType != RateTypes.Antidumping)
					{
						additionalInfoText.Append($" Regime de Tributação: {taxRegime}");
					}
					additionalInfoText.Append($" Base de Cálculo: {fee.CF_BaseValue.Round(2)} {localCurrency?.Code}");
					additionalInfoText.Append($" Alíquota: {fee.CF_Rate.Round(2)}");
					additionalInfoText.AppendLine($" Valor Total: {fee.CF_ChargeAmount.Round(2)} {localCurrency?.Code}");
				}
			}
		}

		ZString GetFeeTypeLabel(ZString feeCode)
		{
			switch (feeCode)
			{
				case ChargeTypesList.Codes.DTY:
					return "II";
				case RateTypes.IPI:
					return "IPI";
				case RateTypes.PIS:
					return "PIS";
				case RateTypes.Cofins:
					return "COFINS";
				case RateTypes.Antidumping:
					return (NoResString)"Antidumping";
				case RateTypes.ICMS:
					return "ICMS";
				default:
					return ZString.Empty;
			}
		}

		ZString GetAdditionalTaxTypeLabel(ZString feeCode, ZString feeDescription, ZString tariffDescription)
		{
			switch (feeCode)
			{
				case AdditionalTaxTypeList.Codes.ExDutyTariff:
					return $"Imposto de Importação (II) Ex-Tarifario: {feeDescription} - {tariffDescription}";
				case AdditionalTaxTypeList.Codes.ExIPITariff:
					return $"IPI Ex-Tarifario: {feeDescription}";
				case AdditionalTaxTypeList.Codes.TariffAgreement:
					return $"Acordo Tarifario: {feeDescription} - {tariffDescription}";
				default:
					return ZString.Empty;
			}
		}
	}
}
