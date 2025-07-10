using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class CalculationUnit : ITCalculationUnits
{
	public static IEnumerable<ITCalculationUnits> ExtractCalculationUnits(CusEntryLine entryLine)
	{
		var secondCode = entryLine.InvoiceLines.Select(ji => ji.JI_CustomsSecondUnitQty).SingleOrDefault();
		if (secondCode.IsValid && !secondCode.IsEmpty)
		{
			yield return new CalculationUnit
			{
				CalculationUnits = entryLine.InvoiceLines.Sum(ji => (ji as JobComInvoiceLine).JI_CustomsSecondQuantity),
				CalculationCode = secondCode,
			};
		}
		else
		{
			yield break;
		}

		var thirdCode = entryLine.InvoiceLines.Select(ji => ji.JI_CustomsThirdUnitQty).SingleOrDefault();
		if (thirdCode.IsValid && !thirdCode.IsEmpty)
		{
			yield return new CalculationUnit
			{
				CalculationUnits = entryLine.InvoiceLines.Sum(ji => (ji as JobComInvoiceLine).JI_CustomsThirdQuantity),
				CalculationCode = thirdCode,
			};
		}
		else
		{
			yield break;
		}

		var fourthCode = entryLine.InvoiceLines.Select(ji => ji.JI_BondedWhsUnitQty).SingleOrDefault();
		if (fourthCode.IsValid && !fourthCode.IsEmpty)
		{
			yield return new CalculationUnit
			{
				CalculationUnits = entryLine.InvoiceLines.Sum(ji => (ji as JobComInvoiceLine).JI_BondedWhsQuantity),
				CalculationCode = fourthCode,
			};
		}
		else
		{
			yield break;
		}
	}
	public ZString CalculationCode { get; set; }
	public ZDecimal CalculationUnits { get; set; }
}
