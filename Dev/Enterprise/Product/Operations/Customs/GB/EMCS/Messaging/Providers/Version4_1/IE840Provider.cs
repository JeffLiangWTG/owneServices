using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie840;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE840Provider : IIE840
	{
		public IE840Provider(Ie840Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie840Type message;

		public ZString MessageIdentifier => message.Header.MessageIdentifier;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE840EventProvider(message.Body.EventReportEnvelope.ExciseMovement));
		IEMCSEvent exciseMovementEad;

		public ZString MrnNumber => message.Body.EventReportEnvelope.ExciseMovement.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => message.Body.EventReportEnvelope.ExciseMovement.SequenceNumber;
	}

	sealed class IE840EventProvider : IEMCSEvent
	{
		public IE840EventProvider(ExciseMovementType exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ExciseMovementType exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
