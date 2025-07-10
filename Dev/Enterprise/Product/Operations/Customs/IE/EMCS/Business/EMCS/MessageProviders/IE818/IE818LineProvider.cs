using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE818LineProvider : LineProvider, IIE818Line
	{
		public IE818LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
		{
			outturn = emcsInvoiceLine.Outturn;
			helper = new Message818LineProviderHelper(emcsInvoiceLine);
		}
		readonly EMCSInvoiceLineCusOutturn outturn;
		readonly Message818LineProviderHelper helper;

		public decimal ObservedShortageOrExcess => helper.ObservedShortageOrExcess;

		public decimal RefusedQuantity => helper.RefusedQuantity;

		public IReadOnlyCollection<IEMCSReason> UnsatisfactoryReasons => unsatisfactoryReasons ?? (unsatisfactoryReasons = outturn.ReportOfReceiptReasons.Cast<ReportOfReceiptReason>().Select(x => new ReasonProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSReason> unsatisfactoryReasons;
	}
}
