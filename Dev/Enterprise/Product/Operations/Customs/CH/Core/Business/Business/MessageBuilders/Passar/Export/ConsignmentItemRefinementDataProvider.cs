using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class ConsignmentItemRefinementDataProvider : IRefinement
{
	public static ConsignmentItemRefinementDataProvider New(CusEntryLine entryLine)
	{
		ConsignmentItemRefinementDataProvider dataProvider = null;

		if (entryLine?.RandomLine is JobComInvoiceLine invoiceLine &&
			(!invoiceLine.InAndOutwardProcessingRefinementType.IsEmpty
			|| !invoiceLine.InAndOutwardProcessingProcessType.IsEmpty
			|| !invoiceLine.InAndOutwardProcessingBillingType.IsEmpty
			|| !invoiceLine.InAndOutwardProcessingRepairReason.IsEmpty
			|| invoiceLine.NotifyCustomsOffices.Count != 0))
		{
			dataProvider = new ConsignmentItemRefinementDataProvider(invoiceLine);
		}
		return dataProvider;
	}

	ConsignmentItemRefinementDataProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}
	readonly JobComInvoiceLine invoiceLine;

	public string SupervisionOfficeReferenceNumber => invoiceLine.NotifyCustomsOffices.FirstOrDefault()?.CY_Data;

	public string Type => GetRefinementType(invoiceLine.InAndOutwardProcessingRefinementType);

	public string ProcessType => GetRefinementProcessType(invoiceLine.InAndOutwardProcessingProcessType);

	public string AccountingType => GetAccountingType(invoiceLine.InAndOutwardProcessingBillingType);

	public string Reason => invoiceLine.InAndOutwardProcessingRepairReason;

	string GetRefinementProcessType(ZString processType)
	{
		switch (processType)
		{
			case InAndOutwardProcessingProcessTypesEdec.DueProcedure:
				return InAndOutwardRefinementProcessTypesPassar.Ordinary;
			case InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure:
				return InAndOutwardRefinementProcessTypesPassar.Simplified;
			case InAndOutwardProcessingProcessTypesEdec.SpecialProcedure:
				return InAndOutwardRefinementProcessTypesPassar.Special;
			default:
				return string.Empty;
		}
	}

	string GetAccountingType(ZString billingType)
	{
		switch (billingType)
		{
			case InAndOutwardProcessingBillingTypesEdec.SuspensiveProcedure:
				return InAndOutwardRefinementAccountingTypesPassar.NonCollection;
			case InAndOutwardProcessingBillingTypesEdec.RefundProcedure:
				return InAndOutwardRefinementAccountingTypesPassar.Refund;
			default:
				return string.Empty;
		}
	}

	string GetRefinementType(ZString refinementType)
	{
		switch (refinementType)
		{
			case InAndOutwardProcessingRefinementTypesEdec.CommercialProcessing:
				return InAndOutwardRefinementTypesPassar.Own;
			case InAndOutwardProcessingRefinementTypesEdec.ContractProcessing:
				return InAndOutwardRefinementTypesPassar.Pay;
			default:
				return string.Empty;
		}
	}
}
