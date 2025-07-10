using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6A93NumberAmountCalculator : IUcc6A93NumberAmountCalculator
{
	public Ucc6A93NumberAmountCalculator(IFee[] fees)
	{
		this.fees = Argument.NotNull(fees, nameof(fees));
	}

	decimal IUcc6A93NumberAmountCalculator.CalculateAmount()
	{
		if (fees.Length == 0)
		{
			return 0m;
		}

		var feesToSubtract = fees.Where(f => chargeTypesForSubtraction.Contains(f.ChargeType)).ToArray();
		var remainingFees = fees.Except(feesToSubtract);
		var feeAmount = remainingFees.Sum(f => f.Amount);
		var amountToSubtract = feesToSubtract.Sum(f => f.Amount);
		return feeAmount - amountToSubtract;
	}

	readonly IEnumerable<ZString> chargeTypesForSubtraction = new ZString[]
	{
		UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406,
		UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode407
	}.ToImmutableArray();

	readonly IFee[] fees;
}
