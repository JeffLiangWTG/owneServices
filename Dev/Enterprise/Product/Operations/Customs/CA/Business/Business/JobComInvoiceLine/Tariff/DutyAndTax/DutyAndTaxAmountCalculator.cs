using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business
{
	static class DutyAndTaxAmountCalculator
	{
		public static void CalculateDutyAndTaxAmount(IDutyAndTaxDataForCalculation dutyOrTax, ZDecimal value)
		{
			var factory = dutyOrTax.Parent?.Factory ?? new BusinessObjectFactory();
			var amountToSet = dutyOrTax.Amount;
			switch (dutyOrTax.RateType)
			{
				case RateTypes.Codes.AdValorem:
					amountToSet = Utilities.Round(value * dutyOrTax.Rate / 100m, 2);
					break;
				case RateTypes.Codes.Specific:
					amountToSet = Utilities.Round(dutyOrTax.Quantity * dutyOrTax.Rate, 2);
					break;
				case RateTypes.Codes.Exempt:
					amountToSet = ZDecimal.Zero;
					break;
			}

			if (!dutyOrTax.NormalValuePerUnit.IsEmpty && !dutyOrTax.NormalValueCurrency.IsEmpty && !dutyOrTax.Quantity.IsEmpty)
			{
				var totalNormalValue = dutyOrTax.NormalValuePerUnit * dutyOrTax.Quantity;
				var normalValueCurrency = dutyOrTax.NormalValueCurrency;
				if (normalValueCurrency != Core.Constants.CurrencyCodes.Canada)
				{
					if (dutyOrTax.Parent is IDutyAndTaxData dutyAndTaxData && dutyAndTaxData.CurrencyConverter is CurrencyConverter converter)
					{
						var fromCurrency = RefCurrency.LoadFromCurrencyCode(factory, normalValueCurrency);
						var toCurrency = RefCurrency.LoadFromCurrencyCode(factory, Core.Constants.CurrencyCodes.Canada);
						var totalNormalValueInCAD = converter.ConvertRounded(new Money(totalNormalValue, fromCurrency), toCurrency);
						if (totalNormalValueInCAD != null)
						{
							totalNormalValue = totalNormalValueInCAD.Amount.Round(2);
						}
					}
				}
				amountToSet = totalNormalValue - value;
			}

			dutyOrTax.Amount = Math.Max(amountToSet, 0m);
			dutyOrTax.ValueForCalculation = value;
		}

		public static ZDecimal GetQuantity(IDutyAndTaxData parent, ZString unitOfMeasure, ZString taxType, ZString rateType, ZDecimal normalValuePerUnit)
		{
			var result = ZDecimal.Zero;
			if (parent != null && (rateType == RateTypes.Codes.Specific || !normalValuePerUnit.IsEmpty))
			{
				//Classification & Tariff duties don't have specified UOM and should use 1st quantity.
				//Only excise duty has specified UOM but it should match 2nd or 3rd quantities.
				//Taxes should match 1st/2nd/3rd quantities where UOM specified, or use 1st where not.
				//NOTE: This conditional operator should match the one in DutyAndTaxAmountDescriptor.GetQuantityDescription() method.
				if (unitOfMeasure.IsEmpty || (unitOfMeasure == parent.CustomsUnits && taxType != DutyAndTaxTypes.Codes.CustomsDuty))
				{
					result = parent.CustomsQuantity;
				}
				else if (unitOfMeasure == parent.CustomsUnits2)
				{
					result = parent.CustomsQuantity2;
				}
				else if (unitOfMeasure == parent.CustomsUnits3)
				{
					result = parent.CustomsQuantity3;
				}
				else
				{
					var declaration = parent?.Declaration;
					if (declaration != null && unitOfMeasure == parent.CustomsUnits && taxType == DutyAndTaxTypes.Codes.CustomsDuty && declaration.IsDefaultExciseDutyQuantityToFirstCustomsQuantity)
					{
						result = parent.CustomsQuantity;
					}
				}
			}
			return result;
		}
	}
}
