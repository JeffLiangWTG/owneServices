using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class ImportPlausiValidation : PlausiValidation
{
	public override void CheckCH0001(ZPropertyInfo targetPropertyInfo, IMessageSendingDeclaration declaration)
	{
		if (declaration.JE_TransportMode == TransportTypeList.Codes.Sea)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageCH0001);
		}
	}

	public override void CheckCH0005(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
		if (permit.CSI_Code == PermitCodes.SingleEPermit && PermitAuthorityCodes.IsApplicableForSingleEPermit(permit.CSI_IssuerType) && !permit.HasQuantityAndLineNumberItemDetails)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageCH0005);
		}
	}

	public override void CheckR121(ZPropertyInfo targetPropertyInfo, IDocAddress address, string nameOfAddress = null)
	{
		if (address != null && !IsCountryCHorLI(address.CountryCode))
		{
			nameOfAddress = nameOfAddress ?? targetPropertyInfo.HumanReadableName;
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.GetMessageR121(nameOfAddress));
		}
	}

	public override void CheckR123(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (!invoiceLine.JI_VATValueConfirmation && invoiceLine.JI_Procedure != ProcedureCodesEdec.ExemptFromDuty)
		{
			var customsValue = invoiceLine.JI_CustomsValue;
			var statisticalValue = invoiceLine.JI_Calc_StatisticalValue;
			if (customsValue < statisticalValue)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR123_1);
			}
			else if (customsValue > 3 * statisticalValue)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR123_2);
			}
		}
	}

	public override void CheckR124(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (!invoiceLine.JI_ZZF_NKTaxType.IsEmpty && invoiceLine.UniversalTariff != null)
		{
			var taxType = invoiceLine.JI_ZZF_NKTaxType;
			var vatCodeConfirmation = invoiceLine.JI_VATCodeConfirmation;
			var applicableVatCodes = invoiceLine.EffectiveVATApplicabilities;
			var taxTypeInApplicableVatCodes = applicableVatCodes.Any(x => x.ZX5_ZZF_NKTaxOrFeeCode == taxType);
			if (!taxTypeInApplicableVatCodes && !vatCodeConfirmation && !TaxCodes.IsAdditionalZeroPercentageVatCode(taxType) ||
				taxTypeInApplicableVatCodes && vatCodeConfirmation)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR124);
			}
		}
	}

	public override void CheckR127(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (!invoiceLine.JI_Weight.IsEmpty && !invoiceLine.JI_GrossMassConfirmation)
		{
			var netWeightKG = new ZWeight(invoiceLine.JI_NetWeight, invoiceLine.JI_NetWeightUQ).InKilogramsSafe;
			var grossWeightKG = new ZWeight(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ).InKilogramsSafe;
			if (netWeightKG <= 10m)
			{
				if (!netWeightKG.IsEmpty && (grossWeightKG < netWeightKG || grossWeightKG > 25 * netWeightKG))
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR127_1);
				}
			}
			else
			{
				if (grossWeightKG < netWeightKG || grossWeightKG > 2.5m * netWeightKG)
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR127_2);
				}
			}
		}
	}

	public override void CheckR130(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (!invoiceLine.JI_CustomsThirdUnitQty.IsEmpty && invoiceLine.JI_CustomsThirdQuantity.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.JobComInvoiceLine.AdditionalQuantityMissing);
		}
	}

	public override void CheckNS30021R132_NumberOfPacks(ZPropertyInfo targetPropertyInfo, InvoiceLinePackagePivot invoiceLinePackagePivot)
	{
		var isLoosePackaging = invoiceLinePackagePivot.Package.IsLoosePackaging;

		if (invoiceLinePackagePivot.CHC_NumberOfPacks > 0 && isLoosePackaging)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30021R132_PackQtyForLoosePackaging(ValidationMessages.Plausi.R132));
		}
	}

	public override void CheckNS30021R132_MarksAndNos(ZPropertyInfo targetPropertyInfo, Package package)
	{
		CheckNS30021R132_MarksAndNos(targetPropertyInfo, package, () => ValidationMessages.Plausi.MessageR132);
	}

	public override void CheckR133a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (!invoiceLine.DutyRateAdditionalCode.IsEmpty && !invoiceLine.DutyRateConfirmation && !invoiceLine.IsFixedRate)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR133a);
		}
	}

	public override void CheckR133b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.DutyRateAdditionalCode.IsEmpty && !invoiceLine.IsWithoutDuty && !invoiceLine.IsFixedRate)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR133b);
		}
	}

	public override void CheckR134abc(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		var ruleName = ValidationMessages.Plausi.R134;
		if (invoiceLine.Permits.Count == 0)
		{
			var universalTariff = invoiceLine.UniversalTariff;
			if (universalTariff != null && universalTariff.HasAttribute(UniversalReferenceConstants.TariffAttributes.HasOptionalPermit, UniversalReferenceConstants.TariffAttributes.Values._1))
			{
				var conditions = ConditionChecker.GetApplicableConditions(invoiceLine.Factory, universalTariff, invoiceLine.ControlConditionSelectionCriteria);
				var hasConditionTypePermitAuthority = conditions.Any(condition => condition.ConditionType.StartsWith(CusConditionType.PermitAuthorityPrefix));
				if (!hasConditionTypePermitAuthority && invoiceLine.JI_PermitObligation != UniversalReferenceConstants.PermitObligationCodes.NoPermit)
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.GetPermitObligationMustBeZeroMessage(ruleName));
				}

				if (hasConditionTypePermitAuthority && invoiceLine.JI_PermitObligation != UniversalReferenceConstants.PermitObligationCodes.NoPermitNeeded)
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.GetPermitObligationMustBeTwoMessage(ruleName));
				}
			}
			else if (invoiceLine.JI_PermitObligation != UniversalReferenceConstants.PermitObligationCodes.NoPermit)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.GetPermitObligationMustBeZeroMessage(ruleName));
			}
		}
	}

	public override void CheckR135(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.Permits.Count > 0)
		{
			if (invoiceLine.JI_PermitObligation != UniversalReferenceConstants.PermitObligationCodes.PermitNeeded)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR135);
			}
		}
	}

	public override void CheckR144abc(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.UniversalTariff is TariffView tariff && !invoiceLine.NonCustomsLaws.Any())
		{
			var hasOptionalNCL = tariff.HasAttribute(TariffAttributes.HasOptionalNCL, TariffAttributes.Values._1);
			var nonCustomsLawObligation = invoiceLine.JI_NonCustomsLawObligation;
			if (hasOptionalNCL)
			{
				var conditions = ConditionChecker.GetApplicableConditions(invoiceLine.Factory, tariff, invoiceLine.ControlConditionSelectionCriteria);
				var hasNclConditions = conditions.Any(c => c.CusConditionType.ZX2_ConditionType.StartsWith(CusConditionType.NonCustomsLawPrefix));
				if (!hasNclConditions && nonCustomsLawObligation != NonCustomsLawObligationCodes.NotPossible)
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR144abc_1);
				}
				if (hasNclConditions && nonCustomsLawObligation != NonCustomsLawObligationCodes.NotNeededAccordingDeclarant)
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR144abc_2);
				}
			}
			else if (nonCustomsLawObligation != NonCustomsLawObligationCodes.NotPossible)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR144abc_1);
			}
		}
	}

	public override void CheckR146(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
		if (!AdditionalTaxesTypes.ExcludedTaxTypes.Contains(tariffDetail.BZ_TaxType.ToString())
			&& tariffDetail.AssessmentCode == AssessmentCodeValues.Per100kgGrossMass
			&& tariffDetail.BZ_Qty1 != tariffDetail.InvoiceLine?.JI_CustomsQuantity)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR146);
		}
	}

	public override void CheckR147(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
		if (!AdditionalTaxesTypes.ExcludedTaxTypes.Contains(tariffDetail.BZ_TaxType.ToString())
			&& tariffDetail.AssessmentCode == AssessmentCodeValues.Per1000kgNetMass
			&& tariffDetail.BZ_Qty1 != tariffDetail.InvoiceLine?.JI_CustomsSecondQuantity)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR147);
		}
	}

	public override void CheckR148(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
		if (!AdditionalTaxesTypes.ExcludedTaxTypes.Contains(tariffDetail.BZ_TaxType.ToString())
			&& CusLineTariffDetail.R148List.Contains(tariffDetail.AssessmentCode.ToString())
			&& tariffDetail.BZ_Qty1 != tariffDetail.InvoiceLine?.JI_CustomsThirdQuantity)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR148);
		}
	}

	public override void CheckR156(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		var universalTariff = invoiceLine.UniversalTariff;
		if (universalTariff != null && universalTariff.HasAttribute(TariffAttributes.StorageType, TariffAttributes.Values.Yes))
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	public override void CheckR159(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.NetDuty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	public override void CheckR160(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine != null && invoiceLine.NetDuty && !invoiceLine.JI_WeightIncludingInnerPackage.IsEmpty)
		{
			var netWeight = new ZWeight(invoiceLine.JI_WeightIncludingInnerPackage, invoiceLine.JI_WeightIncludingInnerPackageUQ).InKilogramsSafe;
			if (netWeight < invoiceLine.JI_CustomsSecondQuantity || netWeight > invoiceLine.JI_CustomsQuantity)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR160);
			}
		}
	}

	public override void CheckR158(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_PrimaryPreference == PrimaryPreferenceCodes.PreferentialTariff && !invoiceLine.HasOriginDocument)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.GetMessageR158(GetOriginDocumentCodes()));
		}

		string GetOriginDocumentCodes() => RefCusCodeListLoader.GetOriginDocumentCodes(invoiceLine.Factory, invoiceLine.EffectiveAssessmentDate).CodesAsString;
	}

	public override void CheckR165(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.ReturnedGoodsVAT
				&& invoiceLine.EntryInstruction is CusEntryInstruction instruction && !instruction.AllInvoiceLinesAreReturnedGoods)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR165);
		}
	}

	public override void CheckR166c(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
		if (entryInstruction != null & reasonCodesForProofOfOrigin.Contains(entryInstruction.CEI_DeclarationReason) && entryInstruction.HasPreferentialTariffPreference)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR166c);
		}
	}

	public override void CheckR166c(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_PrimaryPreference == PrimaryPreferenceCodes.PreferentialTariff
			&& invoiceLine.EntryInstruction != null && reasonCodesForProofOfOrigin.Contains(invoiceLine.EntryInstruction.CEI_DeclarationReason))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR166c);
		}
	}

	public override void CheckR170ab(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_NonCustomsLawObligation != NonCustomsLawObligationCodes.Needed && invoiceLine.NonCustomsLaws.Any())
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR170ab);
		}
	}

	public override void CheckR173(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_CountryOfOrigin != Core.Constants.CountryCodes.Switzerland && invoiceLine.IsReturnedGoods)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR173);
		}
	}

	public override void CheckR174(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.IsReturnedGoods && !invoiceLine.JI_RateOverride)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR174);
		}
	}

	public override void CheckR176(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (!invoiceLine.IsCustomsRelief &&
				invoiceLine.UniversalTariff != null &&
				invoiceLine.UniversalTariff.HasAttribute(TariffAttributes.CustomsFavourHintCode, TariffAttributes.Values._2))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR176);
		}
	}

	public override void CheckR177(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.CustomsRelief && invoiceLine.CustomsFavourCode.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR177);
		}
	}

	public override void CheckR181(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.DutyFree && !invoiceLine.IsOverriddenRateZero)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR181);
		}
	}

	public override void CheckR183a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.ExemptFromDuty &&
			(!invoiceLine.JI_RateOverride ||
			!invoiceLine.JI_OverriddenRate.IsEmpty ||
			invoiceLine.JI_ZZF_NKTaxType != TaxCodes.ExemptVat ||
			invoiceLine.JI_Weight.IsEmpty ||
			invoiceLine.JI_Calc_StatisticalValue.IsEmpty ||
			!invoiceLine.JI_NonTradingGoods))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR183a);
		}
	}

	public override void CheckR183b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.ExemptFromDuty)
		{
			if (!invoiceLine.CustomsFavourCode.IsEmpty || !invoiceLine.StatisticalCode.IsEmpty ||
				invoiceLine.JI_WeightIncludingInnerPackage > 0 || invoiceLine.JI_TareSupplementPercentage > 0 ||
				!invoiceLine.JI_StorageType.IsEmpty)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR183b);
			}
		}
	}

	public override void CheckR191(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.RefinementTransportation
			&& invoiceLine.InAndOutwardProcessingDirection == InAndOutwardProcessingDirectionCodes.Active
			&& invoiceLine.InAndOutwardProcessingBillingType == InAndOutwardProcessingBillingTypesEdec.SuspensiveProcedure
			&& !invoiceLine.IsOverriddenRateZero)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR191);
		}
	}

	public override void CheckR193(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.RefinementTransportation && invoiceLine.JI_Tariff == Tariffs.NegligibleImportTariff)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR193);
		}
	}

	public override void CheckR198AndNP70167(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		CheckR198AndNP70167(targetPropertyInfo, invoiceLine, ValidationMessages.Plausi.MessageR198);
	}

	public override void CheckR208(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		if (inAndOutwardProcessing.CSI_SubType == InAndOutwardProcessingDirectionCodes.Active && inAndOutwardProcessing.CSI_Procedure != InAndOutwardProcessingProcessTypesEdec.DueProcedure && !inAndOutwardProcessing.Repair)
		{
			var invoiceLine = inAndOutwardProcessing.Parent;
			if (invoiceLine.JI_Procedure == ProcedureCodesEdec.RefinementTransportation && invoiceLine.JI_ZZF_NKTaxType != TaxCodes.ExemptVat && invoiceLine.JI_NonTradingGoods)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR208);
			}
		}
	}

	public override void CheckR219(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_ZZF_NKTaxType == TaxCodes.ProcessingTraffic && (invoiceLine.ProcedureCode != ProcedureCodesEdec.RefinementTransportation || invoiceLine.InAndOutwardProcessingDirection != InAndOutwardProcessingDirectionCodes.Active || invoiceLine.InAndOutwardProcessingRefinementType != InAndOutwardProcessingRefinementTypesEdec.ContractProcessing))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR219);
		}
	}

	public override void CheckR224(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.NetDuty)
		{
			var rateFormula = invoiceLine.RateFormulaWithFallbackToNormalTariff;
			if (!rateFormula.IsEmpty && !rateFormula.Contains(FormulaPlaceholder.GrossWeightUOMPlaceHolder))
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR224);
			}
		}
	}

	readonly List<string> reasonCodesForProofOfOrigin = new List<string>()
	{
		DeclarationReason.ProofOfOriginEUCountries,
		DeclarationReason.ProofOfOriginEFTACountries,
		DeclarationReason.ProofOfOriginFreeTradeAgreementCountries,
		DeclarationReason.ProofOfOriginDevelopingCountries
	};

	public override void CheckR167c(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
		if (!declaration.JE_RL_NKPortOfLoading.IsEmpty && !declaration.JE_DispatchCountryConfirmation)
		{
			var dispatchCountryCode = declaration.DispatchCountryCode;
			if (declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(line => CheckInvoiceLine(line, dispatchCountryCode)))
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR167c);
			}
		}

		bool CheckInvoiceLine(JobComInvoiceLine invoiceLine, ZString dispatchCountry)
		{
			return invoiceLine.JI_CountryOfOrigin != dispatchCountry
				&& invoiceLine.JI_PrimaryPreference == PrimaryPreferenceCodes.PreferentialTariff
				&& invoiceLine.IsIndustrialTariff
				&& invoiceLine.OriginIsDirectTransportationCountry;
		}
	}

	public override void CheckR168(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
		var importer = declaration.Importer;
		if (importer != null && importer.GetVATNumber().IsEmpty && declaration.IsRelocationProcedureOrProcessingTraffic)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR168);
		}
	}

	public override void CheckR247(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_ZZF_NKTaxType == TaxCodes.DeferredTaxation && !invoiceLine.IsOverriddenRateZero)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR247);
		}
	}

	public override void CheckR256(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		var cigarsTariff = TariffNumbers.IsCigarsTariffCode(invoiceLine.TariffNumber) && invoiceLine.IsStatisticalCodeOther;

		var industrialManufactureTariff = TariffNumbers.IsIndustrialManufactureTariffCode(invoiceLine.TariffNumber) && TariffStatisticalCodes.IsSnuffTobaccoStatisticalCode(invoiceLine.StatisticalCode) && invoiceLine.JI_CustomsSecondQuantity > 2.5m;

		var chewingRollingOtherTobacco = TariffNumbers.IsChewingRollingOtherTobacco(invoiceLine.TariffNumber) && invoiceLine.JI_CustomsSecondQuantity > 5m;

		if ((cigarsTariff || industrialManufactureTariff || chewingRollingOtherTobacco) && !invoiceLine.HasReversPermit)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR256);
		}
	}

	ZString[] TariffNumbersForR257 => tariffNumbersForR257 ??= new ZString[] { TariffNumbers.CigarCherootsCigarillosContainingTobacco, TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffNumbers.CigarCherootsCigarillosOthers, TariffNumbers.WaterPipeTobaccoSpecifiedInSubheading, TariffNumbers.SmokingTobaccoOther, TariffNumbers.ChewingTobaccoRollTobaccoAndSnuff, TariffNumbers.OtherManufacturedTobaccoOtherOther };
	ZString[] tariffNumbersForR257;

	public override void CheckR249a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_CustomsValue > SwissCustomsConstants.Limits.MaxTobaccoQuantityBasedTaxationCustomsValue
			&& invoiceLine.StatisticalCode == TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF
			&& TariffNumbers.IsTariffNumberForQuantityBasedTaxation(invoiceLine.TariffNumber))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR249a);
		}
	}

	public override void CheckR249b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_CustomsQuantity > SwissCustomsConstants.Limits.MaxTobaccoQuantityBasedTaxationCustomsQuantity
			&& invoiceLine.StatisticalCode == TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF
			&& TariffNumbers.IsTariffNumberForQuantityBasedTaxation(invoiceLine.TariffNumber))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR249b);
		}
	}

	public override void CheckR249c(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
		if (permit.CSI_Code == PermitCodes.ReversTobacco && permit.CSI_IssuerType == PermitAuthorityCodes.STB
			&& permit.Parent.StatisticalCode == TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF
			&& TariffNumbers.IsTariffNumberForQuantityBasedTaxation(permit.Parent.TariffNumber))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR249c);
		}
	}

	public override void CheckR249d(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if ((!invoiceLine.JI_RateOverride || (invoiceLine.JI_RateOverride && invoiceLine.JI_OverriddenRate > ZDecimal.Zero))
			&& invoiceLine.StatisticalCode == TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF
			&& TariffNumbers.IsTariffNumberForQuantityBasedTaxation(invoiceLine.TariffNumber))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR249d);
		}
	}

	public override void CheckR249e(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure != ProcedureCodesEdec.Tobacco
			&& invoiceLine.StatisticalCode == TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF
			&& TariffNumbers.IsTariffNumberForQuantityBasedTaxation(invoiceLine.TariffNumber))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR249e);
		}
	}

	public override void CheckR257(Tobacco tobacco)
	{
		tobacco.ClearRowNotificationsContaining(ValidationMessages.Plausi.MessageR257);
		var invoiceLine = tobacco.Parent;
		if (TariffNumbersForR257.Contains(invoiceLine.TariffNumber)
			&& invoiceLine.StatisticalCode == TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF)
		{
			tobacco.AddRowMessageError(ValidationMessages.Plausi.MessageR257);
		}
	}

	public override void CheckR261IssuerType(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		if (inAndOutwardProcessing.CSI_IssuerType != InAndOutwardProcessingBillingTypesEdec.SuspensiveProcedure)
		{
			CheckR261(targetPropertyInfo, inAndOutwardProcessing.Parent, ValidationMessages.Plausi.MessageR261_1);
		}
	}

	public override void CheckR261NonTradingGoods(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_NonTradingGoods)
		{
			CheckR261(targetPropertyInfo, invoiceLine, ValidationMessages.Plausi.MessageR261_2);
		}
	}

	void CheckR261(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine, string plausiMessage)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.RefinementTransportation && invoiceLine.InAndOutwardProcessingDirection == InAndOutwardProcessingDirectionCodes.Passive && invoiceLine.InAndOutwardProcessingProcessType == InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure && !invoiceLine.InAndOutwardProcessingRepair)
		{
			targetPropertyInfo.AddMessageError(plausiMessage);
		}
	}

	public override void CheckR262(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
		if (tariffDetail.BZ_TaxType == AdditionalTaxesTypes.Beer
			&& tariffDetail.BZ_Qty1 != tariffDetail.InvoiceLine?.JI_CustomsThirdQuantity / 100)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR262);
		}
	}

	public override void CheckR267(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Weight.IsEmpty && !invoiceLine.JI_GrossMassConfirmation && invoiceLine.RateFormulaWithFallbackToNormalTariff.Contains(FormulaPlaceholder.GrossWeightUOMPlaceHolder))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR267);
		}
	}

	public override void CheckR268a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Calc_StatisticalValue <= 0 && invoiceLine.JI_CustomsValue <= 0)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR268a);
		}
	}

	public override void CheckR227a(ZPropertyInfo targetPropertyInfo, SupportingDocument supportingDocument)
	{
		if (supportingDocument.Lookups.ValidOriginDocumentCodes.GetAllCodes().ToList().Contains(supportingDocument.CSI_Code))
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	public override void CheckR268b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Weight.IsEmpty && (invoiceLine.JI_NetWeight.IsEmpty || invoiceLine.JI_CustomsThirdQuantity.IsEmpty))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR268b);
		}
	}

	public override void CheckR271(ZPropertyInfo targetPropertyInfo)
	{
		MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
	}

	public override void CheckR275(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetPropertyInfo);
	}

	public override void CheckR276(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
	}

	public override void CheckR277(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_ZZF_NKTaxType == TaxCodes.RelocationProcedure && !invoiceLine.HasFederalTaxAdministrationCommitmentPermit)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR277);
		}
	}

	public override void CheckR288(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
		if (entryInstruction.CEI_Style == DeclarationTypeCodes.Provisional && entryInstruction.CEI_DeclarationReason == DeclarationReason.ProofOfOriginDevelopingCountries)
		{
			if (!RefCusTradeGroupLoader.IsAnyCountryPartOfDevelopingCountries(entryInstruction.Factory, entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.JI_CountryOfOrigin).Distinct(), entryInstruction.DateOfValuation))
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR288);
			}
		}
	}

	public override void CheckR313(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
		if (PermitCodes.IsEPermit(permit.CSI_Code) && PermitAuthorityCodes.IsNotApplicableForEPermitImport(permit.CSI_IssuerType))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR313);
		}
	}

	public override void CheckR314(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
		if (permit.CSI_Code == PermitCodes.SingleEPermit && PermitAuthorityCodes.IsNotApplicableForSingleEPermit(permit.CSI_IssuerType))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR314);
		}
	}

	public override void CheckR315(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
		if (permit.CSI_Code == PermitCodes.GeneralEPermit && PermitAuthorityCodes.IsNotApplicableForGeneralEPermitImport(permit.CSI_IssuerType))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR315);
		}
	}

	public override void CheckR316(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
		if (!PermitCodes.IsEPermit(permit.CSI_Code) && PermitAuthorityCodes.IsNotApplicableForNonEPermitImport(permit.CSI_IssuerType))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR316);
		}
	}

	public override void CheckR326R327(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
		if (tariffDetail.IsAdditionalTaxApplied)
		{
			switch (tariffDetail.BZ_TaxType)
			{
				case AdditionalTaxesTypes.VeterinaryInspection:
					AddMessageErrorIfAdditionalTaxeIsApplied(tariffDetail.InvoiceLine?.AdditionalTaxes, AdditionalTaxesTypes.CitesFlora, targetPropertyInfo, () => ValidationMessages.Plausi.MessageR327);
					break;
				case AdditionalTaxesTypes.CitesFauna:
					AddMessageErrorIfAdditionalTaxeIsApplied(tariffDetail.InvoiceLine?.AdditionalTaxes, AdditionalTaxesTypes.CitesFlora, targetPropertyInfo, () => ValidationMessages.Plausi.MessageR326);
					break;
				case AdditionalTaxesTypes.CitesFlora:
					AddMessageErrorIfAdditionalTaxeIsApplied(tariffDetail.InvoiceLine?.AdditionalTaxes, AdditionalTaxesTypes.CitesFauna, targetPropertyInfo, () => ValidationMessages.Plausi.MessageR326);
					AddMessageErrorIfAdditionalTaxeIsApplied(tariffDetail.InvoiceLine?.AdditionalTaxes, AdditionalTaxesTypes.VeterinaryInspection, targetPropertyInfo, () => ValidationMessages.Plausi.MessageR327);
					break;
			}
		}
	}

	void AddMessageErrorIfAdditionalTaxeIsApplied(CusLineTariffDetailCollection additionalTaxes, string additionalTaxToSearchFor, ZPropertyInfo targetPropertyInfo, Func<string> messageError)
	{
		if (additionalTaxes?.Cast<CusLineTariffDetail>()?.Any(t => t.BZ_TaxType == additionalTaxToSearchFor && t.IsAdditionalTaxApplied) ?? false)
		{
			targetPropertyInfo.AddMessageError(messageError());
		}
	}

	public override void CheckR330(CusLineTariffDetail lineTariffDetail)
	{
		lineTariffDetail.ClearRowNotificationsContaining(ValidationMessages.Plausi.MessageR330);

		switch (lineTariffDetail.BZ_TaxType)
		{
			case AdditionalTaxesTypes.CitesFlora:
			case AdditionalTaxesTypes.CitesFauna:
				if (lineTariffDetail.IsAdditionalTaxApplied)
				{
					var specialMentions = lineTariffDetail.InvoiceLine?.InvoiceHeader?.SpecialMentions;
					if (specialMentions.HasValue && !AdditionTaxesControlOffices.CitesControlOffices.Any(c => specialMentions.Value.Contains(c)))
					{
						lineTariffDetail.AddRowMessageError(ValidationMessages.Plausi.MessageR330);
					}
				}
				break;
		}
	}

	public override void CheckR331abcd(ZPropertyInfo targetPropertyInfo, Tobacco tobacco)
	{
		var invoiceLine = tobacco.Parent;

		if (invoiceLine.TariffNumber == TariffNumbers.CigarCherootsCigarillosContainingTobacco
			&& invoiceLine.IsStatisticalCodeOther
			&& (tobacco.CSI_Code == TobaccoMainGroupCodes.Cigarettes || tobacco.CSI_Code == TobaccoMainGroupCodes.CutTobacco))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR331a);
		}
		else if (TariffNumbers.IsTariffNumberForCigarettes(invoiceLine.TariffNumber)
			&& invoiceLine.IsStatisticalCodeOther
			&& tobacco.CSI_Code == TobaccoMainGroupCodes.CutTobacco)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR331b);
		}
		else if (TariffNumbers.IsTariffNumberForOtherTobaccoProducts(invoiceLine.TariffNumber)
			&& invoiceLine.IsStatisticalCodeOther
			&& (tobacco.CSI_Code == TobaccoMainGroupCodes.Cigars || tobacco.CSI_Code == TobaccoMainGroupCodes.Cigarettes))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR331c);
		}
		else if (invoiceLine.TariffNumber == TariffNumbers.OtherManufacturedTobaccoOtherOther
			&& invoiceLine.IsStatisticalCodeOther
			&& tobacco.CSI_Code == TobaccoMainGroupCodes.Cigarettes)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR331d);
		}
	}
	public override void CheckR338acd(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.IsStatisticalCodeOther)
		{
			var cutTobacco = invoiceLine.Tobaccos.Where(tobacco => tobacco.CSI_Code == TobaccoMainGroupCodes.CutTobacco).FirstOrDefault();
			ZDecimal? quantity_450 = ZDecimal.Zero;
			ZDecimal? quantity_465 = ZDecimal.Zero;
			if (invoiceLine.TariffNumber == TariffNumbers.CigarettesContainingTobaccoMoreThan
				|| (invoiceLine.TariffNumber == TariffNumbers.CigarettesContainingTobaccoNotMoreThan
					&& invoiceLine.Tobaccos.Where(tobacco => tobacco.CSI_Code == TobaccoMainGroupCodes.Cigarettes).Any()))
			{
				quantity_450 = invoiceLine.AdditionalTaxes.Where(additionalTax => AdditionalTaxesTariffs.IsTobaccoTariff450002Or450202(additionalTax.BZ_Tariff)).FirstOrDefault()?.BZ_Qty1;
				quantity_465 = invoiceLine.AdditionalTaxes.Where(additionalTax => AdditionalTaxesTariffs.IsSOTATariff465001Or465201(additionalTax.BZ_Tariff)).FirstOrDefault()?.BZ_Qty1;
			}
			else if (invoiceLine.TariffNumber == TariffNumbers.SmokingTobaccoOther
					&& (cutTobacco?.CSI_SubType ?? ZString.Empty) == TobaccoSubGroupCodes._02)
			{
				quantity_450 = invoiceLine.AdditionalTaxes.Where(additionalTax => AdditionalTaxesTariffs.IsTobaccoTariff450001Or450201(additionalTax.BZ_Tariff)).FirstOrDefault()?.BZ_Qty1;
				quantity_465 = invoiceLine.AdditionalTaxes.Where(additionalTax => AdditionalTaxesTariffs.IsSOTATariff465002Or465202(additionalTax.BZ_Tariff)).FirstOrDefault()?.BZ_Qty1;
			}
			if (quantity_450 != quantity_465)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR338);
			}
		}
	}

	public override void CheckR348(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
		if (declaration.IsImportDomicile && declaration.Representative == null)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR348);
		}
	}

	public override void CheckR359(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.RepairTransportation)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR359);
		}
	}

	public override void CheckR292(ZPropertyInfo targetPropertyInfo, DeclarationMessageSendingObject sendingObject)
	{
		if (sendingObject.MessageType == PassarMessageTypeList.Codes.NI013 && sendingObject.VOCReason == CorrectionReason.ConvertProvisoryToDefinitiveDeclaration && sendingObject.DeclarationType == DeclarationTypeCodes.Provisional)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR292);
		}
	}

	public override void CheckR162R201(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if ((invoiceLine.ProcedureCode == ProcedureCodesEdec.NormalDuty ||
				 invoiceLine.ProcedureCode == ProcedureCodesEdec.Tobacco) &&
				(invoiceLine.JI_Tariff == Tariffs.NegligibleImportTariff ||
				 !invoiceLine.CustomsFavourCode.IsEmpty))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR162R201);
		}
	}

	public override void CheckR175R179(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if ((invoiceLine.ProcedureCode == ProcedureCodesEdec.CustomsRelief ||
				 invoiceLine.ProcedureCode == ProcedureCodesEdec.ReturnedGoods ||
				 invoiceLine.ProcedureCode == ProcedureCodesEdec.ReturnedGoodsVAT) &&
				invoiceLine.JI_Tariff == Tariffs.NegligibleImportTariff)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR175R179);
		}
	}

	public override void CheckR356(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.ProcedureCode == ProcedureCodesEdec.DutyFree && invoiceLine.InAndOutwardProcessingRepair &&
			(invoiceLine.JI_Tariff == Tariffs.NegligibleImportTariff || !invoiceLine.CustomsFavourCode.IsEmpty))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR356);
		}
	}

	public override void CheckR182(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.ProcedureCode == ProcedureCodesEdec.DutyFree && !invoiceLine.InAndOutwardProcessingRepair && !invoiceLine.CustomsFavourCode.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR182);
		}
	}

	public override void CheckR285(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Tariff == Tariffs.NegligibleImportTariff &&
				invoiceLine.ProcedureCode == ProcedureCodesEdec.DutyFree &&
				!invoiceLine.JI_NonTradingGoods)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR285);
		}
	}

	public override void CheckR325(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_NonTradingGoods && invoiceLine.IsReturnedGoods && !invoiceLine.IsSamnaunFreeZoneTraffic)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR325);
		}
	}

	public override void CheckR290(ZPropertyInfo targetPropertyInfo, ICusSupportingInfoParent parent)
	{
		if (((ISupportingDocumentParent)parent).IsGSPCertificateRequired
			&& !RefCusCodeListLoader.IsAnyGSPCertificate(parent.Factory, ((ISupportingDocumentParent)parent).SupportingDocumentsIncludingInherited.Select(s => s.CSI_Code), parent.JobDeclaration.DateOfValuation))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.GetMessageR290_1(GetGSPCertificateCodes()));
		}

		string GetGSPCertificateCodes() => RefCusCodeListLoader.GetGSPCertificateCodes(parent.Factory, parent.JobDeclaration.DateOfValuation).CodesAsString;
	}

	public override void CheckR290(ZPropertyInfo targetPropertyInfo, SupportingDocument supportingDocument)
	{
		if (supportingDocument.IsGSPCertificate)
		{
			var certificateRequired = ((ISupportingDocumentParent)supportingDocument.Parent)?.IsGSPCertificateRequired ?? false;
			if (!certificateRequired)
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.GetMessageR290_2(GetGSPCertificateCodes()));
			}
		}

		string GetGSPCertificateCodes() => RefCusCodeListLoader.GetGSPCertificateCodes(supportingDocument.Factory, supportingDocument.Parent.DateOfValuation).CodesAsString;
	}

	public override void CheckR361(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.RefinementTransportation
			&& invoiceLine.InAndOutwardProcessingDirection == InAndOutwardProcessingDirectionCodes.Active
			&& invoiceLine.InAndOutwardProcessingProcessType == InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR361);
		}
	}

	public override void CheckR190(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		var invoiceLine = inAndOutwardProcessing.Parent;
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.RefinementTransportation && inAndOutwardProcessing.CSI_Procedure == InAndOutwardProcessingProcessTypesEdec.DueProcedure && !hasPermitIssuedByAuth98())
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR190);
		}
		bool hasPermitIssuedByAuth98() => invoiceLine.Permits.Cast<Permit>().Any(p => p.CSI_IssuerType == PermitAuthorityCodes.FOCBS_Other);
	}

	public override void CheckR229(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		var invoiceLine = inAndOutwardProcessing.Parent;
		if (invoiceLine.JI_Procedure == ProcedureCodesEdec.RefinementTransportation && inAndOutwardProcessing.CSI_Procedure == InAndOutwardProcessingProcessTypesEdec.DueProcedure && !invoiceLine.NotifyCustomsOffices.Any())
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR229);
		}
	}

	public override void CheckR353(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.IsReturnedGoods && invoiceLine.InAndOutwardProcessingRepair)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR353);
		}
	}

	public override void CheckR358(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		if (inAndOutwardProcessing.Parent.JI_Procedure == ProcedureCodesEdec.RefinementTransportation && inAndOutwardProcessing.CSI_SubType == InAndOutwardProcessingDirectionCodes.Active && inAndOutwardProcessing.CSI_Description.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR358);
		}
	}

	public override void CheckR141(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		switch (invoiceLine.JI_StorageType)
		{
			case StorageCodes.ImportHomeWithProvisionalTax:
			case StorageCodes.TransportToApprovedWarehouse:
			case StorageCodes.TransportToStockholdingWarehouse:
			case StorageCodes.ImportWithSpecialDispatchNode:
				if (!invoiceLine.Permits.Cast<Permit>().Any(x => x.CSI_Code == PermitCodes.PeriodicTax && x.CSI_IssuerType == PermitAuthorityCodes.FOCBS_MOT))
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR141);
				}
				break;
		}
	}

	public override void CheckR137(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
		if (cusLineTariffDetail.BZ_Tariff == AdditionalTaxesTariffs.Tariff280200 &&
			!cusLineTariffDetail.InvoiceLine.Permits.Cast<Permit>().Any(p => p.CSI_IssuerType == PermitAuthorityCodes.AAT && p.CSI_Code == PermitCodes.Commitment))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR137);
		}
	}

	public override void CheckR138(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
		if (cusLineTariffDetail.BZ_Tariff == AdditionalTaxesTariffs.Tariff700002 &&
			!cusLineTariffDetail.InvoiceLine.Permits.Cast<Permit>().Any(p => p.CSI_IssuerType == PermitAuthorityCodes.FOCBS_COV && p.CSI_Code == PermitCodes.Commitment))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR138);
		}
	}

	public override void CheckR149a(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
		if (!cusLineTariffDetail.BZ_UQ1.IsEmpty && cusLineTariffDetail.BZ_Qty1 <= 0)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR149a);
		}
	}

	public override void CheckR220a(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
		if (cusLineTariffDetail.IsAdditionalTaxRateFormulaEmpty() && cusLineTariffDetail.BZ_ManualRate <= 0)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR220a);
		}
	}

	public override void CheckR149b(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
		if (cusLineTariffDetail.BZ_TaxType == AdditionalTaxesTypes.Spirits && cusLineTariffDetail.IsAdditionalTaxApplied && cusLineTariffDetail.BZ_AlcoholPercentage <= 0)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR149b);
		}
	}

	public override void CheckR142(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		switch (invoiceLine.JI_StorageType)
		{
			case StorageCodes.ImportHomeWithFinalTax:
			case StorageCodes.ImportHomeWithProvisionalTax:
				if (!invoiceLine.Permits.Cast<Permit>().Any(x => x.CSI_Code == PermitCodes.ObligationMineralOilTax && x.CSI_IssuerType == PermitAuthorityCodes.FOCBS_MOT)
					&& (invoiceLine.UniversalTariff?.HasAttribute(TariffAttributes.CustomsFavourHintCode, TariffAttributes.Values._5) ?? false))
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR142);
				}
				break;
		}
	}

	public override void CheckR205(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		if (inAndOutwardProcessing.Parent?.JI_Procedure.ToString() == ProcedureCodesEdec.RefinementTransportation)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	public override void CheckR206(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		if (inAndOutwardProcessing.Repair)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	public override void CheckR145a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		switch (invoiceLine.JI_StorageType)
		{
			case StorageCodes.ImportHomeWithFinalTax:
			case StorageCodes.ImportHomeWithProvisionalTax:
				if (!invoiceLine.AdditionalTaxes.Cast<CusLineTariffDetail>().Any(x => CheckExistsAdditionalTax(x)))
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR145a);
				}
				break;
		}

		bool CheckExistsAdditionalTax(CusLineTariffDetail additionalTax)
		{
			if (additionalTax.IsAdditionalTaxApplied)
			{
				var additionalTaxType = additionalTax.BZ_TaxType;
				return (AdditionalTaxesTypes.IsAdditionalTaxTypeForMineralOilProducts(additionalTaxType)
					|| (AdditionalTaxesTypes.MineralOilGasoline < additionalTaxType && additionalTaxType < AdditionalTaxesTypes.MineralOilFuelsAndOthers));
			}
			return false;
		}
	}

	public override void CheckR145b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		switch (invoiceLine.JI_StorageType)
		{
			case StorageCodes.TransportToApprovedWarehouse:
			case StorageCodes.TransportToStockholdingWarehouse:
			case StorageCodes.ImportWithSpecialDispatchNode:
				if (invoiceLine.AdditionalTaxes.Cast<CusLineTariffDetail>().Any(x => CheckExistingAdditionalTax(x)))
				{
					targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR145b);
				}
				break;
		}

		bool CheckExistingAdditionalTax(CusLineTariffDetail additionalTax)
		{
			if (additionalTax.IsAdditionalTaxApplied)
			{
				var additionalTaxType = additionalTax.BZ_TaxType;
				return (AdditionalTaxesTypes.IsAdditionalTaxTypeForFuels(additionalTaxType)
					 || (AdditionalTaxesTypes.MineralOilGasoline < additionalTaxType && additionalTaxType < AdditionalTaxesTypes.MineralOilFuelsAndOthers)
					 || (AdditionalTaxesTypes.CO2HeatingOil < additionalTaxType && additionalTaxType < AdditionalTaxesTypes.CO2CoalAndCoke));
			}
			return false;
		}
	}

	public override void CheckR336(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.TariffNumber == UniversalReferenceConstants.TariffNumbers.SmokingTobaccoOther
			&& invoiceLine.IsStatisticalCodeOther
			&& invoiceLine.HasAnyTobaccoSubGroup02or03
			&& (!invoiceLine.HasAnySOTAAdditionalTax || !invoiceLine.HasAnyPreventionAdditionalTax))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR336);
		}
	}

	public override void CheckR334(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
		if (tariffDetail.InvoiceLine.IsStatisticalCodeOther && tariffDetail.IsQuantityBasedAdditionalTax
			&& tariffDetail.BZ_Qty1 != tariffDetail.InvoiceLine.JI_CustomsThirdQuantity
			&& TariffNumbers.IsTariffNumberForNumOfElementsBasedTaxation(tariffDetail.InvoiceLine.TariffNumber))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR334a);
		}
		else if (tariffDetail.InvoiceLine.IsStatisticalCodeOther && tariffDetail.IsQuantityBasedAdditionalTax
			&& tariffDetail.BZ_Qty1 != tariffDetail.InvoiceLine.JI_CustomsSecondQuantity
			&& TariffNumbers.IsTariffNumberForWeightBasedTaxation(tariffDetail.InvoiceLine.TariffNumber))
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR334b);
		}
	}

	public override void CheckR339(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
		if (TariffNumbers.IsTariffNumberForQuantityBasedTaxation(tariffDetail.InvoiceLine.TariffNumber) && tariffDetail.InvoiceLine.StatisticalCode == TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF
			&& tariffDetail.IsQuantityBasedAdditionalTax
			&& AdditionalTaxesTariffs.IsTariffKeyForGrossMassBasedTaxation(tariffDetail.TaxTariffKey)
			&& tariffDetail.BZ_Qty1 != tariffDetail.InvoiceLine.JI_CustomsQuantity)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR339);
		}
	}

	public override void CheckNP70020(ZPropertyInfo targetPropertyInfo, AutoCusHouseContPackInvoiceLinePivot packInvLinePivot)
	{
		base.CheckNP70020(targetPropertyInfo, packInvLinePivot, ValidationMessages.InvoiceLinePackage.TotalLineQtyIsZero);
	}

	public override void CheckR128(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_NetWeight.IsEmpty)
		{
			var universalTariff = invoiceLine.UniversalTariff;
			if (universalTariff != null && universalTariff.HasAttribute(UniversalReferenceConstants.TariffAttributes.QuantityCode1, UniversalReferenceConstants.TariffAttributes.Values._1))
			{
				targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageNotEntered(targetPropertyInfo.HumanReadableName, ValidationMessages.Plausi.R128));
			}
		}
	}

	public override void CheckR337(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
		var invoiceLine = tariffDetail.InvoiceLine;
		var tobacco = invoiceLine.Tobaccos.FirstOrDefault();

		if (invoiceLine.TariffNumber.StartsWith(TariffChapters.Tobaccos) && invoiceLine.IsStatisticalCodeOther)
		{
			CheckR337a(targetPropertyInfo, tariffDetail, invoiceLine);
			if (tobacco != null)
			{
				CheckR337c(targetPropertyInfo, tariffDetail, invoiceLine, tobacco);
				CheckR337d(targetPropertyInfo, tariffDetail, invoiceLine, tobacco);
			}
		}
	}

	void CheckR337a(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail, JobComInvoiceLine invoiceLine)
	{
		if (!targetPropertyInfo.HasMessageError(ValidationMessages.Plausi.MessageR337)
			&& invoiceLine.TariffNumber == TariffNumbers.CigarettesContainingTobaccoMoreThan
			&& (AdditionalTaxesTariffs.IsTobaccoTariff450002Or450202(tariffDetail.BZ_Tariff) || AdditionalTaxesTariffs.IsTobaccoTariff470001Or470201(tariffDetail.BZ_Tariff))
			&& invoiceLine.TariffTobaccoTaxQuantity != invoiceLine.TariffTobaccoPreventionFundQuantity)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR337);
		}
	}

	void CheckR337c(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail, JobComInvoiceLine invoiceLine, Tobacco tobacco)
	{
		if (!targetPropertyInfo.HasMessageError(ValidationMessages.Plausi.MessageR337)
			&& invoiceLine.TariffNumber == TariffNumbers.SmokingTobaccoOther
			&& tobacco.CSI_Code == TobaccoMainGroupCodes.CutTobacco
			&& tobacco.CSI_SubType == TobaccoSubGroupCodes._02
			&& (AdditionalTaxesTariffs.IsTobaccoTariff450001Or450201(tariffDetail.BZ_Tariff) || AdditionalTaxesTariffs.IsTobaccoTariff470002Or470202(tariffDetail.BZ_Tariff))
			&& invoiceLine.TariffTobaccoTaxQuantity != invoiceLine.TariffTobaccoPreventionFundQuantity)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR337);
		}
	}

	void CheckR337d(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail, JobComInvoiceLine invoiceLine, Tobacco tobacco)
	{
		if (!targetPropertyInfo.HasMessageError(ValidationMessages.Plausi.MessageR337)
			&& invoiceLine.TariffNumber == TariffNumbers.CigarettesContainingTobaccoNotMoreThan
			&& tobacco.CSI_Code == TobaccoMainGroupCodes.Cigarettes
			&& (AdditionalTaxesTariffs.IsTobaccoTariff450002Or450202(tariffDetail.BZ_Tariff) || AdditionalTaxesTariffs.IsTobaccoTariff470001Or470201(tariffDetail.BZ_Tariff))
			&& invoiceLine.TariffTobaccoTaxQuantity != invoiceLine.TariffTobaccoPreventionFundQuantity)
		{
			targetPropertyInfo.AddMessageError(ValidationMessages.Plausi.MessageR337);
		}
	}
}
