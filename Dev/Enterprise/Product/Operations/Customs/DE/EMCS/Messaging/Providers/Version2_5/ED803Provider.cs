using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED803Provider : IED803
	{
		public ED803Provider(ED803B message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED803B message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public ZDateTime NotificationDateTime => message.Body.NotificationOfDivertedEad.ExciseNotification.NotificationDateAndTime;

		public ZString NotificationType => message.Body.NotificationOfDivertedEad.ExciseNotification.NotificationType.XmlEnumToString();

		public IReadOnlyCollection<ZString> DownstreamARCs => downstreamARCs ?? (downstreamARCs = message.Body.NotificationOfDivertedEad.DownstreamArc?.Select(x => new ZString(x.AdministrativeReferenceCode)).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> downstreamARCs;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new ED803EMCSEventProvider(message.Body.NotificationOfDivertedEad.ExciseNotification));
		IEMCSEvent exciseMovementEad;
	}

	class ED803EMCSEventProvider : IEMCSEvent
	{
		public ED803EMCSEventProvider(ED803BBodyNotificationOfDivertedEadExciseNotification exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ED803BBodyNotificationOfDivertedEadExciseNotification exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
