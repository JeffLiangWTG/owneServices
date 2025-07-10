using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie807;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE807Provider : IIE807
	{
		public IE807Provider(Ie807Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie807Type message;

		public ZString MrnNumber => message.Body.InterruptionOfMovement.Attributes.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => "1";

		public ZString MessageIdentifier => message.Header.MessageIdentifier;

		public IIE807Event ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE807EventProvider(message.Body.InterruptionOfMovement.Attributes));
		IIE807Event exciseMovementEad;

		public IReadOnlyCollection<ZString> ControlReportNumbers => controlReportNumbers ?? (controlReportNumbers = message.Body.InterruptionOfMovement.ReferenceControlReport?.Select(x => (ZString)x.ControlReportReference).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> controlReportNumbers;

		public IReadOnlyCollection<ZString> EventReportNumbers => eventReportNumbers ?? (eventReportNumbers = message.Body.InterruptionOfMovement.ReferenceEventReport?.Select(x => (ZString)x.EventReportNumber).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> eventReportNumbers;
	}

	sealed class IE807EventProvider : IIE807Event
	{
		public IE807EventProvider(AttributesType attributes)
		{
			this.attributes = Argument.NotNull(attributes, nameof(attributes));
		}
		readonly AttributesType attributes;

		public ZString AdministrativeReferenceCode => attributes.AdministrativeReferenceCode;

		public ZString SequenceNumber => "1";

		public ZString Reason => attributes.ReasonForInterruptionCode;

		public ZString ComplementaryInformation => attributes.ComplementaryInformation.Value;
	}
}
