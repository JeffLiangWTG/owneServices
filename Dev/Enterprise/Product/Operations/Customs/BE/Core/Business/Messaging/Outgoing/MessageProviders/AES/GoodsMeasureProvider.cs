using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;
using CusEntryLine = Enterprise.Customs.BE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.BE.Business;

public class GoodsMeasureProvider : IGoodsMeasure
{
	readonly CusEntryLine entryLine;

	public GoodsMeasureProvider(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}

	public decimal? GrossMass => CachedValueHelper.GetValue(ref grossMass,
		() => entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceLine => invoiceLine.GrossWeightInKG));
	CachedValue<decimal?> grossMass;

	public decimal NetMass => CachedValueHelper.GetValue(ref netMass,
		() => entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceLine => invoiceLine.CustomsFirstQuantityInKG));
	CachedValue<decimal> netMass;

	public decimal? SupplementaryUnits => supplementaryUnits ?? (supplementaryUnits = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceLine => invoiceLine.JI_CustomsSecondQuantity));
	decimal? supplementaryUnits;

	public string SupplementaryUnitsCode => entryLine.InvoiceLines[0].JI_CustomsSecondUnitQty;
}
