using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE803;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE803Provider : IIE803
	{
		public IE803Provider(Ie803Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie803Type message;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE803EMCSEventProvider(message.Body.NotificationOfDivertedEadesad.ExciseNotification));
		IEMCSEvent exciseMovementEad;

		public ZDateTime NotificationDateTime => message.Body.NotificationOfDivertedEadesad.ExciseNotification.NotificationDateAndTime;

		public ZString NotificationType => message.Body.NotificationOfDivertedEadesad.ExciseNotification.NotificationType.XmlEnumToString();

		public IReadOnlyCollection<ZString> DownstreamARCs => downstreamARCs ?? (downstreamARCs = message.Body.NotificationOfDivertedEadesad.DownstreamArc?.Select(x => new ZString(x.AdministrativeReferenceCode)).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> downstreamARCs;

		public ZString MrnNumber => ExciseMovementEad.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => ExciseMovementEad.SequenceNumber;
	}

	sealed class IE803EMCSEventProvider : IEMCSEvent
	{
		public IE803EMCSEventProvider(ExciseNotificationType exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ExciseNotificationType exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
