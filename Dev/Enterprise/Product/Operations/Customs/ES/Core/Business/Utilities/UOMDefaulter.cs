using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business
{
	public static class UOMDefaulter
	{
		public static void DefaultUOMIfApplicable(JobDeclaration declaration)
		{
			foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					var invoiceLine = entryLine.RandomLine;
					foreach (CusEntryLineFee fee in entryLine.Fees)
					{
						SetUOMFromFee(fee, entryLine, invoiceLine);
					}
				}
			}
		}

		static void SetUOMFromFee(CusEntryLineFee fee, CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			var methodOfCalculation = fee.CF_MethodOfCalculation;
			if (!methodOfCalculation.IsEmpty && methodOfCalculation != Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage && methodOfCalculation != UniversalReferenceConstants.ReservedRateFormulaValue.RetailPriceCode)
			{
				if (!UnitIsDeclared(methodOfCalculation, invoiceLine))
				{
					if (invoiceLine.JI_CustomsThirdUnitQty.IsEmpty)
					{
						entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToList().ForEach(x => x.JI_CustomsThirdUnitQty = methodOfCalculation);
					}
					else if (invoiceLine.JI_CustomsFourthUnitQty.IsEmpty)
					{
						entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToList().ForEach(x => x.JI_CustomsFourthUnitQty = methodOfCalculation);
					}
				}
			}
		}

		static bool UnitIsDeclared(ZString unit, JobComInvoiceLine invoiceLine)
		{
			var feeUnitAndConversionsList = GetConvertableUnits(unit);
			var customsUnitsList = new List<string>() { invoiceLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsThirdUnitQty, invoiceLine.JI_CustomsFourthUnitQty };

			return customsUnitsList.Intersect(feeUnitAndConversionsList).Any();
		}

		static IEnumerable<string> GetConvertableUnits(string unit)
		{
			return GetContainingList(
				unit,
				ConvertibleUnitsOfMeasure.WeightConversionDictionary.Keys,
				ConvertibleUnitsOfMeasure.VolumeConversionDictionary.Keys,
				ConvertibleUnitsOfMeasure.AlcoholConversionDictionary.Keys,
				new string[] { Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems, UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems });
		}

		static IEnumerable<string> GetContainingList(string unit, params IEnumerable<string>[] lists)
		{
			foreach (var list in lists)
			{
				if (list.Contains(unit))
				{
					return list;
				}
			}

			return new List<string>() { unit };
		}
	}
}
