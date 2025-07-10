using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class RefundDataProvider : IRefund
{
	public static RefundDataProvider New(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new RefundDataProvider(invoiceLine);

	RefundDataProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = invoiceLine;
	}
	readonly JobComInvoiceLine invoiceLine;

	public string GoodsDeclarationReferenceNumber => invoiceLine.JI_RefundReferenceNumber;

	public int GoodsItemNumber => invoiceLine.JI_RefundGoodsItemNumber;

	public string Reason => invoiceLine.JI_RefundReason;
}
