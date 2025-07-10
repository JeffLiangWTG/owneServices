using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5;

public class ED818EventProvider : IEMCSEvent
{
	public ED818EventProvider(ED818DBodyAcceptedOrRejectedReportOfReceiptExciseMovement exciseMovement)
	{
		this.exciseMovement = Argument.NotNull(exciseMovement, nameof(exciseMovement));
	}
	readonly ED818DBodyAcceptedOrRejectedReportOfReceiptExciseMovement exciseMovement;

	public ZString AdministrativeReferenceCode => exciseMovement.AdministrativeReferenceCode;

	public ZString SequenceNumber => exciseMovement.SequenceNumber;
}
