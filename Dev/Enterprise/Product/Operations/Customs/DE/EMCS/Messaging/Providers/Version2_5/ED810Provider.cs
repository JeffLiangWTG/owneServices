using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED810Provider : IED810
	{
		public ED810Provider(ED810C message)
		{
			this.message = message ?? throw new ArgumentNullException(nameof(message));
		}
		readonly ED810C message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new ED810EventProvider(message.Body.CancellationOfEad.ExciseMovementEad));
		IEMCSEvent exciseMovementEad;

		public string CancellationReasonCode => message.Body.CancellationOfEad.Cancellation.CancellationReasonCode;
	}

	class ED810EventProvider : IEMCSEvent
	{
		public ED810EventProvider(ED810CBodyCancellationOfEadExciseMovementEad exciseMovementEad)
		{
			this.exciseMovementEad = exciseMovementEad ?? throw new ArgumentNullException(nameof(exciseMovementEad));
		}
		readonly ED810CBodyCancellationOfEadExciseMovementEad exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => "1";
	}
}
