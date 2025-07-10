namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message818LineProviderHelper : LineProviderHelper
	{
		public Message818LineProviderHelper(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
		{
			outturn = emcsInvoiceLine.Outturn;
		}
		readonly EMCSInvoiceLineCusOutturn outturn;

		public decimal ObservedShortageOrExcess => outturn.ObservedDifference.Normalize();

		public decimal RefusedQuantity => outturn.C5_RejectedQuantity.Normalize();
	}
}
