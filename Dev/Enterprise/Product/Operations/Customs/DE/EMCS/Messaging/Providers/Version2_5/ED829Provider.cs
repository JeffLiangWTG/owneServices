using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED829Provider : IED829
	{
		public ED829Provider(ED829C message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED829C message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public ZString SendingCustomsOffice => message.Body.NotificationOfAcceptedExport.ExportAcceptance.ReferenceNumberOfSenderCustomsOffice;

		public ZDate AcceptanceDate => new ZDate(message.Body.NotificationOfAcceptedExport.ExportAcceptance.DateOfAcceptance);

		public ZString MRN => message.Body.NotificationOfAcceptedExport.ExportAcceptance.DocumentReferenceNumber;

		public IReadOnlyCollection<IEMCSEvent> ExciseMovementEads => exciseMovementEads ?? (exciseMovementEads = message.Body.NotificationOfAcceptedExport.ExciseMovementEad.Select(x => new ED829EventProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSEvent> exciseMovementEads;
	}

	class ED829EventProvider : IEMCSEvent
	{
		public ED829EventProvider(ED829CBodyNotificationOfAcceptedExportExciseMovementEad exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ED829CBodyNotificationOfAcceptedExportExciseMovementEad exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
