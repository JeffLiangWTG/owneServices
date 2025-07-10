using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class MInvoiceLineTypeProvider : IMoney
	{
		public MInvoiceLineTypeProvider(EntryLineWrapper entryLineWrapper)
		{
			this.entryLine = entryLineWrapper.EntryLine;
			this.randomInvoiceHeader = entryLineWrapper.RandomInvoiceHeader;
		}

		readonly CusEntryLine entryLine;
		readonly JobComInvoiceHeader randomInvoiceHeader;

		public decimal Amount => entryLine.TotalLinePriceInLocalCurrency;

		public string Currency => randomInvoiceHeader.JZ_RX_NKInvoice_Currency;
	}
}
