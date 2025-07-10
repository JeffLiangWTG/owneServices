using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.DocumentWrappers.LiquidationDetails;

public static class LiquidationDetailsHelper
{
	public static ZString GetTaxBreakdown(IEnumerable<IGrouping<ZString, CusEntryLineFee>> entryLinefeesGroupBy, IEnumerable<CusEntryHeaderCharges> entryFees = null, bool isEntryHeaderLevel = false)
	{
		var sb = new ZStringBuilder();

		if (entryLinefeesGroupBy.Any())
		{
			sb.AppendLine();
			if (isEntryHeaderLevel)
			{
				sb.AppendLine(FormattableString.Invariant($"Tax code  Base Amount     Tax rate      Total amount"));
				if (entryFees != null)
				{
					foreach (var charge in entryFees)
					{
						sb.AppendLine(FormattableString.Invariant($"{charge.C1_ChargeType.PadRight(40)}{charge.C1_ChargeAmount.ToString(0)}").TrimEnd());
					}
				}
			}
			else
			{
				sb.AppendLine(FormattableString.Invariant($"Tax code  Base Amount     Tax rate      Total amount    SL"));
				if (entryFees != null)
				{
					foreach (var charge in entryFees)
					{
						sb.AppendLine(FormattableString.Invariant($"{charge.C1_ChargeType.PadRight(40)}{charge.C1_ChargeAmount.ToString(0).PadRight(16)}{charge.C1_MethodOfPayment}").TrimEnd());
					}
				}
			}

			foreach (var fees in entryLinefeesGroupBy)
			{
				var statusLiquidation = ""; // SL

				var fee = fees.FirstOrDefault();
				if (!isEntryHeaderLevel)
				{
					statusLiquidation = fee.CF_MethodOfPayment;
				}
				if (fees.Count() == 1)
				{
					var rate = (isEntryHeaderLevel ? string.Empty : (fee.G4_RateDuty == "%" ? (fee.CF_Rate.ToString("0.00#") + "%") : string.Empty));
					sb.AppendLine(FormattableString.Invariant($"{fee.NationalFeeTypeCode.PadRight(10)}{fee.CF_BaseValue.ToString(0).PadRight(16)}{rate.PadRight(14)}{fee.CF_ChargeAmount.ToString(0).PadRight(16)}{statusLiquidation}").TrimEnd());
				}
				else
				{
					sb.AppendLine(FormattableString.Invariant($"{fee.NationalFeeTypeCode.PadRight(10)}{new ZDecimal(fees.Sum(x => x.CF_BaseValue)).ToString(0).PadRight(16)}{ZString.Empty.PadRight(14)}{new ZDecimal(fees.Sum(x => x.CF_ChargeAmount)).ToString(0).PadRight(16)}{statusLiquidation}").TrimEnd());
				}
			}
		}
		return sb.ToString();
	}

	public static ZString GetLineChargeBreakdown(IEnumerable<InvoiceLineCharge> charges, IEnumerable<EU.Business.Declaration.InvoiceLineApportionCharge> appCharges)
	{
		var groupedCharges = charges.GroupBy(c => (c.J7_ChargeType, c.J7_RX_NKCurrency, c.J7_IsDutiable, c.J7_IsGSTApplicable, c.J7_IsStatisticalValueApplicable, c.J7_IsIncludedInITOT))
								.Select(cc => new GroupedCharge(cc.First().J7_ChargeType, cc.Sum(a => a.J7_Amount), cc.First().J7_RX_NKCurrency, cc.First().J7_IsDutiable, cc.First().J7_IsStatisticalValueApplicable, cc.First().J7_IsStatisticalValueApplicable, cc.First().J7_IsIncludedInITOT));

		var groupedAppCharges = appCharges.GroupBy(c => (c.J7_ChargeType, c.J7_RX_NKCurrency, c.J7_IsDutiable, c.J7_IsGSTApplicable, c.J7_IsStatisticalValueApplicable, c.J7_IsIncludedInITOT))
						.Select(cc => new GroupedCharge(cc.First().J7_ChargeType, cc.Sum(a => a.J7_Amount), cc.First().J7_RX_NKCurrency, cc.First().J7_IsDutiable, cc.First().J7_IsStatisticalValueApplicable, cc.First().J7_IsStatisticalValueApplicable, cc.First().J7_IsIncludedInITOT));

		var allGroupedCharges = groupedCharges.Concat(groupedAppCharges);

		return BuildChargeBreakDown(allGroupedCharges);
	}

