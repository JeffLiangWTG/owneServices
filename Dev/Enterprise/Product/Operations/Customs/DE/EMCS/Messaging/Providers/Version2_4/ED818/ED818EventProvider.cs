using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4;

public class ED818EventProvider : IEMCSEvent
{
	public ED818EventProvider(ED818CBodyAcceptedOrRejectedReportOfReceiptExciseMovementEad exciseMovementEad)
	{
		this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
	}
	readonly ED818CBodyAcceptedOrRejectedReportOfReceiptExciseMovementEad exciseMovementEad;

	public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

	public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
}
