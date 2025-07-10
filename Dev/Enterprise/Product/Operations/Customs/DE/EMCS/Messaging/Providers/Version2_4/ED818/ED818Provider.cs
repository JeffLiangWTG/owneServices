using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED818Provider : IED818
	{
		public ED818Provider(ED818C message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED818C message;

		public IEMCSEvent ExciseMovement => exciseMovementEad ?? (exciseMovementEad = new ED818EventProvider(message.Body.AcceptedOrRejectedReportOfReceipt.ExciseMovementEad));
		IEMCSEvent exciseMovementEad;

		public ZString GlobalConclusionOfReceipt => message.Body.AcceptedOrRejectedReportOfReceipt.ReportOfReceipt.GlobalConclusionOfReceipt.XmlEnumToString();

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IReadOnlyCollection<IED818ReportOfReceipt> ReportOfReceipts => reportOfReceipts ?? (reportOfReceipts = message.Body.AcceptedOrRejectedReportOfReceipt.BodyReportOfReceipt?.Select(x => new ED818ReportOfReceiptProvider(x)).ToArray() ?? Array.Empty<IED818ReportOfReceipt>());
		IReadOnlyCollection<IED818ReportOfReceipt> reportOfReceipts;
	}
}
