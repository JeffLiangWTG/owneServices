using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE818LineProvider : LineProvider, IIE818Line
	{
		public IE818LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
		{
			outturn = emcsInvoiceLine.Outturn;
		}
		readonly EMCSInvoiceLineCusOutturn outturn;

		public decimal ObservedShortageOrExcess => outturn.ObservedDifference.Normalize();

		public decimal RefusedQuantity => outturn.C5_RejectedQuantity.Normalize();

		public IReadOnlyCollection<IEMCSReason> UnsatisfactoryReasons => unsatisfactoryReasons ?? (unsatisfactoryReasons = outturn.ReportOfReceiptReasons.Cast<ReportOfReceiptReason>().Select(x => new ReasonProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSReason> unsatisfactoryReasons;
	}
}
