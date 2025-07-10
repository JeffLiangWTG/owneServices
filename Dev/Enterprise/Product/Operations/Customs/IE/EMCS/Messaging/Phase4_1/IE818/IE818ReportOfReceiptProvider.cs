using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE818;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE818ReportOfReceiptProvider : IIE818ReportOfReceipt
	{
		public IE818ReportOfReceiptProvider(BodyReportOfReceiptExportType reportOfReceipt)
		{
			this.reportOfReceipt = Argument.NotNull(reportOfReceipt, nameof(reportOfReceipt));
		}
		readonly BodyReportOfReceiptExportType reportOfReceipt;

		public ZString LineNumber => reportOfReceipt.BodyRecordUniqueReference;

		public ZString IndicatorOfShortageOrExcess => reportOfReceipt.IndicatorOfShortageOrExcessValueSpecified ? reportOfReceipt.IndicatorOfShortageOrExcessValue.XmlEnumToString() : ZString.Empty;

		public ZDecimal ObservedQuantity => reportOfReceipt.ObservedShortageOrExcessValueSpecified ? reportOfReceipt.ObservedShortageOrExcessValue : 0m;

		public ZDecimal RefusedQuantity => reportOfReceipt.RefusedQuantityValueSpecified ? reportOfReceipt.RefusedQuantityValue : 0m;

		public IReadOnlyCollection<IIE818UnsatisfactoryReason> UnsatisfactoryReasons => unsatisfactoryReasons ?? (unsatisfactoryReasons = reportOfReceipt.UnsatisfactoryReason?.Select(x => new IE818UnsatisfactoryReasonProvider(x)).ToArray() ?? Array.Empty<IIE818UnsatisfactoryReason>());
		IReadOnlyCollection<IIE818UnsatisfactoryReason> unsatisfactoryReasons;
	}
}
