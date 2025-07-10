using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec;

namespace Enterprise.Customs.CH.Business;

public class EdecRefinementDataProvider : IEdecRefinement
{
	public static EdecRefinementDataProvider New(CusEntryLine entryLine)
	{
		EdecRefinementDataProvider dataProvider = null;

		if (entryLine?.RandomLine is JobComInvoiceLine invoiceLine &&
			(!invoiceLine.InAndOutwardProcessingDirection.IsEmpty
			|| !invoiceLine.InAndOutwardProcessingRefinementType.IsEmpty
			|| !invoiceLine.InAndOutwardProcessingProcessType.IsEmpty
			|| !invoiceLine.InAndOutwardProcessingBillingType.IsEmpty
			|| !invoiceLine.InAndOutwardProcessingRepairReason.IsEmpty))
		{
			dataProvider = new EdecRefinementDataProvider(invoiceLine);
		}
		return dataProvider;
	}

	public EdecRefinementDataProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}
	readonly JobComInvoiceLine invoiceLine;

	public string Direction => invoiceLine.InAndOutwardProcessingDirection;

	public string RefinementType => invoiceLine.InAndOutwardProcessingRefinementType;

	public string ProcessType => invoiceLine.InAndOutwardProcessingProcessType;

	public string BillingType => invoiceLine.InAndOutwardProcessingBillingType;

	public string RepairReason => invoiceLine.InAndOutwardProcessingRepairReason;
}
