using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED840Provider : IED840
	{
		public ED840Provider(ED840C message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED840C message;

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public IEMCSEvent ExciseMovement => exciseMovement ?? (exciseMovement = new ED840EventProvider(message.Body.EventReport.ExciseMovementEad));
		IEMCSEvent exciseMovement;
	}

	class ED840EventProvider : IEMCSEvent
	{
		public ED840EventProvider(ED840CBodyEventReportExciseMovementEad exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ED840CBodyEventReportExciseMovementEad exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
