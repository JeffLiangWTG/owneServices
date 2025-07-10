using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415DeclarationTypeValuationInformationProvider : IIM413AndIM415DeclarationTypeValuationInformation
	{
		public IM413AndIM415DeclarationTypeValuationInformationProvider(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}
		readonly CusEntryHeader entryHeader;

		public string InvoiceCurrency => entryHeader.TotalPrice.Currency?.Code ?? string.Empty;

		public decimal InvoiceAmount => entryHeader.TotalPriceAmount;

		public decimal ExchangeRate => 0m;
	}
}
