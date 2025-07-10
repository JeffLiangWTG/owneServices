using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE818;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE818Provider : IIE818
	{
		public IE818Provider(Ie818Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie818Type message;

		public ZString GlobalConclusionOfReceipt => message.Body.AcceptedOrRejectedReportOfReceiptExport.ReportOfReceiptExport.GlobalConclusionOfReceipt.XmlEnumToString();

		public IReadOnlyCollection<IIE818ReportOfReceipt> ReportOfReceipts => reportOfReceipts ?? (reportOfReceipts = message.Body.AcceptedOrRejectedReportOfReceiptExport.BodyReportOfReceiptExport?.Select(x => new IE818ReportOfReceiptProvider(x)).ToArray() ?? Array.Empty<IIE818ReportOfReceipt>());
		IReadOnlyCollection<IIE818ReportOfReceipt> reportOfReceipts;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE818EventProvider(message.Body.AcceptedOrRejectedReportOfReceiptExport.ExciseMovement));
		IEMCSEvent exciseMovementEad;

		public ZString MrnNumber => ExciseMovementEad.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => ExciseMovementEad.SequenceNumber;
	}

	public class IE818EventProvider : IEMCSEvent
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
