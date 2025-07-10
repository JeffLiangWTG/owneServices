using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie818;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE818Provider : IIE818
	{
		public IE818Provider(Ie818Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie818Type message;

		public ZString GlobalConclusionOfReceipt => message.Body.AcceptedOrRejectedReportOfReceiptExport.ReportOfReceiptExport?.GlobalConclusionOfReceipt.XmlEnumToString() ?? ZString.Empty;

		public IReadOnlyCollection<IIE818ReportOfReceipt> ReportOfReceipts => reportOfReceipts ?? (reportOfReceipts = message.Body.AcceptedOrRejectedReportOfReceiptExport.BodyReportOfReceiptExport?.Select(x => new IE818ReportOfReceiptProvider(x)).ToArray() ?? Array.Empty<IIE818ReportOfReceipt>());
		IReadOnlyCollection<IIE818ReportOfReceipt> reportOfReceipts;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE818EventProvider(message.Body.AcceptedOrRejectedReportOfReceiptExport.ExciseMovement));
		IEMCSEvent exciseMovementEad;

		public ZString MrnNumber => ExciseMovementEad.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => ExciseMovementEad.SequenceNumber;
	}

	sealed class IE818EventProvider : IEMCSEvent
	{
		public IE818EventProvider(ExciseMovementType exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ExciseMovementType exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
