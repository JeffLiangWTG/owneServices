using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class GoodsReferenceProvider : IGoodsReference
{
	readonly CusContainerInvoiceLinePivot containerInvoiceLinePivot;
	public GoodsReferenceProvider(CusContainerInvoiceLinePivot containerInvoiceLinePivot, int sequence)
	{
		this.containerInvoiceLinePivot = Argument.NotNull(containerInvoiceLinePivot, nameof(containerInvoiceLinePivot));
		this.SequenceNumber = sequence;
	}

	public int SequenceNumber { get; }

	public int DeclarationGoodsItemNumber => int.TryParse(containerInvoiceLinePivot.InvoiceLine.JI_Calc_MergedLineNumber, out int x) ? x : 0;
}
