using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED839Provider : IED839
	{
		public ED839Provider(ED839C message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED839C message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public ZString SendingCustomsOffice => message.Header.MessageSender;

		public ZDate IssuanceDate => new ZDate(message.Body.CustomsRejectionOfEad.Attributes.DateAndTimeOfIssuance);

		public ZString MRN => message.Body.CustomsRejectionOfEad.ExportCrossCheckingDiagnoses?.DocumentReferenceNumber;

		public ZString LocalReferenceNumber => message.Body.CustomsRejectionOfEad.ExportCrossCheckingDiagnoses?.LocalReferenceNumber;

		public ZString RejectionReasonCode => message.Body.CustomsRejectionOfEad.Rejection.RejectionReasonCode.XmlEnumToString();

		public IReadOnlyCollection<IEMCSEvent> RejectedEads => rejectedEads ?? (rejectedEads = message.Body.CustomsRejectionOfEad.CEadVal.Select(x => new ED839EventProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSEvent> rejectedEads;
	}

	class ED839EventProvider : IEMCSEvent
	{
		public ED839EventProvider(ED839CBodyCustomsRejectionOfEadCEadVal rejectedEad)
		{
			this.rejectedEad = Argument.NotNull(rejectedEad, nameof(rejectedEad));
		}
		readonly ED839CBodyCustomsRejectionOfEadCEadVal rejectedEad;

		public ZString AdministrativeReferenceCode => rejectedEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => rejectedEad.SequenceNumber;
	}
}
