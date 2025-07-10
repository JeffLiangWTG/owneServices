using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED818Provider : IED818
	{
		public ED818Provider(ED818D message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED818D message;

		public IEMCSEvent ExciseMovement => exciseMovement ?? (exciseMovement = new ED818EventProvider(message.Body.AcceptedOrRejectedReportOfReceipt.ExciseMovement));
		IEMCSEvent exciseMovement;

		public ZString GlobalConclusionOfReceipt => message.Body.AcceptedOrRejectedReportOfReceipt.ReportOfReceipt.GlobalConclusionOfReceipt.XmlEnumToString();

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IReadOnlyCollection<IED818ReportOfReceipt> ReportOfReceipts => reportOfReceipts ?? (reportOfReceipts = message.Body.AcceptedOrRejectedReportOfReceipt.BodyReportOfReceipt?.Select(x => new ED818ReportOfReceiptProvider(x)).ToArray() ?? Array.Empty<IED818ReportOfReceipt>());
		IReadOnlyCollection<IED818ReportOfReceipt> reportOfReceipts;
	}
}
