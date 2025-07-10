using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CustomsValueCalculator
	{
		public CustomsValueCalculator(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			isEnteringOrExiting = entryHeader.IsEntering ? EnteringOrExiting.Entering : EnteringOrExiting.Exiting;
		}
		readonly CusEntryHeader entryHeader;
		readonly EnteringOrExiting isEnteringOrExiting;

		public void CalculateCustomsValue()
		{
			var entryLinePrices = GetEntryLinePricesInLocalCurrency(entryHeader);
			var totalLinePrices = entryLinePrices.Sum(x => x.Value);

			var freightFeeAmount = GetFeeAmountInLocalCurrency(entryHeader, entryHeader.FreightFee);
			var freightFeePercentage = GetFeePercentage(entryHeader.FreightFee);
			var insuranceFeeAmount = GetFeeAmountInLocalCurrency(entryHeader, entryHeader.InsuranceFee);
			var insuranceFeePercentage = GetFeePercentage(entryHeader.InsuranceFee);

			var isEntering = isEnteringOrExiting == EnteringOrExiting.Entering;
			decimal royaltyFeeAmount = 0, royaltyFeePercentage = 0;
			if (isEntering)
			{
				royaltyFeeAmount = GetFeeAmountInLocalCurrency(entryHeader, entryHeader.OtherFee);
				royaltyFeePercentage = GetFeePercentage(entryHeader.OtherFee);
			}

			if (totalLinePrices > 0)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					var customsValue = entryLinePrices[entryLine];
					var proportion = customsValue / totalLinePrices;

					if (isEntering)
					{
						customsValue = ApportionFeeToCustomsValue(freightFeeAmount, freightFeePercentage, customsValue, proportion);
						customsValue = ApportionFeeToCustomsValue(royaltyFeeAmount, royaltyFeePercentage, customsValue, proportion);
						customsValue = ApportionFeeToCustomsValue(insuranceFeeAmount, insuranceFeePercentage, customsValue, proportion);
					}
					else
					{
						customsValue = ApportionFeeToCustomsValue(insuranceFeeAmount, insuranceFeePercentage, customsValue, proportion);
						customsValue = ApportionFeeToCustomsValue(freightFeeAmount, freightFeePercentage, customsValue, proportion);
					}

					entryLine.CL_CustomsValue = customsValue;
				}
			}
		}

		decimal ApportionFeeToCustomsValue(decimal feeAmount, decimal feePercentage, decimal customsValue, decimal proportion)
		{
			decimal apportionedValue = 0;
			if (feeAmount > 0)
			{
				apportionedValue = RoundUsingCustomsRule(feeAmount * proportion);
			}
			else if (feePercentage > 0)
			{
				apportionedValue = RoundUsingCustomsRule(customsValue * feePercentage * 0.01m);
			}

			return isEnteringOrExiting == EnteringOrExiting.Entering ? customsValue + apportionedValue : customsValue - apportionedValue;
		}

		decimal RoundUsingCustomsRule(decimal amount)
		{
			return isEnteringOrExiting == EnteringOrExiting.Exiting ? RoundForMinus(amount) : Utilities.Round(amount, 0);
		}

		public static decimal RoundForMinus(decimal amount)
		{
			return (amount % 1 > 0.5m ? Math.Ceiling(amount) : Math.Floor(amount));
		}

		static decimal GetFeeAmountInLocalCurrency(CusEntryHeader entryHeader, CustomsFee fee)
		{
			return fee.MarkCode == FeeMarkTypeList.Codes.TotalPrice ? ConvertToLocalCurreny(entryHeader, fee.Money, 4) : 0;
		}

		static decimal GetFeePercentage(CustomsFee fee)
		{
			return fee.MarkCode == FeeMarkTypeList.Codes.Percentage ? fee.Amount : 0;
		}

		static Dictionary<CusEntryLine, decimal> GetEntryLinePricesInLocalCurrency(CusEntryHeader entryHeader)
		{
			var result = new Dictionary<CusEntryLine, decimal>();
			entryHeader.MergedLines.Cast<CusEntryLine>().ForEach(line => result.Add(line, ConvertToLocalCurreny(entryHeader, line.TotalPriceMoney)));
			return result;
		}

		static decimal ConvertToLocalCurreny(CusEntryHeader entryHeader, Money money, int decimalPlaces = 0)
		{
			return entryHeader.CurrencyConverter.ConvertExact(money, entryHeader.LocalCurrency, false).Round(decimalPlaces).Amount;
		}
	}
}
