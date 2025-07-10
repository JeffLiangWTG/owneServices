using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class TransportChargesDataProvider : ITransportCharges
{
	public static TransportChargesDataProvider New(CusEntryInstruction entryInstruction) => entryInstruction == null || entryInstruction.CEI_TransportChargesMethodOfPayment.IsEmpty ? null : new TransportChargesDataProvider(entryInstruction);

	TransportChargesDataProvider(CusEntryInstruction entryInstruction)
	{
		this.entryInstruction = entryInstruction;
	}

	readonly CusEntryInstruction entryInstruction;

	public string MethodOfPayment => entryInstruction.CEI_TransportChargesMethodOfPayment;

	public string SequenceNumber => null;

	public decimal? Amount => null;

	public string Currency => null;

	public string Type => null;

	public bool? IncludedInInvoice => null;
}
