namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message871LineProviderHelper : LineProviderHelper
	{
		public Message871LineProviderHelper(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
		{
			outturn = emcsInvoiceLine.Outturn;
		}
		readonly EMCSInvoiceLineCusOutturn outturn;

		public decimal ActualQuantity => outturn.ActualQuantity.Normalize();
	}
}
