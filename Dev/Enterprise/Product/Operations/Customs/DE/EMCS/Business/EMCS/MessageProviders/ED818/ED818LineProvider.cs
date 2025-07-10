using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED818LineProvider : LineProvider, IED818Line
	{
		public ED818LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
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
