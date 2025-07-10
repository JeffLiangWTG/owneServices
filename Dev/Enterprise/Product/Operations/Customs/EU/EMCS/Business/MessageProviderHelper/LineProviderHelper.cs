namespace Enterprise.Customs.EU.EMCS.Business
{
	public class LineProviderHelper
	{
		public LineProviderHelper(EMCSJobComInvoiceLine emcsInvoiceLine)
		{
			this.emcsInvoiceLine = emcsInvoiceLine;
		}
		protected readonly EMCSJobComInvoiceLine emcsInvoiceLine;

		public int LineNumber => emcsInvoiceLine.JI_LineNo;

		public string ExciseProductCode => emcsInvoiceLine.ZG_ExciseProductCode;
	}
}
