using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public class TransportChargesDataProvider : ITransportCharges
{
	public static TransportChargesDataProvider New(NctsDepartureMovementHeader movementHeader) => movementHeader == null || movementHeader.BM_MethodOfPayment.IsEmpty ? null : new TransportChargesDataProvider(movementHeader);

	TransportChargesDataProvider(NctsDepartureMovementHeader movementHeader)
	{
		this.movementHeader = movementHeader;
	}
	readonly NctsDepartureMovementHeader movementHeader;

	public string MethodOfPayment => movementHeader.BM_MethodOfPayment;

	public string SequenceNumber => null;

	public decimal? Amount => null;

	public string Currency => null;

	public string Type => null;

	public bool? IncludedInInvoice => null;
}
