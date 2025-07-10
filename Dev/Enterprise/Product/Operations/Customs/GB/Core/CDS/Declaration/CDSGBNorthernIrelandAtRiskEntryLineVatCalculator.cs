using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSGBNorthernIrelandAtRiskEntryLineVatCalculator : EntryLineVatCalculator
	{
		public CDSGBNorthernIrelandAtRiskEntryLineVatCalculator(EU.Business.Declaration.CusEntryLine entryLine) : base(entryLine)
		{
		}

		public IEnumerable<CDSEntryLineVatResult> CalculateVatFees()
		{
			var result = new List<CDSEntryLineVatResult>();
			var vatFeeRawArray = (AppliedTaxAndFee == null) ? null : CalculateVatFeesWithGivenRate(AppliedTaxAndFee.ZZF_Value);

			if (!vatFeeRawArray.IsNullOrEmpty())
			{
				var elementsIncludedInB00Calculation = vatFeeRawArray.Take(vatFeeRawArray.Count - 1).Cast<DutyCalculationIntermediateResult>().ToArray();
				result.Add(new CDSEntryLineVatResult(elementsIncludedInB00Calculation.Sum(x => x.Amount), elementsIncludedInB00Calculation.First().Rate, elementsIncludedInB00Calculation.Sum(x => x.BaseValue), MethodOfCalculation.Percentage)
				{
					Code = UniversalReferenceConstants.RefCusRateCodes.Vat,
				});

				var elementIncludedInB05Calculation = vatFeeRawArray.Last();
				result.Add(new CDSEntryLineVatResult(elementIncludedInB05Calculation.Amount, elementIncludedInB05Calculation.Rate, elementIncludedInB05Calculation.BaseValue, MethodOfCalculation.Percentage)
				{
					Code = UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland,
				});
			}

			return result.ToArray();
		}

		Universal.RefCusTaxOrFee AppliedTaxAndFee => appliedTaxAndFee ?? (appliedTaxAndFee = (entryLine.RandomLine == null) ? null : entryLine.RandomLine.AppliedTaxAndFee);

		Universal.RefCusTaxOrFee appliedTaxAndFee;

		List<DutyCalculationIntermediateResult> CalculateVatFeesWithGivenRate(ZDecimal rate)
		{
			var result = new List<DutyCalculationIntermediateResult>();
			var baseValue = entryLine.CL_ValueForVAT;
			result.Add(new DutyCalculationIntermediateResult(baseValue * rate, rate, baseValue, MethodOfCalculation.Percentage));

			var fees = (EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)entryLine.Fees;
			foreach (CusEntryLineFee fee in fees)
			{
				if (fee.IsNationalIndirectTaxationFee && !fee.CF_IsLandedCostOnly && !fees.HasOverrideFeeOfGivenCode(fee.CF_ChargeType))
				{
					baseValue = new ZDecimal(fee.CF_ChargeAmount).Round(2);
					result.Add(new DutyCalculationIntermediateResult(baseValue * rate, rate, baseValue, MethodOfCalculation.Percentage));
				}
			}

			baseValue = entryLine.DutyDetails;
			result.Add(new DutyCalculationIntermediateResult(baseValue * rate, rate, baseValue, MethodOfCalculation.Percentage));
			return result;
		}

		protected override void RefreshFeesCore()
		{
			if (AppliedTaxAndFee != null)
			{
				var calculationIntermediateResults = CalculateVatFeesWithGivenRate(AppliedTaxAndFee.ZZF_Value);

				if (!calculationIntermediateResults.IsNullOrEmpty())
				{
					var intermediateResultB00 = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.Vat);

					if (intermediateResultB00 != null)
					{
						var intermediateResultB00s = calculationIntermediateResults.Take(calculationIntermediateResults.Count - 1);
						if (intermediateResultB00s != null)
						{
							intermediateResultB00.CF_BaseValue = intermediateResultB00s.Cast<DutyCalculationIntermediateResult>().Sum(x => x.BaseValue);
							intermediateResultB00.CF_ChargeAmount = IsNorthernIrelandDomestic ? 0 : intermediateResultB00s.Cast<DutyCalculationIntermediateResult>().Sum(x => x.Amount);
						}
					}

					var intermediateResultB05s = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland);
					if (intermediateResultB05s != null)
					{
						var intermediateResultB05 = calculationIntermediateResults.Last();
						if (intermediateResultB05 != null)
						{
							intermediateResultB05s.CF_BaseValue = intermediateResultB05.BaseValue;
							intermediateResultB05s.CF_ChargeAmount = IsNorthernIrelandDomestic ? 0 : intermediateResultB05.Amount;
						}
					}
				}
			}
		}

		CusEntryLine GBEntryLine => entryLine as CusEntryLine;

		ZBool IsNorthernIrelandDomestic => GBEntryLine?.IsNorthernIrelandDomestic ?? false;

		public class CDSEntryLineVatResult : DutyCalculationIntermediateResult
		{
			public ZString Code { get; set; }

			public CDSEntryLineVatResult(ZDecimal amount, ZDecimal rate, ZDecimal baseValue, ZString methodOfCalculation) : base(amount, rate, baseValue, methodOfCalculation)
			{
			}
		}
	}
}
