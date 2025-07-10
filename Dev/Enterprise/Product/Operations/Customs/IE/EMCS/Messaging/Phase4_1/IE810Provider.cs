using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE810;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE810Provider : IIE810
	{
		public IE810Provider(Ie810Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie810Type message;

		public ZString MrnNumber => ExciseMovementEad.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => ExciseMovementEad.SequenceNumber;

		public string CancellationReasonCode => message.Body.CancellationOfEad.Cancellation.CancellationReasonCode;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE810EventProvider(message.Body.CancellationOfEad.ExciseMovementEad));
		IEMCSEvent exciseMovementEad;

		class IE810EventProvider : IEMCSEvent
		{
			public IE810EventProvider(ExciseMovementEadType exciseMovementEad)
			{
				this.exciseMovementEad = exciseMovementEad ?? throw new ArgumentNullException(nameof(exciseMovementEad));
			}
			readonly ExciseMovementEadType exciseMovementEad;

			public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

			public ZString SequenceNumber => "1";
		}
	}
}
