using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

public class VatExemptionCalculator : IExtraFeeCalculator
{
	public VatExemptionCalculator(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}

	readonly CusEntryLine entryLine;

	public ZString RateCode => UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406;

	public IEnumerable<IDutyCalculationIntermediateResult> CalculateExtraFees()
	{
		var vatFee = GetEffectiveVatFee();
		if (vatFee != null)
		{
			var vatExemption = new DutyCalculationIntermediateResult(-vatFee.CF_ChargeAmount, ZDecimal.Zero, -vatFee.CF_BaseValue, ZString.Empty)
			{
				MethodOfPayment = vatFee.CF_MethodOfPayment,
			};

			vatExemption.SetRateAndMethodOfCalculation(vatFee.CF_MethodOfCalculation, vatFee.CF_Rate);

			yield return vatExemption;
		}
	}

	CusEntryLineFee GetEffectiveVatFee()
	{
		return entryLine.Fees.Cast<CusEntryLineFee>()
			.FirstOrDefault(
				x =>
					x.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
					&& (x.CF_RateOverrideReasonCode == ZString.Empty || x.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Override)
			);
	}
}
