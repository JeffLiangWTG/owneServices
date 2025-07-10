using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie818;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE818ReportOfReceiptProvider : IIE818ReportOfReceipt
	{
		public IE818ReportOfReceiptProvider(BodyReportOfReceiptExportType reportOfReceipt)
		{
			this.reportOfReceipt = Argument.NotNull(reportOfReceipt, nameof(reportOfReceipt));
		}
		readonly BodyReportOfReceiptExportType reportOfReceipt;

		public ZString LineNumber => reportOfReceipt.BodyRecordUniqueReference;

		public ZString IndicatorOfShortageOrExcess => reportOfReceipt.IndicatorOfShortageOrExcessValueSpecified ? reportOfReceipt.IndicatorOfShortageOrExcess.Value.XmlEnumToString() : ZString.Empty;

		public ZDecimal ObservedQuantity => reportOfReceipt.ObservedShortageOrExcessValueSpecified ? reportOfReceipt.ObservedShortageOrExcess.Value : 0m;

		public ZDecimal RefusedQuantity => reportOfReceipt.RefusedQuantityValueSpecified ? reportOfReceipt.RefusedQuantity.Value : 0m;

		public IReadOnlyCollection<IIE818UnsatisfactoryReason> UnsatisfactoryReasons => unsatisfactoryReasons ?? (unsatisfactoryReasons = reportOfReceipt.UnsatisfactoryReason?.Select(x => new IE818UnsatisfactoryReasonProvider(x)).ToArray() ?? Array.Empty<IIE818UnsatisfactoryReason>());
		IReadOnlyCollection<IIE818UnsatisfactoryReason> unsatisfactoryReasons;
	}
}
