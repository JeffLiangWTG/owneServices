using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DutyCalculatorStrategy : EU.Business.Declaration.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration Declaration => (JobDeclaration)base.declaration;

		public override void CalculateDuties()
		{
			base.CalculateDuties();
			new HarbourFeeEntryHeaderCalculationManager(Declaration.Factory).Calculate(Declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
		}

		protected override void AddNewEntryLineFee(EU.Business.Declaration.CusEntryLine entryLine, ZString rateCode, ZString overrideReasonCode, IDutyCalculationIntermediateResult calculatedFee, RateView rateForCalculation = null)
		{
			if (overrideReasonCode.Equals(RateOverrideReasonList.Codes.Precalcule) || (calculatedFee?.AdjustedRate != ZDecimal.Zero))
			{
				base.AddNewEntryLineFee(entryLine, rateCode, overrideReasonCode, calculatedFee, rateForCalculation);
			}
		}

		protected override void CalculateEntryLineVatFee(EU.Business.Declaration.CusEntryLine entryLine)
		{
			if (Declaration.IsImport)
			{
				base.CalculateEntryLineVatFee(entryLine);
			}
		}

		List<ZString> CodeTypeTariffList(EU.Business.Declaration.CusEntryLine entryLine, ZString codeType)
		{
			return ZZRefCusCodeListCombined.Loader.Load(entryLine.Factory, Core.Constants.CountryCodes.France, codeType, ZDateTime.Today)?.Select(x => x.ZZD_Code).ToList() ?? new List<ZString>();
		}

		protected override IUniversalDutyCalculator GetNewEntryLineDutyCalculator(EU.Business.Declaration.CusEntryLine entryLine)
		{
			return new FREntryLineDutyCalculator((CusEntryLine)entryLine, RateCalculationVisitorMode);
		}

		protected override void SetNationalType(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee, RateView rateForCalculation)
		{
			if (entryLineFee.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)
			{
				var vatApplicability = entryLine.RandomLine.GetEffectiveVATApplicabilities().FirstOrDefault(x => x.ZX5_ZZF_NKTaxOrFeeCode == entryLine.RandomLine.JI_ZZF_NKTaxType);
				if (vatApplicability != null)
				{
					entryLineFee.NationalFeeTypeCode = vatApplicability.ZX5_VATCategory.Left(entryLineFee.NationalFeeTypeCodeInfo.MaxLength);
				}
			}
			else if (rateForCalculation.ZZ2_ZZR_RateTypeCode == Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty)
			{
				if (entryLineFee.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.A00)
				{
					if (CodeTypeTariffList(entryLine, UniversalReferenceConstants.RefCusRateCodes.U167).Contains(entryLine.Tariff))
					{
						entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.U167;
					}
					else if (CodeTypeTariffList(entryLine, UniversalReferenceConstants.RefCusRateCodes.U395).Contains(entryLine.Tariff))
					{
						entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.U395;
					}
					else
					{
						entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.U165;
					}
				}
				else if (entryLineFee.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.A20)
				{
					if (CodeTypeTariffList(entryLine, UniversalReferenceConstants.RefCusRateCodes.U397).Contains(entryLine.Tariff))
					{
						entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.U397;
					}
					else if (CodeTypeTariffList(entryLine, UniversalReferenceConstants.RefCusRateCodes.U437).Contains(entryLine.Tariff))
					{
						entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.U437;
					}
					else
					{
						entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.U425;
					}
				}
			}
			else if (rateForCalculation.ZZ2_ZZR_RateTypeCode == Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty)
			{
				if (entryLineFee.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.A30 || entryLineFee.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.A35)
				{
					entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.U235;
				}
			}
			else if (rateForCalculation.ZZ2_ZZR_RateTypeCode == Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty)
			{
				if (entryLineFee.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.A40 || entryLineFee.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.A45)
				{
					entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.U235;
				}
			}
			else
			{
				entryLineFee.NationalFeeTypeCode = rateForCalculation.RateCode.Left(entryLineFee.NationalFeeTypeCodeInfo.MaxLength);
			}
		}

		protected override ZBool ShouldStash(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee)
		{
			var rfEntryLine = entryLine as CusEntryLine;
			return base.ShouldStash(rfEntryLine, entryLineFee) || entryLineFee.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Precalcule;
		}

		protected override ZString GetOverrideReasonCode(RateView rateView)
		{
			return rateView.ZZ2_RateFormula == UniversalReferenceConstants.RefCusRateFormula.Precalcule ? RateOverrideReasonList.Codes.Precalcule : base.GetOverrideReasonCode(rateView).ToString();
		}
	}
}
