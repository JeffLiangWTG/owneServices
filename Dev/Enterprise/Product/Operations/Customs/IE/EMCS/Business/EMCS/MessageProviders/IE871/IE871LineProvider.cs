using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE871LineProvider : LineProvider, IIE871Line
	{
		public IE871LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
		{
			outturn = emcsInvoiceLine.Outturn;
			helper = new Message871LineProviderHelper(emcsInvoiceLine);
		}
		readonly EMCSInvoiceLineCusOutturn outturn;
		readonly Message871LineProviderHelper helper;

		public decimal ActualQuantity => helper.ActualQuantity;

		public ITextAndLanguage Explanation => explanation ?? (explanation = new TextAndLanguageProvider(outturn.C5_OutturnResultReason));
		ITextAndLanguage explanation;
	}
}
