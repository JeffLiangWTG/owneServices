using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public static class CustomsFeeCalculator
	{
		public static IEnumerable<JobComInvCharge> GetAllChargesFromInvoiceLines(this IEnumerable<BaseJobComInvoiceLine> invoiceLines, ZString chargeType)
		{
			return invoiceLines?.SelectMany(x => x.GetAllChargesFromInvoiceLine(chargeType)) ?? Array.Empty<JobComInvCharge>();
		}

		static IEnumerable<JobComInvCharge> GetAllChargesFromInvoiceLines(CusEntryHeader entryHeader, ZString chargeType)
		{
			return entryHeader?.InvoiceLines.GetAllChargesFromInvoiceLines(chargeType) ?? Array.Empty<JobComInvCharge>();
		}

		static IEnumerable<JobComInvCharge> GetAllChargesFromInvoiceLine(this BaseJobComInvoiceLine invoiceLine, ZString chargeType)
		{
			var chargeCode = invoiceLine.InvoiceHeader.IncoTermAndChargeFactory.GetCharge(chargeType);

			return chargeCode != null ? invoiceLine.Charges.Find(chargeCode.ChargeCodeChargeKey).Cast<JobComInvCharge>()
				.Union(invoiceLine.ApportionedCharges.Find(chargeCode.ChargeCodeChargeKey))
				: Array.Empty<JobComInvCharge>();
		}

		static RefCurrency GetEffectiveCurrencyForCharge(this CusEntryHeader entryHeader, ZString chargeType)
		{
			var lineCharges = GetAllChargesFromInvoiceLines(entryHeader, chargeType);

			var currency = lineCharges?.FirstOrDefault()?.Currency;
			if (currency != null && CNRefCusCodeListLoader.GetCurrency(entryHeader.Factory, currency.Code, entryHeader.DateOfValuation.Date) == null)
			{
				currency = null;
			}

			return currency ?? entryHeader.LocalCurrency;
		}

		static ZDecimal GetChargePercentage(this CusEntryHeader entryHeader, ZString chargeType)
		{
			var result = ZDecimal.Zero;
			if (entryHeader != null)
			{
				var invoiceLines = entryHeader.InvoiceLines;
				var chargesGroupped = invoiceLines.GroupBy(invoiceLine => invoiceLine.GetAllChargesFromInvoiceLine(chargeType));
				var eachLineHasOneCharge = chargesGroupped.All(lineCharges => lineCharges.Key.Count() == 1);

				if (eachLineHasOneCharge)
				{
					var allCharges = chargesGroupped.SelectMany(charge => charge.Key);
					if (allCharges != null && allCharges.Any() && allCharges.AllSame(x => x.J7_Percentage))
					{
						result = allCharges.First().J7_Percentage;
					}
				}
			}

			return result;
		}

		public static CustomsFee CalculateCustomsFee(this CusEntryHeader entryHeader, ZString chargeType)
		{
			CustomsFee result = null;

			if (entryHeader?.ShouldPopulateFee(chargeType) ?? false)
			{
				var percentage = GetChargePercentage(entryHeader, chargeType);
				if (!percentage.IsEmpty)
				{
					result = new CustomsFee(percentage);
				}
				else
				{
					var currency = GetEffectiveCurrencyForCharge(entryHeader, chargeType);
					if (currency != null)
					{
						var total = entryHeader.CurrencyConverter.Sum(GetAllChargesFromInvoiceLines(entryHeader, chargeType).Select(charge => charge.Money), currency);
						result = new CustomsFee(total.RoundUp(2));
					}
				}
			}

			if (result == null)
			{
				result = CustomsFee.Empty;
			}

			return result;
		}

		public static ZBool ShouldPopulateFee(ZString chargeType, ZString incoTermCode, ZBool isEntering, ZBool isExiting)
		{
			ZBool result;
			switch (chargeType)
			{
				case CustomsChargeTypeList.Codes.OverseasFreight:
					result = (isEntering && (incoTermCode == ShipmentIncoTerm.Codes.FOB || incoTermCode == ShipmentIncoTerm.Codes.CAI || incoTermCode == ShipmentIncoTerm.Codes.ExWorks))
								|| (isExiting && (incoTermCode == ShipmentIncoTerm.Codes.CIF || incoTermCode == ShipmentIncoTerm.Codes.CAF));
					break;
				case CustomsChargeTypeList.Codes.OverseasInsurance:
					result = (isEntering && (incoTermCode == ShipmentIncoTerm.Codes.CAF || incoTermCode == ShipmentIncoTerm.Codes.FOB || incoTermCode == ShipmentIncoTerm.Codes.ExWorks))
								|| (isExiting && (incoTermCode == ShipmentIncoTerm.Codes.CIF || incoTermCode == ShipmentIncoTerm.Codes.CAI));
					break;
				case CustomsChargeTypeList.Codes.Royalty:
					result = isEntering;
					break;
				default:
					result = false;
					break;
			}

			return result;
		}

		public static ZBool ShouldPopulateFee(this CusEntryHeader entryHeader, ZString chargeType)
		{
			return entryHeader != null ? ShouldPopulateFee(chargeType, entryHeader.IncoTermCode, entryHeader.IsEntering, entryHeader.IsExiting) : ZBool.False;
		}

		public static Money CalculateTotalPrice(this CusEntryLine entryLine)
		{
			var result = Money.Empty;
			if (entryLine != null)
			{
				var converter = entryLine.CurrencyConverter;
				var currency = entryLine.GetEffectiveCurrency();

				if (currency != null)
				{
					result = converter.Sum(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(line => GetAmountForTotalPrice(entryLine.Header, line)), currency);
				}
			}

			return result.RoundUp(2);
		}

		static IEnumerable<Money> GetAmountForTotalPrice(CusEntryHeader header, JobComInvoiceLine line)
		{
			IEnumerable<Money> result;
			if (header.IncoTermCode == ShipmentIncoTerm.Codes.FOB || header.IncoTermCode == ShipmentIncoTerm.Codes.ExWorks)
			{
				result = line.GetMoneyToAddToITOTForDutiable().Append(line.JI_LinePriceMoney);
			}
			else if (header.IncoTermCode == ShipmentIncoTerm.Codes.CIF)
			{
				result = line.GetMoneyToAddToITOTForVatableGstable().Append(line.JI_LinePriceMoney);
			}
			else if (header.IncoTermCode == ShipmentIncoTerm.Codes.CAF)
			{
				result = line.GetMoneyToAddToITOTForDutiable().Append(line.JI_LinePriceMoney).Append(line.JI_OverseasFreight);
			}
			else if (header.IncoTermCode == ShipmentIncoTerm.Codes.CAI)
			{
				result = line.GetMoneyToAddToITOTForDutiable().Append(line.JI_LinePriceMoney).Append(line.JI_OverseasInsurance);
			}
			else
			{
				result = Array.Empty<Money>();
			}
			return result;
		}

		static RefCurrency GetEffectiveCurrency(this CusEntryLine entryLine)
		{
			var result = entryLine?.RandomLine?.LinePriceRefCurrency;
			if (result != null && CNRefCusCodeListLoader.GetCurrency(entryLine.Factory, result.Code, entryLine.Header.DateOfValuation.Date) == null)
			{
				result = null;
			}

			return result ?? entryLine.Header.LocalCurrency;
		}

		public static Money Sum(this CurrencyConverter converter, IEnumerable<Money> amountsToBeSummed, ICurrency destinationCurrency, bool roundToDestinationCurrencyDecimals = false)
		{
			var totalAmount = 0m;
			foreach (var amountToBeAdded in amountsToBeSummed.GroupBy(x => x.Currency).Select(grouping => new Money(grouping.Sum(x => x.Amount), grouping.Key)))
			{
				totalAmount += converter.ConvertExact(amountToBeAdded, destinationCurrency, false).Amount;
			}
			if (roundToDestinationCurrencyDecimals)
			{
				totalAmount = ZArchitecture.Core.Utilities.Round(totalAmount, destinationCurrency.Decimals);
			}
			return new Money(totalAmount, destinationCurrency);
		}

		static IEnumerable<Money> GetMoneyToAddToITOTForDutiable(this IChargeApportionee chargeApportionee)
		{
			return chargeApportionee.Charges.MoneyToAddToITOTForDutiableCharges().Concat(
				chargeApportionee.ApportionedCharges.MoneyToAddToITOTForDutiableCharges());
		}

		static IEnumerable<Money> GetMoneyToAddToITOTForVatableGstable(this IChargeApportionee chargeApportionee)
		{
			return chargeApportionee.Charges.MoneyToAddToITOTForVatableGstableCharges().Concat(
				chargeApportionee.ApportionedCharges.MoneyToAddToITOTForVatableGstableCharges());
		}
	}
}
