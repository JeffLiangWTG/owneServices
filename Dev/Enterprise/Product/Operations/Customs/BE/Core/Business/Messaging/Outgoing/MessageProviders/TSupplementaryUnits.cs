using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class TSupplementaryUnits : ITSupplementaryUnits
{
	public TSupplementaryUnits(CusEntryLine entryLine)
	{
		Argument.NotNull(entryLine, CusEntryLine.Schema.TableName);
		this.entryLine = entryLine;
	}

	readonly CusEntryLine entryLine;

	public ZString SupplementaryUnitsCode
	{
		get => entryLine.InvoiceLines.Select(ji => ji.JI_CustomsSecondUnitQty).SingleOrDefault();
	}
	public ZDecimal SupplementaryUnits { get => SupplementaryUnitsCode.IsEmpty ? ZDecimal.Zero : new ZDecimal(entryLine.InvoiceLines.Sum(ji => (ji as JobComInvoiceLine).JI_CustomsSecondQuantity)); }
}
