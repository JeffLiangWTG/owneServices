using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED818ReportOfReceiptProvider : IED818ReportOfReceipt
	{
		public ED818ReportOfReceiptProvider(ED818DBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceipt reportOfReceipt)
		{
			this.reportOfReceipt = Argument.NotNull(reportOfReceipt, nameof(reportOfReceipt));
		}
		readonly ED818DBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceipt reportOfReceipt;

		public ZString LineNumber => reportOfReceipt.BodyRecordUniqueReference;
		public ZString IndicatorOfShortageOrExcess => reportOfReceipt.IndicatorOfShortageOrExcessSpecified ? reportOfReceipt.IndicatorOfShortageOrExcess.XmlEnumToString() : ZString.Empty;
		public ZDecimal ObservedQuantity => reportOfReceipt.ObservedShortageOrExcessSpecified ? reportOfReceipt.ObservedShortageOrExcess : 0m;
		public ZDecimal RefusedQuantity => reportOfReceipt.RefusedQuantitySpecified ? reportOfReceipt.RefusedQuantity : 0m;
		public IReadOnlyCollection<IED818UnsatisfactoryReason> UnsatisfactoryReasons => unsatisfactoryReasons ?? (unsatisfactoryReasons = reportOfReceipt.UnsatisfactoryReason?.Select(x => new ED818UnsatisfactoryReasonProvider(x)).ToArray() ?? Array.Empty<IED818UnsatisfactoryReason>());
		IReadOnlyCollection<IED818UnsatisfactoryReason> unsatisfactoryReasons;
	}
}
