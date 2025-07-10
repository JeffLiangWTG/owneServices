using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED840Provider : IED840
	{
		public ED840Provider(ED840D message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED840D message;

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public IEMCSEvent ExciseMovement => exciseMovement ?? (exciseMovement = new ED840EventProvider(message.Body.EventReport.ExciseMovement));
		IEMCSEvent exciseMovement;
	}

	class ED840EventProvider : IEMCSEvent
	{
		public ED840EventProvider(ED840DBodyEventReportExciseMovement exciseMovement)
		{
			this.exciseMovement = Argument.NotNull(exciseMovement, nameof(exciseMovement));
		}
		readonly ED840DBodyEventReportExciseMovement exciseMovement;

		public ZString AdministrativeReferenceCode => exciseMovement.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovement.SequenceNumber;
	}
}
