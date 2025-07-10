using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED871LineProvider : LineProvider, IED871Line
	{
		public ED871LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
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
