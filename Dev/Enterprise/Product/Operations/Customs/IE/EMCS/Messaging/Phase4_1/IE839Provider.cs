using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE839;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public sealed class IE839Provider : IIE839
	{
		public IE839Provider(Ie839Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie839Type message;

		public ZString SendingCustomsOffice => message.Header.MessageSender;

		public ZDate IssuanceDate => new ZDate(message.Body.RefusalByCustoms.Attributes.DateAndTimeOfIssuance);

		public ZString MRN => message.Body.RefusalByCustoms.ExportDeclarationInformation?.DocumentReferenceNumber;

		public ZString RejectionReasonCode => message.Body.RefusalByCustoms.Rejection.RejectionReasonCode.ToString();

		public IReadOnlyCollection<IEMCSEvent> RejectedEads => rejectedEads ?? (rejectedEads = message.Body.RefusalByCustoms.CEadVal.Select(x => new IE839EventProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSEvent> rejectedEads;

		public ZString MrnNumber => RejectedEad?.AdministrativeReferenceCode ?? ZString.Empty;

		public ZString MrnNumberSequenceNumber => RejectedEad?.SequenceNumber ?? ZString.Empty;

		IEMCSEvent RejectedEad
		{
			get
			{
				if (rejectedEad == null)
				{
					var rejectedEads = RejectedEads.Take(2).ToArray();
					if (rejectedEads.Length == 1)
					{
						rejectedEad = rejectedEads[0];
					}
				}

				return rejectedEad;
			}
		}
		IEMCSEvent rejectedEad;
	}

	class IE839EventProvider : IEMCSEvent
	{
		public IE839EventProvider(CEadValType exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly CEadValType exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