	public static ZString GetHeaderChargeBreakdown(IEnumerable<InvoiceCharge> charges, IEnumerable<EU.Business.Declaration.InvoiceApportionCharge> appCharges)
	{
		var groupedCharges = charges.GroupBy(c => (c.J7_ChargeType, c.J7_RX_NKCurrency, c.J7_IsDutiable, c.J7_IsGSTApplicable, c.J7_IsStatisticalValueApplicable, c.J7_IsIncludedInITOT))
						.Select(cc => new GroupedCharge(cc.First().J7_ChargeType, cc.Sum(a => a.J7_Amount), cc.First().J7_RX_NKCurrency, cc.First().J7_IsDutiable, cc.First().J7_IsStatisticalValueApplicable, cc.First().J7_IsStatisticalValueApplicable, cc.First().J7_IsIncludedInITOT));

		var groupedAppCharges = appCharges.GroupBy(c => (c.J7_ChargeType, c.J7_RX_NKCurrency, c.J7_IsDutiable, c.J7_IsGSTApplicable, c.J7_IsStatisticalValueApplicable, c.J7_IsIncludedInITOT))
						.Select(cc => new GroupedCharge(cc.First().J7_ChargeType, cc.Sum(a => a.J7_Amount), cc.First().J7_RX_NKCurrency, cc.First().J7_IsDutiable, cc.First().J7_IsStatisticalValueApplicable, cc.First().J7_IsStatisticalValueApplicable, cc.First().J7_IsIncludedInITOT));

		var allGroupedCharges = groupedCharges.Concat(groupedAppCharges);

		return BuildChargeBreakDown(allGroupedCharges);
	}

	static ZString BuildChargeBreakDown(IEnumerable<GroupedCharge> allGroupedCharges)
	{
		var sb = new ZStringBuilder();

		if (allGroupedCharges.Any())
		{
			sb.AppendLine();
			sb.AppendLine(FormattableString.Invariant($"Code  Amount            Currency  Dutiable  TVA Apply  Statable  Incl. In Line"));
		}

		foreach (var groupedCharge in allGroupedCharges)
		{
			var dutiable = groupedCharge.Dutiable ? "Y" : "N";
			var tvaApply = groupedCharge.Vatable ? "Y" : "N";
			var statisticalApply = groupedCharge.Statable ? "Y" : "N";
			var includeInLine = groupedCharge.IncludedInLine ? "Y" : "N";

			sb.AppendLine(FormattableString.Invariant($"{groupedCharge.Type.PadRight(6)}{groupedCharge.Amount.ToString().PadRight(18)}{groupedCharge.Currency.ToString().PadRight(10)}{dutiable.PadRight(10)}{tvaApply.PadRight(11)}{statisticalApply.PadRight(10)}{includeInLine}").TrimEnd());
		}

		return sb.ToString();
	}

	class GroupedCharge
	{
		public GroupedCharge(ZString chargeType, ZDecimal chargeAmount, ZString currency, ZBool dutiable, ZBool statable, ZBool vatable, ZBool includedInLine)
		{
			this.Type = chargeType;
			this.Amount = chargeAmount;
			this.Currency = currency;
			this.Dutiable = dutiable;
			this.Statable = statable;
			this.Vatable = vatable;
			this.IncludedInLine = includedInLine;
		}

		public ZString Type;
		public ZDecimal Amount;
		public ZString Currency;
		public ZBool Dutiable;
		public ZBool Statable;
		public ZBool Vatable;
		public ZBool IncludedInLine;
	}
}
