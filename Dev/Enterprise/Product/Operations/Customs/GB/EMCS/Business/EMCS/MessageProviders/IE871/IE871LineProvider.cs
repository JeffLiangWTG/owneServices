using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE871LineProvider : LineProvider, IIE871Line
	{
		public IE871LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
		{
			outturn = emcsInvoiceLine.Outturn;
		}
		readonly EMCSInvoiceLineCusOutturn outturn;

		public decimal ActualQuantity => outturn.ActualQuantity.Normalize();

		public ITextAndLanguage Explanation => explanation ?? (explanation = new TextAndLanguageProvider(outturn.C5_OutturnResultReason));
		ITextAndLanguage explanation;
	}
}
