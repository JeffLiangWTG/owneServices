using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class EntryLineVatCalculator
	{
		public EntryLineVatCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		public DutyCalculationIntermediateResult CalculateVatFee()
		{
			var appliedTaxAndFee = (entryLine.RandomLine == null) ? null : entryLine.RandomLine.AppliedTaxAndFee;
			return (appliedTaxAndFee == null) ? null : CalculateVatFeeWithGivenRate(appliedTaxAndFee.ZZF_Value);
		}

		public DutyCalculationIntermediateResult CalculateVatFeeWithGivenRate(ZDecimal rate)
		{
			var fees = entryLine.Fees;
			var importDutyAmount = entryLine.DutyDetailsForVAT;
			var nationalIndirectTaxationAmount = fees.Cast<CusEntryLineFee>()
				.Where(f => f.IsNationalIndirectTaxationFee && !f.CF_IsLandedCostOnly && !fees.HasOverrideFeeOfGivenCode(f.CF_ChargeType))
				.Sum(f => f.CF_ChargeAmount);

			var baseValue = new ZDecimal(entryLine.CL_ValueForVAT + importDutyAmount + nationalIndirectTaxationAmount).Round(2);
			var amount = CalculateAmount(baseValue, rate);

			return new DutyCalculationIntermediateResult(amount, rate, baseValue, Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage);
		}

		protected virtual ZDecimal CalculateAmount(ZDecimal baseValue, ZDecimal rate)
		{
			return baseValue * rate;
		}

		public void RefreshFees() => RefreshFeesCore();

		protected virtual void RefreshFeesCore()
		{
			var vatFeeToBeUpdated = GetSystemAddedVatFee();

			if (vatFeeToBeUpdated != null)
			{
				var percentRate = vatFeeToBeUpdated.CF_Rate;
				var decimalRate = percentRate / 100m;
				var vatCalculation = CalculateVatFeeWithGivenRate(decimalRate);
				vatFeeToBeUpdated.CF_BaseValue = vatCalculation.BaseValue;
				vatFeeToBeUpdated.CF_ChargeAmount = vatCalculation.Amount;
			}
		}

		CusEntryLineFee GetSystemAddedVatFee() => entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.IsSystemAddedVatFee);

		protected readonly CusEntryLine entryLine;
	}
}
