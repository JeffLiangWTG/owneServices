using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED807Provider : IED807
	{
		public ED807Provider(ED807B message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED807B message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IED807Event ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new ED807EventProvider(message.Body.InterruptionOfMovement.Attributes));
		IED807Event exciseMovementEad;

		public IReadOnlyCollection<ZString> ControlReportNumbers => controlReportNumbers ?? (controlReportNumbers = message.Body.InterruptionOfMovement.ReferenceControlReport?.Select(x => (ZString)x.ControlReportReference).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> controlReportNumbers;

		public IReadOnlyCollection<ZString> EventReportNumbers => eventReportNumbers ?? (eventReportNumbers = message.Body.InterruptionOfMovement.ReferenceEventReport?.Select(x => (ZString)x.EventReportNumber).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> eventReportNumbers;
	}

	class ED807EventProvider : IED807Event
	{
		public ED807EventProvider(ED807BBodyInterruptionOfMovementAttributes exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ED807BBodyInterruptionOfMovementAttributes exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => "1";

		public ZString Reason => exciseMovementEad.ReasonForInterruptionCode;

		public ZString ComplementaryInformation => exciseMovementEad.ComplementaryInformation;
	}
}
