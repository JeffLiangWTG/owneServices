using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class AESHeaderDeliveryTermsProvider : IDeliveryTerms
	{
		public AESHeaderDeliveryTermsProvider(JobComInvoiceHeader invoiceHeader)
		{
			this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
		}

		public string IncotermCode => invoiceHeader.JZ_IncoTerm;

		public string Location => IncotermCode != Core.Constants.IncoTerms.Other ? incoTermPlace : string.Empty;

		public string UNLocode => IncotermCode != Core.Constants.IncoTerms.Other && incoTermPlace.IsNullOrEmpty() ? agreedPlaceCode : null;

		public string Country => IncotermCode != Core.Constants.IncoTerms.Other && !incoTermPlace.IsNullOrEmpty() ? agreedPlaceCode : null;

		public string Text => IncotermCode == Core.Constants.IncoTerms.Other ? incoTermPlace : null;

		readonly JobComInvoiceHeader invoiceHeader;

		string incoTermPlace => invoiceHeader.JZ_IncoTermPlace;

		string agreedPlaceCode => invoiceHeader.ZG_AgreedPlaceCode;
	}
}
