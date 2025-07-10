using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLineFreightAdjustmentsCalculator
{
	public InvoiceLineFreightAdjustmentsCalculator(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		currencyConverter = Argument.NotNull(invoiceLine.CurrencyConverter, nameof(invoiceLine.CurrencyConverter));
	}

	readonly JobComInvoiceLine invoiceLine;
	readonly CurrencyConverter currencyConverter;

	public Money EvaluateAmount()
	{
		var allLineCharges = invoiceLine.Charges.Cast<JobComInvCharge>().Concat(invoiceLine.ApportionedCharges);

		var statisticalValueApplicablePart = GetChargesAmount(allLineCharges, IsOFTStatisticalValueApplicableButNotIncludedInLines);
		var statisticalValueNotApplicablePart = GetChargesAmount(allLineCharges, IsOFTStatisticalValueNotApplicableButIncludedInLines);
		return currencyConverter.Subtract(statisticalValueApplicablePart, statisticalValueNotApplicablePart);
	}

	#region Implementation

	Money GetChargesAmount(IEnumerable<JobComInvCharge> charges, Func<JobComInvCharge, bool> filterFunc)
	{
		return charges
			.Where(filterFunc)
			.Aggregate(Money.Empty, (initialAmount, chargeToBeAdded) => currencyConverter.Add(initialAmount, chargeToBeAdded.Money));
	}

	Func<JobComInvCharge, bool> IsOFTStatisticalValueApplicableButNotIncludedInLines => charge => IsOverseasFreightCharge(charge) && charge.J7_IsStatisticalValueApplicable && !charge.J7_IsIncludedInITOT;

	Func<JobComInvCharge, bool> IsOFTStatisticalValueNotApplicableButIncludedInLines => charge => IsOverseasFreightCharge(charge) && !charge.J7_IsStatisticalValueApplicable && charge.J7_IsIncludedInITOT;

	Func<JobComInvCharge, bool> IsOverseasFreightCharge => charge => charge.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasFreight;

	#endregion
}
